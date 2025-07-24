
using System;
using DNExtensions;
using DNExtensions.VFXManager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuOptions : MonoBehaviour
{

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private SOAudioEvent clickSfx;
    [SerializeField] private SOVFEffectsSequence quitTransition;
    [SerializeField] private Button[] buttons;



    private void Awake()
    {
        foreach (var button in buttons)
        {
            button.onClick.AddListener(() => clickSfx?.Play(audioSource));
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        TransitionManager.TransitionToScene(SceneManager.GetActiveScene().buildIndex, quitTransition);
    }

    public void QuitGame()
    {
        Time.timeScale = 1;
        TransitionManager.TransitionQuit(quitTransition);
    }
}
