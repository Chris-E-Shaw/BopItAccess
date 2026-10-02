# Flujo de trabajo de Git para Bop It Access

Git mantiene un historial de cambios en el código fuente y los documentos del proyecto. Una **confirmación** es una instantánea con nombre que puede inspeccionar o regresar. GitHub publica este historial fuente para que otros puedan leer el código y crear el mod ellos mismos. El repositorio no contiene archivos mod compilados ni versiones GitHub.

## Sobre la historia existente

Los archivos fuente para las compilaciones. `v0.1.0` a través de `v0.6.12` se importaron como 37 confirmaciones de Git sucesivas. Cada confirmación describe los cambios de fuente utilizando la entrada correspondiente en [BopItAccess-build-history.html](BopItAccess-build-history.html). Estas confirmaciones se crearon durante la importación de Git, por lo que sus marcas de tiempo de Git **no** son las fechas de compilación originales. Sus sujetos describen los cambios sin números de versión; el documento del historial de compilación registra qué instantánea de origen pertenece a cada versión.

Los archivos ZIP, las DLL compiladas y los resultados de compilación temporales permanecen fuera del historial fuente de Git. La página del historial de compilación enlaza con las confirmaciones de origen GitHub correspondientes. Las etiquetas de versión existentes permanecen locales y no forman parte de la publicación inicial GitHub. Aún no se han publicado etiquetas de versión GitHub ni lanzamientos.

Las confirmaciones de Git utilizan la dirección sin respuesta GitHub de Christopher Shaw como autor. Las confirmaciones escritas con Codex también incluyen un `Co-authored-by` tráiler nombrando el modelo que contribuyó al trabajo. Los registros de sesión identifican GPT-6 Luna para la primera compilación histórica y GPT-6 Sol para las siguientes 36. Si el modelo cambia para una confirmación posterior, use su nuevo nombre en el avance de esa confirmación.

## Comandos útiles

Abra PowerShell en este directorio de proyecto, luego ejecute:

```powershell
git status                         # See changed, added, and untracked files
git diff                           # See changes that have not been staged
git log --oneline                   # Browse source commits
git show --stat HEAD~1             # See files changed in the preceding commit
```

Estos comandos sólo inspeccionan el repositorio; no cambian el mod ni el juego instalado.

## Para cada construcción futura

1. Realice los cambios de origen y elija el siguiente número de versión de compilación.
2. Construye el mod y prepara los archivos locales como de costumbre. Incluir todo el `documentation` carpeta, con la guía en inglés y cada subcarpeta de idioma traducido, en cada archivo de instalación. Copie esa carpeta en la instalación del juego cuando instale una compilación. La guía del juego lee el HTML del idioma actual del juego en cada apertura. Inspeccione el resultado antes de registrar la construcción como completa. Los archivos compilados permanecen fuera del GitHub.
3. correr `git status` y `git diff`. Compruebe qué archivos cambiaron. Organice los cambios previstos en la fuente y la documentación y luego revíselos con `git diff --cached`.
4. Cree una confirmación de fuente descriptiva sin un número de versión en su asunto. Incluir un `Co-authored-by` remolque con el nombre del modelo real cuando Codex escribió la confirmación. Por ejemplo, `git commit -m "feat(speech): add example setting" -m "Co-authored-by: MODEL NAME <noreply@openai.com>"`; reemplazar `MODEL NAME` con el modelo utilizado para ese compromiso.
5. Agregue una entrada para la compilación a `BopItAccess-build-history.html`, utilizando el formato de estilo de confirmación existente. Describa el cambio real, su motivo y cualquier limitación relevante, y vincule la confirmación de origen del paso 4. Actualice las copias traducidas correspondientes antes del empaquetado. Confirme el historial actualizado con el mismo autor y un avance preciso del coautor. Actualice la carpeta de documentación local y archive si el archivo de historial ya se copió en ellos.
6. Publicar confirmaciones tanto de origen como de historial con `git push origin main` cuando esté listo. Esto empuja sólo la rama; no envía etiquetas de versión local ni crea versiones GitHub.

Los trabajos pequeños que no producen una compilación pueden tener su propio compromiso. La siguiente confirmación de compilación puede seguirla. Mantenga los registros personales, las instalaciones de juegos, los archivos binarios generados y otros archivos específicos de la máquina fuera de las confirmaciones. Si las versiones GitHub resultan útiles más adelante, decida las etiquetas y las descargas compiladas en ese momento.

Git no carga automáticamente nuevos trabajos. Después de cada confirmación local, presiónelo deliberadamente cuando esté listo para que otras personas lo vean.
