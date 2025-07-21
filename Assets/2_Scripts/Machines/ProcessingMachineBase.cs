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
    [Header("Base Machine Settings")] 
    [SerializeField, Min(0.5f)] protected float processDuration = 3f;
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
    
    protected int ProcessedOutputNum;
    protected bool IsProcessing;
    protected float ProcessTime;
    protected Coroutine ProcessCoroutine;
    protected Coroutine ScaleAnimationCoroutine;
    protected Sequence ScaleReturnSequence;
    protected Vector3 OriginalScale;
    protected Vector3 TargetScale;
    protected Sequence ProcessBarSequence;
    protected float ProcessingBarFullWidth;
    
    public event Action<NumberdPackage> OnPackageProcessed;
    public event Action<NumberdPackage> OnPackageSpawned;
    public event Action OnProcessingStarted;
    public event Action OnProcessingFinished;
    public event Action OnBeforeSpawnPackage;
    public event Action OnAfterSpawnPackage;
    
    protected virtual void Awake()
    {
        OriginalScale = machineGfx.localScale;
        TargetScale = OriginalScale;
        processingBarCanvasGroup.alpha = 0f;
        ProcessingBarFullWidth = processingBar.rectTransform.sizeDelta.x;
    }
    
    protected virtual void Update()
    {
        CheckForPackages();
    }

    protected virtual void CheckForPackages()
    {
        if (IsProcessing) return;
        
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
                    StartProcessingPackage(package);
                    break;
                }
            }
        }
    }
    
    
    protected virtual void StartProcessingPackage(NumberdPackage package)
    {
        if (IsProcessing) return;

        processedPackageGfx.gameObject.SetActive(true);
        ProcessedOutputNum = CalculateOutput(package);
        
        if (ProcessCoroutine != null) StopCoroutine(ProcessCoroutine);
        ProcessCoroutine = StartCoroutine(Process());
        
        OnPackageProcessed?.Invoke(package);
        package.IntoTheAbyss(packageCheckPosition.transform.position.RemoveY(0.5f));
    }
    
    protected virtual IEnumerator Process()
    {
        OnProcessingStarted?.Invoke();
        
        processingSfx?.Play(audioSource);
        ProcessTime = 0;
        IsProcessing = true;
        
        if (ScaleReturnSequence.isAlive) ScaleReturnSequence.Stop();
        if (ScaleAnimationCoroutine != null) StopCoroutine(ScaleAnimationCoroutine);
        if (ProcessBarSequence.isAlive) ProcessBarSequence.Stop();
        
        ScaleAnimationCoroutine = StartCoroutine(ScaleAnimation());
        ProcessBarSequence = Sequence.Create()
            .Group(Tween.Alpha(processingBarCanvasGroup, 1f, scaleTransitionDuration, Ease.InOutSine))
            .Group(Tween.Custom(
               startValue: 0, 
               endValue: ProcessingBarFullWidth, 
               duration: processDuration, 
               onValueChange: value => processingBar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value),
               Ease.InOutSine));
    
        while (ProcessTime < processDuration)
        {
            ProcessTime += Time.deltaTime;
            yield return null;
        }
    
        FinishProcessing();
    }
    
    protected virtual IEnumerator ScaleAnimation()
    {
        while (IsProcessing)
        {
            float waitTime = UnityEngine.Random.Range(timeBetweenScales.minValue, timeBetweenScales.maxValue);
            yield return new WaitForSeconds(waitTime);
        
            if (!IsProcessing) break;
            
            if (ScaleReturnSequence.isAlive) ScaleReturnSequence.Stop();
            
            float horizontalScale = UnityEngine.Random.Range(scaleRange.minValue, scaleRange.maxValue);
            float verticalScale = 1f / horizontalScale;
        
            TargetScale = new Vector3(
                OriginalScale.x * horizontalScale,
                OriginalScale.y * verticalScale, 
                OriginalScale.z * horizontalScale
            );

            ScaleReturnSequence = Sequence.Create()
                .Group(Tween.Scale(machineGfx, TargetScale, scaleTransitionDuration, Ease.OutElastic));

            while (IsProcessing && ScaleReturnSequence.isAlive)
            {
                yield return null;
            }
        }
    }

    protected virtual void FinishProcessing()
    {
        if (ScaleAnimationCoroutine != null) StopCoroutine(ScaleAnimationCoroutine);
        
        if (ProcessBarSequence.isAlive) ProcessBarSequence.Stop();
        ProcessBarSequence = Sequence.Create()
            .Group(Tween.Alpha(processingBarCanvasGroup, 0f, scaleReturnDuration + 0.5f, Ease.InOutSine));
        
        if (ScaleReturnSequence.isAlive) ScaleReturnSequence.Stop();
        ScaleReturnSequence = Sequence.Create()
            .ChainCallback(() => { machineGfx.localScale = OriginalScale; })
            .Group(Tween.PunchScale(machineGfx, Vector3.one * 0.2f, scaleReturnDuration, 1))
            .ChainCallback(() =>
            {
                IsProcessing = false;
                ProcessTime = 0f;
                processingSfx?.Stop(audioSource);
                finishedProcessingSfx?.Play(audioSource);
                processedPackageGfx.gameObject.SetActive(false);

                OnProcessingFinished?.Invoke();
                SpawnOutputPackage();
            })
            .Group(Tween.Scale(machineGfx, OriginalScale, scaleReturnDuration, Ease.OutElastic));
    }
    
    protected virtual void SpawnOutputPackage()
    {
        OnBeforeSpawnPackage?.Invoke();
        
        var packagePrefab = gameSettings.GetPackagePrefabByNumber(ProcessedOutputNum);
        var newPackage = Instantiate(packagePrefab, outputPosition.position, Quaternion.identity);
        newPackage.SetNumber(ProcessedOutputNum);
        newPackage.Push(outputPosition.forward, 5f);
        OnPackageSpawned?.Invoke(newPackage);
        
        OnAfterSpawnPackage?.Invoke();
    }
    
    protected abstract bool CanProcessPackage(NumberdPackage package);
    protected abstract int CalculateOutput(NumberdPackage package);

    protected virtual void OnDrawGizmos()
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