using System.Collections.Generic;

namespace Guaxinim.Guards
{
    /// <summary>Quais luzes ficam acesas: as <c>maxLit</c> mais perto (no empate, a de menor índice).</summary>
    public static class LightBudget
    {
        public static bool IsLit(int index, IReadOnlyList<float> sqrDistances, int maxLit)
        {
            int closer = 0;
            for (int other = 0; other < sqrDistances.Count; other++)
            {
                if (other == index)
                {
                    continue;
                }

                bool isCloser = sqrDistances[other] < sqrDistances[index] ||
                                (sqrDistances[other] == sqrDistances[index] && other < index);
                if (isCloser)
                {
                    closer++;
                }
            }

            return closer < maxLit;
        }
    }
}
