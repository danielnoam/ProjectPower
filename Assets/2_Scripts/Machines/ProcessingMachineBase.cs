using System;
using System.Collections;
using DNExtensions;
using PrimeTween;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[SelectionBase]
[RequireComponent(typeof(AudioSource))]
public abstract class ProcessingMachineBase : MonoBehaviour
{
    [SerializeField] protected int machineID;
    
    [Header("Base Machine Settings")] 
    [SerializeField, Min(0.5f)] protected float baseProcessDuration = 3f;
    [SerializeField, Min(0.1f)] private float packageCheckRadius = 0.55f;
    [SerializeField] protected LayerMask packageLayerMask;

    [Header("Processing Animation")]
    [SerializeField, MinMaxRange(1,2)] protected RangedFloat scaleRange = new RangedFloat(1, 1.5f);
    [SerializeField, MinMaxRange(0.01f,0.5f)] protected RangedFloat timeBetweenScales = new RangedFloat(0.1f, 0.16f);
    [SerializeField] protected float scaleTransitionDuration = 0.75f;
    [SerializeField] protected float scaleReturnDuration = 0.5f;
    
    [Header("Base References")]
    [SerializeField] protected SOGameSettings gameSettings;
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected Transform machineGfx;
    [SerializeField] protected Transform processedPackageGfx;
    [SerializeField] protected Transform packageCheckPosition;
    [SerializeField] protected Transform outputPosition;
    [SerializeField] protected Image processingBar;
    [SerializeField] protected CanvasGroup processingBarCanvasGroup;
    [SerializeField] protected SOAudioEvent processingSfx;
    [SerializeField] protected SOAudioEvent finishedProcessingSfx;
    
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
    protected float ProcessingDuration;
    public event Action<NumberdPackage> OnPackageProcessed;
    public event Action<NumberdPackage> OnPackageSpawned;
    public event Action OnProcessingStarted;
    public event Action OnProcessingFinished;
    public event Action OnBeforeSpawnPackage;
    public event Action OnAfterSpawnPackage;
    
    protected virtual void Awake()
    {
        ProcessingDuration = baseProcessDuration;
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
            packageLayerMask
        );

        foreach (Collider col in colliders)
        {
            if (col.TryGetComponent(out NumberdPackage package))
            {
                if (CanProcessPackage(package))
                {
                    StartProcessingPackage(package, ProcessingDuration);
                    break;
                }
            }
        }
    }
    
    
    private void StartProcessingPackage(NumberdPackage package, float processingDuration)
    {
        if (_isProcessing) return;

        processedPackageGfx.gameObject.SetActive(true);
        _processedOutputNum = CalculateOutput(package);
        
        if (_processCoroutine != null) StopCoroutine(_processCoroutine);
        _processCoroutine = StartCoroutine(Process(processingDuration));
        
        OnPackageProcessed?.Invoke(package);
        package.IntoTheAbyss(packageCheckPosition.transform.position.RemoveY(0.5f));
    }
    
    private IEnumerator Process(float processingDuration)
    {
        OnProcessingStarted?.Invoke();
        
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
               duration: processingDuration, 
               onValueChange: value => processingBar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value),
               Ease.InOutSine));
    
        while (_processTime < processingDuration)
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
        
        if (_processBarSequence.isAlive) _processBarSequence.Stop();
        _processBarSequence = Sequence.Create()
            .Group(Tween.Alpha(processingBarCanvasGroup, 0f, scaleReturnDuration + 0.5f, Ease.InOutSine));
        
        if (_scaleReturnSequence.isAlive) _scaleReturnSequence.Stop();
        _scaleReturnSequence = Sequence.Create()
            .ChainCallback(() => { machineGfx.localScale = _originalScale; })
            .Group(Tween.PunchScale(machineGfx, Vector3.one * 0.2f, scaleReturnDuration, 1))
            .ChainCallback(() =>
            {
                _isProcessing = false;
                _processTime = 0f;
                processingSfx?.Stop(audioSource);
                finishedProcessingSfx?.Play(audioSource);
                processedPackageGfx.gameObject.SetActive(false);

                OnProcessingFinished?.Invoke();
                SpawnOutputPackage();
            })
            .Group(Tween.Scale(machineGfx, _originalScale, scaleReturnDuration, Ease.OutElastic));
    }
    
    private void SpawnOutputPackage()
    {
        OnBeforeSpawnPackage?.Invoke();
        
        var packagePrefab = gameSettings.GetPackagePrefabByNumber(_processedOutputNum);
        var newPackage = Instantiate(packagePrefab, outputPosition.position, Quaternion.identity);
        newPackage.SetNumber(_processedOutputNum);
        newPackage.Push(outputPosition.forward, 5f);
        OnPackageSpawned?.Invoke(newPackage);
        
        OnAfterSpawnPackage?.Invoke();
    }
    
    protected abstract bool CanProcessPackage(NumberdPackage package);
    public abstract int CalculateOutput(NumberdPackage package);

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