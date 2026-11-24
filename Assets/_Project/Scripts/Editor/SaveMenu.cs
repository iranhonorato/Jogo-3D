using Guaxinim.Core;
using UnityEditor;

namespace Guaxinim.EditorTools
{
    /// <summary>Atalho do Editor para testar o desbloqueio das fases do zero.</summary>
    public static class SaveMenu
    {
        [MenuItem("Tools/Guaxinim/Apagar Save")]
        private static void ClearSave() => SaveSystem.Clear();
    }
}
