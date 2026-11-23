using System.Linq;
using Guaxinim.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Guaxinim.Tests
{
    /// <summary>GDD §3.2: o mapa Player tem as ações e as teclas da tabela de controles.</summary>
    public class GameInputTests
    {
        private GameInput _input;

        [SetUp]
        public void SetUp() => _input = new GameInput();

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_input.asset); // Dispose() usa Destroy, que só vale no Play Mode

        [Test]
        public void PlayerMap_HasOnlyGddActions()
        {
            string[] expected =
            {
                "Move", "Sprint", "Interact", "Drop", "Whistle", "MushroomPower", "Jump", "ShowList", "Pause"
            };

            string[] actual = _input.Player.Get().actions.Select(action => action.name).ToArray();

            CollectionAssert.AreEquivalent(expected, actual);
        }

        [TestCase("Sprint", "<Keyboard>/leftShift", "<Gamepad>/rightTrigger")]
        [TestCase("Interact", "<Keyboard>/e", "<Gamepad>/buttonSouth")]
        [TestCase("Drop", "<Keyboard>/q", "<Gamepad>/buttonEast")]
        [TestCase("Whistle", "<Keyboard>/f", "<Gamepad>/buttonNorth")]
        [TestCase("MushroomPower", "<Keyboard>/r", "<Gamepad>/leftShoulder")]
        [TestCase("Jump", "<Keyboard>/space", "<Gamepad>/buttonSouth")]
        [TestCase("ShowList", "<Keyboard>/tab", "<Gamepad>/select")]
        [TestCase("Pause", "<Keyboard>/escape", "<Gamepad>/start")]
        public void PlayerAction_HasKeyboardAndGamepadBindings(string actionName, string keyboardPath, string gamepadPath)
        {
            InputAction action = _input.Player.Get().FindAction(actionName, throwIfNotFound: true);

            string[] paths = action.bindings.Select(binding => binding.path).ToArray();

            CollectionAssert.AreEquivalent(new[] { keyboardPath, gamepadPath }, paths);
        }

        [Test]
        public void Move_HasWasdAndLeftStick()
        {
            string[] paths = _input.Player.Move.bindings.Select(binding => binding.path).ToArray();

            CollectionAssert.IsSubsetOf(new[] { "<Keyboard>/w", "<Keyboard>/a", "<Keyboard>/s", "<Keyboard>/d",
                "<Gamepad>/leftStick" }, paths);
        }

        [Test]
        public void UIMap_CancelAcceptsGamepadStart()
        {
            string[] paths = _input.UI.Cancel.bindings.Select(binding => binding.path).ToArray();

            CollectionAssert.Contains(paths, "<Gamepad>/start");
        }
    }
}
