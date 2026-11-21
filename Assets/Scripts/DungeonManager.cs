using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class DungeonManager : MonoBehaviour
{
    [Header("Rooms")]
    [SerializeField] private RoomController[] roomPrefabs;
    [SerializeField] private RoomController bossRoomPrefab;

    [Header("Run Settings")]
    [SerializeField] private int roomsBeforeBoss = 4;

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private NavMeshAgent playerAgent;

    [Header("Testing")]
    [SerializeField] private bool enableRoomGeneration = true;

    private readonly List<int> availableRoomIndices = new List<int>();

    private RoomController currentRoom;
    private int normalRoomsLoaded;
    private bool bossRoomLoaded;

    private void Start()
    {
        if (!enableRoomGeneration)
            return;

        InitializeRoomPool();
        LoadNextRoom();
    }

    private void InitializeRoomPool()
    {
        availableRoomIndices.Clear();

        for (int i = 0; i < roomPrefabs.Length; i++)
            availableRoomIndices.Add(i);

        normalRoomsLoaded = 0;
        bossRoomLoaded = false;

        Debug.Log($"Room pool initialized with {availableRoomIndices.Count} rooms.");
    }

    public void ChooseDoor(RoomDoor selectedDoor)
    {
        if (selectedDoor == null)
            return;

        if (bossRoomLoaded)
            return;

        Debug.Log("Door chosen!");

        // Eventually:
        // Apply reward from selectedDoor
        // Play door animation
        // Fade to black
        // Load next room
        // Fade back in

        LoadNextRoom();
    }

    public void LoadNextRoom()
    {
        if (!enableRoomGeneration)
            return;

        if (normalRoomsLoaded >= roomsBeforeBoss)
        {
            LoadBossRoom();
            return;
        }

        LoadRandomRoom();
    }

    private void LoadRandomRoom()
    {
        if (roomPrefabs == null || roomPrefabs.Length == 0)
        {
            Debug.LogError("DungeonManager has no normal room prefabs assigned.");
            return;
        }

        if (availableRoomIndices.Count == 0)
        {
            Debug.LogWarning("No unused normal rooms remain. Loading boss room.");
            LoadBossRoom();
            return;
        }

        DestroyCurrentRoom();

        int poolIndex = Random.Range(0, availableRoomIndices.Count);
        int roomIndex = availableRoomIndices[poolIndex];

        // Remove this room so it cannot appear again during this run.
        availableRoomIndices.RemoveAt(poolIndex);

        currentRoom = Instantiate(roomPrefabs[roomIndex], Vector3.zero, Quaternion.identity);

        if (currentRoom == null)
        {
            Debug.LogError("Failed to instantiate room.");
            return;
        }

        normalRoomsLoaded++;

        Debug.Log($"Loaded room: {currentRoom.name}");
        Debug.Log($"Normal rooms completed: {normalRoomsLoaded}/{roomsBeforeBoss}");
        Debug.Log($"Unused rooms remaining: {availableRoomIndices.Count}");

        ValidateAndMovePlayer();
    }

    private void LoadBossRoom()
    {
        if (bossRoomLoaded)
            return;

        if (bossRoomPrefab == null)
        {
            Debug.LogError("DungeonManager has no boss room prefab assigned.");
            return;
        }

        DestroyCurrentRoom();

        currentRoom = Instantiate(bossRoomPrefab, Vector3.zero, Quaternion.identity);

        if (currentRoom == null)
        {
            Debug.LogError("Failed to instantiate boss room.");
            return;
        }

        bossRoomLoaded = true;

        Debug.Log($"Boss room loaded: {currentRoom.name}");

        ValidateAndMovePlayer();
    }

    private void ValidateAndMovePlayer()
    {
        if (currentRoom.PlayerSpawnPoint == null)
        {
            Debug.LogError($"{currentRoom.name} does not have a PlayerSpawnPoint assigned.");
            return;
        }

        Debug.Log($"Player spawn point: {currentRoom.PlayerSpawnPoint.position}");

        MovePlayerToRoom();
    }

    private void MovePlayerToRoom()
    {
        Transform spawnPoint = currentRoom.PlayerSpawnPoint;

        if (spawnPoint == null)
        {
            Debug.LogError($"{currentRoom.name} has no PlayerSpawnPoint.");
            return;
        }

        bool warped = playerAgent.Warp(spawnPoint.position);

        if (!warped)
        {
            Debug.LogError($"Failed to warp player to spawn point at {spawnPoint.position}.");
            return;
        }

        if (playerAgent.isOnNavMesh)
            playerAgent.ResetPath();

        Debug.Log($"Player successfully warped to {spawnPoint.position}.");
        Debug.Log($"Actual player position: {player.position}");
    }

    private void DestroyCurrentRoom()
    {
        if (currentRoom == null)
            return;

        Destroy(currentRoom.gameObject);
        currentRoom = null;
    }
}