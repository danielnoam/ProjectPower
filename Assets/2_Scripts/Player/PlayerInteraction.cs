using System;
using DNExtensions;
using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerCamera))]
public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRadius = 3f;
    [SerializeField] private LayerMask interactionLayer;
    
    [Header("Throw Settings")]
    [SerializeField, MinMaxRange(0,30)] private RangedFloat throwForceRange = new RangedFloat(5f, 15f);
    [SerializeField, MinMaxRange(1f,4f)] private RangedFloat throwHeldRange = new RangedFloat(1f, 4f);
    
    [Header("References")]
    [SerializeField] private PlayerCamera playerCamera;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Transform holdPosition;
    [SerializeField] private Transform interactionPosition;
    
    
    private Interactable _closestInteractable;
    private PickableObject _heldObject;
    private bool _throwInputHeld;
    private float _throwInputHoldTime;
    
    
    public Transform HoldPosition => holdPosition;

    public PickableObject HeldObject
    {
        get => _heldObject;
        set
        {
            if (_heldObject == value) return;

            if (_heldObject)
            {
                _heldObject.Drop();
                _heldObject = null;
            }
            
            _heldObject = value;
        }
    }

    private void OnEnable()
    {
        playerInput.OnInteractAction += OnInteract;
        playerInput.OnThrowAction += OnThrow;
        playerInput.OnDropAction += OnDrop;
    }

    private void OnDisable()
    {
        playerInput.OnInteractAction -= OnInteract;
        playerInput.OnThrowAction -= OnThrow;
        playerInput.OnDropAction -= OnDrop;
    }
    
    private void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (_closestInteractable)
            {
                _closestInteractable.Interact(this);
            }
        }
    }
    
    private void OnThrow(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _throwInputHeld = true;
        }
        else if (context.canceled)
        {
            _throwInputHeld = false;
            ThrowHeldObject();
        }


        _throwInputHoldTime = 0f;
    }
    
    private void OnDrop(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (_heldObject)
            {
                _heldObject.Drop();
                _heldObject = null;
            }
        }
    }

    private void Update()
    {
        UpdateHeldInputTime();
    }

    private void FixedUpdate()
    {
        CheckForInteractable();
    }

    private void ThrowHeldObject()
    {
        var force = throwForceRange.Lerp(_throwInputHoldTime / throwHeldRange.maxValue);
        _heldObject?.Throw(playerCamera.GetAimDirection(), force);
    }

    private void UpdateHeldInputTime()
    {
        if (!_heldObject) return;
        
        if (_throwInputHeld && _throwInputHoldTime < throwHeldRange.maxValue)
        {
            _throwInputHoldTime += Time.deltaTime;
        }
    }

    
    private void CheckForInteractable()
    {
        Collider[] colliders = Physics.OverlapSphere(
            interactionPosition.position, 
            interactionRadius, 
            interactionLayer
        );
    
        Interactable foundInteractable = null;
        foreach (Collider col in colliders)
        {
            if (col.TryGetComponent(out Interactable interactable))
            {
                if (!interactable.CanInteract) continue;
                foundInteractable = interactable;
                break; 
            }
        }
    

        if (foundInteractable != _closestInteractable)
        {

            if (_closestInteractable) _closestInteractable.UnHighlight();
            
            _closestInteractable = foundInteractable;
        
            if (_closestInteractable) _closestInteractable.Highlight();
        }
    }
    
    private void OnDrawGizmos()
    {
        if (interactionPosition)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(interactionPosition.position, interactionRadius);
        }
        
        if (holdPosition)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(holdPosition.position, 0.3f);
        }
    }
}