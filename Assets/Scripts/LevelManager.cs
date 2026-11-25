using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    private int enemyCount;

    void Start()
    {
        FindAllEnemies();
    }

    void FindAllEnemies()
    {
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        enemyCount = 0;

        foreach (GameObject obj in allObjects)
        {
            if (obj.layer == LayerMask.NameToLayer("Enemy"))
            {
                if (obj.transform.parent == null || (obj.transform.parent.GetComponent<SimpleEnemy>() == null &&
                    obj.transform.parent.GetComponent<SkeletonArcher>() == null &&
                    obj.transform.parent.GetComponent<Lancer>() == null))
                {
                    if (obj.GetComponent<SimpleEnemy>() != null ||
                        obj.GetComponent<SkeletonArcher>() != null ||
                        obj.GetComponent<Lancer>() != null)
                    {
                        enemyCount++;
                    }
                }
            }
        }

    }

    public void EnemyDied()
    {
        enemyCount--;

        if (enemyCount <= 0)
        {
            LoadNextLevel();
        }
    }

    void LoadNextLevel()
    {
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.levelWinClip, false);
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.saveData.currentLevelIndex = nextSceneIndex;

            if (!SaveManager.Instance.saveData.unlockedLevels.Contains(nextSceneIndex))
            {
                SaveManager.Instance.saveData.unlockedLevels.Add(nextSceneIndex);
            }

            SaveManager.Instance.SaveGame();
        }

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }
}