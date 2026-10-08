Bop It Access 0.9.11 - Prism Habla y Braille

¿Qué hace esto?
--------------
El mod sigue la configuración del juego > Selección de idioma para voz. incluye
Inglés, Francés, Italiano, Alemán, Español (España), Español (Latinoamérica),
Japonés, coreano, chino simplificado y portugués brasileño. Cambiando el
El idioma del juego también cambia los anuncios de modificaciones y la guía del usuario del juego.
Las traducciones iniciales son borradores generados automáticamente y necesitan revisión.
por hablantes fluidos.
La voz está activada de forma predeterminada. Cuando el mod se carga con la voz activada, anuncia
"Bop It Access discurso está listo. El juego aún se está cargando. Espere a que aparezca la pantalla de título o el anuncio del menú principal antes de usar los controles". hasta Prism. Si aparece la pantalla de título, el mod anuncia la entrada GOLPEAR actual para abrir el menú principal. se lee
el botón del menú principal enfocado y la fila Configuración enfocada. Los valores de configuración son
hablado con el nombre de la fila enfocado. Cambiar un valor mientras el foco permanece en eso
La fila solo dice el nuevo valor. LATENCIA DE AUDIO, CONTROLES y CONECTARSE son acciones
botones, por lo que se pronuncian sin el marcador de posición "0" sin sentido del juego.
El menú Configuración también tiene un control deslizante LÍMITE FPS con 30, 60, 120, 240 y
Opciones ILIMITADAS. Comienza en 60 para una nueva instalación y recuerda el
valor seleccionado entre sesiones. Focus pronuncia el nombre y el valor; cambiándolo
Sólo habla el nuevo valor. El límite cambia la velocidad de fotogramas objetivo de Unity mientras
dejando intactos la escala de tiempo del juego, el tiempo de actualización fijo y el audio.
Como cualquier límite de fotogramas, una configuración más baja también significa menos encuestas de entrada basadas en fotogramas.
Si 30 FPS parece menos sensible en un juego rápido, elige 60, 120 o ILIMITADO.
La opción SILENCIAR AUDIO EN FONDO aparece directamente debajo de VOZ EN OFF en
Configuración. Cuando está habilitado, silencia el audio del juego mientras la ventana del juego no está abierta.
enfocado, luego restaura el estado de audio del juego anterior cuando vuelve el enfoque.
Comienza apagado y se guarda entre sesiones.
En el primer uso del mod, la MÚSICA, los SFX y la VOZ en OFF nativos del juego
Los controles deslizantes comienzan en 30. La actualización mantiene la configuración de audio del juego previamente guardada.
Una vez que el juego llega a su menú principal por primera vez, aparece una pantalla de bienvenida.
enfocar. Su mensaje se puede enfocar nuevamente con Arriba y sus opciones se abren Mod
Configuración, abre la guía del usuario dentro del juego o continúa al menú principal.
La pantalla de bienvenida se marca como completa solo después de que una elección sale exitosamente
eso. Cerrar el juego mientras está abierto lo deja listo para el próximo lanzamiento.
La indexación de configuraciones ahora espera las filas de audio, FPS y CONFIGURACIÓN DEL MOD antes
anunciando el primer elemento enfocado en una pantalla de Configuración recién abierta.

Debajo de Controles, Configuración ahora tiene un menú de CONFIGURACIÓN DE MOD. SALIDA DE VOZ utiliza el mismo
interruptor maestro guardado como F8 o selección de controlador, incluida la recuperación hablada
instrucciones cuando el habla está apagada. La SALIDA BRAILLE se activa y se guarda.
entre sesiones. Prism envía anuncios a un lector de pantalla compatible
salida braille cuando esta configuración está activada. Al apagarlo se detiene el braille del mod.
mensajes mientras deja la voz disponible.
OUTPUT MODE: Auto usa un lector de pantalla compatible en ejecución, después OneCore y, por último, SAPI.
Auto, OneCore, SAPI, NVDA, JAWS, UI Automation, ZDSR, ZoomText,
Boy PC Reader, PC Talker, Sense Reader, System Access, Window-Eyes.
Cada lector de pantalla y motor de voz solo estará disponible si lo admiten la versión de Prism instalada y el sistema del jugador. Si el modo elegido no está disponible, el mod cambia a una salida disponible y lo anuncia una sola vez.
Las voces SAPI guardadas se buscan por el nombre que muestra Prism; si varias comparten nombre, puede elegirse la primera.
SILENCIAR LA VOZ EN FONDO es una opción guardada, desactivada de forma predeterminada. Cuando está habilitado,
el mod deja de hablar tan pronto como el juego pierde el foco de la ventana. Discurso creado
mientras el juego está en segundo plano se descarta y se reanudan los anuncios
con nueva actividad después de que regresa el foco. Si la voz en sí está desactivada cuando el juego
recupera el enfoque, el mod proporciona la recuperación actual del teclado y del controlador
instrucciones una vez.

El menú MOD SETTINGS también tiene INDICE, activado de forma predeterminada y guardado entre sesiones.
Cuando está habilitado, un elemento de menú enfocado incluye su posición, como "REPRODUCIR, 1 de 6".
Esto se aplica a los menús principal y de configuración, controles, modos de juego, mod.
Configuraciones, opciones de finalización del juego, tablas de clasificación, logros, créditos y otros
Pantallas compatibles. El recuento sigue las opciones disponibles actualmente. Cambiando
un control deslizante o un interruptor mientras permanece enfocado aún anuncia solo el nuevo valor.

DAR FORMATO AL HABLA es una opción guardada en Configuración del mod, activada de forma predeterminada. Usa mayúsculas y minúsculas naturales para los nombres de menú escritos completamente en mayúsculas, conservando palabras con mayúsculas y minúsculas y abreviaturas como SAPI, NVDA, SFX y FPS. En la guía del juego, si se anuncia el número de línea y el texto termina sin puntuación, añade tres puntos para una pausa antes del número. La puntuación existente se conserva. Solo cambia el texto enviado a voz y braille; el texto visible del juego y la guía permanecen intactos. Desactivarla desactiva ambos ajustes. Se conserva la preferencia anterior del filtro de mayúsculas.

ANUNCIAR TIPOS DE CONTROL es otra opción guardada de CONFIGURACIÓN DE MOD, activada de forma predeterminada. Cuando está habilitado,
El tipo del elemento enfocado sigue a su nombre y precede a su valor e índice:
"Control deslizante MÚSICA, 30, 1 de 12", "alternancia de VIBRACIÓN, activado, 6 de 12", o
"Botón REPRODUCIR, 1 de 6". Los menús también identifican pestañas, campos de texto y elementos legibles.
enumere los elementos cuando sea relevante. Los cambios de valor continúan hablando solo del nuevo valor.
RANGOS DESLIZADORES es una opción guardada, desactivada de forma predeterminada. Cuando está habilitado, controles deslizantes enfocados
también informar sus puntos finales disponibles después del valor actual, como
"Control deslizante MÚSICA, 30, rango de 0 a 100, 1 de 12" al indexar y tipos de control
están habilitados. Mover un control deslizante todavía indica solo el nuevo valor.
Los comentarios uno a uno se guardan y están activados de forma predeterminada. Anuncia el color activo al inicio y cuando cambia. Al perder una vida, anuncia la cantidad restante. Al ganar una vida, anuncia el color del jugador y su nuevo total, por ejemplo «Verde, 3 vidas», para que ambos sepan quién fue más rápido. Estos anuncios se desactivan al desactivar los comentarios uno a uno. El límite de tres vidas del juego no cambia. Esta función solo actúa durante una partida uno a uno.

El TIPO DE CONSEJO ahora aparece encima de CONSEJOS DEL BOTÓN DE HABLA AUTOMÁTICA en el menú Configuración del mod.
SUGERENCIAS DEL BOTÓN DE HABLA AUTOMÁTICA es una opción guardada y está activada de forma predeterminada. girándolo
Desactivado suprime las sugerencias automáticas, mientras que SPEAK HINTS permanece disponible bajo demanda.
BUTTON HINTS DELAY tiene Ninguno, 5 segundos
(Puede interrumpir el habla), 10 segundos, 15 segundos, 30 segundos y 60 segundos.
El valor predeterminado es 10 segundos. Con Ninguno, las entradas válidas para la pantalla actual y
sus acciones están incluidas en la cadena de voz ordinaria del elemento enfocado,
después de una parada completa. La acción para el control enfocado se habla ante el general.
navegación del menú. No hay un anuncio de primera pista por separado. con un cronometrado
retraso, el primer anuncio de pista sigue a tanta inactividad. La opción de 5 segundos puede
interrumpir el discurso que ya está en curso; retrasos más largos hacen cola detrás de él. después
el primer anuncio de pista, otro retraso comienza sólo cuando el jugador da
entrada, a menos que las repeticiones estén habilitadas. Pasar a otro elemento enfocado o cambiar
un control deslizante o palanca enfocado cuenta como entrada y reinicia el retraso, incluso si el
El escaneo de enlace de entrada del juego no detecta la tecla o la acción del controlador. Una llave sin usar
eso no cambia la interfaz de usuario todavía no la reinicia.
TIPO DE PISTAS es un control deslizante guardado con Automático, Teclado, Controlador y Ambos.
Automático es el valor predeterminado y sigue el teclado o teclado utilizado más recientemente.
entrada del controlador. El uso del mouse cuenta como teclado. El teclado y el controlador hablan
sólo las sugerencias de ese dispositivo; Ambos proporcionan ambos conjuntos de entradas con dispositivo explícito
nombres. La recuperación de voz en off siempre incluye ambos dispositivos para que el
El jugador puede encontrar el control que vuelve a activar la voz.

Las sugerencias de botones colocan la entrada antes de su acción: "Ingresar o Space, activar elemento".
Las sugerencias para un solo dispositivo omiten el nombre del dispositivo. Se pronuncian los nombres de los sticks del controlador.
en su totalidad, como "Stick izquierdo arriba y abajo". Ambos modos identifican el teclado
y entradas del controlador. El mod lee los enlaces actuales del juego de forma nativa.
Los cambios de vinculación se reflejan en estas sugerencias. Cuando no hay ningún controlador
conectado y el juego tiene diferentes nombres de botones faciales en diferentes controladores
tipos, la sugerencia utiliza "botón de confirmación" o "botón de retroceso" en lugar de asumir un
Diseño de Xbox. Las filas de puntuación de la tabla de clasificación utilizan Re Pág y Av Pág en el teclado.
El controlador arriba/abajo lee filas solo cuando ningún control de la tabla de clasificación tiene foco;
Las sugerencias informan solo los controles disponibles para el TIPO DE PISTA seleccionado. ordinario
Las sugerencias en pantalla también incluyen los enlaces actuales SPEAK HINTS y TOGGLE SPEECH.
Ambos se buscan de forma centralizada, por lo que futuros controles globales pueden unirse al mismo
lista de sugerencias sin cambiar cada pantalla por separado.

REPETIR SUGERENCIAS DEL BOTÓN es un control deslizante guardado por separado: Desactivado, 2x, 3x, 4x, 5x o
Infinitamente. El valor predeterminado es Infinitamente. El número son las lecturas totales en una.
ciclo: 2x significa la primera pista y una repetición; 3x significa la primera pista y
dos repeticiones. Desactivado todavía permite la primera pista automática o manual.
REPEAT INTERVAL establece el retraso entre repeticiones en 15, 30, 45 o
60 segundos y el valor predeterminado es 30 segundos. Con BUTTON HINTS DELAY establecido en Ninguno,
el cronómetro de repetición comienza inmediatamente después de la entrada. Entrada, un enfoque o valor
cambio, o un cambio de pantalla reinicia el ciclo de sugerencias para la pantalla actual.
HABLAR CONSEJOS reemplaza el
pista automática pendiente para ese ciclo, luego usa REPETIR INTERVALO para cualquier
repeticiones configuradas. Esto también funciona con las SUGERENCIAS DEL BOTÓN DE HABLA AUTOMÁTICA desactivadas.
Las pistas se suprimen durante
juego activo y las fases de sincronización de audio de calibración, donde se
el habla podría enmascarar una señal. Un recordatorio existente guardado de 15, 30 o 60 segundos
El retraso de la versión 0.6.2 se convierte en el nuevo valor de RETARDO DE SUGERENCIAS DE BOTÓN.

MOD SETTINGS: Voz, Volumen, Velocidad y Tono ajustan la salida OneCore o SAPI que está realmente en uso, incluso en el modo Auto. Solo aparecen los controles compatibles; con otras salidas se ocultan. Cada motor guarda sus ajustes por separado. Volumen: del 5 % al 100 % en pasos de 5, inicialmente 100 %. Velocidad y tono: de 0 a 100 en pasos de 5, inicialmente 50. El volumen mínimo mantiene audibles los avisos de recuperación.
Las opciones de configuración de mod se recuerdan entre sesiones. RESTAURAR LOS VALORES PREDETERMINADOS DEL MOD
devuelve esas opciones a los valores predeterminados descritos anteriormente. Presiónelo una vez para solicitar
confirmación, luego presiónelo nuevamente dentro de cinco segundos para restaurarlos. mudanza
a otra fila o dejar pasar cinco segundos cancela la solicitud. esto no
cambiar los controles deslizantes de MÚSICA, SFX o VOZ EN OFF del juego, LIMITAR FPS o personalizar
Enlaces de teclado y controlador. Atrás regresa a Configuración.
OPEN USER'S GUIDE lee la guía HTML para el idioma actualmente seleccionado en
en la fila Configuración > Idioma del juego. La guía en inglés está en
documentation\BopItAccess-user-guide.html; las guías traducidas están en el idioma
subcarpetas. Si falta una copia traducida o es ilegible, la guía en inglés
se abre en su lugar. Su lista de temas proviene del índice del documento y
Se recarga cada vez que se abre. Confirmar abre un tema. Arriba y Abajo leen sus líneas. En tablas,
Izquierda mueve una columna hacia la izquierda y Derecha mueve una columna hacia la derecha; Mantener arriba y abajo
la columna actual al moverse entre filas. Celdas de etiqueta de encabezados de columna
en lugar de aparecer como filas de datos. La mesa se anuncia una vez al entrar, y
su fin se anuncia a la salida. Atrás regresa a los temas o sale de la guía.
Mientras lee, el mod aplica el parámetro Filtro de música del menú del juego y
restaura su valor anterior al salir. RESTABLECER PANTALLA DE BIENVENIDA solicita un
presione por segunda vez dentro de cinco segundos y luego aparecerá la pantalla de bienvenida en la
próximo lanzamiento del juego. Cambiar de fila o esperar cinco segundos cancela la confirmación.
Esta actualización restaura el diseño de la fila de configuración nativa del juego, de modo que arriba/abajo
la navegación permanece en las filas de Configuración después de agregar MOD SETTINGS.
También inicia el submenú MOD SETTINGS en SPEECH OUTPUT cada vez que se abre,
evitar que una fila ATRÁS previamente seleccionada cierre el menú inmediatamente
cuando se usa Enter para volver a abrirlo.
La entrada que abre MOD SETTINGS ahora es ignorada por sus filas hasta que esa entrada sea
liberado, por lo que al reabrir el menú no se puede desactivar la voz. los mods
Las filas de enlace de controles agregadas también esperan a que se realice la entrada de apertura.
liberado antes de aceptar una solicitud de nueva vinculación.

Dentro de Play, el mod lee Solo, Party, Pass It y One on One cuando está enfocado.
En la siguiente pantalla de selección de canción, el mod anuncia el tema
(Shapes, Space, City u Office) y la dificultad: Clásico o Extremo. RETORCER
cambia la canción y anuncia solo el nuevo tema. TIRAR cambia la dificultad
y anuncia solo Clásico o Extremo. La introducción también explica RETORCER,
TIRAR, GOLPEAR y Atrás. Esta introducción se repite cada vez que se elige
un modo y se abre la pantalla de canción, con el tema y la dificultad
actuales.
Presiona HABLAR CONSEJOS (H o presiona el joystick derecho de forma predeterminada) en esta pantalla para escuchar
el modo actual y el texto del tutorial nativo de dificultad antes de comenzar. cada uno
La acción nombrada en esa referencia incluye su teclado actualmente asignado o
control del controlador, siguiendo el TIPO DE PISTAS. Los controles reasignados se leen desde
las ataduras del jugador activo; Uno a uno nombra las GOLPEAR entradas de ambos jugadores. el
La superposición del tutorial cronometrado durante el juego activo permanece silenciosa para que no pueda ocultarse.
los comandos hablados del juego. La pista anuncia este uso adicional de SPEAK HINTS.
El control LEER DESCRIPCIONES pronuncia una descripción visual del elemento seleccionado.
Shapes, Space, City o Office etapa bajo demanda. Está disponible en esta pantalla.
solamente, antes de que comience el juego. Sus entradas predeterminadas son G en el teclado y LT.
(gatillo izquierdo) en el controlador. R fue reemplazada porque es el reinicio del juego.
Atajo giroscópico. La introducción de la pantalla anuncia el enlace actual.
Al iniciar la reproducción se detiene cualquier discurso que quede en la selección de la canción para que no pueda enmascarar el
señales verbales del juego. Las descripciones comienzan con los detalles de la escena en lugar de
repitiendo el nombre artístico seleccionado.

En la pantalla de resultados finales, Solo, Party y Pass It anuncian la puntuación final.
antes del discurso del menú. Solo lee los botones enfocados de repetición y clasificación.
y explica Atrás. Los otros modos leen sus opciones Continuar, Reproducir y
Indicaciones de retroceso. Uno a uno muestra un ganador en lugar de una puntuación final numérica, por lo que
el mod anuncia el ganador que se muestra allí. El discurso de partitura tiene prioridad sobre
el anuncio del menú inicial; Las indicaciones de la pantalla de resultados se ponen en cola después.
Los cambios posteriores en el enfoque del menú se interrumpen entre sí. Cambios rápidos inmediatamente
después del final del juego se combinan hasta que el breve anuncio de puntuación haya tenido tiempo
para terminar, manteniendo la última opción de menú enfocada.
Los anuncios de puntuación más alta en solitario y de rango del grupo se pronuncian cuando el juego informa
un nuevo resultado en la clasificación.
LEER PUNTUACIÓN repite el resultado final a pedido solo mientras se completa el resultado del juego.
La pantalla es visible. Las entradas predeterminadas son T en el teclado y presionar el joystick izquierdo
controlador. Solo, Party y Pass It repiten su puntuación final; uno a uno
repite el ganador mostrado por el juego. La acción se desactiva durante el juego.
selección de canciones y todas las demás pantallas. RT (gatillo derecho) no se utilizó como
predeterminado porque el juego ya lo vincula a Reset Gyro y Auto Play.
Las repeticiones solicitadas hablan inmediatamente y pueden ser interrumpidas por el menú de resultados.
navegación. Sólo el anuncio automático del resultado retrasa el menú inicial
discurso para que la partitura se escuche primero.
LA RETROALIMENTACIÓN UNO A UNO está activada de forma predeterminada en Configuración > Configuración de modificación para voz activa
color y vidas restantes durante ese modo. Compartido GOLPEAR señales no por
ellos mismos identifican un color, por lo que el mod conserva el último color definido.

TOGGLE SPEECH activa o desactiva todo el habla mod ordinaria desde cualquier pantalla. su
Los valores predeterminados son F8 en el teclado y Seleccionar en el controlador. Cuando está apagado, el mod
detiene la voz actual y anuncia que la voz está apagada, junto con la voz actual.
controles de teclado y controlador para volver a encenderlo. El estado apagado es
guardado entre sesiones de juego. Si el juego comienza sin hablar, el mod da
esas instrucciones de recuperación en lugar de su mensaje de carga habitual. girando
discurso de nuevo anuncia "Speech on". Otros discursos mod permanecen en silencio mientras están apagados.
El control Alternar voz permanece activo incluso cuando la voz está desactivada.

Las tablas de clasificación alcanzadas desde el menú principal, los resultados en solitario y los resultados en grupo.
anunciar la canción, el dispositivo, el grupo y la fecha seleccionados, cuando estén disponibles. ellos leen
rango, nombre del jugador y puntuación, incluidos los estados de carga y resultado vacío. Página arriba
y Av Pág lee filas de puntuación individuales incluso cuando un filtro tiene el foco. Nativo
controles enfocados como Local, Amigos, Global, Hoy, Este mes, Todos los tiempos,
Se hablan Atrás y Continuar. La clasificación del Partido también informa su nombre
estado de selección y confirmación.

Esta actualización mantiene las tablas de clasificación de resultados en silencio durante la selección de modo y canción.
Los nombres de los filtros de la tabla de clasificación hablan primero, seguidos de los resúmenes de puntuaciones.
El botón del menú de logros habla normalmente; instrucciones del libro espere hasta
sus páginas realmente se abren y el cierre se anuncia sólo después de eso.

El libro de logros del juego anuncia su página visible y el de cada logro.
nombre, descripción y estado bloqueado o desbloqueado. Utilice Arriba y Abajo para leer elementos
en una página. Los controles izquierdo y derecho del juego pasan las páginas como de costumbre.

Los créditos anuncian la primera línea cuando se abren. Usa el menú Arriba y Abajo del juego.
Controles para leer cada línea de crédito. El desplazamiento visual automático continúa como
antes. Si esos controles no están disponibles, las líneas se ponen en cola a medida que aparecen;
si eso tampoco funciona, el texto completo de los créditos se anuncia una vez.

Dentro de Controles, el mod dice GOLPEAR, GOLPEAR (Jugador 2), ATIZAR, RETORCER, GIRAR, TIRAR,
y Restablecer los valores predeterminados. Lee el enlace actual para el dispositivo de entrada activo,
anuncia enlaces modificados y lee los comentarios de enlace visibles del juego.
Anuncia cómo volver a Configuración una vez por visita. Cuando Restablecer valores predeterminados cambia un
vinculante, informa que los enlaces se restablecieron.

El mod expone una fila RESET GYRO nativa y agrega una SALIDA DE VOZ DE CAMBIO
atajo. Reset Gyro se encuentra con los controles del juego; las filas específicas del mod
permanecen juntos en la parte inferior del menú, antes de Restablecer los valores predeterminados. CAMBIAR
SALIDA DE VOZ recorre los mismos modos que Configuración > Configuración de Mod > SALIDA
MODO. El orden de los modos es:
Auto, OneCore, SAPI, NVDA, JAWS, UI Automation, ZDSR, ZoomText,
Boy PC Reader, PC Talker, Sense Reader, System Access, Window-Eyes.
Las entradas predeterminadas del atajo son F9 en el teclado y el botón de la cara oeste (X en una Xbox
controlador). El botón Inicio del controlador está reservado para el nativo del juego.
Acción del menú. La elección actual se anuncia cuando se utiliza el acceso directo y
se guarda con la misma configuración del Modo de salida.

El mod agrega nueve filas al menú Controles del juego: Grupo Anterior,
Grupo Siguiente, Fecha Anterior, Fecha Siguiente, LEER DESCRIPCIONES, LEER PUNTUACIÓN,
ALTERNAR VOZ, HABLAR CONSEJOS y CAMBIAR SALIDA DE VOZ. Los primeros cuatro
abordar los filtros de la tabla de clasificación
alcanzado con O/P y K/L en la distribución de teclado predeterminada, o los parachoques y
D-pad izquierda/derecha en un controlador. Enfoque una fila para escuchar su vinculación actual,
luego usa la acción normal GOLPEAR/confirmar del juego para volver a vincularlo. Las filas se desplazan
dentro del panel de Controles existente. LEER DESCRIPCIONES también puede ser rebotado para
teclado y controlador. El mod guarda su vinculación y se restablece a los valores predeterminados.
restaura G y LT. READ SCORE también puede ser rebote para teclado y controlador;
Restablecer los valores predeterminados restaura T y presionar el joystick izquierdo. La encuadernación original del juego.
Las filas y las cuatro filas de la tabla de clasificación utilizan el mismo flujo de vinculación de controles.
TOGGLE SPEECH se puede rebotar para el teclado y el controlador. Sus fijaciones son
guardado por el mod, y Restablecer valores predeterminados restaura F8 y Seleccionar. Si la unión
se cambia mientras la voz está desactivada, el mod anuncia los nuevos controles de recuperación.
SPEAK HINTS también puede ser rebote. Sus valores predeterminados son H y presionar el joystick derecho.
Dice la pista de la pantalla actual inmediatamente sin programar un segundo
primera pista automática. Las repeticiones configuradas aún pueden seguir. esta en silencio
durante el juego y las señales de calibración de audio cronometradas.
CAMBIAR LA SALIDA DE VOZ se puede recuperar para el teclado y el controlador; Restablecer a
El valor predeterminado restaura F9 y el botón de la cara oeste. Si ya hay un nuevo enlace
asignado a otro juego o acción de modificación, el menú Controles rechaza la
duplicar y mantener la asignación anterior. RESET GYRO puede recuperarse mediante
El mismo procedimiento de controles nativos que las otras acciones del juego.
Al regresar de la selección de canciones al menú principal se restauran las sugerencias del menú principal incluso
cuando un administrador de juegos en caché todavía informa un estado de juego antiguo. Sugerencia de botón
Los temporizadores y la selección automática del dispositivo de pistas siguen el juego y mod asignados.
controles; Las teclas no utilizadas, como una tecla de control no asignada, no las restablecen.
Esta actualización evita que la fila Controles obsoletos anuncie "Error al volver a vincular"
repetidamente después de que se cierra su escena. Leer descripciones ya no temporalmente
anula cualquiera de los enlaces de entrada propios del juego.

Dentro de Calibración de audio, el mod lee los controles Calibrar, Atrás y GOLPEAR,
anuncia las instrucciones y etapas de calibración, lee la cuenta atrás del calentamiento,
y anuncia el resultado de latencia mostrado. No habla cada latido durante
el ejercicio de sincronización para que el ritmo siga siendo audible. Tras la última entrada de calibración, el mod dice inmediatamente «¡Listo!». Deja de golpear y espera el resultado medido. Si la calibración falla porque no se ha realizado ninguna entrada, dice «Calibración fallida».

La pantalla de pausa anuncia Pausa, el botón Continuar o Menú principal seleccionado y sus ayudas de controles. Cambiar de selección interrumpe la lectura anterior del menú de pausa. Continuar o salir de la partida detiene las voces pendientes de la pausa antes de seguir en el juego o el menú principal. Durante una partida, el mod deja intactas las instrucciones de voz del juego y la puntuación en curso.

Instalar
-------
El repositorio de origen no contiene ningún mod compilado ni DLL Prism. Construye el mod por
siguiendo README.md, luego cierra el juego y copia BopItAccess.dll en sus Mods
carpeta. Obtenga el oficial Windows x64 Prism v0.18.3 prism.dll de
https://github.com/ethindp/prism/releases y colóquelo al lado del ejecutable del juego,
no dentro de Mods. Copie la carpeta de documentación de la compilación en la carpeta del juego,
incluidas sus subcarpetas de idioma traducido. Inicie su lector de pantalla si
use uno, luego inicie Bop It! a Steam. Prism puede usar SAPI cuando sea compatible
El lector de pantalla no se está ejecutando. La guía del juego carga el HTML desde el
carpeta de documentación cada vez que se abre. Este mod fue desarrollado para MelonLoader
0.7.3 Open-Beta y Bop It! (Unity 2022.3.50f1, x64).
Las primeras traducciones no inglesas se realizaron con traducción automática.
y necesita revisión por parte de hablantes fluidos. Por favor informe redacción poco clara o incorrecta.
Los nombres de las acciones del juego utilizan los términos traducidos del juego. Shapes, Space, City,
y Office siguen siendo en inglés como títulos escénicos fijos. Si la voz del sistema SAPI no
no pronuncia bien su idioma, seleccione una voz instalada adecuada en Mod
Configuración.

Prueba los menús y pantallas.
-------------------------
Espere a que aparezca el anuncio en la pantalla de título, luego use GOLPEAR para abrir el
menú principal. El juego puede tardar varios segundos después de que aparezca el mensaje de listo del mod.
aceptar esta entrada.
En una primera ejecución, la pantalla de bienvenida aparece antes del menú principal. Seleccione su
mensaje para escuchar la introducción nuevamente. Elija Abrir configuración de mod, Leer usuario
Guía o Continuar al juego. Speak Hints nombra su teclado actual y
asignaciones de controlador en el mensaje de bienvenida independientemente del tipo de sugerencia.
Abre Play y muévete entre los cuatro modos. Elija uno para llegar a la selección de canciones.
RETORCER para recorrer los temas y TIRAR para elegir Clásico o Extremo. el
mod anuncia cada cambio. Presione G o LT para escuchar la etapa seleccionada actualmente
descripción. Presiona H o presiona el joystick derecho para escuchar el texto del tutorial del modo.
controles de acción asignados actualmente y sugerencias de botones. GOLPEAR inicia el modo elegido;
Vuelve la espalda. Durante una ronda, use
el control de Menú del juego para abrir Pausa, luego muévete entre Reanudar y Menú principal.
Al final de un juego, escuche el puntaje o el ganador uno a uno antes del
Se anuncian los controles de la pantalla de resultados. Presiona T o presiona el joystick izquierdo para repetir el
resultado final mientras la pantalla de fin del juego está visible.
Presione F8 o seleccione el controlador para activar o desactivar la voz mod desde cualquier pantalla.
Presione F9 o el controlador Oeste (X en un controlador Xbox) para alternar el discurso
modo de salida. La misma opción está disponible en Configuración > Configuración de Mod > MODO DE SALIDA.
La SALIDA BRAILLE en Configuración > Configuración de Mod está activada de forma predeterminada. Visor Braille de NVDA
puede mostrar el braille y su texto equivalente sin una pantalla física.
Para una comparación ON/OFF, utilice el modo braille de seguimiento de cursores de NVDA con Mostrar
Mensajes habilitados; su modo de visualización-salida de voz reflejaría la voz incluso
cuando la configuración de SALIDA BRAILLE del mod está desactivada.
En Configuración > Configuración de Mod, use SUGERENCIAS DEL BOTÓN DE HABLA AUTOMÁTICA para habilitar o deshabilitar
instrucciones automáticas. Presiona H o presiona el joystick derecho para escuchar la pista actual.
bajo demanda.
TIPO DE PISTAS elige Automático, Teclado, Controlador o Ambos para esas pistas.
BOTÓN SUGERENCIAS DEMORA elige si acompañan al discurso de enfoque o siguen un
periodo de inactividad. REPETIR SUGERENCIAS DE BOTONES e INTERVALO DE REPETIR controlan cualquier
recordatorios adicionales.
Abra Tablas de clasificación desde el menú principal o una pantalla de resultados. Cambiar un filtro a
escuche su nueva selección y lea partituras individuales con Page Up y Page Down.
De forma predeterminada, O/P se mueve entre grupos de clasificación y K/L se mueve entre fechas.
rangos. Sus entradas de controlador correspondientes son parachoques izquierdo/derecho y
Control direccional izquierda/derecha. Las cuatro nuevas filas de Controles están destinadas a reasignarlos.
Abra Logros y pase páginas con Izquierda y Derecha; use Arriba y Abajo para cada uno
entrada. Abra Créditos y use Arriba y Abajo para leer sus líneas independientemente del
desplazamiento visual.

Si falta la voz, marca Mods\BopItAccess.log en la carpeta del juego. Se graba
detección de panel, objetos de interfaz de usuario seleccionados e inicialización y envío de Prism.
El envío exitoso no prueba por sí solo que el habla fuera audible.

Para deshabilitar el mod, elimine Mods\BopItAccess.dll. y MelonLoader puede permanecer instalado.

Archivos y avisos de terceros
-----------------------------
Prism es una biblioteca de accesibilidad de código abierto creada por Ethan Dupuy y sus colaboradores.
Tiene la licencia pública de Mozilla, versión 2.0. Esta fuente
El repositorio no incluye prism.dll. Fuente, versiones y licencia:
https://github.com/ethindp/prism
Ver THIRD-PARTY-NOTICES.txt para conocer los avisos de dependencia vigentes.

Editar el archivo de ajustes
----------------------------
Si un idioma desconocido, el audio demasiado alto o una voz problemática dificultan el uso de los menús, puedes cambiar los ajustes fuera del juego. Tras el inicio, el mod crea automáticamente UserData/BopItAccess.ini en la carpeta de Bop It!, con tus ajustes actuales. Es un archivo de texto que puedes abrir con un editor como el Bloc de notas.

El archivo incluye idioma, volúmenes de música, efectos y voz, vibración, pantalla completa, resolución y latencia de audio del juego; preferencias de voz, braille, pistas y otras opciones del mod; perfiles de voz independientes para OneCore y SAPI; y asignaciones de controles del juego y del mod destinados a los jugadores. Las resoluciones disponibles y las voces instaladas aparecen en los comentarios.

Cierra el juego antes de editar. Busca la sección adecuada y cambia el valor de la entrada existente, guarda el archivo y vuelve a iniciar el juego. Los cambios se leen al arrancar, no inmediatamente durante una sesión. Los cambios realizados en los menús actualizan el archivo automáticamente.

Los nombres de secciones y ajustes permanecen en inglés en todos los idiomas. Se recomiendan On y Off para los interruptores; también se aceptan True/False, Yes/No y 1/0. Los comentarios explican las opciones y los intervalos. Una entrada ausente o no válida conserva el ajuste guardado correspondiente; los demás cambios válidos se aplican. Las asignaciones de controles duplicadas se rechazan.

Los comentarios y las entradas desconocidas se conservan. Si otro programa modifica el archivo mientras el juego está abierto, el mod deja de guardarlo durante el resto de la sesión para proteger esos cambios. Cierra y vuelve a abrir el juego para utilizarlos. Puedes guardar una copia de seguridad antes de editar.

Si una voz falla, escribe Voice=System default en la sección OneCore o SAPI. Las voces OneCore usan nombre | idioma; SAPI acepta el nombre mostrado de una voz instalada o su identificador completo del Registro. El archivo enumera las opciones disponibles. OutputMode=Auto prueba un lector de pantalla compatible activo, después OneCore y por último SAPI.

El siguiente ejemplo restablece el inglés, un audio de juego más bajo y la voz automática con las voces predeterminadas del sistema. Cambia las entradas correspondientes que ya existen en tu archivo; este fragmento sirve de referencia y no es otro bloque para añadir al final. Conserva los demás ajustes.

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

Tras confirmar Uninstall, elige Uninstall for me o Uninstall for everyone. Ambas opciones eliminan los archivos compartidos del mod de esta carpeta del juego, por lo que el mod dejará de estar disponible para cualquier persona que utilice esa instalación. La elección determina de quién se eliminan las preferencias guardadas del mod en Windows: solo de la cuenta que solicita la operación o de todos los perfiles locales de Windows, incluidos aquellos que tienen la sesión cerrada. Se conservan las preferencias del juego original. El SDK de .NET permanece instalado.

Cuando el instalador elimina su propia instalación de MelonLoader y ningún otro mod la necesita, también elimina los archivos conocidos Loader.cfg y MelonPreferences.cfg y las carpetas vacías Plugins, UserLibs y UserData. Se eliminan la configuración de Bop It Access, los registros conocidos, las guías y los archivos del instalador. Se protegen los demás mods, los archivos compartidos del cargador que ya existían y los archivos no reconocidos. Esto también significa que un archivo desconocido puede hacer que una carpeta permanezca; el instalador lo indica en los datos de diagnóstico en lugar de eliminar datos ajenos.

Si la limpieza no puede terminar de forma segura, el instalador lo explica y conserva la información necesaria para volver a intentarlo. En una instalación gestionada, su entrada de desinstalación de Windows y el punto de control de limpieza permanecen hasta que la eliminación se completa correctamente. Una copia manual antigua no dispone de un registro persistente de propiedad; vuelve a intentar las operaciones señaladas en sus advertencias con el instalador abierto. No instales, actualices ni elimines el mod mientras Bop It! esté en ejecución.

Nota de transparencia de IA
--------------------

Este mod se ha creado mediante « vibe coding ». Todo el código fue generado e investigado completamente por inteligencia artificial, con una comprensión humana técnica limitada de su arquitectura subyacente. Utilice este mod bajo su propia responsabilidad.

Dicho esto, cada característica de modificación y decisión de diseño fue escrita y aprobada por humanos. Las pruebas nunca fueron automatizadas; fueron realizadas cuidadosa y exhaustivamente por jugadores y evaluadores humanos reales.

Tenga en cuenta: el texto y la documentación multilingües fueron generados por AI y no han sido revisados por hablantes nativos. Es de esperar una alta imprecisión en la traducción. Sin codificación agente, este proyecto no existiría. ¡Gracias por darle una oportunidad!

¿Qué puede venir después?
------------------

Este proyecto está esencialmente completo y no se planean contenidos ni funciones importantes. Sin embargo, este mod se mantendrá y actualizará activamente con el tiempo según sea necesario, y los comentarios de los jugadores impulsarán estas mejoras. El trabajo potencial futuro incluye revisiones adicionales y correcciones de errores, refinamiento del código y mejoras continuas en la capacidad de respuesta del habla. Prism crea una posible ruta a otras plataformas en el futuro, pero este mod actualmente solo admite Windows x64. El repositorio del proyecto es el lugar para seguir el desarrollo posterior.

gracias
---------

A aquellos que jugaron probaron este mod antes de su lanzamiento y ayudaron a llevarlo a donde está ahora, gracias. Todos sabéis quién sois. A los jugadores que ofrecen comentarios, prueban el mod por primera vez o creen en mí y en este proyecto, gracias. Su apoyo me motiva a seguir haciendo cosas en un mundo que puede parecer loco y profundamente defectuoso. Espero que este proyecto te facilite disfrutar del juego y jugar con otros. Muchas gracias a todos. Disfruta Bop It!

— Christopher Shaw

MelonLoader ventanas de inicio
---------------------------

La plantilla Loader.cfg suministrada oculta la pantalla de inicio y la consola independientes de MelonLoader.
El instalador aplica estos dos valores antes de que abras el juego tú mismo. No omiten la pantalla de título
del juego ni la bienvenida del mod.

Con el juego cerrado, abre UserData/Loader.cfg en la carpeta del juego. Si el archivo ya existe, establece disable_start_screen en true dentro de su sección [loader] existente, y hide_console en true dentro de su sección [console] existente. Conserva todas las demás entradas. Si el archivo no existe, copia la plantilla UserData/Loader.cfg incluida con la compilación, o configuration/Loader.cfg desde el código fuente. Nunca sustituyas un Loader.cfg existente por la plantilla completa.

[loader]
disable_start_screen = true

[console]
hide_console = true

El mod no restablece estas opciones cada vez que se inicia. Puedes volver a cambiar manualmente cualquiera de los dos valores a false para solucionar problemas. Si la desinstalación conserva una instalación compartida de MelonLoader, solo restaura los indicadores de destino establecidos por el instalador que no se hayan modificado y conserva los demás cambios. Si elimina su propia instalación de MelonLoader que ya no se utiliza, también elimina los archivos conocidos Loader.cfg y MelonPreferences.cfg.

Después de un cambio exitoso, el mod anuncia la entrada y la acción a la que está asignado, por ejemplo, "Space asignado a GOLPEAR".

Versión preliminar 0.2.2 del instalador de Windows
--------------------------------------------------

Cierra Bop It!, abre el instalador y acepta la solicitud de permisos de administrador de Windows. El instalador te da la bienvenida, busca el juego en las bibliotecas de Steam de todas las unidades disponibles e intenta traer su ventana al primer plano. Comprueba la carpeta del juego que se muestra; utiliza Browse si necesitas elegir otra carpeta. La tecla Tab permite desplazarse entre los controles. El registro de estado es un campo de texto de solo lectura: sitúa el foco en él para revisar los mensajes con las teclas de dirección, seleccionar texto o copiarlo.

El instalador 0.2.2 solicita brevemente activación en primer plano y foco del teclado al iniciar. Si al terminar su observación inicial limitada sigue activa otra ventana, hace parpadear el título y el botón de la barra de tareas y pide usar Alt+Tab para pasar al instalador. Activa el instalador antes de usar sus controles de teclado o mando. Alt+G enfoca el campo de la carpeta del juego.

La casilla Show advanced está desmarcada al abrir el instalador. Al marcarla, aparecen Install alpha, Save diagnostics y Copy diagnostics. Install descarga la última versión pública de GitHub cuando existe. Todavía no hay ninguna versión pública, por lo que quienes realizan pruebas necesitan actualmente Show advanced e Install alpha. La opción alfa pide confirmación, descarga el código fuente más reciente y lo compila en tu ordenador. Update aparece cuando se encuentra una versión pública más reciente para una copia instalada.

Los mensajes de estado explican con un lenguaje sencillo qué se está descargando, instalando o finalizando. Una única barra muestra el progreso estimado de toda la instalación, sin volver a empezar con cada descarga o archivo. Avanza en incrementos de cinco puntos porcentuales; algunas fases de preparación pueden tardar sin que se vea ningún cambio. El mensaje de bienvenida, el aviso de una nueva actualización disponible y la confirmación de que se han copiado los datos de diagnóstico se envían al lector de pantalla mediante las notificaciones de accesibilidad de Windows. Que se lean en voz alta depende del lector de pantalla y de su compatibilidad con las notificaciones de Windows.

El instalador 0.2.2 nunca inicia Bop It! durante la instalación. La opción alfa reutiliza archivos locales de compilación compatibles o prepara archivos temporales a partir de tu propia copia instalada del juego mientras permanece cerrado. Después, el instalador coloca MelonLoader en la carpeta del juego y añade inmediatamente Mods/BopItAccess.dll, seguido de Prism, la configuración, toda la documentación y los componentes necesarios para la desinstalación. Espera al mensaje de éxito y, cuando estés listo, inicia el juego tú mismo desde Steam.

Tras una instalación completada correctamente, aparece Play Bop It! The Video Game. Actívalo para iniciar el juego tú mismo desde Steam cuando estés listo. El instalador nunca inicia el juego automáticamente durante la instalación.

Una versión compilada necesita el entorno de ejecución de .NET 6 para Windows x64, no un SDK de desarrollo. Se reutilizan los entornos de ejecución completos que ya estén instalados. Si falta el entorno de ejecución, se descarga de Microsoft y se coloca en MelonLoader/Dependencies/dotnet. Install alpha también necesita un SDK de .NET compatible y el paquete de destino de .NET 6: se reutiliza un SDK existente o se instala el SDK oficial de Microsoft para todo el sistema. El instalador no crea ninguna carpeta nueva para el SDK en la carpeta raíz del juego. MelonLoader 0.7.3 Open-Beta y Prism 0.18.3 proceden de sus versiones oficiales. Los componentes compartidos de Microsoft .NET y los SDK permanecen instalados tras cancelar o desinstalar.

Quit cierra el instalador. Si la instalación sigue en curso, pregunta si quieres cancelarla y deshacerla antes de cerrar; Keep open permite continuar con normalidad. Si la instalación termina mientras decides, el diálogo se actualiza para indicar que ha finalizado y Quit no deshace la instalación completada. Una vez que comienza la eliminación, la desinstalación termina de forma segura antes de salir. Abort también pide confirmación y revierte los cambios realizados en los archivos del juego durante este intento. Si cancelas durante la instalación de Microsoft .NET, se espera a que la instalación de ese componente compartido termine de forma segura.

Tras confirmar Uninstall, elige Uninstall for me o Uninstall for everyone. Ambas opciones eliminan los archivos compartidos del mod de esta carpeta del juego, por lo que el mod dejará de estar disponible para cualquier persona que utilice esa instalación. La elección determina de quién se eliminan las preferencias guardadas del mod en Windows: solo de la cuenta que solicita la operación o de todos los perfiles locales de Windows, incluidos aquellos que tienen la sesión cerrada. Se conservan las preferencias del juego original. El SDK de .NET permanece instalado.

Cuando el instalador elimina su propia instalación de MelonLoader y ningún otro mod la necesita, también elimina los archivos conocidos Loader.cfg y MelonPreferences.cfg y las carpetas vacías Plugins, UserLibs y UserData. Se eliminan la configuración de Bop It Access, los registros conocidos, las guías y los archivos del instalador. Se protegen los demás mods, los archivos compartidos del cargador que ya existían y los archivos no reconocidos. Esto también significa que un archivo desconocido puede hacer que una carpeta permanezca; el instalador lo indica en los datos de diagnóstico en lugar de eliminar datos ajenos.

El instalador permanece abierto tras la desinstalación para que puedas revisar el resultado, guardar los datos de diagnóstico o instalar de nuevo. Elige Quit cuando termines. El programa auxiliar de desinstalación en ejecución y los archivos automáticos de diagnóstico se eliminan después de cerrar la ventana. Una reinstalación en la misma ventana inicia un nuevo registro de instalación; la limpieza aplazada no puede eliminar la nueva instalación.

Aplicaciones instaladas de Windows utiliza la misma confirmación, elección de preferencias y limpieza. El instalador proporciona BopItAccess-uninstall.ps1 en la carpeta del juego como acceso directo al desinstalador instalado; las futuras compilaciones del código fuente también incluyen este script entre sus archivos de salida. Copiar manualmente el script no instala el propio desinstalador. En una instalación manual antigua sin registro de propiedad, el instalador elimina los archivos del mod que puede identificar y conserva los archivos compartidos cuyo origen no puede determinar.

Si la limpieza no puede terminar de forma segura, el instalador lo explica y conserva la información necesaria para volver a intentarlo. En una instalación gestionada, su entrada de desinstalación de Windows y el punto de control de limpieza permanecen hasta que la eliminación se completa correctamente. Una copia manual antigua no dispone de un registro persistente de propiedad; vuelve a intentar las operaciones señaladas en sus advertencias con el instalador abierto. No instales, actualices ni elimines el mod mientras Bop It! esté en ejecución.

Atajos de teclado del instalador
--------------------------------

Carpeta del juego: Alt+G. Situar el foco en el campo de la carpeta del juego.
Browse: Alt+B. Elegir la carpeta del juego.
Install: Alt+I. Instalar la última versión pública, cuando esté disponible.
Install alpha: Alt+A. Confirmar y compilar el código fuente más reciente; visible con Show advanced.
Update: Alt+U. Instalar una versión pública más reciente cuando se ofrezca.
Play Bop It! The Video Game: Alt+P. Iniciar el juego desde Steam; disponible tras una instalación completada correctamente.
Uninstall: Alt+N. Confirmar la eliminación y elegir de quién se eliminan las preferencias del mod en Windows.
Abort: Alt+R. Confirmar la cancelación de la instalación actual.
Registro de estado: Alt+L. Situar el foco en los mensajes de estado de solo lectura cuyo texto se puede seleccionar.
Show advanced: Alt+V. Mostrar u ocultar la instalación alfa y las herramientas de diagnóstico.
Save diagnostics: Alt+D. Guardar y seguir registrando toda la sesión de diagnóstico; visible con Show advanced.
Copy diagnostics: Alt+C. Copiar la instantánea completa de diagnóstico; visible con Show advanced.
Quit: Alt+Q. Cerrar, gestionando la cancelación de forma segura si hay una operación en curso.

Uso de un mando en el instalador
--------------------------------

El instalador admite mandos de tipo Xbox y otros mandos que Windows expone mediante XInput. Sus controles son independientes de los controles del juego que se pueden reasignar. La cruceta o la palanca izquierda permiten desplazarse entre los controles; cuando un campo de texto tiene el foco, las direcciones sirven para revisar su texto. Los botones superiores siempre desplazan el foco al control anterior o siguiente que pueda recibirlo. A activa el botón o la casilla que tenga el foco. La entrada del mando solo se procesa mientras este instalador o uno de sus propios diálogos esté en primer plano.

B vuelve atrás o cancela un diálogo; en la ventana principal del instalador, solicita cancelar una instalación en curso y, en los demás casos, realiza la acción de Quit. Start realiza la acción de Quit en la ventana principal y vuelve atrás en un diálogo. Y (el botón frontal de arriba) selecciona todo el texto cuando un campo de texto del instalador tiene el foco. Fuera de los campos de texto de la ventana principal, Y activa o desactiva Show advanced. En el registro de estado u otro campo de texto del instalador, la cruceta o la palanca izquierda actúa como las flechas: Izquierda/Derecha avanza por caracteres y Arriba/Abajo por líneas. Mantén LT como Ctrl: Izquierda/Derecha avanza por palabras y Arriba/Abajo por párrafos. Mantén RT como Mayús para ampliar la selección; mantén LT y RT juntos para seleccionar palabras o párrafos. X copia únicamente el texto seleccionado; selecciona primero la parte que quieras. Ctrl+C en el teclado sigue copiando la selección. Si no hay texto seleccionado, el instalador también envía notificaciones accesibles del carácter, palabra, línea o párrafo en la posición del cursor. El instalador envía una confirmación accesible al copiar texto e informa si no hay selección o falla la copia. Que se lea en voz alta depende de la compatibilidad de tu lector de pantalla con las notificaciones de Windows. La navegación con mando en los diálogos nativos de Windows para elegir carpetas y guardar archivos todavía requiere verificación humana. El teclado sigue disponible para introducir una carpeta o un nombre de archivo. Esta implementación no admite mandos sin compatibilidad con XInput.

El mensaje de bienvenida del registro de estado enumera los atajos del mando para revisar texto; usa Alt+L para volver al registro. Al cambiar Show advanced se envía una notificación de accesibilidad de Windows que indica si está marcado o desmarcado. Seleccionar todo también proporciona una confirmación accesible, o informa de que el campo está vacío.

Diagnóstico del instalador
--------------------------

Show advanced muestra Save diagnostics (Alt+D) y Copy diagnostics (Alt+C). Los registros automáticos UTF-8 se guardan localmente en %ProgramData%\BopItAccess\diagnostics. Save diagnostics escribe toda la sesión actual en el archivo .log o .txt que elijas y continúa registrando hasta que se cierra el instalador; Copy diagnostics copia una instantánea y proporciona una confirmación accesible. Guarda antes de una prueba de instalación o desinstalación para que tu registro se conserve tras la limpieza de los registros automáticos. Aquí se guardan los detalles técnicos de los archivos, las descargas, el compilador y los errores, aunque el campo de estado utiliza mensajes más breves. No se sube nada. Los registros pueden contener nombres de usuario de Windows y rutas completas: revísalos antes de compartirlos. Las copias exportadas expresamente permanecen tras la desinstalación.

En el primer inicio manual tras instalar MelonLoader, este puede descargar archivos auxiliares y preparar los ensamblados del juego. Deja pasar aproximadamente un minuto, o más en algunos sistemas. El mod no puede hablar hasta que MelonLoader lo cargue. Mantén el juego abierto y espera al anuncio de inicio de Bop It Access y, después, al anuncio de la pantalla de título, de bienvenida o del menú principal antes de utilizar los controles del juego.
