# Bop It Access

Bop It Access é um mod de acessibilidade para pessoas cegas, destinado à versão Steam para Windows x64 de **Bop It! The Video Game**. Ele usa [MelonLoader](https://github.com/LavaGang/MelonLoader) e [Prism](https://github.com/ethindp/prism) para oferecer fala e braille nos menus e telas do jogo, com controles e configurações adicionais para uma experiência mais confortável.

**A versão 1.0 está disponível para Windows x64.** O mod e o instalador usam a versão de código-fonte **1.0.0**. Baixe o [instalador](https://github.com/Chris-E-Shaw/BopItAccess/releases/download/v1.0/BopItAccess-Installer.exe) para uma instalação guiada ou o [ZIP compilado](https://github.com/Chris-E-Shaw/BopItAccess/releases/download/v1.0/BopItAccess-v1.0.zip) para instalar manualmente.

## Recursos

- Fala para menus, configurações, tutoriais, placares, conquistas, créditos, telas de pausa e resultados.
- Leitura sob demanda de pontuações, descrições de cenários e dicas que respeitam as atribuições atuais dos controles.
- Fala e um guia integrado em todos os idiomas oferecidos pelo jogo.
- Saída para leitores de tela e braille, com OneCore e SAPI como opções de voz do sistema.
- Nível de detalhe da fala, tempo e repetição de dicas ajustáveis, além de atalhos de fala que podem ser remapeados.
- Atribuições adicionais para controles do jogo, limite de quadros por segundo, controles de áudio em segundo plano e um arquivo de configurações legível.
- Instalador acessível com suporte a teclado e controle, para instalação, atualização e desinstalação.

## Estado do projeto

Os principais recursos estão praticamente completos. O projeto será mantido conforme necessário, com o retorno dos jogadores orientando correções e melhorias. A plataforma atualmente compatível é Windows x64.

A [primeira versão pública, v1.0](https://github.com/Chris-E-Shaw/BopItAccess/releases/tag/v1.0), oferece quatro downloads:

| Download | Finalidade |
| --- | --- |
| [BopItAccess-Installer.exe](https://github.com/Chris-E-Shaw/BopItAccess/releases/download/v1.0/BopItAccess-Installer.exe) | Instalador acessível que encontra o jogo, instala as dependências e o mod estável mais recente e gerencia atualizações e desinstalação. |
| [BopItAccess-v1.0.zip](https://github.com/Chris-E-Shaw/BopItAccess/releases/download/v1.0/BopItAccess-v1.0.zip) | Mod compilado para instalação manual. |
| [Source code (zip)](https://github.com/Chris-E-Shaw/BopItAccess/archive/refs/tags/v1.0.zip) | Arquivos de código-fonte para ler ou compilar esta versão. |
| [Source code (tar.gz)](https://github.com/Chris-E-Shaw/BopItAccess/archive/refs/tags/v1.0.tar.gz) | Os mesmos arquivos fonte em outro formato de arquivo. |

Escolha **Install** no instalador para obter a versão estável mais recente. **Show advanced > Install alpha** é uma opção experimental e opcional que compila o código-fonte mais recente de `main`, após pedir confirmação. Os arquivos fonte do GitHub contêm código e documentação; use o ZIP compilado ou o instalador para instalar o mod já compilado.

Os commits do Git são o histórico do projeto. As primeiras 37 compilações foram importadas como versões separadas do código-fonte; as datas desses commits registram a importação, não as datas das compilações originais. Este repositório não inclui binários compilados, arquivos do jogo ou assemblies do jogo gerados pelo MelonLoader.

## Documentação

[Leia o guia do usuário em inglês](../../BopItAccess-user-guide.html) para aprender sobre instalação, atualizações, desinstalação, controles, configurações, menus e todos os modos de jogo. O guia integrado usa automaticamente o idioma atual do jogo.

## Requisitos

- Windows x64 e sua própria instalação do Steam, adquirida legalmente, de Bop It! The Video Game.
- **MelonLoader 0.7.3 Open-Beta**, x64. O desenvolvimento usa a versão do jogo com Unity 2022.3.50f1.
- O **runtime .NET 6** para Windows x64, para executar o mod.
- O arquivo oficial **Prism v0.18.3** para Windows x64, `prism.dll`, instalado ao lado do executável do jogo.
- Para compilar o mod a partir do código-fonte: um SDK .NET compatível com o **pacote de direcionamento do .NET 6** e as referências geradas pelo MelonLoader a partir do seu próprio jogo.
- Para compilar o instalador a partir do código-fonte: o **SDK .NET 10** no Windows.

O instalador obtém suas dependências das fontes oficiais. A instalação de uma versão compilada não exige um SDK de desenvolvimento; Instalar alfa exige.

<a id="build-from-source"></a>
## Compilar a partir do código-fonte

1. Instale o MelonLoader 0.7.3 Open-Beta na pasta do jogo. Inicie o jogo uma vez, espere o MelonLoader preparar seus arquivos e então feche o jogo. As referências geradas devem estar em `MelonLoader\Il2CppAssemblies`, na pasta do jogo.
2. Baixe ou clone este repositório e abra o PowerShell na pasta raiz dele.
3. Substitua o caminho de exemplo abaixo pelo local do seu jogo e execute:

   ```powershell
   $gameDir = 'C:\Program Files (x86)\Steam\steamapps\common\Bop It!'
   dotnet build .\src\BopItAccess.csproj -c Release "-p:BopItGameDir=$gameDir"
   ```

A DLL compilada é `src\bin\Release\net6.0\BopItAccess.dll`. O projeto informa referências ausentes do jogo ou do carregador antes da compilação. Se o SDK informar que falta o pacote de direcionamento do .NET 6, instale um SDK que o contenha. O arquivo `NuGet.Config` do projeto não configura fontes de pacotes on-line.

O processo alfa do instalador prepara suas referências locais de compilação sem iniciar o jogo. Essas referências são arquivos temporários necessários à compilação; nunca são incluídas nos commits ou em uma versão compilada do mod.

<a id="install-your-build"></a>
## Instalar sua compilação

Com o jogo fechado:

1. Copie o arquivo `BopItAccess.dll` compilado para a pasta `Mods` do jogo, criando essa pasta se necessário.
2. Baixe o Prism v0.18.3 oficial para Windows x64 na página de [versões do Prism](https://github.com/ethindp/prism/releases). Coloque `prism.dll` ao lado do executável do jogo.
3. Copie a pasta `src\bin\Release\net6.0\documentation` da compilação para a pasta do jogo, mantendo todas as subpastas de idiomas. Preserve os arquivos de licença aplicáveis do Prism ao distribuir seu binário.
4. Inicie seu leitor de tela, se usar um, e depois inicie o jogo pelo Steam. Espere o anúncio de inicialização e depois o anúncio da tela de título, de boas-vindas ou do menu principal antes de usar os controles do jogo.

A compilação do mod não baixa nem compila o Prism. Se a fala não iniciar, consulte `Mods\BopItAccess.log` na pasta do jogo. Um envio bem-sucedido no registro confirma que o mod enviou texto; não pode provar que o áudio foi ouvido.

Para o ZIP de uma versão compilada, copie **todo o conteúdo** para a pasta do jogo e mescle pastas ou substitua arquivos quando solicitado. O ZIP inclui o mod, Prism, guias e avisos de licença; MelonLoader e .NET são instalados separadamente. Consulte o guia do usuário para as instruções completas de instalação manual.

## Editar configurações fora do jogo

Após a inicialização, `UserData\BopItAccess.ini`, na pasta do jogo, contém configurações legíveis do jogo e do mod, perfis de voz e atribuições de controles destinados aos jogadores. Feche o jogo, abra o arquivo no Bloco de Notas, edite as entradas existentes e salve. O mod lê as alterações na próxima inicialização. Os comentários explicam as opções e os intervalos válidos.

Por exemplo, defina `Language=en` em `[Game]` para restaurar o inglês, reduza `MusicVolume`, `SfxVolume` e `VoiceOverVolume`, ou defina `Voice=System default` em `[OneCore]` ou `[SAPI]` para substituir uma voz inadequada. Defina `SpeechOutput=On` e `OutputMode=Auto` em `[Mod]` para restaurar a fala automática. Mantenha as outras entradas; não acrescente seções duplicadas.

## Compilar o instalador

No Windows com o SDK .NET 10, execute:

```powershell
dotnet restore .\installer\BopItAccess.Installer.csproj --source https://api.nuget.org/v3/index.json
dotnet publish .\installer\BopItAccess.Installer.csproj -c Release -o .\build\installer --no-restore
```

O resultado é `build\installer\BopItAccess.Installer.exe`. É um executável autossuficiente para Windows x64, portanto os usuários não precisam do .NET 10 para executá-lo. O instalador fornecido não é assinado. O guia do usuário explica seus controles e as solicitações de segurança do Windows.

`scripts/package-mod.ps1` prepara um ZIP de lançamento a partir de um mod já compilado e da versão correspondente. Ele inclui os guias, avisos e licenças necessários aos jogadores. READMEs para desenvolvedores, notas do fluxo de trabalho do Git, referências geradas do jogo e arquivos compilados do instalador ficam fora desse ZIP. A criação do pacote não compila o mod nem publica uma versão no GitHub.

## Nota de transparência sobre IA

Este mod foi criado por meio de «vibe coding». Todo o código foi completamente gerado e pesquisado por inteligência artificial, com compreensão técnica humana limitada de sua arquitetura interna. Use este mod por sua conta e risco.

Dito isso, todos os recursos e decisões de design do mod foram criados e aprovados por humanos. Os testes nunca foram automatizados; foram realizados de forma cuidadosa e extensa por jogadores e testadores humanos reais.

Observe que os textos e a documentação em vários idiomas foram gerados por IA e não foram revisados por falantes nativos. Deve-se esperar um alto grau de imprecisão nas traduções. Sem programação com agentes de IA, este projeto não existiria. Obrigado por dar uma chance a ele!

## Licença e avisos legais

O código-fonte próprio de Bop It Access e sua documentação são licenciados sob a **[licença MIT](../../LICENSE)**. Copyright © 2026 Christopher Shaw. As dependências mantêm suas próprias licenças; a licença MIT não altera essas licenças nem concede direitos sobre os recursos do jogo. Consulte [THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt) para os avisos das dependências.

Bop It Access é um projeto não oficial criado por um fã. Não é produzido, aprovado ou endossado por Hasbro, Alliance, a desenvolvedora/publicadora do jogo, Valve, Microsoft, Unity, MelonLoader, Prism ou qualquer fabricante de leitores de tela. **Bop It!** e seus personagens, ilustrações, sons e marcas pertencem à Hasbro e aos respectivos titulares dos direitos. O Steam pertence à Valve. Outros nomes de produtos, marcas e programas continuam sendo propriedade de seus respectivos titulares.

Você deve obter sua própria cópia legal do jogo. Este repositório não inclui o jogo ou seus recursos e não concede direitos sobre eles. Para informações sobre os direitos do jogo original, consulte o [site oficial de Bop It!](https://bopitthevideogame.com/) e a [página do Steam](https://store.steampowered.com/app/3214360/).

## Obrigado

A quem testou este mod antes do lançamento e ajudou a trazê-lo até aqui: obrigado. Vocês sabem quem são. Aos jogadores que oferecem comentários, experimentam o mod pela primeira vez ou acreditam em mim e neste projeto: obrigado. Seu apoio me motiva a continuar criando neste mundo louco em que vivemos. Espero que este projeto torne mais fácil aproveitar o jogo e jogar com outras pessoas. Muito obrigado a todos. Divirtam-se com Bop It!

— Christopher Shaw
