"""Optional, owner-approved authoring upgrade for speech translations.

Sends only English UI strings to Google Translate. This tool is not part of the
mod build or runtime. Native game terms and hand-curated accessibility text
take precedence; every translated format placeholder is checked before use.
The offline Argos catalogs remain a fallback for rejected translations.
"""

from __future__ import annotations

import argparse
import json
import re
from collections import Counter
from pathlib import Path

from generate_speech_catalogs import (
    DESCRIPTION_PREFIXES, LOCALES, MANUAL, PLACEHOLDER, PUNCTUATION,
    finalize_catalog, native_glossary, native_value, source_keys,
)
from translate_documents import google_translate, regionalize


ROOT = Path(__file__).resolve().parents[1]
TOKEN_LEAK = re.compile(r"(?:98765432\d+|88888888\d+|9999999999|XYZZY)")
KEEP_LITERAL = {"SAPI", "OneCore", "NVDA", "JAWS", "UI Automation",
                "ZDSR", "ZoomText", "Boy PC Reader", "PC Talker",
                "Sense Reader", "Window-Eyes", "System Access",
                "F8", "F9", "LT", "RT", "Bop It Access"}


def translate_batch(sources: list[str], locale: str) -> list[str | None]:
    masked = []
    maps = []
    for source in sources:
        replacements = {}
        text = source
        for index, token in enumerate(sorted(set(PLACEHOLDER.findall(source)))):
            marker = str(888888880000 + index)
            text = text.replace(token, marker)
            replacements[marker] = token
        masked.append(text)
        maps.append(replacements)
    results = google_translate(masked, LOCALES[locale])
    accepted = []
    for source, result, replacements in zip(sources, results, maps):
        for marker, original in replacements.items():
            result = result.replace(marker, original)
        result = regionalize(result, locale)
        if source.endswith(" ") and not result.endswith(" "):
            result += " "
        if (not result or TOKEN_LEAK.search(result) or
                Counter(PLACEHOLDER.findall(result)) !=
                Counter(PLACEHOLDER.findall(source))):
            accepted.append(None)
        else:
            accepted.append(result)
    return accepted


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--locale", choices=LOCALES)
    args = parser.parse_args()
    all_keys = source_keys()
    native = native_glossary()
    for locale in ([args.locale] if args.locale else LOCALES):
        path = ROOT / "src" / "locales" / f"{locale}.json"
        catalog = json.loads(path.read_text(encoding="utf-8"))
        terms = native.get(locale, {})
        keys = sorted(all_keys | set(terms) | set(catalog))
        pending = []
        for source in keys:
            stage = any(source.startswith(prefix) for prefix in
                        DESCRIPTION_PREFIXES.values())
            if source in MANUAL[locale]:
                catalog[source] = MANUAL[locale][source]
                continue
            if source in PUNCTUATION:
                catalog[source] = source
                continue
            if stage:
                continue  # finalize_catalog sets curated stage prose
            if source in KEEP_LITERAL:
                catalog[source] = source
                continue
            native_term = native_value(source, terms)
            if native_term:
                catalog[source] = native_term
                continue
            pending.append(source)
        upgraded = 0
        rejected = []
        batch = []
        length = 0
        for source in pending + [""]:
            if batch and (not source or len(batch) >= 25 or
                          length + len(source) > 2600):
                translated = translate_batch(batch, locale)
                for key, value in zip(batch, translated):
                    if value is None:
                        rejected.append(key)
                    else:
                        catalog[key] = value
                        upgraded += 1
                print(f"{locale}: upgraded {upgraded}/{len(pending)}",
                      flush=True)
                batch = []
                length = 0
            if source:
                batch.append(source)
                length += len(source)
        finalize_catalog(locale, catalog)
        path.write_bytes((json.dumps(catalog, ensure_ascii=False,
            indent=2) + "\n").encode("utf-8"))
        print(f"{locale}: catalog {len(catalog)} keys; {len(rejected)} "
              "Google results rejected in favor of offline fallback", flush=True)
        if rejected:
            cache = (ROOT / "tools" / "translation-cache" /
                     f"google-speech-rejected-{locale}.json")
            cache.parent.mkdir(parents=True, exist_ok=True)
            cache.write_text(json.dumps(rejected, ensure_ascii=False, indent=2),
                             encoding="utf-8")


if __name__ == "__main__":
    main()
