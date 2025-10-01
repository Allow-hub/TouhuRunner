using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace TechC.Main.Player
{
    /// <summary>
    /// プレイヤー入力を管理するクラス
    /// </summary>
    public class PlayerInputManager : MonoBehaviour
    {
        public event Action OnLeftInputStarted;
        public event Action OnLeftInputCanceled;
        public event Action OnRightInputStarted;
        public event Action OnRightInputCanceled;

        public void OnMoveLeft(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnLeftInputStarted?.Invoke();
            }
            else if (context.canceled)
            {
                OnLeftInputCanceled?.Invoke();
            }
        }

        public void OnMoveRight(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnRightInputStarted?.Invoke();
            }
            else if (context.canceled)
            {
                OnRightInputCanceled?.Invoke();
            }
        }
    }
}