using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DNExtensions.InputSystem
{
    

    public class InputReaderBase : MonoBehaviour
    {
        [Header("Cursor Settings")] 
        [SerializeField] private bool hideCursorOnAwake = true;
        [SerializeField] protected PlayerInput playerInput;


        private void OnValidate()
        {

            if (playerInput && playerInput.notificationBehavior != PlayerNotifications.InvokeCSharpEvents)
            {
                playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
                Debug.Log("Set Player Input notification to c# events");
            }
        }

        protected virtual void Awake()
        {
            SetCursorVisibility(hideCursorOnAwake);
        }


        protected void SubscribeToAction(InputAction action, Action<InputAction.CallbackContext> callback)
        {
            if (action == null) return;

            action.performed += callback;
            action.started += callback;
            action.canceled += callback;
        }

        protected void UnsubscribeFromAction(InputAction action, Action<InputAction.CallbackContext> callback)
        {
            if (action == null) return;

            action.performed -= callback;
            action.started -= callback;
            action.canceled -= callback;
        }

        

        [Button("Toggle Cursor")]
        public void ToggleCursorVisibility()
        {
            if (Cursor.visible)
            {
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        
        public void SetCursorVisibility(bool state)
        {
            if (state)
            {
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }


    }
}