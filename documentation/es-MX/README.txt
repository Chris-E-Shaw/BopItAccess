Bop It Access 0.8.0 - Habla y documentación multilingüe

¿Qué hace esto?
--------------
El mod sigue la configuración del juego > Selección de idioma para voz. incluye
Inglés, Francés, Italiano, Alemán, Español (España), Español (Latinoamérica),
Japonés, coreano, chino simplificado y portugués brasileño. Cambiando el
El idioma del juego también cambia los anuncios de modificaciones y la guía del usuario del juego.
Las traducciones iniciales son borradores generados automáticamente y necesitan revisión.
por hablantes fluidos.
La voz está activada de forma predeterminada. Cuando el mod se carga con la voz activada, anuncia
"Bop It Access discurso está listo. El juego aún se está cargando. Espere a que aparezca la pantalla de título o el anuncio del menú principal antes de usar los controles". hasta Tolk. Si aparece la pantalla de título, el mod anuncia la entrada PULSAR actual para abrir el menú principal. se lee
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
entre sesiones. Cuando Tolk usa un lector de pantalla, el mod envía lo mismo
expresión al habla y braille; cuando la SALIDA BRAILLE está desactivada, envía voz
solamente. El discurso directo NVDA y SAPI también envía cada expresión por separado a través de
API braille de Tolk cuando hay disponible un lector de pantalla compatible con braille.
El MODO DE SALIDA comienza en Auto: el mod
habla a través de un lector de pantalla detectado o usa SAPI cuando no hay ninguno en ejecución.
SAPI se puede seleccionar directamente. El menú también incluye JAWS, Window-Eyes, NVDA,
Acceso al sistema y ZoomText. La salida directa NVDA está disponible cuando NVDA está
corriendo. Los otros lectores nombrados se utilizan cuando Tolk los detecta como activos
conductor; Si el lector elegido no está disponible, el mod anuncia un respaldo SAPI.
La compilación de 64 bits de Tolk no es compatible con SuperNova, por lo que no aparece en la lista.
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

FILTRO DE MAYÚSCULAS es una opción guardada de Configuración de Mod, activada de forma predeterminada. solo cambia
el texto se envió a voz y braille, dejando intacta la GUI visible del juego.
Las palabras del menú en mayúsculas se pronuncian en mayúsculas y minúsculas: "PÁSALO" se convierte en "Pásalo".
y "UNO A UNO" se convierte en "Uno a uno". La primera palabra después de un punto es
capitalizado de nuevo. Palabras existentes en casos mixtos y abreviaturas comunes como
Se conservan SAPI, NVDA, SFX y FPS. Al desactivar el interruptor se envía el
texto de voz original.

LEER TIPOS DE CONTROL es otra opción guardada de CONFIGURACIÓN DE MOD, activada de forma predeterminada. Cuando está habilitado,
El tipo del elemento enfocado sigue a su nombre y precede a su valor e índice:
"Control deslizante MÚSICA, 30, 1 de 12", "alternancia de VIBRACIÓN, activado, 6 de 12", o
"Botón REPRODUCIR, 1 de 6". Los menús también identifican pestañas, campos de texto y elementos legibles.
enumere los elementos cuando sea relevante. Los cambios de valor continúan hablando solo del nuevo valor.
RANGOS DESLIZADORES es una opción guardada, desactivada de forma predeterminada. Cuando está habilitado, controles deslizantes enfocados
también informar sus puntos finales disponibles después del valor actual, como
"Control deslizante MÚSICA, 30, rango de 0 a 100, 1 de 12" al indexar y tipos de control
están habilitados. Mover un control deslizante todavía indica solo el nuevo valor.
COMENTARIOS UNO A UNO es una opción guardada, activada de forma predeterminada. Cuando está habilitado,
anuncia el color activo al comienzo de una ronda Uno a Uno y cuando ese
cambios de color. Una vida perdida anuncia el conteo restante, como por ejemplo "2 vidas".
o "1 vida". Una vida ganada anuncia del mismo modo el nuevo conde, hasta
El límite de tres vidas del juego. Si la vida de ambos jugadores cambia juntos,
Ambos recuentos se identifican por color. La función se ejecuta solo durante uno a uno.
jugar.

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

Las sugerencias de botones colocan la entrada antes de su acción: "Entrar o Espacio, activar elemento".
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

El menú MOD SETTINGS también tiene SAPI VOZ, SAPI VOLUMEN, SAPI VELOCIDAD y SAPI PITCH.
controles. SAPI Voice enumera las voces SAPI de 64 bits predeterminadas e instaladas del sistema.
El volumen comienza en 100%; La velocidad y el tono comienzan en 50. El volumen oscila entre el 5 % y el 100 %.
por lo que los avisos de recuperación SAPI siguen siendo audibles. La velocidad y el tono varían de 0 a 100.
Los tres se mudan
pasos de cinco. Se aplican al modo SAPI y al respaldo SAPI de Auto.
La salida directa SAPI evita la enumeración de voz repetida para cambios numéricos,
usa texto sin formato cuando el tono es neutral y omite lectores de pantalla innecesarios
detección. El experimento anterior de silencio y ajuste permanece oculto e inactivo.
Las nuevas mediciones de registro SAPI ubicaron la mayor parte del retraso de envío medido en
el Windows SAPI COM Llamada hablada: las llamadas individuales tardaron entre 188 y 578 ms,
mientras que la configuración de voz inicial tardó entre 375 y 609 ms. Envío separado en braille
normalmente era de 0 a 16 ms. El mod ahora prepara el modo alternativo SAPI en
el trabajador del habla mientras el juego se carga y registra el tiempo de la cola para distinguir
esperando al trabajador desde la hora de despacho. Un primer anuncio después directamente
Al seleccionar SAPI aún se puede incluir la configuración de voz. Estos tiempos no miden
cuando el sonido se vuelve audible, lo que también depende de la voz instalada y
sistema de audio.
La salida braille separada con SAPI ahora almacena en caché las comprobaciones de capacidad Tolk para uno
segundo. Si el envío SAPI es lento, el registro registra la configuración de voz, hablar,
y tiempos de envío en braille.
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
En la siguiente pantalla de selección de canción, anuncia el tema actual.
(Formas, Espacio, Ciudad u Oficina) y si el modo Extremo está activado. Girando hacia
cambiar la cancion habla solo el nuevo tema. Tirar para cambiar la dificultad habla.
sólo el nuevo estado Extremo. La introducción de la pantalla también explica el ROTAR,
TIRAR, PULSAR y acciones Atrás.
Esta introducción completa se repite cada vez que se selecciona un modo y la canción
La pantalla se abre nuevamente, con el tema actual y el estado Extremo.
Presiona HABLAR CONSEJOS (H o presiona el joystick derecho de forma predeterminada) en esta pantalla para escuchar
el modo actual y el texto del tutorial nativo de dificultad antes de comenzar. cada uno
La acción nombrada en esa referencia incluye su teclado actualmente asignado o
control del controlador, siguiendo el TIPO DE PISTAS. Los controles reasignados se leen desde
las ataduras del jugador activo; Uno a uno nombra las PULSAR entradas de ambos jugadores. el
La superposición del tutorial cronometrado durante el juego activo permanece silenciosa para que no pueda ocultarse.
los comandos hablados del juego. La pista anuncia este uso adicional de SPEAK HINTS.
El control LEER DESCRIPCIONES pronuncia una descripción visual del elemento seleccionado.
Escenario de Formas, Espacio, Ciudad u Oficina bajo demanda. Está disponible en esta pantalla.
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
color y vidas restantes durante ese modo. Compartido PULSAR señales no por
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

Dentro de Controles, el mod dice PULSAR, PULSAR (Jugador 2), TOCAR, ROTAR, GIRAR, TIRAR,
y Restablecer los valores predeterminados. Lee el enlace actual para el dispositivo de entrada activo,
anuncia enlaces modificados y lee los comentarios de enlace visibles del juego.
Anuncia cómo volver a Configuración una vez por visita. Cuando Restablecer valores predeterminados cambia un
vinculante, informa que los enlaces se restablecieron.

El mod expone una fila RESET GYRO nativa y agrega una SALIDA DE VOZ DE CAMBIO
atajo. Reset Gyro se encuentra con los controles del juego; las filas específicas del mod
permanecen juntos en la parte inferior del menú, antes de Restablecer los valores predeterminados. CAMBIAR
SALIDA DE VOZ recorre los mismos modos que Configuración > Configuración de Mod > SALIDA
MODO: Automático, SAPI, JAWS, Window-Eyes, NVDA, Acceso al sistema y ZoomText. su
Las entradas predeterminadas son F9 en el teclado y el botón de la cara oeste (X en una Xbox
controlador). El botón Inicio del controlador está reservado para el nativo del juego.
Acción del menú. La elección actual se anuncia cuando se utiliza el acceso directo y
se guarda con la misma configuración del Modo de salida.

El mod agrega nueve filas al menú Controles del juego: Grupo Anterior,
Grupo Siguiente, Fecha Anterior, Fecha Siguiente, LEER DESCRIPCIONES, LEER PUNTUACIÓN,
ALTERNAR VOZ, HABLAR CONSEJOS y CAMBIAR SALIDA DE VOZ. Los primeros cuatro
abordar los filtros de la tabla de clasificación
alcanzado con O/P y K/L en la distribución de teclado predeterminada, o los parachoques y
D-pad izquierda/derecha en un controlador. Enfoque una fila para escuchar su vinculación actual,
luego usa la acción normal PULSAR/confirmar del juego para volver a vincularlo. Las filas se desplazan
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

Dentro de Calibración de audio, el mod lee los controles Calibrar, Atrás y PULSAR,
anuncia las instrucciones y etapas de calibración, lee la cuenta atrás del calentamiento,
y anuncia el resultado de latencia mostrado. No habla cada latido durante
el ejercicio de sincronización para que el ritmo siga siendo audible.

La pantalla de pausa anuncia En pausa, el botón Reanudar o Menú principal enfocado,
y sus sugerencias de botones. Los cambios de enfoque interrumpen el discurso anterior del menú de pausa;
Reanudar o abandonar la ronda borra cualquier discurso de pausa restante antes del juego.
o el menú principal continúa. Un respaldo cauteloso también lee controles enfocados
y texto visible en un panel inesperado orientado al jugador sin tener prioridad
sobre los lectores dedicados anteriores. Durante una ronda activa, el mod deja el
Solo los comandos verbales del juego y la puntuación en progreso. Moviendo el ratón sobre un
elemento sin darle Unity Es posible que el enfoque de la interfaz de usuario no hable.

Instalar
-------
1. Cierre Bop It! si se está ejecutando.
2. Extraiga todos los archivos de este ZIP en la carpeta del juego Bop It!:
   C:\Program Files (x86)\Steam\steamapps\common\Bop It!
   Permita que Windows reemplace Mods\BopItAccess.dll y fusione los Mods y
   carpetas de documentación.
3. Inicie su lector de pantalla, luego inicie Bop It! a Steam como de costumbre.

MelonLoader ya debe estar instalado. El ZIP incluye Mods\BopItAccess.dll,
documentation\BopItAccess-user-guide.html, todas las subcarpetas de idiomas traducidas,
y sus documentos complementarios,
Tolk.dll y nvdaControllerClient64.dll. La guía del juego carga el HTML
desde esa carpeta de documentación cada vez que se abre. Este mod fue creado para los instalados.
MelonLoader 0.7.3 Open-Beta y Bop It! (Unity 2022.3.50f1, x64).
Las primeras traducciones no inglesas se realizaron con traducción automática.
y necesita revisión por parte de hablantes fluidos. Por favor informe redacción poco clara o incorrecta.
Los nombres de acción Gameplay usan los términos traducidos del juego. Shapes, Space, City,
y Office restantes Inglés como títulos de escenario fijo. Si la voz del sistema SAPI lo hace
no pronuncia bien su idioma, seleccione una voz instalada adecuada en Mod
Ajustes.

Prueba los menús y pantallas.
-------------------------
Espere a que aparezca el anuncio en la pantalla de título, luego use PULSAR para abrir el
menú principal. El juego puede tardar varios segundos después de que aparezca el mensaje de listo del mod.
aceptar esta entrada.
En una primera ejecución, la pantalla de bienvenida aparece antes del menú principal. Seleccione su
mensaje para escuchar la introducción nuevamente. Elija Abrir configuración de mod, Leer usuario
Guía o Continuar al juego. Speak Hints nombra su teclado actual y
asignaciones de controlador en el mensaje de bienvenida independientemente del tipo de sugerencia.
Abre Play y muévete entre los cuatro modos. Elija uno para llegar a la selección de canciones.
ROTAR para recorrer los temas y TIRAR para activar o desactivar el modo Extremo. el
mod anuncia cada cambio. Presione G o LT para escuchar la etapa seleccionada actualmente
descripción. Presiona H o presiona el joystick derecho para escuchar el texto del tutorial del modo.
controles de acción asignados actualmente y sugerencias de botones. PULSAR inicia el modo elegido;
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
detección del panel, objetos de la interfaz de usuario seleccionados y si Tolk aceptó anuncios.
Tolk la aceptación no prueba por sí sola que el habla fuera audible.

Para deshabilitar el mod, elimine Mods\BopItAccess.dll. y MelonLoader puede permanecer instalado.

Archivos y avisos de terceros
-----------------------------
Tolk es una biblioteca de accesibilidad de código abierto de Christopher T. D. Kager. esto
El paquete incluye Tolk.dll y nvdaControllerClient64.dll de una compilación pública.
paquete, sin modificar esos binarios. Tolk tiene licencia bajo la versión GNU LGPL
3; Los textos adjuntos de las licencias GNU LGPL y GNU GPL están en
LICENCIAS DE TERCEROS. Tolk fuente y licencia: https://github.com/dkager/tolk

La fuente del mod Bop It Access se encuentra en el archivo ZIP de fuente independiente.
