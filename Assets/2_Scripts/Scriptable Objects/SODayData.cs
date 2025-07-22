using DNExtensions;
using UnityEngine;

[CreateAssetMenu(fileName = "DayData", menuName = "Scriptable Objects/New Day Data", order = 3)]
public class SODayData : ScriptableObject
{
    [Header("Day Settings")]
    [SerializeField, Min(1)] private int ordersNeededToCompleteDay = 30;
    [SerializeField, Min(1)] private int orderFailuresToFailDay = 15;
    [SerializeField, MinMaxRange(1,10)] private RangedFloat timeBetweenOrders = new RangedFloat(3f, 7f);
    
    [Header("Difficulty")]
    [SerializeField, Min(1)] private int ordersNeededToChangeDifficulty = 7;
    [SerializeField] private SOOrderCombinations easyOrderCombinations;
    [SerializeField] private SOOrderCombinations mediumOrderCombinations;
    [SerializeField] private SOOrderCombinations hardOrderCombinations;


    public RangedFloat TimeBetweenOrders => timeBetweenOrders;
    public int OrdersNeededToChangeDifficulty => ordersNeededToChangeDifficulty;
    public int OrdersNeededToCompleteDay => ordersNeededToCompleteDay;
    public int OrderFailuresToFailDay => orderFailuresToFailDay;

    private void OnValidate()
    {
        if (ordersNeededToChangeDifficulty > ordersNeededToCompleteDay)
        {
            ordersNeededToChangeDifficulty = ordersNeededToCompleteDay;
        }
    }

    public SOOrderCombinations GetOrderCombinations(Difficulty difficulty)
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

        return orderCombinations;
    }
    


    public int GetOrderWorth(Difficulty difficulty)
    {
        switch (difficulty)
        {
            case Difficulty.Easy:
                return easyOrderCombinations.OrderReward;
            case Difficulty.Medium:
                return mediumOrderCombinations.OrderReward;
            case Difficulty.Hard:
                return hardOrderCombinations.OrderReward;
            default:
                return easyOrderCombinations.OrderReward;
        }
    }

}
