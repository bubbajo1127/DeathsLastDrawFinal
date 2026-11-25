using System;
using UnityEngine;

[Serializable]
public class EnemySpawnEntry
{
    [SerializeField] private EnemyManager enemyPrefab;
    [SerializeField] private int count = 1;

    public EnemyManager EnemyPrefab => enemyPrefab;
    public int Count => count;
}

[CreateAssetMenu(fileName = "EnemyWaveData", menuName = "Death's Last Draw/Rooms/Enemy Wave")]
public class EnemyWaveData : ScriptableObject
{
    [SerializeField] private EnemySpawnEntry[] enemies;

    public EnemySpawnEntry[] Enemies => enemies;
}