using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace TechC.Main.Player
{
    public class PlayerInputManager : MonoBehaviour
    {
        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;

        public event Action OnLeftInputStarted;
        public event Action OnLeftInputCanceled;
        public event Action OnRightInputStarted;
        public event Action OnRightInputCanceled;

        private void Awake()
        {
            // EventTrigger で PointerDown / PointerUp を追加
            AddEventTrigger(leftButton, OnLeftPressed, OnLeftReleased);
            AddEventTrigger(rightButton, OnRightPressed, OnRightReleased);
        }

        private void AddEventTrigger(Button button, Action onDown, Action onUp)
        {
            var trigger = button.gameObject.GetComponent<EventTrigger>();
            if (trigger == null) trigger = button.gameObject.AddComponent<EventTrigger>();

            // PointerDown
            var entryDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            entryDown.callback.AddListener((data) => onDown());
            trigger.triggers.Add(entryDown);

            // PointerUp
            var entryUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            entryUp.callback.AddListener((data) => onUp());
            trigger.triggers.Add(entryUp);
        }

        private void OnLeftPressed()  => OnLeftInputStarted?.Invoke();
        private void OnLeftReleased() => OnLeftInputCanceled?.Invoke();
        private void OnRightPressed() => OnRightInputStarted?.Invoke();
        private void OnRightReleased() => OnRightInputCanceled?.Invoke();
    }
}