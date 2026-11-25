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

        if (movement.IsTargetInRange(currentDoor.InteractionPoint, currentDoor.InteractionRange))
        {
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
