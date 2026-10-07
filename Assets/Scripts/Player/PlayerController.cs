using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputManager inputManager;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerAttack playerCombat;
    [SerializeField] private PlayerDoorInteraction doorInteraction;

    private void Update()
    {
        if (!inputManager.RightClickPressed)
            return;

        HandleRightClick();
    }

    private void HandleRightClick()
    {
        if (!inputManager.HasMouseHit)
            return;

        // -------------------------
        // Enemy
        // -------------------------

        EnemyManager enemy =
            inputManager.MouseHit.collider.GetComponentInParent<EnemyManager>();

        if (enemy != null)
        {
            doorInteraction.CancelDoorInteraction();

            playerCombat.SetAttackTarget(enemy);

            return;
        }

        // -------------------------
        // Door
        // -------------------------

        RoomDoor door =
            inputManager.MouseHit.collider.GetComponentInParent<RoomDoor>();

        if (door != null)
        {
            playerCombat.CancelAttack();

            doorInteraction.SetDoorTarget(door);

            return;
        }

        // -------------------------
        // Ground
        // -------------------------

        playerCombat.CancelAttack();
        doorInteraction.CancelDoorInteraction();

        playerMovement.MoveToPosition(
            inputManager.MouseWorldPosition
        );
    }
}