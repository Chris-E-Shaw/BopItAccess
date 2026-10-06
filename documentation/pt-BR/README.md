# Bop It Access

Bop It Access é um mod de acessibilidade não oficial para a versão Windows Steam de **Bop It!**. Ele usa MelonLoader e [Prism](https://github.com/ethindp/prism) para adicionar feedback de fala e braille aos menus e telas do jogo. Os recursos atuais incluem uma tela de boas-vindas de primeira execução, um guia do usuário no jogo, título falado e telas de pausa, configurações e controles, seleção de músicas, pontuações finais e tabelas de classificação, conquistas, créditos, dicas de botões, texto tutorial sob demanda com atribuições de controle atuais antes de uma rodada e descrições dos quatro estágios. A versão 0.9.0 usa Prism para saída de fala e braille. O mod segue o idioma selecionado do jogo e inclui um guia para cada idioma que o jogo oferece.

## Status do projeto

Este projeto está em desenvolvimento inicial. Este repositório contém código-fonte e documentação técnica. **Não há compilações compiladas ou versões GitHub aqui ainda.** Para usar o mod deste repositório, construa-o a partir do código-fonte e forneça o tempo de execução Prism descrito abaixo.

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

## Documentação

- [Guia do usuário de jogos e mods (inglês)](BopItAccess-user-guide.html) - um passo a passo de controles, configurações, menus e modos de jogo para iniciantes.
- [Guia do usuário japonês (日本語)](../ja/BopItAccess-user-guide.html). Outros guias traduzidos estão disponíveis nas pastas de idiomas em [`documentation/`](../).
- [Recursos detalhados e guia de controle](README.txt). Sua seção de instalação descreve os ZIPs de instalação preparados localmente; este repositório GitHub fornece apenas a fonte.
- [Histórico técnico de construção](BopItAccess-build-history.html).
- [Fluxo de trabalho Git para este projeto](GIT-WORKFLOW.md).
- [Avisos de terceiros](THIRD-PARTY-NOTICES.txt).

Cópias traduzidas de todos os seis documentos acima estão em [`documentation/`](../) em cada código de idioma suportado. A fonte deles é o inglês; `scripts/translate_documents.py` pode regenerar os rascunhos traduzidos automaticamente após alterações na fonte.

## Transparência de IA

Christopher Shaw dirige este projeto e avalia sua acessibilidade no jogo. Os modelos OpenAI Codex ajudaram na pesquisa, código e documentação. As mensagens de commit publicadas incluem um `Co-authored-by` trailer identificando o modelo que contribuiu para cada mudança; os créditos históricos foram verificados em relação aos registros da sessão deste projeto. O histórico de construção anterior foi reconstruído a partir de arquivos de origem salvos, em vez de registrado como commits no momento. As contribuições assistidas por IA podem conter erros e devem ser revisadas antes do uso.

## Licenciamento

Uma licença para a origem Bop It Access ainda não foi selecionada. Prism possui licença própria; veja o [avisos de terceiros](THIRD-PARTY-NOTICES.txt). Bop It! e seus bens pertencem aos seus respectivos proprietários e não estão incluídos aqui.
