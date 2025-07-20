using System;
using System.Collections.Generic;
using DNExtensions;
using UnityEngine;

public class DeliveryArea : MonoBehaviour
{
    
    [Header("Delivery Area")]
    [SerializeField, ReadOnly] private List<NumberdPackage> packagesInArea = new List<NumberdPackage>();
    
    public event Action<NumberdPackage> OnPackageEnteredArea; 
    

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out NumberdPackage package))
        {
            AddPackage(package);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out NumberdPackage package))
        {
            RemovePackage(package);
        }
    }
    
    
    private void AddPackage(NumberdPackage package)
    {
        if (package && !packagesInArea.Contains(package))
        {
            packagesInArea.Add(package);
            OnPackageEnteredArea?.Invoke(package);
        }
    }

    public void RemovePackage(NumberdPackage package)
    {
        if (package && packagesInArea.Contains(package))
        {
            packagesInArea.Remove(package);
        }
    }
    
}
