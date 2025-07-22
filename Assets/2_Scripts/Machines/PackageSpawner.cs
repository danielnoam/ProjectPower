using System;
using System.Collections;
using System.Collections.Generic;
using DNExtensions;
using UnityEngine;

public class PackageSpawner : MonoBehaviour
{
    [SerializeField, ReadOnly] private int packagesInGame;

    [Header("Spawner Settings")]
    [SerializeField, Min(0)] private int initialPackageCount = 5;
    
    [Header("References")]
    [SerializeField] private SOGameSettings gameSettings;
    [SerializeField] private Transform packageSpawnPosition;
    [SerializeField] private Transform packagesHolder;
    
    private readonly List<NumberdPackage> _packagesInGame = new List<NumberdPackage>();
    private readonly List<PowerMachine> _powerMachines = new List<PowerMachine>();
    private readonly List<OrderCounter> _orderCounters = new List<OrderCounter>();
    private Coroutine _spawnCoroutine;

    private void Awake()
    {
        FindPackagesInGame();
        FindPowerMachines();
        FindOrderCounters();
    }

    private void OnEnable()
    {
        GameManager.Instance.OnDayStarted += OnDayStarted;

        foreach (var orderCounter in _orderCounters)
        {
            orderCounter.OnOrderFinishedEvent += OnOrderFinished;
            orderCounter.OnOrderStartedEvent += OnOrderStarted;
        }
        
        foreach (var powerMachine in _powerMachines)
        {
            powerMachine.OnPackageProcessed += OnPackageProcessed;
            powerMachine.OnPackageSpawned += OnPackageSpawned;
        }
    }
    
    private void OnDisable()
    {
        GameManager.Instance.OnDayStarted -= OnDayStarted;
        
        foreach (var orderCounter in _orderCounters)
        {
            orderCounter.OnOrderFinishedEvent -= OnOrderFinished;
            orderCounter.OnOrderStartedEvent -= OnOrderStarted;
        }
        
        foreach (var powerMachine in _powerMachines)
        {
            powerMachine.OnPackageProcessed -= OnPackageProcessed;
            powerMachine.OnPackageSpawned -= OnPackageSpawned;
        }
    }
    
    
    private void OnDayStarted(int day)
    {
        if (_packagesInGame.Count < initialPackageCount)
        {
            int packagesToSpawn = initialPackageCount - _packagesInGame.Count;
            if (packagesToSpawn > 0)
            {
                SpawnPackagesBatch(packagesToSpawn, 0.5f, 3f);
            }
        }
    }
    

    private void OnOrderStarted(Order order)
    {
        if (order == null) return;
        
        var availableMachines = GameManager.Instance.PowerMachines;
        var missingNumbers = new List<int>();


        foreach (var numberNeeded in order.NumbersNeeded)
        {
            bool canFulfillNumber = false;
            
            // Check all packages in the game
            foreach (var package in _packagesInGame)
            {
                if (!package) continue;
                
                // Case 1: Package already has the exact number we need
                if (package.Number == numberNeeded)
                {
                    canFulfillNumber = true;
                    break;
                }
                
                // Case 2: Package can be processed by a machine to produce the needed number
                foreach (var machine in availableMachines.Keys)
                {
                    if (machine.CalculateOutput(package) == numberNeeded)
                    {
                        canFulfillNumber = true;
                        break;
                    }
                }
                
                if (canFulfillNumber) break;
            }
            
            // If we can't fulfill this number with existing packages, it's missing
            if (!canFulfillNumber)
            {
                missingNumbers.Add(numberNeeded);
            }
        }

        // For missing numbers, spawn the input packages needed to produce them
        if (missingNumbers.Count > 0)
        {

            foreach (var missingNumber in missingNumbers)
            {
                bool canProduce = false;
                
                // Try each available machine to see if it can produce the missing number
                foreach (var machine in availableMachines.Keys)
                {
                    if (machine.CanProduceNumber(missingNumber))
                    {
                        // Calculate what input number is needed
                        double root = Math.Pow(missingNumber, 1.0 / machine.Power);
                        int rootNumber = Mathf.RoundToInt((float)root);
                        
                        // Verify this input will actually produce the target (handle precision issues)
                        int actualOutput = PowerInt(rootNumber, machine.Power);
                        if (actualOutput == missingNumber)
                        {
                            SpawnPackage(rootNumber);
                            canProduce = true;
                            break;
                        }
                        
                        // If rounding caused issues, try adjacent values
                        for (int testRoot = rootNumber - 1; testRoot <= rootNumber + 1; testRoot++)
                        {
                            if (testRoot > 0 && PowerInt(testRoot, machine.Power) == missingNumber)
                            {
                                SpawnPackage(testRoot);
                                canProduce = true;
                                break;
                            }
                        }
                        
                        if (canProduce) break;
                    }
                }
                
                if (!canProduce)
                {
                    Debug.LogWarning($"Cannot produce missing number {missingNumber} with available machines!");
                }
            }
        }
        else
        {
            Debug.Log("All needed numbers can be fulfilled with existing packages!");
        }
    }
    
    private void OnOrderFinished(bool success, List<NumberdPackage> packagesInDeliveryArea, int orderWorth)
    {
        if (!success) return;
        
        foreach (var package in packagesInDeliveryArea)
        {
            if (!package || !_packagesInGame.Contains(package)) continue;
            _packagesInGame.Remove(package);
            packagesInGame = _packagesInGame.Count;

        }
    }

    private void OnPackageProcessed(NumberdPackage package)
    {
        if (package && _packagesInGame.Contains(package))
        {
            _packagesInGame.Remove(package);
            packagesInGame = _packagesInGame.Count;
        }
        
    }
    
    private void OnPackageSpawned(NumberdPackage package)
    {
        if (package && !_packagesInGame.Contains(package))
        {
            _packagesInGame.Add(package);
            packagesInGame = _packagesInGame.Count;
        }
    }

    private IEnumerator SpawnPackages(int count, float timeBetweenSpawns = 0.5f, float delayBeforeStart = 0f)
    {
        if (!packageSpawnPosition || !gameSettings) yield break;

        if (delayBeforeStart > 0f) yield return new WaitForSeconds(delayBeforeStart);
        
        int totalAfterSpawning = _packagesInGame.Count + count;
        if (totalAfterSpawning > gameSettings.MaxPackagesInGame)
        {
            int excessCount = totalAfterSpawning - gameSettings.MaxPackagesInGame;
        
            for (int i = 0; i < excessCount; i++)
            {
                if (_packagesInGame.Count > 0)
                {
                    var oldestPackage = _packagesInGame[0];
                    _packagesInGame.RemoveAt(0);
                    packagesInGame = _packagesInGame.Count;
                    oldestPackage.IntoTheAbyss();
                }
            }
        }
    
        for (int i = 0; i < count; i++)
        {
            SpawnPackage(gameSettings.PackageNumbersRange.RandomValue);
            yield return new WaitForSeconds(timeBetweenSpawns);
        }
    }

    [Button]
    public void SpawnPackagesBatch(int count = 3, float timeBetweenSpawns = 0.5f, float delayBeforeStart = 0f)
    {
        if (_spawnCoroutine != null) 
        {
            StopCoroutine(_spawnCoroutine);
        }
        _spawnCoroutine = StartCoroutine(SpawnPackages(count, timeBetweenSpawns, delayBeforeStart));
    }
    
    [Button]
    private void SpawnPackage(int packageNumber = 2)
    {
        if (!packageSpawnPosition || !gameSettings) return;

        if (_packagesInGame.Count >= gameSettings.MaxPackagesInGame)
        {
            var oldestPackage = _packagesInGame[0];
            _packagesInGame.RemoveAt(0);
            packagesInGame = _packagesInGame.Count;
            oldestPackage.IntoTheAbyss();
        }
        
        var randomRotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
        var randomPositionOffset = new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 0f, UnityEngine.Random.Range(-0.5f, 0.5f));
        NumberdPackage package = Instantiate(gameSettings.GetPackagePrefabByNumber(packageNumber), packageSpawnPosition.position + randomPositionOffset, randomRotation, packagesHolder);
        package.SetNumber(packageNumber);
        _packagesInGame.Add(package);
        packagesInGame = _packagesInGame.Count;
    }
    
    private int PowerInt(int baseValue, int exponent)
    {
        if (exponent == 0) return 1;
        if (exponent == 1) return baseValue;
    
        int result = 1;
        for (int i = 0; i < exponent; i++)
        {
            result *= baseValue;
        }
        return result;
    }
    
    private void FindPackagesInGame()
    {
        _packagesInGame.Clear();
        NumberdPackage[] allPackages = FindObjectsByType<NumberdPackage>(FindObjectsSortMode.None);
        foreach (var package in allPackages)
        {
            if (package && !_packagesInGame.Contains(package))
            {
                _packagesInGame.Add(package);
                packagesInGame = _packagesInGame.Count;
            }
        }
    }
    
    private void FindPowerMachines()
    {
        _powerMachines.Clear();
        PowerMachine[] allPowerMachines = FindObjectsByType<PowerMachine>(FindObjectsSortMode.None);
        foreach (var powerMachine in allPowerMachines)
        {
            if (powerMachine && !_powerMachines.Contains(powerMachine))
            {
                _powerMachines.Add(powerMachine);
            }
        }
    }
    
    private void FindOrderCounters()
    {
        _orderCounters.Clear();
        OrderCounter[] allOrderCounters = FindObjectsByType<OrderCounter>(FindObjectsSortMode.None);
        foreach (var orderCounter in allOrderCounters)
        {
            if (orderCounter && !_orderCounters.Contains(orderCounter))
            {
                _orderCounters.Add(orderCounter);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (packageSpawnPosition)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(packageSpawnPosition.position, 0.5f);
        }
    }
}
