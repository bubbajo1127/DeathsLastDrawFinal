using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomEnemySpawner : MonoBehaviour
{
    [Header("Waves")]
    [SerializeField] private EnemyWaveData[] waves;
    [SerializeField] private float initialSpawnDelay = 2f;
    [SerializeField] private float betweenWaveDelay = 1f;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Doors")]
    [SerializeField] private RoomDoor[] doors;

    private readonly List<EnemyStats> activeEnemies = new List<EnemyStats>();

    private RunManager runManager;
    private bool started;
    private int roomNumber;

    public int CurrentWave { get; private set; }
    public bool IsRoomCleared { get; private set; }
    public int TotalWaves => waves.Length;

    public bool TryBegin(RunManager manager)
    {
        if (started || manager == null)
            return false;

        SetDoorsUnlocked(false);

        if (!ValidateConfiguration(manager.CurrentRoomType))
            return false;

        runManager = manager;
        roomNumber = manager.CurrentRoomNumber;

        started = true;
        IsRoomCleared = false;
        CurrentWave = 0;

        StartCoroutine(WaveRoutine());

        return true;
    }

    private IEnumerator WaveRoutine()
    {
        yield return new WaitForSeconds(initialSpawnDelay);

        for (int i = 0; i < waves.Length; i++)
        {
            if (!IsCurrentRoomActive())
                yield break;

            CurrentWave = i + 1;

            SpawnWave(waves[i]);

            Debug.Log($"Wave {CurrentWave}/{TotalWaves} started with {activeEnemies.Count} enemies.");

            while (HasLivingEnemies())
            {
                if (!IsCurrentRoomActive())
                    yield break;

                yield return null;
            }

            Debug.Log($"Wave {CurrentWave}/{TotalWaves} cleared.");

            if (i < waves.Length - 1)
                yield return new WaitForSeconds(betweenWaveDelay);
        }

        if (IsCurrentRoomActive())
            CompleteRoom();
    }

    private void SpawnWave(EnemyWaveData wave)
    {
        activeEnemies.Clear();

        List<Transform> availableSpawnPoints = new List<Transform>(spawnPoints);

        foreach (EnemySpawnEntry entry in wave.Enemies)
        {
            for (int i = 0; i < entry.Count; i++)
            {
                int index = Random.Range(0, availableSpawnPoints.Count);

                Transform spawnPoint = availableSpawnPoints[index];
                availableSpawnPoints.RemoveAt(index);

                EnemyManager enemy = Instantiate(entry.EnemyPrefab, spawnPoint.position, spawnPoint.rotation, transform);

                EnemyStats stats = enemy.GetComponent<EnemyStats>();

                activeEnemies.Add(stats);
            }
        }
    }

    private bool HasLivingEnemies()
    {
        foreach (EnemyStats enemy in activeEnemies)
        {
            if (enemy != null && !enemy.IsDead())
                return true;
        }

        return false;
    }

    private void CompleteRoom()
    {
        if (IsRoomCleared)
            return;

        IsRoomCleared = true;

        Debug.Log("All enemy waves cleared.");

        runManager.MarkRoomCleared();
    }

    private void SetDoorsUnlocked(bool unlocked)
    {
        if (doors == null)
            return;

        foreach (RoomDoor door in doors)
        {
            if (door != null)
                door.SetUnlocked(unlocked);
        }
    }

    private bool IsCurrentRoomActive()
    {
        return runManager != null &&
               runManager.State == RunState.InRoom &&
               runManager.CurrentRoomNumber == roomNumber;
    }

    public void UnlockDoorsAfterReward()
    {
        if (!IsRoomCleared)
            return;

        if (runManager == null || runManager.State != RunState.RoomCleared)
            return;

        SetDoorsUnlocked(true);
    }

    private bool ValidateConfiguration(RunRoomType roomType)
    {
        int requiredWaves = roomType == RunRoomType.Starting ? 1 : 2;

        if (roomType == RunRoomType.Boss)
            return ConfigurationError("Boss rooms should not use standard enemy waves.");

        if (waves == null || waves.Length != requiredWaves)
            return ConfigurationError($"Room requires exactly {requiredWaves} wave(s).");

        if (spawnPoints == null || spawnPoints.Length == 0)
            return ConfigurationError("No enemy spawn points assigned.");

        foreach (Transform spawnPoint in spawnPoints)
        {
            if (spawnPoint == null)
                return ConfigurationError("An enemy spawn point is missing.");
        }

        if (doors == null || doors.Length == 0)
            return ConfigurationError("No room doors assigned.");

        foreach (RoomDoor door in doors)
        {
            if (door == null)
                return ConfigurationError("A room door reference is missing.");
        }

        foreach (EnemyWaveData wave in waves)
        {
            if (wave == null || wave.Enemies == null)
                return ConfigurationError("Enemy wave is missing.");

            int enemyCount = 0;

            foreach (EnemySpawnEntry entry in wave.Enemies)
            {
                if (entry == null || entry.EnemyPrefab == null || entry.Count <= 0)
                    return ConfigurationError("Invalid enemy spawn entry.");

                if (entry.EnemyPrefab.GetComponent<EnemyStats>() == null)
                    return ConfigurationError("Enemy prefab is missing EnemyStats.");

                enemyCount += entry.Count;
            }

            if (enemyCount == 0 || enemyCount > spawnPoints.Length)
                return ConfigurationError("Wave requires more spawn points or has no enemies.");
        }

        return true;
    }

    private bool ConfigurationError(string message)
    {
        Debug.LogError($"{gameObject.name}: {message}", this);
        return false;
    }
}