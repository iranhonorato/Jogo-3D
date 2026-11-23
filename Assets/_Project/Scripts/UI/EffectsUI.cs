using System.Collections.Generic;
using System.Text;
using Guaxinim.Effects;
using Guaxinim.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Guaxinim.UI
{
    /// <summary>
    /// Efeitos ativos e o tempo restante na HUD (GDD §8.1 e §13). O texto só é refeito quando o número de
    /// segundos inteiros muda, para não alocar a cada frame.
    /// </summary>
    public class EffectsUI : MonoBehaviour
    {
        [Tooltip("Efeitos do guaxinim (ligado na montagem da fase)")]
        [SerializeField] private RaccoonEffects _effects;

        [SerializeField] private Text _text;

        private readonly StringBuilder _builder = new StringBuilder();
        private int _lastSignature = -1;

        private void Awake()
        {
            if (_effects == null || _text == null)
            {
                Debug.LogError("EffectsUI is missing references.", this);
                enabled = false;
            }
        }

        private void Update()
        {
            IReadOnlyList<ConsumableEffect> active = _effects.Active.Effects;
            int signature = ComputeSignature(active);
            if (signature == _lastSignature)
            {
                return;
            }

            _lastSignature = signature;
            _builder.Clear();
            for (int i = 0; i < active.Count; i++)
            {
                _builder.Append(active[i].DisplayName).Append("  ").Append(SecondsLeft(active[i])).AppendLine(" s");
            }

            _text.text = _builder.ToString();
        }

        private static int SecondsLeft(ConsumableEffect effect) => Mathf.CeilToInt(effect.RemainingSeconds);

        /// <summary>Muda quando um efeito entra, sai ou perde um segundo inteiro.</summary>
        private static int ComputeSignature(IReadOnlyList<ConsumableEffect> active)
        {
            int signature = active.Count;
            for (int i = 0; i < active.Count; i++)
            {
                signature = signature * 31 + SecondsLeft(active[i]);
            }

            return signature;
        }
    }
}
