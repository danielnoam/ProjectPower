
using System;
using System.Collections.Generic;
using System.Linq;
using DNExtensions;
using PrimeTween;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "Game Settings", menuName = "Scriptable Objects/New Game Settings", order = 1)]
public class SOGameSettings : ScriptableObject
{
    [Header("Package Settings")]
    [SerializeField] private int smallPackageMaxNumber = 255;
    [SerializeField] private int mediumPackageMaxNumber = 1023;
    [SerializeField, Min(20)] private int maxPackagesInGame = 20;
    [SerializeField, MinMaxRange(2,9)] private RangedInt packageNumbersRange = new RangedInt(2, 9);
    
    [Header("Interaction Settings")]
    [SerializeField, Range(0,10)] private float outlineWidth = 5f;
    [SerializeField] private Color outlineColor = Color.cornflowerBlue;
    [SerializeField] private Outline.Mode outlineMode = Outline.Mode.OutlineVisible;
    
    [Header("Resources")]
    [SerializeField] private NumberdPackage smallPackagePrefab;
    [SerializeField] private NumberdPackage mediumPackagePrefab;
    [SerializeField] private NumberdPackage largePackagePrefab;
    [SerializeField] private GameObject[] clientsPrefabs;
    [SerializeField] private SODayData[] dayData = Array.Empty<SODayData>();
    
    
    public int MaxPackagesInGame => maxPackagesInGame;

    public RangedInt PackageNumbersRange => packageNumbersRange;

    public float OutlineWidth => outlineWidth;
    public Color OutlineColor => outlineColor;
    public Outline.Mode OutlineMode => outlineMode;

    

    public NumberdPackage GetPackagePrefabByNumber(int number)
    {
        if (number <= smallPackageMaxNumber)
        {
            return smallPackagePrefab;
        }
        
        if (number <= mediumPackageMaxNumber)
        {
            return mediumPackagePrefab;
        }
         
         
        return largePackagePrefab;
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

    public SODayData GetDayData(int dayNumber)
    {
        dayNumber -= 1;
        
        if (dayNumber < 0)
        {
            Debug.LogError("Invalid day index");
            return null;
        }

        if (dayNumber >= dayData.Length)
        {
            return dayData.LastOrDefault();
        }

        return dayData[dayNumber];
        
    }


}
