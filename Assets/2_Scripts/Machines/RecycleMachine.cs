using System.Collections.Generic;
using UnityEngine;

public class RecycleMachine : ProcessingMachineBase
{
    [Header("Recycle Machine Settings")]
    [SerializeField, Min(0)] private int recycleOutputNumber = 2;
    


    private void OnValidate()
    {
        if (recycleOutputNumber < gameSettings.PackageNumbersRange.minValue)
        {
            recycleOutputNumber = gameSettings.PackageNumbersRange.minValue;
        }

        if (machineID <= 0)
        {
            machineID = GetInstanceID();
        }
    }


    protected override bool CanProcessPackage(NumberdPackage package)
    {
        return true;
    }

    public override int CalculateOutput(NumberdPackage package)
    {
        return recycleOutputNumber;
    }
    
    


    
    
    



}