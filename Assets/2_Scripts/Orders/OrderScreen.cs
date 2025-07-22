using System;
using System.Collections.Generic;
using DNExtensions;
using PrimeTween;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public class OrderScreen : MonoBehaviour
{
    [Header("Screen Settings")]
    [SerializeField] private float stateChangeDuration = 0.5f;
    [SerializeField] private Color lowTimeColor = Color.red;
    [SerializeField] private SOAudioEvent startOrderSfx;
    [SerializeField] private SOAudioEvent orderFailedSfx;
    [SerializeField] private SOAudioEvent orderCompletedSfx;
    
    [Header("References")]
    [SerializeField] private SOGameSettings gameSettings;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private OrderCounter orderCounter;
    [SerializeField] private OrderDeliveryArea orderDeliveryArea;
    [SerializeField] private TextMeshProUGUI packageNumber;
    [SerializeField] private TextMeshProUGUI timeLeft;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private CanvasGroup informationCanvas;
    [SerializeField] private CanvasGroup statusCanvas;



    private Vector3 _originalTimeScale;
    private Color _originalTimeColor;
    private Sequence _stateChangeSequence;
    private Sequence _timeLeftPunchSequence;
    
    private void Awake()
    {
        informationCanvas.alpha = 0;
        statusCanvas.alpha = 0;
        statusText.text = "";
        packageNumber.text = "";
        timeLeft.text = "";
        _originalTimeColor = timeLeft.color;
        _originalTimeScale = timeLeft.transform.localScale;
    }

    private void OnEnable()
    {
        orderCounter.OnOrderStartedEvent += OnOrderStarted;
        orderCounter.OnOrderTimeChangedEvent += OnOrderTimeChanged;
        orderCounter.OnOrderFinishedEvent += OnOrderFinished;
        orderCounter.OnStoppedTakingOrdersEvent += OnStoppedTakingOrders;
    }

    private void OnStoppedTakingOrders()
    {
        if (_stateChangeSequence.isAlive) _stateChangeSequence.Stop();
        _stateChangeSequence = Sequence.Create()
            .Group(Tween.Alpha(informationCanvas, 0, stateChangeDuration/2))
            .Chain(Tween.Alpha(statusCanvas, 0, stateChangeDuration/2));
        
        
        statusText.text = "";
        packageNumber.text = "";
        timeLeft.text = "";
        timeLeft.color = _originalTimeColor;
        timeLeft.transform.localScale = _originalTimeScale;
    }

    private void OnDisable()
    {
        orderCounter.OnOrderStartedEvent -= OnOrderStarted;
        orderCounter.OnOrderTimeChangedEvent -= OnOrderTimeChanged;
        orderCounter.OnOrderFinishedEvent -= OnOrderFinished;
        orderCounter.OnStoppedTakingOrdersEvent -= OnStoppedTakingOrders;
    }

    private void OnOrderTimeChanged(Order order)
    {
        if (order == null) return;
        
        timeLeft.text = $"{order.TimeLeft:F0}";

        if (_timeLeftPunchSequence.isAlive) _timeLeftPunchSequence.Stop();
        timeLeft.transform.localScale = _originalTimeScale;
        _timeLeftPunchSequence = Sequence.Create()
            .Group(Tween.PunchScale(timeLeft.transform, Vector3.one * 1f, 1f, 1f));

        if (order.TimeLeft <= 10)
        {
            timeLeft.color = lowTimeColor;
        }
    }

    private void OnOrderStarted(Order order)
    {
        if (_stateChangeSequence.isAlive) _stateChangeSequence.Stop();

        _stateChangeSequence = Sequence.Create()
            .Group(Tween.Alpha(statusCanvas, 0, stateChangeDuration/2))
            .Chain(Tween.Alpha(informationCanvas, 1, stateChangeDuration/2));
        
        timeLeft.color = _originalTimeColor;
        timeLeft.transform.localScale = _originalTimeScale;

        if (order.NumbersNeeded.Count > 1)
        {
            for (var index = 0; index < order.NumbersNeeded.Count; index++)
            {
                var number = order.NumbersNeeded[index];
                
                if (index == order.NumbersNeeded.Count - 1)
                {
                    packageNumber.text += $"{number}";
                }
                else
                {
                    packageNumber.text += $"{number}, ";
                }
            }
        }
        else
        {
            packageNumber.text += $"{order.NumbersNeeded[0]}";
        }

        startOrderSfx?.Play(audioSource);
    }
    
    
    
    private void OnOrderFinished(bool success, List<NumberdPackage> packagesInDeliveryArea, int orderWorth)
    {
        if (_stateChangeSequence.isAlive) _stateChangeSequence.Stop();
        _stateChangeSequence = Sequence.Create()
            .Group(Tween.Alpha(informationCanvas, 0, stateChangeDuration/2))
            .Chain(Tween.Alpha(statusCanvas, 1, stateChangeDuration/2));
        
        if (success)
        {
            statusText.text = $"Order Completed!\n +{orderWorth}$";
            statusText.color = Color.green;
            orderCompletedSfx?.Play(audioSource);
        }
        else
        {
            statusText.text = "Order Failed!";
            statusText.color = Color.red;
            orderFailedSfx?.Play(audioSource);
        }
        
        packageNumber.text = "";
        timeLeft.text = "";
    }
    
}
