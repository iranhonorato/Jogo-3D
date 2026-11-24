using Guaxinim.EditorTools;
using Guaxinim.Guards;
using Guaxinim.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Guaxinim.Tests
{
    /// <summary>
    /// GDD §6.2 e §6.5: o segurança persegue mais devagar que a corrida do guaxinim sem carga, e o faxineiro anda
    /// a metade da velocidade do segurança em todos os estados. Lê os assets do projeto (gerados pelo menu
    /// Tools/Guaxinim/Greybox/Prefabs dos Seguranças).
    /// </summary>
    public class EnemyBalanceTests
    {
        private const float Tolerance = 0.001f;

        private GuardData _guard;
        private GuardData _janitor;

        [SetUp]
        public void SetUp()
        {
            _guard = AssetDatabase.LoadAssetAtPath<GuardData>(GuardFactory.GuardDataPath);
            _janitor = AssetDatabase.LoadAssetAtPath<GuardData>(GuardFactory.JanitorDataPath);
            Assert.IsNotNull(_guard, "GuardData_Guard não existe: rode Tools/Guaxinim/Greybox/Prefabs dos Seguranças.");
            Assert.IsNotNull(_janitor, "GuardData_Janitor não existe: rode Tools/Guaxinim/Greybox/Prefabs dos Seguranças.");
        }

        [Test]
        public void GuardChase_IsSlowerThanUnloadedRaccoonSprint()
        {
            var playerData = ScriptableObject.CreateInstance<PlayerData>();
            float raccoonSprint = playerData.SprintSpeed;
            Object.DestroyImmediate(playerData);

            Assert.Less(_guard.ChaseSpeed, raccoonSprint);
        }

        [Test]
        public void Janitor_EverySpeed_IsHalfTheGuard()
        {
            float factor = GuardFactory.JanitorSpeedFactor;

            Assert.AreEqual(_guard.PatrolSpeed * factor, _janitor.PatrolSpeed, Tolerance);
            Assert.AreEqual(_guard.SuspiciousSpeed * factor, _janitor.SuspiciousSpeed, Tolerance);
            Assert.AreEqual(_guard.ChaseSpeed * factor, _janitor.ChaseSpeed, Tolerance);
            Assert.AreEqual(_guard.SearchSpeed * factor, _janitor.SearchSpeed, Tolerance);
        }

        [Test]
        public void Janitor_Chase_IsSlowerThanWalkingRaccoon()
        {
            var playerData = ScriptableObject.CreateInstance<PlayerData>();
            float raccoonWalk = playerData.WalkSpeed;
            Object.DestroyImmediate(playerData);

            Assert.Less(_janitor.ChaseSpeed, raccoonWalk); // andando, o guaxinim já foge do faxineiro
        }
    }
}
