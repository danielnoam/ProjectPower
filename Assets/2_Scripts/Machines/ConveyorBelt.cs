using UnityEngine;

public class ConveyorBelt : MonoBehaviour

{
    [Header("Conveyor Settings")]
    public float beltSpeed = 2f;
    
    // Direction vectors based on rotation
    private Vector3[] directions = {
        Vector3.forward,  // 0 degrees (North)
        Vector3.right,    // 90 degrees (East)  
        Vector3.back,     // 180 degrees (South)
        Vector3.left      // 270 degrees (West)
    };
    
    
    private void OnDrawGizmos()
    {

    }
}