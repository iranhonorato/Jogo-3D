using UnityEngine;

namespace Guaxinim.Effects
{
    /// <summary>
    /// Asset com os números de um efeito e a fábrica dele (Factory Method): o ItemData do consumível aponta
    /// para cá, e quem pega o item só chama <see cref="CreateEffect"/>, sem saber o tipo.
    /// </summary>
    public abstract class ConsumableEffectData : ScriptableObject
    {
        [Tooltip("Nome que o jogador vê na HUD")]
        [SerializeField] private string _displayName = "Efeito";

        [Tooltip("Segundos de duração")]
        [SerializeField, Min(0.1f)] private float _durationSeconds = 8f;

        public string DisplayName => _displayName;
        public float DurationSeconds => _durationSeconds;

        public abstract ConsumableEffect CreateEffect();
    }
}
