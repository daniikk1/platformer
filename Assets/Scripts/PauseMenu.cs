using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;

    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        AudioManager.Instance?.SetPausedAudio(false);
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.uiBackClip, false);
    }


    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        AudioManager.Instance?.SetPausedAudio(true);
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.uiClickClip, false);
    }


    public void RestartLevel()
    {
        Time.timeScale = 1f;
        AudioManager.Instance?.SetPausedAudio(false);
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.uiClickClip, false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        AudioManager.Instance?.SetPausedAudio(false);
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.uiBackClip, false);
        SceneManager.LoadScene("MainMenu");
    }
}