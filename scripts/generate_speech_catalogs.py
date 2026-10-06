"""Generate speech catalogs using installed offline Argos language models.

This authoring tool reads mod source and a small glossary of short labels from
the installed game's Unity Localization tables. It never contacts a network
service. Game assets are not copied into the repository.

Usage (from repository root):
    tools/translation-py310/Scripts/python.exe scripts/generate_speech_catalogs.py
"""

from __future__ import annotations

import argparse
import json
import os
import re
import sys
from collections import Counter
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
LOCALES = {
    "fr": "fr", "it": "it", "de": "de", "es": "es", "es-MX": "es",
    "ja": "ja", "ko": "ko", "zh": "zh", "pt-BR": "pt",
}
LITERAL_CALL = re.compile(r'\b(?:L|LF)\(\s*("(?:\\.|[^"\\])*")')
STRING_LITERAL = re.compile(r'"(?:\\.|[^"\\])*"')
DESCRIPTION = re.compile(
    r'private const string \w+Description\s*=\s*(.*?);', re.DOTALL)
PLACEHOLDER = re.compile(r"\{\d+\}")
PRODUCTS = re.compile(
    r"Bop It Access|Bop It!|Bop It|MelonLoader|Window-Eyes|System Access|"
    r"ZoomText|Steam|SAPI|NVDA|JAWS|Tolk|Prism|OneCore|UI Automation|"
    r"ZDSR|Boy PC Reader|PC Talker|Sense Reader|(?<![A-Za-z])F8(?![A-Za-z])|"
    r"(?<![A-Za-z])F9(?![A-Za-z])|(?<![A-Za-z])LT(?![A-Za-z])|"
    r"(?<![A-Za-z])RT(?![A-Za-z])"
)
PUNCTUATION = {".", ",", ", ", ":", ";", "?", "!", " ", "—", "-"}

# Dynamic values passed to L(variable), legacy speech fragments, native labels,
# and critical words that can appear at the start of composed announcements.
EXTRA_KEYS = {
    "Bop", "Twist", "Pull", "Spin", "Flick", "Bop, Player 2",
    "BOP", "TWIST", "PULL", "SPIN", "FLICK",
    "BOP IT", "TWIST IT", "PULL IT", "SPIN IT", "FLICK IT",
    "Space", "City", "Office", "Shapes", "Extreme", "Classic", "Green",
    "Yellow", "PLAY", "LEADERBOARDS", "ACHIEVEMENTS", "SETTINGS", "CREDITS",
    "QUIT", "SOLO", "PARTY", "PASS IT", "ONE ON ONE", "MUSIC", "SFX",
    "VOICE OVER", "LANGUAGE", "VIBRATION", "FULLSCREEN", "RESOLUTION",
    "AUDIO LATENCY", "CONTROLS", "REPLAY", "BACK", "SCORE",
    "LOCAL", "FRIENDS", "GLOBAL", "TODAY", "THIS MONTH", "ALL TIME",
    "On", "Off", "Auto", "Automatic", "Keyboard", "Controller", "Both",
    "None", "Never", "Disabled", "Enabled", "Unlimited", "UNLIMITED",
    "button", "toggle", "slider", "tab", "list item", "text field",
    "Button", "Toggle", "Slider", "Tab", "List item", "Text field",
    "Left stick", "Right stick", "D-Pad", "Up Arrow", "Down Arrow",
    "Left Arrow", "Right Arrow", "Space", "Enter", "Escape", "Backspace",
    "Page Up", "Page Down", "Select", "Start", "Left Trigger",
    "Right Trigger", "Left Shoulder", "Right Shoulder", "F8", "F9",
    "SAPI", "OneCore", "NVDA", "JAWS", "UI Automation", "ZDSR",
    "ZoomText", "Boy PC Reader", "PC Talker", "Sense Reader",
    "System Access", "Window-Eyes",
    "Bop It Access speech is off. ",
    "Speech off. ", "Speech is off. ", "Speech on.",
    "{0} on keyboard or {1} on controller, turn speech back on.",
    "Score: {0}.", "Green wins", "Yellow wins",
    "Are you sure? Press again to confirm.",
    "No scores available.", "The user's guide is unavailable because the game menu is not ready.",
    "SPEECH OUTPUT", "BRAILLE OUTPUT", "MUTE SPEECH IN BACKGROUND",
    "INDEXING", "FILTER CAPITALISATION", "SPEAK CONTROL TYPES",
    "SLIDER RANGES", "ONE-ON-ONE FEEDBACK", "HINTS TYPE",
    "AUTO-SPEAK BUTTON HINTS", "BUTTON HINTS DELAY",
    "REPEAT BUTTON HINTS", "REPEAT INTERVAL", "OUTPUT MODE",
    "SAPI VOICE", "SAPI VOLUME", "SAPI RATE", "SAPI PITCH",
    "RESET WELCOME SCREEN", "RESTORE MOD DEFAULTS", "OPEN USER'S GUIDE",
    "MUTE AUDIO IN BACKGROUND", "GROUP PREVIOUS", "GROUP NEXT",
    "DATE PREVIOUS", "DATE NEXT", "RESET GYRO", "READ DESCRIPTIONS",
    "READ SCORE", "TOGGLE SPEECH", "SPEAK HINTS", "CHANGE SPEECH OUTPUT",
    "Leaderboard group previous", "Leaderboard group next",
    "Leaderboard date previous", "Leaderboard date next",
    "This month", "All time", "Local", "Friends", "Global", "Today",
    "open main menu", "activate item", "choose item", "return",
    "activate option", "choose Calibrate or Back", "return to Settings",
    "cancel rebinding", "restore default bindings", "reassign control",
    "choose control", "return to main menu", "resume game",
    "choose pause option", "confirm name", "continue",
    "return to result screen", "read current page", "close book",
    "read credits line by line", "choose Replay or Leaderboard",
    "activate Continue or Replay", "change song", "change difficulty",
    "start game", "return to mode selection", "select mode", "choose mode",
    "choose menu item", "choose previous group", "choose next group",
    "choose previous date", "choose next date", "change slider",
    "change toggle", "activate button", "choose option", "toggle speech",
    "Enter or Space", "Backspace", "Left Arrow", "Right Arrow",
    "O/P/K/L", "confirm button", "back button", "top face button",
    "right stick", "left shoulder button", "right shoulder button",
    "D-Pad Left", "D-Pad Right", "East face button",
    "Space key", "Enter key", "Select button", "Start button",
    "Menu button", "Control key", "Ctrl key",
    "Left", "Right", "Up", "Down", "Shift", "Control", "Alt",
    "Tab", "Arrow", "Stick", "Press", "Bumper", "Trigger",
    "Button", "D-Pad", "Left Shift", "Right Shift", "Left Control",
    "Right Control", "Left Alt", "Right Alt", "Left Stick Press",
    "Right Stick Press", "Left Bumper", "Right Bumper",
    "Left Trigger", "Right Trigger", "West face button",
    "North face button", "South face button", "Select button",
    "Start button", "Home button", "1 life", "{0} lives",
    "choose a welcome item", "open the selected option",
    "your score, ", "most recent score, ", "current score, ",
    ", score ", "Rank {0}, {1}{2}, score {3}",
    "Rank {0}, {1}, score {2}",
}

# Human-checked core accessibility phrases. The table deliberately keeps
# placeholders and keyboard tokens unchanged. Other keys use installed Argos.
MANUAL = {
    "fr": {
        "Control": "Commande", "No scores available.": "Aucun score disponible.",
        "Score: {0}.": "Score : {0}.", "Speech off. ": "Voix désactivée. ",
        "Speech is off. ": "La voix est désactivée. ", "Speech on.": "Voix activée.",
        "Bop It Access speech is off. ": "La voix de Bop It Access est désactivée. ",
        "Bop It Access speech is ready. The game is still loading. Wait for the title screen or main menu announcement before using the controls.": "La voix de Bop It Access est prête. Le jeu est encore en cours de chargement. Attendez l'annonce de l'écran titre ou du menu principal avant d'utiliser les commandes.",
        "{0} on keyboard or {1} on controller, turn speech back on.": "{0} au clavier ou {1} à la manette pour réactiver la voix.",
        "Are you sure? Press again to confirm.": "Êtes-vous sûr ? Appuyez à nouveau pour confirmer.",
        "The user's guide is unavailable because the game menu is not ready.": "Le guide d'utilisation est indisponible, car le menu du jeu n'est pas encore prêt.",
        "Green wins": "Le vert gagne", "Yellow wins": "Le jaune gagne",
    },
    "it": {
        "Control": "Comando", "No scores available.": "Nessun punteggio disponibile.",
        "Score: {0}.": "Punteggio: {0}.", "Speech off. ": "Voce disattivata. ",
        "Speech is off. ": "La voce è disattivata. ", "Speech on.": "Voce attivata.",
        "Bop It Access speech is off. ": "La voce di Bop It Access è disattivata. ",
        "Bop It Access speech is ready. The game is still loading. Wait for the title screen or main menu announcement before using the controls.": "La voce di Bop It Access è pronta. Il gioco si sta ancora caricando. Attendi l'annuncio della schermata del titolo o del menu principale prima di usare i comandi.",
        "{0} on keyboard or {1} on controller, turn speech back on.": "{0} sulla tastiera o {1} sul controller per riattivare la voce.",
        "Are you sure? Press again to confirm.": "Sei sicuro? Premi di nuovo per confermare.",
        "The user's guide is unavailable because the game menu is not ready.": "La guida utente non è disponibile perché il menu del gioco non è ancora pronto.",
        "Green wins": "Vince il verde", "Yellow wins": "Vince il giallo",
    },
    "de": {
        "Control": "Steuerung", "No scores available.": "Keine Punktzahlen verfügbar.",
        "Score: {0}.": "Punktzahl: {0}.", "Speech off. ": "Sprachausgabe aus. ",
        "Speech is off. ": "Die Sprachausgabe ist aus. ", "Speech on.": "Sprachausgabe an.",
        "Bop It Access speech is off. ": "Die Sprachausgabe von Bop It Access ist aus. ",
        "Bop It Access speech is ready. The game is still loading. Wait for the title screen or main menu announcement before using the controls.": "Die Sprachausgabe von Bop It Access ist bereit. Das Spiel wird noch geladen. Warte auf die Ansage des Titelbildschirms oder Hauptmenüs, bevor du die Steuerung benutzt.",
        "{0} on keyboard or {1} on controller, turn speech back on.": "{0} auf der Tastatur oder {1} am Controller drücken, um die Sprachausgabe wieder einzuschalten.",
        "Are you sure? Press again to confirm.": "Bist du sicher? Drücke erneut zum Bestätigen.",
        "The user's guide is unavailable because the game menu is not ready.": "Das Benutzerhandbuch ist nicht verfügbar, weil das Spielmenü noch nicht bereit ist.",
        "Green wins": "Grün gewinnt", "Yellow wins": "Gelb gewinnt",
    },
    "es": {
        "Control": "Control", "No scores available.": "No hay puntuaciones disponibles.",
        "Score: {0}.": "Puntuación: {0}.", "Speech off. ": "Voz desactivada. ",
        "Speech is off. ": "La voz está desactivada. ", "Speech on.": "Voz activada.",
        "Bop It Access speech is off. ": "La voz de Bop It Access está desactivada. ",
        "Bop It Access speech is ready. The game is still loading. Wait for the title screen or main menu announcement before using the controls.": "La voz de Bop It Access está lista. El juego todavía se está cargando. Espera a que se anuncie la pantalla de título o el menú principal antes de usar los controles.",
        "{0} on keyboard or {1} on controller, turn speech back on.": "{0} en el teclado o {1} en el mando para volver a activar la voz.",
        "Are you sure? Press again to confirm.": "¿Seguro? Pulsa otra vez para confirmar.",
        "The user's guide is unavailable because the game menu is not ready.": "La guía de usuario no está disponible porque el menú del juego aún no está listo.",
        "Green wins": "Gana el verde", "Yellow wins": "Gana el amarillo",
    },
    "es-MX": {
        "Control": "Control", "No scores available.": "No hay puntuaciones disponibles.",
        "Score: {0}.": "Puntuación: {0}.", "Speech off. ": "Voz desactivada. ",
        "Speech is off. ": "La voz está desactivada. ", "Speech on.": "Voz activada.",
        "Bop It Access speech is off. ": "La voz de Bop It Access está desactivada. ",
        "Bop It Access speech is ready. The game is still loading. Wait for the title screen or main menu announcement before using the controls.": "La voz de Bop It Access está lista. El juego todavía se está cargando. Espera a escuchar la pantalla de título o el menú principal antes de usar los controles.",
        "{0} on keyboard or {1} on controller, turn speech back on.": "{0} en el teclado o {1} en el control para volver a activar la voz.",
        "Are you sure? Press again to confirm.": "¿Estás seguro? Presiona otra vez para confirmar.",
        "The user's guide is unavailable because the game menu is not ready.": "La guía de usuario no está disponible porque el menú del juego aún no está listo.",
        "Green wins": "Gana el verde", "Yellow wins": "Gana el amarillo",
    },
    "ja": {
        "Control": "操作", "No scores available.": "スコアはありません。",
        "1 life": "1ライフ", "{0} lives": "{0}ライフ",
        "Score: {0}.": "スコア：{0}。", "Speech off. ": "音声をオフにしました。",
        "Speech is off. ": "音声はオフです。", "Speech on.": "音声をオンにしました。",
        "Bop It Access speech is off. ": "Bop It Access の音声はオフです。",
        "Bop It Access speech is ready. The game is still loading. Wait for the title screen or main menu announcement before using the controls.": "Bop It Access の音声が準備できました。ゲームはまだ読み込み中です。タイトル画面またはメインメニューの案内が聞こえるまで、操作をお待ちください。",
        "{0} on keyboard or {1} on controller, turn speech back on.": "音声を再びオンにするには、キーボードの{0}、またはコントローラーの{1}を押してください。",
        "Are you sure? Press again to confirm.": "よろしいですか？もう一度押すと確定します。",
        "The user's guide is unavailable because the game menu is not ready.": "ゲームのメニューがまだ準備できていないため、ユーザーガイドを開けません。",
        "Green wins": "緑の勝ち", "Yellow wins": "黄色の勝ち",
    },
    "ko": {
        "Control": "조작", "No scores available.": "점수 기록이 없습니다.",
        "1 life": "목숨 1개", "{0} lives": "목숨 {0}개",
        "Score: {0}.": "점수: {0}.", "Speech off. ": "음성 출력 꺼짐. ",
        "Speech is off. ": "음성 출력이 꺼져 있습니다. ", "Speech on.": "음성 출력 켜짐.",
        "Bop It Access speech is off. ": "Bop It Access 음성 출력이 꺼져 있습니다. ",
        "Bop It Access speech is ready. The game is still loading. Wait for the title screen or main menu announcement before using the controls.": "Bop It Access 음성 출력이 준비되었습니다. 게임은 아직 로딩 중입니다. 조작하기 전에 타이틀 화면이나 메인 메뉴 안내를 기다려 주세요.",
        "{0} on keyboard or {1} on controller, turn speech back on.": "음성 출력을 다시 켜려면 키보드의 {0} 또는 컨트롤러의 {1}을 누르세요.",
        "Are you sure? Press again to confirm.": "확실합니까? 다시 누르면 확인합니다.",
        "The user's guide is unavailable because the game menu is not ready.": "게임 메뉴가 아직 준비되지 않아 사용자 가이드를 열 수 없습니다.",
        "Green wins": "초록색 승리", "Yellow wins": "노란색 승리",
    },
    "zh": {
        "Control": "操作", "No scores available.": "暂无得分记录。",
        "1 life": "1条命", "{0} lives": "{0}条命",
        "Score: {0}.": "得分：{0}。", "Speech off. ": "语音已关闭。",
        "Speech is off. ": "语音已关闭。", "Speech on.": "语音已开启。",
        "Bop It Access speech is off. ": "Bop It Access 的语音已关闭。",
        "Bop It Access speech is ready. The game is still loading. Wait for the title screen or main menu announcement before using the controls.": "Bop It Access 的语音已准备就绪。游戏仍在加载。请等待标题画面或主菜单的语音提示，再使用控制键。",
        "{0} on keyboard or {1} on controller, turn speech back on.": "按键盘上的 {0} 或手柄上的 {1}，即可重新开启语音。",
        "Are you sure? Press again to confirm.": "确定吗？再按一次以确认。",
        "The user's guide is unavailable because the game menu is not ready.": "游戏菜单尚未准备好，暂时无法打开用户指南。",
        "Green wins": "绿色获胜", "Yellow wins": "黄色获胜",
    },
    "pt-BR": {
        "Control": "Controle", "No scores available.": "Nenhuma pontuação disponível.",
        "Score: {0}.": "Pontuação: {0}.", "Speech off. ": "Voz desligada. ",
        "Speech is off. ": "A voz está desligada. ", "Speech on.": "Voz ligada.",
        "Bop It Access speech is off. ": "A voz do Bop It Access está desligada. ",
        "Bop It Access speech is ready. The game is still loading. Wait for the title screen or main menu announcement before using the controls.": "A voz do Bop It Access está pronta. O jogo ainda está carregando. Aguarde o anúncio da tela de título ou do menu principal antes de usar os controles.",
        "{0} on keyboard or {1} on controller, turn speech back on.": "Pressione {0} no teclado ou {1} no controle para ligar a voz novamente.",
        "Are you sure? Press again to confirm.": "Tem certeza? Pressione novamente para confirmar.",
        "The user's guide is unavailable because the game menu is not ready.": "O guia do usuário está indisponível porque o menu do jogo ainda não está pronto.",
        "Green wins": "Verde vence", "Yellow wins": "Amarelo vence",
    },
}

SCORE_TERMS = {
    "fr": ("Rang", ", score "),
    "it": ("Posizione", ", punteggio "),
    "de": ("Rang", ", Punktzahl "),
    "es": ("Puesto", ", puntuación "),
    "es-MX": ("Lugar", ", puntuación "),
    "ja": ("順位", "、スコア "),
    "ko": ("순위", ", 점수 "),
    "zh": ("排名", "，得分 "),
    "pt-BR": ("Posição", ", pontuação "),
}

# Terms where short, context-free model output is commonly misleading.
CORE_UI = {
    "fr": {
        "Binding changed": "Commande réassignée", "Binding unchanged": "Commande inchangée",
        "Binding unavailable": "Commande indisponible",
        "Binding unavailable for this device": "Commande indisponible sur cet appareil",
        "Bindings reset to default": "Commandes rétablies par défaut",
        "That input is already assigned to {0}": "Cette entrée est déjà attribuée à {0}",
        "That input is already assigned to {0}. Binding unchanged": "Cette entrée est déjà attribuée à {0}. Commande inchangée",
        "AUTO-SPEAK BUTTON HINTS": "Lire automatiquement les aides de touches",
        "BUTTON HINTS DELAY": "Délai des aides de touches",
        "REPEAT BUTTON HINTS": "Répéter les aides de touches",
        "REPEAT INTERVAL": "Intervalle de répétition",
        "HINTS TYPE": "Type d'aide", "SPEAK HINTS": "Lire les aides",
        "Bullet": "Élément de liste",
    },
    "it": {
        "Binding changed": "Comando riassegnato", "Binding unchanged": "Comando invariato",
        "Binding unavailable": "Comando non disponibile",
        "Binding unavailable for this device": "Comando non disponibile per questo dispositivo",
        "Bindings reset to default": "Comandi ripristinati ai valori predefiniti",
        "That input is already assigned to {0}": "Questo input è già assegnato a {0}",
        "That input is already assigned to {0}. Binding unchanged": "Questo input è già assegnato a {0}. Comando invariato",
        "AUTO-SPEAK BUTTON HINTS": "Leggi automaticamente i suggerimenti sui comandi",
        "BUTTON HINTS DELAY": "Ritardo dei suggerimenti sui comandi",
        "REPEAT BUTTON HINTS": "Ripeti i suggerimenti sui comandi",
        "REPEAT INTERVAL": "Intervallo di ripetizione",
        "HINTS TYPE": "Tipo di suggerimenti", "SPEAK HINTS": "Leggi suggerimenti",
        "Bullet": "Elemento dell'elenco",
    },
    "de": {
        "Binding changed": "Tastenbelegung geändert", "Binding unchanged": "Tastenbelegung unverändert",
        "Binding unavailable": "Belegung nicht verfügbar",
        "Binding unavailable for this device": "Belegung für dieses Gerät nicht verfügbar",
        "Bindings reset to default": "Tastenbelegung auf Standard zurückgesetzt",
        "That input is already assigned to {0}": "Diese Eingabe ist bereits {0} zugewiesen",
        "That input is already assigned to {0}. Binding unchanged": "Diese Eingabe ist bereits {0} zugewiesen. Tastenbelegung unverändert",
        "AUTO-SPEAK BUTTON HINTS": "Tastenhinweise automatisch vorlesen",
        "BUTTON HINTS DELAY": "Verzögerung der Tastenhinweise",
        "REPEAT BUTTON HINTS": "Tastenhinweise wiederholen",
        "REPEAT INTERVAL": "Wiederholungsintervall",
        "HINTS TYPE": "Hinweistyp", "SPEAK HINTS": "Hinweise vorlesen",
        "Bullet": "Listeneintrag",
    },
    "es": {
        "Binding changed": "Control reasignado", "Binding unchanged": "Control sin cambios",
        "Binding unavailable": "Control no disponible",
        "Binding unavailable for this device": "Control no disponible para este dispositivo",
        "Bindings reset to default": "Controles restablecidos a los valores predeterminados",
        "That input is already assigned to {0}": "Esta entrada ya está asignada a {0}",
        "That input is already assigned to {0}. Binding unchanged": "Esta entrada ya está asignada a {0}. El control no ha cambiado",
        "AUTO-SPEAK BUTTON HINTS": "Leer automáticamente las ayudas de botones",
        "BUTTON HINTS DELAY": "Retardo de las ayudas de botones",
        "REPEAT BUTTON HINTS": "Repetir las ayudas de botones",
        "REPEAT INTERVAL": "Intervalo de repetición",
        "HINTS TYPE": "Tipo de ayuda", "SPEAK HINTS": "Leer ayudas",
        "Bullet": "Elemento de lista",
    },
    "es-MX": {
        "Binding changed": "Control reasignado", "Binding unchanged": "Control sin cambios",
        "Binding unavailable": "Control no disponible",
        "Binding unavailable for this device": "Control no disponible para este dispositivo",
        "Bindings reset to default": "Controles restablecidos a los valores predeterminados",
        "That input is already assigned to {0}": "Esta entrada ya está asignada a {0}",
        "That input is already assigned to {0}. Binding unchanged": "Esta entrada ya está asignada a {0}. El control no cambió",
        "AUTO-SPEAK BUTTON HINTS": "Leer automáticamente las indicaciones de botones",
        "BUTTON HINTS DELAY": "Retraso de las indicaciones de botones",
        "REPEAT BUTTON HINTS": "Repetir las indicaciones de botones",
        "REPEAT INTERVAL": "Intervalo de repetición",
        "HINTS TYPE": "Tipo de indicaciones", "SPEAK HINTS": "Leer indicaciones",
        "Bullet": "Elemento de lista",
    },
    "ja": {
        "Binding changed": "割り当てを変更しました", "Binding unchanged": "割り当ては変更されていません",
        "Binding unavailable": "この割り当ては使用できません",
        "Binding unavailable for this device": "この機器では割り当てできません",
        "Bindings reset to default": "操作の割り当てを初期設定に戻しました",
        "That input is already assigned to {0}": "その入力はすでに{0}に割り当てられています",
        "That input is already assigned to {0}. Binding unchanged": "その入力はすでに{0}に割り当てられています。割り当ては変更されていません",
        "AUTO-SPEAK BUTTON HINTS": "ボタン操作のヒントを自動で読み上げる",
        "BUTTON HINTS DELAY": "ボタン操作のヒントの遅延",
        "REPEAT BUTTON HINTS": "ボタン操作のヒントを繰り返す",
        "REPEAT INTERVAL": "繰り返し間隔",
        "HINTS TYPE": "ヒントの種類", "SPEAK HINTS": "ヒントを読み上げる",
        "Bullet": "リスト項目",
    },
    "ko": {
        "Binding changed": "입력 할당이 변경되었습니다", "Binding unchanged": "입력 할당이 변경되지 않았습니다",
        "Binding unavailable": "이 입력은 사용할 수 없습니다",
        "Binding unavailable for this device": "이 장치에서는 입력을 사용할 수 없습니다",
        "Bindings reset to default": "입력 할당을 기본값으로 되돌렸습니다",
        "That input is already assigned to {0}": "이 입력은 이미 {0}에 할당되어 있습니다",
        "That input is already assigned to {0}. Binding unchanged": "이 입력은 이미 {0}에 할당되어 있습니다. 할당이 변경되지 않았습니다",
        "AUTO-SPEAK BUTTON HINTS": "버튼 힌트 자동 읽기",
        "BUTTON HINTS DELAY": "버튼 힌트 지연 시간",
        "REPEAT BUTTON HINTS": "버튼 힌트 반복",
        "REPEAT INTERVAL": "반복 간격",
        "HINTS TYPE": "힌트 유형", "SPEAK HINTS": "힌트 읽기",
        "Bullet": "목록 항목",
    },
    "zh": {
        "Binding changed": "按键绑定已更改", "Binding unchanged": "按键绑定未更改",
        "Binding unavailable": "此按键无法使用",
        "Binding unavailable for this device": "此设备无法使用该按键",
        "Bindings reset to default": "按键绑定已恢复默认值",
        "That input is already assigned to {0}": "该输入已分配给{0}",
        "That input is already assigned to {0}. Binding unchanged": "该输入已分配给{0}。按键绑定未更改",
        "AUTO-SPEAK BUTTON HINTS": "自动朗读按键提示",
        "BUTTON HINTS DELAY": "按键提示延迟",
        "REPEAT BUTTON HINTS": "重复按键提示",
        "REPEAT INTERVAL": "重复间隔",
        "HINTS TYPE": "提示类型", "SPEAK HINTS": "朗读提示",
        "Bullet": "列表项",
    },
    "pt-BR": {
        "Binding changed": "Comando remapeado", "Binding unchanged": "Comando não alterado",
        "Binding unavailable": "Comando indisponível",
        "Binding unavailable for this device": "Comando indisponível para este dispositivo",
        "Bindings reset to default": "Comandos restaurados para o padrão",
        "That input is already assigned to {0}": "Essa entrada já está atribuída a {0}",
        "That input is already assigned to {0}. Binding unchanged": "Essa entrada já está atribuída a {0}. Comando não alterado",
        "AUTO-SPEAK BUTTON HINTS": "Ler dicas de botões automaticamente",
        "BUTTON HINTS DELAY": "Atraso das dicas de botões",
        "REPEAT BUTTON HINTS": "Repetir dicas de botões",
        "REPEAT INTERVAL": "Intervalo de repetição",
        "HINTS TYPE": "Tipo de dica", "SPEAK HINTS": "Ler dicas",
        "Bullet": "Item de lista",
    },
}
for _locale, _terms in CORE_UI.items():
    MANUAL[_locale].update(_terms)

CONTROL_TYPE_LABELS = {
    "fr": "ANNONCER LES TYPES DE COMMANDES",
    "it": "ANNUNCIA TIPI DI CONTROLLO",
    "de": "STEUERELEMENTTYPEN ANSAGEN",
    "es": "ANUNCIAR TIPOS DE CONTROL",
    "es-MX": "ANUNCIAR TIPOS DE CONTROL",
    "ja": "操作部品の種類を読み上げる",
    "ko": "컨트롤 유형 말하기",
    "zh": "播报控件类型",
    "pt-BR": "ANUNCIAR TIPOS DE CONTROLE",
}
for _locale, _label in CONTROL_TYPE_LABELS.items():
    MANUAL[_locale]["SPEAK CONTROL TYPES"] = _label

INPUT_NAMES = {
    "fr": ("Barre d'espace", "Entrée", "Bouton Select", "Bouton Start", "Bouton Menu", "Touche Ctrl"),
    "it": ("Barra spaziatrice", "Invio", "Pulsante Select", "Pulsante Start", "Pulsante Menu", "Tasto Ctrl"),
    "de": ("Leertaste", "Eingabetaste", "Select-Taste", "Start-Taste", "Menütaste", "Strg-Taste"),
    "es": ("Barra espaciadora", "Intro", "Botón Select", "Botón Start", "Botón Menú", "Tecla Ctrl"),
    "es-MX": ("Barra espaciadora", "Enter", "Botón Select", "Botón Start", "Botón Menú", "Tecla Ctrl"),
    "ja": ("スペースキー", "エンターキー", "セレクトボタン", "スタートボタン", "メニューボタン", "コントロールキー"),
    "ko": ("스페이스 키", "Enter 키", "Select 버튼", "Start 버튼", "메뉴 버튼", "Ctrl 키"),
    "zh": ("空格键", "回车键", "Select 按钮", "Start 按钮", "菜单按钮", "Ctrl 键"),
    "pt-BR": ("Barra de espaço", "Enter", "Botão Select", "Botão Start", "Botão Menu", "Tecla Ctrl"),
}
for _locale, _values in INPUT_NAMES.items():
    MANUAL[_locale].update(dict(zip(("Space key", "Enter key",
        "Select button", "Start button", "Menu button", "Control key"),
        _values)))
    MANUAL[_locale]["Ctrl key"] = _values[-1]

SAPI_LABELS = {
    "fr": ("Voix SAPI", "Volume SAPI", "Vitesse SAPI", "Hauteur de voix SAPI"),
    "it": ("Voce SAPI", "Volume SAPI", "Velocità SAPI", "Tono SAPI"),
    "de": ("SAPI Stimme", "SAPI Lautstärke", "SAPI Sprechgeschwindigkeit", "SAPI Tonhöhe"),
    "es": ("Voz SAPI", "Volumen SAPI", "Velocidad SAPI", "Tono SAPI"),
    "es-MX": ("Voz SAPI", "Volumen SAPI", "Velocidad SAPI", "Tono SAPI"),
    "ja": ("SAPI 音声", "SAPI 音量", "SAPI 読み上げ速度", "SAPI 音程"),
    "ko": ("SAPI 음성", "SAPI 볼륨", "SAPI 말하기 속도", "SAPI 음높이"),
    "zh": ("SAPI 语音", "SAPI 音量", "SAPI 语速", "SAPI 音高"),
    "pt-BR": ("Voz SAPI", "Volume SAPI", "Velocidade SAPI", "Tom SAPI"),
}
STICK_PRESS = {
    "fr": ("Appui sur le stick gauche", "Appui sur le stick droit"),
    "it": ("Pressione della levetta sinistra", "Pressione della levetta destra"),
    "de": ("Linken Stick drücken", "Rechten Stick drücken"),
    "es": ("Pulsación del stick izquierdo", "Pulsación del stick derecho"),
    "es-MX": ("Presionar la palanca izquierda", "Presionar la palanca derecha"),
    "ja": ("左スティック押し込み", "右スティック押し込み"),
    "ko": ("왼쪽 스틱 누르기", "오른쪽 스틱 누르기"),
    "zh": ("按下左摇杆", "按下右摇杆"),
    "pt-BR": ("Pressionar o analógico esquerdo", "Pressionar o analógico direito"),
}
for _locale, _values in SAPI_LABELS.items():
    MANUAL[_locale].update(dict(zip(("SAPI VOICE", "SAPI VOLUME",
        "SAPI RATE", "SAPI PITCH"), _values)))
    MANUAL[_locale].update(dict(zip(("Left Stick Press",
        "Right Stick Press"), STICK_PRESS[_locale])))

DESCRIPTION_PREFIXES = {
    "Shapes": "An abstract digital scene with no recognizable room or ground",
    "Space": "A bright, playful galaxy surrounds the Bop It device",
    "City": "A layered cartoon city glows at twilight",
    "Office": "A whimsical workplace scene centers on a desk and computer",
}

# The four descriptions are a core accessibility feature. Offline machine
# output was checked and replaced where it garbled visual details or mixed
# untranslated words into CJK prose.
STAGE_PROSE = {
    "fr": {
        "Shapes": "Une scène numérique abstraite, sans pièce ni sol reconnaissable, évoque une pizzeria des années 1990 transformée en soirée DJ au néon. Le rose vif, le violet et le bleu sarcelle remplissent l'espace. Des carrés, des formes anguleuses, des cubes arrondis, des sphères et des pyramides se mêlent à des lignes ondulées flottantes et à d'autres motifs géométriques. Les formes dérivent et rebondissent comme dans un ancien économiseur d'écran, donnant au décor un rythme vivant et ludique.",
        "Space": "Une galaxie lumineuse et ludique entoure l'appareil Bop It. Le bleu profond, le violet et le noir évoquent l'espace, tandis que des étoiles, des nuages cosmiques, des planètes et des rochers flottants apportent de la profondeur. De sympathiques extraterrestres animés dérivent à proximité et encouragent le joueur. Leurs touches de vert lumineux, d'argent et de bleu néon ressortent sur le fond sombre et créent une atmosphère de science-fiction joyeuse.",
        "City": "Une ville de dessin animé se déploie en plusieurs plans à la tombée du jour, avec des immeubles bleu foncé et des lumières chaudes orange et jaunes. Gratte-ciels, fenêtres éclairées, lampadaires, panneaux publicitaires et enseignes fantaisistes composent un centre-ville animé. Certaines enseignes affichent des noms de rues inspirés de la musique, comme « Bop It Blvd. » et « Spin It Street ». Le chat d'un grand panneau publicitaire bondit et roule tandis que la ville s'anime autour de lui.",
        "Office": "Une scène de bureau fantaisiste s'organise autour d'un bureau et d'un ordinateur. L'écran devient un aquarium : des poissons y nagent au milieu d'images aquatiques colorées. Un clavier, des plantes feuillues et de petites fleurs complètent l'espace de travail.",
    },
    "it": {
        "Shapes": "Una scena digitale astratta, senza una stanza o un pavimento riconoscibile, ricorda una pizzeria degli anni Novanta trasformata in una serata DJ illuminata al neon. Rosa acceso, viola e verde acqua riempiono lo spazio. Quadrati, forme spigolose, cubi arrotondati, sfere e piramidi si mescolano a linee ondulate sospese e ad altri motivi geometrici. Le forme fluttuano e rimbalzano come in un vecchio salvaschermo, dando alla scena un ritmo allegro e vivace.",
        "Space": "Una galassia luminosa e giocosa circonda il dispositivo Bop It. Blu scuro, viola e nero suggeriscono lo spazio aperto; stelle, nubi cosmiche, pianeti e rocce sospese aggiungono profondità. Simpatici alieni animati fluttuano lì vicino e incoraggiano il giocatore. I loro accenti luminosi di verde, argento e blu neon risaltano sullo sfondo scuro, creando un'atmosfera fantascientifica allegra.",
        "City": "Una città da cartone animato, disposta su più livelli, risplende al crepuscolo. Gli edifici blu scuro sono illuminati da calde luci arancioni e gialle. Grattacieli, finestre illuminate, lampioni, cartelloni e insegne giocose formano un centro vivace. Alcune insegne riportano nomi di strade a tema musicale, come «Bop It Blvd.» e «Spin It Street». Il gatto sul cartellone principale salta e rotola mentre la città si muove attorno a lui.",
        "Office": "Una scena d'ufficio stravagante ruota attorno a una scrivania e a un computer. Il monitor diventa un acquario: i pesci nuotano al suo interno, circondati da immagini acquatiche colorate. Una tastiera, piante frondose e piccoli fiori arricchiscono la postazione di lavoro.",
    },
    "de": {
        "Shapes": "Eine abstrakte digitale Szene ohne erkennbaren Raum oder Boden erinnert an eine Pizzeria aus den 1990er Jahren, die zur Neon-DJ-Party geworden ist. Leuchtendes Rosa, Violett und Türkis füllen den Raum. Quadrate, kantige Formen, abgerundete Würfel, Kugeln und Pyramiden mischen sich mit schwebenden Wellenlinien und anderen geometrischen Mustern. Die Formen treiben und hüpfen wie in einem alten Bildschirmschoner und geben der Szene einen lebhaften Rhythmus.",
        "Space": "Eine helle, verspielte Galaxie umgibt das Bop It Gerät. Dunkelblau, Violett und Schwarz lassen den Weltraum weit erscheinen. Sterne, kosmische Wolken, Planeten und schwebende Felsen schaffen Tiefe. Freundliche animierte Außerirdische treiben in der Nähe und feuern die spielende Person an. Ihre leuchtend grünen, silbernen und neonblauen Akzente heben sich vom dunklen Hintergrund ab und sorgen für eine fröhliche Science-Fiction-Stimmung.",
        "City": "Eine mehrschichtige Stadt im Zeichentrickstil leuchtet in der Dämmerung. Dunkelblaue Gebäude tragen warme orangefarbene und gelbe Lichter. Hochhäuser, beleuchtete Fenster, Straßenlaternen, Werbetafeln und verspielte Schilder bilden eine belebte Innenstadt. Einige Schilder zeigen musikalische Straßennamen wie „Bop It Blvd.“ und „Spin It Street“. Die Katze auf der auffälligen Werbetafel hüpft und rollt, während die Stadt um sie herum in Bewegung ist.",
        "Office": "Eine verspielte Büroszene dreht sich um einen Schreibtisch und einen Computer. Der Bildschirm wird zum Aquarium: Fische schwimmen darin, umgeben von farbenfrohen Unterwasserbildern. Eine Tastatur, Blattpflanzen und kleine Blumen schmücken den Arbeitsplatz.",
    },
    "es": {
        "Shapes": "Una escena digital abstracta, sin una habitación ni un suelo reconocibles, recuerda a una pizzería de los años noventa convertida en una noche de DJ con luces de neón. El rosa intenso, el morado y el verde azulado llenan el espacio. Cuadrados, formas angulosas, cubos redondeados, esferas y pirámides se mezclan con líneas onduladas flotantes y otros motivos geométricos. Las formas flotan y rebotan como en un antiguo salvapantallas, dando a la escena un ritmo alegre y animado.",
        "Space": "Una galaxia luminosa y divertida rodea el dispositivo Bop It. El azul oscuro, el morado y el negro sugieren el espacio abierto, mientras que las estrellas, las nubes cósmicas, los planetas y las rocas flotantes aportan profundidad. Unos simpáticos alienígenas animados flotan cerca y animan al jugador. Sus toques luminosos de verde, plata y azul neón destacan sobre el fondo oscuro y crean una alegre atmósfera de ciencia ficción.",
        "City": "Una ciudad de dibujos animados, organizada en varias capas, brilla al anochecer. Sus edificios azul oscuro muestran cálidas luces naranjas y amarillas. Rascacielos, ventanas iluminadas, farolas, vallas publicitarias y letreros juguetones forman un centro bullicioso. Algunos carteles muestran nombres de calles con temática musical, como «Bop It Blvd.» y «Spin It Street». El gato del cartel principal salta y rueda mientras la ciudad se mueve a su alrededor.",
        "Office": "Una peculiar escena de oficina gira en torno a un escritorio y un ordenador. El monitor se convierte en un acuario: los peces nadan en su interior, rodeados de imágenes acuáticas de colores. Un teclado, plantas frondosas y pequeñas flores completan el puesto de trabajo.",
    },
    "es-MX": {
        "Shapes": "Una escena digital abstracta, sin una habitación ni un piso reconocibles, recuerda a una pizzería de los años noventa convertida en una noche de DJ con luces de neón. El rosa intenso, el morado y el verde azulado llenan el espacio. Cuadrados, formas angulosas, cubos redondeados, esferas y pirámides se mezclan con líneas onduladas flotantes y otros patrones geométricos. Las formas flotan y rebotan como en un viejo protector de pantalla, dando a la escena un ritmo alegre y animado.",
        "Space": "Una galaxia luminosa y divertida rodea el dispositivo Bop It. El azul oscuro, el morado y el negro sugieren el espacio abierto, mientras que las estrellas, las nubes cósmicas, los planetas y las rocas flotantes aportan profundidad. Unos simpáticos extraterrestres animados flotan cerca y animan al jugador. Sus luces verdes, plateadas y azul neón destacan sobre el fondo oscuro y crean una alegre atmósfera de ciencia ficción.",
        "City": "Una ciudad de caricatura, formada por varias capas, brilla al atardecer. Sus edificios azul oscuro muestran cálidas luces naranjas y amarillas. Rascacielos, ventanas iluminadas, farolas, espectaculares y letreros divertidos forman un centro muy activo. Algunos letreros muestran nombres de calles con tema musical, como “Bop It Blvd.” y “Spin It Street”. El gato del espectacular principal salta y rueda mientras la ciudad se mueve a su alrededor.",
        "Office": "Una peculiar escena de oficina gira alrededor de un escritorio y una computadora. El monitor se convierte en una pecera: los peces nadan adentro, rodeados de coloridas imágenes acuáticas. Un teclado, plantas frondosas y pequeñas flores completan el área de trabajo.",
    },
    "pt-BR": {
        "Shapes": "Uma cena digital abstrata, sem sala ou chão reconhecíveis, lembra uma pizzaria dos anos 1990 transformada em uma noite de DJ iluminada por néon. Rosa vibrante, roxo e verde-azulado preenchem o espaço. Quadrados, formas angulares, cubos arredondados, esferas e pirâmides se misturam a linhas onduladas flutuantes e outros padrões geométricos. As formas flutuam e saltam como em um antigo protetor de tela, dando à fase um ritmo alegre e animado.",
        "Space": "Uma galáxia brilhante e divertida envolve o aparelho Bop It. Azul escuro, roxo e preto sugerem o espaço aberto, enquanto estrelas, nuvens cósmicas, planetas e rochas flutuantes criam profundidade. Alienígenas animados e simpáticos flutuam por perto e torcem pelo jogador. Seus detalhes luminosos em verde, prata e azul néon se destacam contra o fundo escuro e criam uma atmosfera alegre de ficção científica.",
        "City": "Uma cidade de desenho animado, formada por várias camadas, brilha ao anoitecer. Prédios azul-escuros exibem luzes quentes em laranja e amarelo. Arranha-céus, janelas acesas, postes, outdoors e placas divertidas compõem um centro movimentado. Algumas placas trazem nomes de ruas inspirados em música, como “Bop It Blvd.” e “Spin It Street”. O gato no outdoor principal pula e rola enquanto a cidade se move ao seu redor.",
        "Office": "Uma cena de escritório bem-humorada se concentra em uma mesa e um computador. O monitor vira um aquário: peixes nadam dentro dele, cercados por imagens aquáticas coloridas. Um teclado, plantas com muitas folhas e pequenas flores completam o espaço de trabalho.",
    },
    "ja": {
        "Shapes": "部屋や地面の区別がない、抽象的なデジタル空間です。1990年代のピザ店がネオンのDJイベントになったような雰囲気です。鮮やかなピンク、紫、青緑が広がります。四角形、角ばった形、丸みのある立方体、球体、ピラミッドに、浮かぶ波線などの幾何学模様が混ざります。形は昔のスクリーンセーバーのように漂い、弾むように動きます。",
        "Space": "明るく遊び心のある銀河が、Bop Itの装置を取り囲みます。濃い青、紫、黒の宇宙に、星、星雲、惑星、浮かぶ岩が奥行きを添えます。親しみやすいアニメーションの宇宙人が近くを漂い、プレイヤーを応援します。光る緑、銀色、ネオンブルーのアクセントが暗い背景に映え、楽しいSFの雰囲気を作ります。",
        "City": "夕暮れの漫画風の街が、何層にも重なって輝いています。紺色の建物には暖かいオレンジ色や黄色の明かりがともります。高層ビル、光る窓、街灯、看板や遊び心のある標識が、にぎやかな繁華街を作ります。「Bop It Blvd.」や「Spin It Street」のような音楽を題材にした通りの名前も見えます。目立つ看板の猫は、街の動きに合わせて体を弾ませ、転がります。",
        "Office": "机とコンピューターを中心にした、風変わりなオフィスです。モニターが水槽になり、色とりどりの水中風景の中を魚が泳ぎます。作業台の周りにはキーボード、葉の茂った植物、小さな花があります。",
    },
    "ko": {
        "Shapes": "뚜렷한 방이나 바닥이 없는 추상적인 디지털 공간입니다. 1990년대 피자 가게가 네온빛 DJ 파티로 바뀐 듯한 분위기입니다. 선명한 분홍색, 보라색, 청록색이 공간을 채웁니다. 사각형, 각진 형태, 모서리가 둥근 정육면체, 구와 피라미드 사이로 떠다니는 물결선과 기하학 무늬가 섞입니다. 모양들은 옛 화면 보호기처럼 떠다니고 통통 튀어 장면에 활기찬 리듬을 더합니다.",
        "Space": "밝고 장난기 가득한 은하가 Bop It 장치를 둘러쌉니다. 짙은 파랑, 보라, 검정으로 표현된 우주에는 별, 우주 구름, 행성, 떠다니는 바위가 깊이를 더합니다. 친근한 애니메이션 외계인들이 근처를 떠다니며 플레이어를 응원합니다. 빛나는 초록색, 은색, 네온 파랑이 어두운 배경에서 돋보여 즐거운 공상과학 분위기를 만듭니다.",
        "City": "해 질 녘, 여러 겹으로 펼쳐진 만화풍 도시가 빛납니다. 짙은 파란색 건물에 따뜻한 주황색과 노란색 불빛이 켜집니다. 고층 건물, 밝은 창문, 가로등, 광고판과 장난스러운 간판이 번화한 도심을 이룹니다. 'Bop It Blvd.'와 'Spin It Street'처럼 음악을 주제로 한 거리 이름도 보입니다. 눈에 띄는 광고판의 고양이는 도시가 움직이는 동안 몸을 튕기고 구르며 우스꽝스러운 볼거리가 됩니다.",
        "Office": "책상과 컴퓨터를 중심으로 한 기발한 사무실입니다. 모니터가 수족관으로 바뀌어 물고기들이 다채로운 수중 장면 속을 헤엄칩니다. 작업 공간 주변에는 키보드, 잎이 무성한 식물과 작은 꽃이 놓여 있습니다.",
    },
    "zh": {
        "Shapes": "这是一个抽象的数字空间，看不出具体的房间或地面，仿佛一家怀旧的二十世纪九十年代披萨店变成了霓虹灯下的DJ之夜。亮粉色、紫色和蓝绿色充满画面。方块、棱角分明的形状、圆角立方体、球体和金字塔，与漂浮的波浪线及其他几何图案交织在一起。各种形状像旧式屏幕保护程序一样漂移、弹跳，让场景充满活泼的节奏。",
        "Space": "明亮而俏皮的银河环绕着Bop It装置。深蓝、紫色和黑色构成广阔的太空，星星、宇宙云、行星和漂浮的岩石增添了层次。友善的动画外星人在附近飘动，为玩家加油。发光的绿色、银色和霓虹蓝点缀在深色背景上，营造出愉快的科幻氛围。",
        "City": "黄昏时分，一座层次丰富的卡通城市闪闪发光。深蓝色的建筑映着温暖的橙色和黄色灯光。摩天大楼、亮着的窗户、路灯、广告牌和俏皮的招牌组成热闹的市中心。有些招牌写着音乐主题的街名，例如“Bop It Blvd.”和“Spin It Street”。一只猫出现在醒目的广告牌上，随着城市的律动蹦跳、翻滚，十分滑稽。",
        "Office": "这是一个围绕书桌和电脑布置的奇趣办公室。显示器变成了水族箱，鱼儿在缤纷的水下画面中游动。工作台周围还有键盘、枝叶繁茂的植物和小花。",
    },
}


def finalize_catalog(locale: str, catalog: dict[str, str]) -> None:
    """Keep leaderboard row and its type insertion separator synchronized."""
    rank, separator = SCORE_TERMS[locale]
    catalog[", score "] = separator
    catalog["Rank {0}, {1}{2}, score {3}"] = (
        f"{rank} {{0}}, {{1}}{{2}}{separator}{{3}}")
    catalog["Rank {0}, {1}, score {2}"] = (
        f"{rank} {{0}}, {{1}}{separator}{{2}}")
    for source in tuple(catalog):
        stage = next((name for name, prefix in DESCRIPTION_PREFIXES.items()
                      if source.startswith(prefix)), None)
        if stage is not None:
            catalog[source] = STAGE_PROSE[locale][stage]
    for source, translated in MANUAL[locale].items():
        if source in catalog:
            catalog[source] = translated
    terms = native_glossary().get(locale, {})
    for action in ("BOP", "TWIST", "PULL", "SPIN", "FLICK",
                   "BOP IT", "TWIST IT", "PULL IT", "SPIN IT", "FLICK IT"):
        translated = native_value(action, terms)
        if translated:
            catalog[action] = translated
            if not action.endswith(" IT"):
                catalog[action.title()] = translated
    second_player = native_value("BOP - PLAYER 2", terms)
    if locale == "ja":
        second_player = "叩く、プレイヤー2"
    elif locale == "de":
        second_player = "Klopfen – Spieler 2"
    elif locale == "pt-BR":
        second_player = "Bater, Jogador 2"
    if second_player:
        catalog["BOP - PLAYER 2"] = second_player
        catalog["Bop, Player 2"] = second_player
    # Stage titles are proper names shown unchanged in the game's static
    # assets. Keep speech and documentation aligned with those visible names.
    for stage in ("Shapes", "Space", "City", "Office"):
        catalog[stage] = stage
        catalog[stage.upper()] = stage
    stop = "。" if locale in {"ja", "zh"} else "."
    controls = native_value("CONTROLS", terms)
    if controls:
        catalog["Controls."] = controls + stop
    credits = native_value("CREDITS", terms)
    if credits:
        catalog["Credits."] = credits + stop
        catalog["Credits. {0}."] = credits + stop + " {0}" + stop
    for source in tuple(catalog):
        if source in PUNCTUATION:
            catalog[source] = source


def csharp_literal(quoted: str) -> str:
    return json.loads(quoted)


def source_keys() -> set[str]:
    keys = set(EXTRA_KEYS)
    for source in (ROOT / "src").glob("*.cs"):
        text = source.read_text(encoding="utf-8-sig")
        for match in LITERAL_CALL.finditer(text):
            try:
                keys.add(csharp_literal(match.group(1)))
            except json.JSONDecodeError:
                pass
        if source.name == "BopItAccessMod.TrackSelect.cs":
            for description in DESCRIPTION.finditer(text):
                keys.add("".join(csharp_literal(item.group())
                    for item in STRING_LITERAL.finditer(description.group(1))))
        if source.name == "BopItAccessMod.Localization.cs":
            binding_terms = re.search(
                r"BindingDisplayTerms\s*=\s*\{(.*?)\};", text, re.DOTALL)
            if binding_terms:
                keys.update(csharp_literal(item.group()) for item in
                    STRING_LITERAL.finditer(binding_terms.group(1)))
    keys.discard("")
    return keys


def native_glossary() -> dict[str, dict[str, str]]:
    source = ROOT / "tools" / "translation-cache" / "native-game-terms.json"
    if not source.is_file():
        return {}
    return json.loads(source.read_text(encoding="utf-8"))


def case_native(term: str, source: str) -> str:
    if source.isupper() and source not in {"SFX", "SAPI", "NVDA", "JAWS"}:
        return term[0].upper() + term[1:].lower() if term else term
    if source[0].isupper() and term.isupper() and len(term) > 1:
        return term[0] + term[1:].lower()
    return term


def native_value(source: str, terms: dict[str, str]) -> str | None:
    if source in terms:
        return case_native(terms[source], source)
    value = next((term for label, term in terms.items()
                  if label.casefold() == source.casefold()), None)
    if value:
        if source.islower():
            return value.lower()
        return value[:1].upper() + value[1:].lower()
    return None


def translate_argos(source: str, translator) -> str:
    if source in PUNCTUATION:
        return source
    # Let Argos see a natural sentence first. Synthetic marker tokens were
    # once used here, but models can spell them differently and leak them into
    # spoken text. Split into literal pieces only when a token is changed.
    translated = translator.translate(source)
    if Counter(PLACEHOLDER.findall(translated)) != Counter(
            PLACEHOLDER.findall(source)):
        pieces = PLACEHOLDER.split(source)
        tokens = PLACEHOLDER.findall(source)
        translated = "".join(translator.translate(piece) + (tokens[index]
            if index < len(tokens) else "")
            for index, piece in enumerate(pieces))
    for product in dict.fromkeys(PRODUCTS.findall(source)):
        if product in translated:
            continue
        pieces = source.split(product)
        translated = product.join(translator.translate(piece)
                                  for piece in pieces)
        break
    if source.endswith(" ") and not translated.endswith(" "):
        translated += " "
    return translated.strip("\r\n") or source


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--locale", choices=LOCALES)
    parser.add_argument("--models-dir", type=Path,
                        default=ROOT / "tools" / "argos-packages")
    parser.add_argument("--finalize-only", action="store_true",
                        help="Apply curated score terms to existing JSON files")
    args = parser.parse_args()
    destination = ROOT / "src" / "locales"
    if args.finalize_only:
        for locale in ([args.locale] if args.locale else LOCALES):
            path = destination / f"{locale}.json"
            catalog = json.loads(path.read_text(encoding="utf-8"))
            finalize_catalog(locale, catalog)
            path.write_bytes((json.dumps(catalog, ensure_ascii=False,
                indent=2) + "\n").encode("utf-8"))
            print(f"{locale}: curated leaderboard strings")
        return
    os.environ["ARGOS_PACKAGES_DIR"] = str(args.models_dir.resolve())
    import argostranslate.translate  # after environment setup

    keys = source_keys()
    glossary = native_glossary()
    destination.mkdir(parents=True, exist_ok=True)
    for locale in ([args.locale] if args.locale else LOCALES):
        translation = argostranslate.translate.get_translation_from_codes(
            "en", LOCALES[locale])
        terms = glossary.get(locale, {})
        catalog = {}
        for index, source in enumerate(sorted(keys | set(terms))):
            result = MANUAL[locale].get(source)
            if result is None:
                stage = next((name for name, prefix in DESCRIPTION_PREFIXES.items()
                              if source.startswith(prefix)), None)
                if stage is not None:
                    result = STAGE_PROSE[locale][stage]
            if result is None:
                result = native_value(source, terms)
            if result is None:
                if source in {"SAPI", "OneCore", "NVDA", "JAWS",
                              "UI Automation", "ZDSR", "ZoomText",
                              "Boy PC Reader", "PC Talker", "Sense Reader",
                              "Window-Eyes", "System Access", "F8", "F9",
                              "LT", "RT"} or source in PUNCTUATION:
                    result = source
                else:
                    result = translate_argos(source, translation)
            if Counter(PLACEHOLDER.findall(result)) != Counter(
                    PLACEHOLDER.findall(source)):
                raise ValueError(f"Placeholder mismatch in {locale}: {source}")
            catalog[source] = result
            if index % 75 == 0:
                print(locale, index, "/", len(keys | set(terms)), flush=True)
        finalize_catalog(locale, catalog)
        path = destination / f"{locale}.json"
        path.write_bytes((json.dumps(catalog, ensure_ascii=False,
            indent=2) + "\n").encode("utf-8"))
        print(f"{locale}: wrote {len(catalog)} strings to {path}", flush=True)


if __name__ == "__main__":
    main()
