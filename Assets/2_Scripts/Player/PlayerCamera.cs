
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(PlayerMovement))]
public class PlayerCamera : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] [Range(0,0.1f)] private float lookSensitivity = 0.04f;
    [SerializeField] [Range(0,0.1f)] private float lookSmoothing;
    [SerializeField] private Vector2 verticalAxisRange = new Vector2(-90, 90);
    [SerializeField] private bool invertHorizontal = false;
    [SerializeField] private bool invertVertical = false;
    
    [Header("FOV")]
    [SerializeField] private float baseFov = 60f;
    [SerializeField] private float runFovMultiplier = 1.3f;
    [SerializeField] private float fovChangeSmoothing = 5;
    
    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Transform playerHead;
    [SerializeField] private CinemachineCamera cam;
    

    private float _currentPanAngle;
    private float _currentTiltAngle;
    private float _targetPanAngle;
    private float _targetTiltAngle;
    private Vector2 _rotationVelocity;
    
    
    private void OnValidate()
    {
        if (!cam) return;
        cam.Lens.FieldOfView = baseFov;
    }
    
    
    private void Awake()
    {
        _currentPanAngle = transform.eulerAngles.y;
        _currentTiltAngle = playerHead.localEulerAngles.x;
        _targetPanAngle = _currentPanAngle;
        _targetTiltAngle = _currentTiltAngle;
    }
    
    private void OnEnable()
    {
        playerInput.OnLookAction += OnLook;
    }
    
    private void OnDisable()
    {
        playerInput.OnLookAction -= OnLook;
    }
    
    private void Update()
    {
        UpdateFov();
        UpdateHeadRotation();
    }
    
    private void OnLook(InputAction.CallbackContext context)
    {
        if (!playerHead) return;
        
        Vector2 lookDelta = context.ReadValue<Vector2>();


        float horizontalInput = invertHorizontal ? -lookDelta.x : lookDelta.x;
        float verticalInput = invertVertical ? lookDelta.y : -lookDelta.y;
        
        _targetPanAngle += horizontalInput * lookSensitivity;
        _targetTiltAngle += verticalInput * lookSensitivity;
        _targetTiltAngle = Mathf.Clamp(_targetTiltAngle, verticalAxisRange.x, verticalAxisRange.y);

        if (lookSmoothing <= 0) 
        {
            _currentPanAngle = _targetPanAngle;
            _currentTiltAngle = _targetTiltAngle;
        }
    }
    
    private void UpdateHeadRotation()   
    {
        if (!playerHead) return;

        if (lookSmoothing > 0)
        {
            _currentPanAngle = Mathf.SmoothDampAngle(_currentPanAngle, _targetPanAngle, ref _rotationVelocity.x, lookSmoothing);
            _currentTiltAngle = Mathf.SmoothDamp(_currentTiltAngle, _targetTiltAngle, ref _rotationVelocity.y, lookSmoothing);
        }
        
        transform.rotation = Quaternion.Euler(0, _currentPanAngle, 0);
        playerHead.localRotation = Quaternion.Euler(_currentTiltAngle, 0, 0);
    }
    
    private void UpdateFov()
    {
        if (!cam) return;
        
        float targetFov = baseFov;
        if (playerMovement.isRunning)
        {
            targetFov *= runFovMultiplier;
        }
        
        cam.Lens.FieldOfView = Mathf.Lerp(cam.Lens.FieldOfView, targetFov, Time.deltaTime * fovChangeSmoothing);
    }
    
    
    public Vector3 GetMovementDirection()
    {
        Vector3 direction = Quaternion.Euler(0, _currentPanAngle, 0) * Vector3.forward;
        return direction.normalized;
    }

    public Vector3 GetAimDirection()
    {
        return Quaternion.Euler(_currentTiltAngle, _currentPanAngle, 0) * Vector3.forward;
    }
    
}