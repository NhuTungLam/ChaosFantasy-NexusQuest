using System.Collections.Generic;
using UnityEngine;
using static DungeonApiClient;

public class RewardSystem : MonoBehaviour
{
    public static RewardSystem Instance;

    public int baseReward = 20;
    public int goldPerKill = 3;
    public int goldPerRoom = 5;
    public int deathPenalty = 4;

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
    public int CalculateExp(PlayerProgressDTO dto, int roomsCleared)
    {
        int baseExp = 10;
        int killBonus = dto.enemyKills * 2;
        int roomBonus = roomsCleared * 3;
        int deathPenalty = dto.deathCount * 2;

        return Mathf.Max(0, baseExp + killBonus + roomBonus - deathPenalty);
    }

    public int CalculateGold(PlayerProgressDTO dto, int roomsCleared)
    {
        int reward = baseReward
            + (dto.enemyKills * goldPerKill)
            + (roomsCleared * goldPerRoom)
            - (dto.deathCount * deathPenalty);

        return Mathf.Max(0, reward); // không cho < 0
    }

    public Dictionary<PlayerProgressDTO, int> GenerateRewards(List<PlayerProgressDTO> players, int roomsCleared)
    {
        var rewards = new Dictionary<PlayerProgressDTO, int>();

        foreach (var p in players)
        {
            rewards[p] = CalculateGold(p, roomsCleared);
        }

        return rewards;
    }
}
