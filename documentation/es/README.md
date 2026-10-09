# Bop It Access

Bop It Access es un mod de accesibilidad para personas ciegas, destinado a la versión de Steam para Windows x64 de **Bop It! The Video Game**. Utiliza [MelonLoader](https://github.com/LavaGang/MelonLoader) y [Prism](https://github.com/ethindp/prism) para ofrecer voz y braille en los menús y pantallas del juego, con controles y ajustes adicionales para una experiencia más cómoda.

Versiones actuales del código fuente: **mod 0.9.13** e **instalador 0.2.8**. La primera versión pública llegará próximamente.

## Funciones

- Voz para menús, ajustes, tutoriales, clasificaciones, logros, créditos, pantallas de pausa y resultados.
- Lectura a petición de puntuaciones, descripciones de escenarios y pistas que respetan las asignaciones actuales de los controles.
- Voz y una guía integrada en todos los idiomas que ofrece el juego.
- Salida para lectores de pantalla y braille, con OneCore y SAPI como opciones de voz del sistema.
- Nivel de detalle de la voz, tiempos y repeticiones de pistas ajustables, además de atajos de voz reasignables.
- Asignaciones adicionales de controles del juego, límite de fotogramas por segundo, controles de audio en segundo plano y un archivo de ajustes legible.
- Instalador accesible con teclado y mando para instalar, actualizar y desinstalar.

## Estado del proyecto

Las funciones principales están prácticamente completas. El proyecto se mantendrá según sea necesario, con los comentarios de los jugadores orientando las correcciones y mejoras. La plataforma compatible actualmente es Windows x64.

Este repositorio contiene código fuente y documentación. **Todavía no se ha publicado ninguna versión pública.** La próxima publicación ofrecerá `BopItAccess-Installer.exe` y un archivo compilado `BopItAccess-v1.0.zip`. Los archivos de código fuente ZIP y TAR.GZ generados automáticamente por GitHub contienen código, no un mod listo para instalar. Hasta la primera publicación, la opción **Mostrar opciones avanzadas > Instalar alfa** del instalador compila el código fuente más reciente de `main`.

Los commits de Git constituyen el historial del proyecto. Las primeras 37 compilaciones se importaron como versiones separadas del código fuente; las fechas de esos commits corresponden a la importación, no a las fechas de las compilaciones originales. Este repositorio no incluye archivos binarios compilados, archivos del juego ni ensamblados del juego generados por MelonLoader.

## Documentación

[Lee la guía de usuario en inglés](../../BopItAccess-user-guide.html) para conocer la instalación, las actualizaciones, la desinstalación, los controles, los ajustes, los menús y todos los modos de juego. La guía integrada usa automáticamente el idioma actual del juego.

## Requisitos

- Windows x64 y tu propia instalación de Steam adquirida legalmente de Bop It! The Video Game.
- **MelonLoader 0.7.3 Open-Beta**, x64. El desarrollo utiliza la compilación del juego con Unity 2022.3.50f1.
- El **entorno de ejecución .NET 6** para Windows x64, para ejecutar el mod.
- El archivo oficial **Prism v0.18.3** para Windows x64, `prism.dll`, instalado junto al ejecutable del juego.
- Para compilar el mod desde el código fuente: un SDK de .NET compatible con el **paquete de destino de .NET 6** y las referencias que MelonLoader genera a partir de tu propio juego.
- Para compilar el instalador desde el código fuente: el **SDK de .NET 10** en Windows.

El instalador obtiene sus dependencias de sus fuentes oficiales. Instalar una versión compilada no requiere un SDK de desarrollo; Instalar alfa sí lo requiere.

<a id="build-from-source"></a>
## Compilar desde el código fuente

1. Instala MelonLoader 0.7.3 Open-Beta en la carpeta del juego. Inicia el juego una vez, espera a que MelonLoader prepare sus archivos y después ciérralo. Las referencias generadas deberían estar en `MelonLoader\Il2CppAssemblies`, dentro de la carpeta del juego.
2. Descarga o clona este repositorio y abre PowerShell en su carpeta raíz.
3. Sustituye la ruta de ejemplo siguiente por la ubicación de tu juego y ejecuta:

   ```powershell
   $gameDir = 'C:\Program Files (x86)\Steam\steamapps\common\Bop It!'
   dotnet build .\src\BopItAccess.csproj -c Release "-p:BopItGameDir=$gameDir"
   ```

La DLL compilada es `src\bin\Release\net6.0\BopItAccess.dll`. El proyecto informa de las referencias del juego o del cargador que faltan antes de compilar. Si el SDK indica que falta el paquete de destino de .NET 6, instala un SDK que lo contenga. El archivo `NuGet.Config` del proyecto no configura fuentes de paquetes en línea.

La instalación alfa prepara sus referencias locales de compilación sin iniciar el juego. Estas referencias son archivos temporales necesarios para compilar; nunca se incluyen en los commits ni en una versión compilada del mod.

<a id="install-your-build"></a>
## Instalar tu compilación

Con el juego cerrado:

1. Copia el archivo `BopItAccess.dll` compilado a la carpeta `Mods` del juego. Crea esa carpeta si es necesario.
2. Descarga Prism v0.18.3 oficial para Windows x64 desde las [versiones de Prism](https://github.com/ethindp/prism/releases). Coloca `prism.dll` junto al ejecutable del juego.
3. Copia la carpeta `src\bin\Release\net6.0\documentation` de la compilación a la carpeta del juego, conservando todas sus subcarpetas de idiomas. Conserva los archivos de licencia de Prism correspondientes cuando distribuyas su archivo binario.
4. Inicia tu lector de pantalla si utilizas uno y después abre el juego mediante Steam. Espera al anuncio de inicio y después al anuncio del título, de bienvenida o del menú principal antes de usar los controles del juego.

La compilación del mod no descarga ni compila Prism. Si la voz no empieza, revisa `Mods\BopItAccess.log` en la carpeta del juego. Un envío correcto en el registro confirma que el mod envió texto; no demuestra que se haya oído el audio.

Para un ZIP de una versión compilada, copia **todo su contenido** a la carpeta del juego y combina las carpetas o reemplaza los archivos cuando se te pida. El ZIP contiene el mod, Prism, guías y avisos de licencia; MelonLoader y .NET se instalan por separado. Consulta la guía de usuario para las instrucciones completas de instalación manual.

## Editar los ajustes fuera del juego

Después del inicio, `UserData\BopItAccess.ini`, en la carpeta del juego, contiene ajustes legibles del juego y del mod, perfiles de voz y asignaciones de controles para los jugadores. Cierra el juego, abre el archivo en el Bloc de notas, modifica las entradas existentes y guárdalo. El mod lee los cambios en el siguiente inicio. Los comentarios explican las opciones y los rangos válidos.

Por ejemplo, establece `Language=en` en `[Game]` para recuperar el inglés, reduce `MusicVolume`, `SfxVolume` y `VoiceOverVolume`, o establece `Voice=System default` en `[OneCore]` o `[SAPI]` para sustituir una voz inadecuada. Establece `SpeechOutput=On` y `OutputMode=Auto` en `[Mod]` para recuperar la voz automática. Conserva las demás entradas; no añadas secciones duplicadas.

## Compilar el instalador

En Windows con el SDK de .NET 10, ejecuta:

```powershell
dotnet restore .\installer\BopItAccess.Installer.csproj --source https://api.nuget.org/v3/index.json
dotnet publish .\installer\BopItAccess.Installer.csproj -c Release -o .\build\installer --no-restore
```

El resultado es `build\installer\BopItAccess.Installer.exe`. Es un ejecutable autónomo para Windows x64, por lo que los usuarios no necesitan .NET 10 para ejecutarlo. El instalador suministrado no está firmado. La guía de usuario explica sus controles y los avisos de seguridad de Windows.

`scripts/package-mod.ps1` prepara un ZIP de publicación a partir de un mod ya compilado de la versión correspondiente. Incluye las guías, avisos y licencias que necesitan los jugadores. Los README para desarrolladores, las notas del flujo de trabajo de Git, las referencias generadas del juego y los archivos compilados del instalador se excluyen de ese ZIP. Preparar el paquete no compila el mod ni publica una versión en GitHub.

## Nota de transparencia sobre la IA

Este mod se ha creado mediante «vibe coding». Todo el código fue generado e investigado íntegramente por inteligencia artificial, con una comprensión técnica humana limitada de su arquitectura interna. Usa este mod bajo tu propia responsabilidad.

Dicho esto, todas las funciones y decisiones de diseño del mod fueron creadas y aprobadas por humanos. Las pruebas nunca fueron automatizadas; jugadores y probadores reales las realizaron con cuidado y de forma exhaustiva.

Ten en cuenta que los textos y la documentación multilingües fueron generados por IA y no han sido revisados por hablantes nativos. Cabe esperar importantes imprecisiones en las traducciones. Sin la programación mediante agentes de IA, este proyecto no existiría. ¡Gracias por darle una oportunidad!

## Licencia y avisos legales

El código fuente propio de Bop It Access y su documentación se distribuyen bajo la **[licencia MIT](../../LICENSE)**. Copyright © 2026 Christopher Shaw. Las dependencias mantienen sus propias licencias; la licencia MIT no cambia sus licencias ni concede derechos sobre los recursos del juego. Consulta [THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt) para los avisos de las dependencias.

Bop It Access es un proyecto no oficial creado por un aficionado. No está creado, aprobado ni respaldado por Hasbro, Alliance, el desarrollador/editor del juego, Valve, Microsoft, Unity, MelonLoader, Prism ni ningún fabricante de lectores de pantalla. **Bop It!** y sus personajes, ilustraciones, sonidos y marcas pertenecen a Hasbro y a sus respectivos titulares de derechos. Steam pertenece a Valve. Los demás nombres de productos, marcas y programas siguen siendo propiedad de sus respectivos titulares.

Debes obtener tu propia copia legal del juego. Este repositorio no incluye el juego ni sus recursos y no concede derechos sobre ellos. Para obtener información sobre los derechos del juego original, consulta el [sitio oficial de Bop It!](https://bopitthevideogame.com/) y su [página de Steam](https://store.steampowered.com/app/3214360/).

## Gracias

A quienes probaron este mod antes de su lanzamiento y ayudaron a que llegara hasta aquí: gracias. Ya sabéis quiénes sois. A los jugadores que ofrecen comentarios, prueban el mod por primera vez o creen en mí y en este proyecto: gracias. Vuestro apoyo me motiva a seguir creando en este mundo loco en el que vivimos. Espero que este proyecto os facilite disfrutar del juego y jugar con otras personas. Muchísimas gracias a todos. ¡Disfrutad de Bop It!

— Christopher Shaw
