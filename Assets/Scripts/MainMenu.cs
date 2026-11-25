using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    [Header("Audio Settings UI")]
    public Slider musicSlider;
    public Slider sfxSlider;

    public GameObject levelSelectPanel;

    public Button level2Button;

    void Start()
    {
        if (levelSelectPanel == null)
        {
            levelSelectPanel = GameObject.Find("LevelSelectPanel");
        }

        if (levelSelectPanel != null)
        {
            levelSelectPanel.SetActive(false);
        }

        if (SaveManager.Instance != null && SaveManager.Instance.saveData != null)
        {
            if (musicSlider != null)
            {
                musicSlider.SetValueWithoutNotify(SaveManager.Instance.saveData.musicVolume);
                musicSlider.onValueChanged.AddListener(val => AudioManager.Instance?.SetMusicVolume(val));
            }

            if (sfxSlider != null)
            {
                sfxSlider.SetValueWithoutNotify(SaveManager.Instance.saveData.sfxVolume);
                sfxSlider.onValueChanged.AddListener(val => AudioManager.Instance?.SetSFXVolume(val));
            }
        }

        UpdateLevelButtons();
    }

    public void PlayGame()
    {
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.uiPlayClip, false);
        int levelToLoad = 1;
        if (SaveManager.Instance != null && SaveManager.Instance.saveData != null)
        {
            levelToLoad = SaveManager.Instance.saveData.currentLevelIndex;
            if (levelToLoad >= SceneManager.sceneCountInBuildSettings) levelToLoad = 1;
        }
        SceneManager.LoadScene(levelToLoad);
    }

    public void OpenLevelSelectMenu()
    {
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.uiClickClip, false);
        if (levelSelectPanel != null)
        {
            UpdateLevelButtons();
            levelSelectPanel.SetActive(true);
        }
    }

    public void CloseLevelSelectMenu()
    {
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.uiBackClip, false);
        if (levelSelectPanel != null)
        {
            levelSelectPanel.SetActive(false);
        }
    }

    public void UpdateLevelButtons()
    {
        if (level2Button != null && SaveManager.Instance != null)
        {
            bool isLevel2Unlocked = SaveManager.Instance.saveData.unlockedLevels.Contains(2);
            level2Button.interactable = isLevel2Unlocked;
        }
    }

    public void LoadLevel1()
    {
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.uiPlayClip, false);
        SceneManager.LoadScene(1);
    }

    public void LoadLevel2()
    {
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.uiPlayClip, false);
        SceneManager.LoadScene(2);
    }

    public void ResetProgress()
    {
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.uiClickClip, false);
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.saveData = new SaveData();
            SaveManager.Instance.SaveGame();
        }

        UpdateLevelButtons();
    }
}