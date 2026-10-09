Bop It Access 0.9.12 - Prism Fala e Braille

Compilações fonte atuais: mod 0.9.12 (59 compilações do mod), instalador 0.2.6. A primeira versão pública segue futura.

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

Após confirmar Uninstall, escolha Uninstall for me ou Uninstall for everyone. Ambas as opções removem os arquivos compartilhados do mod desta pasta do jogo, portanto o mod deixará de estar disponível para qualquer pessoa que use essa instalação. A escolha determina de quem serão removidas as preferências salvas do mod no Windows: apenas da conta que solicitou a operação ou de todos os perfis locais do Windows, incluindo os perfis sem sessão iniciada. As preferências do jogo original são mantidas. O SDK do .NET permanece instalado.

Quando o instalador remove sua própria instalação do MelonLoader e nenhum outro mod precisa dela, ele também remove os arquivos conhecidos Loader.cfg e MelonPreferences.cfg e as pastas vazias Plugins, UserLibs e UserData. As configurações do Bop It Access, os registros conhecidos, os guias e os arquivos do instalador são removidos. Outros mods, arquivos compartilhados do carregador que já existiam e arquivos não reconhecidos são protegidos. Isso também significa que um arquivo desconhecido pode fazer uma pasta permanecer; o instalador informa isso nos dados de diagnóstico em vez de excluir dados sem relação com o mod.

Se a limpeza não puder terminar com segurança, o instalador explica isso e mantém as informações necessárias para tentar novamente. Para uma instalação gerenciada, a entrada de desinstalação no Windows e o ponto de verificação da limpeza permanecem até que a remoção seja concluída com sucesso. Uma cópia manual antiga não possui um registro persistente de propriedade; tente novamente as operações indicadas nos avisos com o instalador aberto. Não instale, atualize nem remova o mod enquanto o Bop It! estiver em execução.

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

O mod não redefine essas opções a cada inicialização. Você pode alterar manualmente qualquer um dos dois valores de volta para false para solucionar problemas. Se a desinstalação mantiver uma instalação compartilhada do MelonLoader, ela restaura apenas os sinalizadores de destino definidos pelo instalador que não tenham sido alterados e preserva as demais edições. Se ela remover sua própria instalação do MelonLoader que não é mais usada, também remove os arquivos conhecidos Loader.cfg e MelonPreferences.cfg.

Após uma alteração bem-sucedida, o mod anuncia a entrada e a ação à qual está atribuído, por exemplo, “Space atribuído a BATER”.

Escolha o download correto

A primeira publicação pública no GitHub prevê os quatro downloads abaixo. São arquivos futuros, ainda indisponíveis; nenhuma versão pública ou tag foi publicada. Até lá, use um instalador fornecido ou o código-fonte. Um arquivo de código-fonte não é o ZIP de instalação compilado.

https://github.com/Chris-E-Shaw/BopItAccess/releases

- BopItAccess-Installer.exe: O instalador autônomo para Windows x64. Encontra o jogo e gerencia dependências, instalação, atualizações, diagnósticos e remoção. Não é assinado.
- BopItAccess-v1.0.zip: O pacote compilado do mod para instalação manual sem executar o EXE Bop It Access. Inclui Mods/BopItAccess.dll, prism.dll, toda a documentação e licenças Prism, modelo Loader.cfg, README.txt e atalho de desinstalação. Não inclui MelonLoader, .NET, arquivos do jogo ou assemblies gerados.
- Source code (zip): O ZIP do código-fonte da versão gerado automaticamente pelo GitHub. Serve para ler ou compilar o código; não é o pacote compilado do mod.
- Source code (tar.gz): O mesmo código-fonte em um arquivo tar compactado com gzip. Outro formato de origem, não outro instalador do mod.

Instalador sem assinatura e avisos do Windows 11

Este instalador não é assinado. Um programa sem assinatura ou pouco conhecido pode gerar avisos SmartScreen ou antivírus, inclusive possíveis falsos positivos; isso não prova que toda detecção seja incorreta. Obtenha somente do projeto oficial Bop It Access ou de fornecimento direto confiável e decida se confia no arquivo. O ZIP compilado evita este EXE. Não desative o antivírus nem exclua uma unidade ou pasta inteira do jogo.
https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/publish-first-app

Permitir uma detecção específica do Defender

Pressione Win+I, depois Privacidade e segurança > Segurança do Windows > Abrir Segurança do Windows > Proteção contra vírus e ameaças > Histórico de proteção (às vezes chamado histórico de ameaças). Expanda o item correspondente ao instalador. Use Tab até Ações ou Mais ações, pressione Enter e escolha Permitir no dispositivo ou Permitir; aprove o pedido de administrador se aparecer. Um arquivo em quarentena pode exigir Restaurar primeiro, depois permitir caso seja detectado novamente. Se removido, baixe outra cópia do projeto oficial. Confira o item exato antes de permitir.
https://support.microsoft.com/en-us/windows/security/windows-security/protection-history-in-the-windows-security-app · https://support.microsoft.com/en-us/defender/antivirus-and-antimalware-software-faq

Exclusões Defender opcionais e limitadas

Em Proteção contra vírus e ameaças escolha Gerenciar configurações sob o cabeçalho de configurações, depois Exclusões > Adicionar ou remover exclusões. Aprove com Sim o pedido de administrador se aparecer. Escolha Adicionar uma exclusão > Processo, digite exatamente BopItAccess-Installer.exe e pressione Enter. O nome deve corresponder ao executável realmente usado.

A exclusão Processo da Microsoft cobre arquivos abertos por esse processo; não exclui o EXE do instalador, restaura arquivos em quarentena ou ignora SmartScreen. Se Defender detectar o próprio EXE e você confiar nele, uma exclusão Arquivo opcional para aquele EXE baixado específico é a alternativa limitada pertinente. Remova exceções quando não forem mais necessárias.
https://learn.microsoft.com/en-us/defender-endpoint/microsoft-defender-antivirus-exclusions-overview

SmartScreen é um aviso separado. Se confiar nesse EXE e Windows oferecer, escolha Mais informações > Executar assim mesmo. Permitir no Defender ou excluir o processo não evita esse aviso; uma política pode impedir a execução.

Passos revisados em 9 de outubro de 2026 para Windows 11 25H2, compilação 26200.9550. Rótulos podem variar; nenhum teste de interface foi realizado.

Instalador para Windows 0.2.6
-----------------------------

Instalar, atualizar ou remover com o instalador

1. Feche o jogo, execute BopItAccess-Installer.exe e aprove a solicitação de administrador do Windows. Leia o campo Welcome and controls, depois confira Game folder ou use Browse. Welcome and controls é somente leitura, selecionável e recebe o primeiro foco; Alt+W volta a ele.
2. Escolha Install para a última versão pública compilada quando disponível. Antes da primeira publicação, Show advanced exibe Install alpha, que pede confirmação e compila as últimas fontes no computador. Aguarde o sucesso. Play Bop It! The Video Game inicia depois o jogo pelo Steam somente quando escolhido.
3. Para uma instalação existente, abra o instalador com o jogo fechado e confira o status. Update aparece se encontrar uma versão pública mais nova. Escolha Update e aguarde o fim; mantém as configurações salvas. Install alpha é a opção separada para as últimas fontes, não a atualização pública.
4. Para remover o mod, escolha Uninstall e confirme; depois selecione Uninstall for me ou Uninstall for everyone. Ambos removem os arquivos compartilhados do mod desta pasta do jogo. A escolha determina se remove preferências Windows do mod da sua conta ou de todos os perfis locais; as preferências originais do jogo ficam. Aplicativos instalados do Windows usa o mesmo fluxo. Confira o resultado antes de Quit.

A tabela abaixo descreve todas as ações e campos de texto da janela principal. Installer status e Installation progress são informações, não botões. Welcome and controls contém instruções reutilizáveis; Status log contém mensagens variáveis. Abort pergunta antes de reverter uma instalação ativa; Quit usa as mesmas regras de cancelamento seguro. Diálogos incluem Keep open/Quit, confirmação/cancelamento e os dois escopos de preferências na remoção. Show advanced muda apenas as ações visíveis.

Use BopItAccess-Installer-0.2.6.exe ou BopItAccess-Installer.exe fornecido pelo projeto. Os dois nomes contêm o mesmo instalador autônomo para Windows x64. O projeto inclui o código-fonte; nenhum binário compilado público ou GitHub Release foi publicado ainda.

Feche o Bop It!, abra o instalador e aprove a solicitação de permissões de administrador do Windows. O instalador apresenta uma mensagem de boas-vindas, procura o jogo nas bibliotecas do Steam em todas as unidades disponíveis e tenta trazer sua janela para o primeiro plano. Confira a pasta do jogo exibida; use Browse se precisar escolher outra pasta. A tecla Tab move o foco entre os controles. O registro de status é um campo de texto somente leitura: coloque o foco nele para revisar as mensagens com as teclas de direção, selecionar texto ou copiá-lo.

O instalador 0.2.6 solicita brevemente ativação em primeiro plano e foco do teclado ao iniciar. Se outra janela ainda estiver ativa ao terminar a observação inicial limitada, ele faz o título e o botão da barra de tarefas piscarem e pede que você use Alt+Tab para mudar para o instalador. Ative o instalador antes de usar seus comandos de teclado ou controle. Alt+G coloca o foco no campo da pasta do jogo.

A caixa Show advanced está desmarcada quando o instalador é aberto. Ao marcá-la, aparecem Install alpha, Save diagnostics e Copy diagnostics. Install baixa a versão pública mais recente do GitHub quando existe uma. Ainda não há uma versão pública, portanto quem faz os testes precisa atualmente de Show advanced e Install alpha. A opção alfa pede confirmação, baixa o código-fonte mais recente e o compila no seu computador. Update aparece quando é encontrada uma versão pública mais recente para uma cópia instalada.

As mensagens de status explicam em linguagem simples o que está sendo baixado, instalado ou concluído. Uma única barra mostra o progresso estimado de toda a instalação, sem reiniciar a cada download ou arquivo. Ela avança em incrementos de cinco pontos percentuais; algumas etapas de preparação podem levar algum tempo sem uma mudança visível. A mensagem de boas-vindas, o aviso de uma nova atualização disponível e a confirmação de que os dados de diagnóstico foram copiados são enviados ao leitor de tela pelas notificações de acessibilidade do Windows. A leitura em voz alta depende do seu leitor de tela e do suporte dele às notificações do Windows.

O instalador 0.2.6 nunca inicia o Bop It! durante a instalação. A opção alfa reutiliza arquivos locais de compilação compatíveis ou prepara arquivos temporários a partir da sua própria cópia instalada do jogo enquanto ele permanece fechado. Em seguida, o instalador coloca o MelonLoader na pasta do jogo e adiciona imediatamente Mods/BopItAccess.dll, seguido de Prism, configurações, toda a documentação e os componentes necessários para a desinstalação. Aguarde a mensagem de sucesso e, quando estiver pronto, inicie o jogo por conta própria pelo Steam.

Após uma instalação bem-sucedida, Play Bop It! The Video Game aparece. Ative esse botão para iniciar o jogo por conta própria pelo Steam quando estiver pronto. O instalador nunca inicia o jogo automaticamente durante a instalação.

Uma versão compilada precisa do ambiente de execução do .NET 6 para Windows x64, não de um SDK de desenvolvimento. Ambientes de execução completos já existentes são reutilizados. Se o ambiente de execução estiver ausente, ele será baixado da Microsoft e colocado em MelonLoader/Dependencies/dotnet. Install alpha também precisa de um SDK do .NET compatível e do pacote de direcionamento do .NET 6: um SDK existente é reutilizado ou o SDK oficial da Microsoft é instalado para todo o sistema. O instalador não cria uma nova pasta de SDK na raiz do jogo. MelonLoader 0.7.3 Open-Beta e Prism 0.18.3 vêm de suas versões oficiais. Os componentes compartilhados do Microsoft .NET e os SDKs permanecem instalados após o cancelamento ou a desinstalação.

Quit fecha o instalador. Se a instalação ainda estiver em andamento, ele pergunta se você quer cancelá-la e desfazê-la antes de fechar; Keep open permite continuar normalmente. Se a instalação terminar enquanto você decide, a caixa de diálogo é atualizada para informar que ela terminou, e Quit não desfaz a instalação concluída. Depois que a remoção começa, a desinstalação termina com segurança antes de sair. Abort também pede confirmação e reverte as alterações feitas nos arquivos do jogo durante esta tentativa. O cancelamento durante a instalação do Microsoft .NET aguarda a conclusão segura da instalação desse componente compartilhado.

Após confirmar Uninstall, escolha Uninstall for me ou Uninstall for everyone. Ambas as opções removem os arquivos compartilhados do mod desta pasta do jogo, portanto o mod deixará de estar disponível para qualquer pessoa que use essa instalação. A escolha determina de quem serão removidas as preferências salvas do mod no Windows: apenas da conta que solicitou a operação ou de todos os perfis locais do Windows, incluindo os perfis sem sessão iniciada. As preferências do jogo original são mantidas. O SDK do .NET permanece instalado.

Quando o instalador remove sua própria instalação do MelonLoader e nenhum outro mod precisa dela, ele também remove os arquivos conhecidos Loader.cfg e MelonPreferences.cfg e as pastas vazias Plugins, UserLibs e UserData. As configurações do Bop It Access, os registros conhecidos, os guias e os arquivos do instalador são removidos. Outros mods, arquivos compartilhados do carregador que já existiam e arquivos não reconhecidos são protegidos. Isso também significa que um arquivo desconhecido pode fazer uma pasta permanecer; o instalador informa isso nos dados de diagnóstico em vez de excluir dados sem relação com o mod.

O instalador permanece aberto após a desinstalação para que você possa revisar o resultado, salvar os dados de diagnóstico ou instalar novamente. Escolha Quit quando terminar. O programa auxiliar de desinstalação em execução e os arquivos automáticos de diagnóstico são removidos depois que a janela é fechada. Uma reinstalação na mesma janela inicia um novo registro de instalação; a limpeza adiada não pode remover a nova instalação.

Aplicativos instalados do Windows usa a mesma confirmação, escolha de preferências e limpeza. O instalador fornece BopItAccess-uninstall.ps1 na pasta do jogo como um atalho para o desinstalador instalado; futuras compilações do código-fonte também incluem esse script nos arquivos de saída. Um script copiado manualmente não instala o próprio desinstalador. Para uma instalação manual antiga sem um registro de propriedade, o instalador remove os arquivos do mod que consegue identificar e mantém os arquivos compartilhados cuja origem não pode ser determinada.

Se a limpeza não puder terminar com segurança, o instalador explica isso e mantém as informações necessárias para tentar novamente. Para uma instalação gerenciada, a entrada de desinstalação no Windows e o ponto de verificação da limpeza permanecem até que a remoção seja concluída com sucesso. Uma cópia manual antiga não possui um registro persistente de propriedade; tente novamente as operações indicadas nos avisos com o instalador aberto. Não instale, atualize nem remova o mod enquanto o Bop It! estiver em execução.

Atalhos de teclado do instalador
--------------------------------

Welcome and controls: Alt+W. O campo Welcome and controls separado lista atalhos de revisão com controle; Alt+W volta a ele e Alt+L abre o Status log variável. Ambos são somente leitura, selecionáveis e revisáveis. Show advanced anuncia marcado ou desmarcado. Selecionar tudo confirma sucesso ou campo vazio. No teclado, Ctrl+A seleciona todo o texto e Ctrl+C copia a seleção.
Pasta do jogo: Alt+G. Colocar o foco no campo da pasta do jogo.
Browse: Alt+B. Escolher a pasta do jogo.
Install: Alt+I. Instalar a versão pública mais recente, quando disponível.
Install alpha: Alt+A. Confirmar e compilar o código-fonte mais recente; visível com Show advanced.
Update: Alt+U. Instalar uma versão pública mais recente quando oferecida.
Play Bop It! The Video Game: Alt+P. Iniciar o jogo pelo Steam; disponível após uma instalação bem-sucedida.
Uninstall: Alt+N. Confirmar a remoção e escolher de quem remover as preferências do mod no Windows.
Abort: Alt+R. Confirmar o cancelamento da instalação atual.
Registro de status: Alt+L. Colocar o foco nas mensagens de status somente leitura cujo texto pode ser selecionado.
Show advanced: Alt+V. Mostrar ou ocultar a instalação alfa e as ferramentas de diagnóstico.
Save diagnostics: Alt+D. Salvar e continuar registrando toda a sessão de diagnóstico; visível com Show advanced.
Copy diagnostics: Alt+C. Copiar a captura completa dos dados de diagnóstico; visível com Show advanced.
Quit: Alt+Q. Fechar, tratando o cancelamento com segurança se houver uma operação em andamento.

Como usar um controle no instalador
-----------------------------------

O instalador oferece suporte a controles do tipo Xbox e a outros controles que o Windows disponibiliza pelo XInput. Seus comandos são separados dos comandos remapeáveis do jogo. O direcional ou a alavanca analógica esquerda move o foco entre os controles da interface; quando um campo de texto está em foco, as direções passam a revisar o texto. Os botões superiores sempre movem o foco para o controle anterior ou seguinte que pode recebê-lo. A ativa o botão ou a caixa de seleção em foco. As entradas do controle só são processadas enquanto este instalador ou uma de suas próprias caixas de diálogo estiver em primeiro plano.

B volta ou cancela uma caixa de diálogo; na janela principal do instalador, ele pede o cancelamento de uma instalação em andamento ou, nos demais casos, executa a ação de Quit. Start executa a ação de Quit na janela principal e volta em uma caixa de diálogo. Y (o botão superior da parte frontal) seleciona todo o texto quando um campo de texto do instalador está em foco. Fora dos campos de texto da janela principal, Y ativa ou desativa Show advanced. No registro de status ou em outro campo de texto do instalador, o direcional ou a alavanca esquerda funciona como as setas: Esquerda/Direita move por caractere e Cima/Baixo por linha. Segure LT como Ctrl: Esquerda/Direita move por palavra e Cima/Baixo por parágrafo. Segure RT como Shift para ampliar a seleção; segure LT e RT juntos para selecionar palavras ou parágrafos. X copia somente o texto selecionado; selecione primeiro a parte desejada. Ctrl+C no teclado continua copiando a seleção. Quando não há texto selecionado, o instalador também envia notificações acessíveis para o caractere, palavra, linha ou parágrafo na posição do cursor. O instalador envia uma confirmação acessível quando o texto é copiado e informa quando não há seleção ou ocorre falha na cópia. A leitura em voz alta depende do suporte do leitor de tela às notificações do Windows. A navegação com controle nas caixas de diálogo nativas do Windows para escolher pastas e salvar arquivos ainda precisa de verificação humana. O teclado continua disponível para digitar uma pasta ou um nome de arquivo. Esta implementação não abrange controles sem suporte a XInput.

O campo Welcome and controls separado lista atalhos de revisão com controle; Alt+W volta a ele e Alt+L abre o Status log variável. Ambos são somente leitura, selecionáveis e revisáveis. Show advanced anuncia marcado ou desmarcado. Selecionar tudo confirma sucesso ou campo vazio. No teclado, Ctrl+A seleciona todo o texto e Ctrl+C copia a seleção.

O instalador 0.2.6 solicita que cada anúncio de voz emitido substitua a fala anterior do instalador, incluindo revisão de texto, Selecionar tudo, estado marcado/desmarcado de Show advanced, Copy diagnostics e outras confirmações. LB/RB continua anunciando o novo comando em foco. A frequência dos anúncios de status permanece igual; nem toda entrada do log é lida automaticamente. A interrupção real depende do suporte do leitor de tela às notificações do Windows e ainda precisa de verificação humana.

Diagnóstico do instalador
-------------------------

Show advanced exibe Save diagnostics (Alt+D) e Copy diagnostics (Alt+C). Os registros automáticos em UTF-8 são mantidos localmente em %ProgramData%\BopItAccess\diagnostics. Save diagnostics grava toda a sessão atual no arquivo .log ou .txt escolhido e continua registrando até o instalador fechar; Copy diagnostics copia uma captura dos dados e fornece uma confirmação acessível. Salve antes de uma tentativa de instalação ou desinstalação para que seu registro permaneça após a limpeza dos registros automáticos. Os detalhes técnicos de arquivos, downloads, compilador e erros são mantidos aqui, embora o campo de status use mensagens mais curtas. Nada é enviado. Os registros podem conter nomes de usuário do Windows e caminhos completos: revise-os antes de compartilhar. As cópias exportadas deliberadamente permanecem após a desinstalação.

Na primeira inicialização manual após instalar o MelonLoader, ele pode baixar arquivos de suporte e preparar os assemblies do jogo. Aguarde cerca de um minuto, ou mais em alguns sistemas. O mod não pode falar até que o MelonLoader o carregue. Mantenha o jogo aberto e aguarde o anúncio de inicialização do Bop It Access e, em seguida, o anúncio da tela de título, de boas-vindas ou do menu principal antes de usar os comandos do jogo.


Instalar o ZIP compilado sem o EXE Bop It Access

Quando BopItAccess-v1.0.zip for publicado, este caminho usará o DLL já compilado e não precisará do .NET SDK. Ainda exige seu jogo comprado para Windows x64, MelonLoader oficial x64 0.7.3 Open-Beta e runtime .NET 6 Windows x64. Siga as instruções oficiais de MelonLoader e Microsoft; o ZIP não fornece esses requisitos.

https://github.com/LavaGang/MelonLoader#how-to-use-the-installer
https://dotnet.microsoft.com/en-us/download/dotnet/6.0

1. Instale o jogo pelo Steam e localize sua pasta. Feche Bop It! antes de alterar arquivos; use se necessário a função Steam para explorar os arquivos instalados.
2. Instale o MelonLoader oficial x64 nessa pasta e confira que o runtime .NET 6 x64 está instalado. Ainda não execute o jogo: coloque o mod primeiro.
3. Extraia BopItAccess-v1.0.zip compilado para uma pasta temporária. Copie Mods/BopItAccess.dll para Mods do jogo, criando ou mesclando a pasta sem apagar outros mods. Copie prism.dll ao lado de BopIt!.exe.
4. Copie inteiramente documentation e THIRD-PARTY-LICENSES, incluindo idiomas e avisos/licenças Prism. Copie README.txt e BopItAccess-uninstall.ps1 do pacote. O script é só um atalho para um desinstalador gerenciado pelo instalador; copiá-lo não cria desinstalador funcional ou registro em Aplicativos instalados.
5. Para UserData/Loader.cfg: se faltar, copie o modelo. Se existir, mescle apenas [loader] disable_start_screen=true e [console] hide_console=true nas seções corretas, preservando os outros ajustes. Não substitua uma configuração existente pelo modelo.
6. Inicie seu leitor de tela se usado e depois o jogo pelo Steam. MelonLoader pode baixar arquivos auxiliares e gerar assemblies no primeiro início, com o mod já em Mods. Aguarde os anúncios de início do mod e menu antes de usar os controles.

Para atualizar manualmente, feche o jogo e copie mod, Prism, documentação e licenças do pacote novo para os mesmos locais. Preserve BopItAccess.ini, outros mods e arquivos alheios; mescle Loader.cfg como acima. Para desativar/remover o mod manual, apague somente Mods/BopItAccess.dll. Para limpeza adicional, remova apenas arquivos copiados para este mod e UserData/BopItAccess.ini ou seu .tmp; mantenha Prism/MelonLoader se compartilhados. Preferências Windows podem ficar. O ZIP manual não tem registro de propriedade ou desinstalador registrado. Se depois escolher o instalador, Uninstall pode identificar uma cópia manual antiga e limpar preferências protegendo arquivos de propriedade desconhecida. Os logs do mod são Mods/BopItAccess.log e Mods/BopItAccess.log.previous; remova apenas esses logs conhecidos na limpeza.

Empacotamento avançado: scripts/package-mod.ps1 empacota um mod já compilado correspondente e arquivos conhecidos de documentação/configuração/Prism. Confere versões fonte/DLL e exclui arquivos do jogo, gerados ou antigos; não compila. Arquivo e preparação ficam locais.
