using System;
using System.Collections;
using System.Collections.Generic;
using DNExtensions;
using UnityEngine;
using UnityEngine.Rendering;

public class PowerMachineNumberIndicator : MonoBehaviour
{
    [Header("Indicator Settings")]
    [SerializeField] private SOGameSettings gameSettings;
    [SerializeField] private Transform[] numbers;
    public void SetNumber(int machinePower)
    {
        if (numbers == null || numbers.Length == 0 || numbers.Length < gameSettings.PackageNumbersRange.maxValue) return;

        for (int i = 0; i < numbers.Length; i++)
        {
            numbers[i].gameObject.SetActive(i + 1 == machinePower);
        }
    }
    
    [Button]
    private void FindAllNumbers()
    {
        numbers = Array.Empty<Transform>();
        var childNumbers = new List<Transform>();
        foreach (Transform child in transform)
        {
            if (child.parent == transform)
            {
                childNumbers.Add(child);
            }

        }
        if (childNumbers.Count > 0)
        {
            numbers = childNumbers.ToArray();
        }
    }
    
    [Button]
    private void ReverseNumbers()
    {
        if (numbers == null || numbers.Length == 0) return;

        for (int i = 0; i < numbers.Length / 2; i++)
        {
            (numbers[i], numbers[numbers.Length - 1 - i]) = (numbers[numbers.Length - 1 - i], numbers[i]);
        }
    }
}
