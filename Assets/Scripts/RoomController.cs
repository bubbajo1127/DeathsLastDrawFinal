using UnityEngine;

public class RoomController : MonoBehaviour
{
    [Header("Room References")]
    [SerializeField] private Transform playerSpawnPoint;

    public Transform PlayerSpawnPoint => playerSpawnPoint;
}
