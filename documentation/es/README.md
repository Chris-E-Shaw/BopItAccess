# Bop It Access

Bop It Access es un mod de accesibilidad no oficial para la versión Windows Steam de **Bop It!**. Utiliza MelonLoader y [Prism](https://github.com/ethindp/prism) para agregar comentarios de voz y braille a menús y pantallas de juegos. Las características actuales incluyen una pantalla de bienvenida de primera ejecución, una guía del usuario en el juego, títulos hablados y pantallas de pausa, configuraciones y controles, selección de canciones, puntuaciones finales y tablas de clasificación, logros, créditos, sugerencias de botones, texto tutorial a pedido con asignaciones de control actuales antes de una ronda y descripciones de las cuatro etapas. La versión 0.9.0 utiliza Prism para salida de voz y braille. El mod sigue el idioma seleccionado del juego e incluye una guía para cada idioma que ofrece el juego.

## Estado del proyecto

Este proyecto se encuentra en desarrollo inicial. Este repositorio contiene código fuente y documentación técnica. **Aún no hay compilaciones compiladas ni versiones GitHub aquí.** Para usar el mod de este repositorio, compílelo desde el código fuente y proporcione el tiempo de ejecución Prism que se describe a continuación.

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

Las acciones del juego usan sus traducciones oficiales. Shapes, Space, City y Office conservan sus nombres de escenario en inglés. Para OneCore o SAPI, elija en Configuración del mod una voz instalada para el idioma del juego si la voz predeterminada no suena bien. Los cuatro controles de voz muestran el nombre del motor activo, OneCore o SAPI, incluso en el modo Auto. Solo aparecen los controles compatibles; con otras salidas se ocultan. Cada motor guarda por separado la voz, el volumen, la velocidad y el tono.

## Documentación

- [Guía del usuario de juegos y mods (inglés)](BopItAccess-user-guide.html) — un tutorial de controles, configuraciones, menús y modos de juego para principiantes.
- [Guía del usuario en japonés (日本語)](../ja/BopItAccess-user-guide.html). Otras guías traducidas están disponibles en las carpetas de idiomas en [`documentation/`](../).
- [Guía detallada de funciones y control](README.txt). Su sección de instalación describe los ZIP de instalación preparados localmente; este repositorio GitHub proporciona únicamente la fuente.
- [Historial de construcción técnica](BopItAccess-build-history.html).
- [Flujo de trabajo de Git para este proyecto](GIT-WORKFLOW.md).
- [Avisos de terceros](THIRD-PARTY-NOTICES.txt).

Las copias traducidas de los seis documentos anteriores se encuentran en [`documentation/`](../) debajo de cada código de idioma admitido. La fuente para ellos es el inglés; `scripts/translate_documents.py` puede regenerar los borradores traducidos automáticamente después de cambios de fuente.

## Transparencia de la IA

Christopher Shaw dirige este proyecto y evalúa su accesibilidad en el juego. Los modelos OpenAI Codex han ayudado con la investigación, el código y la documentación. Los mensajes de confirmación publicados incluyen un `Co-authored-by` tráiler que identifica el modelo que contribuyó a cada cambio; Los créditos históricos se compararon con los registros de sesión de este proyecto. El historial de compilación anterior se reconstruyó a partir de archivos fuente guardados en lugar de registrarse como confirmaciones en ese momento. Las contribuciones asistidas por IA pueden contener errores y deben revisarse antes de su uso.

## Licencias

Aún no se ha seleccionado una licencia para la fuente Bop It Access. Prism tiene licencia propia; ver el [avisos de terceros](THIRD-PARTY-NOTICES.txt). Bop It! y sus bienes pertenecen a sus respectivos dueños y no están incluidos aquí.
