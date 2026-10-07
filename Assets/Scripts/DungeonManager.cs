using UnityEngine;
using UnityEngine.AI;

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
