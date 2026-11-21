---
name: game-design-patterns
description: Padrões de projeto de jogos aplicados ao Operação Guaxinim (Unity 6) — quando usar Component, State, Observer, Mediator, Singleton, ScriptableObject, Strategy, Factory, Object Pool e interfaces, onde cada um entra no projeto e quais evitar. Use ao criar ou mudar a estrutura de qualquer sistema.
---

# Skill: Padrões de projeto de jogos

**Regra de ouro: um padrão resolve um problema que existe agora.** Antes de aplicar um, diga qual problema ele resolve no passo atual. Se a resposta for "pode ser útil depois", não aplique (YAGNI, ver [clean-code.md](clean-code.md)). A tabela de padrões do [README](../README.md) continua valendo; esta skill diz como aplicá-los.

## Mapa rápido

| Problema | Padrão | Onde no projeto |
| --- | --- | --- |
| Montar uma entidade com várias responsabilidades | Component | Guaxinim = `RaccoonController` + `PlayerState` + `Inventory` + `FragileShake` + `MushroomPower` |
| O comportamento muda conforme a situação | State | `Guards/States/`, `PlayerState` |
| Vários sistemas reagem a um acontecimento | Observer (eventos C#) | `Inventory.OnChanged`, `ShoppingList.OnItemDelivered`, `PlayerState.OnCaught` |
| Quem emite não deve conhecer quem reage | Mediator | `NoiseSystem`, `AlertManager` |
| Um serviço único que atravessa cenas | Singleton, com moderação | `GameManager`, `SceneLoader`, música |
| Muitas variações do mesmo tipo de dado | Type Object com `ScriptableObject` | `ItemData`, `GuardData`, `LevelData` |
| Variações de comportamento com o mesmo contrato | Template Method e Strategy | `ConsumableEffect`, reação ao medidor de detecção |
| O tipo do objeto criado depende de dados | Factory | Efeito criado a partir do `ItemData`; `MenuItem` que gera assets |
| Criar e destruir muitos objetos iguais | Object Pool | Anéis de som, poças, partículas de vidro |
| Tratar coisas diferentes do mesmo jeito | Interface | `IInteractable` (item, lixeira, aspirador, toca), `IDetectable` |

## Component

- **Composição antes de herança.** Cada componente tem uma responsabilidade, e a entidade é a soma deles.
- Herança só onde existe um contrato claro e variações reais: os estados do segurança e o `ConsumableEffect`. Nada de `Guard : Enemy : Character : Entity`.
- Componentes do mesmo objeto se encontram no `Awake` (`GetComponent` com `[RequireComponent]`). Entre objetos diferentes, por referência no Inspector, evento ou interface.

## Update Method

- Só tem `Update` quem muda a cada frame. Um componente que só reage a eventos não tem `Update`.
- Timers simples ficam no próprio dono (um `float` decrementado no `Update`, ou uma coroutine). Não crie um "TimerManager" global.

## State

- **Segurança:** uma classe por estado (`PatrolState`, `SuspiciousState`, `ChaseState`, `SearchState`, `ReturnState`), todas com o mesmo contrato. O `GuardAI` só guarda o estado atual e faz a troca; cada estado decide quando sair.
- Estados são classes C# comuns (não `MonoBehaviour`), recebem o `GuardAI` no construtor e são criados uma vez no `Awake`, sem alocar a cada troca.
- **Jogador:** o `PlayerState` pode começar como um `enum` (normal, escondido, montado, modo cogumelo, pego) com um único método de troca que valida a transição e dispara o evento. Promova para classes só quando cada modo ganhar comportamento próprio.

```csharp
namespace Guaxinim.Guards
{
    public interface IGuardState
    {
        void Enter();
        void Tick(float deltaTime);
        void Exit();
    }
}
```

```csharp
using UnityEngine;

namespace Guaxinim.Guards
{
    public class GuardAI : MonoBehaviour
    {
        private IGuardState _currentState;

        public PatrolState Patrol { get; private set; }
        public ChaseState Chase { get; private set; }

        private void Awake()
        {
            Patrol = new PatrolState(this);
            Chase = new ChaseState(this);
        }

        private void Start() => ChangeState(Patrol);

        private void Update() => _currentState.Tick(Time.deltaTime);

        public void ChangeState(IGuardState nextState)
        {
            if (nextState == _currentState)
            {
                return;
            }

            _currentState?.Exit(); // pode usar ?. : IGuardState não é UnityEngine.Object
            _currentState = nextState;
            _currentState.Enter();
        }
    }
}
```

## Observer (eventos C#)

- O dono do dado declara `public event Action<T> OnAlgo;` e dispara com `OnAlgo?.Invoke(...)`.
- Quem escuta assina no `OnEnable` e cancela no `OnDisable`, sempre em par.
- O evento carrega o que o ouvinte precisa (o item entregue, a carga nova), para ele não ter que perguntar de volta.
- **UI e áudio são sempre ouvintes.** A lógica de gameplay nunca chama a UI.
- Nenhum ouvinte pode depender de outro ter rodado antes: a ordem de disparo não é garantida.
- `UnityEvent` só quando a ligação precisa ser feita no Inspector (por exemplo, um gatilho de tutorial). Entre scripts, use evento C#.

```csharp
// ShoppingList
public event Action<ItemData> OnItemDelivered;

private void MarkDelivered(ItemData item)
{
    _delivered.Add(item);
    OnItemDelivered?.Invoke(item);
}

// ShoppingListUI
private void OnEnable() => _shoppingList.OnItemDelivered += HandleItemDelivered;
private void OnDisable() => _shoppingList.OnItemDelivered -= HandleItemDelivered;
```

## Mediator

- **`NoiseSystem`:** quem faz barulho chama `NoiseSystem.Emit(position, radius)` sem saber quem ouve. Cada `NoiseListener` se registra no `OnEnable` e sai no `OnDisable`. Como o sistema não tem dados de cena, pode ser uma classe estática com a lista de ouvintes e um evento `OnNoiseEmitted`; o anel visual (`NoiseRingVFX`) é só mais um ouvinte desse evento.
- **`AlertManager`:** quem detecta (segurança ou câmera) chama o alerta com a posição, e todos os seguranças são avisados. A câmera nunca conhece os seguranças. Como ele tem o timer de 8 s, é um componente da cena da fase.
- **Mediadores são específicos por assunto.** Não crie um `EventBus` genérico com eventos identificados por string: perde a tipagem e ninguém sabe quem escuta o quê.

## Singleton

Use pouco. Um singleton é uma variável global com outro nome.

- **Só para serviços únicos e globais que vivem entre cenas:** `GameManager` (ciclo da fase: começar, pausar, perder, vencer), `SceneLoader` e o tocador de música.
- **Se não precisa de Inspector, `Update` nem coroutine, não precisa ser `MonoBehaviour`.** O `SaveSystem`, que só usa `PlayerPrefs`, pode ser uma classe estática.
- **Gerenciadores de fase** (como o `AlertManager`) vivem na cena, sem `DontDestroyOnLoad`. Reiniciar a fase recarrega a cena e zera o estado deles de graça, o que é bom para a rubrica 4.
- **Singleton persistente nunca guarda estado da fase** (itens carregados, alerta ativo); senão o reinício herda lixo.
- **Nunca** para coisas com várias instâncias (seguranças, itens) nem para "facilitar o acesso" ao jogador: use referência, evento ou interface.
- **Jogar a partir de qualquer cena:** os singletons persistentes ficam num prefab criado por um método `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]`, para o Play funcionar direto numa fase sem passar pela cena Boot.
- **Zere o estado estático** com `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]`. Hoje o projeto recarrega o domínio ao entrar no Play Mode, mas o código precisa continuar certo se alguém desligar isso para iterar mais rápido. Vale também para o registro estático do `NoiseSystem`.

```csharp
using UnityEngine;

namespace Guaxinim.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject); // só funciona num objeto raiz da cena
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Instance = null;
    }
}
```

## Type Object com ScriptableObject

- **Um tipo de dado, muitos assets:** um `ItemData` para cada item, um `GuardData` para cada variação de segurança e um `LevelData` por fase. Item novo é asset novo, não classe nova.
- A categoria vem dos dados (`weight`, `volume`, `isFragile`, `isConsumable`), não de subclasses como `FragileItem : Item`.
- **Os assets são compartilhados** (Flyweight): dez bananas no mapa apontam para o mesmo `ItemData`.
- **Somente leitura durante o jogo.** Estado que muda fica no componente.

## Template Method e Strategy

- **`ConsumableEffect`** (classe base abstrata) controla o ciclo e a duração: aplicar, contar o tempo e desfazer. Cada subclasse implementa só o que é dela (`Apply` e `Revert`).
- **Efeitos não escrevem direto nos atributos do jogador.** Eles registram modificadores que o jogador combina; assim o energético e a barra de proteína funcionam juntos sem um sobrescrever o outro (GDD §8.1).
- **Reação ao medidor de detecção:** o `DetectionMeter` é o mesmo no segurança e na câmera de segurança. Muda só quem escuta o "medidor cheio": o segurança entra em Perseguição e a câmera dispara o alerta geral. É composição, não herança.

```csharp
namespace Guaxinim.Effects
{
    public abstract class ConsumableEffect
    {
        protected ConsumableEffect(float durationSeconds) => RemainingSeconds = durationSeconds;

        public float RemainingSeconds { get; private set; }
        public bool IsFinished => RemainingSeconds <= 0f;

        public void Tick(float deltaTime)
        {
            if (IsFinished)
            {
                return;
            }

            RemainingSeconds -= deltaTime;

            if (IsFinished)
            {
                Revert();
            }
        }

        public abstract void Apply();
        protected abstract void Revert();
    }
}
```

## Factory

- **Use quando o tipo do objeto criado depende de dados.** Por exemplo, o `ItemData` de um consumível aponta para um asset de efeito com um método `CreateEffect()` que devolve a instância certa (Factory Method). Quem pega o item não precisa de um `switch` por tipo.
- **Ferramentas de Editor como fábricas de assets:** um `MenuItem` que gera os `ItemData` da tabela §7.8 ou os prefabs de greybox é reproduzível e evita escrever YAML à mão.
- **Não crie uma fábrica para um `Instantiate` simples** de um único prefab.
- **Item largado:** o objeto do mundo é desativado ao ser pego e reativado ao ser largado, em vez de `Destroy` seguido de `Instantiate`.

## Object Pool

- Use o `UnityEngine.Pool.ObjectPool<T>` da própria Unity para objetos criados com frequência: anéis de som (correr emite vários por segundo), poças e partículas de vidro.
- O objeto devolvido ao pool volta ao estado inicial (`actionOnGet` e `actionOnRelease`).
- Não use pool para o que nasce uma vez por fase (seguranças, itens).
- Motivo: no WebGL, `Instantiate` e `Destroy` em sequência geram picos do coletor de lixo.

```csharp
_ringPool = new ObjectPool<NoiseRingVFX>(
    createFunc: () => Instantiate(_ringPrefab),
    actionOnGet: ring => ring.gameObject.SetActive(true),
    actionOnRelease: ring => ring.gameObject.SetActive(false),
    defaultCapacity: 8);
```

## Interfaces e inversão de dependência

- **Contratos em `Shared/` quebram dependências entre módulos.** Os seguranças dependem de algo detectável (`IDetectable`: posição, `IsVisible`, `IsUntouchable`), não do `RaccoonController`.
- **Tudo que responde ao E implementa `IInteractable`** (item, lixeira, aspirador, toca). O jogador escolhe o mais próximo e chama `Interact`, sem `if` por tipo. Para achar o contrato, use `TryGetComponent<IInteractable>(out var target)`.
- **Interfaces facilitam testes:** um `IDetectable` falso testa a visão sem precisar do guaxinim.
- **O Inspector não serializa campos de interface.** Serialize o componente concreto ou busque a interface com `GetComponent` no `Awake`.
- **Crie uma interface quando houver duas ou mais implementações ou uma fronteira entre módulos.** Não crie `IInventory` para uma classe única usada só dentro do próprio módulo.

## Padrões que não usamos

| Padrão | Por que não |
| --- | --- |
| Command (input como objetos, desfazer, replay) | O jogo não tem desfazer nem replay; o `InputReader` com eventos basta. |
| Service Locator e frameworks de injeção (Zenject, VContainer) | Complexidade sem retorno num projeto curto e de uma pessoa. |
| `EventBus` genérico por string | Ver Mediator. |
| ECS/DOTS | Poucos agentes em cena; `MonoBehaviour` dá conta. |
| Spatial partition | No máximo 4 seguranças: percorrer a lista é suficiente. |

## Antipadrões

- **God object:** um `GameManager` que move o jogador, toca música e conta pontos. Ele cuida só do ciclo da fase e delega o resto.
- `switch` gigante de estados dentro do `Update`.
- `FindObjectOfType` ou `GameObject.Find` para achar sistemas durante o jogo.
- Singletons acessados de todo lugar como variáveis globais.
- Estado estático que sobrevive ao reinício da fase.
- `ScriptableObject` modificado durante o jogo.
- Herança profunda e uma classe `Manager` para cada coisa. Um `Manager` só existe se coordena várias instâncias (como o `AlertManager`).

## Checklist de arquitetura

- [ ] Cada padrão novo resolve um problema do passo atual, e a resposta diz qual
- [ ] Dependências apontam para `Shared/` ou para dentro do módulo; gameplay não conhece a UI
- [ ] Módulos se falam por evento, mediador ou interface, nunca por busca na cena
- [ ] Nenhum singleton novo sem justificar por que ele é único e global
- [ ] O estado da fase morre com a cena (reinício limpo)
- [ ] Objetos criados com frequência vêm de um pool
