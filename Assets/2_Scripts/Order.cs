
using System;
using UnityEngine;

public class Order
{
    public int order;
    public float timeLeft;
    
    public event Action OnOrderFailedEvent;
    
    public Order(SOGameSettings gameSettings)
    {
        var orderCombinations = gameSettings.GetEasyOrderCombination();
        order = orderCombinations.key;
        timeLeft = orderCombinations.value;;
    }
    
    public void UpdateTimeLeft()
    {
        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0)
        {
            OnOrderFailedEvent?.Invoke();
            timeLeft = 0;
        }
    }
    
    public bool TryCompleteOrder(NumberdPackage package)
    {
        return package.PackageNumber == order && timeLeft > 0;
    }

}