using System;
using System.Collections.Generic;
using UnityEngine;

namespace Guaxinim.Core
{
    /// <summary>O que dispara uma dica do tutorial.</summary>
    public enum TipTrigger
    {
        /// <summary>Alguns segundos depois do começo da fase.</summary>
        AfterSeconds,

        /// <summary>A primeira vez que há um item ao alcance do E.</summary>
        NearPickup,

        /// <summary>A primeira vez que o guaxinim pega um item.</summary>
        FirstPickup,

        /// <summary>A primeira vez que há uma lixeira ao alcance do E.</summary>
        NearHidingSpot,
    }

    /// <summary>Uma dica curta mostrada no topo da tela (GDD §11: a Fase 1 ensina os controles).</summary>
    [Serializable]
    public class TutorialTip
    {
        [SerializeField] private string _text = string.Empty;
        [SerializeField] private TipTrigger _trigger;

        [Tooltip("Segundos desde o começo da fase (só para o gatilho Depois de segundos)")]
        [SerializeField, Min(0f)] private float _triggerSeconds;

        public TutorialTip(string text, TipTrigger trigger, float triggerSeconds = 0f)
        {
            _text = text;
            _trigger = trigger;
            _triggerSeconds = triggerSeconds;
        }

        private TutorialTip()
        {
            // O serializador da Unity cria a instância por aqui ao carregar o asset.
        }

        public string Text => _text;
        public TipTrigger Trigger => _trigger;
        public float TriggerSeconds => _triggerSeconds;
    }

    /// <summary>Dicas de uma fase, na ordem em que o jogador precisa delas. Somente leitura durante o jogo.</summary>
    [CreateAssetMenu(fileName = "TutorialTips", menuName = "Guaxinim/Tutorial Tips")]
    public class TutorialTipsData : ScriptableObject
    {
        [Tooltip("Segundos que cada dica fica na tela")]
        [SerializeField, Min(0.5f)] private float _displaySeconds = 4f;

        [SerializeField] private TutorialTip[] _tips =
        {
            new TutorialTip("WASD anda em silêncio", TipTrigger.AfterSeconds, 1f),
            new TutorialTip("Shift corre, mas faz barulho", TipTrigger.AfterSeconds, 7f),
            new TutorialTip("E pega os itens da lista", TipTrigger.NearPickup),
            new TutorialTip("Leve os itens até a toca", TipTrigger.FirstPickup),
            new TutorialTip("Lixeiras escondem você (E)", TipTrigger.NearHidingSpot),
        };

        public float DisplaySeconds => _displaySeconds;
        public IReadOnlyList<TutorialTip> Tips => _tips;
    }
}
