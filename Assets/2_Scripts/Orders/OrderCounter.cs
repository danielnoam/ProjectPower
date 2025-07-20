using System;
using System.Collections;
using DNExtensions;
using UnityEngine;

[DisallowMultipleComponent]
public class OrderCounter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SOGameSettings gameSettings;
    [SerializeField] private DeliveryArea deliveryArea;


    private Coroutine _startOrderCoroutine;
    private Order _currentOrder;
    

    public event Action<Order> OnOrderStartedEvent;
    public event Action<bool> OnOrderFinishedEvent;
    public event Action<Order> OnOrderTimeChangedEvent;

    private void OnEnable()
    {
        deliveryArea.OnPackageEnteredArea += OnPackageEnteredDeliveryArea;
    }

    private void OnDisable()
    {
        deliveryArea.OnPackageEnteredArea -= OnPackageEnteredDeliveryArea;
    }
    

    private void Update()
    {
        UpdateOrderTimeLeft();
    }
    
    private void OnPackageEnteredDeliveryArea(NumberdPackage package)
    {
        TryCompleteOrder(package);
    }
    
    private void TryCompleteOrder(NumberdPackage package)
    {
        if (_currentOrder == null) return;
        
        if (_currentOrder.IsOrderCompleted(package))
        {
            OnOrderFinishedEvent?.Invoke(true);

            _currentOrder = null;
            StartNewOrder(gameSettings.TimeBetweenOrders.RandomValue);
        }
    }
    
    private void UpdateOrderTimeLeft()
    {
        if (_currentOrder == null) return;
        
        _currentOrder.timeLeft -= Time.deltaTime;
        OnOrderTimeChangedEvent?.Invoke(_currentOrder);
        if (_currentOrder.timeLeft <= 0)
        {
            _currentOrder.timeLeft = 0;
            OrderFailed();
        }
    }    
    
    private void OrderFailed()
    {
        OnOrderFinishedEvent?.Invoke(false);
        _currentOrder = null;
        StartNewOrder(gameSettings.TimeBetweenOrders.RandomValue);
    }
    
    private IEnumerator StartOrderIn(float time)
    {
        yield return new WaitForSeconds(time);
        _currentOrder = new Order(gameSettings, GameManager.Instance?.GameDifficulty ?? Difficulty.Easy);
        OnOrderStartedEvent?.Invoke(_currentOrder);
    }
    
    [Button]
    public void StartNewOrder(float time = 0.1f)
    {
        if (!gameSettings) return;
        
        if (_startOrderCoroutine != null)
        {
            StopCoroutine(_startOrderCoroutine);
        }
        _startOrderCoroutine = StartCoroutine(StartOrderIn(time));
    }

    public void StopTakingOrders()
    {
        if (_startOrderCoroutine != null)
        {
            StopCoroutine(_startOrderCoroutine);
        }

        _currentOrder = null;
    }
}
