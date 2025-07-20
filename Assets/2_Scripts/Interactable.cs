using System;
using DNExtensions;
using PrimeTween;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Outline))]
[RequireComponent(typeof(Collider))]
public class Interactable : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private bool canInteract = true;
    [SerializeField] private SOAudioEvent interactionSfx;
    
    [Header("References")]
    [SerializeField] private SOGameSettings gameSettings;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Outline outline;
    
    private bool _isHighlighted;
    private Sequence _highlightSequence;
    
    public bool CanInteract => canInteract;
    
    public event Action<PlayerInteraction> OnInteract;
    public event Action OnUnHighlight;
    public event Action OnHighlight;


    private void Awake()
    {
        if (gameSettings)
        {
            outline.OutlineColor = gameSettings.OutlineColor;
            outline.OutlineWidth = 0;
            outline.OutlineMode = gameSettings.OutlineMode;
        }
    }
    

    public void Highlight()
    {
        if (_isHighlighted) return;

        _isHighlighted = true;
        
        if (gameSettings)
        {
            if (_highlightSequence.isAlive) _highlightSequence.Stop();
            _highlightSequence = Sequence.Create()
                .Group(Tween.Custom(
                    startValue: outline.OutlineWidth,
                    endValue: gameSettings.OutlineWidth,
                    duration: 0.5f,
                    onValueChange: value => outline.OutlineWidth = value
                    ));
        }
        OnHighlight?.Invoke();
    }
    
    public void UnHighlight()
    {
        if (!_isHighlighted) return;
        
        _isHighlighted = false;
        if (gameSettings)
        {
            if (_highlightSequence.isAlive) _highlightSequence.Stop();
            _highlightSequence = Sequence.Create()
                .Group(Tween.Custom(
                    startValue: outline.OutlineWidth,
                    endValue: 0,
                    duration: 0.5f,
                    onValueChange: value => outline.OutlineWidth = value
                ));
        }
        OnUnHighlight?.Invoke();
    }
    
    public void Interact(PlayerInteraction interactor)
    {
        if (!canInteract) return;
        
        interactionSfx?.Play(audioSource);
        OnInteract?.Invoke(interactor);
    }
    
    public void SetCanInteract(bool value)
    {
        canInteract = value;
    }
}