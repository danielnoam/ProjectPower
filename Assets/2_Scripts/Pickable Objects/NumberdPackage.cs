using DNExtensions;
using PrimeTween;
using TMPro;
using UnityEngine;

[SelectionBase]
[DisallowMultipleComponent]
public class NumberdPackage : PickableObject
{
    [Header("Package Settings")]
    [SerializeField, Min(2)] private int packageNumber = 2;
    
    [Header("References")]
    [SerializeField] private TextMeshProUGUI[] numberTexts;

    private Sequence _scaleSequence;
    
    public int PackageNumber => packageNumber;

    private void OnValidate()
    {
        packageNumber = Mathf.Max(2, packageNumber);
        if (numberTexts == null || numberTexts.Length == 0)
        {
            numberTexts = GetComponentsInChildren<TextMeshProUGUI>();
        }
        UpdatePackageVisuals();
    }

    public void StartedProcessing()
    {
        if (_scaleSequence.isAlive) _scaleSequence.Stop();
        _scaleSequence = Sequence.Create()
            .Group(Tween.Scale(transform, Vector3.one, Vector3.one * 0.25f, 0.4f, Ease.InOutSine))
            .ChainCallback((() => Destroy(gameObject)));
    }
    
    public void SetNumber(int number)
    {
        packageNumber = Mathf.Max(2, number);
        UpdatePackageVisuals();

    }

    private void UpdatePackageVisuals()
    {
        foreach (var text in numberTexts)
        {
            if (text)
            {
                text.text = packageNumber.ToString();
            }
        }
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
