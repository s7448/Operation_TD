using System.Collections.Generic;

[System.Serializable]
public class TowerSaveData
{
    public string prefabName;
    public float posX, posY, posZ;
    public int[] upgradeLevels;
    public int targetingMode;
}

[System.Serializable]
public class AbilitySaveData
{
    public bool mineUnlocked;
    public bool hedgehogUnlocked;
    public bool mortarUnlocked;
    public bool radarUnlocked;

    public int mineUpgradeLevel;
    public int hedgehogUpgradeLevel;
    public int mortarUpgradeLevel;

    public int mineCooldown;
    public int hedgehogCooldown;
    public int mortarCooldown;
    public int radarCooldown;
}

[System.Serializable]
public class GameSaveData
{
    public string sceneName;
    public int lastCompletedWave;
    public int currentMoney;
    public int currentLives;
    public int enemiesDefeated;
    public float timeElapsed;
    public List<TowerSaveData> towers = new List<TowerSaveData>();
    public AbilitySaveData abilities = new AbilitySaveData();
}