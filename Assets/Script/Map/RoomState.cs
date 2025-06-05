using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonSystem
{
    [Serializable]
    public class RoomState
    {
        public string currentRoomId;
        public List<string> clearedRooms;
        public List<EnemyState> enemies;
        public List<DungeonPlayerState> players;
        public int currentWave;
        public bool bossDefeated;
    }

    [Serializable]
    public class EnemyState
    {
        public string id;
        public string type;
        public Position position;
        public float hp;
    }

    [Serializable]
    public class Position
    {
        public float x;
        public float y;

        public Vector3 ToVector3() => new Vector3(x, y, 0);
    }
}
