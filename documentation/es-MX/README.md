# Bop It Access

Bop It Access es un mod de accesibilidad no oficial para la versión Windows Steam de **Bop It!**. Utiliza MelonLoader y [Prism](https://github.com/ethindp/prism) para agregar comentarios de voz y braille a menús y pantallas de juegos. Las características actuales incluyen una pantalla de bienvenida de primera ejecución, una guía del usuario en el juego, títulos hablados y pantallas de pausa, configuraciones y controles, selección de canciones, puntuaciones finales y tablas de clasificación, logros, créditos, sugerencias de botones, texto tutorial a pedido con asignaciones de control actuales antes de una ronda y descripciones de las cuatro etapas. La versión 0.9.0 utiliza Prism para salida de voz y braille. El mod sigue el idioma seleccionado del juego e incluye una guía para cada idioma que ofrece el juego.

## Editar el archivo de configuración

Si un idioma desconocido, el audio demasiado alto o una voz problemática dificultan usar los menús, puedes cambiar la configuración fuera del juego. Después del inicio, el mod crea automáticamente UserData/BopItAccess.ini en la carpeta de Bop It!, usando tu configuración actual. Es un archivo de texto que puedes abrir con un editor como el Bloc de notas.

El archivo incluye idioma, volúmenes de música, efectos y voz, vibración, pantalla completa, resolución y latencia de audio del juego; preferencias de voz, braille, pistas y otras opciones del mod; perfiles de voz separados para OneCore y SAPI; y asignaciones de controles del juego y del mod destinados a los jugadores. Las resoluciones disponibles y las voces instaladas se muestran en los comentarios.

Cierra el juego antes de editar. Busca la sección adecuada y cambia el valor de la entrada existente, guarda el archivo y vuelve a abrir el juego. Los cambios se leen al iniciar, no de inmediato durante una sesión. Los cambios realizados en los menús actualizan el archivo automáticamente.

Los nombres de secciones y opciones permanecen en inglés en todos los idiomas. Se recomiendan On y Off para activar o desactivar opciones; también se aceptan True/False, Yes/No y 1/0. Los comentarios explican las opciones y los rangos. Una entrada ausente o no válida conserva la opción guardada correspondiente; los demás cambios válidos se aplican. Las asignaciones de controles duplicadas se rechazan.

Los comentarios y las entradas desconocidas se conservan. Si otro programa modifica el archivo mientras el juego está abierto, el mod deja de guardarlo por el resto de la sesión para proteger esos cambios. Cierra y vuelve a abrir el juego para aplicarlos. Puedes guardar una copia de respaldo antes de editar.

Si una voz falla, escribe Voice=System default en la sección OneCore o SAPI. Las voces OneCore usan nombre | idioma; SAPI acepta el nombre mostrado de una voz instalada o su identificador completo del Registro. El archivo enumera las opciones disponibles. OutputMode=Auto intenta usar un lector de pantalla compatible activo, después OneCore y finalmente SAPI.

El siguiente ejemplo restaura el inglés, un audio de juego más bajo y la voz automática con las voces predeterminadas del sistema. Cambia las entradas correspondientes que ya existen en tu archivo; este fragmento es una referencia, no otro bloque para agregar al final. Conserva las demás opciones.

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

Los archivos instalados se registran en un manifiesto de propiedad para que las actualizaciones conserven los archivos preexistentes y una instalación cancelada pueda revertir sus propios cambios. **Desinstalar** y **Aplicaciones instaladas** de Windows usan el mismo código de desinstalación. El archivo suministrado `installer/uninstall.ps1` abre una ventana accesible de desinstalación desde Aplicaciones instaladas. Después de una eliminación completamente correcta, elimina el lanzador de desinstalación, los registros de propiedad y la entrada de Windows. Se conservan otros mods existentes y archivos MelonLoader compartidos. La limpieza de instalaciones gestionadas y de instalaciones manuales antiguas cubre los registros conocidos del mod, incluido `Mods/BopItAccess.log.previous`, `UserData/BopItAccess.ini`, el antiguo `UserData/BopItAccess.ini.tmp`, y los residuos validados de `BopItAccess.ini.<GUID>.tmp` en `UserData`. Aquí `<GUID>` debe contener exactamente 32 caracteres hexadecimales sin guiones; no se eliminan archivos arbitrarios que coincidan con un comodín amplio.

En una copia antigua instalada manualmente sin manifiesto de propiedad, la desinstalación elimina los archivos Bop It Access identificables y conserva los archivos compartidos cuyo origen no puede demostrarse. La desinstalación también elimina solo los valores de preferencias `BopItAccess.*` de cada perfil de usuario local de Windows, incluidos los perfiles con sesión cerrada. Las preferencias del propio juego y el SDK .NET permanecen. Si Windows deniega el acceso o alguna otra fase no puede terminar con seguridad, el instalador informa de limpieza incompleta. En una copia gestionada por el instalador, su entrada de Windows, el lanzador de desinstalación y el punto persistente de recuperación siguen disponibles hasta que la limpieza termine correctamente, para reintentar los pasos pendientes. Una instalación manual antigua no tiene ese registro persistente de propiedad; sus avisos pueden reintentarse en el instalador abierto.

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

Leer posiciones de los menús. Esta opción guardada, activada de forma predeterminada, anuncia la posición del elemento dentro de su menú. Los comentarios uno a uno se guardan y están activados de forma predeterminada. Anuncia el color activo al inicio y cuando cambia. Al perder una vida, anuncia la cantidad restante. Al ganar una vida, anuncia el color del jugador y su nuevo total, por ejemplo «Verde, 3 vidas», para que ambos sepan quién fue más rápido. Estos anuncios se desactivan al desactivar los comentarios uno a uno. El límite de tres vidas del juego no cambia. Esta función solo actúa durante una partida uno a uno. Después de la última entrada de calibración, el mod dice inmediatamente «¡Listo!». Deja de golpear y espera el resultado medido. Si la calibración falla porque no se ha realizado ninguna entrada, dice «Calibración fallida».

**Dar formato al habla**: Hace más natural el texto en mayúsculas para voz y braille. En la guía del juego añade puntos suspensivos antes del número de línea si el texto termina sin puntuación. El texto visible no cambia. Desactívalo para recibir el texto tal como está escrito.

### MelonLoader ventanas de inicio

La plantilla Loader.cfg incluida oculta la pantalla de inicio y la consola separadas de MelonLoader. El instalador aplica estos dos valores antes de que abras el juego tú mismo. No omiten la pantalla de título del juego ni la bienvenida del mod.

Con el juego cerrado, abre `UserData/Loader.cfg` en la carpeta del juego. Si el archivo ya existe, establece `disable_start_screen` en `true` dentro de su sección `[loader]` existente, y `hide_console` en `true` dentro de su sección `[console]` existente. Conserva todas las demás entradas. Si el archivo no existe, copia la plantilla `UserData/Loader.cfg` incluida con la compilación, o `configuration/Loader.cfg` desde el código fuente. Nunca reemplaces un Loader.cfg existente con la plantilla completa.

```ini
[loader]
disable_start_screen = true

[console]
hide_console = true
```

El mod no restablece estas opciones en cada inicio. Puede cambiar manualmente cualquiera de los valores a falso si necesita las ventanas del cargador para solucionar problemas. La desinstalación del instalador restaura los valores originales solo mientras los valores verdaderos del instalador aún están presentes, preservando otras ediciones de configuración del cargador.

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

## Instalación y primer inicio

La versión preliminar 0.1.9 del instalador nunca abre Bop It! durante la instalación. Install descarga una versión pública compilada cuando existe. Install alpha descarga el código más reciente, pide confirmación y lo compila antes de copiar los archivos. Alpha reutiliza referencias locales completas o genera referencias temporales de compilación a partir de tu juego instalado, sin ejecutarlo. Después de preparar todo, el instalador coloca MelonLoader en la carpeta del juego e inmediatamente BopItAccess.dll en Mods. Luego termina los archivos de Prism, configuración, documentación y desinstalación. Espera el mensaje de éxito y abre el juego tú mismo mediante Steam cuando estés listo.

Una versión compilada necesita el entorno de ejecución Windows x64 de .NET 6, no un SDK de desarrollo. Se reutilizan los entornos completos existentes. Si falta, el instalador descarga el ZIP oficial Microsoft de .NET 6.0.36 y lo coloca en MelonLoader/Dependencies/dotnet, una ubicación compatible. Esos archivos se registran para reversión y desinstalación; se conservan si otros mods necesitan el cargador compartido. Install alpha también necesita un SDK compatible y el paquete de destino .NET 6. Reutiliza un SDK instalado o una carpeta dotnet anterior compatible; si hace falta, instala un SDK oficial Microsoft para todo el sistema. El SDK permanece después de desinstalar o cancelar. Esta versión no crea nuevas carpetas SDK dotnet en la raíz del juego. Obtiene MelonLoader 0.7.3 y Prism 0.18.3 oficiales cuando hacen falta. Las actualizaciones siguen usando GitHub. Abort pide confirmación y revierte los cambios de esta instalación en los archivos del juego.

En el primer inicio manual después de instalar MelonLoader, puede descargar archivos de apoyo y generar los ensamblados del juego. Espera aproximadamente un minuto, o más en algunos equipos. El mod no puede hablar hasta que MelonLoader termine de cargarlo. Deja abierto el juego y espera el anuncio de inicio de Bop It Access, y luego el de la pantalla de título o el menú, antes de usar los controles.

La versión preliminar 0.1.9 del instalador corrige una dependencia faltante de la herramienta de compilación que podía detener Install alpha. Revisa la lista de dependencias y los archivos de la herramienta antes de descargar o ejecutar las herramientas de ensamblados. El instalador sigue sin iniciar nunca el juego. Guarda los diagnósticos antes del próximo intento.
