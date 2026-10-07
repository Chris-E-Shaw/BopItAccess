# Bop It Access

Bop It Access é um mod de acessibilidade não oficial para a versão Windows Steam de **Bop It!**. Ele usa MelonLoader e [Prism](https://github.com/ethindp/prism) para adicionar feedback de fala e braille aos menus e telas do jogo. Os recursos atuais incluem uma tela de boas-vindas de primeira execução, um guia do usuário no jogo, título falado e telas de pausa, configurações e controles, seleção de músicas, pontuações finais e tabelas de classificação, conquistas, créditos, dicas de botões, texto tutorial sob demanda com atribuições de controle atuais antes de uma rodada e descrições dos quatro estágios. A versão 0.9.0 usa Prism para saída de fala e braille. O mod segue o idioma selecionado do jogo e inclui um guia para cada idioma que o jogo oferece.

## Editar o arquivo de configurações

Se um idioma desconhecido, o áudio muito alto ou uma voz problemática dificultarem o uso dos menus, você pode mudar as configurações fora do jogo. Depois da inicialização, o mod cria automaticamente UserData/BopItAccess.ini na pasta de Bop It!, usando suas configurações atuais. É um arquivo de texto que pode ser aberto em um editor como o Bloco de Notas.

O arquivo inclui idioma, volumes de música, efeitos e voz, vibração, tela cheia, resolução e latência de áudio do jogo; preferências de fala, braille, dicas e outras opções do mod; perfis de voz separados para OneCore e SAPI; e atribuições dos controles do jogo e do mod destinados aos jogadores. As resoluções disponíveis e as vozes instaladas aparecem nos comentários.

Feche o jogo antes de editar. Encontre a seção adequada e mude o valor da entrada existente, salve o arquivo e inicie o jogo novamente. As alterações são lidas na inicialização, não imediatamente durante uma sessão. Mudanças feitas nos menus atualizam o arquivo automaticamente.

Os nomes das seções e configurações permanecem em inglês em todos os idiomas. On e Off são recomendados para ligar e desligar opções; True/False, Yes/No e 1/0 também são aceitos. Os comentários explicam as opções e os intervalos. Uma entrada ausente ou inválida mantém a configuração salva correspondente; as outras alterações válidas ainda são aplicadas. Atribuições de controles duplicadas são rejeitadas.

Comentários e entradas desconhecidas são preservados. Se outro programa modificar o arquivo enquanto o jogo estiver aberto, o mod para de salvá-lo pelo restante da sessão para proteger essas alterações. Feche e reabra o jogo para aplicá-las. Você pode guardar uma cópia de segurança antes de editar.

Se uma voz apresentar problemas, defina Voice=System default na seção OneCore ou SAPI. As vozes OneCore usam nome | idioma; SAPI aceita o nome exibido de uma voz instalada ou seu identificador completo do Registro. O arquivo lista as opções disponíveis. OutputMode=Auto tenta um leitor de tela compatível em execução, depois OneCore e finalmente SAPI.

O exemplo abaixo restaura o inglês, um áudio de jogo mais baixo e a saída de fala automática com as vozes padrão do sistema. Mude as entradas correspondentes que já estão no seu arquivo; este trecho serve de referência, não é outro bloco para acrescentar ao final. Preserve suas outras configurações.

```ini
[Game]
Language=en
MusicVolume=30
SfxVolume=30
VoiceOverVolume=30

[Mod]
SpeechOutput=On
OutputMode=Auto

[OneCore]
Voice=System default

[SAPI]
Voice=System default
```

Arquivos instalados são registrados em um manifesto de propriedade para que atualizações preservem arquivos já existentes e uma instalação cancelada possa reverter suas próprias alterações. **Desinstalar** e **Aplicativos Instalados** do Windows usam o mesmo código de desinstalação. O arquivo fornecido `installer/uninstall.ps1` abre uma janela acessível de desinstalação a partir dos Aplicativos Instalados. Após a remoção completamente bem-sucedida, ele remove o iniciador de desinstalação, os registros de propriedade e a entrada do Windows. Outros mods existentes e arquivos compartilhados do MelonLoader são preservados. Tanto a limpeza de instalações gerenciadas quanto a de antigas instalações manuais abrangem os registros conhecidos do mod, incluindo `Mods/BopItAccess.log.previous`, `UserData/BopItAccess.ini`, o antigo `UserData/BopItAccess.ini.tmp`, e restos validados de `BopItAccess.ini.<GUID>.tmp` em `UserData`. Aqui, `<GUID>` deve ter exatamente 32 caracteres hexadecimais sem hífens; arquivos arbitrários que correspondam a um curinga amplo não são removidos.

Para cópias antigas instaladas manualmente sem manifesto de propriedade, a desinstalação remove os arquivos identificáveis do Bop It Access e preserva arquivos compartilhados cuja origem não pode ser comprovada. A desinstalação também remove apenas os valores de preferências `BopItAccess.*` de cada perfil local de usuário Windows, incluindo perfis desconectados. As preferências do jogo e o SDK .NET permanecem. Se o Windows negar acesso ou outra etapa não puder terminar com segurança, o instalador informa limpeza incompleta. Para cópias gerenciadas pelo instalador, sua entrada do Windows, iniciador de desinstalação e ponto de controle durável permanecem disponíveis até a limpeza ter êxito, permitindo repetir as etapas restantes. Instalações manuais antigas não têm esse registro durável; seus avisos podem ser tentados novamente no instalador aberto.

## Status do projeto

Este projeto está essencialmente completo e nenhum conteúdo ou recursos importantes estão planejados. Ele será mantido conforme necessário, com o feedback dos jogadores orientando as melhorias. O repositório GitHub contém código-fonte e documentação técnica. **Ainda não há versões GitHub.** A fonte agora também contém um projeto de instalação Windows. Até que uma versão seja publicada, o botão **Instalar** explica que nenhuma versão está disponível; **Instalar alfa** cria o commit do branch principal mais recente a partir do código-fonte.

O histórico de commits inclui snapshots de origem reconstruídos de 37 compilações anteriores. Os commits foram criados quando esses arquivos foram importados para o Git; suas datas não são as datas originais de construção. O [histórico de construção técnica](BopItAccess-build-history.html) descreve o trabalho por trás de cada instantâneo.

## Requisitos

- Windows x64 e instalação própria de Bop It! para Steam.
- MelonLoader instalado no diretório do jogo. O desenvolvimento usou MelonLoader **0.7.3 Open-Beta** com a versão x64 Unity **2022.3.50f1** do jogo. Outras combinações não foram verificadas.
- Um SDK .NET com o **pacote de segmentação .NET 6**, porque o mod tem como alvo `net6.0`.
- Para instalação, o oficial Windows x64 Prism v0.18.3 `prism.dll`. Este binário de terceiros não está neste repositório.

O mod faz referência a DLLs geradas ou instaladas por MelonLoader no diretório do jogo. Não inclui nem redistribui montagens de jogos.

<a id="build-from-source"></a>
## Construir a partir da fonte

1. Instale MelonLoader, inicie Bop It! uma vez e feche o jogo. MelonLoader deveria criar `MelonLoader\Il2CppAssemblies` abaixo do diretório do jogo.
2. Clone ou baixe este repositório. Abra PowerShell no diretório raiz do repositório.
3. Definir `$gameDir` para **seu** diretório de instalação Bop It! e, em seguida, crie:

   ```powershell
   $gameDir = 'C:\Program Files (x86)\Steam\steamapps\common\Bop It!'
   dotnet build .\src\BopItAccess.csproj -c Release "-p:BopItGameDir=$gameDir"
   ```

   O caminho de exemplo é o local habitual Windows de Steam. Altere-o se sua biblioteca Steam estiver em outro lugar. O projeto verifica o MelonLoader necessário e as DLLs do jogo geradas e relata um caminho ausente antes de compilar.

4. A DLL do mod construído estará em `src\bin\Release\net6.0\BopItAccess.dll`.

Se o SDK relatar a falta de um pacote de segmentação .NET 6, instale um SDK que inclua esse pacote. O projeto `NuGet.Config` não configura feeds de pacotes on-line.

<a id="install-your-build"></a>
## Instale sua compilação

1. Feche o jogo. Copie o construído `BopItAccess.dll` em `<game directory>\Mods\`. Crie o `Mods` diretório se MelonLoader não o tiver criado.
2. Obtenha a versão oficial do Prism v0.18.3 para Windows x64 (`prism.dll`) na [página de lançamentos do Prism](https://github.com/ethindp/prism/releases), ou compile a mesma versão a partir do código-fonte. Coloque `prism.dll` ao lado do executável do jogo, na pasta principal do jogo, não na pasta `Mods`.
3. Copie todo o build `src\bin\Release\net6.0\documentation\` pasta no diretório do jogo. Ele contém o guia em inglês na raiz e guias traduzidos em `fr`, `it`, `de`, `es`, `es-MX`, `ja`, `ko`, `zh`e `pt-BR`. Mantenha essas subpastas e os documentos complementares. O guia do jogo lê o HTML da linguagem atual do jogo sempre que é aberto, portanto, a substituição de um guia atualiza seu conteúdo sem reconstruir a DLL.
4. Inicie o leitor de tela antes do jogo. Sem um leitor compatível em execução, o Prism prefere OneCore e usa SAPI se OneCore não estiver disponível.

O comando build Bop It Access compila apenas este mod; ele não compila ou baixa Prism. Se a fala não iniciar, inspecione `<game directory>\Mods\BopItAccess.log`. O log registra a inicialização Prism e o envio de fala, embora um envio bem-sucedido por si só não possa provar que o áudio foi ouvido.

Na primeira execução, a tela de boas-vindas aparece após o menu principal do jogo estar pronto. Suas opções abrem as configurações do mod, leem o guia do usuário no jogo ou continuam no jogo. Mod Settings também oferece **Abrir o Guia do Usuário** e uma ação confirmada **Redefinir tela de boas-vindas** que mostra a tela de boas-vindas na próxima inicialização. No guia, use Para cima/Para baixo para escolher tópicos ou ler linhas e Confirmar para abrir um tópico. Nas tabelas, Esquerda move uma coluna para a esquerda, Direita move uma coluna para a direita e Cima/Baixo mantém a coluna atual enquanto altera as linhas. Os títulos das colunas rotulam as células em vez de aparecerem como linhas de dados; a tabela é anunciada na entrada e seu fim na saída. Atrás deixa um tópico ou o guia.

Escolha um idioma na linha **Configurações > Idioma** do jogo. O discurso mod segue essa seleção. O guia do jogo usa o documento HTML traduzido correspondente, com o inglês como alternativa se a cópia selecionada estiver faltando ou ilegível. O texto incluído em outro idioma que não o inglês é uma primeira passagem traduzida automaticamente; correções de falantes fluentes são bem-vindas.

As ações usam as traduções do jogo. Shapes, Space, City e Office mantêm os nomes de cenário em inglês. Para OneCore ou SAPI, escolha nas configurações do mod uma voz instalada para o idioma do jogo se a voz padrão não soar adequada. Voz, Volume, Velocidade e Tom ajustam a saída OneCore ou SAPI realmente em uso, mesmo no modo Auto. Só aparecem os controles compatíveis; com outras saídas, eles são ocultados. Cada mecanismo guarda suas configurações separadamente.

Ler posições nos menus. Essa opção salva, ativada por padrão, anuncia a posição do item dentro do menu. O feedback individual é salvo e fica ativado por padrão. Anuncia a cor ativa no início e quando ela muda. Ao perder uma vida, anuncia a quantidade restante. Ao ganhar uma vida, anuncia a cor do jogador e o novo total, como “Verde, 3 vidas”, para que ambos saibam quem foi mais rápido. Esses anúncios são desativados quando o feedback individual está desligado. O limite de três vidas do jogo não muda. Esse recurso atua apenas durante uma partida individual entre dois jogadores. Após a última entrada de calibração, o mod diz imediatamente “Concluído!”. Pare de bater e espere o resultado medido. Se a calibração falhar porque nenhuma entrada foi feita, ele diz “Falha na calibração.”.

**Formatar fala**: Deixa o texto em maiúsculas mais natural para fala e braille. No guia dentro do jogo, acrescenta reticências antes do número da linha se o texto terminar sem pontuação. O texto visível não muda. Desative para receber o texto como foi escrito.

### MelonLoader janelas de inicialização

O modelo Loader.cfg fornecido oculta a tela inicial e o console separados do MelonLoader. O instalador aplica esses dois padrões antes de você iniciar o jogo por conta própria. Eles não pulam a tela de título do jogo nem a tela de boas-vindas do mod.

Com o jogo fechado, abra `UserData/Loader.cfg` na pasta do jogo. Se o arquivo já existir, defina `disable_start_screen` como `true` na seção `[loader]` existente e `hide_console` como `true` na seção `[console]` existente. Mantenha todas as outras entradas. Se o arquivo não existir, copie o modelo `UserData/Loader.cfg` fornecido com a compilação, ou `configuration/Loader.cfg` a partir do código-fonte. Nunca substitua um Loader.cfg existente pelo modelo completo.

```ini
[loader]
disable_start_screen = true

[console]
hide_console = true
```

O mod não redefine essas opções a cada inicialização. Você pode alterar manualmente qualquer valor de volta para falso se precisar das janelas do carregador para solução de problemas. A desinstalação do instalador restaura os valores originais apenas enquanto os valores verdadeiros do instalador ainda estão presentes, preservando outras edições de configuração do carregador.

## Documentação

- [Guia do usuário de jogos e mods (inglês)](BopItAccess-user-guide.html) - um passo a passo de controles, configurações, menus e modos de jogo para iniciantes.
- [Guia do usuário japonês (日本語)](../ja/BopItAccess-user-guide.html). Outros guias traduzidos estão disponíveis nas pastas de idiomas em [`documentation/`](../).
- [Recursos detalhados e guia de controle](README.txt). Sua seção de instalação descreve os ZIPs de instalação preparados localmente; este repositório GitHub fornece apenas a fonte.
- [Histórico técnico de construção](BopItAccess-build-history.html).
- [Revisão do código para preparar o lançamento](BopItAccess-release-review.html) — problemas implementados, arquivos revisados, resultados de compilação e limites restantes.
- [Fluxo de trabalho Git para este projeto](GIT-WORKFLOW.md).
- [Avisos de terceiros](THIRD-PARTY-NOTICES.txt).

Cópias traduzidas dos documentos acima estão em [`documentation/`](../) sob o código de cada idioma compatível. A fonte é o inglês; `scripts/translate_documents.py` pode regenerar os rascunhos traduzidos automaticamente após alterações na fonte.

## O que pode vir a seguir

Este projeto está essencialmente completo e nenhum conteúdo ou recursos importantes estão planejados. No entanto, este mod será mantido e atualizado ativamente ao longo do tempo, conforme necessário, com o feedback dos jogadores impulsionando essas melhorias. O trabalho futuro potencial inclui revisões adicionais e correções de bugs, refinamento de código e melhorias contínuas na capacidade de resposta da fala. Prism cria um caminho possível para outras plataformas no futuro, mas este mod atualmente suporta apenas Windows x64. O repositório do projeto é o local para acompanhar o desenvolvimento.

## Nota sobre transparência de IA

Este mod foi criado por meio de “vibe coding”. Todo o código foi totalmente gerado e pesquisado por inteligência artificial, com compreensão técnica humana limitada de sua arquitetura subjacente. Por favor, use este mod por sua própria conta e risco.

Dito isto, cada recurso de mod e decisão de design foram criados e aprovados por humanos. Os testes nunca foram automatizados; foram realizados cuidadosa e extensivamente por jogadores e testadores humanos reais.

Observação: o texto e a documentação multilíngues foram gerados pela IA e não foram revisados por falantes nativos. É esperada uma alta imprecisão na tradução. Sem codificação agente, este projeto não existiria. Obrigado por dar uma chance!

## Obrigado

Para aqueles que testaram este mod antes do lançamento e ajudaram a chegar onde está agora, obrigado. Vocês sabem quem vocês são. Aos jogadores que deram feedback, experimentem o mod pela primeira vez, ou acreditem em mim e neste projeto, obrigado. Seu apoio me motiva a continuar fazendo coisas em um mundo que pode parecer louco e profundamente falho. Espero que este projeto torne mais fácil para você aproveitar o jogo e jogar com outras pessoas. Muito obrigado a todos. Aproveite Bop It!

-Christopher Shaw

## Licenciamento

Uma licença para a origem Bop It Access ainda não foi selecionada. Prism possui licença própria; veja o [avisos de terceiros](THIRD-PARTY-NOTICES.txt). Bop It! e seus bens pertencem aos seus respectivos proprietários e não estão incluídos aqui.

## Instalação e primeira inicialização

A prévia 0.1.8 do instalador nunca inicia Bop It! durante a instalação. Install baixa uma versão pública compilada quando ela existe. Install alpha baixa o código mais recente, pede confirmação e o compila antes de copiar os arquivos. Alpha reutiliza referências locais completas ou gera referências temporárias a partir do jogo instalado, sem executá-lo. Após a preparação, o instalador coloca MelonLoader na pasta do jogo e imediatamente BopItAccess.dll em Mods. Depois termina Prism, configuração, documentação e arquivos de desinstalação. Aguarde a mensagem de sucesso e inicie o jogo por conta própria pelo Steam quando estiver pronto.

Uma versão compilada precisa do runtime Windows x64 do .NET 6, não de um SDK de desenvolvimento. Runtimes completos existentes são reutilizados. Se faltar, o instalador baixa o ZIP oficial Microsoft do runtime .NET 6.0.36 e o coloca em MelonLoader/Dependencies/dotnet, um local compatível. Esses arquivos ficam registrados para reversão e desinstalação; são mantidos se outros mods precisarem do loader compartilhado. Install alpha também precisa de SDK compatível e pacote de direcionamento .NET 6. Reutiliza um SDK instalado ou uma pasta dotnet antiga compatível; se necessário instala um SDK oficial Microsoft para todo o sistema. O SDK permanece após desinstalação ou cancelamento. Esta prévia não cria nova pasta SDK dotnet na raiz do jogo. Ela obtém MelonLoader 0.7.3 e Prism 0.18.3 oficiais quando necessário. As atualizações continuam pelo GitHub. Abort pede confirmação e desfaz as alterações desta instalação nos arquivos do jogo.

Na primeira inicialização manual após instalar MelonLoader, ele pode baixar arquivos auxiliares e gerar os assemblies do jogo. Aguarde cerca de um minuto, ou mais em alguns sistemas. O mod não pode falar até que MelonLoader termine de carregá-lo. Mantenha o jogo aberto e espere o anúncio inicial do Bop It Access, depois o anúncio da tela de título ou do menu, antes de usar os controles.

A prévia 0.1.8 do instalador corrige um erro de empacotamento que podia interromper Install alpha após a geração das referências. Ela verifica os modelos de compilação incluídos antes de baixar os pré-requisitos ou gerar referências. A ordem de instalação não muda e o instalador nunca inicia o jogo. Salve os diagnósticos antes da próxima tentativa para permitir a investigação de outros problemas.
