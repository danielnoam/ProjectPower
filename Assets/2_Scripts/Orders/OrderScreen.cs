using System;
using TMPro;
using UnityEngine;

public class OrderScreen : MonoBehaviour
{
    
    [Header("References")]
    [SerializeField] private SOGameSettings gameSettings;
    [SerializeField] private OrderCounter orderCounter;
    [SerializeField] private DeliveryArea deliveryArea;
    [SerializeField] private TextMeshProUGUI packageNumber;
    [SerializeField] private TextMeshProUGUI timeLeft;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private CanvasGroup informationCanvas;
    [SerializeField] private CanvasGroup statusCanvas;


    private void Awake()
    {
        informationCanvas.alpha = 0;
        statusCanvas.alpha = 0;
    }

    private void OnEnable()
    {
        orderCounter.OnOrderStartedEvent += OnOrderStarted;
        orderCounter.OnOrderTimeChangedEvent += OnOrderTimeChanged;
        orderCounter.OnOrderFinishedEvent += OnOrderFinished;
    }
    private void OnDisable()
    {
        orderCounter.OnOrderStartedEvent -= OnOrderStarted;
        orderCounter.OnOrderTimeChangedEvent -= OnOrderTimeChanged;
        orderCounter.OnOrderFinishedEvent -= OnOrderFinished;
    }

    private void OnOrderTimeChanged(Order order)
    {
        if (order == null) return;
        
        timeLeft.text = $"{order.timeLeft:F0}";
        packageNumber.text = $"{order.order}";
    }

    private void OnOrderStarted(Order order)
    {
        statusCanvas.alpha = 0;
        informationCanvas.alpha = 1;
    }
    
    
    
    private void OnOrderFinished(bool success)
    {
        informationCanvas.alpha = 0;
        statusCanvas.alpha = 1;
        
        if (success)
        {
            statusText.text = "Order Completed!";
            statusText.color = Color.green;
        }
        else
        {
            statusText.text = "Order Failed!";
            statusText.color = Color.red;
        }
    }
    
}
