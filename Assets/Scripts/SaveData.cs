using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public int version = 1;
    public int currentLevelIndex = 1;

    public List<int> unlockedLevels = new List<int> { 1 };
}