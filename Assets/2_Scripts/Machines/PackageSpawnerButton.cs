


using System;
using PrimeTween;
using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class PackageSpawnerButton : MonoBehaviour
{
    [Header("Button Settings")]
    [SerializeField] private int packagesToSpawn = 3;
    [SerializeField] private float spawnDelay = 0.5f;
    
    [Header("Button Animation")]
    [SerializeField] private float buttonPressDuration = 0.2f;
    [SerializeField] private Vector3 positionOffset = new Vector3(0, -0.1f, 0);
    
    [Header("References")]
    [SerializeField] private Interactable interactable;
    [SerializeField] private PackageSpawner packageSpawner;
    [SerializeField] private Transform buttonGfx;


    private Vector3 _originalButtonPosition;
    private Sequence _buttonPressSequence;
    
    private void OnValidate()
    {
        if (!packageSpawner) GetComponentInParent<PackageSpawner>();
    }

    private void Awake()
    {
        _originalButtonPosition = buttonGfx.localPosition;
    }

    private void OnEnable()
    {
        interactable.OnInteract += OnInteract;
    }
    
    private void OnDisable()
    {
        interactable.OnInteract -= OnInteract;
    }

    private void OnInteract(PlayerInteraction interaction)
    {
        SpawnPackages();
    }

    private void SpawnPackages()
    {
        packageSpawner?.SpawnPackagesBatch(packagesToSpawn, spawnDelay);
        interactable.SetCanInteract(false);
        
        if (_buttonPressSequence.isAlive) _buttonPressSequence.Stop();
        _buttonPressSequence = Sequence.Create()
            .Group(Tween.LocalPosition(buttonGfx, startValue: buttonGfx.localPosition, endValue: buttonGfx.localPosition + positionOffset, duration: buttonPressDuration, Ease.InOutSine))
            .ChainDelay(0.2f)
            .Group(Tween.LocalPosition(buttonGfx, startValue: buttonGfx.localPosition + positionOffset, endValue: _originalButtonPosition, duration: buttonPressDuration, Ease.InOutSine))
            .ChainCallback(() => { interactable.SetCanInteract(true); });
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + positionOffset, 0.1f);
    }
}