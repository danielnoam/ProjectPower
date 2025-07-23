using PrimeTween;
using UnityEngine;

[DisallowMultipleComponent]
[SelectionBase]
[RequireComponent(typeof(Interactable))]
public class StartDayLever : MonoBehaviour
{

    [Header("Pull Animation")]
    [SerializeField, Min(0.1f)] private float pullDuration = 1f;
    [SerializeField] private Vector3 pullRotation = new Vector3(0, 0f, -60f);
    [SerializeField] private Ease pullEase = Ease.OutBack;
    [SerializeField, Min(0.1f)] private float releaseDuration = 1.5f;

    [Header("References")] 
    [SerializeField] private SOGameSettings gameSettings;
    [SerializeField] private Transform leverHandleGfx;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Interactable interactable;

    private bool _isLeverPulled;
    private Vector3 _initialLeverRotation;
    private Sequence _leverSequence;

    private void Awake()
    {
        _initialLeverRotation = leverHandleGfx.localEulerAngles;
        canvasGroup.alpha = 1f;
    }

    private void OnEnable()
    {
        interactable.OnInteract += OnInteract;
        if (GameManager.Instance) GameManager.Instance.OnDayFinished += ReleaseLever;
    }

    private void OnDisable()
    {
        interactable.OnInteract -= OnInteract;
        if (GameManager.Instance) GameManager.Instance.OnDayFinished -= ReleaseLever;
    }

    private void OnInteract(PlayerInteraction interactor)
    {
        if (!_isLeverPulled)
        {
            PullLever();
            interactable.SetCanInteract(false);
        }
    }

    private void PullLever()
    {
        if (_leverSequence.isAlive) _leverSequence.Stop();

        _isLeverPulled = true;
        
        _leverSequence = Sequence.Create()
            .Group(Tween.LocalEulerAngles(leverHandleGfx,leverHandleGfx.localEulerAngles, pullRotation, pullDuration, pullEase))
            .Group(Tween.Alpha(canvasGroup, canvasGroup.alpha, 0f, pullDuration, pullEase))
            .OnComplete(() =>
            {
                GameManager.Instance.StartNewDay();
            });
    }
    
    private void ReleaseLever(SODayData dayData)
    {
        if (_leverSequence.isAlive) _leverSequence.Stop();

        _isLeverPulled = false;
        
        _leverSequence = Sequence.Create()
            .Group(Tween.LocalEulerAngles(leverHandleGfx, leverHandleGfx.localEulerAngles, _initialLeverRotation, releaseDuration, pullEase))
            .OnComplete(() =>
            {
                interactable.SetCanInteract(true);
            });
    }
}
