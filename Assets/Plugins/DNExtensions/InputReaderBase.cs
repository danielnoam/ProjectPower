using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DNExtensions
{
    
    [RequireComponent(typeof(PlayerInput))]
    public class InputReaderBase : MonoBehaviour
    {
        [Header("Cursor Settings")] [SerializeField]
        private bool hideCursor = true;

        [SerializeField, HideInInspector] protected PlayerInput playerInput;


        private void OnValidate()
        {
            if (!playerInput) playerInput = GetComponent<PlayerInput>();

            if (playerInput) playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
        }

        protected virtual void Awake()
        {
            SetCursorVisibility(hideCursor);
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



        private void SetCursorVisibility(bool state)
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

        [Button("Toggle Cursor")]
        private void ToggleCursorVisibility()
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



    }
}