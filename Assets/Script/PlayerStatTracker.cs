using UnityEngine;

public class PlayerStatTracker : MonoBehaviour
{
    public static PlayerStatTracker Instance;

    public int enemyKillCount = 0;
    public int deathCount = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddKill() => enemyKillCount++;
    public void AddDeath() => deathCount++;

    public void ResetStats()
    {
        enemyKillCount = 0;
        deathCount = 0;
    }
}
