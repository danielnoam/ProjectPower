using System;
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
    }

    private void OnEnable()
    {
        
        if (upgradable)
        {
            upgradable.OnUpgradesSetup += OnUpgradesSetup;
            upgradable.OnUpgradeBought += OnUpgradeBought;
        }
    }

    private void OnDisable()
    {
        if (upgradable)
        {
            upgradable.OnUpgradesSetup -= OnUpgradesSetup;
            upgradable.OnUpgradeBought -= OnUpgradeBought;
        }
    }

    private void OnUpgradeBought(SOUpgrade upgrade)
    {
        if (upgrade is SORecycleMachineUpgrade recycleUpgrade)
        {
            LowerProcessDurationBy(recycleUpgrade.recycleDurationReduction);
        }
    }

    private void OnUpgradesSetup(List<SOUpgrade> upgrades)
    {
        foreach (var upgrade in upgrades)
        {
            if (upgrade is SORecycleMachineUpgrade recycleUpgrade)
            {
                LowerProcessDurationBy(recycleUpgrade.recycleDurationReduction);
            }
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