


using System;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class OrderFailButton : MonoBehaviour
{
    
    [Header("Button Animation")]
    [SerializeField] private float buttonPressDuration = 0.2f;
    [SerializeField] private Vector3 positionOffset = new Vector3(0, -0.1f, 0);
    
    [Header("References")]
    [SerializeField] private Interactable interactable;
    [SerializeField] private OrderCounter orderCounter;
    [SerializeField] private Transform buttonGfx;
    
    private Vector3 _originalButtonPosition;
    private Sequence _buttonPressSequence;
    
    private void OnValidate()
    {
        if (!orderCounter) GetComponentInParent<OrderCounter>();
    }

    private void Awake()
    {
        _originalButtonPosition = buttonGfx.localPosition;
    }

    private void OnEnable()
    {
        interactable.OnInteract += OnInteract;
        orderCounter.OnOrderStartedEvent += OnOrderStarted;
        orderCounter.OnOrderFinishedEvent += OnOrderFinished;
    }
    
    private void OnDisable()
    {
        interactable.OnInteract -= OnInteract;
        orderCounter.OnOrderStartedEvent -= OnOrderStarted;
        orderCounter.OnOrderFinishedEvent -= OnOrderFinished;
    }

    private void OnInteract(PlayerInteraction interaction)
    {
        TryFailOrder();
    }
    
    private void OnOrderStarted(Order order)
    {
        if (order == null) return;
        interactable.SetCanInteract(true);
    }
    
    private void OnOrderFinished(bool success, List<NumberdPackage> packagesInDeliveryArea, int orderWorth)
    {
        interactable.SetCanInteract(false);
    }

    private void TryFailOrder()
    {
        interactable.SetCanInteract(false);
        
        if (_buttonPressSequence.isAlive) _buttonPressSequence.Stop();
        _buttonPressSequence = Sequence.Create()
            .Group(Tween.LocalPosition(buttonGfx, startValue: buttonGfx.localPosition, endValue: buttonGfx.localPosition + positionOffset, duration: buttonPressDuration, Ease.InOutSine))
            .ChainCallback(()=> orderCounter.FailOrder())
            .ChainDelay(0.2f)
            .Group(Tween.LocalPosition(buttonGfx, startValue: buttonGfx.localPosition + positionOffset, endValue: _originalButtonPosition, duration: buttonPressDuration, Ease.InOutSine));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + positionOffset, 0.1f);
    }
}