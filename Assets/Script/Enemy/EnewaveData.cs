using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class EnewaveData : ScriptableObject
{
    public List<EnemyData> enemylist = new List<EnemyData>();
    public List<int> enemycount = new List<int>();
}
