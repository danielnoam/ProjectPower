using System;
using System.Collections.Generic;
using DNExtensions;
using UnityEngine;

[DisallowMultipleComponent]
[SelectionBase]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [SerializeField, ReadOnly] private Difficulty gameDifficulty;
    [SerializeField, ReadOnly] private int currentCompletedOrders;
    [SerializeField, ReadOnly] private int totalCompletedOrders;
    [SerializeField, ReadOnly] private int totalFailedOrders;
    
    [Header("References")]
    [SerializeField] private SOGameSettings gameSettings;
    private readonly List<OrderCounter> _orderCounters = new List<OrderCounter>();
    
    
    public Difficulty GameDifficulty => gameDifficulty;

    
    
    private void Awake()
    {
        if (!Instance || Instance == this)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        FindOrderCounters();
        StartGame();
    }

    private void OnEnable()
    {
        if (_orderCounters.Count == 0) return;
        
        foreach (var orderCounter in _orderCounters)
        {
            orderCounter.OnOrderFinishedEvent += OnOrderFinished;
        }
    }
    
    private void OnDisable()
    {
        if (_orderCounters.Count == 0) return;
        
        foreach (var orderCounter in _orderCounters)
        {
            orderCounter.OnOrderFinishedEvent -= OnOrderFinished;
        }
    }

    private void OnOrderFinished(bool success)
    {
        if (success) 
        {
            totalCompletedOrders++;
            
            if (totalCompletedOrders >= gameSettings.OrdersNeededToCompleteGame)
            {
                FinishGame();
            }
            else
            {
                if (currentCompletedOrders >= gameSettings.OrdersNeededToChangeDifficulty)
                {
                    switch (gameDifficulty)
                    {
                        case Difficulty.Easy:
                            gameDifficulty = Difficulty.Medium;
                            currentCompletedOrders = 0;
                            break;
                        case Difficulty.Medium:
                            gameDifficulty = Difficulty.Hard;
                            currentCompletedOrders = 0;
                            break;
                    }
                }
            }
        }
        else
        {
            totalFailedOrders++;
        }
    }

    private void FinishGame()
    {
        foreach (var orderCounter in _orderCounters)
        {
            orderCounter.OnOrderFinishedEvent -= OnOrderFinished;
            orderCounter.StopTakingOrders();
        }
    }

    private void FindOrderCounters()
    {
        _orderCounters.Clear();

        var orderCounterObjects = FindObjectsByType<OrderCounter>(FindObjectsSortMode.None);
        
        foreach (var orderCounter in orderCounterObjects)
        {
            if (orderCounter)
            {
                _orderCounters.Add(orderCounter);
                orderCounter.OnOrderFinishedEvent += OnOrderFinished;
            }
        }
    }
    

    private void StartGame()
    {
        if (_orderCounters.Count == 0) return;
        
        gameDifficulty = Difficulty.Easy;
        currentCompletedOrders = 0;
        totalCompletedOrders = 0;
        totalFailedOrders = 0;
        var firstOrderCounterStartedWithNoDelay = false;
        foreach (var orderCounter in _orderCounters)
        {
            if (!firstOrderCounterStartedWithNoDelay)
            {
                firstOrderCounterStartedWithNoDelay = true;
                orderCounter.StartNewOrder();
            }
            else
            {
                orderCounter.StartNewOrder(gameSettings.GetRandomTimeBetweenOrders());
            }
        }
    }
}
