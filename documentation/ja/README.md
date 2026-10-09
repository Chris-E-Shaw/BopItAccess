# Bop It Access

Bop It Access は、Windows x64 向け Steam 版 **Bop It! The Video Game** を目の見えない方にも遊びやすくするアクセシビリティ MOD です。[MelonLoader](https://github.com/LavaGang/MelonLoader) と [Prism](https://github.com/ethindp/prism) を使い、ゲームのメニューや画面に音声読み上げと点字出力を追加します。より快適に遊べるよう、追加の操作や設定も用意しています。

**Windows x64 用のバージョン 1.0 を公開しました。** MOD とインストーラーのソースのバージョンは **1.0.0** です。[インストーラー](https://github.com/Chris-E-Shaw/BopItAccess/releases/download/v1.0/BopItAccess-Installer.exe)で案内に従って導入するか、[コンパイル済み ZIP](https://github.com/Chris-E-Shaw/BopItAccess/releases/download/v1.0/BopItAccess-v1.0.zip)で手動導入してください。

## 主な機能

- メニュー、設定、チュートリアル、ランキング、実績、クレジット、ポーズ画面、結果画面の読み上げ。
- 必要なときに呼び出せるスコア、ステージの情景説明、現在の操作割り当てに対応するヒント。
- ゲームが提供するすべての言語に対応した読み上げと、ゲーム内ユーザーガイド。
- スクリーンリーダーと点字への出力。Windows の音声出力として OneCore と SAPI も利用できます。
- 読み上げの詳しさ、ヒントのタイミングと繰り返しの調整、および読み上げ用ショートカットの再割り当て。
- ゲーム本来の操作に対する追加の割り当て、フレームレート制限、バックグラウンド時の音声設定、読みやすい設定ファイル。
- キーボードとコントローラーに対応し、インストール、更新、削除を行えるアクセシブルなインストーラー。

## プロジェクトの状況

主な機能はほぼ完成しています。プレイヤーからのフィードバックをもとに、不具合の修正や改善を行い、必要に応じてプロジェクトを維持していきます。現在対応している環境は Windows x64 です。

[初めての一般公開リリース v1.0](https://github.com/Chris-E-Shaw/BopItAccess/releases/tag/v1.0)には、次の 4 種類のダウンロードがあります。

| ダウンロード | 用途 |
| --- | --- |
| [BopItAccess-Installer.exe](https://github.com/Chris-E-Shaw/BopItAccess/releases/download/v1.0/BopItAccess-Installer.exe) | ゲームを検出し、必要な部品と最新の安定版 MOD を導入し、更新と削除を管理するアクセシブルなインストーラー。 |
| [BopItAccess-v1.0.zip](https://github.com/Chris-E-Shaw/BopItAccess/releases/download/v1.0/BopItAccess-v1.0.zip) | 手動導入用のコンパイル済み MOD。 |
| [Source code (zip)](https://github.com/Chris-E-Shaw/BopItAccess/archive/refs/tags/v1.0.zip) | このリリースのコードを読んだりビルドしたりするためのソースファイル。 |
| [Source code (tar.gz)](https://github.com/Chris-E-Shaw/BopItAccess/archive/refs/tags/v1.0.tar.gz) | 同じソースファイルを別の形式でまとめたアーカイブ。 |

最新の安定版を導入するにはインストーラーで **Install** を選んでください。**Show advanced > Install alpha** は任意の実験的な選択肢です。確認後に `main` の最新ソースからビルドします。GitHub のソースアーカイブにはコードと文書が含まれます。ビルド済みの MOD を導入するには、コンパイル済み ZIP またはインストーラーを使用してください。

Git のコミットがこのプロジェクトの履歴です。最初の 37 ビルドは、それぞれ別のソーススナップショットとして取り込まれました。そのコミット日時は、元のビルド日時ではなく、取り込みを行った日時を示します。コンパイル済みの実行ファイル、ゲームのファイル、MelonLoader が生成するゲームのアセンブリは、このリポジトリには含まれません。

## ドキュメント

インストール、更新、削除、操作、設定、メニュー、すべてのゲームモードについては、[英語のユーザーガイド](../../BopItAccess-user-guide.html)をご覧ください。ゲーム内のガイドは、現在のゲーム言語を自動的に使用します。

## 必要なもの

- Windows x64 と、正規に入手した Steam 版 Bop It! The Video Game。
- x64 版 **MelonLoader 0.7.3 Open-Beta**。開発には Unity 2022.3.50f1 のゲームビルドを使用しています。
- MOD の実行には、Windows x64 版の **.NET 6 ランタイム**。
- 公式の Windows x64 版 **Prism v0.18.3** の `prism.dll`。ゲームの実行ファイルと同じフォルダーに配置します。
- MOD をソースからビルドする場合：**.NET 6 ターゲティングパック**を含む互換性のある .NET SDK と、ご自身のゲームから MelonLoader が生成した参照ファイル。
- インストーラーをソースからビルドする場合：Windows 上の **.NET 10 SDK**。

インストーラーは必要なものを公式の配布元から取得します。コンパイル済みリリースのインストールには開発用 SDK は不要ですが、アルファ版のインストールには必要です。

<a id="build-from-source"></a>
## ソースからビルドする

1. MelonLoader 0.7.3 Open-Beta をゲームのフォルダーにインストールします。ゲームを一度起動し、MelonLoader によるファイルの準備が終わるまで待ってから終了します。生成された参照ファイルは、ゲームのフォルダー内の `MelonLoader\Il2CppAssemblies` にあるはずです。
2. このリポジトリをダウンロードまたはクローンし、そのルートフォルダーで PowerShell を開きます。
3. 以下の例のパスをご自身のゲームの場所に置き換えてから実行します。

   ```powershell
   $gameDir = 'C:\Program Files (x86)\Steam\steamapps\common\Bop It!'
   dotnet build .\src\BopItAccess.csproj -c Release "-p:BopItGameDir=$gameDir"
   ```

コンパイル済みの DLL は `src\bin\Release\net6.0\BopItAccess.dll` です。ゲームやローダーの参照ファイルが不足している場合、プロジェクトはコンパイル前にその旨を知らせます。.NET 6 ターゲティングパックが見つからないと SDK が表示した場合は、そのパックを含む SDK をインストールしてください。プロジェクトの `NuGet.Config` には、オンラインのパッケージ配布元は設定されていません。

インストーラーのアルファ版インストールでは、ゲームを起動せずにローカルのビルド用参照ファイルを準備します。これらはビルドに使う一時的なファイルであり、コミットしたり、コンパイル済み MOD のリリースに含めたりすることはありません。

<a id="install-your-build"></a>
## ビルドした MOD をインストールする

ゲームを終了した状態で、次の操作を行います。

1. ビルドした `BopItAccess.dll` をゲームの `Mods` フォルダーにコピーします。フォルダーがなければ作成します。
2. [Prism のリリース](https://github.com/ethindp/prism/releases)から公式の Windows x64 版 Prism v0.18.3 をダウンロードします。`prism.dll` をゲームの実行ファイルと同じフォルダーに置きます。
3. ビルド出力の `src\bin\Release\net6.0\documentation` フォルダーをゲームのフォルダーにコピーし、すべての言語サブフォルダーを残します。Prism のバイナリを配布する際は、適用される Prism のライセンスファイルも保持してください。
4. スクリーンリーダーを使う場合は先に起動し、その後 Steam からゲームを起動します。起動時の読み上げがあり、続いてタイトル、ようこそ画面、またはメインメニューの読み上げが聞こえてからゲームを操作してください。

MOD のビルドでは Prism のダウンロードやコンパイルは行いません。読み上げが始まらない場合は、ゲームのフォルダーにある `Mods\BopItAccess.log` を確認してください。ログに送信成功と記録されていれば、MOD がテキストを送ったことは確認できますが、実際に音声が聞こえたことまでは証明できません。

コンパイル済みリリースの ZIP を使う場合は、**内容をすべて**ゲームのフォルダーにコピーし、確認が表示されたらフォルダーを統合するかファイルを置き換えてください。ZIP には MOD、Prism、ガイド、ライセンスに関する通知が含まれます。MelonLoader と .NET は別途インストールします。手動インストールの詳しい手順はユーザーガイドをご覧ください。

## ゲームを使わずに設定を編集する

起動後、ゲームのフォルダー内の `UserData\BopItAccess.ini` に、ゲームと MOD の設定、音声プロファイル、プレイヤーが使う操作の割り当てが読みやすい形で保存されます。ゲームを終了し、メモ帳でこのファイルを開き、既存の項目を編集して保存してください。変更は次回の起動時に読み込まれます。コメントに、設定できる選択肢や範囲が説明されています。

例えば、`[Game]` の `Language=en` で英語に戻せます。`MusicVolume`、`SfxVolume`、`VoiceOverVolume` を下げたり、`[OneCore]` または `[SAPI]` の `Voice=System default` で適さない音声の選択を解除したりできます。`[Mod]` の `SpeechOutput=On` と `OutputMode=Auto` で自動の読み上げに戻せます。他の項目はそのまま残し、同じセクションを重複して追加しないでください。

## インストーラーをビルドする

Windows 上で .NET 10 SDK を使い、次を実行します。

```powershell
dotnet restore .\installer\BopItAccess.Installer.csproj --source https://api.nuget.org/v3/index.json
dotnet publish .\installer\BopItAccess.Installer.csproj -c Release -o .\build\installer --no-restore
```

出力は `build\installer\BopItAccess.Installer.exe` です。必要な実行環境を含む Windows x64 実行ファイルなので、ユーザーは実行のために .NET 10 をインストールする必要はありません。提供されるインストーラーは未署名です。操作方法と Windows のセキュリティ確認については、ユーザーガイドで説明しています。

`scripts/package-mod.ps1` は、すでにコンパイルされた対応する MOD からリリース用 ZIP を作成します。プレイヤーに必要なガイド、通知、ライセンスを含めます。開発者向け README、Git の作業手順、生成されたゲームの参照ファイル、コンパイル済みインストーラーは ZIP に含まれません。パッケージ作成では、MOD のコンパイルや GitHub リリースの公開は行いません。

## AI に関する透明性の説明

この MOD は、AI にコード生成を任せる「バイブコーディング」で作られています。コードの生成と調査はすべて人工知能が行い、その内部構造に関する人間の技術的理解は限られています。この MOD はご自身の責任でご利用ください。

ただし、MOD のすべての機能と設計上の決定は、人間が考案し、承認しました。テストは自動化されておらず、実際のプレイヤーとテスターが、丁寧に十分な時間をかけて行いました。

ご注意ください。多言語のテキストとドキュメントは AI が生成したもので、母語話者による確認は行われていません。翻訳には多くの不正確な箇所がある可能性があります。自ら作業を進める AI によるコーディングがなければ、このプロジェクトは存在しなかったでしょう。試してくださり、ありがとうございます！

## ライセンスと法的事項

Bop It Access 独自のソースコードとドキュメントは、**[MIT ライセンス](../../LICENSE)**で提供されます。Copyright © 2026 Christopher Shaw。依存ソフトウェアにはそれぞれのライセンスが適用されます。MIT ライセンスによってそれらのライセンスが変更されたり、ゲームの素材に関する権利が与えられたりすることはありません。依存ソフトウェアに関する通知は [THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt) をご覧ください。

Bop It Access は、ファンが作成した非公式プロジェクトです。Hasbro、ゲームの開発・販売元 Alliance、Valve、Microsoft、Unity、MelonLoader、Prism、またはスクリーンリーダーの開発元が制作、承認、推奨するものではありません。**Bop It!** とそれに関するキャラクター、絵、音、商標は、Hasbro およびそれぞれの権利者に帰属します。Steam は Valve に帰属します。他の製品名、商標、ソフトウェアも、それぞれの所有者に帰属します。

ゲームは必ずご自身で正規に入手してください。このリポジトリにはゲームやその素材は含まれておらず、それらに関する権利も与えません。元のゲームの権利については、[Bop It! の公式ゲームサイト](https://bopitthevideogame.com/)と [Steam の掲載ページ](https://store.steampowered.com/app/3214360/)をご覧ください。

## ありがとうございます

公開前にこの MOD をプレイテストし、今の状態にするために力を貸してくださった皆さん、ありがとうございます。皆さんなら、ご自身のことだと分かるでしょう。フィードバックをくださる方、初めて MOD を試す方、私とこのプロジェクトを信じてくださる方も、ありがとうございます。この大変な世界で作り続けていこうと思えるのは、皆さんの支えのおかげです。このプロジェクトが、ゲームを楽しみ、ほかの人と一緒に遊ぶための助けになればうれしく思います。本当にありがとうございます。Bop It! をお楽しみください！

— Christopher Shaw
