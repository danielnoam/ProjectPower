using System;
using DNExtensions;
using PrimeTween;
using TMPro;
using UnityEngine;

[SelectionBase]
[DisallowMultipleComponent]
public class NumberdPackage : PickableObject
{
    [Header("Package Settings")]
    [SerializeField] private int number = 2;
    
    [Header("References")]
    [SerializeField] private SOGameSettings gameSettings;
    [SerializeField] private Light packageLight;
    [SerializeField] private TextMeshProUGUI[] numberTexts;

    private Sequence _scaleSequence;
    private Sequence _lightSequence;
    
    public int Number => number;

    private void OnValidate()
    {

        if (gameSettings)
        {
            number = Mathf.Clamp(number, gameSettings.PackageNumbersRange.minValue, 9999999);
        }
        
        if (numberTexts == null || numberTexts.Length == 0)
        {
            numberTexts = GetComponentsInChildren<TextMeshProUGUI>();
        }
        UpdatePackageVisuals();
    }

    private void Awake()
    {
        packageLight.enabled = false;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        
        if (GameManager.Instance)
        {
            GameManager.Instance.OnDayFinished += OnDayFinished;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (GameManager.Instance)
        {
            GameManager.Instance.OnDayFinished -= OnDayFinished;
        }
    }
    
    private void OnDayFinished(SODayData dayData)
    {
        IntoTheAbyss();
    }
    
    
    

    private void UpdatePackageVisuals()
    {
        foreach (var text in numberTexts)
        {
            if (text)
            {
                text.text = number.ToString();
            }
        }
    }
    
    public void ToggleLight(bool state)
    {
        if (packageLight.enabled == state) return;
        
        if (_lightSequence.isAlive) _lightSequence.Stop();

        if (state)
        {
            _lightSequence = Sequence.Create()
                .ChainCallback(() => packageLight.enabled = true)
                .Group(Tween.LightIntensity(packageLight, 1f, 0.5f, Ease.InOutSine));
        }
        else
        {
            _lightSequence = Sequence.Create()
                .Group(Tween.LightIntensity(packageLight, 0f, 0.5f, Ease.InOutSine))
                .OnComplete(() => packageLight.enabled = false);
        }

    }

    public void IntoTheAbyss(Vector3 abyssPosition = default)
    {
        if (_scaleSequence.isAlive) _scaleSequence.Stop();
        _scaleSequence = Sequence.Create()
            .Group(Tween.Scale(transform, Vector3.one, Vector3.one * 0.25f, 0.5f, Ease.InOutSine));
        
        if (abyssPosition != default)
        {
            rigidBody.useGravity = false;
            rigidBody.isKinematic = false;
            _scaleSequence.Group(Tween.Position(rigidBody.transform, abyssPosition, 0.5f, Ease.InOutSine));
        }
        _scaleSequence.OnComplete(() => Destroy(gameObject));

    }
    
    public void SetNumber(int number)
    {
        this.number = Mathf.Clamp(number, gameSettings.PackageNumbersRange.minValue, 9999999);
        UpdatePackageVisuals();
    }
    
    

    public void Push(Vector3 direction, float force)
    {
        if (!rigidBody) return;
        rigidBody.AddForce(direction * force, ForceMode.Impulse);
        if (_scaleSequence.isAlive) _scaleSequence.Stop();
        _scaleSequence = Sequence.Create()
            .Group(Tween.Scale(transform, Vector3.one * 0.5f, Vector3.one, 0.5f, Ease.OutElastic));
    }
}
