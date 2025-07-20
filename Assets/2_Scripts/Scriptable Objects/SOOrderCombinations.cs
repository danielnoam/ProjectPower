using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using DNExtensions;
using UnityEngine;

[CreateAssetMenu(fileName = "Order Combinations", menuName = "Scriptable Objects/New Order Combinations", order = 2)]
public class SOOrderCombinations : ScriptableObject
{

    [SerializedDictionary, SerializeField] private SerializedDictionary<int , float> orderCombinations = new SerializedDictionary<int, float>
    {
        {4, 20},
        {9, 20},
        {16, 20},
        {25, 20},
        {36, 20},
        {49, 20},
        {8, 20},
        {27, 20},
    };

    
    public Dictionary<int, float> GetCombinations()
    {
        return orderCombinations.ToDictionary(pair => pair.Key, pair => pair.Value);
    }

}
