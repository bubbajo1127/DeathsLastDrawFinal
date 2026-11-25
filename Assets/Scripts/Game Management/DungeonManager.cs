using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class DungeonManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RunManager runManager;

    [Header("Rooms")]
    [SerializeField] private RoomController startingRoomPrefab;
    [SerializeField] private RoomController[] roomPrefabs;
    [SerializeField] private RoomController bossRoomPrefab;

    [Header("Run Settings")]
    [SerializeField] private int roomsBeforeBoss = 4;

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private NavMeshAgent playerAgent;
    [SerializeField] private CameraFollow cameraFollow;

    [Header("Testing")]
    [SerializeField] private bool enableRoomGeneration = true;

    private readonly List<int> availableRoomIndices = new List<int>();

    private RoomController currentRoom;
    private int normalRoomsLoaded;
    private bool bossRoomLoaded;
    private bool startingRoomLoaded;

    public bool RoomGenerationEnabled => enableRoomGeneration;
    public RoomController CurrentRoom => currentRoom;

    public void InitializeRoomPool()
    {
        availableRoomIndices.Clear();

        if (roomPrefabs != null)
        {
            for (int i = 0; i < roomPrefabs.Length; i++)
                availableRoomIndices.Add(i);
        }

        startingRoomLoaded = false;
        normalRoomsLoaded = 0;
        bossRoomLoaded = false;

        Debug.Log($"Room pool initialized with {availableRoomIndices.Count} rooms.");
    }

    public void ChooseDoor(RoomDoor selectedDoor)
    {
        if (selectedDoor == null)
            return;

        runManager.TryAdvanceRoom();
    }

    public bool TryLoadNextRoom(out RunRoomType roomType)
    {
        roomType = RunRoomType.Normal;

        if (!enableRoomGeneration || bossRoomLoaded)
            return false;

        if (!startingRoomLoaded)
        {
            roomType = RunRoomType.Starting;
            return LoadStartingRoom();
        }

        if (normalRoomsLoaded >= roomsBeforeBoss)
        {
            roomType = RunRoomType.Boss;
            return LoadBossRoom();
        }

        if (availableRoomIndices.Count == 0)
        {
            Debug.LogWarning("No unused normal rooms remain. Loading boss room.");

            roomType = RunRoomType.Boss;
            return LoadBossRoom();
        }

        return LoadRandomRoom();
    }
    private bool LoadStartingRoom()
    {
        if (!LoadRoom(startingRoomPrefab))
            return false;

        startingRoomLoaded = true;

        Debug.Log("Starting room loaded.");

        return true;
    }

    private bool LoadRandomRoom()
    {
        int poolIndex = Random.Range(0, availableRoomIndices.Count);
        int roomIndex = availableRoomIndices[poolIndex];

        if (!LoadRoom(roomPrefabs[roomIndex]))
            return false;

        availableRoomIndices.RemoveAt(poolIndex);
        normalRoomsLoaded++;

        Debug.Log($"Normal rooms loaded: {normalRoomsLoaded}/{roomsBeforeBoss}");
        Debug.Log($"Unused rooms remaining: {availableRoomIndices.Count}");

        return true;
    }

    private bool LoadBossRoom()
    {
        if (bossRoomLoaded)
            return false;

        if (!LoadRoom(bossRoomPrefab))
            return false;

        bossRoomLoaded = true;

        Debug.Log("Boss room loaded.");

        return true;
    }

    private bool LoadRoom(RoomController roomPrefab)
    {
        if (roomPrefab == null)
        {
            Debug.LogError("Room prefab is not assigned.");
            return false;
        }

        if (roomPrefab.PlayerSpawnPoint == null)
        {
            Debug.LogError($"{roomPrefab.name} has no PlayerSpawnPoint.");
            return false;
        }

        if (playerAgent == null)
        {
            Debug.LogError("Player NavMeshAgent is not assigned.");
            return false;
        }

        DestroyCurrentRoom();

        currentRoom = Instantiate(roomPrefab, Vector3.zero, Quaternion.identity);

        Debug.Log($"Loaded room: {currentRoom.name}");

        return MovePlayerToRoom();
    }

    private bool MovePlayerToRoom()
    {
        Transform spawnPoint = currentRoom.PlayerSpawnPoint;

        if (spawnPoint == null)
        {
            Debug.LogError($"{currentRoom.name} has no PlayerSpawnPoint.");
            return false;
        }

        bool warped = playerAgent.Warp(spawnPoint.position);

        if (!warped)
        {
            Debug.LogError($"Failed to warp player to {spawnPoint.position}.");
            return false;
        }

        if (playerAgent.isOnNavMesh)
            playerAgent.ResetPath();

        if (cameraFollow != null)
            cameraFollow.SnapToTarget();

        return true;
    }

    private void DestroyCurrentRoom()
    {
        if (currentRoom == null)
            return;

        Destroy(currentRoom.gameObject);
        currentRoom = null;
    }
}