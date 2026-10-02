# Fluxo de trabalho Git para Bop It Access

O Git mantém um histórico de alterações no código-fonte e nos documentos do projeto. Um **commit** é um instantâneo nomeado que você pode inspecionar ou retornar. GitHub publica este histórico de origem para que outros possam ler o código e construir o mod por conta própria. O repositório não contém arquivos mod compilados ou versões GitHub.

## Sobre a história existente

Os arquivos de origem para compilações `v0.1.0` através `v0.6.12` foram importados como 37 commits sucessivos do Git. Cada commit descreve as alterações de origem usando a entrada correspondente em [BopItAccess-build-history.html](BopItAccess-build-history.html). Esses commits foram criados durante a importação do Git, portanto, seus carimbos de data/hora do Git **não** são as datas de compilação originais. Seus assuntos descrevem as alterações sem números de versão; o documento histórico de construção registra qual snapshot de origem pertence a cada versão.

Arquivos ZIP liberados, DLLs compiladas e saída de compilação temporária ficam fora do histórico de origem do Git. A página do histórico de construção está vinculada aos commits de origem GitHub correspondentes. As tags de versão existentes permanecem locais e não fazem parte da publicação inicial GitHub. Nenhuma tag de versão GitHub ou lançamento foi publicada ainda.

Os commits do Git usam o endereço sem resposta GitHub de Christopher Shaw como autor. Os commits escritos com Codex também incluem um `Co-authored-by` trailer nomeando o modelo que contribuiu para a obra. Os registros de sessão identificam GPT-6 Luna para a primeira compilação histórica e GPT-6 Sol para as 36 seguintes. Se o modelo mudar para um commit posterior, use seu novo nome no trailer desse commit.

## Comandos úteis

Abra PowerShell neste diretório do projeto e execute:

```powershell
git status                         # See changed, added, and untracked files
git diff                           # See changes that have not been staged
git log --oneline                   # Browse source commits
git show --stat HEAD~1             # See files changed in the preceding commit
```

Esses comandos inspecionam apenas o repositório; eles não alteram o mod ou o jogo instalado.

## Para cada construção futura

1. Faça as alterações na origem e escolha o próximo número de versão de compilação.
2. Construa o mod e prepare os arquivos locais normalmente. Incluir todo `documentation` pasta, com o guia em inglês e todas as subpastas de idiomas traduzidos, em cada arquivo de instalação. Copie essa pasta para a instalação do jogo ao instalar uma compilação. O guia do jogo lê o HTML da linguagem atual do jogo em cada abertura. Inspecione o resultado antes de registrar a construção como concluída. Os arquivos compilados permanecem fora de GitHub.
3. Corre `git status` e `git diff`. Verifique quais arquivos foram alterados. Prepare as alterações pretendidas na fonte e na documentação e, em seguida, revise-as com `git diff --cached`.
4. Crie um commit de origem descritivo sem um número de versão no assunto. Incluir um `Co-authored-by` trailer com o nome real do modelo quando Codex escreveu o commit. Por exemplo, `git commit -m "feat(speech): add example setting" -m "Co-authored-by: MODEL NAME <noreply@openai.com>"`; substituir `MODEL NAME` com o modelo usado para esse commit.
5. Adicione uma entrada para a compilação `BopItAccess-build-history.html`, usando o formato de estilo de commit existente. Descreva a mudança real, seu motivo e quaisquer limitações relevantes e vincule o commit de origem da etapa 4. Atualize as cópias traduzidas correspondentes antes de empacotar. Confirme o histórico atualizado com o mesmo autor e o trailer preciso do coautor. Atualize a pasta de documentação local e arquive se o arquivo de histórico já tiver sido copiado para eles.
6. Publique os commits de origem e de histórico com `git push origin main` quando estiver pronto. Isso empurra apenas o branch; ele não envia tags de versão local nem cria versões GitHub.

Pequenos trabalhos que não produzem uma compilação podem ter seu próprio commit. O próximo commit de compilação pode segui-lo. Mantenha registros pessoais, instalações de jogos, binários gerados e outros arquivos específicos da máquina fora dos commits. Se as versões GitHub se tornarem úteis posteriormente, decida as tags e os downloads compilados naquele momento.

O Git não carrega automaticamente novos trabalhos. Após cada commit local, empurre-o deliberadamente quando estiver pronto para que outras pessoas vejam.
