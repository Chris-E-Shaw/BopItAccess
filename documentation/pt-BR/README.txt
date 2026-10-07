Bop It Access 0.9.11 - Prism Fala e Braille

O que isso faz
--------------
O mod segue as Configurações do jogo> Seleção de idioma para fala. Inclui
Inglês, francês, italiano, alemão, espanhol (Espanha), espanhol (América Latina),
Japonês, Coreano, Chinês Simplificado e Português Brasileiro. Mudando o
a linguagem do jogo também altera os anúncios de mod e o guia do usuário do jogo.
As traduções iniciais são rascunhos gerados por máquina e precisam de revisão
por falantes fluentes.
A fala está ativada por padrão. Quando o mod carrega com a fala ativada, ele anuncia
"Bop It Access discurso está pronto. O jogo ainda está carregando. Aguarde a tela de título ou o anúncio do menu principal antes de usar os controles." através de Prism. Se a tela de título aparecer, o mod anuncia a entrada atual BATER para abrir o menu principal. Ele lê
o botão do menu principal em foco e a linha Configurações em foco. Os valores das configurações são
falado com o nome da linha em foco. Alterar um valor enquanto o foco permanece nele
row fala apenas o novo valor. LATÊNCIA DE ÁUDIO, CONTROLES e GO ONLINE são ação
botões, para que sejam falados sem o espaço reservado sem sentido do jogo "0".
O menu Configurações também possui um controle deslizante LIMIT FPS com 30, 60, 120, 240 e
Escolhas ILIMITADAS. Começa aos 60 para uma nova instalação e lembra o
valor selecionado entre sessões. O foco fala o nome e o valor; mudando isso
fala apenas o novo valor. O limite altera a taxa de quadros alvo de Unity enquanto
deixando a escala de tempo do jogo, o tempo de atualização fixo e o áudio intactos.
Como qualquer limite de quadro, uma configuração mais baixa também significa menos pesquisas de entrada baseadas em quadros.
Se 30 FPS parecer menos responsivo em um jogo rápido, escolha 60, 120 ou ILIMITADO.
A alternância MUTE AUDIO IN BACKGROUND aparece diretamente abaixo de VOICE OVER em
Configurações. Quando ativado, silencia o áudio do jogo enquanto a janela do jogo não está
focado e, em seguida, restaura o estado de áudio do jogo anterior quando o foco retorna.
Ele começa desligado e é salvo entre as sessões.
No primeiro uso do mod, a MÚSICA, SFX e VOICE OVER nativos do jogo
os controles deslizantes começam em 30. A atualização mantém as configurações de áudio do jogo salvas anteriormente.
Assim que o jogo chega ao menu principal pela primeira vez, uma tela de boas-vindas aparece
foco. Sua mensagem pode ser focada novamente com Up, e suas escolhas abrem Mod
Configurações, abra o guia do usuário dentro do jogo ou vá para o menu principal.
A tela de boas-vindas é marcada como concluída somente depois que uma escolha sai com sucesso
isso. Fechar o jogo enquanto ele está aberto deixa-o pronto para o próximo lançamento.
A indexação de configurações agora aguarda as linhas de áudio, FPS e MOD SETTINGS do mod antes
anunciando o primeiro item em foco em uma tela de configurações recém-aberta.

Abaixo de Controles, Configurações agora tem um menu MOD SETTINGS. SPEECH OUTPUT usa o mesmo
chave mestre salva como F8 ou seleção do controlador, incluindo a recuperação falada
instruções quando a fala está desligada. BRAILLE OUTPUT começa ligado e é salvo
entre as sessões. Prism envia anúncios para um leitor de tela compatível
saída braille quando esta configuração está ativada. Desligá-lo interrompe o braille do mod
mensagens enquanto deixa a fala disponível.
OUTPUT MODE: Auto usa um leitor de tela compatível em execução, depois OneCore e, por último, SAPI.
Auto, OneCore, SAPI, NVDA, JAWS, UI Automation, ZDSR, ZoomText,
Boy PC Reader, PC Talker, Sense Reader, System Access, Window-Eyes.
Cada leitor de tela e mecanismo de voz só está disponível se for compatível com a versão do Prism instalada e com o sistema do jogador. Se o modo escolhido não estiver disponível, o mod muda para uma saída disponível e avisa por voz uma única vez.
As vozes SAPI salvas são localizadas pelo nome exibido no Prism; se várias tiverem o mesmo nome, a primeira poderá ser escolhida.
MUTE SPEECH IN BACKGROUND é um botão de alternância salvo, desativado por padrão. Quando ativado,
o mod para de falar assim que o jogo perde o foco da janela. Discurso criado
enquanto o jogo está em segundo plano é descartado e os anúncios são retomados
com nova atividade após o retorno do foco. Se a própria fala estiver desligada durante o jogo
recupera o foco, o mod dá recuperação ao teclado e controlador atuais
instruções uma vez.

O menu MOD SETTINGS também possui Ler posições nos menus, ativado por padrão e salvo entre sessões.
Quando ativado, um item de menu em foco inclui sua posição, como "PLAY, 1 of 6".
Isso se aplica aos menus principal e de configurações, controles, modos de jogo, Mod
Configurações, opções de fim de jogo, tabelas de classificação, conquistas, créditos e outros
telas suportadas. A contagem segue as escolhas atualmente disponíveis. Mudando
um controle deslizante ou alternância enquanto permanece em foco ainda anuncia apenas o novo valor.

FORMATAR FALA é uma opção salva nas configurações do mod, ativada por padrão. Usa maiúsculas e minúsculas naturais nos nomes de menu escritos apenas em maiúsculas, preservando palavras com maiúsculas e minúsculas e abreviações como SAPI, NVDA, SFX e FPS. No guia dentro do jogo, quando o número da linha é anunciado e o texto termina sem pontuação, acrescenta três pontos para uma pausa antes do número. A pontuação existente é preservada. Só muda o texto enviado à fala e ao braille; o texto visível do jogo e o guia ficam intactos. Desativar a opção desativa os dois ajustes. Sua preferência anterior de filtro de maiúsculas é mantida.

ANUNCIAR TIPOS DE CONTROLE é outro botão de alternância de MOD SETTINGS salvo, ativado por padrão. Quando ativado,
o tipo do item em foco segue seu nome e precede seu valor e índice:
"Controle deslizante de MÚSICA, 30, 1 de 12", "Alternar VIBRAÇÃO, Ligado, 6 de 12" ou
"Botão PLAY, 1 de 6". Os menus também identificam guias, campos de texto e campos legíveis.
liste os itens quando relevante. As alterações de valor continuam a falar apenas do novo valor.
SLIDER RANGES é uma alternância salva, desativada por padrão. Quando ativado, controles deslizantes focados
também reportam seus endpoints disponíveis após o valor atual, como
"MUSIC slider, 30, range 0 to 100, 1 of 12" ao indexar e controlar tipos
estão habilitados. Mover um controle deslizante ainda fala apenas o novo valor.
O feedback individual é salvo e fica ativado por padrão. Anuncia a cor ativa no início e quando ela muda. Ao perder uma vida, anuncia a quantidade restante. Ao ganhar uma vida, anuncia a cor do jogador e o novo total, como “Verde, 3 vidas”, para que ambos saibam quem foi mais rápido. Esses anúncios são desativados quando o feedback individual está desligado. O limite de três vidas do jogo não muda. Esse recurso atua apenas durante uma partida individual entre dois jogadores.

O TIPO DE DICA agora aparece acima de DICAS DO BOTÃO AUTO-SPEAK no menu Mod Settings.
DICAS DO BOTÃO AUTO-SPEAK é uma alternância salva e está ativada por padrão. Girando
Off suprime dicas automáticas, enquanto SPEAK HINTS permanece disponível sob demanda.
BOTÃO DICAS DELAY tem Nenhum, 5 segundos
(Pode interromper a fala), 10 segundos, 15 segundos, 30 segundos e 60 segundos.
O padrão é 10 segundos. Com None, as entradas válidas para a tela atual e
suas ações estão incluídas na sequência de fala comum do item em foco,
depois de um ponto final. A ação para o controle focado é falada antes do
navegação nos menus. Não há anúncio de primeira dica separado. Com um tempo cronometrado
atraso, o anúncio da primeira dica segue tanta inatividade. A opção de 5 segundos pode
interromper a fala já em andamento; atrasos mais longos ficam na fila atrás dele. Depois
o anúncio da primeira dica, outro atraso começa apenas quando o jogador dá
entrada, a menos que as repetições estejam habilitadas. Mover para outro item em foco ou alterar
um controle deslizante ou alternância focado conta como entrada e reinicia o atraso, mesmo que o
a verificação de ligação de entrada do jogo perde a ação da tecla ou do controlador. Uma chave não utilizada
isso não altera a IU ainda não a reinicia.
DICAS TYPE é um controle deslizante salvo com Automático, Teclado, Controlador e Ambos.
Automático é o padrão e segue o teclado ou teclado usado mais recentemente.
entrada do controlador. O uso do mouse conta como teclado. Teclado e controlador falam
apenas as dicas desse dispositivo; Ambos fornecem ambos os conjuntos de entradas com dispositivo explícito
nomes. A recuperação de fala desligada sempre inclui ambos os dispositivos para que o
o jogador pode encontrar o controle que reativa a fala.

As dicas do botão colocam a entrada antes de sua ação: "Entre ou Space, ative o item."
As dicas de dispositivo único omitem o nome do dispositivo. Os nomes dos sticks do controlador são falados
na íntegra, como "controle esquerdo para cima e para baixo". Ambos os modos identificam o teclado
e entradas do controlador. O mod lê as ligações atuais do jogo de forma nativa
as alterações de religação são refletidas nessas dicas. Quando nenhum controlador estiver
conectado e o jogo tem nomes de botões diferentes em controladores diferentes
tipos, a dica usa "botão confirmar" ou "botão voltar" em vez de assumir um
Layout do Xbox. As linhas de pontuação do placar usam Page Up e Page Down no teclado.
O controlador para cima/para baixo lê as linhas somente quando nenhum controle do placar está em foco;
dicas relatam apenas os controles disponíveis para o TIPO DE DICAS selecionado. Comum
as dicas de tela também incluem as ligações atuais SPEAK HINTS e TOGGLE SPEECH.
Ambos são analisados centralmente, para que futuros controles globais possam se unir ao mesmo
lista de dicas sem alterar cada tela separadamente.

REPEAT BUTTON DICAS é um controle deslizante salvo separado: Desligado, 2x, 3x, 4x, 5x ou
Infinitamente. O padrão é Infinitamente. O número é o total de leituras em um
ciclo: 2x significa a primeira dica e uma repetição; 3x significa a primeira dica e
duas repetições. Off ainda permite a primeira dica automática ou manual.
REPEAT INTERVAL define o atraso entre as repetições para 15, 30, 45 ou
60 segundos e o padrão é 30 segundos. Com BUTTON HINTS DELAY definido como Nenhum,
o temporizador de repetição começa imediatamente após a entrada. Entrada, um foco ou valor
alteração ou uma alteração de tela reinicia o ciclo de dicas para a tela atual.
FALAR DICAS substitui o
dica automática pendente para esse ciclo e, em seguida, usa REPEAT INTERVAL para qualquer
repetições configuradas. Isso também funciona com DICAS DO BOTÃO AUTO-SPEAK desativadas.
As dicas são suprimidas durante
jogabilidade ativa e as fases de tempo de batida da calibração de áudio, onde extras
a fala pode mascarar uma deixa. Um lembrete existente salvo de 15, 30 ou 60 segundos
atraso da versão 0.6.2 torna-se o novo valor BUTTON HINTS DELAY.

MOD SETTINGS: Voz, Volume, Velocidade e Tom ajustam a saída OneCore ou SAPI realmente em uso, mesmo no modo Auto. Só aparecem os controles compatíveis; com outras saídas, eles são ocultados. Cada mecanismo guarda suas configurações separadamente. Volume: de 5% a 100% em passos de 5, inicialmente 100%. Velocidade e tom: de 0 a 100 em passos de 5, inicialmente 50. O volume mínimo mantém audíveis os avisos de recuperação.
As opções de configurações do mod são lembradas entre as sessões. RESTAURAR PADRÕES DO MOD
retorna essas opções aos padrões descritos acima. Pressione uma vez para solicitar
confirmação e pressione-o novamente dentro de cinco segundos para restaurá-los. Movendo-se
para outra linha ou deixar passar cinco segundos cancela a solicitação. Isso não
altere os controles deslizantes de MÚSICA, SFX ou VOICE OVER do jogo, LIMIT FPS ou personalizado
ligações de teclado e controlador. Voltar retorna para Configurações.
OPEN USER'S GUIDE lê o guia HTML para o idioma atualmente selecionado em
a linha Configurações> Idioma do jogo. O guia em inglês está em
documentation\BopItAccess-user-guide.html; guias traduzidos estão no idioma
subpastas. Se uma cópia traduzida estiver faltando ou ilegível, o guia em inglês
abre em vez disso. Sua lista de tópicos vem do índice do documento e
recarrega sempre que aberto. Confirmar abre um tópico. Acima e Abaixo leem suas falas. Nas tabelas,
A esquerda move uma coluna para a esquerda e a direita move uma coluna para a direita; Para cima e para baixo, mantenha
a coluna atual ao mover entre linhas. Células de rótulo de cabeçalhos de coluna
em vez de aparecer como linhas de dados. A tabela é anunciada uma vez na entrada e
seu fim é anunciado na saída. Voltar retorna aos tópicos ou sai do guia.
Durante a leitura, o mod aplica o parâmetro Filtro de música do menu do jogo e
restaura seu valor anterior na saída. RESET WELCOME SCREEN pede um
pressione pela segunda vez em cinco segundos e, em seguida, faz com que a tela de boas-vindas apareça no
próximo lançamento do jogo. Alterar linhas ou esperar cinco segundos cancela a confirmação.
Esta atualização restaura o layout da linha de configurações nativa do jogo para cima/para baixo
a navegação permanece nas linhas Configurações após a adição de MOD SETTINGS.
Ele também inicia o submenu MOD SETTINGS em SPEECH OUTPUT cada vez que é aberto,
impedindo que uma linha BACK selecionada anteriormente feche o menu imediatamente
quando Enter é usado para reabri-lo.
A entrada que abre MOD SETTINGS agora é ignorada por suas linhas até que essa entrada seja
liberado, portanto, reabrir o menu também não pode desativar a fala. O mod
linhas de ligação de controles adicionados também aguardam que a entrada de abertura seja
liberado antes de aceitar uma solicitação de religação.

Dentro do Play, o mod diz Solo, Party, Pass It e One on One quando focado.
Na tela seguinte de seleção de música, o mod anuncia o tema atual
(Shapes, Space, City ou Office) e a dificuldade: Clássico ou Extremo. GIRAR
altera a música e anuncia apenas o novo tema. PUXAR altera a dificuldade
e anuncia apenas Clássico ou Extremo. A introdução também explica GIRAR,
PUXAR, BATER e Voltar. Ela se repete sempre que um modo é escolhido
e a tela de música é aberta, com o tema e a dificuldade
atuais.
Pressione SPEAK HINTS (H ou pressione o botão direito por padrão) nesta tela para ouvir
o modo atual e o texto do tutorial nativo da dificuldade antes de começar. Cada
a ação nomeada nessa referência inclui o teclado ou teclado atualmente atribuído
controle do controlador, seguindo HINTS TYPE. Os controles reatribuídos são lidos
as ligações do jogador ativo; One on One nomeia as entradas BATER de ambos os jogadores. O
a sobreposição de tutorial cronometrada durante o jogo ativo permanece silenciosa para não ser obscurecida
os comandos falados do jogo. A dica anuncia esse uso extra de SPEAK HINTS.
O controle READ DESCRIPTIONS fala uma descrição visual do selecionado
Shapes, Space, City ou Office estágio sob demanda. Está disponível nesta tela
apenas, antes do início do jogo. Suas entradas padrão são G no teclado e LT
(gatilho esquerdo) no controlador. R foi substituído porque é o Reset do jogo
Atalho do giroscópio. A introdução da tela anuncia a ligação atual.
Iniciar a reprodução interrompe qualquer fala restante da seleção de música, de modo que não possa mascarar o
dicas verbais do jogo. As descrições começam com os detalhes da cena, em vez de
repetindo o nome artístico selecionado.

Na tela do resultado final, Solo, Party e Pass It anunciam a pontuação final
antes do discurso do menu. Solo lê os botões focados de Replay e Leaderboard
e explica Voltar. Os outros modos leem seus disponíveis Continue, Replay e
Solicitações de volta. One on One mostra um vencedor em vez de uma pontuação final numérica, então
o mod anuncia o vencedor mostrado lá. O discurso de pontuação tem prioridade sobre
o anúncio inicial do menu; os prompts da tela de resultados são enfileirados depois dele.
As alterações subsequentes do foco do menu interrompem-se mutuamente. Mudanças rápidas imediatamente
após o fim do jogo são combinados até que o anúncio do placar curto tenha tempo
para finalizar, mantendo a última opção de menu focada.
Anúncios de pontuação alta individual e classificação de grupo são falados quando o jogo relata
um novo resultado na tabela de classificação.
READ SCORE repete o resultado final sob demanda apenas enquanto o resultado do fim do jogo
a tela fica visível. As entradas padrão são T no teclado e pressione o controle esquerdo
controlador. Solo, Party e Pass It repetem sua pontuação final; Um contra um
repete o vencedor mostrado pelo jogo. A ação é desativada durante o jogo,
seleção de músicas e todas as outras telas. O RT (gatilho direito) não foi utilizado como
padrão porque o jogo já o vincula a Reset Gyro e Auto Play.
As repetições solicitadas falam imediatamente e podem ser interrompidas pelo menu de resultados
navegação. Apenas o anúncio automático do resultado atrasa o menu inicial
discurso para que a partitura seja ouvida primeiro.
O FEEDBACK INDIVIDUAL está ativado por padrão em Configurações> Configurações de mod para ativo falado
cor e vidas restantes durante esse modo. As dicas BATER compartilhadas não são feitas por
eles próprios identificam uma cor, então o mod mantém a última cor definida.

TOGGLE SPEECH ativa ou desativa toda a fala mod comum em qualquer tela. É
os padrões são F8 no teclado e Select no controlador. Quando desligado, o mod
interrompe a fala atual e anuncia que a fala está desativada, junto com a fala atual
controles de teclado e controlador para ligá-lo novamente. O estado desligado é
salvo entre sessões de jogo. Se o jogo começar com a fala desligada, o mod dá
essas instruções de recuperação em vez de sua mensagem de carregamento normal. Virando
o discurso de volta anuncia "Discurso ativado". Outro discurso mod permanece em silêncio enquanto está desligado.
O controle Alternar fala permanece ativo mesmo quando a fala está desligada.

As tabelas de classificação alcançadas no menu principal, resultados Solo e resultados do Grupo
anuncie a música, dispositivo, grupo e data selecionados, quando disponível. Eles lêem
classificação, nome do jogador e pontuação, incluindo estados de carregamento e resultado vazio. Página para cima
e Page Down leem linhas de pontuação individuais mesmo quando um filtro está em foco. Nativo
controles focados, como Local, Amigos, Global, Hoje, Este Mês, Todo o Tempo,
Voltar e Continuar são falados. A tabela de classificação do Partido também informa seu nome
estado de seleção e confirmação.

Esta atualização mantém as tabelas de classificação de resultados silenciosas durante a seleção do modo e da música.
Os nomes dos filtros do placar falam primeiro, com os resumos de pontuação na fila depois deles.
O botão do menu de conquistas fala normalmente; instruções do livro espere até
suas páginas são realmente abertas e o fechamento só é anunciado depois disso.

O livro de conquistas do jogo anuncia sua página visível e o valor de cada conquista
nome, descrição e estado bloqueado ou desbloqueado. Use para cima e para baixo para ler itens
em uma página. Os controles Esquerdo e Direito do jogo viram as páginas normalmente.

Os créditos anunciam a primeira linha quando abertos. Use o menu para cima e para baixo do jogo
controles para ler cada linha de crédito. A rolagem visual automática continua conforme
antes. Se esses controles não estiverem disponíveis, as linhas serão enfileiradas conforme aparecem;
se isso também não funcionar, todo o texto dos créditos será anunciado uma vez.

Dentro dos controles, o mod diz BATER, BATER (Jogador 2), PETELECO, GIRAR, RODAR, PUXAR,
e redefinir para o padrão. Ele lê a ligação atual do dispositivo de entrada ativo,
anuncia as ligações alteradas e lê o feedback visível da religação do jogo.
Anuncia como retornar às Configurações uma vez por visita. Quando Redefinir para padrão altera um
vinculação, ele relata que as vinculações foram redefinidas.

O mod expõe uma linha nativa RESET GYRO e adiciona uma CHANGE SPEECH OUTPUT
atalho. Reset Gyro fica com os controles do jogo; as linhas específicas do mod
permanecem juntos na parte inferior do menu, antes de Redefinir para o padrão. MUDAR
SPEECH OUTPUT percorre os mesmos modos de Configurações > Configurações de Mod > SAÍDA
MODO. A ordem dos modos é:
Auto, OneCore, SAPI, NVDA, JAWS, UI Automation, ZDSR, ZoomText,
Boy PC Reader, PC Talker, Sense Reader, System Access, Window-Eyes.
As entradas padrão do atalho são F9 no teclado e o botão oeste (X em um Xbox
controlador). O botão Iniciar do controlador é reservado pelo nativo do jogo
Ação do menu. A escolha atual é anunciada quando o atalho é usado e
é salvo pela mesma configuração do modo de saída.

O mod adiciona nove linhas ao menu de controles do jogo: Grupo Anterior,
Grupo Próximo, Data Anterior, Data Próxima, LER DESCRIÇÕES, LER PONTUAÇÃO,
ALTERNAR DISCURSO, FALAR DICAS e ALTERAR SAÍDA DE DISCURSO. Os primeiros quatro
abordar os filtros do placar
alcançado com O/P e K/L no layout de teclado padrão, ou os bumpers e
D-pad esquerdo/direito em um controlador. Foque uma linha para ouvir sua vinculação atual,
em seguida, use a ação normal BATER/confirm do jogo para religá-lo. As linhas rolam
dentro do painel de controles existente. LEIA AS DESCRIÇÕES também pode ser recuperada para
teclado e controlador. Sua ligação é salva pelo mod e redefinida para padrão
restaura G e LT. READ SCORE também pode ser recuperado para teclado e controlador;
Redefinir para o padrão restaura T e pressione o controle esquerdo. A encadernação original do jogo
linhas e as quatro linhas do placar usam o mesmo fluxo de religação de controles.
TOGGLE SPEECH pode ser recuperado para teclado e controlador. Suas ligações são
salvo pelo mod, e Reset to Default restaura F8 e Select. Se a vinculação
é alterado enquanto a fala está desligada, o mod anuncia os novos controles de recuperação.
SPEAK HINTS também pode ser recuperado. Seus padrões são H e pressione o botão direito.
Ele fala a dica da tela atual imediatamente, sem agendar um segundo
primeira dica automática. As repetições configuradas ainda podem seguir. Está em silêncio
durante o jogo e as dicas cronometradas de calibração de áudio.
CHANGE SPEECH OUTPUT pode ser recuperado para teclado e controlador; Redefinir para
O padrão restaura F9 e o botão da face oeste. Se uma nova ligação já estiver
atribuído a outro jogo ou ação de mod, o menu Controles rejeita o
duplicar e manter a atribuição anterior. RESET GYRO pode ser recuperado por
o mesmo procedimento de controles nativos das demais ações do jogo.
Retornar da seleção de músicas para o menu principal restaura as dicas do menu principal, mesmo
quando um gerenciador de jogo em cache ainda relata um estado de jogo antigo. Dica de botão
temporizadores e seleção automática de dispositivo de dica seguem o jogo e mod atribuídos
controles; teclas não utilizadas, como uma tecla de controle não atribuída, não as reinicializam.
Esta atualização impede que a linha de controles obsoleta anuncie "Falha na religação"
repetidamente após sua cena fechar. Leia as descrições não mais temporariamente
substitui qualquer uma das ligações de entrada do próprio jogo.

Dentro da calibração de áudio, o mod lê os controles Calibrar, Voltar e BATER,
anuncia as instruções e etapas de calibração, lê a contagem regressiva de aquecimento,
e anuncia o resultado de latência exibido. Ele não fala todas as batidas durante
o exercício de cronometragem para que a batida permaneça audível. Após a última entrada de calibração, o mod diz imediatamente “Concluído!”. Pare de bater e espere o resultado medido. Se a calibração falhar porque nenhuma entrada foi feita, ele diz “Falha na calibração.”.

A tela de pausa anuncia Pausado, o botão Retomar ou Menu principal selecionado e suas dicas de controles. Mudar a seleção interrompe a fala anterior do menu de pausa. Retomar ou sair da rodada interrompe as falas de pausa restantes antes de continuar no jogo ou no menu principal. Durante uma rodada, o mod deixa intactos os comandos de voz do jogo e a pontuação em andamento.

Instalar
-------
O repositório de origem não contém mod compilado ou DLL Prism. Construa o mod por
seguindo README.md, feche o jogo e copie BopItAccess.dll em seus Mods
pasta. Obtenha o oficial Windows x64 Prism v0.18.3 prism.dll em
https://github.com/ethindp/prism/releases e coloque-o ao lado do executável do jogo,
não dentro de Mods. Copie a pasta de documentação da compilação para a pasta do jogo,
incluindo suas subpastas de idiomas traduzidos. Inicie seu leitor de tela se você
use um e inicie Bop It! a Steam. Prism pode usar SAPI quando um compatível
o leitor de tela não está funcionando. O guia do jogo carrega o HTML do
pasta de documentação sempre que ela for aberta. Este mod foi desenvolvido para MelonLoader
0.7.3 Open-Beta e Bop It! (Unity 2022.3.50f1, x64).
As primeiras traduções para outro idioma não-inglês foram feitas com tradução automática
e precisa de revisão por falantes fluentes. Por favor, relate palavras pouco claras ou incorretas.
Os nomes das ações de jogo usam os termos traduzidos do jogo. Shapes, Space, City,
e Office permanecem em inglês como títulos de estágio fixo. Se a voz do sistema SAPI não
não pronuncia bem o seu idioma, selecione uma voz instalada adequada no Mod
Configurações.

Experimente os menus e telas
-------------------------
Aguarde o anúncio da tela de título, se ele aparecer, e use BATER para abrir o
menu principal. O jogo pode levar vários segundos após a mensagem de prontidão do mod para
aceite esta entrada.
Na primeira execução, a tela de boas-vindas aparece antes do menu principal. Selecione seu
mensagem para ouvir a introdução novamente. Escolha Abrir configurações do mod, leia as do usuário
Guia ou Continue para o jogo. Speak Hints nomeia seu teclado atual e
atribuições de controlador na mensagem de boas-vindas, independentemente do tipo de dicas.
Abra o Play e navegue entre os quatro modos. Escolha um para acessar a seleção de músicas.
GIRAR para percorrer os temas e PUXAR para escolher Clássico ou Extremo. O
mod anuncia cada mudança. Pressione G ou LT para ouvir o estágio atualmente selecionado
descrição. Pressione H ou pressione o botão direito para ouvir o texto do tutorial do modo,
controles de ação atribuídos atualmente e dicas de botões. BATER inicia o modo escolhido;
De volta retorna. Durante uma rodada, use
o controle Menu do jogo para abrir Pausa e, em seguida, mover-se entre Retomar e Menu Principal.
No final de um jogo, ouça o placar ou o vencedor do One on One antes do
os controles da tela de resultados são anunciados. Pressione T ou pressione o controle esquerdo para repetir o
resultado final enquanto a tela de fim de jogo estiver visível.
Pressione F8 ou selecione o controlador para ativar ou desativar a fala mod em qualquer tela.
Pressione F9 ou controle Oeste (X em um controle Xbox) para alternar a fala
modo de saída. A mesma escolha está disponível em Configurações > Configurações de Mod > MODO DE SAÍDA.
A SAÍDA BRAILLE em Configurações > Configurações do Mod está ativada por padrão. Visualizador Braille do NVDA
pode exibir o braille e seu texto equivalente sem uma exibição física.
Para uma comparação ON/OFF, use o modo braille de cursores seguintes do NVDA com Mostrar
Mensagens habilitadas; seu modo display-speech-output espelharia a fala mesmo
quando a configuração BRAILLE OUTPUT do mod está desligada.
Em Configurações> Configurações do Mod, use DICAS DO BOTÃO AUTO-SPEAK para ativar ou desativar
instruções automáticas. Pressione H ou pressione o controle direito para ouvir a dica atual
sob demanda.
TIPO DE DICAS escolhe Automático, Teclado, Controlador ou Ambos para essas dicas.
BUTTON DICAS DELAY escolhe se acompanham o discurso de foco ou seguem um
período de inatividade. REPEAT BUTTON DICAS e REPEAT INTERVAL controlam qualquer
lembretes adicionais.
Abra Placares no menu principal ou na tela de resultados. Alterar um filtro para
ouça sua nova seleção e leia partituras individuais com Page Up e Page Down.
Por padrão, o O/P se move entre os grupos do placar e o K/L se move entre as datas
intervalos. Suas entradas de controlador correspondentes são bumper esquerdo/direito e
D-pad esquerda/direita. As quatro novas linhas de Controles destinam-se a reatribuí-los.
Abra Conquistas e vire as páginas com Esquerda e Direita; use para cima e para baixo para cada
entrada. Abra Créditos e use Up e Down para ler suas linhas independentemente do
rolagem visual.

Se faltar fala, verifique Mods\BopItAccess.log na pasta do jogo. Ele registra
detecção de painel, objetos de UI selecionados e inicialização e envio de Prism.
O envio bem-sucedido não prova, por si só, que a fala foi audível.

Para desabilitar o mod, remova Mods\BopItAccess.dll. MelonLoader pode permanecer instalado.

Arquivos e avisos de terceiros
-----------------------------
Prism é uma biblioteca de acessibilidade de código aberto de Ethan Dupuy e colaboradores.
Está licenciado sob a Licença Pública Mozilla, versão 2.0. Esta fonte
repositório não inclui prism.dll. Fonte, versões e licença:
https://github.com/ethindp/prism
Consulte THIRD-PARTY-NOTICES.txt para obter os avisos de dependência atuais.

Editar o arquivo de configurações
---------------------------------
Se um idioma desconhecido, o áudio muito alto ou uma voz problemática dificultarem o uso dos menus, você pode mudar as configurações fora do jogo. Depois da inicialização, o mod cria automaticamente UserData/BopItAccess.ini na pasta de Bop It!, usando suas configurações atuais. É um arquivo de texto que pode ser aberto em um editor como o Bloco de Notas.

O arquivo inclui idioma, volumes de música, efeitos e voz, vibração, tela cheia, resolução e latência de áudio do jogo; preferências de fala, braille, dicas e outras opções do mod; perfis de voz separados para OneCore e SAPI; e atribuições dos controles do jogo e do mod destinados aos jogadores. As resoluções disponíveis e as vozes instaladas aparecem nos comentários.

Feche o jogo antes de editar. Encontre a seção adequada e mude o valor da entrada existente, salve o arquivo e inicie o jogo novamente. As alterações são lidas na inicialização, não imediatamente durante uma sessão. Mudanças feitas nos menus atualizam o arquivo automaticamente.

Os nomes das seções e configurações permanecem em inglês em todos os idiomas. On e Off são recomendados para ligar e desligar opções; True/False, Yes/No e 1/0 também são aceitos. Os comentários explicam as opções e os intervalos. Uma entrada ausente ou inválida mantém a configuração salva correspondente; as outras alterações válidas ainda são aplicadas. Atribuições de controles duplicadas são rejeitadas.

Comentários e entradas desconhecidas são preservados. Se outro programa modificar o arquivo enquanto o jogo estiver aberto, o mod para de salvá-lo pelo restante da sessão para proteger essas alterações. Feche e reabra o jogo para aplicá-las. Você pode guardar uma cópia de segurança antes de editar.

Se uma voz apresentar problemas, defina Voice=System default na seção OneCore ou SAPI. As vozes OneCore usam nome | idioma; SAPI aceita o nome exibido de uma voz instalada ou seu identificador completo do Registro. O arquivo lista as opções disponíveis. OutputMode=Auto tenta um leitor de tela compatível em execução, depois OneCore e finalmente SAPI.

O exemplo abaixo restaura o inglês, um áudio de jogo mais baixo e a saída de fala automática com as vozes padrão do sistema. Mude as entradas correspondentes que já estão no seu arquivo; este trecho serve de referência, não é outro bloco para acrescentar ao final. Preserve suas outras configurações.

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

Desinstalar no instalador e nos Aplicativos Instalados do Windows remove os registros conhecidos do mod, incluindo Mods/BopItAccess.log.previous, UserData/BopItAccess.ini, o antigo UserData/BopItAccess.ini.tmp e restos validados de BopItAccess.ini.<GUID>.tmp em UserData. <GUID> significa exatamente 32 caracteres hexadecimais sem hífens; arquivos não relacionados são preservados. Preferências nativas do jogo, outros mods e o SDK .NET permanecem. Se a limpeza estiver incompleta, o registro de status informa isso. Para cópias gerenciadas pelo instalador, a entrada do Windows, o iniciador de desinstalação e o ponto de controle permanecem disponíveis para nova tentativa até a limpeza ter êxito. Instalações manuais antigas não têm registro durável de propriedade, portanto seus avisos podem ser tentados novamente no instalador aberto.

Nota sobre transparência de IA
--------------------

Este mod foi criado por meio de “vibe coding”. Todo o código foi totalmente gerado e pesquisado por inteligência artificial, com compreensão técnica humana limitada de sua arquitetura subjacente. Por favor, use este mod por sua própria conta e risco.

Dito isto, cada recurso de mod e decisão de design foram criados e aprovados por humanos. Os testes nunca foram automatizados; foram realizados cuidadosa e extensivamente por jogadores e testadores humanos reais.

Observação: o texto e a documentação multilíngues foram gerados pela IA e não foram revisados por falantes nativos. É esperada uma alta imprecisão na tradução. Sem codificação agente, este projeto não existiria. Obrigado por dar uma chance!

O que pode vir a seguir
------------------

Este projeto está essencialmente completo e nenhum conteúdo ou recursos importantes estão planejados. No entanto, este mod será mantido e atualizado ativamente ao longo do tempo, conforme necessário, com o feedback dos jogadores impulsionando essas melhorias. O trabalho futuro potencial inclui revisões adicionais e correções de bugs, refinamento de código e melhorias contínuas na capacidade de resposta da fala. Prism cria um caminho possível para outras plataformas no futuro, mas este mod atualmente suporta apenas Windows x64. O repositório do projeto é o local para acompanhar o desenvolvimento.

Obrigado
---------

Para aqueles que testaram este mod antes do lançamento e ajudaram a chegar onde está agora, obrigado. Vocês sabem quem vocês são. Aos jogadores que deram feedback, experimentem o mod pela primeira vez, ou acreditem em mim e neste projeto, obrigado. Seu apoio me motiva a continuar fazendo coisas em um mundo que pode parecer louco e profundamente falho. Espero que este projeto torne mais fácil para você aproveitar o jogo e jogar com outras pessoas. Muito obrigado a todos. Aproveite Bop It!

— Christopher Shaw

MelonLoader janelas de inicialização
---------------------------

O modelo Loader.cfg fornecido oculta a tela inicial e o console separados do MelonLoader. O instalador aplica
esses dois padrões antes de você iniciar o jogo por conta própria. Eles não pulam a tela de título do jogo nem
a tela de boas-vindas do mod.

Com o jogo fechado, abra UserData/Loader.cfg na pasta do jogo. Se o arquivo já existir, defina disable_start_screen como true na seção [loader] existente e hide_console como true na seção [console] existente. Mantenha todas as outras entradas. Se o arquivo não existir, copie o modelo UserData/Loader.cfg fornecido com a compilação, ou configuration/Loader.cfg a partir do código-fonte. Nunca substitua um Loader.cfg existente pelo modelo completo.

[loader]
disable_start_screen = true

[console]
hide_console = true

O mod não redefine essas opções a cada inicialização. Você pode alterar manualmente qualquer valor de volta para falso se precisar das janelas do carregador para solução de problemas. A desinstalação do instalador restaura os valores originais apenas enquanto os valores verdadeiros do instalador ainda estão presentes, preservando outras edições de configuração do carregador.

Após uma alteração bem-sucedida, o mod anuncia a entrada e a ação à qual está atribuído, por exemplo, “Space atribuído a BATER”.

Diagnósticos do instalador
--------------------------

A prévia 0.1.7 do instalador salva automaticamente registros de diagnóstico locais em %ProgramData%\BopItAccess\diagnostics. Save diagnostics (Alt+D) permite escolher um arquivo de texto, salva a sessão atual e continua acrescentando entradas até o instalador fechar; Copy diagnostics (Alt+C) copia um retrato do momento atual. Escolha Save diagnostics antes do próximo teste de instalação ou desinstalação para preservar o registro completo mesmo após a remoção dos registros automáticos. Os registros incluem mensagens de status, etapas do progresso e detalhes de erros. Nada é enviado pela internet. Confira o registro antes de compartilhá-lo: ele pode conter nomes de usuário do Windows e caminhos completos de pastas. Uma desinstalação bem-sucedida remove os registros automáticos; cópias salvas intencionalmente em outro local permanecem.

Instalação e primeira inicialização
-----------------------------------

A prévia 0.1.7 do instalador nunca inicia Bop It! durante a instalação. Install baixa uma versão pública
compilada quando ela existe. Install alpha baixa o código mais recente, pede confirmação e o compila antes de
copiar os arquivos. Alpha reutiliza referências locais completas ou gera referências temporárias a partir do
jogo instalado, sem executá-lo. Após a preparação, o instalador coloca MelonLoader na pasta do jogo e
imediatamente BopItAccess.dll em Mods. Depois termina Prism, configuração, documentação e arquivos de
desinstalação. Aguarde a mensagem de sucesso e inicie o jogo por conta própria pelo Steam quando estiver
pronto.

Uma versão compilada precisa do runtime Windows x64 do .NET 6, não de um SDK de desenvolvimento. Runtimes
completos existentes são reutilizados. Se faltar, o instalador baixa o ZIP oficial Microsoft do runtime .NET
6.0.36 e o coloca em MelonLoader/Dependencies/dotnet, um local compatível. Esses arquivos ficam registrados
para reversão e desinstalação; são mantidos se outros mods precisarem do loader compartilhado. Install alpha
também precisa de SDK compatível e pacote de direcionamento .NET 6. Reutiliza um SDK instalado ou uma pasta
dotnet antiga compatível; se necessário instala um SDK oficial Microsoft para todo o sistema. O SDK permanece
após desinstalação ou cancelamento. Esta prévia não cria nova pasta SDK dotnet na raiz do jogo. Ela obtém
MelonLoader 0.7.3 e Prism 0.18.3 oficiais quando necessário. As atualizações continuam pelo GitHub. Abort pede
confirmação e desfaz as alterações desta instalação nos arquivos do jogo.

Na primeira inicialização manual após instalar MelonLoader, ele pode baixar arquivos auxiliares e gerar os
assemblies do jogo. Aguarde cerca de um minuto, ou mais em alguns sistemas. O mod não pode falar até que
MelonLoader termine de carregá-lo. Mantenha o jogo aberto e espere o anúncio inicial do Bop It Access, depois
o anúncio da tela de título ou do menu, antes de usar os controles.
