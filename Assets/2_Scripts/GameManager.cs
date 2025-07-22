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
    private int _currencyAtStartOfDay;
    
    public Dictionary<PowerMachine, int> PowerMachines => _powerMachines;
    public Difficulty GameDifficulty => currentDifficulty;
    public event Action OnGameStarted;
    public event Action<int> OnDayStarted; // day number
    public event Action<int> OnDayFinished; // day number
    public event Action<int> OnCurrencyChanged; // current currency amount
    public event Action<int> OnOrderCompleted; // total completed orders in the current day
    public event Action<int> OnOrderFailed; // total failed orders in the current day
    
    
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

        FindOrderCounters();
        FindPackageSpawners();
        FindPowerMachines();
        
        currentDay = 1;
        currentCurrency = 0;
        lifetimeCompletedOrders = 0;
        lifetimeFailedOrders = 0;
    }

    private void Start()
    {
        VFXManager.Instance?.PlayVFX(introVFXSequence);
        OnGameStarted?.Invoke();
    }

    private void OnEnable()
    {
        if (_orderCounters.Count != 0)
        {
            foreach (var orderCounter in _orderCounters)
            {
                orderCounter.OnOrderFinishedEvent += OnOrderFinished;
            }
        }
        

    }
    
    private void OnDisable()
    {
        if (_orderCounters.Count != 0)
        {
            foreach (var orderCounter in _orderCounters)
            {
                orderCounter.OnOrderFinishedEvent -= OnOrderFinished;
            }
        }
    }

    private void OnOrderFinished(bool success, List<NumberdPackage> packagesInDeliveryArea, int orderWorth)
    {
        if (success) 
        {
            totalDayCompletedOrders += 1;
            currentDifficultyCompletedOrders += 1;
            currentCurrency += orderWorth;
            OnCurrencyChanged?.Invoke(currentCurrency);
            OnOrderCompleted?.Invoke(totalDayCompletedOrders);
            
            if (totalDayCompletedOrders >= gameSettings.OrdersNeededToCompleteDay)
            {
                FinishDay(true);
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
            totalDayFailedOrders += 1;
            OnOrderFailed?.Invoke(totalDayFailedOrders);

            
            if (totalDayFailedOrders >= gameSettings.OrderFailuresToFailDay)
            {
                FinishDay(false);
                return;
            }
        }
    }




    private void FinishDay(bool success)
    {
        if (success)
        {
            currentDay++;
            lifetimeCompletedOrders += totalDayCompletedOrders;
            lifetimeFailedOrders += totalDayFailedOrders;
            OnDayFinished?.Invoke(currentDay);
        }
        else
        {
            currentDifficulty = Difficulty.Easy;
            currentDifficultyCompletedOrders = 0;
            totalDayCompletedOrders = 0;
            totalDayFailedOrders = 0;
            currentCurrency = _currencyAtStartOfDay; 
            lifetimeFailedOrders += totalDayFailedOrders;
            OnDayFinished?.Invoke(currentDay);
        }

    }
    

    public void StartDay()
    {
        if (_orderCounters.Count == 0) return;
        
        currentDifficulty = Difficulty.Easy;
        currentDifficultyCompletedOrders = 0;
        totalDayCompletedOrders = 0;
        totalDayFailedOrders = 0;
        
        
        OnDayStarted?.Invoke(currentDay);
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


}
