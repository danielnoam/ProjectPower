using System;
using System.Collections;
using System.Collections.Generic;
using DNExtensions;
using UnityEngine;

public class DeliveryArea : MonoBehaviour
{
    
    [Header("Delivery Area")]
    [SerializeField, ReadOnly] private List<NumberdPackage> packagesInArea = new List<NumberdPackage>();

    [Header("References")]
    [SerializeField] private SOGameSettings gameSettings;

    private Coroutine _startOrderCoroutine;
    private Order _currentOrder;
    private int _completedOrdersCount;
    private int _failedOrdersCount;
    
    public Order CurrentOrder => _currentOrder;
    public event Action<Order> OnOrderStartedEvent;
    public event Action<Order> OnOrderFinishedEvent;
    
    private void Update()
    {
        _currentOrder?.UpdateTimeLeft();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out NumberdPackage package))
        {
            AddPackage(package);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out NumberdPackage package))
        {
            RemovePackage(package);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (_currentOrder == null) return;
        
        if (other.TryGetComponent(out NumberdPackage package))
        {
            TryCompleteOrder(package);
        }
    }
    
    private void AddPackage(NumberdPackage package)
    {
        if (package && !packagesInArea.Contains(package))
        {
            packagesInArea.Add(package);
            TryCompleteOrder(package);
        }
    }

    private void RemovePackage(NumberdPackage package)
    {
        if (package && packagesInArea.Contains(package))
        {
            packagesInArea.Remove(package);
        }
    }
    
    private void TryCompleteOrder(NumberdPackage package)
    {
        if (_currentOrder == null) return;
        
        if (_currentOrder.TryCompleteOrder(package))
        {
            RemovePackage(package);
            _completedOrdersCount++;
            OnOrderFinishedEvent?.Invoke(_currentOrder);
            _currentOrder.OnOrderFailedEvent -= OnOrderFailed;
            _currentOrder = null;
            StartNewOrder();
        }
    }
    
    
    private void OnOrderFailed()
    {
        _failedOrdersCount++;
        _currentOrder.OnOrderFailedEvent -= OnOrderFailed;
        OnOrderFinishedEvent?.Invoke(_currentOrder);
        _currentOrder = null;
        StartNewOrder();
    }
    
    [Button]
    private void StartNewOrder()
    {
        if (!gameSettings) return;
        
        if (_startOrderCoroutine != null)
        {
            StopCoroutine(_startOrderCoroutine);
        }
        _startOrderCoroutine = StartCoroutine(StartOrderIn(gameSettings.GetRandomTimeBetweenOrders()));
    }

    private IEnumerator StartOrderIn(float time)
    {
        yield return new WaitForSeconds(time);
        _currentOrder = new Order(gameSettings);
        _currentOrder.OnOrderFailedEvent += OnOrderFailed;
        OnOrderStartedEvent?.Invoke(_currentOrder);
        
    }
}
