using System.Collections.Generic;
using UnityEngine;
using DungeonSystem;

public class RoomStateBuilder
{
    public static RoomState BuildRoomState()
    {
        RoomState state = new RoomState();

        state.currentRoomId = Photon.Pun.PhotonNetwork.CurrentRoom.Name;
        state.clearedRooms = new List<string>();
        state.currentWave = 1;
        state.bossDefeated = false;

        // Collect all active enemies in the scene
        state.enemies = new List<EnemyState>();
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject e in enemies)
        {
            EnemyHandler handler = e.GetComponent<EnemyHandler>();
            if (handler != null && handler.enemyData != null)
            {
                Position pos = new Position
                {
                    x = e.transform.position.x,
                    y = e.transform.position.y
                };

                EnemyState enemyState = new EnemyState
                {
                    id = handler.enemyData.name,
                    type = handler.enemyData.e_name,
                    hp = handler.currentHealth,
                    position = pos
                };
                state.enemies.Add(enemyState);
            }
        }

        // Collect all player states in the scene
        state.players = new List<DungeonPlayerState>();
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in players)
        {
            CharacterHandler ch = p.GetComponent<CharacterHandler>();
            if (ch != null)
            {
                string className = ch.profile != null ? ch.profile.@class : "Unknown";


                Position playerPos = new Position
                {
                    x = p.transform.position.x,
                    y = p.transform.position.y
                };

                DungeonPlayerState ps = new DungeonPlayerState
                {
                    userId = Photon.Pun.PhotonView.Get(p).Owner.UserId,
                    @class = className,
                    hp = ch.currentHealth,
                    mana = ch.currentMana,
                    position = playerPos
                };
                state.players.Add(ps);
            }
        }

        return state;
    }
}
