using System;
using System.Collections.Generic;
using DNExtensions;
using DNExtensions.VFXManager;
using UnityEngine;

[DefaultExecutionOrder(-1)]
[DisallowMultipleComponent]
[SelectionBase]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private SOGameSettings gameSettings;
    [SerializeField] private SOVFEffectsSequence introVFXSequence;


    [Header("Game State")]
    [SerializeField, ReadOnly] private int currentDay;
    [SerializeField, ReadOnly] private int currentCurrency;
    [SerializeField, ReadOnly] private int lifetimeCompletedOrders;
    [SerializeField, ReadOnly] private int lifetimeFailedOrders;
    
    [Header("Current Day")]
    [SerializeField, ReadOnly] private Difficulty currentDifficulty;
    [SerializeField, ReadOnly] private int currentDifficultyCompletedOrders;
    [SerializeField, ReadOnly] private int totalDayCompletedOrders;
    [SerializeField, ReadOnly] private int totalDayFailedOrders;
    
    
    private readonly List<OrderCounter> _orderCounters = new List<OrderCounter>();
    private readonly List<PackageSpawner> _packageSpawners = new List<PackageSpawner>();
    private readonly Dictionary<PowerMachine, int> _powerMachines = new Dictionary<PowerMachine, int>();
    
    
    public Dictionary<PowerMachine, int> PowerMachines => _powerMachines;
    public Difficulty GameDifficulty => currentDifficulty;
    public event Action OnDayStarted;
    public event Action OnDayFinished;
    
    
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

        currentDay = 1;
        currentCurrency = 0;
        lifetimeCompletedOrders = 0;
        lifetimeFailedOrders = 0;
    }

    private void Start()
    {
        VFXManager.Instance?.PlayVFX(introVFXSequence);
        FindOrderCounters();
        FindPackageSpawners();
        FindPowerMachines();
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

    private void OnOrderFinished(bool success, NumberdPackage package, int orderWorth)
    {
        if (success) 
        {
            totalDayCompletedOrders++;
            currentCurrency += orderWorth;
            
            if (totalDayCompletedOrders >= gameSettings.OrdersNeededToCompleteDay)
            {
                FinishDay();
                return;
            }

            if (currentDifficultyCompletedOrders >= gameSettings.OrdersNeededToChangeDifficulty)
            {
                switch (currentDifficulty)
                {
                    case Difficulty.Easy:
                        currentDifficulty = Difficulty.Medium;
                        currentDifficultyCompletedOrders = 0;
                        break;
                    case Difficulty.Medium:
                        currentDifficulty = Difficulty.Hard;
                        currentDifficultyCompletedOrders = 0;
                        break;
                }
            }
        }
        else
        {
            totalDayFailedOrders++;
            
            if (totalDayFailedOrders >= gameSettings.OrderFailuresToFailDay)
            {
                FailDay();
                return;
            }
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
    
    private void FindPowerMachines()
    {
        _powerMachines.Clear();

        var powerMachineObjects = FindObjectsByType<PowerMachine>(FindObjectsSortMode.None);
        
        foreach (var powerMachine in powerMachineObjects)
        {
            if (powerMachine)
            {
                if (!_powerMachines.ContainsKey(powerMachine))
                {
                    _powerMachines.Add(powerMachine, powerMachine.Power);
                }
            }
        }
    }
    
    private void FinishDay()
    {
        foreach (var orderCounter in _orderCounters)
        {
            orderCounter.OnOrderFinishedEvent -= OnOrderFinished;
            orderCounter.StopTakingOrders();
        }
        currentDay++;
        lifetimeCompletedOrders += totalDayCompletedOrders;
        lifetimeFailedOrders += totalDayFailedOrders;
        OnDayFinished?.Invoke();
    }
    
    private void FailDay()
    {
        foreach (var orderCounter in _orderCounters)
        {
            orderCounter.OnOrderFinishedEvent -= OnOrderFinished;
            orderCounter.StopTakingOrders();
        }
        lifetimeFailedOrders += totalDayFailedOrders;
        OnDayFinished?.Invoke();
    }
    

    public void StartDay()
    {
        if (_orderCounters.Count == 0) return;
        
        currentDifficulty = Difficulty.Easy;
        currentDifficultyCompletedOrders = 0;
        totalDayCompletedOrders = 0;
        totalDayFailedOrders = 0;
        
        var firstOrderCounterStartedWithNoDelay = false;
        foreach (var orderCounter in _orderCounters)
        {
            if (!firstOrderCounterStartedWithNoDelay)
            {
                firstOrderCounterStartedWithNoDelay = true;
                orderCounter.StartNewOrder(2);
            }
            else
            {
                orderCounter.StartNewOrder(gameSettings.TimeBetweenOrders.maxValue);
            }
        }
        
        OnDayStarted?.Invoke();
    }
    

}
