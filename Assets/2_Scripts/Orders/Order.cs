
using System.Collections.Generic;

public class Order
{
    public int targetNumber;
    public float timeLeft;
    public int worth;
    
    
    public Order(SOGameSettings gameSettings, Difficulty difficulty)
    {
        Dictionary<PowerMachine, int> availablePowerMachines = GameManager.Instance.PowerMachines;
        var orderCombinations = gameSettings.GetOrderNumber(availablePowerMachines, difficulty);
        
        worth = gameSettings.GetOrderWorth(difficulty);
        targetNumber = orderCombinations.key;
        timeLeft = orderCombinations.value;

    }
    
    public bool IsOrderCompleted(NumberdPackage package)
    {
        return package.Number == targetNumber && timeLeft > 0;
    }

}