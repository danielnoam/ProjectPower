using System;
using System.Collections.Generic;
using DNExtensions;
using DNExtensions.VFXManager;
using UnityEngine;

[DisallowMultipleComponent]
[SelectionBase]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private SOGameSettings gameSettings;
    [SerializeField] private SOVFEffectsSequence introVFXSequence;

    
    [Header("Current Game State")]
    [SerializeField, ReadOnly] private Difficulty gameDifficulty;
    [SerializeField, ReadOnly] private int currentCompletedOrders;
    [SerializeField, ReadOnly] private int totalCompletedOrders;
    [SerializeField, ReadOnly] private int totalFailedOrders;
    
    
    private readonly List<OrderCounter> _orderCounters = new List<OrderCounter>();
    private readonly List<PackageSpawner> _packageSpawners = new List<PackageSpawner>();
    public Difficulty GameDifficulty => gameDifficulty;
    public event Action OnGameStarted;
    public event Action OnGameFinished;
    
    
    private void Awake()
    {
        if (!Instance || Instance == this)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
    }

    private void Start()
    {
        VFXManager.Instance?.PlayVFX(introVFXSequence);
        FindOrderCounters();
        FindPackageSpawners();
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
        OnGameFinished?.Invoke();
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
    
    private void FindPackageSpawners()
    {
        _packageSpawners.Clear();

        var packageSpawnerObjects = FindObjectsByType<PackageSpawner>(FindObjectsSortMode.None);
        
        foreach (var packageSpawner in packageSpawnerObjects)
        {
            if (packageSpawner)
            {
                _packageSpawners.Add(packageSpawner);
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
                orderCounter.StartNewOrder(5);
            }
            else
            {
                orderCounter.StartNewOrder(gameSettings.TimeBetweenOrders.maxValue);
            }
        }
        OnGameStarted?.Invoke();
    }
}
