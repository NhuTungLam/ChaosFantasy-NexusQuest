using UnityEngine;
using System.Collections.Generic;
using DungeonSystem;

public class RoomStateRestorer : MonoBehaviour
{
    public void RestoreFromJson(string json)
    {
        RoomState state = JsonUtility.FromJson<RoomState>(json);
        if (state == null)
        {
            Debug.LogError("[Restore] Failed to parse RoomState");
            return;
        }

        RestoreEnemies(state.enemies);
        RestorePlayers(state.players);

        Debug.Log("[Restore] Room restored with " + state.enemies.Count + " enemies and " + state.players.Count + " players.");
    }

    void RestoreEnemies(List<EnemyState> enemies)
    {
        foreach (var e in enemies)
        {
            GameObject obj = Photon.Pun.PhotonNetwork.InstantiateRoomObject(
                e.type, // prefab name
                e.position.ToVector3(),
                Quaternion.identity
            );

            if (obj.TryGetComponent(out EnemyHandler handler))
            {
                handler.currentHealth = e.hp;
                // Optional: call Init(enemyData) if needed
            }
        }
    }

    void RestorePlayers(List<DungeonPlayerState> players)
    {
        foreach (var p in players)
        {
            Debug.Log($"[Restore] Found player: {p.userId} at ({p.position.x}, {p.position.y})");
            // Optionally re-spawn ghost/dummy/player preview
        }
    }
}
