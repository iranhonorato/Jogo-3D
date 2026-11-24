using System.Collections.Generic;
using UnityEngine;

namespace Guaxinim.UI
{
    /// <summary>Dicas sorteadas na tela "Você foi pego!" (GDD §13). A lista é editável no Inspector.</summary>
    [CreateAssetMenu(fileName = "CaughtTips", menuName = "Guaxinim/Caught Tips")]
    public class CaughtTipsData : ScriptableObject
    {
        [SerializeField, TextArea] private string[] _tips =
        {
            "Andar não faz barulho: corra só quando precisar.",
            "Largue itens (Q) para fugir mais rápido.",
            "Lixeiras escondem, mas não com carga volumosa.",
            "Sem carga, você corre mais rápido que o segurança.",
            "Item frágil quebra se você correr com ele.",
            "Assobie (F) para atrair o segurança para longe.",
        };

        public IReadOnlyList<string> Tips => _tips;
    }
}
