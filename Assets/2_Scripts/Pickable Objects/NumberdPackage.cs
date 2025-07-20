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
    [SerializeField, Min(2)] private int number = 2;
    
    [Header("References")]
    [SerializeField] private Light packageLight;
    [SerializeField] private TextMeshProUGUI[] numberTexts;

    private Sequence _scaleSequence;
    private Sequence _lightSequence;
    
    public int Number => number;

    private void OnValidate()
    {
        number = Mathf.Max(2, number);
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
                .ChainCallback(() => packageLight.enabled = false);
        }

    }

    public void IntoTheAbyss()
    {
        if (_scaleSequence.isAlive) _scaleSequence.Stop();
        _scaleSequence = Sequence.Create()
            .Group(Tween.Scale(transform, Vector3.one, Vector3.one * 0.25f, 0.4f, Ease.InOutSine))
            .ChainCallback((() => Destroy(gameObject)));
    }
    
    public void SetNumber(int number)
    {
        this.number = Mathf.Max(2, number);
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
