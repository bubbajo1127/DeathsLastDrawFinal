using UnityEngine;
using UnityEngine.AI;

public class DungeonManager : MonoBehaviour
{
    [Header("Rooms")]
    [SerializeField] private RoomController[] roomPrefabs;

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private NavMeshAgent playerAgent;

    [Header("Testing")]
    [SerializeField] private bool enableRoomGeneration = true;

    private RoomController currentRoom;

    private void Start()
    {
        if (enableRoomGeneration) //for testing
        {
            LoadRandomRoom();
        }
    }

    public void ChooseDoor(RoomDoor selectedDoor)
    {
        if (selectedDoor == null)
            return;

        Debug.Log("Door chosen!");

        // Eventually:
        // Apply reward from selectedDoor
        // Play door animation
        // Fade to black
        // Load next room
        // Fade back in

        LoadRandomRoom();
    }

    public void LoadRandomRoom()
    {
        if (!enableRoomGeneration) return;  //for testing

        if (currentRoom != null)
        {
            Destroy(currentRoom.gameObject);
        }

        int randomIndex = Random.Range(0, roomPrefabs.Length);

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
