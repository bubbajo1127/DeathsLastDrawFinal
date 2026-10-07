using UnityEngine;

public class PlayerDoorInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private DungeonManager dungeonManager;

    private RoomDoor currentDoor;

    private void Update()
    {
        HandleDoorInteraction();
    }

    public void SetDoorTarget(RoomDoor door)
    {
        if (door == null)
            return;

        if (!door.IsUnlocked)
            return;

        currentDoor = door;

        movement.MoveToTarget(
            currentDoor.InteractionPoint,
            currentDoor.InteractionRange
        );
    }

    private void HandleDoorInteraction()
    {
        if (currentDoor == null)
            return;

        if (!currentDoor.IsUnlocked)
        {
            CancelDoorInteraction();
            return;
        }

        float distance = Vector3.Distance(
            transform.position,
            currentDoor.InteractionPoint.position
        );

        Debug.Log("HandleDoorInteraction");
        if (distance <= currentDoor.InteractionRange)
        {
            Debug.Log("Enter Door should be called");
            EnterDoor();
        }
    }

    private void EnterDoor()
    {
        if (currentDoor == null)
            return;

        movement.StopMovement();

        RoomDoor selectedDoor = currentDoor;

        // Clear this before the room gets destroyed.
        currentDoor = null;

        dungeonManager.ChooseDoor(selectedDoor);
    }

    public void CancelDoorInteraction()
    {
        currentDoor = null;
    }
}
