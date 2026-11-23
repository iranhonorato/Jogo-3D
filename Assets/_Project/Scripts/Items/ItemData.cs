using Guaxinim.Effects;
using UnityEngine;

namespace Guaxinim.Items
{
    /// <summary>
    /// Um tipo de item (GDD §7.2 e §7.8). A categoria vem dos dados (peso, volume, frágil, consumível),
    /// não de subclasses. Dez bananas no mapa apontam para o mesmo asset. Somente leitura durante o jogo.
    /// </summary>
    [CreateAssetMenu(fileName = "Item", menuName = "Guaxinim/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Tooltip("Nome que o jogador vê")]
        [SerializeField] private string _displayName = "Item";

        [SerializeField] private Sprite _icon;

        [Tooltip("Unidades de peso (o guaxinim carrega até 10)")]
        [SerializeField, Min(0f)] private float _weight = 1f;

        [Tooltip("Unidades de volume (o guaxinim carrega até 6; 4 ou mais tapa a visão)")]
        [SerializeField, Min(0f)] private float _volume = 1f;

        [Tooltip("Quebra se o guaxinim correr com ele (GDD §7.5)")]
        [SerializeField] private bool _isFragile;

        [Tooltip("Usado na hora, ao pegar (GDD §8.1)")]
        [SerializeField] private bool _isConsumable;

        [Tooltip("Efeito aplicado ao pegar, se for consumível")]
        [SerializeField] private ConsumableEffectData _effect;

        public string DisplayName => _displayName;
        public Sprite Icon => _icon;
        public float Weight => _weight;
        public float Volume => _volume;
        public bool IsFragile => _isFragile;
        public bool IsConsumable => _isConsumable;
        public ConsumableEffectData Effect => _effect;
    }
}
