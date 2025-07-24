using System;
using DNExtensions;
using DNExtensions.InputSystem;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : InputReaderBase
{


    
        private InputActionMap _playerActionMap;
        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _jumpAction;
        private InputAction _runAction;
        private InputAction _interactAction;
        private InputAction _throwAction;
        private InputAction _dropAction;
        private InputAction _toggleMenu;
        private bool _sendInput = true;
        
        
        public event Action<InputAction.CallbackContext> OnMoveAction;
        public event Action<InputAction.CallbackContext> OnLookAction;
        public event Action<InputAction.CallbackContext> OnJumpAction;
        public event Action<InputAction.CallbackContext> OnRunAction;
        public event Action<InputAction.CallbackContext> OnInteractAction;
        public event Action<InputAction.CallbackContext> OnThrowAction;
        public event Action<InputAction.CallbackContext> OnDropAction;
        public event Action<InputAction.CallbackContext> OnToggleMenuAction;


        protected override void Awake()
        {
            base.Awake();


            _playerActionMap = playerInput.actions.FindActionMap("Player");

            if (_playerActionMap == null)
            {
                Debug.LogError("Player Action Map not found. Please check the action maps in the Player Input component.");
                return;
            }

            _moveAction = _playerActionMap.FindAction("Move");
            _lookAction = _playerActionMap.FindAction("Look");
            _jumpAction = _playerActionMap.FindAction("Jump");
            _runAction = _playerActionMap.FindAction("Run");
            _interactAction = _playerActionMap.FindAction("Interact");
            _throwAction = _playerActionMap.FindAction("Throw");
            _dropAction = _playerActionMap.FindAction("Drop");
            _toggleMenu = _playerActionMap.FindAction("ToggleMenu");

        }


        private void OnEnable()
        {
            SubscribeToAction(_moveAction, OnMove);
            SubscribeToAction(_lookAction, OnLook);
            SubscribeToAction(_jumpAction, OnJump);
            SubscribeToAction(_runAction, OnRun);
            SubscribeToAction(_interactAction, OnInteract);
            SubscribeToAction(_throwAction, OnThrow);
            SubscribeToAction(_dropAction, OnDrop);
            SubscribeToAction(_toggleMenu, OnToggleMenu);


        }

        private void OnDisable()
        {
            UnsubscribeFromAction(_moveAction, OnMove);
            UnsubscribeFromAction(_lookAction, OnLook);
            UnsubscribeFromAction(_jumpAction, OnJump);
            UnsubscribeFromAction(_runAction, OnRun);
            UnsubscribeFromAction(_interactAction, OnInteract);
            UnsubscribeFromAction(_throwAction, OnThrow);
            UnsubscribeFromAction(_dropAction, OnDrop);
            UnsubscribeFromAction(_toggleMenu, OnToggleMenu);
        }

        

        private void OnMove(InputAction.CallbackContext context)
        {
            if (!_sendInput) return;
            
            OnMoveAction?.Invoke(context);
        }
        
        private void OnLook(InputAction.CallbackContext context)
        {
            if (!_sendInput) return;

            OnLookAction?.Invoke(context);
        }
        
        private void OnJump(InputAction.CallbackContext context)
        {
            if (!_sendInput) return;

            OnJumpAction?.Invoke(context);
        }
        
        private void OnRun(InputAction.CallbackContext context)
        {
            if (!_sendInput) return;

            OnRunAction?.Invoke(context);
        }
        
        
        private void OnInteract(InputAction.CallbackContext context)
        {
            if (!_sendInput) return;

            OnInteractAction?.Invoke(context);
        }
        
        private void OnThrow(InputAction.CallbackContext context)
        {

            OnThrowAction?.Invoke(context);
        }
        
        private void OnDrop(InputAction.CallbackContext context)
        {
            if (!_sendInput) return;

            OnDropAction?.Invoke(context);
        }
        
        
        private void OnToggleMenu(InputAction.CallbackContext context)
        {
            if (!GameManager.Instance) return;
            
            OnToggleMenuAction?.Invoke(context);
            
            var gameIsPaused = GameManager.Instance.TogglePause();
            _sendInput = !gameIsPaused;

        }
        

   
}