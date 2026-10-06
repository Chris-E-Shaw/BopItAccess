"""Generate localized project documents with offline or approved translation.

This is an authoring tool, not a dependency of the mod or its build. Install
argostranslate and beautifulsoup4, then install the English-to-target models.
Argos runs locally. The optional Google backend sends source text to Google
Translate and must be used only with project-owner authorization. Generated
documents are machine-translated drafts and need native-speaker review.
"""

from __future__ import annotations

import argparse
import json
import re
import time
from pathlib import Path

from bs4 import (
    BeautifulSoup,
    Comment,
    Declaration,
    Doctype,
    NavigableString,
    ProcessingInstruction,
)


ROOT = Path(__file__).resolve().parent.parent
DOCUMENTS = (
    "BopItAccess-user-guide.html",
    "BopItAccess-build-history.html",
    "BopItAccess-release-review.html",
    "README.md",
    "README.txt",
    "GIT-WORKFLOW.md",
    "THIRD-PARTY-NOTICES.txt",
)
MODEL_CODE = {
    "fr": "fr",
    "it": "it",
    "de": "de",
    "es": "es",
    "es-MX": "es",
    "ja": "ja",
    "ko": "ko",
    "zh": "zh",
    "pt-BR": "pt",
}
SKIP_HTML = {"script", "style", "code", "pre", "kbd", "samp"}
MARKDOWN_PROTECTED = re.compile(
    r"(`[^`]*`|\[[^\]]+\]\([^)]+\)|https?://\S+)"
)
MARKDOWN_PREFIX = re.compile(r"^(\s*(?:#{1,6}\s+|[-*+]\s+|\d+[.)]\s+|>\s+)?)")
SEPARATOR_LINE = re.compile(r"^[\s|:+\-=]+$")
WINDOWS_PATH = re.compile(r"[A-Za-z]:\\[^\r\n]+")
RELATIVE_PATH = re.compile(r"(?<!\w)(?:[\w!.-]+\\)+[\w!.-]+")
FILE_NAME = re.compile(r"\b[\w-]+\.(?:dll|html|md|txt|csproj|json|exe|zip)\b",
    re.IGNORECASE)
VERSION = re.compile(r"(?<![\w])[vV]?\d+(?:\.\d+){1,3}(?:f\d+)?\b")
PROTECTED_TERMS = (
    "Bop It Access", "Bop It! The Video Game", "Bop It!", "Bop It",
    "MelonLoader",
    "Tolk", "Prism", "NVDA", "JAWS", "SAPI", "OneCore",
    "UI Automation", "ZDSR", "ZoomText", "Boy PC Reader",
    "PC Talker", "Sense Reader", "System Access", "Window-Eyes",
    "Harmony", "Codex", "GPT-6 Luna",
    "GPT-6 Sol", "GitHub", "Steam", "Windows", "PowerShell", ".NET",
    "Unity", "FMOD", "Il2Cpp", "OpenAI",
)
PROTECTED_PATTERN = re.compile(
    "|".join(re.escape(term) for term in sorted(PROTECTED_TERMS,
        key=len, reverse=True))
)
OUTPUT_MODE_LIST_LINES = {
    "Auto, SAPI, OneCore, NVDA, JAWS, UI Automation, ZDSR, ZoomText,",
    "Boy PC Reader, PC Talker, Sense Reader, System Access, Window-Eyes.",
}
ACTION_CUES = re.compile(r"(?<![A-Za-z])(?:Bop|Twist|Pull|Spin|Flick)(?![A-Za-z])",
    re.IGNORECASE)
STAGE_TITLES = re.compile(
    r"(?<![A-Za-z])(?:Shapes|Space|City|Office)(?![A-Za-z])")
NATIVE_ACTION_NAMES = {
    "fr": {"bop": "TAPER", "twist": "TOURNER", "pull": "TIRER",
           "spin": "PIVOTER", "flick": "BALAYER"},
    "it": {"bop": "COLPISCI", "twist": "RUOTA", "pull": "TIRA",
           "spin": "GIRA", "flick": "SCUOTI"},
    "de": {"bop": "KLOPFEN", "twist": "DREHEN", "pull": "ZIEHEN",
           "spin": "KREISEN", "flick": "SCHNIPSEN"},
    "es": {"bop": "GOLPEAR", "twist": "RETORCER", "pull": "TIRAR",
           "spin": "GIRAR", "flick": "ATIZAR"},
    "es-MX": {"bop": "PULSAR", "twist": "ROTAR", "pull": "TIRAR",
              "spin": "GIRAR", "flick": "TOCAR"},
    "ja": {"bop": "叩く", "twist": "ねじる", "pull": "引く",
           "spin": "回す", "flick": "はじく"},
    "ko": {"bop": "치기", "twist": "비틀기", "pull": "당기기",
           "spin": "돌리기", "flick": "뒤집기"},
    "zh": {"bop": "拍打", "twist": "扭转", "pull": "拉动",
           "spin": "转动", "flick": "拨动"},
    "pt-BR": {"bop": "BATER", "twist": "GIRAR", "pull": "PUXAR",
              "spin": "RODAR", "flick": "PETELECO"},
}
CURATED_TOPICS = {
    "ja": (
        "ようこそ", "Bop It!とは？", "Bop It Accessとは？",
        "AIの使用に関する透明性の説明", "アクセシビリティ機能", "便利な機能",
        "注意事項と制限", "ゲームの購入と動作環境の確認", "Modの入手とインストール",
        "操作一覧", "初めて起動するとき", "対応言語", "ゲーム内でガイドを読む",
        "Mod設定メニュー", "メニュー案内", "メインメニュー", "プレイメニュー",
        "曲と難易度の選択", "設定メニュー", "操作設定メニュー", "音声遅延の調整",
        "ランキング", "実績ブック", "ゲームのクレジット", "結果とゲームオーバー",
        "ポーズメニュー", "各モードの詳しい説明", "今後の予定",
        "クレジットと第三者ソフトウェアの表記", "法的な注意事項", "感謝の言葉",
    ),
    "ko": (
        "환영합니다", "Bop It!은 어떤 게임인가요?", "Bop It Access란?",
        "AI 사용에 관한 투명성 안내", "접근성 기능", "편의 기능",
        "참고 사항과 제한 사항", "게임 구매 및 PC 사양 확인", "모드 다운로드 및 설치",
        "플레이어 조작 목록", "처음 실행하기", "지원 언어", "게임 안에서 가이드 읽기",
        "모드 설정 메뉴", "메뉴 안내", "메인 메뉴", "플레이 메뉴", "노래와 난이도 선택",
        "설정 메뉴", "조작 설정 메뉴", "오디오 지연 보정", "순위표", "도전 과제 책",
        "게임 제작진", "결과 및 게임 오버", "일시 정지 메뉴", "모드별 자세한 안내",
        "향후 계획", "제작진 및 타사 고지", "법적 고지", "감사의 말",
    ),
    "zh": (
        "欢迎", "什么是 Bop It!？", "什么是 Bop It Access？", "人工智能透明度说明",
        "无障碍功能", "便利功能", "注意事项与限制", "购买游戏并检查电脑配置",
        "获取并安装模组", "玩家操作一览", "首次启动", "支持的语言",
        "在游戏中阅读指南", "模组设置菜单", "菜单指南", "主菜单", "游玩菜单",
        "选择歌曲和难度", "设置菜单", "控制设置菜单", "音频延迟校准",
        "排行榜", "成就图册", "游戏制作人员", "结算与游戏结束", "暂停菜单",
        "各模式详细指南", "未来计划", "制作人员及第三方声明", "法律声明", "感谢",
    ),
}
ENGLISH_TOPICS = (
    "Welcome", "What is Bop It!?", "What is Bop It Access?",
    "AI Transparency Note", "Accessibility features", "Quality of life features",
    "Notes and limitations", "Buy the game and check your computer",
    "Get and install the mod", "Player control reference", "Your first launch",
    "Languages", "Read the guide in game", "Mod Settings menu", "Menu guide",
    "Main menu", "Play menu", "Song and difficulty selection", "Settings menu",
    "Controls menu", "Audio latency calibration", "Leaderboards",
    "Achievements book", "Game credits", "Results and game over", "Pause menu",
    "Detailed mode guide", "What may come next",
    "Credits and third-party notices", "Legal notes", "A thank-you",
)


def protect_terms(source: str,
                  action_locale: str | None = None) -> tuple[str, dict[str, str]]:
    originals: dict[str, str] = {}

    def replace(match: re.Match[str]) -> str:
        token = str(987654321000 + len(originals) + 1)
        originals[token] = match.group(0)
        return token

    def replace_action(match: re.Match[str]) -> str:
        token = str(987654321000 + len(originals) + 1)
        action = match.group(0)
        originals[token] = (NATIVE_ACTION_NAMES[action_locale][action.lower()]
            if action_locale else action)
        return token

    protected = WINDOWS_PATH.sub(replace, source)
    protected = RELATIVE_PATH.sub(replace, protected)
    protected = FILE_NAME.sub(replace, protected)
    protected = VERSION.sub(replace, protected)
    protected = PROTECTED_PATTERN.sub(replace, protected)
    protected = STAGE_TITLES.sub(replace, protected)
    # These are distinct gameplay commands, not ordinary verbs. Preserve
    # their exact names so translated instructions never conflate actions.
    protected = ACTION_CUES.sub(replace_action, protected)
    return protected, originals


def regionalize(text: str, locale: str) -> str:
    if locale == "es-MX":
        for source, replacement in (
            (r"\bordenador(?:es)?\b", "computadora"),
            (r"\bOrdenador(?:es)?\b", "Computadora"),
            (r"\bmandos\b", "controles"),
            (r"\bmando\b", "control"),
        ):
            text = re.sub(source, replacement, text)
    elif locale == "pt-BR":
        for source, replacement in (
            (r"\bficheiros\b", "arquivos"),
            (r"\bficheiro\b", "arquivo"),
            (r"\becrãs\b", "telas"),
            (r"\becrã\b", "tela"),
            (r"\butilizadores\b", "usuários"),
            (r"\butilizador\b", "usuário"),
            (r"\bcontrolos\b", "controles"),
            (r"\bcontrolo\b", "controle"),
        ):
            text = re.sub(source, replacement, text)
    return text


class Translator:
    def __init__(self, locale: str, backend: str = "argos"):
        self.locale = locale
        self.target = MODEL_CODE[locale]
        self.backend = backend
        catalog_path = ROOT / "src" / "locales" / f"{locale}.json"
        self.speech_catalog = (json.loads(catalog_path.read_text(
            encoding="utf-8")) if catalog_path.exists() else {})
        self.collecting = False
        self.pending: list[str] = []
        self.pending_set: set[str] = set()
        cache_dir = ROOT / "tools" / "translation-cache"
        cache_dir.mkdir(parents=True, exist_ok=True)
        self.cache_path = cache_dir / (
            f"google-{locale}.json" if backend == "google" else
            f"hybrid-{locale}.json" if backend == "google-cache" else
            f"argos-v3-{locale}.json")
        self.cache = (
            json.loads(self.cache_path.read_text(encoding="utf-8"))
            if self.cache_path.exists()
            else {}
        )
        if backend == "google-cache" and not self.cache:
            google_cache = cache_dir / f"google-{locale}.json"
            if google_cache.exists():
                self.cache = json.loads(google_cache.read_text(
                    encoding="utf-8"))
        if backend == "google":
            # Older drafts translated cue names as ordinary words, sometimes
            # making Twist and Spin identical or Flick a movie. Refresh only
            # affected entries after adding command-name protection.
            for source, translated in list(self.cache.items()):
                if source.count("Bop It") > translated.count("Bop It"):
                    del self.cache[source]
                    continue
                stages = STAGE_TITLES.findall(source)
                if any(translated.count(stage) < stages.count(stage)
                       for stage in set(stages)):
                    del self.cache[source]
                    continue
                without_product_names = PROTECTED_PATTERN.sub("", source)
                cues = [cue.lower() for cue in
                        ACTION_CUES.findall(without_product_names)]
                if any(translated.count(NATIVE_ACTION_NAMES[locale][cue]) <
                       cues.count(cue) for cue in set(cues)):
                    del self.cache[source]
        if not self.cache and backend == "argos":
            previous = cache_dir / f"argos-v2-{locale}.json"
            if previous.exists():
                prior = json.loads(previous.read_text(encoding="utf-8"))
                self.cache = {source: translated
                    for source, translated in prior.items()
                    if not any(pattern.search(source) for pattern in
                        (WINDOWS_PATH, RELATIVE_PATH, FILE_NAME, VERSION))}
        self.new_entries = 0

    def save(self) -> None:
        self.cache_path.write_text(
            json.dumps(self.cache, ensure_ascii=False, indent=2) + "\n",
            encoding="utf-8",
        )

    def text(self, source: str) -> str:
        if not source or not re.search(r"[A-Za-z]", source):
            return source
        leading = source[: len(source) - len(source.lstrip())]
        trailing = source[len(source.rstrip()) :]
        body = source.strip()
        if not body or body in {"Bop It Access", "Bop It!", "Prism"} or \
                body in OUTPUT_MODE_LIST_LINES:
            return source
        if self.locale in CURATED_TOPICS and body in ENGLISH_TOPICS:
            position = ENGLISH_TOPICS.index(body)
            return leading + CURATED_TOPICS[self.locale][position] + trailing
        if body not in self.cache:
            if self.collecting:
                if body not in self.pending_set:
                    self.pending_set.add(body)
                    self.pending.append(body)
                return source
            if self.backend == "google":
                translated = google_translate([body], self.target,
                    self.locale)[0]
            else:
                import argostranslate.translate

                protected, originals = protect_terms(body, self.locale)
                translated = (protected if protected in originals and
                    len(originals) == 1 else argostranslate.translate.translate(
                        protected, "en", self.target))
                for token, original in originals.items():
                    translated = translated.replace(token, original)
                if re.search(r"987654321\d+", translated):
                    # A damaged product/control token is worse than a single
                    # English source line in an otherwise translated draft.
                    translated = body
            self.cache[body] = regionalize(translated, self.locale)
            self.new_entries += 1
            if self.new_entries % 100 == 0:
                self.save()
                print(f"  {self.locale}: translated {self.new_entries} new strings", flush=True)
        translated = self.cache[body]
        if self.locale == "zh":
            translated = translated.replace("欢迎拨打Bop It Access！",
                "欢迎使用 Bop It Access！")
        return leading + translated + trailing

    def fill_pending(self) -> None:
        if not self.pending:
            return
        print(f"  {self.locale}: translating {len(self.pending)} unique strings",
              flush=True)
        batch: list[str] = []
        characters = 0
        for body in self.pending:
            if batch and (characters + len(body) > 3000 or len(batch) >= 30):
                self._fill_batch(batch)
                batch = []
                characters = 0
            batch.append(body)
            characters += len(body)
        if batch:
            self._fill_batch(batch)
        self.pending.clear()
        self.pending_set.clear()
        self.save()

    def _fill_batch(self, batch: list[str]) -> None:
        translated = google_translate(batch, self.target, self.locale)
        for source, result in zip(batch, translated):
            self.cache[source] = regionalize(result, self.locale)
        self.new_entries += len(batch)
        if self.new_entries % 120 < len(batch):
            self.save()
            print(f"  {self.locale}: translated {self.new_entries} strings",
                  flush=True)


def google_translate(strings: list[str], target: str,
                     action_locale: str | None = None) -> list[str]:
    # Offline authoring does not need the Google HTTP client, and Google
    # authoring does not need an installed Argos model/runtime.
    import requests

    separator = "\n9999999999\n"
    protected: list[str] = []
    replacements: list[dict[str, str]] = []
    for source in strings:
        masked, originals = protect_terms(source, action_locale)
        protected.append(masked)
        replacements.append(originals)
    query = separator.join(protected)
    last_error: Exception | None = None
    for attempt in range(5):
        try:
            response = requests.get(
                "https://translate.googleapis.com/translate_a/single",
                params={"client": "gtx", "sl": "en", "tl": target,
                        "dt": "t", "q": query},
                timeout=45,
            )
            response.raise_for_status()
            joined = "".join(part[0] or "" for part in response.json()[0])
            parts = re.split(r"\s*9999999999\s*", joined)
            if len(parts) != len(strings):
                if len(strings) > 1:
                    middle = len(strings) // 2
                    return (google_translate(strings[:middle], target,
                                action_locale) +
                            google_translate(strings[middle:], target,
                                action_locale))
                raise ValueError("Translation response lost its segment boundary")
            for index, result in enumerate(parts):
                for token, original in replacements[index].items():
                    if result.count(token) != protected[index].count(token):
                        raise ValueError("Translation response changed a protected term")
                    result = result.replace(token, original)
                parts[index] = result.strip()
            return parts
        except (requests.RequestException, ValueError, KeyError, IndexError) as error:
            last_error = error
            time.sleep(min(2 ** attempt, 16))
    raise RuntimeError(f"Translation failed: {last_error}")

def translate_html(source: str, translator: Translator) -> str:
    soup = BeautifulSoup(source, "html.parser")
    html = soup.find("html")
    if html:
        html["lang"] = translator.locale
    if soup.find("nav", attrs={"aria-label": "Table of contents"}):
        soup.find("nav", attrs={"aria-label": "Table of contents"})["id"] = "contents-nav"
    for tag in soup.find_all(True):
        for attribute in ("alt", "aria-label", "title"):
            if tag.has_attr(attribute):
                tag[attribute] = translator.text(str(tag[attribute]))
    for node in list(soup.find_all(string=True)):
        if not isinstance(node, NavigableString) or isinstance(node,
            (Comment, Declaration, Doctype, ProcessingInstruction)):
            continue
        if node.parent and node.parent.name in SKIP_HTML:
            continue
        if node.parent and node.parent.name == "span" and any(
                kind in node.parent.get("class", [])
                for kind in ("version", "kind")):
            # Git-style metadata and exact version identifiers are technical
            # tokens, not prose for translation.
            continue
        translated = None
        if node.parent and node.parent.name in {"td", "th"}:
            source_cell = str(node).strip()
            binding_cell = node.find_parent("section", id="controls")
            catalog_key = ("Space key" if binding_cell and
                source_cell == "Space" else source_cell)
            if source_cell and catalog_key in translator.speech_catalog:
                leading = str(node)[:len(str(node)) - len(str(node).lstrip())]
                trailing = str(node)[len(str(node).rstrip()):]
                translated = (leading +
                    translator.speech_catalog[catalog_key] + trailing)
        if translated is None:
            translated = translator.text(str(node))
        if translated != str(node):
            node.replace_with(translated)
    return str(soup).rstrip("\n") + "\n"


def translate_markdown_piece(source: str, translator: Translator) -> str:
    if source.startswith("[") and "](" in source and source.endswith(")"):
        separator = source.index("](")
        label = source[1:separator]
        # Backticked link labels are literal paths or filenames. Translating
        # them can corrupt both the code formatting and the displayed path.
        if label.startswith("`") and label.endswith("`"):
            return source
        return "[" + translator.text(label) + source[separator:]
    if source.startswith("`") or source.startswith("http"):
        return source
    return translator.text(source)


def translate_markdown_line(source: str, translator: Translator) -> str:
    if not source.strip() or SEPARATOR_LINE.fullmatch(source):
        return source
    if WINDOWS_PATH.search(source):
        # A bare install path is an exact instruction, not translatable prose.
        return source
    prefix_match = MARKDOWN_PREFIX.match(source)
    prefix = prefix_match.group(0) if prefix_match else ""
    content = source[len(prefix) :]
    if content.startswith("|") and content.endswith("|"):
        cells = content[1:-1].split("|")
        return prefix + "|" + "|".join(
            translate_markdown_line(cell, translator) for cell in cells
        ) + "|"
    return prefix + "".join(
        translate_markdown_piece(part, translator)
        for part in MARKDOWN_PROTECTED.split(content)
    )


def translate_markdown(source: str, translator: Translator) -> str:
    output = []
    in_fence = False
    for line in source.splitlines(keepends=True):
        end = "\n" if line.endswith("\n") else ""
        body = line[:-1] if end else line
        if body.lstrip().startswith("```"):
            in_fence = not in_fence
            output.append(line)
        elif in_fence:
            output.append(line)
        else:
            output.append(translate_markdown_line(body, translator) + end)
    return "".join(output)


def polish_japanese_guide(html: str) -> str:
    """Keep reviewed menu names and the first-use explanation in sync with the game."""
    soup = BeautifulSoup(html, "html.parser")
    welcome = soup.find("section", id="welcome")
    if welcome:
        paragraphs = welcome.find_all("p", recursive=False)
        if len(paragraphs) > 1:
            paragraphs[1].replace_with(BeautifulSoup(
                '<p><strong>キーボード</strong>はコンピューターのキーをまとめたものです。'
                '<strong>コントローラー</strong>は手に持って使うゲームパッドです。'
                'このガイドの Xbox ボタン名は一例で、ほかのコントローラーでは同じ位置の'
                'ボタンに別の名前が付いている場合があります。<strong>メニュー項目</strong>は'
                '「プレイ」や「設定」などの選択肢です。現在選ばれている項目に'
                '<strong>フォーカス</strong>があり、決定ボタンを押すとその項目が実行されます。</p>',
                "html.parser").p)
    row_names = {
        "main-menu": {"遊ぶ": "プレイ", "業績": "実績", "やめる": "終了"},
        "play-menu": {"パスイット": "バトンタッチ"},
    }
    for section_id, names in row_names.items():
        section = soup.find("section", id=section_id)
        if section:
            for heading in section.find_all("th", attrs={"scope": "row"}):
                text = heading.get_text(strip=True)
                if text in names:
                    heading.string = names[text]
    pass_it = soup.find("section", id="pass-it-mode")
    if pass_it:
        heading = pass_it.find("h3")
        if heading:
            heading.string = "バトンタッチ"
        paragraph = pass_it.find("p")
        if paragraph:
            paragraph.replace_with(BeautifulSoup(
                '<p>バトンタッチは、みんなで一つの記録に挑戦するモードです。'
                'ゲームが<q>バトンタッチ！</q>と指示するまで、まず一人がコマンドに応えます。'
                'その指示が聞こえたら、すみやかに次の人へコントローラーを渡してください。'
                '次のプレイヤーは<em>同じ</em>連続記録を引き継ぎます。'
                '以後もゲームの指示に従ってコントローラーを渡し、コマンドに応え続けます。'
                '誰に渡すかを先に決め、安全に受け渡せる場所で遊びましょう。</p>',
                "html.parser").p)
    for node in list(soup.find_all(string=True)):
        if "パスイット" in node:
            node.replace_with(str(node).replace("パスイット", "バトンタッチ"))
    return str(soup).rstrip("\n") + "\n"


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("locale", choices=MODEL_CODE)
    parser.add_argument("--backend", choices=("argos", "google",
                        "google-cache"),
                        default="argos")
    args = parser.parse_args()
    locale = args.locale
    translator = Translator(locale, args.backend)
    destination = ROOT / "documentation" / locale
    destination.mkdir(parents=True, exist_ok=True)
    if args.backend == "google":
        translator.collecting = True
        for filename in DOCUMENTS:
            source = (ROOT / filename).read_text(encoding="utf-8")
            if filename.endswith(".html"):
                translate_html(source, translator)
            else:
                translate_markdown(source, translator)
        translator.collecting = False
        translator.fill_pending()
    for filename in DOCUMENTS:
        source = (ROOT / filename).read_text(encoding="utf-8")
        translated = (
            translate_html(source, translator)
            if filename.endswith(".html")
            else translate_markdown(source, translator)
        )
        if locale == "ja" and filename == "BopItAccess-user-guide.html":
            translated = polish_japanese_guide(translated)
        if filename == "README.md":
            # From a language folder, the parent is documentation/.
            translated = translated.replace("(documentation/", "(../")
            # The HTML guide links into these sections. Markdown heading
            # slugs change when their text is translated, so give every
            # localized README stable, language-independent targets.
            source_lines = source.splitlines(keepends=True)
            translated_lines = translated.splitlines(keepends=True)
            if len(source_lines) != len(translated_lines):
                raise ValueError("README line count changed during translation")
            for index, line in enumerate(source_lines):
                anchor = {
                    "## Build from source": "build-from-source",
                    "## Install your build": "install-your-build",
                }.get(line.rstrip("\r\n"))
                if anchor:
                    translated_lines[index] = (
                        f'<a id="{anchor}"></a>\n' + translated_lines[index]
                    )
            translated = "".join(translated_lines)
        (destination / filename).write_bytes(translated.encode("utf-8"))
        print(f"  {locale}: wrote {filename}", flush=True)
        translator.save()


if __name__ == "__main__":
    main()
