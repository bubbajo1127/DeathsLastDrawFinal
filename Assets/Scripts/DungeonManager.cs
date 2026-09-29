using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class DungeonManager : MonoBehaviour
{
    [Header("Rooms")]
    [SerializeField] private RoomController[] roomPrefabs;

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private NavMeshAgent playerAgent;

    private RoomController currentRoom;

    private void Start()
    {
        LoadRandomRoom();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.nKey.wasPressedThisFrame)
        {
            LoadRandomRoom();
        }
    }

    public void LoadRandomRoom()
    {
        // Destroy previous room.
        if (currentRoom != null)
        {
            Destroy(currentRoom.gameObject);
        }

        // Pick random room.
        int randomIndex = Random.Range(0, roomPrefabs.Length);

        // Spawn the room at the origin.
        currentRoom = Instantiate(
            roomPrefabs[randomIndex],
            Vector3.zero,
            Quaternion.identity
        );

        MovePlayerToRoom();
    }

    private void MovePlayerToRoom()
    {
        Transform spawnPoint = currentRoom.PlayerSpawnPoint;

        if (spawnPoint == null)
            return;

        playerAgent.Warp(spawnPoint.position);
    }
}
