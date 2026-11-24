# Operação Guaxinim (nome provisório)

Jogo 3D furtivo com câmera isométrica: um guaxinim invade supermercados à noite para roubar os itens da lista de compras e levá-los até a toca, sem ser pego pelos seguranças. Cada item muda o jeito de jogar: pesado deixa lento, volumoso tapa a visão, frágil quebra se ele correr.

Projeto 3D Hiper-Casual — Jogos Digitais, Insper 2026.2.

- **GDD:** [docs/GDD_Operacao_Guaxinim.md](docs/GDD_Operacao_Guaxinim.md)
- **itch.io:** _link a adicionar na publicação_

## Controles

| Ação | Teclado |
| --- | --- |
| Mover (silencioso) | WASD |
| Correr (faz barulho) | Shift |
| Pegar item / interagir / lixeira / aspirador | E |
| Largar o último item | Q |
| Assobiar (distração) | F |
| Ativar o modo cogumelo (com 3 cogumelos) | R |
| Pular (no modo cogumelo) | Espaço |
| Ver a lista inteira | Tab (segurar) |
| Pausar | Esc |

## Requisitos

- **Unity 6000.3.24f1** (Unity 6.3 LTS), Universal Render Pipeline
- **Blender 5.3** para os modelos (ver [Tools/Blender](Tools/Blender/README.md))
- Pacotes: Input System, Cinemachine 3, AI Navigation, uGUI/TextMeshPro, Test Framework

## Como abrir

1. Clone o repositório.
2. No Unity Hub: **Add → Add project from disk** e selecione a pasta do repositório.
3. Abra com a versão **6000.3.24f1**. A primeira abertura importa tudo e leva alguns minutos.
4. As cenas do jogo são geradas por menus do Editor (`Tools/Guaxinim/`): **Montar Fase 1** e depois **Montar Menu e Mapa**. O jogo começa na cena `MainMenu`, segue para o `CityMap` e daí para a fase escolhida.
5. Cena de testes: `Assets/_Project/Scenes/Sandbox/SampleScene` (menu **Montar Sandbox de Teste**; fica fora do build).

## Estrutura do projeto

```
Assets/_Project/
├── Art/        Modelos (FBX do Blender), materiais, texturas, animações e ícones
├── Audio/      Music/ e SFX/
├── Data/       ScriptableObjects: Items, Guards, Levels, Player, Noise, Effects
├── Prefabs/    Camera, Environment (kit modular), Guards, Hazards, Items, Player, UI
├── Scenes/     MainMenu, CityMap, Levels/ (uma cena por fase) e Sandbox/ (testes)
├── Scripts/    Código, um módulo por pasta
├── Settings/   URP (Rendering/) e Input Actions (Input/)
└── Tests/      Testes automatizados (EditMode/)
docs/           GDD e arte de referência
Tools/Blender/  Scripts bpy que geram os modelos
```

### Módulos de código (`Assets/_Project/Scripts/`)

| Módulo | Responsabilidade |
| --- | --- |
| `Core` | Ciclo do jogo, troca de cenas, save e dados de cada fase |
| `Player` | Movimento do guaxinim, estado, inventário e fragilidade |
| `Items` | Itens no mundo, lista de compras e toca |
| `Effects` | Consumíveis e modo cogumelo |
| `Noise` | Emissão e escuta de sons, anéis visuais no chão |
| `Guards` | IA do segurança, visão, detecção e alerta geral (`States/` guarda a máquina de estados) |
| `Hazards` | Robô aspirador, lixeiras e câmeras de segurança |
| `Camera` | Câmera isométrica e transparência das prateleiras |
| `UI` | HUD e menus |
| `Shared` | Interfaces e tipos usados por mais de um módulo |
| `Editor` | Ferramentas do Editor (não entram no build) |

## Arquitetura e convenções

### Assemblies

| Assembly | Pasta | Conteúdo |
| --- | --- | --- |
| `Guaxinim.Runtime` | `Scripts/` | Todo o código do jogo |
| `Guaxinim.EditorTools` | `Scripts/Editor/` | Ferramentas do Editor |
| `Guaxinim.Tests.EditMode` | `Tests/EditMode/` | Testes da lógica pura (fórmulas de peso, raio de som, etc.) |

O namespace do Editor é `Guaxinim.EditorTools`, e não `Guaxinim.Editor`, para não colidir com a classe `UnityEditor.Editor`. Pelo mesmo motivo, a pasta `Camera` usa o namespace `Guaxinim.CameraSystem`, e não `Guaxinim.Camera`, que esconderia o tipo `UnityEngine.Camera` em todo o projeto.

### Regras de dependência

- `Shared` não depende de nenhum outro módulo do projeto.
- Os módulos de gameplay se comunicam por **eventos C#** e por **interfaces de `Shared`**, nunca por buscas na cena durante o jogo (GDD, seção 14.5).
- Nenhum módulo de gameplay conhece a `UI`. A UI assina eventos para se atualizar e só chama a API pública de `Core` (pausar, reiniciar, trocar de cena).

### Padrões de projeto

| Padrão | Onde | Por quê |
| --- | --- | --- |
| State | `Guards/States/` e `PlayerState` | Cada estado do segurança (Patrulha, Desconfiado, Perseguição, Busca, Retorno) é uma classe própria, sem um `switch` gigante |
| Observer | Eventos C# (`Inventory.OnChanged`, `AlertManager.OnAlertStarted`…) | HUD e áudio reagem sem que a lógica conheça quem está ouvindo |
| Template Method / Strategy | `Effects/ConsumableEffect` | Um consumível novo é uma subclasse nova, sem mexer no jogador |
| Mediator | `NoiseSystem` e `AlertManager` | Quem emite um som ou alerta não precisa conhecer quem reage |
| Dados em ScriptableObject | `Data/` | Números ajustáveis ficam fora do código e são balanceados no Inspector |
| Inversão de dependência | Interfaces em `Shared/` | Os seguranças dependem de um contrato (ex.: algo detectável), não da classe concreta do guaxinim |

### Convenções de código

- Código em inglês, textos da tela em português.
- Uma classe por arquivo, com o nome do arquivo igual ao da classe.
- Namespace `Guaxinim.<Módulo>`, igual à pasta (ex.: `Guaxinim.Guards`).
- `[SerializeField] private` em vez de campos públicos.
- Nada de números mágicos: valores ajustáveis vão para os ScriptableObjects em `Data/`.
- Apenas o Input System novo, nunca a classe `Input` antiga.
- Greybox primeiro: toda mecânica é testada com formas simples antes de receber o modelo final.
- Prefira prefabs a mudanças diretas nas cenas, para reduzir conflitos de merge.

## Créditos

Todo asset de terceiros ou gerado por IA entra aqui, com fonte e licença (GDD, seção 15.6). O mesmo vale para trechos de código de terceiros com mais de 2 ou 3 linhas.

### Assets

| Asset | Autor | Fonte | Licença |
| --- | --- | --- | --- |
| Modelos e texturas em `Art/Models/` e `Art/Textures/` (personagens, câmera de segurança, kit modular) | Gerados por scripts Python do Blender escritos com o Claude (Anthropic) | `Tools/Blender/` | Feitos para este projeto |
| Ilustração de fundo do menu principal (`Art/UI/Background_MainMenu.jpg`) | Gerada por IA (ferramenta e prompt a confirmar) | _a preencher_ | _a preencher_ |
| Fonte do título do jogo (`Art/Fonts/LilitaOne-Regular.ttf`) | Juan Montoreano | [Lilita One no Google Fonts](https://fonts.google.com/specimen/Lilita+One), arquivo do repositório [google/fonts](https://github.com/google/fonts/tree/main/ofl/lilitaone) | SIL Open Font License 1.1 (cópia em `Art/Fonts/OFL.txt`) |
| Fundo do fim de jogo (`Art/UI/Background_Ending.jpg`) | Derivado da ilustração do menu (desfocada, dourada e com brilhos, por script) | `Tools/Art/make_ui_backgrounds.py` | Mesma origem e licença da ilustração do menu |
| Mapa da cidade (`Art/UI/Background_CityMap.jpg`) | Desenhado por script com o Pillow, escrito com o Claude (Anthropic) | `Tools/Art/make_ui_backgrounds.py` | Feito para este projeto |

### Código de terceiros

| Trecho | Arquivo | Fonte |
| --- | --- | --- |
| _a preencher_ | | |

### Ferramentas

- [Unity 6](https://unity.com/) e pacotes oficiais da Unity
- [Blender](https://www.blender.org/)
- [Claude Code](https://claude.com/claude-code) (Anthropic), assistência na implementação
