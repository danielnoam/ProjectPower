using System;
using System.Collections.Generic;
using UnityEngine;

public class PowerMachine : ProcessingMachineBase
{
    [Header("Power Machine Settings")] 
    [SerializeField, Range(2,9)] private int power = 2;
    
    [Header("Power Machine References")]
    [SerializeField] private PowerMachineNumberIndicator[] powerNumbers;
    
    public int Power => power;
    
    

    private void OnValidate()
    {
        if (powerNumbers == null || powerNumbers.Length == 0)
        {
            powerNumbers = GetComponentsInChildren<PowerMachineNumberIndicator>();
        }
        foreach (var indicator in powerNumbers)
        {
            if (indicator)
            {
                indicator.SetNumber(power);
            }
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
        if (upgrade is SOPowerMachineUpgrade powerMachineUpgrade)
        {
            LowerProcessDurationBy(powerMachineUpgrade.processDurationReduction);
        }
    }

    private void OnUpgradesSetup(List<SOUpgrade> upgrades)
    {
        foreach (var upgrade in upgrades)
        {
            if (upgrade is SOPowerMachineUpgrade powerMachineUpgrade)
            {
                LowerProcessDurationBy(powerMachineUpgrade.processDurationReduction);
            }
        }
    }
    

    protected override bool CanProcessPackage(NumberdPackage package)
    {
        return true;
    }

    public override int CalculateOutput(NumberdPackage package)
    {
        return PowerInt(package.Number, power);
    }
    
    public bool CanProduceNumber(int targetNumber)
    {
        if (targetNumber is < 1 or 1) return false;
    
        // Find the integer root by testing values
        int root = (int)Math.Round(Math.Pow(targetNumber, 1.0 / power));
    
        // Test a small range around the calculated root to handle precision issues
        for (int testRoot = Math.Max(1, root - 1); testRoot <= root + 1; testRoot++)
        {
            if (PowerInt(testRoot, power) == targetNumber)
                return true;
        }
    
        return false;
    }
    
    private int PowerInt(int baseValue, int exponent)
    {
        int result = 1;
        for (int i = 0; i < exponent; i++)
        {
            result *= baseValue;
        }
        return result;
    }
    
    

}