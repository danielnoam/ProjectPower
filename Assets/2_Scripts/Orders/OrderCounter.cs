using System;
using System.Collections;
using System.Collections.Generic;
using DNExtensions;
using PrimeTween;
using UnityEngine;

[DisallowMultipleComponent]
public class OrderCounter : MonoBehaviour
{
    [Header("Blinds Animation")]
    [SerializeField] private float blindsOpenDuration = 1.5f;
    [SerializeField] private float blindsCloseDuration = 1.5f;
    [SerializeField] private Ease blindsOpenEase = Ease.OutBack;
    [SerializeField] private Ease blindsCloseEase = Ease.InBack;
    [SerializeField] private Vector3 blindsOpenPosition = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 blindsClosePosition = new Vector3(0, -1.5f, 0);
    
    [Header("References")]
    [SerializeField] private SOGameSettings gameSettings;
    [SerializeField] private OrderDeliveryArea orderDeliveryArea;
    [SerializeField] private Transform clientHolder;
    [SerializeField] private Transform blindsTransform;

    private Sequence _blindsAnimationSequence;
    private Coroutine _startOrderCoroutine;
    private Order _currentOrder;
    private GameObject _currentClient;
    private bool _isTakingOrders;
    private SODayData _currentDayData;

    public event Action<Order> OnOrderStartedEvent;
    public event Action<bool, List<NumberdPackage>, int> OnOrderFinishedEvent;
    public event Action<Order> OnOrderTimeChangedEvent;
    public event Action OnStoppedTakingOrdersEvent;
    

    private void OnEnable()
    {
        orderDeliveryArea.OnPackageEnteredArea += OnPackageEnteredOrderDeliveryArea;
        
        if (GameManager.Instance)
        {
            GameManager.Instance.OnDayFinished += OnDayFinished;
            GameManager.Instance.OnDayStarted += OnDayStarted;
        }
    }


    private void OnDisable()
    {
        orderDeliveryArea.OnPackageEnteredArea -= OnPackageEnteredOrderDeliveryArea;
        
        if (GameManager.Instance)
        {
            GameManager.Instance.OnDayFinished -= OnDayFinished;
            GameManager.Instance.OnDayStarted -= OnDayStarted;
        }
    }
    

    private void Update()
    {
        UpdateOrderTimeLeft();
    }
    
    private void OnDayFinished(SODayData dayData)
    {
        _currentDayData = null;
        StopTakingOrders();
    }
    
    private void OnDayStarted(SODayData dayData)
    {
        if (!dayData) return;
        
        _currentDayData = dayData;
        StartTakingOrders(_currentDayData.TimeBetweenOrders.minValue);
    }

    
    private void OnPackageEnteredOrderDeliveryArea(List<NumberdPackage> packagesInDeliveryArea)
    {
        TryCompleteOrder(packagesInDeliveryArea);
    }
    
    private void TryCompleteOrder(List<NumberdPackage> packagesInDeliveryArea)
    {
        if (_currentOrder == null) return;
        
        if (_currentOrder.IsOrderCompleted(packagesInDeliveryArea, out List<NumberdPackage> usedPackages))
        {
            foreach (var package in usedPackages)
            {
                orderDeliveryArea.RemovePackage(package);
                package.IntoTheAbyss();
            }
            OnOrderFinishedEvent?.Invoke(true, usedPackages, _currentOrder.Worth);
            _currentOrder = null;
            
            if (_currentDayData) TakeAnotherOrder(_currentDayData.TimeBetweenOrders.RandomValue);
        }
    }
    
    private void UpdateOrderTimeLeft()
    {
        if (_currentOrder == null) return;
        
        _currentOrder.TimeLeft -= Time.deltaTime;
        OnOrderTimeChangedEvent?.Invoke(_currentOrder);
        if (_currentOrder.TimeLeft <= 0)
        {
            FailOrder();
        }
    }    
    
    private IEnumerator StartOrderIn(float time)
    {
        if (_currentOrder != null || !_isTakingOrders) yield break;
        
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
    private void StartTakingOrders(float time = 0.1f)
    {
        if (_isTakingOrders) return;
        
        _isTakingOrders = true;
        
        if (_blindsAnimationSequence.isAlive) _blindsAnimationSequence.Stop();
        _blindsAnimationSequence = Sequence.Create()
            .Group(Tween.LocalPosition(blindsTransform, blindsOpenPosition, blindsOpenDuration, blindsOpenEase));
        
        if (_startOrderCoroutine != null)
        {
            StopCoroutine(_startOrderCoroutine);
        }
        _startOrderCoroutine = StartCoroutine(StartOrderIn(time));
    }

    [Button]
    private void StopTakingOrders()
    {
        if (!_isTakingOrders) return;
        
        _isTakingOrders = false;
        
        if (_blindsAnimationSequence.isAlive) _blindsAnimationSequence.Stop();
        _blindsAnimationSequence = Sequence.Create()
            .Group(Tween.LocalPosition(blindsTransform, blindsClosePosition, blindsCloseDuration, blindsCloseEase));
        
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
    
    
    private void TakeAnotherOrder(float time)
    {
        if (!_isTakingOrders || _currentOrder != null) return;
        
        
        if (_startOrderCoroutine != null)
        {
            StopCoroutine(_startOrderCoroutine);
        }
        _startOrderCoroutine = StartCoroutine(StartOrderIn(time));
    }

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
        
        if (_currentDayData) TakeAnotherOrder(_currentDayData.TimeBetweenOrders.RandomValue);
    }
    
    
}
