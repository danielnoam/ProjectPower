using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using DNExtensions;
using UnityEngine;

[CreateAssetMenu(fileName = "Game Settings", menuName = "Scriptable Objects/New Game Settings", order = 1)]
public class SOGameSettings : ScriptableObject
{
    [Header("Package Settings")]
    [SerializeField] private int smallPackageMaxNumber = 500;
    [SerializeField] private int mediumPackageMaxNumber = 1000;
    [SerializeField, Min(20)] private int maxPackagesInGame = 80;
    [SerializeField, MinMaxRange(2,9)] private RangedInt packageNumbersRange = new RangedInt(2, 9);
    
    
    [Header("Order Settings")]
    [SerializeField, MinMaxRange(1,10)] private RangedFloat timeBetweenOrders = new RangedFloat(5f, 10f);
    [SerializedDictionary, SerializeField] private SerializedDictionary<int , float> easyOrderCombinations = new SerializedDictionary<int, float>
    {
        {4, 15},
        {9, 20},
        {16, 20},
        {25, 20},
        {36, 20},
        {49, 20},
        {64, 20},
        {81, 20},
        {8, 15},
        {27, 20},
        {125, 30},
        {216, 30},
        {343, 30},
        {512, 30},
    };
    
    [Header("Interaction Settings")]
    [SerializeField, Range(0,10)] private float outlineWidth = 5f;
    [SerializeField] private Color outlineColor = Color.cornflowerBlue;
    [SerializeField] private Outline.Mode outlineMode = Outline.Mode.OutlineVisible;
    
    [Header("Resources")]
    [SerializeField] private NumberdPackage smallPackagePrefab;
    [SerializeField] private NumberdPackage mediumPackagePrefab;
    [SerializeField] private NumberdPackage largePackagePrefab;
    
    
    public int MinPackageNumber => packageNumbersRange.minValue;
    public int MaxPackageNumber => packageNumbersRange.maxValue;
    public int MaxPackagesInGame => maxPackagesInGame;
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

    public int GetRandomPackageNumber()
    {
        return packageNumbersRange.RandomValue;
    }
    

    public (int key, float value) GetEasyOrderCombination()
    {
        var keys = easyOrderCombinations.Keys.ToList();
        int randomIndex = Random.Range(0, keys.Count);
        int key = keys[randomIndex];
        float value = easyOrderCombinations[key];
        return (key, value);
    }
    
    public float GetRandomTimeBetweenOrders()
    {
        return timeBetweenOrders.RandomValue;
    }

}
