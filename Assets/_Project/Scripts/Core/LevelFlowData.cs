using UnityEngine;

namespace Guaxinim.Core
{
    /// <summary>Tempos do ciclo da fase (GDD §6.5 e §13). Somente leitura durante o jogo.</summary>
    [CreateAssetMenu(fileName = "LevelFlowData", menuName = "Guaxinim/Level Flow Data")]
    public class LevelFlowData : ScriptableObject
    {
        [Tooltip("Segundos com o jogo congelado depois de ser pego, antes de mostrar a tela \"Você foi pego!\"")]
        [SerializeField, Min(0f)] private float _caughtPanelDelaySeconds = 0.6f;

        [Tooltip("Pontos por item bônus entregue (GDD §12)")]
        [SerializeField, Min(0)] private int _pointsPerBonusItem = 100;

        public float CaughtPanelDelaySeconds => _caughtPanelDelaySeconds;
        public int PointsPerBonusItem => _pointsPerBonusItem;
    }
}
