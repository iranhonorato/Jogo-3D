using System.Collections.Generic;
using Guaxinim.Items;
using UnityEngine;

namespace Guaxinim.Core
{
    /// <summary>
    /// Dados de uma fase (GDD §11 e §14.3). Por enquanto, a lista de compras; tempo-alvo, cogumelos e
    /// música entram com as estrelas e o modo cogumelo. Somente leitura durante o jogo.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelData", menuName = "Guaxinim/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Tooltip("Nome que o jogador vê")]
        [SerializeField] private string _displayName = "Fase";

        [Tooltip("Itens obrigatórios (GDD §7.1: de 3 a 10). Repetir um item pede mais de uma unidade")]
        [SerializeField] private ItemData[] _shoppingList = new ItemData[0];

        public string DisplayName => _displayName;
        public IReadOnlyList<ItemData> ShoppingList => _shoppingList;
    }
}
