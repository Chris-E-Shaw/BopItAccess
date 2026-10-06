"""Legacy v0.9 migration for short Prism installation instructions.

This preserves the original migration text for historical authoring. Its
backend descriptions and positional replacements predate later OneCore and
installer changes. Do not use it to update current project documents.
An explicit --legacy-migration flag is required to run the archived workflow.
"""

from __future__ import annotations

import html
import argparse
import re
from pathlib import Path

from bs4 import BeautifulSoup

ROOT = Path(__file__).resolve().parents[1]
RELEASES = "https://github.com/ethindp/prism/releases"
MODES = (
    "Auto, SAPI, OneCore, NVDA, JAWS, UI Automation, ZDSR, ZoomText,\n"
    "Boy PC Reader, PC Talker, Sense Reader, System Access, Window-Eyes."
)

COPY = {
    "de": {
        "install": f'Beziehen Sie die offizielle Windows-x64-Version von Prism v0.18.3 (<code>prism.dll</code>) von den <a href="{RELEASES}">Prism-Releases</a> oder erstellen Sie dieselbe Version aus dem Quellcode. Legen Sie <code>prism.dll</code> neben die ausführbare Spieldatei im Hauptordner des Spiels, <strong>nicht in den Ordner <code>Mods</code></strong>.',
        "launch": "Starten Sie Ihren Screenreader, falls Sie einen verwenden, und starten Sie danach das Spiel über Steam. Wenn kein unterstützter Screenreader läuft, kann Prism SAPI für die Sprachausgabe verwenden.",
        "first": "Falls Sie einen Screenreader verwenden, starten Sie ihn zuerst. Installieren Sie MelonLoader, die Mod-DLL und <code>prism.dll</code>, und starten Sie Bop It! anschließend über Steam.",
        "mode": "<code>Auto</code> verwendet über Prism einen laufenden unterstützten Screenreader; ist keiner verfügbar, nutzt es SAPI. <code>OneCore</code> kann separat gewählt werden. Ist die gewählte Ausgabe nicht verfügbar, wird automatisch eine verfügbare Ausgabe verwendet und dies einmal angesagt. Die Tastenkombination „Sprachausgabe wechseln“ ändert dieselbe Einstellung.",
        "notice": 'Laufzeitdetails finden Sie in den <a href="THIRD-PARTY-NOTICES.txt">Hinweisen zu Drittanbietern</a>. Das aktuelle GitHub-Repository enthält <strong>weder</strong> die kompilierte Mod-DLL <strong>noch</strong> <code>prism.dll</code>.',
        "mode_intro": "OUTPUT MODE beginnt mit Auto: Über Prism wird ein laufender unterstützter Screenreader verwendet; ist keiner verfügbar, wird SAPI verwendet. OneCore kann separat gewählt werden.",
        "availability": "Die einzelnen Screenreader und Sprach-Engines sind nur verfügbar, wenn die installierte Prism-Version und das System sie unterstützen. Ist der gewählte Modus nicht verfügbar, wechselt der Mod automatisch zu einer verfügbaren Ausgabe und sagt dies einmal an.",
        "note": "Prism-Backends können nicht unterbrechende Ansagen unterschiedlich einreihen. Bitte melden Sie Punktestände oder Credits, die in falscher Reihenfolge gesprochen werden. Gespeicherte SAPI-Stimmkennungen werden mit den Anzeigenamen in Prism abgeglichen; bei gleichen Namen kann die erste passende Stimme gewählt werden.",
        "note_label": "Die Reihenfolge von Ansagen kann variieren.",
    },
    "es": {
        "install": f'Obtenga la versión oficial de Prism v0.18.3 para Windows x64 (<code>prism.dll</code>) en las <a href="{RELEASES}">versiones publicadas de Prism</a>, o compile esa misma versión desde el código fuente. Coloque <code>prism.dll</code> junto al ejecutable del juego, en la carpeta principal del juego, <strong>no en la carpeta <code>Mods</code></strong>.',
        "launch": "Si utiliza un lector de pantalla, inícielo primero. Después, abra el juego desde Steam. Si no hay un lector de pantalla compatible en ejecución, Prism puede utilizar SAPI para la voz.",
        "first": "Si utiliza un lector de pantalla, inícielo primero. Instale MelonLoader, la DLL del mod y <code>prism.dll</code>; después, abra Bop It! desde Steam.",
        "mode": "<code>Auto</code> utiliza a través de Prism un lector de pantalla compatible que esté en ejecución; si no hay ninguno, utiliza SAPI. <code>OneCore</code> se elige por separado. Si la salida seleccionada no está disponible, se usa automáticamente otra disponible y se anuncia una sola vez. El atajo Cambiar salida de voz recorre las mismas opciones.",
        "notice": 'Consulte los <a href="THIRD-PARTY-NOTICES.txt">avisos de terceros</a> para conocer los componentes necesarios durante la ejecución. El repositorio actual de GitHub <strong>no incluye</strong> la DLL compilada del mod ni <code>prism.dll</code>.',
        "mode_intro": "OUTPUT MODE comienza en Auto: utiliza a través de Prism un lector de pantalla compatible en ejecución y, si no hay ninguno, SAPI. OneCore se puede elegir por separado.",
        "availability": "Cada lector de pantalla y motor de voz solo estará disponible si lo admiten la versión de Prism instalada y el sistema del jugador. Si el modo elegido no está disponible, el mod cambia a una salida disponible y lo anuncia una sola vez.",
        "note": "Los motores de Prism pueden poner en cola de forma diferente los avisos que no interrumpen la voz. Informe si los resultados o créditos se anuncian fuera de orden. Las voces SAPI guardadas se buscan por el nombre que muestra Prism; si varias comparten nombre, puede elegirse la primera.",
        "note_label": "El orden de los avisos puede variar.",
    },
    "fr": {
        "install": f'Obtenez la version officielle de Prism v0.18.3 pour Windows x64 (<code>prism.dll</code>) sur la <a href="{RELEASES}">page des versions de Prism</a>, ou compilez cette même version depuis les sources. Placez <code>prism.dll</code> à côté du fichier exécutable du jeu, dans le dossier principal du jeu, <strong>pas dans le dossier <code>Mods</code></strong>.',
        "launch": "Si vous utilisez un lecteur d’écran, démarrez-le avant le jeu, puis lancez le jeu depuis Steam. Si aucun lecteur d’écran compatible n’est actif, Prism peut utiliser la voix SAPI.",
        "first": "Si vous utilisez un lecteur d’écran, démarrez-le d’abord. Installez MelonLoader, la DLL du mod et <code>prism.dll</code>, puis lancez Bop It! depuis Steam.",
        "mode": "<code>Auto</code> utilise, via Prism, un lecteur d’écran compatible en cours d’exécution ; si aucun n’est disponible, il utilise SAPI. <code>OneCore</code> est un choix distinct. Si la sortie choisie est indisponible, le mod passe automatiquement à une sortie disponible et l’annonce une seule fois. Le raccourci Changer la sortie vocale parcourt les mêmes choix.",
        "notice": 'Consultez les <a href="THIRD-PARTY-NOTICES.txt">mentions relatives aux logiciels tiers</a> pour les composants requis à l’exécution. Le dépôt GitHub actuel ne contient <strong>ni</strong> la DLL compilée du mod <strong>ni</strong> <code>prism.dll</code>.',
        "mode_intro": "OUTPUT MODE démarre sur Auto : via Prism, le mod utilise un lecteur d’écran compatible en cours d’exécution ou SAPI si aucun n’est disponible. OneCore peut être choisi séparément.",
        "availability": "Chaque lecteur d’écran et moteur vocal n’est disponible que si la version de Prism installée et le système du joueur le prennent en charge. Si le mode choisi est indisponible, le mod utilise une sortie disponible et l’annonce une seule fois.",
        "note": "Les moteurs Prism peuvent mettre en attente différemment les annonces qui n’interrompent pas la parole. Signalez les scores ou crédits annoncés dans le désordre. Les voix SAPI enregistrées sont recherchées par le nom affiché dans Prism ; si plusieurs voix portent ce nom, la première peut être choisie.",
        "note_label": "L’ordre des annonces peut varier.",
    },
    "it": {
        "install": f'Ottieni la versione ufficiale di Prism v0.18.3 per Windows x64 (<code>prism.dll</code>) dalla <a href="{RELEASES}">pagina delle versioni di Prism</a>, oppure compila la stessa versione dal codice sorgente. Metti <code>prism.dll</code> accanto al file eseguibile del gioco, nella cartella principale del gioco, <strong>non nella cartella <code>Mods</code></strong>.',
        "launch": "Se usi uno screen reader, avvialo prima del gioco, poi avvia il gioco tramite Steam. Se non è in esecuzione uno screen reader compatibile, Prism può usare la voce SAPI.",
        "first": "Se usi uno screen reader, avvialo per primo. Installa MelonLoader, la DLL della mod e <code>prism.dll</code>, quindi avvia Bop It! tramite Steam.",
        "mode": "<code>Auto</code> usa tramite Prism uno screen reader compatibile in esecuzione; se non ce n’è uno, usa SAPI. <code>OneCore</code> si può scegliere separatamente. Se l’uscita selezionata non è disponibile, la mod passa automaticamente a una disponibile e lo comunica una sola volta. La scorciatoia Cambia uscita vocale scorre le stesse opzioni.",
        "notice": 'Consulta gli <a href="THIRD-PARTY-NOTICES.txt">avvisi sui componenti di terze parti</a> per i dettagli di esecuzione. L’attuale repository GitHub <strong>non contiene</strong> la DLL compilata della mod né <code>prism.dll</code>.',
        "mode_intro": "OUTPUT MODE parte da Auto: tramite Prism, la mod usa uno screen reader compatibile in esecuzione oppure SAPI se non ce n’è uno. OneCore è una scelta separata.",
        "availability": "Ogni screen reader e motore vocale è disponibile solo se supportato dalla versione di Prism installata e dal sistema del giocatore. Se la modalità scelta non è disponibile, la mod usa un’uscita disponibile e lo comunica una sola volta.",
        "note": "I motori Prism possono accodare in modo diverso gli annunci che non interrompono la voce. Segnala se punteggi o crediti vengono letti fuori ordine. Le voci SAPI salvate vengono cercate tramite il nome mostrato da Prism; se più voci hanno lo stesso nome, può essere scelta la prima.",
        "note_label": "L’ordine degli annunci può variare.",
    },
    "ja": {
        "install": f'<a href="{RELEASES}">Prism のリリースページ</a>から、Windows x64 用の公式 Prism v0.18.3 <code>prism.dll</code> を入手します。同じバージョンをソースからビルドすることもできます。<code>prism.dll</code> はゲームの実行ファイルと同じフォルダーに置き、<strong><code>Mods</code> フォルダーには入れないでください</strong>。',
        "launch": "スクリーンリーダーを使用する場合は先に起動し、Steam でゲームを起動します。対応するスクリーンリーダーが実行されていない場合、Prism は SAPI の音声を使用できます。",
        "first": "スクリーンリーダーを使用する場合は、先に起動します。MelonLoader、MOD の DLL、<code>prism.dll</code> をインストールしてから、Steam で Bop It! を起動します。",
        "mode": "<code>Auto</code> は Prism を通じて実行中の対応スクリーンリーダーを使い、ない場合は SAPI を使います。<code>OneCore</code> は個別に選択できます。選んだ出力が利用できない場合は、利用可能な出力に自動で切り替え、一度だけ音声で知らせます。「音声出力の変更」ショートカットでも同じ設定を切り替えられます。",
        "notice": '実行時に必要なファイルについては、プロジェクトの <a href="THIRD-PARTY-NOTICES.txt">第三者ソフトウェアの通知</a>を参照してください。現在の GitHub リポジトリには、コンパイル済みの MOD DLL と <code>prism.dll</code> は<strong>含まれていません</strong>。',
        "mode_intro": "OUTPUT MODE の初期設定は Auto です。実行中の対応スクリーンリーダーを Prism 経由で使用し、利用できない場合は SAPI を使用します。OneCore は個別に選べます。",
        "availability": "各スクリーンリーダーと音声エンジンを使用できるかどうかは、インストールした Prism とプレイヤーのシステムによります。選んだモードを利用できない場合は、利用可能な出力へ自動で切り替わり、そのことを MOD が一度だけ音声で知らせます。",
        "note": "Prism の出力方式によって、ほかの音声を中断しない通知の順番が異なる場合があります。スコアやクレジットの読み上げ順が乱れる場合は報告してください。保存された SAPI 音声の ID は Prism に表示される音声名と照合します。同じ名前の音声が複数ある場合、最初に一致した音声が選ばれることがあります。",
        "note_label": "読み上げ順は出力方式によって異なる場合があります。",
    },
    "ko": {
        "intro": 'Bop It Access는 Steam에서 판매하는 <strong>Windows x64 버전 Bop It!</strong>용 비공식 모드입니다. 이 모드는 메뉴와 결과 화면의 음성 및 점자 출력, 네 가지 스테이지의 설명, 추가 조작 방법, 프레임 제한 등의 설정을 제공합니다. 게임의 음성 명령과 진행 시간을 유지하면서 더 쉽게 탐색할 수 있도록 돕습니다.',
        "install": f'<a href="{RELEASES}">Prism 릴리스 페이지</a>에서 Windows x64용 공식 Prism v0.18.3 <code>prism.dll</code>을 받거나 같은 버전을 소스에서 빌드합니다. <code>prism.dll</code>은 게임 실행 파일이 있는 기본 게임 폴더에 놓으세요. <strong><code>Mods</code> 폴더 안에는 넣지 마세요</strong>.',
        "launch": "스크린 리더를 사용한다면 먼저 실행한 뒤 Steam에서 게임을 시작하세요. 호환되는 스크린 리더가 실행 중이지 않으면 Prism이 SAPI 음성을 사용할 수 있습니다.",
        "first": "스크린 리더를 사용한다면 먼저 실행하세요. MelonLoader, 모드 DLL, <code>prism.dll</code>을 설치한 후 Steam에서 Bop It!을 시작하세요.",
        "mode": "<code>Auto</code>는 Prism을 통해 실행 중인 호환 스크린 리더를 사용하고, 없으면 SAPI를 사용합니다. <code>OneCore</code>는 별도로 선택할 수 있습니다. 선택한 출력 방식을 사용할 수 없으면 사용 가능한 방식으로 자동 전환하고 한 번 음성으로 알립니다. 음성 출력 변경 단축키도 같은 설정을 순환합니다.",
        "notice": '실행에 필요한 구성 요소는 <a href="THIRD-PARTY-NOTICES.txt">타사 소프트웨어 고지</a>를 참고하세요. 현재 GitHub 저장소에는 컴파일된 모드 DLL이나 <code>prism.dll</code>이 <strong>포함되어 있지 않습니다</strong>.',
        "mode_intro": "OUTPUT MODE의 기본값은 Auto입니다. Prism을 통해 실행 중인 호환 스크린 리더를 사용하고, 없으면 SAPI를 사용합니다. OneCore는 별도로 선택할 수 있습니다.",
        "availability": "각 스크린 리더와 음성 엔진은 설치된 Prism 빌드와 플레이어의 시스템이 지원할 때만 사용할 수 있습니다. 선택한 방식을 사용할 수 없으면 사용 가능한 방식으로 자동 전환하고 한 번 음성으로 알립니다.",
        "note": "Prism 출력 방식마다 다른 음성을 끊지 않는 안내의 대기 순서가 다를 수 있습니다. 점수나 크레딧이 잘못된 순서로 읽히면 알려 주세요. 저장된 SAPI 음성 ID는 Prism에 표시되는 이름과 대조합니다. 같은 이름의 음성이 여러 개면 처음 일치하는 음성이 선택될 수 있습니다.",
        "note_label": "안내 순서는 출력 방식에 따라 다를 수 있습니다.",
    },
    "pt-BR": {
        "install": f'Obtenha a versão oficial do Prism v0.18.3 para Windows x64 (<code>prism.dll</code>) na <a href="{RELEASES}">página de lançamentos do Prism</a>, ou compile a mesma versão a partir do código-fonte. Coloque <code>prism.dll</code> ao lado do executável do jogo, na pasta principal do jogo, <strong>não na pasta <code>Mods</code></strong>.',
        "launch": "Se você usa um leitor de tela, inicie-o antes do jogo. Depois, abra o jogo pelo Steam. Se não houver um leitor de tela compatível em execução, o Prism poderá usar a voz SAPI.",
        "first": "Se você usa um leitor de tela, inicie-o primeiro. Instale MelonLoader, a DLL do mod e <code>prism.dll</code>; depois, abra Bop It! pelo Steam.",
        "mode": "<code>Auto</code> usa, pelo Prism, um leitor de tela compatível em execução; se não houver nenhum, usa SAPI. <code>OneCore</code> pode ser escolhido separadamente. Se a saída selecionada não estiver disponível, o mod muda automaticamente para outra disponível e avisa por voz uma única vez. O atalho Alterar saída de voz percorre as mesmas opções.",
        "notice": 'Consulte os <a href="THIRD-PARTY-NOTICES.txt">avisos sobre componentes de terceiros</a> para saber quais arquivos são necessários durante a execução. O repositório atual do GitHub <strong>não inclui</strong> a DLL compilada do mod nem <code>prism.dll</code>.',
        "mode_intro": "OUTPUT MODE começa em Auto: pelo Prism, o mod usa um leitor de tela compatível em execução ou SAPI se nenhum estiver disponível. OneCore pode ser escolhido separadamente.",
        "availability": "Cada leitor de tela e mecanismo de voz só está disponível se for compatível com a versão do Prism instalada e com o sistema do jogador. Se o modo escolhido não estiver disponível, o mod muda para uma saída disponível e avisa por voz uma única vez.",
        "note": "Os mecanismos do Prism podem enfileirar de modo diferente avisos que não interrompem a fala. Informe se pontuações ou créditos forem lidos fora de ordem. As vozes SAPI salvas são localizadas pelo nome exibido no Prism; se várias tiverem o mesmo nome, a primeira poderá ser escolhida.",
        "note_label": "A ordem dos avisos pode variar.",
    },
    "zh": {
        "intro": 'Bop It Access 是针对 Steam 上的 <strong>Windows x64 版《Bop It!》</strong>制作的非官方模组。它为菜单和结果提供语音与盲文输出，还增加了四个关卡的按需说明、额外控制方式和帧率限制等设置。模组旨在方便玩家操作，同时保留游戏原有的语音指令和节奏。',
        "install": f'从 <a href="{RELEASES}">Prism 发布页面</a>获取适用于 Windows x64 的官方 Prism v0.18.3 <code>prism.dll</code>，或从源代码构建相同版本。将 <code>prism.dll</code> 放在游戏主文件夹中，与游戏可执行文件放在一起，<strong>不要放进 <code>Mods</code> 文件夹</strong>。',
        "launch": "如果使用屏幕阅读器，请先启动它，再通过 Steam 启动游戏。如果没有运行兼容的屏幕阅读器，Prism 可以使用 SAPI 语音。",
        "first": "如果使用屏幕阅读器，请先启动它。安装 MelonLoader、模组 DLL 和 <code>prism.dll</code> 后，通过 Steam 启动 Bop It!。",
        "mode": "<code>Auto</code> 通过 Prism 使用正在运行的兼容屏幕阅读器；如果没有，则使用 SAPI。<code>OneCore</code> 可单独选择。如果所选输出不可用，模组会自动切换到可用的输出，并通过语音提示一次。“切换语音输出”快捷键也会轮换这些选项。",
        "notice": '有关运行时所需文件，请参阅<a href="THIRD-PARTY-NOTICES.txt">第三方软件声明</a>。当前 GitHub 仓库<strong>不包含</strong>编译后的模组 DLL 或 <code>prism.dll</code>。',
        "mode_intro": "OUTPUT MODE 默认为 Auto：模组通过 Prism 使用正在运行的兼容屏幕阅读器；如果没有，则使用 SAPI。OneCore 可单独选择。",
        "availability": "各屏幕阅读器和语音引擎只有在已安装的 Prism 版本及玩家系统支持时才能使用。如果所选模式不可用，模组会自动切换到可用输出，并通过语音提示一次。",
        "note": "不同的 Prism 输出方式可能以不同顺序排队播放不中断当前语音的提示。如果分数或制作人员名单的朗读顺序不对，请反馈。已保存的 SAPI 语音 ID 会与 Prism 显示的语音名称匹配；如果多个语音同名，可能选中第一个匹配项。",
        "note_label": "提示的朗读顺序可能因输出方式而异。",
    },
}
COPY["es-MX"] = COPY["es"]


def replace_tag(old, markup: str) -> None:
    old.replace_with(BeautifulSoup(markup, "html.parser").find())


def markdown_from_html(markup: str) -> str:
    markup = re.sub(
        r'<a href="([^"]+)">([^<]+)</a>',
        lambda match: f"[{match.group(2)}]({match.group(1)})",
        markup,
    )
    markup = markup.replace("<code>", chr(96)).replace("</code>", chr(96))
    markup = markup.replace("<strong>", "").replace("</strong>", "")
    return html.unescape(markup)


def polish_guide(path: Path, copy: dict[str, str]) -> None:
    soup = BeautifulSoup(path.read_text(encoding="utf-8"), "html.parser")
    if intro := copy.get("intro"):
        replace_tag(soup.select("#mod > p")[0], f"<p>{intro}</p>")
    install = soup.select_one("#install")
    prism_step = next(
        li for li in install.find_all("li")
        if li.find("a", href=RELEASES)
    )
    launch_step = prism_step.find_next_sibling("li")
    replace_tag(launch_step, f'<li>{copy["launch"]}</li>')
    replace_tag(prism_step, f'<li>{copy["install"]}</li>')
    replace_tag(soup.select_one("#first-launch ol li"), f'<li>{copy["first"]}</li>')
    row = next(
        row for row in soup.select("#speech-options tr")
        if row.find("code", string="OneCore") and row.find("th", scope="row")
    )
    replace_tag(row.find_all("td")[-1], f'<td>{copy["mode"]}</td>')
    replace_tag(
        soup.select("#project-credits > p")[-1],
        f'<p>{copy["notice"]}</p>',
    )
    limitations = soup.select_one("#limitations ul")
    if not limitations.find("li", attrs={"data-prism-note": "true"}):
        sapi_item = limitations.find_all("li", recursive=False)[1]
        note = (
            f'<li data-prism-note="true"><strong>{copy["note_label"]}</strong> '
            f'{copy["note"]}</li>'
        )
        sapi_item.insert_after(BeautifulSoup(note, "html.parser").li)
    path.write_text(str(soup).rstrip("\n") + "\n", encoding="utf-8")


def polish_readme(path: Path, copy: dict[str, str]) -> None:
    lines = path.read_text(encoding="utf-8").splitlines(keepends=True)
    for index, line in enumerate(lines):
        if line.startswith("2. ") and "prism.dll" in line:
            lines[index] = "2. " + markdown_from_html(copy["install"]) + "\n"
        if line.startswith("4. ") and "Steam" in line:
            lines[index] = "4. " + copy["launch"] + "\n"
    path.write_text("".join(lines), encoding="utf-8")


def polish_readme_txt(path: Path, copy: dict[str, str], locale: str) -> None:
    lines = path.read_text(encoding="utf-8").splitlines()
    if copy["note"] in "\n".join(lines):
        return
    modes_line = next(
        index for index, line in enumerate(lines)
        if line.startswith("Auto, SAPI, OneCore, NVDA, JAWS")
    )
    start = modes_line - 3
    end = modes_line + (5 if locale == "ja" else 6)
    replacement = [
        copy["mode_intro"],
        "Available output modes:" if locale == "en" else "",
        *MODES.splitlines(),
        copy["availability"],
        copy["note"],
    ]
    replacement = [line for line in replacement if line]
    lines[start:end] = replacement
    path.write_text("\n".join(lines) + "\n", encoding="utf-8")


def polish_locale(locale: str) -> None:
    copy = COPY[locale]
    folder = ROOT / "documentation" / locale
    polish_guide(folder / "BopItAccess-user-guide.html", copy)
    polish_readme(folder / "README.md", copy)
    polish_readme_txt(folder / "README.txt", copy, locale)


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--legacy-migration", action="store_true",
                        help="Apply historical v0.9 text, replacing current documentation")
    args = parser.parse_args()
    if not args.legacy_migration:
        parser.error("This archived migration requires --legacy-migration; it is not a current documentation updater")
    for language in COPY:
        polish_locale(language)
        print(f"{language}: polished Prism documentation", flush=True)
