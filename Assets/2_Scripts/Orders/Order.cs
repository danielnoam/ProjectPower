
using System.Collections.Generic;

public class Order
{
    public readonly List<int> NumbersNeeded = new List<int>();
    public float TimeLeft = 0;
    public readonly int Worth;
    
    
    public Order(SOGameSettings gameSettings, Difficulty difficulty)
    {

        if (!GameManager.Instance || !gameSettings) return;

        var dayData = GameManager.Instance.CurrentDayData;
        var orderCombinations = dayData.GetOrderCombinations(difficulty);
        Worth = dayData.GetOrderWorth(difficulty);
        

        if (orderCombinations.AllowMultipleNumbers)
        {
            for (int i = 0; i < orderCombinations.MultipleOrderRange.RandomValue; i++)
            {
                var order = orderCombinations.GetOrder(GameManager.Instance.PowerMachines, difficulty);
                NumbersNeeded.Add(order.key);
                TimeLeft += order.value;
            }
            
            Worth = dayData.GetOrderWorth(difficulty) * NumbersNeeded.Count;
        }
        else
        {
            var order = orderCombinations.GetOrder(GameManager.Instance.PowerMachines, difficulty);
            NumbersNeeded.Add(order.key);
            TimeLeft = order.value;
            Worth = dayData.GetOrderWorth(difficulty);
            
        }
        
    }
    
    public bool IsOrderCompleted(List<NumberdPackage> packagesInDeliveryArea, out List<NumberdPackage> usedPackages)
    {
        usedPackages = new List<NumberdPackage>();
    
        if (packagesInDeliveryArea == null || packagesInDeliveryArea.Count == 0) return false;
        
        List<NumberdPackage> availablePackages = new List<NumberdPackage>(packagesInDeliveryArea);
    
        foreach (var neededNumber in NumbersNeeded)
        {
            bool found = false;
            
            for (int i = 0; i < availablePackages.Count; i++)
            {
                if (availablePackages[i].Number == neededNumber)
                {
                    usedPackages.Add(availablePackages[i]);
                    availablePackages.RemoveAt(i);
                    found = true;
                    break;
                }
            }
            
            if (!found)
            {
                usedPackages.Clear();
                return false;
            }
        }
    
        return true;
    }


}