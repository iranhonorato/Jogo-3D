using Guaxinim.Player;
using NUnit.Framework;
using UnityEngine;

namespace Guaxinim.Tests
{
    /// <summary>GDD §3.2 e §4: input relativo à câmera, andar a 3 m/s, correr a 6 m/s e gravidade.</summary>
    public class RaccoonLocomotionTests
    {
        private const float Tolerance = 0.001f;
        private const float WalkSpeed = 3f;
        private const float SprintSpeed = 6f;
        private const float IsometricYaw = 45f;

        [Test]
        public void GetMoveDirection_ForwardWithIsometricCamera_PointsDiagonally()
        {
            Vector3 direction = RaccoonLocomotion.GetMoveDirection(Vector2.up, IsometricYaw);

            float diagonal = Mathf.Sqrt(0.5f);
            Assert.AreEqual(diagonal, direction.x, Tolerance);
            Assert.AreEqual(0f, direction.y, Tolerance);
            Assert.AreEqual(diagonal, direction.z, Tolerance);
        }

        [Test]
        public void GetMoveDirection_KeyboardDiagonal_IsClampedToLengthOne()
        {
            Vector3 direction = RaccoonLocomotion.GetMoveDirection(new Vector2(1f, 1f), 0f);

            Assert.AreEqual(1f, direction.magnitude, Tolerance);
        }

        [Test]
        public void GetMoveDirection_HalfStick_KeepsHalfLength()
        {
            Vector3 direction = RaccoonLocomotion.GetMoveDirection(new Vector2(0f, 0.5f), IsometricYaw);

            Assert.AreEqual(0.5f, direction.magnitude, Tolerance);
        }

        [Test]
        public void GetTargetSpeed_Walking_ReturnsWalkSpeed()
        {
            float speed = RaccoonLocomotion.GetTargetSpeed(1f, false, WalkSpeed, SprintSpeed);

            Assert.AreEqual(3f, speed, Tolerance);
        }

        [Test]
        public void GetTargetSpeed_Sprinting_ReturnsSprintSpeed()
        {
            float speed = RaccoonLocomotion.GetTargetSpeed(1f, true, WalkSpeed, SprintSpeed);

            Assert.AreEqual(6f, speed, Tolerance);
        }

        [Test]
        public void GetTargetSpeed_SprintWithoutInput_ReturnsZero()
        {
            float speed = RaccoonLocomotion.GetTargetSpeed(0f, true, WalkSpeed, SprintSpeed);

            Assert.AreEqual(0f, speed, Tolerance);
        }

        [Test]
        public void GetTargetSpeed_HalfStickWalking_ReturnsHalfWalkSpeed()
        {
            float speed = RaccoonLocomotion.GetTargetSpeed(0.5f, false, WalkSpeed, SprintSpeed);

            Assert.AreEqual(1.5f, speed, Tolerance);
        }

        [Test]
        public void StepHorizontalVelocity_Accelerating_DoesNotOvershootTarget()
        {
            Vector3 target = Vector3.forward * SprintSpeed;

            Vector3 velocity = RaccoonLocomotion.StepHorizontalVelocity(Vector3.zero, target, 40f, 50f, 1f);

            Assert.AreEqual(SprintSpeed, velocity.magnitude, Tolerance);
        }

        [Test]
        public void StepHorizontalVelocity_Braking_UsesDeceleration()
        {
            Vector3 current = Vector3.forward * SprintSpeed;

            Vector3 velocity = RaccoonLocomotion.StepHorizontalVelocity(current, Vector3.zero, 40f, 50f, 0.1f);

            Assert.AreEqual(1f, velocity.magnitude, Tolerance);
        }

        [Test]
        public void StepVerticalVelocity_OnGround_KeepsSmallDownwardSpeed()
        {
            float velocity = RaccoonLocomotion.StepVerticalVelocity(-10f, true, 25f, 2f, 0.1f);

            Assert.AreEqual(-2f, velocity, Tolerance);
        }

        [Test]
        public void StepVerticalVelocity_InAir_AccumulatesGravity()
        {
            float velocity = RaccoonLocomotion.StepVerticalVelocity(0f, false, 25f, 2f, 0.1f);

            Assert.AreEqual(-2.5f, velocity, Tolerance);
        }
    }
}
