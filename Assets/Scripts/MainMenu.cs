using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
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

        UpdateLevelButtons();
    }

    public void PlayGame()
    {
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
        if (levelSelectPanel != null)
        {
            UpdateLevelButtons();
            levelSelectPanel.SetActive(true);
        }
    }

    public void CloseLevelSelectMenu()
    {
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
        SceneManager.LoadScene(1);
    }

    public void LoadLevel2()
    {
        SceneManager.LoadScene(2);
    }

    public void ResetProgress()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.saveData = new SaveData();
            SaveManager.Instance.SaveGame();
        }

        UpdateLevelButtons();
    }
}