
using System;
using UnityEngine;

public class Order
{
    public int order;
    public float timeLeft;
    
    
    public Order(SOGameSettings gameSettings, Difficulty difficulty)
    {
        (int key, float value) orderCombinations;
        switch (difficulty)
        {
            case Difficulty.Easy:
                 orderCombinations = gameSettings.GetEasyOrderCombination();
                break;
            case Difficulty.Medium:
                orderCombinations = gameSettings.GetHardOrderCombination();
                break;
            case Difficulty.Hard:
                orderCombinations = gameSettings.GetHardOrderCombination();
                break;
            default:
                 orderCombinations = gameSettings.GetEasyOrderCombination();
                break;
        }

        order = orderCombinations.key;
        timeLeft = orderCombinations.value;;
    }
    
    public bool IsOrderCompleted(NumberdPackage package)
    {
        return package.PackageNumber == order && timeLeft > 0;
    }

}