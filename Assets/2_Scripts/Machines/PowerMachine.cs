using System;
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

    protected override bool CanProcessPackage(NumberdPackage package)
    {
        return true;
    }

    protected override int CalculateOutput(NumberdPackage package)
    {
        return PowerInt(package.Number, power);
    }
    
    public bool CanProduceNumber(int targetNumber)
    {
        double root = Math.Pow(targetNumber, 1.0 / power);
        int intRoot = Mathf.RoundToInt((float)root);
        
        return Mathf.Approximately(Mathf.Pow(intRoot, power), targetNumber);
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