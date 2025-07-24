using System;
using System.Collections.Generic;
using DNExtensions;
using DNExtensions.MenuSystem;
using DNExtensions.VFXManager;
using PrimeTween;
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
    [SerializeField] private MenuController mainMenu;


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
    [SerializeField, ReadOnly] private SODayData currentDayData;
    
    
    private readonly List<OrderCounter> _orderCounters = new List<OrderCounter>();
    private readonly Dictionary<PowerMachine, int> _powerMachines = new Dictionary<PowerMachine, int>();
    private int _currencyAtStartOfDay;

    
    public Dictionary<PowerMachine, int> PowerMachines => _powerMachines;
    public Difficulty GameDifficulty => currentDifficulty;
    public SODayData CurrentDayData => currentDayData;
    public int CurrentDay => currentDay;
    public int CurrentCurrency => currentCurrency;

    public event Action OnGameStarted;
    public event Action<SODayData> OnDayStarted; // day number
    public event Action<SODayData> OnDayFinished; // day number
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

        PrimeTweenConfig.warnEndValueEqualsCurrent = false;
        
        FindOrderCounters();
        FindPowerMachines();
    }

    private void Start()
    {
        currentDay = 0;
        lifetimeCompletedOrders = 0;
        lifetimeFailedOrders = 0;
        UpdateCurrency(0);
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
            UpdateCurrency(currentCurrency+orderWorth);
            OnOrderCompleted?.Invoke(totalDayCompletedOrders);
            
            if (totalDayCompletedOrders >= currentDayData.OrdersNeededToCompleteDay)
            {
                FinishDay(true);
                return;
            }

            if (currentDifficultyCompletedOrders >= currentDayData.OrdersNeededToChangeDifficulty)
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

            
            if (totalDayFailedOrders >= currentDayData.OrderFailuresToFailDay)
            {
                FinishDay(false);
                return;
            }
        }
    }




    private void FinishDay(bool success)
    {
        if (!currentDayData) return;
        
        if (success)
        {
            lifetimeCompletedOrders += totalDayCompletedOrders;
            lifetimeFailedOrders += totalDayFailedOrders;
            OnDayFinished?.Invoke(currentDayData);
            currentDayData = null;
        }
        else
        {
            UpdateCurrency(_currencyAtStartOfDay); 
            lifetimeFailedOrders += totalDayFailedOrders;
            OnDayFinished?.Invoke(currentDayData);
            currentDayData = null;
        }

    }
    

    public void StartNewDay()
    {

        currentDay += 1;
        currentDayData = gameSettings.GetDayData(currentDay);
        currentDifficulty = Difficulty.Easy;
        _currencyAtStartOfDay = currentCurrency;
        currentDifficultyCompletedOrders = 0;
        totalDayCompletedOrders = 0;
        totalDayFailedOrders = 0;
        
        OnDayStarted?.Invoke(currentDayData);
    }

    public void UpdateCurrency(int amount)
    {
        currentCurrency = amount;
        OnCurrencyChanged?.Invoke(currentCurrency);
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

    [Button]
    private void ForceCompleteDay()
    {
        FinishDay(true);
    }

    [Button]
    public bool TogglePause()
    {
        if (mainMenu.gameObject.activeSelf == false)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            mainMenu.gameObject.SetActive(true);
            Time.timeScale = 0;
            return true;
        }
        
        
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
        mainMenu.gameObject.SetActive(false);
        Time.timeScale = 1;
        return false;
    }


}
