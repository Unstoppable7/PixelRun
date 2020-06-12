using UnityEngine;

[System.Serializable]
public class PlayerData
{
    #region Variables a persistir

    public int Coins { get; private set; }

    /**
    int gems;
    int highestScore;
    int mostCoins;
    int mostGems;
    int totalRuns;
    int totalDistance;
    int totalCoins;
    int totalGems;
    int bestDailyStreak;
    **/

    #endregion

    public PlayerData(GameManager managerData)
    {
        Coins = managerData.Coins;
    }
}
