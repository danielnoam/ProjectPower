using System;
using DNExtensions;
using UnityEngine;

[SelectionBase]
[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Interactable))]
[RequireComponent(typeof(AudioSource))]
public class PickableObject : MonoBehaviour
{

    [Header("Pickable Object Settings")]
    [SerializeField, Min(1)] private float objectWeight = 1f;
    [SerializeField] protected Rigidbody rigidBody;
    [SerializeField] protected Interactable interactable;
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] private SOAudioEvent collisionSfx;

    private bool _isBeingHeld;
    private Transform _holdPosition;
    
    public float ObjectWeight => objectWeight;
    private void OnValidate()
    {
        if (!rigidBody) rigidBody = this.GetOrAddComponent<Rigidbody>();
        if (!interactable) interactable = this.GetOrAddComponent<Interactable>();
        if (!audioSource) audioSource = this.GetOrAddComponent<AudioSource>();
    }

    private void OnEnable()
    {
        interactable.OnInteract += OnInteract;
    }
    
    private void OnDisable()
    {
        interactable.OnInteract -= OnInteract;
    }   
    
    private void OnInteract(PlayerInteraction interactor)
    {
        if (!rigidBody || !interactor) return;

        if (!_isBeingHeld)
        {
            PickUp(interactor);
        }

    }

    private void FixedUpdate()
    {
        if (_isBeingHeld && _holdPosition)
        {
            var direction = _holdPosition.position - rigidBody.position;
            rigidBody.AddForce(direction * 15f, ForceMode.Force);
        }

    }
    
    private void PickUp(PlayerInteraction interactor)
    {
        if (!rigidBody || _isBeingHeld) return;

        interactable?.SetCanInteract(false);
        rigidBody.useGravity = false;
        _isBeingHeld = true;
        _holdPosition = interactor.HoldPosition;
        interactor.HeldObject = this;
    }

    public void Drop()
    {
        if (!rigidBody || !_isBeingHeld) return;
        interactable?.SetCanInteract(true);
        rigidBody.useGravity = true;
        _isBeingHeld = false;
        _holdPosition = null;
    }

    public void Throw(Vector3 direction, float force)
    {
        if (!rigidBody) return;

        interactable?.SetCanInteract(true);
        rigidBody.useGravity = true;
        _isBeingHeld = false;
        _holdPosition = null;
        rigidBody.AddForce(direction * force, ForceMode.Impulse);
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > 0.3f)
        {
            collisionSfx?.Play(audioSource);
        }
    }

    

}
