# Operação Guaxinim (nome provisório) — Documento Base do Jogo

Documento de especificação para implementar o jogo com o Claude Code em Unity 6, com os modelos 3D feitos no Blender. Projeto individual, prazo de cerca de 1 mês, para PC e WebGL (itch.io).

---

## 0. Instruções para o Claude Code

Leia este documento inteiro antes de qualquer tarefa. Regras de trabalho:

1. **Siga os marcos da seção 16 em ordem.** Implemente só o marco pedido e não adiante funcionalidades.
2. **Greybox primeiro.** Toda mecânica é feita e testada com cubos e cápsulas antes de ganhar modelo do Blender. Arte nunca bloqueia código.
3. **Números ajustáveis ficam em `ScriptableObject`**, nunca fixos no código. As tabelas deste documento são valores iniciais.
4. **Itens da seção 17 (Decisões em aberto) não estão decididos.** Pergunte antes de assumir.
5. **Não edite arquivos de cena (`.unity`) sem pedir.** Prefira prefabs e scripts, e diga ao final o que precisa ser montado à mão no Editor.
6. **Código em inglês, textos da tela em português.** Uma classe por arquivo, `[SerializeField] private` para campos do Inspector.
7. **Use o Input System novo**, nunca a classe `Input` antiga.
8. **Pense no WebGL desde o início** (seção 14.6): poucas luzes em tempo real, sem threads, poucos draw calls.
9. **Estilo visual:** siga a Direção de arte (seção 15.1) e a imagem de referência em `docs/art/referencia_estilo.png`. No greybox, use formas simples; nos modelos finais, siga o estilo definido.
10. **Marcas:** os mercados são paródias. Use paletas de cores que lembrem os originais, mas **crie logos originais**, sem copiar o símbolo, a tipografia ou o formato dos logos reais.

---

## 1. Visão geral

Um jogo 3D furtivo de roubo, com câmera isométrica, em que um guaxinim invade supermercados à noite para roubar os itens de uma lista de compras e levá-los até a sua toca. Ele precisa fugir do segurança, evitar fazer barulho e decidir o que carregar, porque cada item muda o jeito de jogar: pesado deixa lento, volumoso tapa a visão, frágil quebra se ele correr.

**Inspiração: The Marvellous Miss Take.** Mantemos o núcleo dele:

- Fases curtas de roubo furtivo, com um objetivo claro por fase.
- Guardas com área de visão visível e medidor de detecção.
- **Barulho visível:** correr e assobiar soltam ondas de som que os guardas ouvem.
- Distrair guardas com som para abrir caminho.
- Objetivos extras (tempo, não ser visto) que dão estrelas.

**O que é nosso:**

- Saque com **peso, volume e fragilidade**, que muda velocidade, barulho e visão.
- **Lista de compras** como objetivo e **toca** como ponto de entrega.
- **Consumíveis** com efeitos na hora (energético, etc.).
- **Superpoder dos cogumelos:** andar e pular por cima das prateleiras sem ser pego.
- **Robô aspirador** que atrapalha e também vira veículo.
- Progressão por **mercados cada vez maiores**, do mercadinho de bairro ao atacado.

**Pilares de design** (toda decisão deve servir a pelo menos um):

1. **Legível:** a qualquer momento o jogador vê os guardas, as visões e as ondas de som.
2. **Toda escolha de item é estratégica:** a ordem e a combinação do que carregar decidem a fase.
3. **Humor de guaxinim:** animações exageradas, sons engraçados, situações absurdas.

---

## 2. Loop de jogo

1. **Mapa da cidade:** o jogador escolhe um mercado já desbloqueado.
2. **Início da fase:** o guaxinim sai da toca (um bueiro ou buraco nos fundos) e vê a **lista de compras**.
3. **Infiltração:** anda pelo mercado, coleta itens, consome alguns, junta cogumelos, foge do segurança.
4. **Entrega:** leva os itens da lista até a toca. Pode fazer **várias viagens**.
5. **Fim:** com todos os itens da lista entregues, a fase termina ao entrar na toca. Aparece o resultado com as estrelas.
6. **Pego pelo segurança:** a **fase reinicia** do começo. Por isso as fases devem durar de 3 a 5 minutos.

---

## 3. Câmera e controles

### 3.1 Câmera

- **Isométrica seguindo o guaxinim**, com Cinemachine. Rotação inicial X = 45°, Y = 45°. Perspectiva com FOV baixo (cerca de 25°) ou ortográfica, a decidir no protótipo.
- A câmera segue suavemente, com um pequeno avanço na direção do movimento.
- **Prateleiras na frente do guaxinim ficam transparentes:** qualquer objeto entre a câmera e o jogador some ou fica translúcido (`ObstacleFader`), para o guaxinim nunca ficar escondido atrás de uma gôndola.
- Sem rotação da câmera pelo jogador no MVP.

### 3.2 Controles

| Ação | Teclado | Controle (opcional) |
| --- | --- | --- |
| Mover (anda silencioso) | WASD | Analógico esquerdo |
| Correr (faz barulho) | Shift | RT |
| Pegar item / interagir / entrar na lixeira / subir no aspirador | E | A |
| Largar o último item | Q | B |
| Assobiar (distração) | F | Y |
| Ativar o modo cogumelo (com 3 cogumelos) | R | LB |
| Pular (no modo cogumelo) | Espaço | A |
| Ver a lista inteira | Tab (segurar) | Select |
| Pausar | Esc | Start |

---

## 4. O guaxinim

| Atributo | Valor inicial |
| --- | --- |
| Velocidade andando | 3 m/s |
| Velocidade correndo | 6 m/s |
| Peso máximo carregado | 10 unidades |
| Volume máximo carregado | 6 unidades |
| Altura | cerca de 0,6 m (cabe sob algumas prateleiras e em vãos) |

Usa `CharacterController` (não `Rigidbody`) para um movimento preciso. Animações: parado, andando, correndo, carregando, entrando na lixeira, pulando, montado no aspirador, pego e o "modo cogumelo".

---

## 5. Sistema de som (barulho)

Todo barulho é um evento `NoiseSystem.Emit(posição, raio)`. Os seguranças a menos de `raio` metros ouvem e vão investigar o ponto. **Toda onda de som aparece no chão** como um anel que expande e some (`NoiseRingVFX`), como no Miss Take.

| Fonte | Raio (m) |
| --- | --- |
| Andar | 0 (silencioso) |
| Correr sem carga | 6 |
| Correr com carga | 6 + 3 × (peso / peso máximo) |
| Assobiar | 8 |
| Largar um item | 3 |
| Item frágil quebrando | 10 |
| Esbarrar no robô aspirador | 4 |
| Robô aspirador com o guaxinim montado (contínuo) | 2 |
| Descer da prateleira para o chão | 5 |

O segurança não ouve através de paredes externas, mas ouve através das prateleiras.

---

## 6. Detecção e o segurança

### 6.1 Visão

- **Cone de luz da lanterna**, desenhado no chão por um mesh (`VisionConeMesh`) e por uma luz spot. Alcance de 9 m e ângulo de 70°.
- A visão é feita por raycast até o guaxinim, e é bloqueada por objetos da layer `Obstacle` (prateleiras altas, paredes, freezers).
- Guaxinim **dentro de uma lixeira** ou **em cima das prateleiras (modo cogumelo)** nunca é visto.

### 6.2 Estados do segurança

| Estado | Comportamento |
| --- | --- |
| Patrulha | Segue waypoints (NavMesh) a 2 m/s, com pausas em que olha em volta. |
| Desconfiado | Ouviu um som ou viu o guaxinim de relance. Vai até o ponto a 3 m/s e olha em volta por 4 s. |
| Perseguição | O medidor de detecção encheu. Corre atrás do guaxinim a 5,5 m/s e dispara o **alerta geral**. |
| Busca | Perdeu o guaxinim de vista. Vai até a última posição conhecida e procura por 6 s. |
| Retorno | Volta para a patrulha. |

### 6.3 Medidor de detecção

- Um ícone sobre a cabeça do segurança enche enquanto o guaxinim está no cone: de 0 a 100% em 1 s a 9 m, e mais rápido quanto mais perto ele está. Esvazia devagar quando o guaxinim sai do cone.
- Em 100%, entra em Perseguição.

### 6.4 Alerta geral

- Quando um segurança entra em Perseguição, **todos os seguranças da fase** ficam em alerta por 8 s: vão até a última posição conhecida, e a música muda.

### 6.5 Ser pego

- Se o segurança chegar a menos de 1 m do guaxinim em Perseguição, o guaxinim é pego: animação curta e a **fase reinicia**.
- **Repare no balanço:** sem carga, o guaxinim (6 m/s) foge do segurança (5,5 m/s). Com carga, ele fica mais lento e é pego. Fugir obriga a **largar itens**.

### 6.6 Câmeras de segurança (fases 4 e 5)

- Presas no teto, giram de um lado para o outro num ciclo fixo (ex.: 90° em 4 s, pausa de 1 s em cada ponta).
- Têm um cone de visão desenhado no chão, como o segurança, em outra cor (vermelho), alcance de 8 m e ângulo de 50°.
- **Não perseguem:** se o medidor de detecção da câmera encher (1 s no cone), ela dispara o **alerta geral** e todos os seguranças vão até o ponto onde o guaxinim foi visto.
- São bloqueadas pelos mesmos obstáculos e não veem o guaxinim escondido ou no modo cogumelo.
- O ciclo é previsível: o desafio é cronometrar a passagem.
- Não dá para desligar as câmeras no escopo atual.

---

## 7. Saque: lista, inventário e entrega

### 7.1 Lista de compras

- Cada fase define uma lista de 3 a 10 itens (`LevelData.shoppingList`).
- Itens da lista ficam com um contorno sutil quando estão perto do guaxinim.
- A HUD mostra a lista com caixas que são marcadas **só quando o item é entregue na toca**.
- Itens fora da lista podem ser levados como **bônus**.

### 7.2 Categorias de item

Cada item é um `ItemData` (`ScriptableObject`) com: nome, ícone, modelo, `weight`, `volume`, `isFragile`, `isConsumable` e `effect`.

| Categoria | Exemplo | Efeito |
| --- | --- | --- |
| Normal | Pão de forma, banana | Pouco peso e volume. |
| Pesado | Melancia, galão de água, saco de arroz | Muito peso: deixa lento e barulhento. |
| Volumoso | Salgadinho gigante, fardo de papel higiênico, caixa de cereal | Muito volume: tapa a visão e não passa em vãos. |
| Frágil | Garrafa de suco, caixa de ovos, pote de geleia | Quebra se o guaxinim correr com ele. |
| Consumível | Energético, barra de proteína | Usado na hora, ao pegar (seção 8). |

### 7.3 Efeito do peso

```
velocidade = velocidadeBase × (1 − 0,5 × pesoAtual / pesoMáximo)
```

Com o peso máximo, o guaxinim fica com metade da velocidade. O raio do som de correr também cresce (seção 5).

### 7.4 Efeito do volume

Com volume igual ou maior que 4:

- **A visão fica tapada:** os cones dos seguranças a mais de 6 m do guaxinim deixam de ser desenhados na tela, e uma vinheta escurece as bordas. O jogador fica sem saber o que vem pela frente.
- **Não passa em vãos estreitos** (embaixo de prateleiras, buracos na parede), marcados com a layer `NarrowGap`.
- **Não cabe na lixeira.**

### 7.5 Efeito da fragilidade

- Carregar um item frágil enche uma barra de "chacoalhar" **enquanto o guaxinim corre**. Ela esvazia ao andar.
- Com a barra cheia (2 s de corrida seguida), o item quebra: perde o item, solta som de 10 m e deixa uma poça.
- É uma regra previsível, sem sorte: o jogador aprende a andar quando carrega vidro.

### 7.6 Largar item

- **Q** larga o último item pego no chão, fazendo um som de 3 m. Ele pode ser pego de novo depois.
- É a principal forma de fugir quando o guaxinim está carregado.

### 7.7 A toca (entrega)

- Um ponto fixo de cada fase (bueiro, buraco nos fundos ou janela).
- Ao entrar na área da toca, **todos os itens carregados são entregues** automaticamente e marcados na lista.
- Com a lista completa, entrar na toca termina a fase.

### 7.8 Tabela de itens (valores iniciais)

Lembrete: peso máximo 10 e volume máximo 6. O volume total igual ou maior que 4 aplica as penalidades da seção 7.4.

| Item | Categoria | Peso | Volume | Frágil |
| --- | --- | --- | --- | --- |
| Pão de forma | Normal | 1 | 1 | Não |
| Banana (cacho) | Normal | 1 | 1 | Não |
| Queijo | Normal | 1 | 1 | Não |
| Pacote de biscoito | Normal | 1 | 1 | Não |
| Caixa de leite | Normal | 1 | 1 | Não |
| Melancia | Pesado | 6 | 2 | Não |
| Galão de água | Pesado | 5 | 2 | Não |
| Saco de arroz 5 kg | Pesado | 5 | 2 | Não |
| Abóbora | Pesado | 4 | 2 | Não |
| Salgadinho gigante | Volumoso | 1 | 4 | Não |
| Fardo de papel higiênico | Volumoso | 2 | 5 | Não |
| Caixa de cereal | Volumoso | 1 | 3 | Não |
| Pipoca de micro-ondas (caixa grande) | Volumoso | 1 | 3 | Não |
| Garrafa de suco (vidro) | Frágil | 2 | 1 | Sim |
| Caixa de ovos | Frágil | 1 | 2 | Sim |
| Pote de geleia | Frágil | 1 | 1 | Sim |
| Azeite | Frágil | 2 | 1 | Sim |
| Energético | Consumível | — | — | — |
| Barra de proteína | Consumível | — | — | — |
| Cogumelo brilhante | Coletável do superpoder | — | — | — |

---

## 8. Consumíveis e superpoder

### 8.1 Consumíveis (efeito na hora em que pega)

| Item | Efeito | Duração |
| --- | --- | --- |
| **Energético** | +50% de velocidade, inclusive carregado | 8 s |
| **Barra de proteína** | Peso máximo sobe de 10 para 15, e a penalidade do peso cai pela metade: a fórmula vira `velocidadeBase × (1 − 0,25 × pesoAtual / pesoMáximo)`. Uma melancia passa a pesar bem menos na velocidade. | 15 s |

**Fim da barra de proteína com excesso de peso:** se o guaxinim estiver carregando mais que 10 quando o efeito acabar, ele não perde os itens, mas fica na velocidade mínima (40% da base) e não pode pegar mais nada até largar o excesso ou entregar na toca.

Os dois efeitos podem acontecer juntos (energético + proteína). Eles herdam de `ConsumableEffect` (classe base), e um ícone com o tempo restante aparece na HUD.

**Ideias guardadas para depois** (fora do escopo atual): bala de menta (correr sem barulho), biscoito da sorte (mostra os itens no mapa) e refrigerante (arroto que faz barulho).

### 8.2 Superpoder: modo cogumelo

- Cada fase tem **cogumelos brilhantes** espalhados, alguns em lugares arriscados (sugestão: 6 por fase).
- Ao juntar **3 cogumelos**, o poder fica **pronto** (o contador da HUD brilha e mostra "R"). O jogador **ativa quando quiser com R** e pode guardar o poder para o momento certo, como uma fuga.
- Cogumelos a mais depois de 3 não acumulam: o contador fica em 3 até o poder ser usado.
- Ativado, o guaxinim entra no **modo cogumelo** por **12 s**:
    - Pula para o topo da prateleira mais próxima.
    - **Anda por cima das gôndolas** a 7 m/s e **pula entre prateleiras** de corredores vizinhos (Espaço, até 3 m de distância).
    - **Não pode ser visto nem pego** enquanto está em cima.
    - **Não pode pegar itens** lá de cima: precisa descer para coletar.
    - Visual: cores saturadas, rastro colorido. Áudio: música distorcida e acelerada.
- **Descer:** Espaço na borda de uma prateleira (som de 5 m ao cair) ou automaticamente quando o tempo acaba, caindo no corredor mais próximo.
- O contador de cogumelos volta a zero após o uso.
- **Implementação:** o topo de cada gôndola tem um collider na layer `ShelfTop`. Durante o modo, o guaxinim passa a andar nessa layer, e os seguranças ignoram o jogador (`PlayerState.IsUntouchable`).

---

## 9. Esconderijo: lixeiras

- Lixeiras espalhadas pelo mapa. **E** entra e sai (0,5 s de animação).
- Dentro da lixeira, o guaxinim é invisível e não pode ser pego.
- **Se um segurança viu o guaxinim entrar** (estava em Perseguição com visão direta), ele abre a lixeira e o guaxinim é pego.
- Se o segurança perder o guaxinim de vista antes de ele entrar, ele procura em volta e desiste depois de 6 s.
- Não cabe com volume igual ou maior que 4 (seção 7.4).

---

## 10. Robô aspirador

| Comportamento | Detalhe |
| --- | --- |
| Movimento | Anda em linha reta a 1,2 m/s. Ao bater numa parede ou prateleira, gira de 90° a 180° (aleatório) e segue. |
| Esbarrão | Se trombar com o guaxinim, faz som de 4 m e o empurra um pouco. Não causa dano. |
| Montar | **E** do lado dele faz o guaxinim subir. WASD vira o robô, que continua na mesma velocidade fixa (2 m/s montado). Faz um som baixo contínuo de 2 m. **E** desce. |
| Uso estratégico | Atravessar áreas sem correr, levar carga pesada sem perder velocidade, ou soltá-lo vazio para fazer barulho longe do guaxinim. |

---

## 11. Fases (progressão de mercados)

Os nomes são paródias. Use paletas que lembrem os originais e **logos originais** (seção 0, regra 10). As cores finais serão confirmadas com as referências que o autor vai enviar.

O jogo tem **5 fases**. O mercado de orgânicos ficou de fora.

| # | Mercado (paródia) | Inspirado em | Tamanho | O que a fase ensina ou introduz | Inimigos |
| --- | --- | --- | --- | --- | --- |
| 1 | **Mercadinho do Seu Zé** | Mercado de bairro | Muito pequeno, 2 corredores | Tutorial: andar, correr e som, assobio, pegar, entregar na toca, lixeira | 1 segurança (o próprio Seu Zé com lanterna) |
| 2 | **XOXO 24h** | Loja de conveniência | Pequeno e aberto | Robô aspirador, itens frágeis, energético | 1 segurança, 1 aspirador |
| 3 | **Pão de Mel** | Supermercado de bairro nobre | Médio, 4 corredores | Cogumelos e modo cogumelo, itens volumosos | 2 seguranças, 1 aspirador |
| 4 | **Carrecinco** | Hipermercado | Grande | Itens pesados, barra de proteína, várias viagens até a toca, câmeras | 3 seguranças, 2 aspiradores, 2 câmeras |
| 5 | **Atacadaço** | Atacado | Enorme, prateleiras de pallets muito altas | Tudo junto, lista longa | 4 seguranças, 2 aspiradores, 4 câmeras |

**Lista de compras de cada fase** (itens da seção 7.8)

| # | Lista (obrigatória) | Itens bônus no mapa | Consumíveis no mapa | Cogumelos |
| --- | --- | --- | --- | --- |
| 1 | Pão de forma, banana, queijo | Pacote de biscoito | — | — |
| 2 | Caixa de leite, garrafa de suco, pote de geleia, pacote de biscoito | Queijo | 2 energéticos | — |
| 3 | Caixa de ovos, queijo, salgadinho gigante, caixa de cereal, azeite | Pipoca, pão de forma | 2 energéticos | 6 |
| 4 | Melancia, galão de água, saco de arroz, fardo de papel higiênico, pão de forma, banana, garrafa de suco | Abóbora, caixa de cereal | 2 energéticos, 2 barras de proteína | 6 |
| 5 | Melancia, abóbora, saco de arroz, galão de água, fardo de papel higiênico, salgadinho gigante, caixa de ovos, azeite, queijo, pipoca | Melancia extra, caixa de leite | 3 energéticos, 3 barras de proteína | 9 |

Nas fases 4 e 5, a soma do peso da lista passa bastante do limite, de propósito: o jogador precisa planejar as viagens até a toca e usar bem a barra de proteína.

**Escopo para o prazo:** o **MVP são as fases 1 a 3** completas. As fases 4 e 5 entram em seguida, reaproveitando o kit modular de gôndolas.

Cada fase é uma cena (`Level_01_Mercadinho`, etc.) com um `LevelData` (lista, tempo-alvo, número de cogumelos, música). Os mapas são montados com um **kit modular** de peças do Blender: gôndola, ponta de gôndola, freezer, caixa, parede, piso, porta, lixeira e toca.

---

## 12. Pontuação, progressão e save

**Estrelas por fase:**

- ⭐ Completar a lista e voltar à toca (obrigatório para passar).
- ⭐ Terminar abaixo do tempo-alvo.
- ⭐ Não disparar nenhum alerta geral.

**Bônus:** itens extras entregues somam pontos na tela de resultado.

**Progressão:** cada fase completa desbloqueia a próxima no mapa da cidade.

**Save:** fases desbloqueadas, estrelas e melhor tempo de cada fase, em `PlayerPrefs` (funciona no WebGL), por meio de um `SaveSystem` que guarda um JSON numa chave.

---

## 13. Telas e HUD

| Tela | Conteúdo |
| --- | --- |
| Menu principal | Jogar, Como jogar, Créditos, Sair (Sair escondido no WebGL) |
| Mapa da cidade | Fachadas dos mercados como botões; bloqueados em cinza; estrelas de cada fase |
| Início da fase | Nome do mercado e a lista de compras, com botão Começar |
| HUD | Lista (canto superior esquerdo), barras de peso e volume, contador de cogumelos (x/3, brilhando com "R" quando o poder está pronto), ícones de efeitos ativos, cronômetro, aviso de alerta geral |
| Pausa | Continuar, Reiniciar fase, Mapa |
| Pego | "Você foi pego!", e a fase reinicia após 1,5 s ou com um botão |
| Resultado | Estrelas, tempo, itens bônus, botões Próxima fase e Mapa |

Os **ícones dos itens** na HUD são renderizados a partir dos próprios modelos do Blender (render ortográfico de cada item em PNG transparente).

---

## 14. Arquitetura técnica

### 14.1 Tecnologia

- **Unity 6 LTS**, template **Universal 3D** (URP).
- Pacotes: **Input System**, **Cinemachine**, **AI Navigation** (NavMesh), **TextMeshPro**.
- Renderização: URP com **Forward+**, poucas luzes em tempo real (ver 14.6).

### 14.2 Pastas

```
Assets/_Project/
  Scenes/       Boot, MainMenu, CityMap, Level_01_Mercadinho, Level_02_XOXO, Level_03_PaoDeMel, Level_04_Carrecinco, Level_05_Atacadaco
  Scripts/
    Core/       GameManager, SceneLoader, SaveSystem, LevelData
    Player/     RaccoonController, PlayerState, Inventory, FragileShake
    Items/      ItemData, PickupItem, ShoppingList, Den
    Effects/    ConsumableEffect, EnergyDrinkEffect, ProteinBarEffect, MushroomPower
    Noise/      NoiseSystem, NoiseListener, NoiseRingVFX
    Guards/     GuardAI, GuardData, VisionCone, VisionConeMesh, DetectionMeter, AlertManager
    Hazards/    RobotVacuum, HidingSpot, SecurityCamera
    Camera/     CameraRig, ObstacleFader
    UI/         HUD, ShoppingListUI, LevelResultUI, PauseMenu, CityMapUI, MainMenuUI
  Data/         Items/, Guards/, Levels/
  Art/
    Models/     (FBX exportados do Blender)
    Materials/
    Icons/      (PNG dos itens)
  Audio/
  Prefabs/
```

### 14.3 Scripts principais

| Script | Responsabilidade |
| --- | --- |
| `RaccoonController` | Movimento com `CharacterController`, andar e correr, velocidade afetada por peso e efeitos, pulo no modo cogumelo. |
| `PlayerState` | Estado atual: normal, escondido, montado, modo cogumelo, pego. Expõe `IsVisible` e `IsUntouchable`. |
| `Inventory` | Itens carregados, soma de peso e volume, largar o último, entregar tudo na toca. |
| `FragileShake` | Barra de chacoalhar dos itens frágeis. |
| `ItemData` | Dados de cada item (`ScriptableObject`). |
| `PickupItem` | Item no mundo; ao interagir, vai para o inventário ou aplica o efeito. |
| `ShoppingList` | Lista da fase, o que já foi entregue, condição de vitória. |
| `Den` | Área da toca: entrega e fim da fase. |
| `MushroomPower` | Contagem de cogumelos, poder pronto, ativação com R e o modo cogumelo (troca de layer, timer, descida). |
| `EnergyDrinkEffect` / `ProteinBarEffect` | Efeitos temporários de velocidade e de capacidade de carga (seção 8.1). |
| `SecurityCamera` | Rotação em ciclo, cone de visão e disparo do alerta geral (seção 6.6). |
| `NoiseSystem` | `Emit(position, radius)`: avisa todos os `NoiseListener` no raio e cria o anel visual. |
| `GuardAI` | Máquina de estados (seção 6.2) com `NavMeshAgent`. |
| `VisionCone` + `VisionConeMesh` | Teste de visão por raycast e o desenho do cone no chão. |
| `DetectionMeter` | Medidor sobre a cabeça do segurança. |
| `AlertManager` | Alerta geral: avisa todos os seguranças e troca a música. |
| `RobotVacuum` | Movimento, esbarrão e modo montado. |
| `HidingSpot` | Lixeira: entrar e sair, regra de ser visto entrando. |
| `ObstacleFader` | Deixa transparentes as prateleiras entre a câmera e o guaxinim. |
| `LevelData` | Lista de compras, tempo-alvo, cogumelos e música da fase (`ScriptableObject`). |

### 14.4 Layers

| Layer | Uso |
| --- | --- |
| `Player` | O guaxinim |
| `Guard` | Seguranças |
| `Obstacle` | Bloqueia a visão (prateleiras altas, paredes, freezers) |
| `ShelfTop` | Topo das gôndolas, caminhável só no modo cogumelo |
| `NarrowGap` | Vãos onde só passa com volume menor que 4 |
| `Interactable` | Itens, lixeiras, aspirador, toca |

### 14.5 Comunicação

Eventos C# em vez de buscas na cena durante o jogo: `Inventory.OnChanged`, `ShoppingList.OnItemDelivered`, `AlertManager.OnAlertStarted`, `PlayerState.OnCaught`, `MushroomPower.OnModeStarted`.

### 14.6 Limites para WebGL

- Iluminação noturna **baked** (lightmaps) para o mercado, e só as **lanternas dos seguranças** em tempo real (no máximo 4 ou 5 luzes por cena).
- **Draw calls baixos:** modelos low poly, poucos materiais e **texturas pintadas agrupadas em atlas** por conjunto (personagens, itens, cenário), com static batching nas peças do cenário.
- Sem `System.Threading`; nada de shaders pesados de pós-processamento.
- **Build:** em Player Settings > Publishing Settings, **Compression Format = Disabled**, ou **Gzip** com **Decompression Fallback** marcado, para funcionar no itch.io.
- Testar o build no navegador ao fim de cada marco.

---

## 15. Direção de arte, Blender e áudio

### 15.1 Direção de arte

O jogo é em **3D, com câmera isométrica**, e tem um visual estilizado de filme de animação cinematográfico. As imagens 2D de conceito servem de referência para os modelos 3D. Personagens e cenários têm volume, proporções naturais e textura de material, com acabamento de pintura digital: nos modelos, isso vem de texturas pintadas à mão e de uma iluminação suave. O visual é expressivo, mas maduro: evite cabeças grandes, olhos enormes, formas de brinquedo e expressões infantis.

A história se passa em um supermercado à noite. A iluminação combina sombras frias em tons de roxo e azul com pontos de luz quente, como a lanterna do segurança e as luzes do mercado. Esse contraste cria uma atmosfera de furtividade e suspense leve, com espaço para humor. Os personagens devem parecer do mesmo mundo: o guaxinim tem pelagem detalhada e aparência esperta, os humanos têm proporções e roupas convincentes, e os objetos têm os materiais e o desgaste de um supermercado usado.

**Referência visual:** `docs/art/referencia_estilo.png` (folha com guaxinim, guaxinim no modo cogumelo, segurança, Seu Zé, robô aspirador e câmera). O **cajado** que aparece com o guaxinim na referência **não faz parte do personagem**: o guaxinim usa só a bolsa de couro transversal.

### 15.2 Leitura durante o jogo

- Cada elemento precisa ter uma silhueta reconhecível **vista de cima, em diagonal**, e em tamanho pequeno na tela.
- **Código de cores fixo:** guaxinim em tons quentes (cinza amarronzado e creme), inimigos em tons frios (azul-marinho), lanterna com luz quente amarelada, câmera de segurança com luz vermelha.
- Itens que podem ser pegos se destacam do cenário com cores mais saturadas ou um brilho sutil.
- Mantenha a paleta controlada e a direção da luz igual entre personagens, objetos e cenários.

### 15.3 Texto-base para gerar novas referências 2D

> Crie um elemento para um jogo 3D de furtividade e humor, com câmera isométrica, ambientado em um supermercado à noite. A imagem é arte conceitual 2D que servirá de referência para um modelo 3D. Siga o estilo visual da imagem de referência: arte estilizada com aparência de filme de animação cinematográfico, proporções naturais, formas bem definidas, volume e pintura digital com pinceladas suaves visíveis. O visual deve ser expressivo e ter personalidade, mas com um tom maduro, sem aparência infantil, cabeças exageradas ou formas de brinquedo.
>
> Use iluminação cinematográfica, com sombras frias em tons de roxo e azul e detalhes de luz quente. Aplique texturas que combinem com o material de cada elemento, como pelagem, tecido, metal, plástico ou madeira. Mantenha a paleta e a iluminação coerentes com o supermercado noturno. Mostre o elemento em vista 3/4 e também visto de cima, em diagonal, com uma silhueta clara e reconhecível mesmo em tamanho pequeno. Fundo neutro e liso. Não inclua texto, logotipos ou marcas.

### 15.4 Fluxo com o Blender

- Os modelos são criados no Blender por script Python (`bpy`), rodando em background (`blender -b -P script.py`) ou pela integração disponível. Cada script fica salvo em `Tools/Blender/` para poder ser rodado de novo.
- **Escala real em metros**, pivô na base do objeto e frente para −Y no Blender (vira +Z no Unity; verificado importando um FBX de teste no Unity 6.3).
- **Exportar FBX** para `Assets/_Project/Art/Models/` com o mesmo nome do prefab (`Item_Watermelon.fbx`, `Shelf_Aisle_2m.fbx`).
- **Limites de polígonos:** guaxinim até 3 mil triângulos, segurança até 3 mil, itens até 500, peças de cenário até 1.500.
- **Materiais:** texturas com aparência pintada à mão (cores base com variação de pincel, sombra e luz pintadas de leve), seguindo a seção 15.1. Para manter poucos draw calls no WebGL, agrupe as texturas de cada conjunto (personagens, itens, cenário) em atlas.
- **Animações:** rig simples do guaxinim e do segurança no Blender (andar, correr, carregar, pular, pego). Animações pequenas (balançar, quicar) podem ser feitas no Unity.
- **Ícones:** render ortográfico de cada item com fundo transparente, 256×256, em `Art/Icons/`.
- **Estilo:** proporções, paleta e acabamento seguem a seção 15.1 e a imagem de referência. Compare cada modelo com a referência renderizando-o com a câmera isométrica do jogo.

### 15.5 Lista de assets

- **Personagens:** guaxinim (com variação "modo cogumelo"), segurança (com uma variação por mercado: uniforme e cor), Seu Zé.
- **Kit modular:** gôndola de 2 m e 4 m, ponta de gôndola, prateleira alta de pallet, freezer horizontal e vertical, balcão de caixa, parede, piso, porta automática, lixeira, toca (bueiro).
- **Itens:** os 19 da tabela da seção 7.8, mais o cogumelo brilhante.
- **Fachadas e logos** dos 5 mercados para o mapa da cidade (logos originais, seção 0).
- **Robô aspirador** e **câmera de segurança** de teto.

### 15.6 Áudio

- Música: menu, furtividade (calma), alerta (tensa) e modo cogumelo (distorcida).
- Efeitos: passos andando e correndo, pegar item, largar item, vidro quebrando, assobio, aspirador, câmera detectando, lixeira (entrar e sair), detecção do segurança ("Ei!"), alerta geral, pego, entrega na toca, fase completa.
- Todo asset de terceiros ou gerado por IA vai para a lista de **créditos no README**, com a fonte e a licença.

---

## 16. Marcos (sozinho, cerca de 4 semanas)

### Semana 1: núcleo furtivo em greybox

- [ ] Projeto Unity 6 (URP), pacotes, pastas, Git e `.gitignore`.
- [ ] `RaccoonController` com andar e correr, e câmera isométrica com Cinemachine.
- [ ] `NoiseSystem` com anéis visíveis.
- [ ] `GuardAI` com patrulha, visão (cone visível), audição, medidor de detecção, perseguição e busca.
- [ ] Ser pego reinicia a fase.
- [ ] Fase 1 (Mercadinho) em greybox.

### Semana 2: saque e mundo

- [ ] `ItemData`, `PickupItem`, `Inventory` com peso, volume e fragilidade, e largar item.
- [ ] `ShoppingList`, `Den` e a condição de vitória.
- [ ] Energético e barra de proteína.
- [ ] Lixeiras e robô aspirador (com o modo montado).
- [ ] `ObstacleFader`.
- [ ] Fase 2 (XOXO) em greybox.

### Semana 3: superpoder, telas e progressão

- [ ] Cogumelos e modo cogumelo (poder pronto com 3, ativação com R, topo das prateleiras, pulo, descida).
- [ ] Alerta geral e câmeras de segurança.
- [ ] HUD, menu principal, mapa da cidade, pausa, resultado com estrelas, save.
- [ ] Fase 3 (Pão de Mel) em greybox.
- [ ] Primeiros modelos no Blender: guaxinim, segurança e kit modular.

### Semana 4: arte, áudio e entrega

- [ ] Modelos dos itens, ícones e fachadas; substituir o greybox nas fases 1 a 3.
- [ ] Iluminação baked, áudio e efeitos visuais.
- [ ] Balanceamento dos valores.
- [ ] Build WebGL testado no itch.io e README com controles e créditos.
- [ ] Fases 4 (Carrecinco) e 5 (Atacadaço).

---

## 17. Decisões em aberto

- [ ] Nome do jogo.
- [ ] Paleta exata de cada mercado (a confirmar com as referências).

**Já decidido** (refletido no documento):

- Estilo visual: Direção de arte e imagem de referência (seção 15.1).
- Consumíveis: energético e barra de proteína (seção 8.1).
- Modo cogumelo: fica pronto com 3 cogumelos e é ativado com R (seção 8.2).
- Assobio: mantido (seções 3.2 e 5).
- Câmeras de segurança: nas fases 4 e 5 (seção 6.6).
- Itens e valores: tabela da seção 7.8 e listas da seção 11, a balancear nos testes.
- Fases: 5 no total, sem o mercado de orgânicos; o MVP são as 3 primeiras (seção 11).
