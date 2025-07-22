
using System;
using PrimeTween;
using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float uiToggleDuration = 1f;
    [SerializeField] private float uiToggleStartDelay = 2f;
    
    [Header("References")]
    [SerializeField] private SOGameSettings gameSettings;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private CanvasGroup uiCanvasGroup;
    [SerializeField] private CanvasGroup currencyCanvasGroup;
    [SerializeField] private CanvasGroup ordersCanvasGroup;
    [SerializeField] private TextMeshProUGUI currencyText;
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI ordersCompletedText;
    [SerializeField] private TextMeshProUGUI ordersFailedText;

    private Sequence _uiSequence;
    private Sequence _currencySequence;
    private Sequence _ordersSequence;
    

    private void Awake()
    {
        UpdateCurrency(0);
        UpdateOrdersCompleted(0);
        UpdateOrdersFailed(0);
        ToggleUI(false,false);
        ToggleCurrencyUI(false, false);
        ToggleOrdersUI(false, false);
    }

    private void Start()
    {
        ToggleUI(true, true, uiToggleStartDelay);
        ToggleCurrencyUI(true, false);
    }


    private void OnEnable()
    {
        if (GameManager.Instance)
        {
            GameManager.Instance.OnDayStarted += OnDayStarted;
            GameManager.Instance.OnDayFinished += OnDayFinished;
            GameManager.Instance.OnCurrencyChanged += UpdateCurrency;
            GameManager.Instance.OnOrderCompleted += UpdateOrdersCompleted;
            GameManager.Instance.OnOrderFailed += UpdateOrdersFailed;
        }
    }
    
    private void OnDisable()
    {
        if (GameManager.Instance)
        {
            GameManager.Instance.OnDayStarted -= OnDayStarted;
            GameManager.Instance.OnDayFinished -= OnDayFinished;
            GameManager.Instance.OnCurrencyChanged -= UpdateCurrency;
            GameManager.Instance.OnOrderCompleted -= UpdateOrdersCompleted;
            GameManager.Instance.OnOrderFailed -= UpdateOrdersFailed;
        }
    }
    
    private void OnDayStarted(int day)
    {
        UpdateDay(day);
        UpdateCurrency(0);
        UpdateOrdersCompleted(0);
        UpdateOrdersFailed(0);
        ToggleCurrencyUI(true);
        ToggleOrdersUI(true);
    }
    
    private void OnDayFinished(int day)
    {
        ToggleCurrencyUI(true);
        ToggleOrdersUI(false);
    }
    

    private void ToggleUI( bool isVisible, bool animate = true, float delay = 0f)
    {
        if (!uiCanvasGroup) return;

        if (_uiSequence.isAlive) _uiSequence.Stop();
        
        if (!animate)
        {
            uiCanvasGroup.alpha = isVisible ? 1 : 0;
        }
        else
        {
            _uiSequence = Sequence.Create()
                .Group(Tween.Alpha(uiCanvasGroup , isVisible ? 1 : 0, uiToggleDuration, startDelay: delay));
        }
    }
    
    private void ToggleCurrencyUI(bool isVisible, bool animate = true)
    {
        if (!currencyCanvasGroup) return;

        if (_currencySequence.isAlive) _currencySequence.Stop();
        
        if (!animate)
        {
            currencyCanvasGroup.alpha = isVisible ? 1 : 0;
        }
        else
        {
            _currencySequence = Sequence.Create()
                .Group(Tween.Alpha(currencyCanvasGroup , isVisible ? 1 : 0, uiToggleDuration));
        }
    }
    
    private void ToggleOrdersUI(bool isVisible, bool animate = true)
    {
        if (!ordersCanvasGroup) return;

        if (_ordersSequence.isAlive) _ordersSequence.Stop();
        
        if (!animate)
        {
            ordersCanvasGroup.alpha = isVisible ? 1 : 0;
        }
        else
        {
            _ordersSequence = Sequence.Create()
                .Group(Tween.Alpha(ordersCanvasGroup , isVisible ? 1 : 0, uiToggleDuration));
        }
    }
    
    private void UpdateCurrency(int amount)
    {
        if (!currencyText) return;

        currencyText.text = $"{amount}$";
        
    }
    
    private void UpdateOrdersCompleted(int amount)
    {
        if (!ordersCompletedText) return;

        ordersCompletedText.text = $"{amount}/{gameSettings.OrdersNeededToCompleteDay}";
    }
    
    private void UpdateOrdersFailed(int amount)
    {
        if (!ordersFailedText) return;

        ordersFailedText.text = $"{amount}/{gameSettings.OrderFailuresToFailDay}";
    }

    private void UpdateDay(int day)
    {
        if (!dayText) return;

        dayText.text = $"Day {day}";
    }
}
