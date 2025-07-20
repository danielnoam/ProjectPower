using System;
using System.Collections;
using System.Collections.Generic;
using DNExtensions;
using UnityEngine;

public class PackageSpawner : MonoBehaviour
{

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

    private void Start()
    {
        FindPackagesInGame();
        FindPowerMachines();
    }

    private void OnEnable()
    {
        GameManager.Instance.OnGameStarted += OnGameStarted;
    }
    
    private void OnDisable()
    {
        GameManager.Instance.OnGameStarted -= OnGameStarted;
    }
    
    
    private void OnGameStarted()
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
    
    
    private void FindPackagesInGame()
    {
        _packagesInGame.Clear();
        NumberdPackage[] allPackages = FindObjectsByType<NumberdPackage>(FindObjectsSortMode.None);
        foreach (var package in allPackages)
        {
            if (package && !_packagesInGame.Contains(package))
            {
                _packagesInGame.Add(package);
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
                powerMachine.OnPackageProcessed += OnPackageProcessed;
                powerMachine.OnPackageSpawned += OnPackageSpawned;
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
                orderCounter.OnOrderFinishedEvent += OnOrderFinished;
                orderCounter.OnOrderStartedEvent += OnOrderStarted;
            }
        }
    }

    private void OnOrderStarted(Order order)
    {
        if (order == null) return;
    
        bool canFulfillOrder = false;

        foreach (var package in _packagesInGame)
        {
            if (package && package.Number == order.targetNumber)
            {
                canFulfillOrder = true;
                break;
            }
        }

        if (!canFulfillOrder)
        {
            var availableMachines = GameManager.Instance.PowerMachines;
        
            foreach (var machine in availableMachines.Keys)
            {
                if (machine.CanProduceNumber(order.targetNumber))
                {
                    float root = Mathf.Pow(order.targetNumber, 1f / machine.Power);
                    int rootNumber = Mathf.RoundToInt(root);
                
                    SpawnPackage(rootNumber);
                    break;
                }
            }
        }
    }
    
    private void OnOrderFinished(bool success, NumberdPackage package)
    {
        if (success &&package && _packagesInGame.Contains(package))
        {
            _packagesInGame.Remove(package);
            Destroy(package.gameObject);
        }
    }

    private void OnPackageProcessed(NumberdPackage package)
    {
        if (package && _packagesInGame.Contains(package))
        {
            _packagesInGame.Remove(package);
            Destroy(package.gameObject);
        }
        
    }
    
    private void OnPackageSpawned(NumberdPackage package)
    {
        if (package && !_packagesInGame.Contains(package))
        {
            _packagesInGame.Add(package);
        }
    }

    private IEnumerator SpawnPackages(int count, float timeBetweenSpawns = 0.5f, float delayBeforeStart = 0f)
    {
        if (!packageSpawnPosition || !gameSettings) yield break;

        if (delayBeforeStart > 0f)
        {
            yield return new WaitForSeconds(delayBeforeStart);
        }
        
        for (int i = 0; i < count; i++)
        {
            if (_packagesInGame.Count >= gameSettings.MaxPackagesInGame) yield break;
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
            oldestPackage.IntoTheAbyss();
        }
        
        var randomRotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
        var randomPositionOffset = new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 0f, UnityEngine.Random.Range(-0.5f, 0.5f));
        NumberdPackage package = Instantiate(gameSettings.GetPackagePrefabByNumber(packageNumber), packageSpawnPosition.position + randomPositionOffset, randomRotation, packagesHolder);
        package.SetNumber(packageNumber);
        _packagesInGame.Add(package);
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
