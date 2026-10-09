# Flujo de trabajo de Git para Bop It Access

Git conserva el historial del código fuente y la documentación de este proyecto. Un **commit** es una instantánea con nombre que puedes consultar o recuperar. GitHub publica estos commits para que otras personas puedan leer los cambios y compilar el proyecto por sí mismas. Un commit no crea automáticamente una versión pública.

## Sobre el historial existente

Las primeras 37 compilaciones del código fuente, de `0.1.0` a `0.6.12`, se importaron como commits separados a partir de los archivos de código disponibles y sus notas originales de cambios. Sus marcas de tiempo en Git indican la importación, no las fechas de las compilaciones originales. El trabajo posterior se registra directamente en los commits del código fuente. Git y GitHub son ahora el historial de cambios del proyecto; ya no se mantiene un documento separado con el historial de compilaciones.

La dirección de GitHub sin respuesta de Christopher Shaw se usa como autor de los commits. Los commits realizados con ayuda de IA incluyen una línea `Co-authored-by` con el modelo que realmente contribuyó. Los registros de las sesiones identifican a GPT-6 Luna en la primera compilación histórica y a GPT-6 Sol en las 36 siguientes. Utiliza el nombre actual del modelo que contribuyó en los futuros commits.

Las DLL compiladas, instaladores, ZIP de publicación, ensamblados generados del juego, registros personales y archivos temporales de compilación quedan fuera del historial de código fuente de Git. Las etiquetas de versión locales no se publican automáticamente. Crear una versión en GitHub es un paso separado y deliberado.

## Comandos útiles

Abre PowerShell en el repositorio y ejecuta:

```powershell
git status                 # See changed, added and untracked files
git diff                   # Inspect changes that are not staged
git log --oneline           # Browse commits
git show --stat HEAD~1      # Inspect the previous commit's changed files
```

`git status` muestra archivos modificados, añadidos y sin seguimiento. `git diff` muestra cambios que no están preparados. `git log --oneline` permite consultar los commits. `git show --stat HEAD~1` muestra los archivos modificados en el commit anterior.

Estos comandos consultan el repositorio sin modificar el mod instalado ni el juego.

## Para cada cambio futuro

1. Haz los cambios previstos en el código fuente y la documentación. Para una nueva compilación, actualiza su versión.
2. Actualiza todas las guías traducidas afectadas. Mantén la documentación de los jugadores y los avisos de licencia junto a los archivos compilados; los README para desarrolladores y este flujo de trabajo no forman parte de las versiones para jugadores.
3. Compila cuando el cambio necesite un nuevo binario y prepara los archivos locales. Las pruebas del juego las realizan jugadores humanos cuando se solicitan; no afirmes que el funcionamiento se ha verificado solo por haber compilado.
4. Ejecuta `git status` y `git diff`. Añade los archivos previstos al área de preparación y después revisa `git diff --cached`. No incluyas binarios generados, registros privados ni referencias del juego entre los cambios preparados.
5. Crea un commit descriptivo cuyo título no tenga número de versión. En el cuerpo, explica qué cambió y por qué, así como las comprobaciones y limitaciones pertinentes. Incluye el modelo real de IA en una línea de coautor cuando haya contribuido:

   ```text
   Co-authored-by: MODEL NAME <noreply@openai.com>
   ```

   Sustituye `MODEL NAME` por el modelo que escribió el trabajo. Mantén a Christopher Shaw como autor, con `336230252+Chris-E-Shaw@users.noreply.github.com`.
6. Cuando los cambios estén listos para publicarse, ejecuta `git push origin main`. Esto publica los commits de la rama sin enviar las etiquetas locales ni crear una versión.

Un cambio coherente puede incluir el código fuente y la documentación en el mismo commit. Los commits separados siguen siendo útiles para cambios independientes. Git no sube el trabajo automáticamente: envíalo de forma deliberada cuando esté listo para que otros lo lean.
