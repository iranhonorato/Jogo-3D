# CLAUDE.md — Operação Guaxinim

Jogo 3D furtivo com câmera isométrica em Unity 6 (URP): um guaxinim rouba os itens de uma lista de compras em supermercados à noite e os leva até a toca, fugindo dos seguranças. Este arquivo diz **como trabalhar** e **em que ordem construir**. O design e os números ficam no GDD.

## Fontes de verdade

| Arquivo | Para quê |
| --- | --- |
| [docs/GDD_Operacao_Guaxinim.md](docs/GDD_Operacao_Guaxinim.md) | Design, valores iniciais e regras de trabalho (§0). **Em design e números, o GDD manda.** Leia a seção indicada antes de cada passo. |
| `docs/Projeto_3D_20262.docx.pdf` | Enunciado da disciplina: prazo, entrega e rubricas (resumo abaixo). |
| `docs/sprites_operacao_guaxinim.jpeg` | Referência de estilo. O GDD cita `docs/art/referencia_estilo.png`, que não existe: a referência é este arquivo. |
| [README.md](README.md) | Arquitetura, assemblies, padrões, convenções e créditos. |
| [skills/](skills/) | Skills de código: clean code e padrões de projeto de jogos (ver "Skills de código"). |

## O que a disciplina cobra

- **Entrega até 25/11/2026**, data rígida, −2 pontos por dia de atraso.
- Entregar: build **Windows** (obrigatório; o GDD também mira WebGL) na jam do itch.io (`projeto-jogo-3d-insper-20262`), repositório com créditos no README, **GDD atualizado** (documento vivo: mudanças de rumo vão para lá) e página do itch.io caprichada (como jogar, teclas, imagens ou vídeo).
- Nota = média de 4 rubricas: (1) controles, câmera e sensação 3D; (2) desempenho, fluidez e colisões; (3) modelos, materiais e texturas; (4) gerenciamento de estados e erros (começar, perder, vencer e reiniciar sem ficar preso).
- Código de terceiros com mais de 2 ou 3 linhas precisa de crédito no README.

As rubricas premiam acabamento, não quantidade de mecânicas: uma fase polida vale mais que várias cruas.

## Estado atual

- **Pronto:** Unity 6000.3.24f1 com URP; pacotes (Input System, Cinemachine 3, AI Navigation, uGUI/TMP, Test Framework); pastas de `Assets/_Project/`; assemblies `Guaxinim.Runtime`, `Guaxinim.EditorTools` e `Guaxinim.Tests.EditMode`; layers 6 a 11 (`Player`, `Guard`, `Obstacle`, `ShelfTop`, `NarrowGap`, `Interactable`); `.gitignore` e `.gitattributes`.
- **Adiantado a pedido do usuário (parte do passo 3.10):** modelos estáticos do guaxinim, do segurança e do Seu Zé em `Art/Models/Characters/`, gerados pelos scripts de `Tools/Blender/` e validados na Unity (escala, frente em +Z, textura ligada). Em 23/11 também foram gerados a câmera de segurança (`Art/Models/Hazards/`) e o kit modular (gôndolas de 2 m e 4 m, parede, piso e toca, em `Art/Models/Environment/`, com um atlas só), ainda sem conferência na Unity. Faltam rig e animações.
- **Prazo confirmado:** entrega em 25/11/2026, com cerca de 15 h de trabalho por dia em 23 e 24/11. O plano dos 2 dias: marcos 0 e 1 no dia 23; no dia 24, passos 2.1 a 2.6, 3.3 e 3.4 (alerta e câmeras), 3.5, 3.6 e 3.8, e a troca do greybox pela arte; no dia 25, build final e página do itch.io. Lixeiras, aspirador, consumíveis, cogumelo, save e as fases 2 e 3 ficam como extras.
- **Feito, falta testar no Play Mode (o usuário vai testar tudo junto mais adiante):**
  - 0.1: mapa `Player` com as ações da §3.2, classe gerada `Core/GameInput.cs` e `Core/InputReader` (eventos e troca para o mapa `UI` na pausa); `UI/Cancel` também aceita o Start do controle.
  - 0.2: WebGL em Gzip com Decompression Fallback. **Nome do produto e da empresa ficaram para depois**, a pedido do usuário.
  - 0.3: `BuildScript` (menu `Tools/Guaxinim/Build`). O build Windows abriu; falta abrir o WebGL.
  - 1.1: `PlayerData`, `RaccoonLocomotion` (testado), `RaccoonController` e o menu `Tools/Guaxinim/Greybox/Prefab do Guaxinim`.
  - 1.2: menu `Tools/Guaxinim/Greybox/Rigs de Câmera Isométrica` gera `Prefabs/Camera/CameraRig_Perspective` (FOV 25°) e `CameraRig_Orthographic` (tamanho 6), com Position Composer e lookahead. **O usuário ainda escolhe qual usar**; registrar a escolha no GDD §3.1.
  - 1.3: `Noise/` (`NoiseSystem` mediador estático, `NoiseListener`, `NoiseRadius` testado, `NoiseRingVFX` com pool no `NoiseRingSpawner`) e `Player/RaccoonNoiseMaker` (correr em intervalos, assobio de 8 m). Menu `Tools/Guaxinim/Greybox/Sistema de Som` gera `NoiseData`, o anel e o prefab `Prefabs/Systems/LevelSystems` (vai em toda fase). Rodar de novo o menu do guaxinim para ele ganhar o `RaccoonNoiseMaker`.
  - 1.4: `EnvironmentKitFactory` (menu `Kit de Cenário`): prefabs de piso, parede, gôndolas de 2 m e 4 m e toca em `Prefabs/Environment/`, **já com os modelos do Blender** (decisão para não ter que trocar o greybox depois) e BoxColliders; paredes e gôndolas na layer `Obstacle`, todos estáticos.
  - 1.5: `Guards/` (`GuardData`, `IGuardState`, `GuardAI`, `PatrolRoute` e `LookAround` testados, `States/PatrolState`) e `GuardFactory` (prefab com NavMeshAgent e o modelo `Char_Guard`). A Sandbox ganha um segurança com 4 waypoints.
  - 1.6: `Shared/IDetectable` e `Shared/DetectableRegistry` (os detectores encontram o guaxinim sem buscar na cena), `Player/PlayerState` mínimo, `Guards/VisionData` (SO próprio, para a câmera reaproveitar), `VisionMath` testado, `VisionCone` (raycast na layer `Obstacle`), `VisionConeMesh` (leque no chão recortado pelas paredes) e lanterna spot no `Socket_Flashlight`, sem sombras.
  - 1.7: `Guards/DetectionData`, `DetectionLevel` (lógica pura testada: 1 s a 9 m, até 3× mais rápido de perto, esvazia 0,35/s) e `DetectionMeter` (`OnFilled`), mais `UI/DetectionMeterIcon` (barra sobre a cabeça, amarelo ao vermelho).
  - 1.8: estados `SuspiciousState` e `SearchState` (base `InvestigateState`, Template Method), `ChaseState` (5,5 m/s, `LostSightTimer` testado) e `ReturnState`; `LookAroundPause` compartilhado. O `GuardAI` liga os sentidos: medidor cheio com o guaxinim à vista leva à Perseguição; som (`NoiseListener`) ou relance levam ao Desconfiado. Teste do balanço: guaxinim sem carga (6 m/s) mais rápido que o segurança (5,5 m/s).
  - 1.9: `PlayerState.Catch()`/`OnCaught` (idempotente), `Guards/CatchRule` testada (1 m na Perseguição), `Core/LevelFlow` + `LevelFlowData` (reinicia a fase em 1,5 s), `Core/SceneLoader` estático e `UI/CaughtMessageUI` ("Você foi pego!", com `Text` do uGUI por enquanto). **Decisão:** o estado da fase fica no `LevelFlow`, que vive na cena; o `GameManager` persistente entra só com pausa e menu (3.6 e 3.8), para não guardar estado da fase num singleton. Menu `Sistemas da Fase` gera o `LevelSystems` (som, `LevelFlow` e aviso).
  - 1.10: menu **`Tools/Guaxinim/Montar Fase 1`** cria `Scenes/Levels/Level_01_Mercadinho` (12 × 10 m, 2 corredores com um vão de 1,2 m entre as gôndolas, toca no fundo onde o guaxinim começa, Seu Zé patrulhando 4 waypoints), luz noturna com 4 luzes em tempo real (lua, 2 luzes da loja e a lanterna), NavMesh assado em asset e a cena como primeira do Build Profile. Prefab `SeuZe` (variante do segurança com o modelo dele). `LevelBuildKit` concentra o que a Sandbox e as fases têm em comum.
- **Ferramentas:** **`Tools/Guaxinim/Montar Sandbox de Teste`** gera todos os prefabs e monta na cena aberta um mercadinho de teste já ligado (guaxinim, câmera em perspectiva, LevelSystems, NavMesh assado). É o jeito de testar tudo no Play Mode. `Tools/Guaxinim/Vitrine de Modelos` mostra os modelos lado a lado.
- **Marco 2, passos 2.1 a 2.6 (branch `pedro2`, sem teste no Play Mode ainda):**
  - 2.1: `Shared/IInteractable`, `Items/ItemData`, `IItemCarrier`, `PickupItem`, `Player/RaccoonInteractor` (E no mais próximo da layer `Interactable`) e menu `Itens da Tabela` (19 `ItemData` da §7.8 e prefabs greybox na cor da categoria).
  - 2.2: `Inventory` com peso 10 e volume 6, `CarrySpeed` e `CarryCapacity` testados; velocidade e som de correr com carga.
  - 2.3: `RaccoonDropper` (Q larga o último, som de 3 m).
  - 2.4: `RaccoonBulkyLoad` (volume 4 ou mais: `Shared/ViewObstruction` esconde cones a mais de 6 m, `UI/BulkyVignetteUI`, `Physics.IgnoreLayerCollision` com `NarrowGap`) e prefab `NarrowGap_1.2m`, posto no vão entre as gôndolas da Fase 1.
  - 2.5: `ShakeMeter` testado e `FragileShake` (quebra o último frágil, som de 10 m e poça). `DetectionMeterIcon` virou `UI/WorldMeterIcon` (lê `Shared/IMeterValue`).
  - 2.6: `Core/LevelData`, `Items/ShoppingListProgress` testado, `ShoppingList`, `Den` (trigger na toca, entrega automática), `ShoppingListHighlighter` (contorno), vitória no `LevelFlow` (congela o tempo; o `SceneLoader` volta a 1) e `UI/LevelResultUI` provisória com "Jogar de novo". `LevelSystems` ganhou EventSystem com `InputSystemUIInputModule`.
  - 2.7: `Effects/` (`ConsumableEffect` base, `EnergyDrinkEffect`, `ProteinBarEffect`, `ActiveEffects` testado e os assets `ConsumableEffectData` com `CreateEffect()`), `Player/RaccoonEffects`; o `Inventory` usa o consumível na hora, soma o bônus de peso da proteína e tem `IsOverweight` (velocidade mínima de 40% e não pega nada).
  - 2.8: `Shared/IHider`, `Hazards/HidingSpot` (E entra e sai), `Player/RaccoonHiding` (0,5 s, modelo some, não cabe com volume 4 ou mais) e estados escondido e em transição no `PlayerState` (`CanMove`). No `ChaseState`, quem viu o guaxinim entrar vai até a lixeira e o pega (`HidingWitness` testado); quem já tinha perdido de vista cai na Busca de 6 s. Menu `Lixeira` (greybox); duas na Fase 1 e uma na Sandbox.
  - 2.10: `CameraSystem/ObstacleFader` na Main Camera dos rigs: SphereCast até o guaxinim na layer `Obstacle` e troca para `Art/Materials/M_Env_Kit_Faded` (asset gerado do material do kit, para o build incluir a variante transparente). Namespace `Guaxinim.CameraSystem` (exceção no README, como o `EditorTools`).
  - 3.5 (adiantado na ordem combinada): HUD no `LevelSystems` com `UI/ShoppingListUI` (só o que falta; Tab mostra a lista inteira e os bônus), `LevelTimerUI` (`LevelFlow.ElapsedSeconds`), `CarryLoadUI` (barras de peso e volume) e `EffectsUI` (efeitos e segundos restantes). Contador de cogumelos e aviso de alerta entram com os passos 3.1 e 3.3.
  - 3.6: `Core/LevelResult` testado (estrelas: lista, tempo-alvo do `LevelData` e sem alerta; bônus de 100 pontos por item), `Core/PauseController` (Esc pausa e passa para o mapa UI; trava o input no fim da fase), `UI/PauseMenuUI` (Continuar e Reiniciar fase) e `LevelResultUI` com as 3 estrelas (losangos) e o detalhe de cada uma. A estrela "sem alerta" sai sempre ganha até o passo 3.3. Tempo-alvo da Fase 1: 90 s.
  - 3.7: `Core/SaveData` (testado), `LevelRecord` e `SaveSystem` estático (JSON numa chave do PlayerPrefs). O `LevelData` ganhou `Id` (e `SceneName`); o `LevelFlow` grava o resultado ao vencer (`SaveSystem.RecordLevelResult`; fase sem id, como a Sandbox, não entra no save). Menu `Tools/Guaxinim/Apagar Save` zera o progresso para testar o desbloqueio.
  - 3.8 (24/11): `Core/LevelCatalog` (SO com as 5 fases em ordem; `IsUnlocked` e `TryGetNext`, testados), `SceneLoader` com `LoadMainMenu`, `LoadCityMap` e `LoadLevel`, e `UI/MainMenuUI` (Jogar, Como jogar, Créditos e Sair, escondido no WebGL), `UI/CityMapUI` + `UI/LevelButtonUI` (fachadas com estrelas e melhor tempo; "Bloqueada" e "Em breve" em cinza) e o início da fase (nome, lista e Começar) como painel do próprio mapa. A pausa ganhou o botão Mapa, e o resultado ganhou Próxima fase (só quando liberada) e Mapa. Menus do Editor: **`Tools/Guaxinim/Montar Menu e Mapa`** (cria `Scenes/MainMenu` e `Scenes/CityMap`), `Catálogo de Fases` (cria os `LevelData` das 5 fases; só marca a cena quando o arquivo existe) e `BuildSceneList` (Build Profile em MainMenu, CityMap e fases; **a Sandbox sai do build**). `Editor/EditorUI` concentra as peças de uGUI dos montadores. O menu principal tem a ilustração `Art/UI/Background_MainMenu.jpg` de fundo (24/11; imagens de `Art/UI/` entram como Sprite pelo `UITexturePostprocessor`), com o título e os botões na coluna da esquerda. A origem da imagem (IA) ainda precisa de fonte e licença no README. **Para ver tudo:** rodar `Montar Fase 1` (regera o `LevelSystems` com os botões novos) e depois `Montar Menu e Mapa`.
  - A Sandbox tem geleia, papel higiênico, melancia, energético, barra de proteína e uma lixeira para testar os efeitos; a lista dela é geleia e melancia.
- **Pendências de visual (combinadas em 23/11, para depois):** o guaxinim já usa o modelo `Char_Raccoon` (rodar `Montar Fase 1` de novo para ver); fazer animação procedural (quique e balanço ao andar, inclinação ao correr, respiração parado) para os três personagens, já que não há rig (GDD §15.4 permite animações pequenas no Unity); aproximar a câmera de 28 m para cerca de 16 m.
- **Próximo passo:** validar o 3.7 e o 3.8 no Editor (rodar `Montar Fase 1` e `Montar Menu e Mapa`, jogar do menu até vencer e voltar ao mapa, ver a Fase 2 como "Em breve"). Título provisório: "Operação Guaxinim" (nome em aberto, GDD §17); a tela de créditos ainda não tem o nome do autor (editar `MainMenuSceneBuilder.CreditsBody` ou o texto na cena). Depois: pendências de visual, 3.3 (alerta geral) e, só se sobrar tempo, 3.4 (câmeras de segurança), aspirador, cogumelo e fases 2 e 3.
- **Teste no Play Mode ainda pendente** de tudo desde o Marco 1: rodar `Montar Fase 1` (e `Montar Sandbox de Teste` na `SampleScene`), jogar, Test Runner (EditMode) e builds Windows e WebGL. Câmera (perspectiva ou ortográfica) ainda a escolher e registrar no GDD §3.1.
- **Teste automatizado na Unity (24/11):** o Unity 6000.3.24f1 do projeto não está instalado nesta máquina (só a 6000.6.4f1 e a 6000.6.6f1), e abrir o projeto com outra versão o atualizaria. Por isso o teste rodou numa **cópia isolada** (Assets, Packages e ProjectSettings em outra pasta, com outro `productName` para não misturar o PlayerPrefs) com a 6000.6.6f1 em modo batch. Resultado: tudo compila, os **118 testes EditMode passam**, os menus `Montar Fase 1` e `Montar Menu e Mapa` rodam sem erro, e 7 testes de Play Mode (só na cópia, não estão no repositório) passam: menu, mapa, início da fase, fase 1, vitória gravando o resultado no save, pausa, Mapa e Jogar de novo. Isso **não substitui** jogar na 6000.3.24f1 (versões e pacotes diferentes; o Play Mode com teclado, a câmera, o movimento e os builds seguem sem teste). Como repetir: copiar as pastas, rodar `Unity.exe -batchmode -projectPath <cópia> -runTests -testPlatform EditMode`, sem `-quit`.
- **Checagem de compilação sem o Editor:** o Editor fica aberto, então não dá para rodar a Unity em modo batch. Compile com o Roslyn da Unity (`Editor/Data/DotNetSdkRoslyn/csc.dll` via `Editor/Data/NetCoreRuntime/dotnet.exe`), referenciando os módulos de `Editor/Data/Managed/UnityEngine/` e só os assemblies listados em cada `.asmdef` (de `Library/ScriptAssemblies/`): referência faltando no `.asmdef` só aparece assim.

Atualize esta seção ao fim de cada passo.

## Como trabalhar

### Ciclo de cada passo

1. No começo da sessão, leia "Estado atual" e confirme com o usuário qual passo será feito.
2. **Um passo por vez, na ordem do plano.** Não adiante nada de passos seguintes (GDD §0.1).
3. Leia as seções do GDD indicadas no passo. Se algo estiver ambíguo ou cair em "Decisões em aberto", pergunte antes.
4. Implemente seguindo as duas skills de código (ver "Skills de código"): scripts, `ScriptableObject` com os números e testes EditMode da lógica pura. Se o passo cria ou muda a estrutura de um sistema, diga qual padrão usou e por quê.
5. Ao terminar, entregue ao usuário: (a) o que ele precisa montar no Editor, em passos curtos; (b) como testar no Play Mode; (c) uma mensagem de commit sugerida.
6. Marque o passo com `[x]` e atualize "Estado atual". Ao fechar um marco, marque também os itens do GDD §16 e registre no GDD qualquer mudança de design.

### Pronto quer dizer

- Compila sem erros nem warnings novos no Console do Unity.
- Testes EditMode passando.
- Testado no Play Mode pelo usuário (na Sandbox ou na fase).
- Nenhum `Debug.Log` de depuração esquecido.
- Checklists das duas skills de código conferidos.
- No fim de cada marco: build Windows e WebGL rodando.

### Testes

Fórmulas e regras (velocidade com peso, raio do som, medidor de detecção, barra de frágil, efeitos, estrelas, save) ficam em **classes C# comuns, sem `MonoBehaviour`**, para serem testadas em `Tests/EditMode/`. O `MonoBehaviour` só liga a lógica à cena.

### Commits

Um commit por passo, no padrão já usado no repositório: Conventional Commits com a descrição em português, por exemplo `feat(guards): estado de patrulha com waypoints`.

## Regras do projeto

Resumo do GDD §0 e do README; os detalhes estão lá.

- **Greybox primeiro.** Toda mecânica é feita com cubos e cápsulas; arte nunca bloqueia código.
- **Números ajustáveis em `ScriptableObject`** (`Data/`), nunca fixos no código. As tabelas do GDD são valores iniciais.
- **Não edite arquivos `.unity` sem pedir.** Prefira prefabs e scripts.
- Código em inglês, textos da tela em português. Uma classe por arquivo, namespace `Guaxinim.<Módulo>` igual à pasta, `[SerializeField] private` em vez de campos públicos.
- Só o Input System novo.
- Módulos se falam por **eventos C#** e **interfaces de `Shared/`**. Nada de buscas na cena durante o jogo. A lógica de gameplay não conhece a `UI`.
- **WebGL desde o início:** no máximo 4 ou 5 luzes em tempo real por cena, sem `System.Threading`, poucos draw calls, sem pós-processamento pesado.
- Mercados são paródias: paletas que lembram os originais, **logos originais**.
- Todo asset de terceiros ou gerado por IA entra nos créditos do README, com fonte e licença.

## Skills de código

Todo código do projeto segue estas duas skills, ao escrever, revisar ou refatorar. Elas são importadas abaixo e valem em toda sessão.

| Skill | Conteúdo |
| --- | --- |
| [skills/clean-code.md](skills/clean-code.md) | Nomes legíveis e o glossário do domínio (termo do GDD para o nome em inglês), métodos curtos, organização das classes, Unity sem sujeira, comentários, erros e estados robustos, formatação e testes. |
| [skills/game-design-patterns.md](skills/game-design-patterns.md) | Quando usar Component, State, Observer, Mediator, Singleton, `ScriptableObject`, Template Method, Strategy, Factory, Object Pool e interfaces; onde cada um entra no projeto; padrões e antipadrões a evitar. |

Se uma skill e o GDD divergirem, o GDD manda no design e as skills mandam na forma do código.

@skills/clean-code.md
@skills/game-design-patterns.md

## Armadilhas técnicas

### Unity 6.3 e pacotes

- O Unity compila em **C# 9**: sem namespace de arquivo (`namespace X;`) e sem `global using`.
- **Cinemachine 3:** `using Unity.Cinemachine;`, `CinemachineCamera` com `CinemachineFollow` ou `CinemachinePositionComposer`, e `CinemachineBrain` na Main Camera. `CinemachineVirtualCamera` e o namespace `Cinemachine` são da versão 2 e não existem aqui.
- **AI Navigation 2:** o NavMesh é gerado pelo componente `NavMeshSurface` (`Unity.AI.Navigation`) em cada cena. A janela Navigation antiga não existe.
- O projeto aceita **só** o Input System: qualquer uso de `UnityEngine.Input` lança exceção.
- APIs do Unity 6: `FindFirstObjectByType`/`FindAnyObjectByType` (só em setup, nunca por frame) e `Rigidbody.linearVelocity`.
- O TextMeshPro vem dentro do uGUI 2.0. Na primeira UI, é preciso importar o "TMP Essential Resources" (Window > TextMeshPro).

### Arquivos e assets

- **Não escreva YAML de cena, prefab ou asset à mão.** Para gerar assets em lote (os itens da tabela §7.8, os `LevelData`, os prefabs de greybox), crie em `Scripts/Editor/` um `MenuItem` no menu `Tools/Guaxinim/...`, usando `AssetDatabase` e `PrefabUtility`.
- O Unity gera os `.meta` ao importar. Faça commit do `.meta` junto com o arquivo e nunca apague nem recrie o `.meta` de um asset existente: as referências são por GUID.
- O `.inputactions` é JSON e pode ser editado direto.
- O `.inputactions` gera a classe `Core/GameInput.cs` (opção "Generate C# Class" no `.meta`). Ela só é regenerada quando o asset é reimportado, e a Unity não reimporta nada se houver erro de compilação: ao renomear ou adicionar uma ação usada no código, gere a classe antes de usar a ação nova. O `Dispose()` dela usa `Destroy`; em testes EditMode, use `Object.DestroyImmediate(input.asset)`.
- O Editor sobrescreve `ProjectSettings/*.asset` se estiver aberto. Peça ao usuário para mudar no Editor, ou edite com o Editor fechado.
- **Modelos do Blender:** frente em −Y no Blender, que vira +Z na Unity (verificado). O FBX traz o material `M_<Nome>`, e o `BlenderModelPostprocessor` o liga à textura `Art/Textures/<pasta>/<Nome>_BaseColor.png`. Pontos de encaixe são empties `Socket_<Uso>`. Detalhes em [Tools/Blender/README.md](Tools/Blender/README.md).

### Configurações que ainda precisam de ajuste

- **WebGL:** a compressão está em Brotli sem fallback, o que quebra no itch.io. Trocar para Gzip com Decompression Fallback, ou Disabled (GDD §14.6). Passo 0.2.
- `productName` está "Jogo-3D" e `companyName` está "DefaultCompany". Passo 0.2.
- A matriz de colisão das layers está toda ligada. Configurar nos passos que usam `NarrowGap` (2.4) e `ShelfTop` (3.2).
- **Qualidade por plataforma:** o WebGL usa o nível Mobile (renderer Forward, render scale 0,8) e o Windows usa o nível PC (Forward+). Confira o visual nos dois.

## Comandos

Testes EditMode por linha de comando (PowerShell, **com o Editor fechado**: o projeto fica travado enquanto ele está aberto; leva cerca de 1,5 min):

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.24f1\Editor\Unity.exe" -batchmode -projectPath . -runTests -testPlatform EditMode -testResults Logs/editmode-results.xml -logFile Logs/editmode.log | Out-Null; "exit=$LASTEXITCODE"
```

Exit 0 significa que tudo passou; o resultado detalhado fica em `Logs/editmode-results.xml`. Com o Editor aberto, use Window > General > Test Runner.

- `dotnet build Jogo-3D.slnx` só serve como checagem rápida depois que o Unity regenerou os `.csproj`; arquivos novos só entram neles quando o Editor atualiza. Na dúvida, vale o Console do Unity.
- Build: depois do passo 0.3, `-executeMethod Guaxinim.EditorTools.BuildScript.BuildWindows` (ou `BuildWebGL`), com saída em `Builds/` (ignorada pelo Git).
- Blender: ver [Tools/Blender/README.md](Tools/Blender/README.md).

## Plano incremental

Os marcos seguem o GDD §16, quebrados em passos pequenos. Cada passo termina em algo que dá para testar no Play Mode, e cada marco termina num build jogável.

### Marco 0: fundação

- [x] **0.1 Input Actions do jogo.** Reescrever o mapa `Player` com as ações da §3.2: `Move`, `Sprint`, `Interact`, `Drop`, `Whistle`, `MushroomPower`, `Jump`, `ShowList` e `Pause`, com teclado e gamepad da tabela. Remover as ações que sobram e manter o mapa `UI`. Criar um único ponto de leitura de input (`Core/InputReader`) que o resto do jogo usa e que troca para o mapa `UI` na pausa. *Pronto:* cada ação dispara no Input Debugger.
- [x] **0.2 Player Settings.** Nome do produto e da empresa, compressão WebGL (ver Armadilhas). O usuário muda no Editor e Claude lista os campos.
- [ ] **0.3 Script de build.** `Scripts/Editor/BuildScript.cs` com `MenuItem` e métodos para `-executeMethod`, gerando Windows e WebGL em `Builds/<plataforma>/`. *Pronto:* os dois builds da Sandbox abrem.

### Marco 1: núcleo furtivo em greybox (GDD §16, semana 1)

- [ ] **1.1 Movimento.** `PlayerData` (SO) e `RaccoonController` com `CharacterController`: andar e correr, gravidade, girar para a direção do movimento, input relativo à câmera isométrica (yaw de 45°). Prefab de cápsula com 0,6 m na layer `Player`. *§3.2, §4.* *Pronto:* anda e corre na Sandbox sem prender em quinas.
- [ ] **1.2 Câmera isométrica.** Rig com Cinemachine 3 (X 45°, Y 45°), seguindo suave com um avanço na direção do movimento. Deixar as duas opções prontas, perspectiva com FOV de cerca de 25° e ortográfica, e **o usuário escolhe**. Registrar a escolha no GDD. *§3.1.*
- [ ] **1.3 Sistema de som.** `NoiseData` (SO com os raios), `NoiseSystem.Emit(position, radius)`, `NoiseListener` e `NoiseRingVFX` (anel no chão que expande e some). Correr emite som em intervalos e o assobio (F) emite 8 m. Testar a fórmula do raio de correr com carga. *§5.*
- [ ] **1.4 Kit greybox.** Prefabs de piso, parede e gôndolas de 2 m e 4 m (layer `Obstacle`), criados por um `MenuItem`. `NavMeshSurface` na Sandbox. *§11, §14.4.*
- [ ] **1.5 Segurança: patrulha.** `GuardData` (SO) e `GuardAI` com máquina de estados, uma classe por estado em `Guards/States/`. Estado Patrulha com `NavMeshAgent`, waypoints e pausas olhando em volta. *§6.2.*
- [ ] **1.6 Visão.** Interface em `Shared/` para o alvo detectável. `VisionCone` (raycast bloqueado por `Obstacle`, 9 m e 70°) e `VisionConeMesh` (cone desenhado no chão) com a luz spot da lanterna. *§6.1.*
- [ ] **1.7 Medidor de detecção.** `DetectionMeter`: enche de 0 a 100% em 1 s a 9 m, mais rápido de perto, e esvazia devagar. Ícone sobre a cabeça. Lógica testada. *§6.3.*
- [ ] **1.8 Desconfiado, Perseguição, Busca e Retorno.** Audição pelo `NoiseListener` e transições da §6.2. Confirmar o balanço: sem carga, o guaxinim correndo foge do segurança. *§6.2, §6.5.*
- [ ] **1.9 Ser pego e reiniciar.** `PlayerState` (`IsVisible`, `IsUntouchable`, `OnCaught`), captura a menos de 1 m em Perseguição, aviso "Você foi pego!" e reinício após 1,5 s, com `GameManager` e `SceneLoader` mínimos. *§6.5, §13.* *Pronto:* ser pego e reiniciar 10 vezes seguidas sem erro no Console.
- [ ] **1.10 Fase 1 em greybox.** Cena `Level_01_Mercadinho`: 2 corredores, 1 segurança (o Seu Zé) e a toca. Claude propõe o layout e o usuário monta, ou autoriza Claude a montar. Build Windows e WebGL. *§11.*

### Marco 2: saque e mundo (semana 2)

- [ ] **2.1 Itens.** `ItemData` (SO), um `MenuItem` que gera os itens da tabela §7.8 em `Data/Items/`, `PickupItem` e a interação (E) com o objeto mais próximo na layer `Interactable`. *§7.2, §7.8.*
- [ ] **2.2 Inventário e peso.** `Inventory` (`OnChanged`, limites de peso 10 e volume 6), velocidade pela fórmula da §7.3 e raio de correr com carga. Fórmulas testadas. *§7.3.*
- [ ] **2.3 Largar item.** Q larga o último item, com som de 3 m. Ele pode ser pego de novo. *§7.6.*
- [ ] **2.4 Volume.** Com volume ≥ 4: esconde os cones a mais de 6 m e aplica a vinheta, bloqueia a passagem pela layer `NarrowGap` (ajustar a matriz de colisão) e impede entrar na lixeira. *§7.4.*
- [ ] **2.5 Frágeis.** `FragileShake`: enche correndo, esvazia andando e quebra após 2 s seguidos (perde o item, som de 10 m e poça). Testado. *§7.5.*
- [ ] **2.6 Lista, toca e vitória.** `LevelData` (SO), `ShoppingList` (`OnItemDelivered`, condição de vitória), `Den` (entrega automática e fim da fase), contorno nos itens da lista por perto e tela de resultado provisória. *§7.1, §7.7.* *Pronto:* a Fase 1 é jogável do começo à vitória, e da derrota ao reinício.
- [ ] **2.7 Consumíveis.** `ConsumableEffect` (base), `EnergyDrinkEffect` e `ProteinBarEffect`, que podem acontecer juntos, e a regra do excesso de peso quando a proteína acaba. Testado. *§8.1.*
- [ ] **2.8 Lixeiras.** `HidingSpot`: E entra e sai (0,5 s), o guaxinim fica invisível, regra do segurança que viu entrar, não cabe com volume ≥ 4. *§9.*
- [ ] **2.9 Robô aspirador.** `RobotVacuum`: anda reto, gira de 90° a 180° ao bater, esbarrão (som de 4 m e empurrão) e modo montado (2 m/s, som contínuo de 2 m). *§10.*
- [ ] **2.10 ObstacleFader.** O que fica entre a câmera e o guaxinim fica translúcido, de forma barata para o WebGL. *§3.1.*
- [ ] **2.11 Fase 2 em greybox.** `Level_02_XOXO`: 1 segurança, 1 aspirador, itens frágeis e energéticos. Build. *§11.*

### Marco 3: superpoder, telas e progressão (semana 3)

- [ ] **3.1 Cogumelos.** Coletável e `MushroomPower`: conta até 3, fica pronto, R ativa, e cogumelos a mais não acumulam. *§8.2.*
- [ ] **3.2 Modo cogumelo.** 12 s: sobe na prateleira mais próxima, anda na layer `ShelfTop` a 7 m/s, pula até 3 m (Espaço), fica intocável, não pega itens e desce (pela borda, com som de 5 m, ou no corredor mais próximo quando o tempo acaba). *§8.2.*
- [ ] **3.3 Alerta geral.** `AlertManager`: 8 s, todos os seguranças vão à última posição conhecida, `OnAlertStarted`. *§6.4.*
- [ ] **3.4 Câmeras de segurança.** `SecurityCamera`: varredura em ciclo, cone vermelho de 8 m e 50°, 1 s no cone dispara o alerta, sem perseguir. *§6.6.*
- [ ] **3.5 HUD.** Lista (Tab mostra inteira), barras de peso e volume, contador de cogumelos, efeitos ativos, cronômetro e aviso de alerta. A HUD só assina eventos. *§13.*
- [ ] **3.6 Pausa, Pego e Resultado.** Telas da §13, com estrelas (lista, tempo-alvo, sem alerta) e bônus. Cálculo das estrelas testado. *§12, §13.*
- [ ] **3.7 SaveSystem.** Um JSON numa chave do `PlayerPrefs`: fases desbloqueadas, estrelas e melhor tempo. Serialização testada. *§12.*
- [ ] **3.8 Fluxo de cenas.** Boot, MainMenu, CityMap e a tela de início da fase. "Sair" escondido no WebGL. *§13.* *Pronto:* do menu, jogar a fase, vencer, voltar ao mapa e ver a próxima fase desbloqueada, sem fechar o jogo.
- [ ] **3.9 Fase 3 em greybox.** `Level_03_PaoDeMel`: 2 seguranças, 1 aspirador, 6 cogumelos e itens volumosos. Build. *§11.*
- [ ] **3.10 Primeiros modelos.** Scripts `bpy` em `Tools/Blender/` para o guaxinim, o segurança e o kit modular, exportando FBX para `Art/Models/`. *§15.4.* Personagens estáticos (guaxinim, segurança e Seu Zé), câmera de segurança e kit modular já feitos; faltam rig e animações.

### Marco 4: arte, áudio e entrega (semana 4)

- [ ] **4.1 Arte das fases 1 a 3.** Itens, ícones e fachadas no Blender, substituindo o greybox. *§15.4, §15.5.*
- [ ] **4.2 Luz, áudio e efeitos.** Iluminação baked com no máximo 4 ou 5 luzes em tempo real, áudio da §15.6 e efeitos visuais.
- [ ] **4.3 Balanceamento.** Testes de jogo, mudando só os `ScriptableObject`.
- [ ] **4.4 Entrega do MVP.** Builds Windows e WebGL testados (itch.io), README com controles e créditos, GDD atualizado e página do itch.io com como jogar e imagens ou vídeo.
- [ ] **4.5 Fases 4 e 5.** `Level_04_Carrecinco` e `Level_05_Atacadaco`, reaproveitando o kit, e nova entrega. *§11.*

## Se o prazo apertar

O que preservar, em ordem:

1. **O ciclo completo sem becos sem saída:** menu, fase, ser pego e reiniciar, vencer, resultado e voltar ao menu (rubrica 4).
2. **Controles, câmera e colisões bem ajustados**, sem prender em quinas (rubricas 1 e 2).
3. **Modelos e materiais coerentes** em tudo que aparece na tela, mesmo que simples (rubrica 3).
4. A Fase 1 polida antes da Fase 2. As fases 4 e 5 são opcionais.

Cortar uma mecânica é decisão do usuário: pergunte antes e registre o corte no GDD.

## Decisões em aberto

Pergunte antes de assumir qualquer uma destas:

- Nome do jogo (GDD §17).
- Paleta exata de cada mercado (GDD §17).
- Câmera em perspectiva ou ortográfica (GDD §3.1, decidida no passo 1.2).
- ~~Prazo~~: decidido em 23/11 (entrega em 25/11; ver "Estado atual").
