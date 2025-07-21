using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using DNExtensions;
using UnityEngine;

[CreateAssetMenu(fileName = "Order Combinations", menuName = "Scriptable Objects/New Order Combinations", order = 2)]
public class SOOrderCombinations : ScriptableObject
{

    [SerializeField,Min(1)] private int orderReward = 10;
    [Tooltip("Dictionary of order combinations where key is the number of packages and value is the time in seconds to complete the order.")]
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

    public int OrderReward => orderReward;
    
    public Dictionary<int, float> GetCombinations()
    {
        return orderCombinations.ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    public (int, float) GeRandomOrder()
    {
        if (orderCombinations.Count == 0) return (0, 0);
        
        var randomIndex = Random.Range(0, orderCombinations.Count);
        var randomPair = orderCombinations.ElementAt(randomIndex);
        return (randomPair.Key, randomPair.Value);
    }



}
