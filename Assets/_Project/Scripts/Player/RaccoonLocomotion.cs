using UnityEngine;

namespace Guaxinim.Player
{
    /// <summary>Fórmulas do movimento do guaxinim (GDD §3.2 e §4), sem dependência da cena.</summary>
    public static class RaccoonLocomotion
    {
        /// <summary>
        /// Converte o input (WASD ou analógico) numa direção no chão relativa à câmera: "para cima"
        /// é para onde a câmera olha. O tamanho vai de 0 a 1 (analógico pela metade anda devagar).
        /// </summary>
        public static Vector3 GetMoveDirection(Vector2 input, float cameraYawDegrees)
        {
            Vector2 clamped = Vector2.ClampMagnitude(input, 1f);
            var local = new Vector3(clamped.x, 0f, clamped.y);
            return Quaternion.Euler(0f, cameraYawDegrees, 0f) * local;
        }

        /// <summary>Correr vai direto à velocidade máxima; andar segue a inclinação do analógico.</summary>
        public static float GetTargetSpeed(float inputMagnitude, bool isSprinting, float walkSpeed, float sprintSpeed)
        {
            if (inputMagnitude <= 0f)
            {
                return 0f;
            }

            return isSprinting ? sprintSpeed : walkSpeed * Mathf.Clamp01(inputMagnitude);
        }

        /// <summary>Aproxima a velocidade no chão do alvo, acelerando ou freando.</summary>
        public static Vector3 StepHorizontalVelocity(Vector3 current, Vector3 target, float acceleration,
            float deceleration, float deltaTime)
        {
            float rate = target.sqrMagnitude >= current.sqrMagnitude ? acceleration : deceleration;
            return Vector3.MoveTowards(current, target, rate * deltaTime);
        }

        /// <summary>No chão, mantém uma pequena velocidade para baixo; no ar, acumula a gravidade.</summary>
        public static float StepVerticalVelocity(float current, bool isGrounded, float gravity,
            float groundedFallSpeed, float deltaTime)
        {
            if (isGrounded && current <= 0f)
            {
                return -groundedFallSpeed;
            }

            return current - gravity * deltaTime;
        }
    }
}
