
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuOptions : MonoBehaviour
{


    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1;
    }

    public void QuitGame()
    {
        
#if UNITY_EDITOR
        if (Application.isEditor)
        {
            if (Application.isPlaying)
            {
                UnityEditor.EditorApplication.isPlaying = false;
                return;
            }
        }
#endif
        Application.Quit();
    }
}
