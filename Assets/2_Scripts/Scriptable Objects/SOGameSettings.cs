
using System.Collections.Generic;
using System.Linq;
using DNExtensions;
using PrimeTween;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "Game Settings", menuName = "Scriptable Objects/New Game Settings", order = 1)]
public class SOGameSettings : ScriptableObject
{
    public SOGameSettings()
    {
        PrimeTweenConfig.warnEndValueEqualsCurrent = false;
    }
    
    [Header("Package Settings")]
    [SerializeField] private int smallPackageMaxNumber = 500;
    [SerializeField] private int mediumPackageMaxNumber = 1000;
    [SerializeField, Min(20)] private int maxPackagesInGame = 80;
    [SerializeField, MinMaxRange(2,9)] private RangedInt packageNumbersRange = new RangedInt(2, 9);
    
    
    [Header("Order Settings")]
    [SerializeField] private int ordersNeededToChangeDifficulty = 7;
    [SerializeField] private int ordersNeededToCompleteGame = 30;
    [SerializeField, MinMaxRange(1,10)] private RangedFloat timeBetweenOrders = new RangedFloat(5f, 10f);
    [SerializeField] private SOOrderCombinations easyOrderCombinations;
    [SerializeField] private SOOrderCombinations mediumOrderCombinations;
    [SerializeField] private SOOrderCombinations hardOrderCombinations;
    
    [Header("Interaction Settings")]
    [SerializeField, Range(0,10)] private float outlineWidth = 5f;
    [SerializeField] private Color outlineColor = Color.cornflowerBlue;
    [SerializeField] private Outline.Mode outlineMode = Outline.Mode.OutlineVisible;
    
    [Header("Resources")]
    [SerializeField] private NumberdPackage smallPackagePrefab;
    [SerializeField] private NumberdPackage mediumPackagePrefab;
    [SerializeField] private NumberdPackage largePackagePrefab;
    [SerializeField] private GameObject[] clientsPrefabs;
    
    
    public int MaxPackagesInGame => maxPackagesInGame;
    public RangedFloat TimeBetweenOrders => timeBetweenOrders;
    public RangedInt PackageNumbersRange => packageNumbersRange;
    public int OrdersNeededToChangeDifficulty => ordersNeededToChangeDifficulty;
    public int OrdersNeededToCompleteGame => ordersNeededToCompleteGame;
    public float OutlineWidth => outlineWidth;
    public Color OutlineColor => outlineColor;
    public Outline.Mode OutlineMode => outlineMode;

    
    
    public NumberdPackage GetPackagePrefabByNumber(int number)
    {
        if (number <= smallPackageMaxNumber)
        {
            return smallPackagePrefab;
        }
        else if (number <= mediumPackageMaxNumber)
        {
            return mediumPackagePrefab;
        }
        else
        {
            return largePackagePrefab;
        }
    }
    
    
    public GameObject GetRandomClientPrefab()
    {
        if (clientsPrefabs == null || clientsPrefabs.Length == 0)
        {
            return null;
        }
        
        int randomIndex = Random.Range(0, clientsPrefabs.Length);
        return clientsPrefabs[randomIndex];
    }
    

    public (int key, float value) GetOrderNumber(Dictionary<PowerMachine, int> availablePowerMachines, Difficulty difficulty)
    {
        var orderCombinations = easyOrderCombinations;
        switch (difficulty)
        {
            case Difficulty.Medium:
                orderCombinations = mediumOrderCombinations;
                break;
            case Difficulty.Hard:
                orderCombinations = hardOrderCombinations;
                break;
        }
        
        var validCombinations = orderCombinations.GetCombinations().Where(kvp => CanAchieveWithAvailableMachines(kvp.Key, availablePowerMachines)).ToList();
        if (validCombinations.Count == 0)
        {
            var keys = orderCombinations.GetCombinations().Keys.ToList();
            int randomIndex = Random.Range(0, keys.Count);
            int key = keys[randomIndex];
            float value = orderCombinations.GetCombinations()[key];
            return (key, value);
        }
        
        int validRandomIndex = Random.Range(0, validCombinations.Count);
        var selectedCombination = validCombinations[validRandomIndex];
        return (selectedCombination.Key, selectedCombination.Value);
    }
    
    private bool CanAchieveWithAvailableMachines(int targetNumber, Dictionary<PowerMachine, int> availablePowerMachines)
    {
        foreach (var machine in availablePowerMachines.Keys)
        {
            if (machine.CanProduceNumber(targetNumber))
            {
                return true;
            }
        }
        return false;
    }
    
    
    


}
