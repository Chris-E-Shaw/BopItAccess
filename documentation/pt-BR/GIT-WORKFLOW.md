# Fluxo de trabalho do Git para Bop It Access

O Git mantém o histórico do código-fonte e da documentação deste projeto. Um **commit** é um retrato com nome que você pode examinar ou recuperar. O GitHub publica esses commits para que outras pessoas possam ler as alterações e compilar o projeto por conta própria. Um commit não cria automaticamente uma versão pública.

## Sobre o histórico existente

As primeiras 37 compilações do código-fonte, de `0.1.0` a `0.6.12`, foram importadas como commits separados usando os arquivos de código disponíveis e suas notas originais de alterações. Os carimbos de data e hora do Git registram a importação, não as datas das compilações originais. Os trabalhos posteriores são registrados diretamente em commits do código-fonte. Git e GitHub são agora o histórico de alterações do projeto; não se mantém mais um documento separado do histórico de compilações.

O endereço sem resposta do GitHub de Christopher Shaw é usado para o autor dos commits. Os commits com assistência de IA incluem uma linha `Co-authored-by` indicando o modelo que realmente contribuiu. Os registros das sessões identificam GPT-6 Luna na primeira compilação histórica e GPT-6 Sol nas 36 seguintes. Use o nome atual do modelo que contribuiu nos commits futuros.

DLLs compiladas, instaladores, ZIPs de lançamento, assemblies gerados do jogo, registros pessoais e arquivos temporários de compilação ficam fora do histórico de código-fonte do Git. Tags de versão locais não são publicadas automaticamente. Criar uma versão no GitHub é uma etapa separada e deliberada.

## Comandos úteis

Abra o PowerShell no repositório e execute:

```powershell
git status                 # See changed, added and untracked files
git diff                   # Inspect changes that are not staged
git log --oneline           # Browse commits
git show --stat HEAD~1      # Inspect the previous commit's changed files
```

`git status` mostra arquivos alterados, adicionados e não rastreados. `git diff` mostra alterações que não estão preparadas. `git log --oneline` permite consultar os commits. `git show --stat HEAD~1` mostra os arquivos alterados no commit anterior.

Esses comandos consultam o repositório sem alterar o mod instalado ou o jogo.

## Para cada alteração futura

1. Faça as alterações pretendidas no código-fonte e na documentação. Para uma nova compilação, atualize a versão.
2. Atualize todos os guias traduzidos afetados. Mantenha a documentação dos jogadores e os avisos de licença junto dos arquivos compilados; READMEs para desenvolvedores e este fluxo de trabalho não fazem parte das versões para jogadores.
3. Compile quando a alteração exigir um novo binário e prepare os arquivos locais. Os testes de jogo são realizados por jogadores humanos quando solicitados; não afirme que o funcionamento foi verificado apenas por ter compilado.
4. Execute `git status` e `git diff`. Adicione os arquivos pretendidos à área de preparação e depois examine `git diff --cached`. Não inclua binários gerados, registros privados nem referências do jogo nas alterações preparadas.
5. Crie um commit descritivo cujo título não tenha número de versão. No corpo, explique o que mudou e por quê, além das verificações e limitações relevantes. Inclua o modelo real de IA em uma linha de coautor quando ele tiver contribuído:

   ```text
   Co-authored-by: MODEL NAME <noreply@openai.com>
   ```

   Substitua `MODEL NAME` pelo modelo que escreveu o trabalho. Mantenha Christopher Shaw como autor, usando `336230252+Chris-E-Shaw@users.noreply.github.com`.
6. Quando as alterações estiverem prontas para publicação, execute `git push origin main`. Isso publica os commits da branch sem enviar tags locais nem criar uma versão.

Uma alteração coerente pode incluir seu código-fonte e sua documentação no mesmo commit. Commits separados continuam úteis para alterações independentes. O Git não envia o trabalho automaticamente: envie de forma deliberada quando estiver pronto para que outras pessoas possam ler.
