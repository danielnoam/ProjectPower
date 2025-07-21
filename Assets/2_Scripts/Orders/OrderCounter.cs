using System;
using System.Collections;
using DNExtensions;
using UnityEngine;

[DisallowMultipleComponent]
public class OrderCounter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SOGameSettings gameSettings;
    [SerializeField] private OrderDeliveryArea orderDeliveryArea;
    [SerializeField] private Transform clientHolder;

    private Coroutine _startOrderCoroutine;
    private Order _currentOrder;
    private GameObject _currentClient;
    

    public event Action<Order> OnOrderStartedEvent;
    public event Action<bool, NumberdPackage, int> OnOrderFinishedEvent;
    public event Action<Order> OnOrderTimeChangedEvent;
    public event Action OnStoppedTakingOrdersEvent;
    

    private void OnEnable()
    {
        orderDeliveryArea.OnPackageEnteredArea += OnPackageEnteredOrderDeliveryArea;
    }

    private void OnDisable()
    {
        orderDeliveryArea.OnPackageEnteredArea -= OnPackageEnteredOrderDeliveryArea;
    }
    

    private void Update()
    {
        UpdateOrderTimeLeft();
    }
    
    
    private void OnPackageEnteredOrderDeliveryArea(NumberdPackage package)
    {
        TryCompleteOrder(package);
    }
    
    private void TryCompleteOrder(NumberdPackage package)
    {
        if (_currentOrder == null) return;
        
        if (_currentOrder.IsOrderCompleted(package))
        {
            OnOrderFinishedEvent?.Invoke(true, package, _currentOrder.worth);

            _currentOrder = null;
            StartNewOrder(gameSettings.TimeBetweenOrders.RandomValue);
            orderDeliveryArea.RemovePackage(package);
            package.IntoTheAbyss();
        }
    }
    
    private void UpdateOrderTimeLeft()
    {
        if (_currentOrder == null) return;
        
        _currentOrder.timeLeft -= Time.deltaTime;
        OnOrderTimeChangedEvent?.Invoke(_currentOrder);
        if (_currentOrder.timeLeft <= 0)
        {
            FailOrder();
        }
    }    
    
    private IEnumerator StartOrderIn(float time)
    {
        yield return new WaitForSeconds(time);
        _currentOrder = new Order(gameSettings, GameManager.Instance?.GameDifficulty ?? Difficulty.Easy);
        if (_currentClient) 
        {
            Destroy(_currentClient);
        }
        _currentClient = Instantiate(gameSettings.GetRandomClientPrefab(), clientHolder);
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
        
        if (_currentClient)
        {
            Destroy(_currentClient);
            _currentClient = null;
        }
        
        OnStoppedTakingOrdersEvent?.Invoke();
    }
    
    [Button]
    public void FailOrder()
    {
        if (_currentOrder == null) return;
        
        OnOrderFinishedEvent?.Invoke(false, null, 0);
        _currentOrder = null;
        if (_currentClient)
        {
            Destroy(_currentClient);
            _currentClient = null;
        }
        StartNewOrder(gameSettings.TimeBetweenOrders.RandomValue);
    }
}
