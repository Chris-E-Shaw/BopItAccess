"""Build an authoring glossary of common menu labels from installed assets.

Only short interface terms are exported. The asset bundles themselves are not
copied into the project. This script makes no network requests.
"""

from __future__ import annotations

import json
import argparse
import os
from pathlib import Path

from extract_native_localization import read_bundle, string_table


ROOT = Path(__file__).resolve().parents[1]
DEFAULT_GAME_DIR = Path(os.environ.get("BOPIT_GAME_DIR",
    r"C:\Program Files (x86)\Steam\steamapps\common\Bop It!"))
LOCALES = {
    "en": "english(en)",
    "fr": "french(fr)",
    "it": "italian(it)",
    "de": "german(de)",
    "es": "spanish(es)",
    "es-MX": "spanish(mexico)(es-mx)",
    "ja": "japanese(ja)",
    "ko": "korean(ko)",
    "zh": "chinese(simplified)(zh)",
    "pt-BR": "portuguese(brazil)(pt-br)",
}
COMMON = {
    "PLAY", "LEADERBOARDS", "ACHIEVEMENTS", "SETTINGS", "CREDITS",
    "MUSIC", "SFX", "VOICE OVER", "LANGUAGE", "AUDIO LATENCY",
    "VIBRATION", "FULLSCREEN", "RESOLUTION", "CONTROLS", "SOLO",
    "PARTY", "PASS IT", "ONE ON ONE", "REPLAY", "BACK", "QUIT",
    "LOCAL", "FRIENDS", "Global", "TODAY", "MONTH", "ALL TIME",
    "SUBMIT", "Continue", "SCORE", "RESET TO DEFAULT", "RESUME",
    "MAIN MENU", "Calibrate", "RANK", "CONFIRM", "SELECT", "NAME",
    "CLASSIC", "EXTREME", "green", "yellow",
    "BOP", "TWIST", "PULL", "SPIN", "FLICK",
    "BOP IT", "TWIST IT", "PULL IT", "SPIN IT", "FLICK IT",
    "BOP - PLAYER 2",
}


def load_table(code: str, bundle_root: Path) -> dict[int, str]:
    label = LOCALES[code]
    bundle = next(bundle_root.glob(
        f"localization-string-tables-{label}_assets_all.bundle"))
    return string_table(read_bundle(bundle), f"General_{code}", code)


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--game-dir", type=Path, default=DEFAULT_GAME_DIR)
    args = parser.parse_args()
    bundle_root = (args.game_dir / "BopIt!_Data" / "StreamingAssets" /
                   "aa" / "StandaloneWindows64")
    english = load_table("en", bundle_root)
    result: dict[str, dict[str, str]] = {}
    for code in LOCALES:
        if code == "en":
            continue
        native = load_table(code, bundle_root)
        terms = {}
        for identifier, source in english.items():
            if source in COMMON and identifier in native:
                term = native[identifier].replace("\n", " ").strip()
                if "<" not in term and term:
                    terms[source] = term
        result[code] = terms
    destination = ROOT / "tools" / "translation-cache" / "native-game-terms.json"
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_text(json.dumps(result, ensure_ascii=False, indent=2),
                           encoding="utf-8")
    print(destination)
    print({code: len(terms) for code, terms in result.items()})


if __name__ == "__main__":
    main()
