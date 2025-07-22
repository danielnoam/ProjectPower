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

    [SerializeField] private bool allowMultipleNumbers = true;
    [SerializeField, MinMaxRange(1,4)] private RangedInt multipleOrderRange = new RangedInt(2, 3);
    
    
    
    public int OrderReward => orderReward;
    public bool AllowMultipleNumbers => allowMultipleNumbers;
    public RangedInt MultipleOrderRange => multipleOrderRange;
    
    
    public (int key, float value) GetOrder(Dictionary<PowerMachine, int> availablePowerMachines, Difficulty difficulty)
    {
        
        var validCombinations = orderCombinations.ToDictionary(pair => pair.Key, pair => pair.Value).Where(kvp => CanAchieveWithAvailableMachines(kvp.Key, availablePowerMachines)).ToList();
        if (validCombinations.Count == 0)
        {
            var keys = orderCombinations.ToDictionary(pair => pair.Key, pair => pair.Value).Keys.ToList();
            int randomIndex = Random.Range(0, keys.Count);
            int key = keys[randomIndex];
            float value = orderCombinations.ToDictionary(pair => pair.Key, pair => pair.Value)[key];
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
