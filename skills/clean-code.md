---
name: clean-code
description: Regras de código limpo para o C# do Operação Guaxinim (Unity 6, C# 9) — nomes legíveis, organização de classes e métodos, Unity sem sujeira, comentários, erros, formatação e testes. Use ao escrever, revisar ou refatorar qualquer script do projeto.
---

# Skill: Clean Code

Objetivo: código que outra pessoa (ou outra sessão do Claude) entende de primeira, consegue testar e muda sem medo. Vale para todo script em `Assets/_Project/Scripts/` e `Assets/_Project/Tests/`. As convenções do [README](../README.md) continuam valendo; esta skill as detalha. Os padrões de arquitetura estão em [game-design-patterns.md](game-design-patterns.md).

## 1. Princípios

- **Legível antes de esperto.** Se o código precisa de um comentário para explicar *o que* faz, renomeie ou quebre em partes.
- **Uma responsabilidade por classe e por método.** Se o nome precisa de "And" (`MoveAndPlaySound`), são duas coisas.
- **KISS e YAGNI.** Resolva o passo atual do plano. Nada de parâmetros, abstrações ou "ganchos para depois" que o passo não pede.
- **DRY com bom senso.** Repetiu duas vezes, tudo bem; na terceira, extraia. Não junte código que só *parece* igual, mas muda por motivos diferentes.
- **Deixe melhor do que encontrou**, só no que o passo toca. Refatoração grande é um passo próprio, combinado com o usuário.

## 2. Nomes

### Convenções

| Elemento | Estilo | Exemplo |
| --- | --- | --- |
| Classe, struct, enum | PascalCase, substantivo | `GuardAI`, `NoiseEvent`, `PlayerMode` |
| Interface | `I` + PascalCase | `IInteractable`, `IDetectable` |
| Método | PascalCase, verbo | `DeliverAll()`, `TryPickUp(item)` |
| Propriedade | PascalCase | `CurrentWeight`, `IsVisible` |
| Campo privado, inclusive `[SerializeField]` | `_camelCase` | `_walkSpeed`, `_visionCone` |
| Parâmetro e variável local | camelCase | `noiseRadius`, `closestItem` |
| Constante e `static readonly` | PascalCase | `MaxCarriedItems`, `SpeedParam` |
| Evento | `On` + acontecimento (padrão do GDD §14.5) | `OnChanged`, `OnItemDelivered`, `OnCaught` |
| Método que trata um evento | `Handle` + acontecimento | `HandleItemDelivered` |

O Inspector esconde o `_` (o campo `_walkSpeed` aparece como "Walk Speed").

### Regras

- **Nomes revelam intenção:** `closestInteractable`, não `obj`; `elapsedChaseTime`, não `t`.
- **Unidade sempre clara:** `[Tooltip("Segundos")]` nos campos do Inspector, e sufixo (`durationSeconds`, `radiusMeters`) em parâmetros quando houver dúvida.
- **Booleanos como pergunta:** `IsHidden`, `HasLineOfSight`, `CanPickUp`. Nunca na negativa (`IsNotVisible`).
- **Sem abreviações**, exceto as universais (`id`, `UI`, `HUD`, `VFX`, `AI`). `i` e `j` só em laços curtos.
- **Coleções no plural:** `_listeners`, `carriedItems`. Sem o tipo no nome (`itemList` vira `items`) e sem notação húngara (`fSpeed`, `strName`).
- **Métodos que podem falhar sem exceção** começam com `Try`, retornam `bool` e entregam o resultado por `out`: `TryGetClosestInteractable(out IInteractable target)`.
- **Renomear um campo serializado apaga o valor no Inspector.** Use `[FormerlySerializedAs("nomeAntigo")]` ao renomear.

### Glossário do domínio

Cada conceito do GDD tem **um só** nome em inglês no código. Os nomes de classe da §14.3 do GDD são canônicos.

| GDD | Código | GDD | Código |
| --- | --- | --- | --- |
| guaxinim | `Raccoon` | lixeira (esconderijo) | `HidingSpot` |
| segurança | `Guard` | robô aspirador | `RobotVacuum` |
| faxineiro | `Janitor` | | |
| toca | `Den` | montar / descer | `Mount` / `Dismount` |
| lista de compras | `ShoppingList` | câmera de segurança | `SecurityCamera` |
| pegar / largar / entregar | `PickUp` / `Drop` / `Deliver` | cogumelo / modo cogumelo | `Mushroom` / `MushroomMode` |
| peso / volume / frágil | `Weight` / `Volume` / `Fragile` | energético / barra de proteína | `EnergyDrink` / `ProteinBar` |
| barra de chacoalhar | `Shake` | consumível / efeito | `Consumable` / `Effect` |
| barulho, som | `Noise` | gôndola / topo da gôndola | `Shelf` / `ShelfTop` |
| assobio | `Whistle` | vão estreito | `NarrowGap` |
| cone de visão | `VisionCone` | fase / mercado | `Level` / `Store` |
| medidor de detecção | `DetectionMeter` | mapa da cidade | `CityMap` |
| patrulha / desconfiado / perseguição / busca / retorno | `Patrol` / `Suspicious` / `Chase` / `Search` / `Return` | estrelas / tempo-alvo | `Stars` / `TargetTime` |
| alerta geral | `Alert` (`AlertManager`) | ser pego | `Caught` |

## 3. Métodos

- **Curtos e com um só nível de abstração.** Passou de ~20 linhas ou de 2 níveis de indentação, extraia métodos cujos nomes contam a história.
- **No máximo 3 parâmetros.** Mais que isso, agrupe num `struct` (`NoiseEvent`).
- **Sem parâmetro booleano que troca o comportamento:** `Drop(bool silent)` vira `Drop()` e `DropSilently()`.
- **Retorno antecipado** (guard clause) em vez de `if` aninhado.
- **Propriedades não têm efeito colateral.** Ler `IsVisible` nunca muda estado.
- **Pergunta ou comando:** um método responde algo *ou* muda estado, não os dois. A exceção é o padrão `Try...`.

```csharp
// Ruim: aninhado, número mágico, conhece cada tipo de objeto
public void Interact()
{
    if (_nearest != null)
    {
        if (_nearest.GetComponent<PickupItem>() != null)
        {
            if (_inventory.Weight + _nearest.GetComponent<PickupItem>().Data.Weight <= 10)
            {
                // ...
            }
        }
    }
}

// Bom: retorno antecipado, polimorfismo, regra de capacidade fica no Inventory
public void Interact()
{
    if (!TryGetClosestInteractable(out IInteractable target))
    {
        return;
    }

    target.Interact(_interactor);
}
```

## 4. Classes e arquivos

- Uma classe por arquivo, com o nome do arquivo igual ao da classe, e namespace `Guaxinim.<Módulo>` igual à pasta. O Unity usa **C# 9**: namespace com chaves, nunca `namespace X;`.
- **MonoBehaviour fino.** Ele liga a cena (input, eventos, Inspector) à lógica, que fica em classes C# comuns testáveis em EditMode:

```csharp
using UnityEngine;

namespace Guaxinim.Player
{
    /// <summary>GDD §7.3: velocidade = base × (1 − penalidade × peso / pesoMáximo).</summary>
    public static class CarrySpeed
    {
        public static float Calculate(float baseSpeed, float currentWeight, float maxWeight, float weightPenalty)
        {
            if (maxWeight <= 0f)
            {
                return baseSpeed;
            }

            float load = Mathf.Clamp01(currentWeight / maxWeight);
            return baseSpeed * (1f - weightPenalty * load);
        }
    }
}
```

  A penalidade (0,5, ou 0,25 com a barra de proteína) chega por parâmetro, vinda do `ScriptableObject`. Nada de número mágico.

- **Ordem dos membros:**
  1. Constantes e campos estáticos
  2. Campos `[SerializeField]`, agrupados com `[Header]`
  3. Campos privados
  4. Eventos
  5. Propriedades
  6. Mensagens da Unity, na ordem do ciclo de vida: `Awake`, `OnEnable`, `Start`, `Update`, `FixedUpdate`, `LateUpdate`, `OnDisable`, `OnDestroy`; depois `OnTrigger*`, `OnCollision*`, `OnValidate` e `OnDrawGizmos`
  7. Métodos públicos
  8. Métodos privados, perto de quem os chama
- **Encapsulamento:** campos sempre privados. Exponha o mínimo com `{ get; private set; }`, e coleções como `IReadOnlyList<T>`.

## 5. Unity sem sujeira

- **Referências:** `[SerializeField]` para o que é montado no Inspector; `GetComponent` no `Awake`, com `[RequireComponent]`, para componentes do mesmo objeto. Nunca `GetComponent`, `Find*` ou `Camera.main` dentro de `Update`.
- **Valide cedo:** referência obrigatória vazia gera `Debug.LogError("...", this)` no `Awake` (o `this` destaca o objeto no Editor). Use `OnValidate` para checar valores do Inspector.
- **O "null falso" da Unity:** um `UnityEngine.Object` destruído finge ser `null` só para `==`. Não use `?.`, `??` nem `is null` com ele; use `if (obj != null)` ou `TryGetComponent`. Em classes C# comuns, `?.` funciona normalmente.
- **Eventos sempre em par:** assine no `OnEnable` e cancele no `OnDisable`. Um evento esquecido vira erro depois de reiniciar a fase.
- **Layers e tags:** `LayerMask` serializado em vez de número de layer no código, e `CompareTag` em vez de `tag ==`. Se o código precisar do nome de uma layer, ele fica num só lugar (`Shared/Layers.cs`).
- **Animator:** `private static readonly int SpeedParam = Animator.StringToHash("Speed");`, nunca string a cada frame.
- **Sem alocação por frame** (o WebGL sente o coletor de lixo): nada de LINQ, `new List`, concatenação de string ou lambdas que capturam variáveis dentro de `Update`. Use as versões `NonAlloc` da física e reaproveite buffers.
- **ScriptableObject é só leitura durante o jogo.** Estado que muda (peso atual, tempo restante de um efeito) fica no componente. Mudanças num SO feitas no Play Mode ficam salvas no asset.
- **Inspector legível:** `[Header]`, `[Tooltip]` (com a unidade), `[Min]` e `[Range]` nos campos de dados.
- **Logs em inglês** (fazem parte do código). Texto que o jogador vê, em português.

## 6. Comentários

- O código diz *o quê*; o comentário diz *por quê*: `// GDD §6.5: sem carga, o guaxinim precisa ser mais rápido que o segurança.`
- Cite a seção do GDD quando a regra vem de lá.
- `///` (XML doc) na API pública que outros módulos usam (`NoiseSystem.Emit`, `Inventory.TryAdd`).
- Nada de código comentado (o Git guarda o histórico) nem de comentário que repete o código.
- `TODO` só com contexto e passo do plano: `// TODO(passo 3.2): ignorar o jogador no modo cogumelo.`

## 7. Erros e estados robustos

A rubrica 4 da disciplina avalia começar, perder, vencer e reiniciar sem ficar preso.

- **Falhe alto em desenvolvimento:** `Debug.LogError` e `Debug.Assert`. Nunca `catch` vazio, nunca exceção como fluxo normal.
- **Transições idempotentes:** ser pego duas vezes no mesmo frame, entregar duas vezes, apertar Esc na tela de resultado. Cada caso tem uma guarda (`if (_isCaught) { return; }`).
- **Todo estado tem saída:** timers terminam, telas têm botão, a fase reinicia. Pergunte sempre: "o jogador pode ficar preso aqui?"

## 8. Formatação

- Chaves no estilo Allman (cada `{` na própria linha) e 4 espaços de indentação.
- Sempre use chaves, mesmo em `if` de uma linha.
- `var` só quando o tipo é óbvio pelo lado direito (`var items = new List<ItemData>();`).
- `=>` para propriedades e métodos de uma linha.
- `using` no topo, `System` primeiro, sem `using` sobrando.

## 9. Testes

- Lógica pura (fórmulas, regras, transições, estrelas, save) tem teste em `Tests/EditMode/`, namespace `Guaxinim.Tests`.
- Nome no formato `Metodo_Cenario_Resultado`: `Calculate_WithMaxWeight_ReturnsHalfSpeed`.
- Estrutura Arrange, Act, Assert; um comportamento por teste; valores do GDD nos casos, com tolerância para `float` (`Assert.AreEqual(1.5f, speed, 0.001f)`).

## Checklist antes de entregar um passo

- [ ] Nomes seguem as convenções e o glossário
- [ ] Nenhum número mágico: valores no `ScriptableObject` ou em constantes nomeadas
- [ ] Métodos curtos, sem `if` aninhado profundo
- [ ] Eventos assinados e cancelados em par
- [ ] Nada de `Find*`, `GetComponent` ou alocação dentro de `Update`
- [ ] Sem `Debug.Log` de depuração, código comentado ou `using` sobrando
- [ ] Lógica pura com teste EditMode
- [ ] Nenhum estado sem saída
