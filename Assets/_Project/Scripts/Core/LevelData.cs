using System.Collections.Generic;
using Guaxinim.Items;
using UnityEngine;

namespace Guaxinim.Core
{
    /// <summary>
    /// Dados de uma fase (GDD §11, §12 e §14.3): identidade (save e cena), lista de compras e tempo-alvo da
    /// estrela. Somente leitura durante o jogo.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelData", menuName = "Guaxinim/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Tooltip("Identificador da fase no save. Não mude depois de publicar: o progresso fica ligado a ele")]
        [SerializeField] private string _id = string.Empty;

        [Tooltip("Nome da cena da fase no Build Profile. Vazio: a fase ainda não tem cena e aparece como \"Em breve\"")]
        [SerializeField] private string _sceneName = string.Empty;

        [Tooltip("Nome que o jogador vê")]
        [SerializeField] private string _displayName = "Fase";

        [Tooltip("Itens obrigatórios (GDD §7.1: de 3 a 10). Repetir um item pede mais de uma unidade")]
        [SerializeField] private ItemData[] _shoppingList = new ItemData[0];

        [Tooltip("Segundos: terminar abaixo disto dá uma estrela (GDD §12)")]
        [SerializeField, Min(1f)] private float _targetTimeSeconds = 120f;

        [Tooltip("Dicas do tutorial desta fase (a Fase 1). Vazio: a fase não mostra dicas")]
        [SerializeField] private TutorialTipsData _tips;

        public string Id => _id;
        public string SceneName => _sceneName;
        public bool IsPlayable => !string.IsNullOrEmpty(_sceneName);
        public string DisplayName => _displayName;
        public float TargetTimeSeconds => _targetTimeSeconds;
        public TutorialTipsData Tips => _tips;
        public IReadOnlyList<ItemData> ShoppingList => _shoppingList;
    }
}
