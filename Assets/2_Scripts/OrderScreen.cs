using System;
using TMPro;
using UnityEngine;

public class OrderScreen : MonoBehaviour
{

    [Header("Settings")]

    
    [Header("References")]
    [SerializeField] private SOGameSettings gameSettings;
    [SerializeField] private DeliveryArea deliveryArea;
    [SerializeField] private TextMeshProUGUI packageNumber;
    [SerializeField] private TextMeshProUGUI timeLeft;
    [SerializeField] private CanvasGroup canvasGroup;


    private void Awake()
    {
        canvasGroup.alpha = 0;
    }

    private void OnEnable()
    {
        deliveryArea.OnOrderStartedEvent += OnOrderStarted;
        deliveryArea.OnOrderFinishedEvent += OnOrderFinished;
    }
    private void OnDisable()
    {
        deliveryArea.OnOrderStartedEvent -= OnOrderStarted;
        deliveryArea.OnOrderFinishedEvent -= OnOrderFinished;
    }

    private void OnOrderStarted(Order order)
    {
        canvasGroup.alpha = 1;
    }
    
    private void OnOrderFinished(Order order)
    {
        canvasGroup.alpha = 0;
    }

    private void Update()
    {
        if (deliveryArea.CurrentOrder != null)
        {
            timeLeft.text = $"{deliveryArea.CurrentOrder.timeLeft:F0}";
            packageNumber.text = $"{deliveryArea.CurrentOrder.order}";
        }
    }
}
