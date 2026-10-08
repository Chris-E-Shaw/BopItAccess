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

Após confirmar Uninstall, escolha Uninstall for me ou Uninstall for everyone. Ambas as opções removem os arquivos compartilhados do mod desta pasta do jogo, portanto o mod deixará de estar disponível para qualquer pessoa que use essa instalação. A escolha determina de quem serão removidas as preferências salvas do mod no Windows: apenas da conta que solicitou a operação ou de todos os perfis locais do Windows, incluindo os perfis sem sessão iniciada. As preferências do jogo original são mantidas. O SDK do .NET permanece instalado.

Quando o instalador remove sua própria instalação do MelonLoader e nenhum outro mod precisa dela, ele também remove os arquivos conhecidos Loader.cfg e MelonPreferences.cfg e as pastas vazias Plugins, UserLibs e UserData. As configurações do Bop It Access, os registros conhecidos, os guias e os arquivos do instalador são removidos. Outros mods, arquivos compartilhados do carregador que já existiam e arquivos não reconhecidos são protegidos. Isso também significa que um arquivo desconhecido pode fazer uma pasta permanecer; o instalador informa isso nos dados de diagnóstico em vez de excluir dados sem relação com o mod.

Aplicativos instalados do Windows usa a mesma confirmação, escolha de preferências e limpeza. O instalador fornece BopItAccess-uninstall.ps1 na pasta do jogo como um atalho para o desinstalador instalado; futuras compilações do código-fonte também incluem esse script nos arquivos de saída. Um script copiado manualmente não instala o próprio desinstalador. Para uma instalação manual antiga sem um registro de propriedade, o instalador remove os arquivos do mod que consegue identificar e mantém os arquivos compartilhados cuja origem não pode ser determinada.

Se a limpeza não puder terminar com segurança, o instalador explica isso e mantém as informações necessárias para tentar novamente. Para uma instalação gerenciada, a entrada de desinstalação no Windows e o ponto de verificação da limpeza permanecem até que a remoção seja concluída com sucesso. Uma cópia manual antiga não possui um registro persistente de propriedade; tente novamente as operações indicadas nos avisos com o instalador aberto. Não instale, atualize nem remova o mod enquanto o Bop It! estiver em execução.

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

O mod não redefine essas opções a cada inicialização. Você pode alterar manualmente qualquer um dos dois valores de volta para false para solucionar problemas. Se a desinstalação mantiver uma instalação compartilhada do MelonLoader, ela restaura apenas os sinalizadores de destino definidos pelo instalador que não tenham sido alterados e preserva as demais edições. Se ela remover sua própria instalação do MelonLoader que não é mais usada, também remove os arquivos conhecidos Loader.cfg e MelonPreferences.cfg.

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


## Prévia 0.2.2 do instalador para Windows

Feche o Bop It!, abra o instalador e aprove a solicitação de permissões de administrador do Windows. O instalador apresenta uma mensagem de boas-vindas, procura o jogo nas bibliotecas do Steam em todas as unidades disponíveis e tenta trazer sua janela para o primeiro plano. Confira a pasta do jogo exibida; use Browse se precisar escolher outra pasta. A tecla Tab move o foco entre os controles. O registro de status é um campo de texto somente leitura: coloque o foco nele para revisar as mensagens com as teclas de direção, selecionar texto ou copiá-lo.

O instalador 0.2.2 solicita brevemente ativação em primeiro plano e foco do teclado ao iniciar. Se outra janela ainda estiver ativa ao terminar a observação inicial limitada, ele faz o título e o botão da barra de tarefas piscarem e pede que você use Alt+Tab para mudar para o instalador. Ative o instalador antes de usar seus comandos de teclado ou controle. Alt+G coloca o foco no campo da pasta do jogo.

A caixa Show advanced está desmarcada quando o instalador é aberto. Ao marcá-la, aparecem Install alpha, Save diagnostics e Copy diagnostics. Install baixa a versão pública mais recente do GitHub quando existe uma. Ainda não há uma versão pública, portanto quem faz os testes precisa atualmente de Show advanced e Install alpha. A opção alfa pede confirmação, baixa o código-fonte mais recente e o compila no seu computador. Update aparece quando é encontrada uma versão pública mais recente para uma cópia instalada.

As mensagens de status explicam em linguagem simples o que está sendo baixado, instalado ou concluído. Uma única barra mostra o progresso estimado de toda a instalação, sem reiniciar a cada download ou arquivo. Ela avança em incrementos de cinco pontos percentuais; algumas etapas de preparação podem levar algum tempo sem uma mudança visível. A mensagem de boas-vindas, o aviso de uma nova atualização disponível e a confirmação de que os dados de diagnóstico foram copiados são enviados ao leitor de tela pelas notificações de acessibilidade do Windows. A leitura em voz alta depende do seu leitor de tela e do suporte dele às notificações do Windows.

O instalador 0.2.2 nunca inicia o Bop It! durante a instalação. A opção alfa reutiliza arquivos locais de compilação compatíveis ou prepara arquivos temporários a partir da sua própria cópia instalada do jogo enquanto ele permanece fechado. Em seguida, o instalador coloca o MelonLoader na pasta do jogo e adiciona imediatamente Mods/BopItAccess.dll, seguido de Prism, configurações, toda a documentação e os componentes necessários para a desinstalação. Aguarde a mensagem de sucesso e, quando estiver pronto, inicie o jogo por conta própria pelo Steam.

Após uma instalação bem-sucedida, Play Bop It! The Video Game aparece. Ative esse botão para iniciar o jogo por conta própria pelo Steam quando estiver pronto. O instalador nunca inicia o jogo automaticamente durante a instalação.

Uma versão compilada precisa do ambiente de execução do .NET 6 para Windows x64, não de um SDK de desenvolvimento. Ambientes de execução completos já existentes são reutilizados. Se o ambiente de execução estiver ausente, ele será baixado da Microsoft e colocado em MelonLoader/Dependencies/dotnet. Install alpha também precisa de um SDK do .NET compatível e do pacote de direcionamento do .NET 6: um SDK existente é reutilizado ou o SDK oficial da Microsoft é instalado para todo o sistema. O instalador não cria uma nova pasta de SDK na raiz do jogo. MelonLoader 0.7.3 Open-Beta e Prism 0.18.3 vêm de suas versões oficiais. Os componentes compartilhados do Microsoft .NET e os SDKs permanecem instalados após o cancelamento ou a desinstalação.

Quit fecha o instalador. Se a instalação ainda estiver em andamento, ele pergunta se você quer cancelá-la e desfazê-la antes de fechar; Keep open permite continuar normalmente. Se a instalação terminar enquanto você decide, a caixa de diálogo é atualizada para informar que ela terminou, e Quit não desfaz a instalação concluída. Depois que a remoção começa, a desinstalação termina com segurança antes de sair. Abort também pede confirmação e reverte as alterações feitas nos arquivos do jogo durante esta tentativa. O cancelamento durante a instalação do Microsoft .NET aguarda a conclusão segura da instalação desse componente compartilhado.

Após confirmar Uninstall, escolha Uninstall for me ou Uninstall for everyone. Ambas as opções removem os arquivos compartilhados do mod desta pasta do jogo, portanto o mod deixará de estar disponível para qualquer pessoa que use essa instalação. A escolha determina de quem serão removidas as preferências salvas do mod no Windows: apenas da conta que solicitou a operação ou de todos os perfis locais do Windows, incluindo os perfis sem sessão iniciada. As preferências do jogo original são mantidas. O SDK do .NET permanece instalado.

Quando o instalador remove sua própria instalação do MelonLoader e nenhum outro mod precisa dela, ele também remove os arquivos conhecidos Loader.cfg e MelonPreferences.cfg e as pastas vazias Plugins, UserLibs e UserData. As configurações do Bop It Access, os registros conhecidos, os guias e os arquivos do instalador são removidos. Outros mods, arquivos compartilhados do carregador que já existiam e arquivos não reconhecidos são protegidos. Isso também significa que um arquivo desconhecido pode fazer uma pasta permanecer; o instalador informa isso nos dados de diagnóstico em vez de excluir dados sem relação com o mod.

O instalador permanece aberto após a desinstalação para que você possa revisar o resultado, salvar os dados de diagnóstico ou instalar novamente. Escolha Quit quando terminar. O programa auxiliar de desinstalação em execução e os arquivos automáticos de diagnóstico são removidos depois que a janela é fechada. Uma reinstalação na mesma janela inicia um novo registro de instalação; a limpeza adiada não pode remover a nova instalação.

Aplicativos instalados do Windows usa a mesma confirmação, escolha de preferências e limpeza. O instalador fornece BopItAccess-uninstall.ps1 na pasta do jogo como um atalho para o desinstalador instalado; futuras compilações do código-fonte também incluem esse script nos arquivos de saída. Um script copiado manualmente não instala o próprio desinstalador. Para uma instalação manual antiga sem um registro de propriedade, o instalador remove os arquivos do mod que consegue identificar e mantém os arquivos compartilhados cuja origem não pode ser determinada.

Se a limpeza não puder terminar com segurança, o instalador explica isso e mantém as informações necessárias para tentar novamente. Para uma instalação gerenciada, a entrada de desinstalação no Windows e o ponto de verificação da limpeza permanecem até que a remoção seja concluída com sucesso. Uma cópia manual antiga não possui um registro persistente de propriedade; tente novamente as operações indicadas nos avisos com o instalador aberto. Não instale, atualize nem remova o mod enquanto o Bop It! estiver em execução.

### Atalhos de teclado do instalador

| Ação | Atalho de teclado | Função |
| --- | --- | --- |
| Pasta do jogo | Alt+G | Colocar o foco no campo da pasta do jogo. |
| Browse | Alt+B | Escolher a pasta do jogo. |
| Install | Alt+I | Instalar a versão pública mais recente, quando disponível. |
| Install alpha | Alt+A | Confirmar e compilar o código-fonte mais recente; visível com Show advanced. |
| Update | Alt+U | Instalar uma versão pública mais recente quando oferecida. |
| Play Bop It! The Video Game | Alt+P | Iniciar o jogo pelo Steam; disponível após uma instalação bem-sucedida. |
| Uninstall | Alt+N | Confirmar a remoção e escolher de quem remover as preferências do mod no Windows. |
| Abort | Alt+R | Confirmar o cancelamento da instalação atual. |
| Registro de status | Alt+L | Colocar o foco nas mensagens de status somente leitura cujo texto pode ser selecionado. |
| Show advanced | Alt+V | Mostrar ou ocultar a instalação alfa e as ferramentas de diagnóstico. |
| Save diagnostics | Alt+D | Salvar e continuar registrando toda a sessão de diagnóstico; visível com Show advanced. |
| Copy diagnostics | Alt+C | Copiar a captura completa dos dados de diagnóstico; visível com Show advanced. |
| Quit | Alt+Q | Fechar, tratando o cancelamento com segurança se houver uma operação em andamento. |

### Como usar um controle no instalador

O instalador oferece suporte a controles do tipo Xbox e a outros controles que o Windows disponibiliza pelo XInput. Seus comandos são separados dos comandos remapeáveis do jogo. O direcional ou a alavanca analógica esquerda move o foco entre os controles da interface; quando um campo de texto está em foco, as direções passam a revisar o texto. Os botões superiores sempre movem o foco para o controle anterior ou seguinte que pode recebê-lo. A ativa o botão ou a caixa de seleção em foco. As entradas do controle só são processadas enquanto este instalador ou uma de suas próprias caixas de diálogo estiver em primeiro plano.

B volta ou cancela uma caixa de diálogo; na janela principal do instalador, ele pede o cancelamento de uma instalação em andamento ou, nos demais casos, executa a ação de Quit. Start executa a ação de Quit na janela principal e volta em uma caixa de diálogo. Y (o botão superior da parte frontal) seleciona todo o texto quando um campo de texto do instalador está em foco. Fora dos campos de texto da janela principal, Y ativa ou desativa Show advanced. No registro de status ou em outro campo de texto do instalador, o direcional ou a alavanca esquerda funciona como as setas: Esquerda/Direita move por caractere e Cima/Baixo por linha. Segure LT como Ctrl: Esquerda/Direita move por palavra e Cima/Baixo por parágrafo. Segure RT como Shift para ampliar a seleção; segure LT e RT juntos para selecionar palavras ou parágrafos. X copia somente o texto selecionado; selecione primeiro a parte desejada. Ctrl+C no teclado continua copiando a seleção. Quando não há texto selecionado, o instalador também envia notificações acessíveis para o caractere, palavra, linha ou parágrafo na posição do cursor. O instalador envia uma confirmação acessível quando o texto é copiado e informa quando não há seleção ou ocorre falha na cópia. A leitura em voz alta depende do suporte do leitor de tela às notificações do Windows. A navegação com controle nas caixas de diálogo nativas do Windows para escolher pastas e salvar arquivos ainda precisa de verificação humana. O teclado continua disponível para digitar uma pasta ou um nome de arquivo. Esta implementação não abrange controles sem suporte a XInput.

A mensagem de boas-vindas no registro de status lista os atalhos do controle para revisar texto; use Alt+L para voltar ao registro. Alterar Show advanced envia uma notificação de acessibilidade do Windows informando se está marcado ou desmarcado. Selecionar todo o texto também fornece confirmação acessível ou informa que o campo está vazio.

### Diagnóstico do instalador

Show advanced exibe Save diagnostics (Alt+D) e Copy diagnostics (Alt+C). Os registros automáticos em UTF-8 são mantidos localmente em %ProgramData%\BopItAccess\diagnostics. Save diagnostics grava toda a sessão atual no arquivo .log ou .txt escolhido e continua registrando até o instalador fechar; Copy diagnostics copia uma captura dos dados e fornece uma confirmação acessível. Salve antes de uma tentativa de instalação ou desinstalação para que seu registro permaneça após a limpeza dos registros automáticos. Os detalhes técnicos de arquivos, downloads, compilador e erros são mantidos aqui, embora o campo de status use mensagens mais curtas. Nada é enviado. Os registros podem conter nomes de usuário do Windows e caminhos completos: revise-os antes de compartilhar. As cópias exportadas deliberadamente permanecem após a desinstalação.

Na primeira inicialização manual após instalar o MelonLoader, ele pode baixar arquivos de suporte e preparar os assemblies do jogo. Aguarde cerca de um minuto, ou mais em alguns sistemas. O mod não pode falar até que o MelonLoader o carregue. Mantenha o jogo aberto e aguarde o anúncio de inicialização do Bop It Access e, em seguida, o anúncio da tela de título, de boas-vindas ou do menu principal antes de usar os comandos do jogo.
