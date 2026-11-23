using System.Collections.Generic;
using Guaxinim.Items;
using UnityEngine;

namespace Guaxinim.Core
{
    /// <summary>
    /// Dados de uma fase (GDD §11, §12 e §14.3): lista de compras e tempo-alvo da estrela. Cogumelos e
    /// música entram com o modo cogumelo e o áudio. Somente leitura durante o jogo.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelData", menuName = "Guaxinim/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Tooltip("Nome que o jogador vê")]
        [SerializeField] private string _displayName = "Fase";

        [Tooltip("Itens obrigatórios (GDD §7.1: de 3 a 10). Repetir um item pede mais de uma unidade")]
        [SerializeField] private ItemData[] _shoppingList = new ItemData[0];

        [Tooltip("Segundos: terminar abaixo disto dá uma estrela (GDD §12)")]
        [SerializeField, Min(1f)] private float _targetTimeSeconds = 120f;

        public string DisplayName => _displayName;
        public float TargetTimeSeconds => _targetTimeSeconds;
        public IReadOnlyList<ItemData> ShoppingList => _shoppingList;
    }
}
