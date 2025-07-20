using System;
using System.Collections;
using System.Collections.Generic;
using DNExtensions;
using PrimeTween;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[SelectionBase]
[RequireComponent(typeof(AudioSource))]
public class PowerMachine : MonoBehaviour
{
    [Header("Power Machine Settings")] 
    [SerializeField, Range(2,9)] private int machinePower = 2;
    [SerializeField, Min(0.5f)] private float machineDuration = 5f;
    [SerializeField, Min(0.1f)] private float packageCheckRadius = 5f;
    [SerializeField] private LayerMask layerMask;

    [Header("Processing Animation")]
    [SerializeField, MinMaxRange(1,2)] private RangedFloat scaleRange = new RangedFloat(1, 1.5f);
    [SerializeField, MinMaxRange(0.1f,0.5f)] private RangedFloat timeBetweenScales = new RangedFloat(0.1f, 0.5f);
    [SerializeField] private float scaleTransitionDuration = 0.75f;
    [SerializeField] private float scaleReturnDuration = 0.5f;
    
    [Header("Processing Bar")]

    
    [Header("References")]
    [SerializeField] private SOGameSettings gameSettings;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Transform machineGfx;
    [SerializeField] private Transform packageCheckPosition;
    [SerializeField] private Transform outputPosition;
    [SerializeField] private Transform processedPackageGfx;
    [SerializeField] private Image processingBar;
    [SerializeField] private CanvasGroup processingBarCanvasGroup;
    [SerializeField] private SOAudioEvent processingSfx;
    [SerializeField] private SOAudioEvent finishedProcessingSfx;
    [SerializeField] private PowerMachineNumberIndicator[] powerNumbers;
    
    private int _processedOutputNum;
    private bool _isProcessing;
    private float _processTime;
    private Coroutine _processCoroutine;
    private Coroutine _scaleAnimationCoroutine;
    private Sequence _scaleReturnSequence;
    private Vector3 _originalScale;
    private Vector3 _targetScale;
    private Sequence _processBarSequence;
    private float _processingBarFullWidth;



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
                indicator.SetNumber(machinePower);
            }
        }
    }
    
    
    private void Awake()
    {
        _originalScale = machineGfx.localScale;
        _targetScale = _originalScale;
        processingBarCanvasGroup.alpha = 0f;
        _processingBarFullWidth = processingBar.rectTransform.sizeDelta.x;
    }
    
    private void Update()
    {
        CheckForPackages();
    }


    private void CheckForPackages()
    {
        if (_isProcessing) return;
        
    
        Collider[] colliders = Physics.OverlapSphere(
            packageCheckPosition.position, 
            packageCheckRadius, 
            layerMask
        );

        foreach (Collider col in colliders)
        {
            if (col.TryGetComponent(out NumberdPackage package))
            {
                StartProcessingPackage(package);
                break;
            }
        }
    }
    
    private void StartProcessingPackage(NumberdPackage package)
    {
        if (_isProcessing) return;

        processedPackageGfx.gameObject.SetActive(true);
        _processedOutputNum = PowerInt(package.PackageNumber, machinePower);
        if (_processCoroutine != null) StopCoroutine(_processCoroutine);
        _processCoroutine = StartCoroutine(Process());
        package.StartedProcessing();
    }
    
    private IEnumerator Process()
    {
        processingSfx?.Play(audioSource);
        _processTime = 0;
        _isProcessing = true;
        if (_scaleReturnSequence.isAlive) _scaleReturnSequence.Stop();
        if (_scaleAnimationCoroutine != null) StopCoroutine(_scaleAnimationCoroutine);
        if (_processBarSequence.isAlive) _processBarSequence.Stop();
        _scaleAnimationCoroutine = StartCoroutine(ScaleAnimation());
        _processBarSequence = Sequence.Create()
            .Group(Tween.Alpha(processingBarCanvasGroup, 1f, scaleTransitionDuration, Ease.InOutSine))
            .Group(Tween.Custom(
               startValue: 0, 
               endValue: _processingBarFullWidth, 
               duration: machineDuration, 
               onValueChange: value => processingBar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value) ,
               Ease.InOutSine));
    
        while (_processTime < machineDuration)
        {
            _processTime += Time.deltaTime;
            yield return null;
        }
    
        FinishProcessing();
    }
    
    private IEnumerator ScaleAnimation()
    {
        while (_isProcessing)
        {
            float waitTime = UnityEngine.Random.Range(timeBetweenScales.minValue, timeBetweenScales.maxValue);
            yield return new WaitForSeconds(waitTime);
        
            if (!_isProcessing) break;
            
            if (_scaleReturnSequence.isAlive) _scaleReturnSequence.Stop();
            
            float horizontalScale = UnityEngine.Random.Range(scaleRange.minValue, scaleRange.maxValue);
            float verticalScale = 1f / horizontalScale;
        
            _targetScale = new Vector3(
                _originalScale.x * horizontalScale,
                _originalScale.y * verticalScale, 
                _originalScale.z * horizontalScale
            );

            _scaleReturnSequence = Sequence.Create()
                .Group(Tween.Scale(machineGfx, _targetScale, scaleTransitionDuration, Ease.OutElastic));
        

            while (_isProcessing && _scaleReturnSequence.isAlive)
            {
                yield return null;
            }
        }
    }

    
    

    private void FinishProcessing()
    {
        if (_scaleAnimationCoroutine != null) StopCoroutine(_scaleAnimationCoroutine);
        if (_scaleReturnSequence.isAlive) _scaleReturnSequence.Stop();
        _scaleReturnSequence = Sequence.Create()
            .ChainCallback(() => { machineGfx.localScale = _originalScale; })
            .Group(Tween.PunchScale(machineGfx, Vector3.one * 0.2f, scaleReturnDuration, 1))
            .ChainCallback(() =>
            {
                _isProcessing = false;
                _processTime = 0f;
                finishedProcessingSfx?.Play(audioSource);
                processedPackageGfx.gameObject.SetActive(false);

                var packagePrefab = gameSettings.GetPackagePrefabByNumber(_processedOutputNum);
                var newPackage = Instantiate(packagePrefab, outputPosition.position, Quaternion.identity);
                newPackage.SetNumber(_processedOutputNum);
                newPackage.Push(outputPosition.forward, 5f);
            })
            .Group(Tween.Scale(machineGfx, _originalScale, scaleReturnDuration, Ease.OutElastic));
        
        if (_processBarSequence.isAlive) _processBarSequence.Stop();
        _processBarSequence = Sequence.Create()
            .Group(Tween.Alpha(processingBarCanvasGroup, 0f, scaleReturnDuration +0.5f, Ease.InOutSine));
        
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

    private void OnDrawGizmos()
    {
        if (packageCheckPosition)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(packageCheckPosition.position, packageCheckRadius);
        }

        if (outputPosition)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(outputPosition.position, 0.1f);

            
            #if UNITY_EDITOR
            Handles.color = Color.green;
            Handles.ArrowHandleCap(0, outputPosition.position, Quaternion.LookRotation(outputPosition.forward), 0.5f, EventType.Repaint);
            #endif
        }

    }
}