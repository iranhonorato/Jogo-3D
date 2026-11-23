using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Guaxinim.Core
{
    /// <summary>
    /// Único ponto de leitura de input do jogo (GDD §3.2). Os outros componentes leem
    /// <see cref="Move"/> e <see cref="IsSprinting"/> ou assinam os eventos, sem conhecer o Input System.
    /// Na pausa, <see cref="EnableUIInput"/> troca para o mapa UI, e <see cref="EnablePlayerInput"/> volta.
    /// </summary>
    public class InputReader : MonoBehaviour
    {
        private GameInput _input;

        public event Action OnInteractPressed;
        public event Action OnDropPressed;
        public event Action OnWhistlePressed;
        public event Action OnMushroomPowerPressed;
        public event Action OnJumpPressed;
        public event Action OnPausePressed;
        public event Action OnResumePressed;
        public event Action<bool> OnShowListHeldChanged;

        public Vector2 Move => _input.Player.Move.ReadValue<Vector2>();
        public bool IsSprinting => _input.Player.Sprint.IsPressed();
        public bool IsPlayerInputEnabled => _input.Player.enabled;

        private void Awake() => _input = new GameInput();

        private void OnEnable()
        {
            GameInput.PlayerActions player = _input.Player;
            player.Interact.performed += HandleInteract;
            player.Drop.performed += HandleDrop;
            player.Whistle.performed += HandleWhistle;
            player.MushroomPower.performed += HandleMushroomPower;
            player.Jump.performed += HandleJump;
            player.Pause.performed += HandlePause;
            player.ShowList.performed += HandleShowListPressed;
            player.ShowList.canceled += HandleShowListReleased;
            _input.UI.Cancel.performed += HandleResume;
            EnablePlayerInput();
        }

        private void OnDisable()
        {
            GameInput.PlayerActions player = _input.Player;
            player.Interact.performed -= HandleInteract;
            player.Drop.performed -= HandleDrop;
            player.Whistle.performed -= HandleWhistle;
            player.MushroomPower.performed -= HandleMushroomPower;
            player.Jump.performed -= HandleJump;
            player.Pause.performed -= HandlePause;
            player.ShowList.performed -= HandleShowListPressed;
            player.ShowList.canceled -= HandleShowListReleased;
            _input.UI.Cancel.performed -= HandleResume;
            _input.Disable();
        }

        private void OnDestroy() => _input.Dispose();

        public void EnablePlayerInput()
        {
            _input.UI.Disable();
            _input.Player.Enable();
        }

        public void EnableUIInput()
        {
            // Desligar o mapa cancela as ações em andamento: o Tab segurado dispara OnShowListHeldChanged(false).
            _input.Player.Disable();
            _input.UI.Enable();
        }

        private void HandleInteract(InputAction.CallbackContext context) => OnInteractPressed?.Invoke();

        private void HandleDrop(InputAction.CallbackContext context) => OnDropPressed?.Invoke();

        private void HandleWhistle(InputAction.CallbackContext context) => OnWhistlePressed?.Invoke();

        private void HandleMushroomPower(InputAction.CallbackContext context) => OnMushroomPowerPressed?.Invoke();

        private void HandleJump(InputAction.CallbackContext context) => OnJumpPressed?.Invoke();

        private void HandlePause(InputAction.CallbackContext context) => OnPausePressed?.Invoke();

        private void HandleResume(InputAction.CallbackContext context) => OnResumePressed?.Invoke();

        private void HandleShowListPressed(InputAction.CallbackContext context) => OnShowListHeldChanged?.Invoke(true);

        private void HandleShowListReleased(InputAction.CallbackContext context) => OnShowListHeldChanged?.Invoke(false);
    }
}
