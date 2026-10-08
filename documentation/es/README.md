# Bop It Access

Bop It Access es un mod de accesibilidad no oficial para la versión Windows Steam de **Bop It!**. Utiliza MelonLoader y [Prism](https://github.com/ethindp/prism) para agregar comentarios de voz y braille a menús y pantallas de juegos. Las características actuales incluyen una pantalla de bienvenida de primera ejecución, una guía del usuario en el juego, títulos hablados y pantallas de pausa, configuraciones y controles, selección de canciones, puntuaciones finales y tablas de clasificación, logros, créditos, sugerencias de botones, texto tutorial a pedido con asignaciones de control actuales antes de una ronda y descripciones de las cuatro etapas. La versión 0.9.0 utiliza Prism para salida de voz y braille. El mod sigue el idioma seleccionado del juego e incluye una guía para cada idioma que ofrece el juego.

## Editar el archivo de ajustes

Si un idioma desconocido, el audio demasiado alto o una voz problemática dificultan el uso de los menús, puedes cambiar los ajustes fuera del juego. Tras el inicio, el mod crea automáticamente UserData/BopItAccess.ini en la carpeta de Bop It!, con tus ajustes actuales. Es un archivo de texto que puedes abrir con un editor como el Bloc de notas.

El archivo incluye idioma, volúmenes de música, efectos y voz, vibración, pantalla completa, resolución y latencia de audio del juego; preferencias de voz, braille, pistas y otras opciones del mod; perfiles de voz independientes para OneCore y SAPI; y asignaciones de controles del juego y del mod destinados a los jugadores. Las resoluciones disponibles y las voces instaladas aparecen en los comentarios.

Cierra el juego antes de editar. Busca la sección adecuada y cambia el valor de la entrada existente, guarda el archivo y vuelve a iniciar el juego. Los cambios se leen al arrancar, no inmediatamente durante una sesión. Los cambios realizados en los menús actualizan el archivo automáticamente.

Los nombres de secciones y ajustes permanecen en inglés en todos los idiomas. Se recomiendan On y Off para los interruptores; también se aceptan True/False, Yes/No y 1/0. Los comentarios explican las opciones y los intervalos. Una entrada ausente o no válida conserva el ajuste guardado correspondiente; los demás cambios válidos se aplican. Las asignaciones de controles duplicadas se rechazan.

Los comentarios y las entradas desconocidas se conservan. Si otro programa modifica el archivo mientras el juego está abierto, el mod deja de guardarlo durante el resto de la sesión para proteger esos cambios. Cierra y vuelve a abrir el juego para utilizarlos. Puedes guardar una copia de seguridad antes de editar.

Si una voz falla, escribe Voice=System default en la sección OneCore o SAPI. Las voces OneCore usan nombre | idioma; SAPI acepta el nombre mostrado de una voz instalada o su identificador completo del Registro. El archivo enumera las opciones disponibles. OutputMode=Auto prueba un lector de pantalla compatible activo, después OneCore y por último SAPI.

El siguiente ejemplo restablece el inglés, un audio de juego más bajo y la voz automática con las voces predeterminadas del sistema. Cambia las entradas correspondientes que ya existen en tu archivo; este fragmento sirve de referencia y no es otro bloque para añadir al final. Conserva los demás ajustes.

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

Tras confirmar Uninstall, elige Uninstall for me o Uninstall for everyone. Ambas opciones eliminan los archivos compartidos del mod de esta carpeta del juego, por lo que el mod dejará de estar disponible para cualquier persona que utilice esa instalación. La elección determina de quién se eliminan las preferencias guardadas del mod en Windows: solo de la cuenta que solicita la operación o de todos los perfiles locales de Windows, incluidos aquellos que tienen la sesión cerrada. Se conservan las preferencias del juego original. El SDK de .NET permanece instalado.

Cuando el instalador elimina su propia instalación de MelonLoader y ningún otro mod la necesita, también elimina los archivos conocidos Loader.cfg y MelonPreferences.cfg y las carpetas vacías Plugins, UserLibs y UserData. Se eliminan la configuración de Bop It Access, los registros conocidos, las guías y los archivos del instalador. Se protegen los demás mods, los archivos compartidos del cargador que ya existían y los archivos no reconocidos. Esto también significa que un archivo desconocido puede hacer que una carpeta permanezca; el instalador lo indica en los datos de diagnóstico en lugar de eliminar datos ajenos.

Aplicaciones instaladas de Windows utiliza la misma confirmación, elección de preferencias y limpieza. El instalador proporciona BopItAccess-uninstall.ps1 en la carpeta del juego como acceso directo al desinstalador instalado; las futuras compilaciones del código fuente también incluyen este script entre sus archivos de salida. Copiar manualmente el script no instala el propio desinstalador. En una instalación manual antigua sin registro de propiedad, el instalador elimina los archivos del mod que puede identificar y conserva los archivos compartidos cuyo origen no puede determinar.

Si la limpieza no puede terminar de forma segura, el instalador lo explica y conserva la información necesaria para volver a intentarlo. En una instalación gestionada, su entrada de desinstalación de Windows y el punto de control de limpieza permanecen hasta que la eliminación se completa correctamente. Una copia manual antigua no dispone de un registro persistente de propiedad; vuelve a intentar las operaciones señaladas en sus advertencias con el instalador abierto. No instales, actualices ni elimines el mod mientras Bop It! esté en ejecución.

## Estado del proyecto

Este proyecto está esencialmente completo y no se planean contenidos ni funciones importantes. Se mantendrá según sea necesario y los comentarios de los jugadores guiarán las mejoras. El repositorio GitHub contiene código fuente y documentación técnica. **Aún no hay versiones de GitHub.** La fuente ahora también contiene un proyecto de instalación de Windows. Hasta que se publique una versión, su botón **Instalar** explica que no hay ninguna versión disponible; **Instalar alfa** crea la última confirmación de la rama principal desde la fuente.

El historial de confirmaciones incluye instantáneas de origen reconstruidas de 37 compilaciones anteriores. Las confirmaciones se crearon cuando esos archivos se importaron a Git; sus fechas no son las fechas de construcción originales. el [historial técnico de construcción](BopItAccess-build-history.html) describe el trabajo detrás de cada instantánea.

## Requisitos

- Windows x64 y tu propia instalación de Bop It! para Steam.
- MelonLoader instalado en el directorio del juego. El desarrollo ha utilizado MelonLoader **0.7.3 Open-Beta** con la compilación del juego x64 Unity **2022.3.50f1**. Otras combinaciones no han sido verificadas.
- Un SDK .NET con el **.NET paquete de 6 objetivos**, porque el mod apunta `net6.0`.
- Para instalación, el oficial Windows x64 Prism v0.18.3 `prism.dll`. Este binario de terceros no se encuentra en este repositorio.

El mod hace referencia a las DLL generadas o instaladas por MelonLoader en el directorio del juego. No incluye ni redistribuye montajes de juegos.

<a id="build-from-source"></a>
## Construir desde la fuente

1. Instala MelonLoader, inicia Bop It! una vez y luego cierra el juego. MelonLoader debería crear `MelonLoader\Il2CppAssemblies` debajo del directorio del juego.
2. Clona o descarga este repositorio. Abra PowerShell en el directorio raíz del repositorio.
3. conjunto `$gameDir` a **tu** directorio de instalación Bop It!, luego compila:

   ```powershell
   $gameDir = 'C:\Program Files (x86)\Steam\steamapps\common\Bop It!'
   dotnet build .\src\BopItAccess.csproj -c Release "-p:BopItGameDir=$gameDir"
   ```

   La ruta de ejemplo es la ubicación Windows habitual de Steam. Cámbielo si su biblioteca Steam está en otro lugar. El proyecto busca el MelonLoader requerido y las DLL del juego generadas e informa que falta una ruta antes de compilar.

4. La DLL mod construida estará en `src\bin\Release\net6.0\BopItAccess.dll`.

Si el SDK informa que falta un paquete de destino .NET 6, instale un SDK que incluya ese paquete. el proyecto `NuGet.Config` no configura fuentes de paquetes en línea.

<a id="install-your-build"></a>
## Instala tu compilación

1. Cierra el juego. Copiar lo construido `BopItAccess.dll` en `<game directory>\Mods\`. Crea el `Mods` directorio si MelonLoader no lo ha creado.
2. Obtenga la versión oficial de Prism v0.18.3 para Windows x64 (`prism.dll`) en las [versiones publicadas de Prism](https://github.com/ethindp/prism/releases), o compile esa misma versión desde el código fuente. Coloque `prism.dll` junto al ejecutable del juego, en la carpeta principal del juego, no en la carpeta `Mods`.
3. Copia la compilación completa `src\bin\Release\net6.0\documentation\` carpeta en el directorio del juego. Contiene la guía en inglés en su raíz y guías traducidas en `fr`, `it`, `de`, `es`, `es-MX`, `ja`, `ko`, `zh`, y `pt-BR`. Conserve esas subcarpetas y los documentos complementarios. La guía del juego lee el HTML del idioma actual del juego cada vez que se abre, por lo que reemplazar una guía actualiza su contenido sin reconstruir la DLL.
4. Inicie el lector de pantalla antes del juego. Si no hay uno compatible en ejecución, Prism prefiere OneCore y usa SAPI si OneCore no está disponible.

El comando de compilación Bop It Access compila solo este mod; no compila ni descarga Prism. Si el habla no comienza, inspeccione `<game directory>\Mods\BopItAccess.log`. El registro registra la inicialización Prism y el envío de voz, aunque un envío exitoso por sí solo no puede demostrar que se escuchó el audio.

En una primera ejecución, la pantalla de bienvenida aparece después de que el menú principal del juego esté listo. Sus opciones abren la Configuración de Mod, leen la guía del usuario en el juego o continúan con el juego. Mod Settings también ofrece **Abrir la Guía del usuario** y una acción confirmada de **Restablecer pantalla de bienvenida** que muestra la pantalla de bienvenida en el próximo inicio. En la guía, use Arriba/Abajo para elegir temas o leer líneas y Confirmar para abrir un tema. Dentro de las tablas, Izquierda mueve una columna hacia la izquierda, Derecha mueve una columna hacia la derecha y Arriba/Abajo mantiene la columna actual mientras cambia de fila. Los encabezados de las columnas etiquetan las celdas en lugar de aparecer como filas de datos; la mesa se anuncia a la entrada y su final a la salida. Atrás sale un tema o la guía.

Elige un idioma en la fila **Configuración > Idioma** del juego. El discurso mod sigue esa selección. La guía del juego utiliza el documento HTML traducido correspondiente, con el inglés como alternativa si la copia seleccionada falta o es ilegible. El texto incluido en otro idioma es una primera versión traducida automáticamente; Las correcciones de hablantes fluidos son bienvenidas.

Las acciones del juego usan sus traducciones oficiales. Shapes, Space, City y Office conservan sus nombres de escenario en inglés. Para OneCore o SAPI, elija en Configuración del mod una voz instalada para el idioma del juego si la voz predeterminada no suena bien. Voz, Volumen, Velocidad y Tono ajustan la salida OneCore o SAPI que está realmente en uso, incluso en el modo Auto. Solo aparecen los controles compatibles; con otras salidas se ocultan. Cada motor guarda sus ajustes por separado.

Leer posiciones de los menús. Esta opción guardada, activada de forma predeterminada, anuncia la posición del elemento dentro de su menú. Los comentarios uno a uno se guardan y están activados de forma predeterminada. Anuncia el color activo al inicio y cuando cambia. Al perder una vida, anuncia la cantidad restante. Al ganar una vida, anuncia el color del jugador y su nuevo total, por ejemplo «Verde, 3 vidas», para que ambos sepan quién fue más rápido. Estos anuncios se desactivan al desactivar los comentarios uno a uno. El límite de tres vidas del juego no cambia. Esta función solo actúa durante una partida uno a uno. Tras la última entrada de calibración, el mod dice inmediatamente «¡Listo!». Deja de golpear y espera el resultado medido. Si la calibración falla porque no se ha realizado ninguna entrada, dice «Calibración fallida».

**Dar formato al habla**: Hace más natural el texto en mayúsculas para voz y braille. En la guía del juego añade puntos suspensivos antes del número de línea si el texto termina sin puntuación. El texto visible no cambia. Desactívalo para recibir el texto tal como está escrito.

### MelonLoader ventanas de inicio

La plantilla Loader.cfg suministrada oculta la pantalla de inicio y la consola independientes de MelonLoader. El instalador aplica estos dos valores antes de que abras el juego tú mismo. No omiten la pantalla de título del juego ni la bienvenida del mod.

Con el juego cerrado, abre `UserData/Loader.cfg` en la carpeta del juego. Si el archivo ya existe, establece `disable_start_screen` en `true` dentro de su sección `[loader]` existente, y `hide_console` en `true` dentro de su sección `[console]` existente. Conserva todas las demás entradas. Si el archivo no existe, copia la plantilla `UserData/Loader.cfg` incluida con la compilación, o `configuration/Loader.cfg` desde el código fuente. Nunca sustituyas un Loader.cfg existente por la plantilla completa.

```ini
[loader]
disable_start_screen = true

[console]
hide_console = true
```

El mod no restablece estas opciones cada vez que se inicia. Puedes volver a cambiar manualmente cualquiera de los dos valores a false para solucionar problemas. Si la desinstalación conserva una instalación compartida de MelonLoader, solo restaura los indicadores de destino establecidos por el instalador que no se hayan modificado y conserva los demás cambios. Si elimina su propia instalación de MelonLoader que ya no se utiliza, también elimina los archivos conocidos Loader.cfg y MelonPreferences.cfg.

## Documentación

- [Guía del usuario de juegos y mods (inglés)](BopItAccess-user-guide.html) — un tutorial de controles, configuraciones, menús y modos de juego para principiantes.
- [Guía del usuario en japonés (日本語)](../ja/BopItAccess-user-guide.html). Otras guías traducidas están disponibles en las carpetas de idiomas en [`documentation/`](../).
- [Guía detallada de funciones y control](README.txt). Su sección de instalación describe los ZIP de instalación preparados localmente; este repositorio GitHub proporciona únicamente la fuente.
- [Historial de construcción técnica](BopItAccess-build-history.html).
- [Revisión del código para preparar la publicación](BopItAccess-release-review.html) — problemas resueltos, archivos revisados, resultados de compilación y límites restantes.
- [Flujo de trabajo de Git para este proyecto](GIT-WORKFLOW.md).
- [Avisos de terceros](THIRD-PARTY-NOTICES.txt).

Las copias traducidas de los documentos anteriores están en [`documentation/`](../) bajo el código de cada idioma compatible. Su fuente está en inglés; `scripts/translate_documents.py` puede regenerar los borradores traducidos automáticamente después de cambios en la fuente.

## ¿Qué puede venir después?

Este proyecto está esencialmente completo y no se planean contenidos ni funciones importantes. Sin embargo, este mod se mantendrá y actualizará activamente con el tiempo según sea necesario, y los comentarios de los jugadores impulsarán estas mejoras. El trabajo potencial futuro incluye revisiones adicionales y correcciones de errores, refinamiento del código y mejoras continuas en la capacidad de respuesta del habla. Prism crea una posible ruta a otras plataformas en el futuro, pero este mod actualmente solo admite Windows x64. El repositorio del proyecto es el lugar para seguir el desarrollo posterior.

## Nota de transparencia de IA

Este mod se ha creado mediante « vibe coding ». Todo el código fue generado e investigado completamente por inteligencia artificial, con una comprensión humana técnica limitada de su arquitectura subyacente. Utilice este mod bajo su propia responsabilidad.

Dicho esto, cada característica de modificación y decisión de diseño fue escrita y aprobada por humanos. Las pruebas nunca fueron automatizadas; fueron realizadas cuidadosa y exhaustivamente por jugadores y evaluadores humanos reales.

Tenga en cuenta: el texto y la documentación multilingües fueron generados por AI y no han sido revisados por hablantes nativos. Es de esperar una alta imprecisión en la traducción. Sin codificación agente, este proyecto no existiría. ¡Gracias por darle una oportunidad!

## gracias

A aquellos que jugaron probaron este mod antes de su lanzamiento y ayudaron a llevarlo a donde está ahora, gracias. Todos sabéis quién sois. A los jugadores que ofrecen comentarios, prueban el mod por primera vez o creen en mí y en este proyecto, gracias. Su apoyo me motiva a seguir haciendo cosas en un mundo que puede parecer loco y profundamente defectuoso. Espero que este proyecto te facilite disfrutar del juego y jugar con otros. Muchas gracias a todos. Disfruta Bop It!

— Christopher Shaw

## Licencias

Aún no se ha seleccionado una licencia para la fuente Bop It Access. Prism tiene licencia propia; ver el [avisos de terceros](THIRD-PARTY-NOTICES.txt). Bop It! y sus bienes pertenecen a sus respectivos dueños y no están incluidos aquí.


## Versión preliminar 0.2.1 del instalador de Windows

Cierra Bop It!, abre el instalador y acepta la solicitud de permisos de administrador de Windows. El instalador te da la bienvenida, busca el juego en las bibliotecas de Steam de todas las unidades disponibles e intenta traer su ventana al primer plano. Comprueba la carpeta del juego que se muestra; utiliza Browse si necesitas elegir otra carpeta. La tecla Tab permite desplazarse entre los controles. El registro de estado es un campo de texto de solo lectura: sitúa el foco en él para revisar los mensajes con las teclas de dirección, seleccionar texto o copiarlo.

El instalador 0.2.1 intenta durante un periodo breve y limitado llevar su ventana al primer plano y dar el foco del teclado al campo de la carpeta del juego. Windows aún puede rechazarlo; usa Alt+Tab para pasar al instalador y Alt+G para enfocar ese campo.

La casilla Show advanced está desmarcada al abrir el instalador. Al marcarla, aparecen Install alpha, Save diagnostics y Copy diagnostics. Install descarga la última versión pública de GitHub cuando existe. Todavía no hay ninguna versión pública, por lo que quienes realizan pruebas necesitan actualmente Show advanced e Install alpha. La opción alfa pide confirmación, descarga el código fuente más reciente y lo compila en tu ordenador. Update aparece cuando se encuentra una versión pública más reciente para una copia instalada.

Los mensajes de estado explican con un lenguaje sencillo qué se está descargando, instalando o finalizando. Una única barra muestra el progreso estimado de toda la instalación, sin volver a empezar con cada descarga o archivo. Avanza en incrementos de cinco puntos porcentuales; algunas fases de preparación pueden tardar sin que se vea ningún cambio. El mensaje de bienvenida, el aviso de una nueva actualización disponible y la confirmación de que se han copiado los datos de diagnóstico se envían al lector de pantalla mediante las notificaciones de accesibilidad de Windows. Que se lean en voz alta depende del lector de pantalla y de su compatibilidad con las notificaciones de Windows.

El instalador 0.2.1 nunca inicia Bop It! durante la instalación. La opción alfa reutiliza archivos locales de compilación compatibles o prepara archivos temporales a partir de tu propia copia instalada del juego mientras permanece cerrado. Después, el instalador coloca MelonLoader en la carpeta del juego y añade inmediatamente Mods/BopItAccess.dll, seguido de Prism, la configuración, toda la documentación y los componentes necesarios para la desinstalación. Espera al mensaje de éxito y, cuando estés listo, inicia el juego tú mismo desde Steam.

Tras una instalación completada correctamente, aparece Play Bop It! The Video Game. Actívalo para iniciar el juego tú mismo desde Steam cuando estés listo. El instalador nunca inicia el juego automáticamente durante la instalación.

Una versión compilada necesita el entorno de ejecución de .NET 6 para Windows x64, no un SDK de desarrollo. Se reutilizan los entornos de ejecución completos que ya estén instalados. Si falta el entorno de ejecución, se descarga de Microsoft y se coloca en MelonLoader/Dependencies/dotnet. Install alpha también necesita un SDK de .NET compatible y el paquete de destino de .NET 6: se reutiliza un SDK existente o se instala el SDK oficial de Microsoft para todo el sistema. El instalador no crea ninguna carpeta nueva para el SDK en la carpeta raíz del juego. MelonLoader 0.7.3 Open-Beta y Prism 0.18.3 proceden de sus versiones oficiales. Los componentes compartidos de Microsoft .NET y los SDK permanecen instalados tras cancelar o desinstalar.

Quit cierra el instalador. Si la instalación sigue en curso, pregunta si quieres cancelarla y deshacerla antes de cerrar; Keep open permite continuar con normalidad. Si la instalación termina mientras decides, el diálogo se actualiza para indicar que ha finalizado y Quit no deshace la instalación completada. Una vez que comienza la eliminación, la desinstalación termina de forma segura antes de salir. Abort también pide confirmación y revierte los cambios realizados en los archivos del juego durante este intento. Si cancelas durante la instalación de Microsoft .NET, se espera a que la instalación de ese componente compartido termine de forma segura.

Tras confirmar Uninstall, elige Uninstall for me o Uninstall for everyone. Ambas opciones eliminan los archivos compartidos del mod de esta carpeta del juego, por lo que el mod dejará de estar disponible para cualquier persona que utilice esa instalación. La elección determina de quién se eliminan las preferencias guardadas del mod en Windows: solo de la cuenta que solicita la operación o de todos los perfiles locales de Windows, incluidos aquellos que tienen la sesión cerrada. Se conservan las preferencias del juego original. El SDK de .NET permanece instalado.

Cuando el instalador elimina su propia instalación de MelonLoader y ningún otro mod la necesita, también elimina los archivos conocidos Loader.cfg y MelonPreferences.cfg y las carpetas vacías Plugins, UserLibs y UserData. Se eliminan la configuración de Bop It Access, los registros conocidos, las guías y los archivos del instalador. Se protegen los demás mods, los archivos compartidos del cargador que ya existían y los archivos no reconocidos. Esto también significa que un archivo desconocido puede hacer que una carpeta permanezca; el instalador lo indica en los datos de diagnóstico en lugar de eliminar datos ajenos.

El instalador permanece abierto tras la desinstalación para que puedas revisar el resultado, guardar los datos de diagnóstico o instalar de nuevo. Elige Quit cuando termines. El programa auxiliar de desinstalación en ejecución y los archivos automáticos de diagnóstico se eliminan después de cerrar la ventana. Una reinstalación en la misma ventana inicia un nuevo registro de instalación; la limpieza aplazada no puede eliminar la nueva instalación.

Aplicaciones instaladas de Windows utiliza la misma confirmación, elección de preferencias y limpieza. El instalador proporciona BopItAccess-uninstall.ps1 en la carpeta del juego como acceso directo al desinstalador instalado; las futuras compilaciones del código fuente también incluyen este script entre sus archivos de salida. Copiar manualmente el script no instala el propio desinstalador. En una instalación manual antigua sin registro de propiedad, el instalador elimina los archivos del mod que puede identificar y conserva los archivos compartidos cuyo origen no puede determinar.

Si la limpieza no puede terminar de forma segura, el instalador lo explica y conserva la información necesaria para volver a intentarlo. En una instalación gestionada, su entrada de desinstalación de Windows y el punto de control de limpieza permanecen hasta que la eliminación se completa correctamente. Una copia manual antigua no dispone de un registro persistente de propiedad; vuelve a intentar las operaciones señaladas en sus advertencias con el instalador abierto. No instales, actualices ni elimines el mod mientras Bop It! esté en ejecución.

### Atajos de teclado del instalador

| Acción | Atajo de teclado | Función |
| --- | --- | --- |
| Carpeta del juego | Alt+G | Situar el foco en el campo de la carpeta del juego. |
| Browse | Alt+B | Elegir la carpeta del juego. |
| Install | Alt+I | Instalar la última versión pública, cuando esté disponible. |
| Install alpha | Alt+A | Confirmar y compilar el código fuente más reciente; visible con Show advanced. |
| Update | Alt+U | Instalar una versión pública más reciente cuando se ofrezca. |
| Play Bop It! The Video Game | Alt+P | Iniciar el juego desde Steam; disponible tras una instalación completada correctamente. |
| Uninstall | Alt+N | Confirmar la eliminación y elegir de quién se eliminan las preferencias del mod en Windows. |
| Abort | Alt+R | Confirmar la cancelación de la instalación actual. |
| Registro de estado | Alt+L | Situar el foco en los mensajes de estado de solo lectura cuyo texto se puede seleccionar. |
| Show advanced | Alt+V | Mostrar u ocultar la instalación alfa y las herramientas de diagnóstico. |
| Save diagnostics | Alt+D | Guardar y seguir registrando toda la sesión de diagnóstico; visible con Show advanced. |
| Copy diagnostics | Alt+C | Copiar la instantánea completa de diagnóstico; visible con Show advanced. |
| Quit | Alt+Q | Cerrar, gestionando la cancelación de forma segura si hay una operación en curso. |

### Uso de un mando en el instalador

El instalador admite mandos de tipo Xbox y otros mandos que Windows expone mediante XInput. Sus controles son independientes de los controles del juego que se pueden reasignar. La cruceta o la palanca izquierda permiten desplazarse entre los controles; cuando un campo de texto tiene el foco, las direcciones sirven para revisar su texto. Los botones superiores siempre desplazan el foco al control anterior o siguiente que pueda recibirlo. A activa el botón o la casilla que tenga el foco. La entrada del mando solo se procesa mientras este instalador o uno de sus propios diálogos esté en primer plano.

B vuelve atrás o cancela un diálogo; en la ventana principal del instalador, solicita cancelar una instalación en curso y, en los demás casos, realiza la acción de Quit. Start realiza la acción de Quit en la ventana principal y vuelve atrás en un diálogo. Y activa o desactiva Show advanced en la ventana principal. En el registro de estado u otro campo de texto del instalador, la cruceta o la palanca izquierda actúa como las flechas: Izquierda/Derecha avanza por caracteres y Arriba/Abajo por líneas. Mantén LT como Ctrl: Izquierda/Derecha avanza por palabras y Arriba/Abajo por párrafos. Mantén RT como Mayús para ampliar la selección; mantén LT y RT juntos para seleccionar palabras o párrafos. X copia únicamente el texto seleccionado; selecciona primero la parte que quieras. Ctrl+C en el teclado sigue copiando la selección. Si no hay texto seleccionado, el instalador también envía notificaciones accesibles del carácter, palabra, línea o párrafo en la posición del cursor. El instalador envía una confirmación accesible al copiar texto e informa si no hay selección o falla la copia. Que se lea en voz alta depende de la compatibilidad de tu lector de pantalla con las notificaciones de Windows. La navegación con mando en los diálogos nativos de Windows para elegir carpetas y guardar archivos todavía requiere verificación humana. El teclado sigue disponible para introducir una carpeta o un nombre de archivo. Esta implementación no admite mandos sin compatibilidad con XInput.

### Diagnóstico del instalador

Show advanced muestra Save diagnostics (Alt+D) y Copy diagnostics (Alt+C). Los registros automáticos UTF-8 se guardan localmente en %ProgramData%\BopItAccess\diagnostics. Save diagnostics escribe toda la sesión actual en el archivo .log o .txt que elijas y continúa registrando hasta que se cierra el instalador; Copy diagnostics copia una instantánea y proporciona una confirmación accesible. Guarda antes de una prueba de instalación o desinstalación para que tu registro se conserve tras la limpieza de los registros automáticos. Aquí se guardan los detalles técnicos de los archivos, las descargas, el compilador y los errores, aunque el campo de estado utiliza mensajes más breves. No se sube nada. Los registros pueden contener nombres de usuario de Windows y rutas completas: revísalos antes de compartirlos. Las copias exportadas expresamente permanecen tras la desinstalación.

En el primer inicio manual tras instalar MelonLoader, este puede descargar archivos auxiliares y preparar los ensamblados del juego. Deja pasar aproximadamente un minuto, o más en algunos sistemas. El mod no puede hablar hasta que MelonLoader lo cargue. Mantén el juego abierto y espera al anuncio de inicio de Bop It Access y, después, al anuncio de la pantalla de título, de bienvenida o del menú principal antes de utilizar los controles del juego.
