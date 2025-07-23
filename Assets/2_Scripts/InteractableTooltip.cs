using System;
using DNExtensions;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractableTooltip : MonoBehaviour
{
        [Header("Settings")]
        [SerializeField] private string actionPrompt = "Action";
        [SerializeField, Multiline(4)] private string descriptionPrompt = "Description";
        [SerializeField,Range(0f,1f)] private float maxAlpha = 0.5f;
        [SerializeField] private float animationDuration = 0.5f;
        
        [Header("References")]
        [SerializeField] private SOGameSettings gameSettings;
        [SerializeField] private Interactable interactable;
        [SerializeField] private CanvasGroup tooltipCanvas;
        [SerializeField] private Image canvasBackground;
        [SerializeField] private TextMeshProUGUI actionText;
        [SerializeField] private TextMeshProUGUI descriptionText;


        private Camera _camera;
        private Sequence _tooltipSequence;
        private bool _isVisible = true;
        
        private void OnValidate()
        {
                if (!interactable) interactable = GetComponentInParent<Interactable>();
                if (!tooltipCanvas) tooltipCanvas = GetComponentInChildren<CanvasGroup>();
                if (tooltipCanvas)  tooltipCanvas.alpha = maxAlpha;
                
                actionText.text = actionPrompt;
                descriptionText.text = descriptionPrompt;

                if (gameSettings)
                {
                        canvasBackground.color = gameSettings.OutlineColor;
                }
        }

        private void Awake()
        {
                _camera = Camera.main;
                ToggleTooltip(false,false);
        }

        private void OnEnable()
        {
                interactable.OnHighlight += InteractableOnOnHighlight;
                interactable.OnUnHighlight += InteractableOnOnUnHighlight;
                interactable.OnInteract += InteractableOnOnInteract;
        }

        private void OnDisable()
        {
                interactable.OnHighlight -= InteractableOnOnHighlight;
                interactable.OnUnHighlight -= InteractableOnOnUnHighlight;
                interactable.OnInteract -= InteractableOnOnInteract;
        }

        private void Update()
        {
                if (_isVisible)
                {
                        var direction = _camera.transform.position - transform.position;
        
                        if (direction.sqrMagnitude > 0.001f)
                        {
                                var lookRotation = Quaternion.LookRotation(direction);
                                var eulerAngles = lookRotation.eulerAngles.AddY(180);
                                eulerAngles.x = -eulerAngles.x;
                                eulerAngles.z = 0;
                                transform.rotation = Quaternion.Euler(eulerAngles);
                        }
                }
        }

        private void InteractableOnOnInteract(PlayerInteraction interactor)
        {
                
        }


        private void InteractableOnOnUnHighlight()
        {
                ToggleTooltip(false);
        }

        private void InteractableOnOnHighlight()
        {
                ToggleTooltip(true);
        }

        private void ToggleTooltip(bool isVisible, bool animate = true)
        {
                if (_tooltipSequence.isAlive) _tooltipSequence.Stop();

                if (animate)
                {

                        if (isVisible) _isVisible = true;
                        
                        _tooltipSequence = Sequence.Create()
                                .Group(Tween.Alpha(tooltipCanvas, isVisible ? maxAlpha : 0, animationDuration))
                                .OnComplete((() => { if (!isVisible) _isVisible = false;}));
                }
                else
                {
                        tooltipCanvas.alpha = isVisible ? maxAlpha : 0;
                        _isVisible = isVisible;
                }
        }

        public void SetText(string action, string description, int cost)
        {
                actionText.text = action;
                if (cost <= 0)
                {
                        descriptionText.text = $"{description}";
                }
                else
                {
                        descriptionText.text = $"{description}\n\nCosts = {cost}$";
                }

        }
}