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
    private Coroutine _spawnCoroutine;

    private void Awake()
    {
        FindPackagesInGame();
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
            SpawnPackage(gameSettings.GetRandomPackageNumber());
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
        if (!packageSpawnPosition || !gameSettings || _packagesInGame.Count >= gameSettings.MaxPackagesInGame) return;
        
        NumberdPackage package = Instantiate(gameSettings.GetPackagePrefabByNumber(packageNumber), packageSpawnPosition.position, Quaternion.identity, packagesHolder);
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
