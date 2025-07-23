using UnityEngine;


[CreateAssetMenu(fileName = "New Recycle Machine Upgrade", menuName = "Upgrades/Recycle Machine")]
public class SORecycleMachineUpgrade : SOUpgrade
{
    [Header("Machine Upgrade")]
    public int recycleDurationReduction = 1;
}
