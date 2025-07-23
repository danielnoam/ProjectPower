using UnityEngine;


[CreateAssetMenu(fileName = "New Power Machine Upgrade", menuName = "Upgrades/Power Machine")]
public class SOPowerMachineUpgrade : SOUpgrade
{
    [Header("Machine Upgrade")]
    public float processDurationReduction = 1f;
}
