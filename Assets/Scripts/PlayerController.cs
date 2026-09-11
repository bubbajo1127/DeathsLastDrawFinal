using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputManager inputManager;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerAttack playerCombat;

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

        EnemyManager enemy =
            inputManager.MouseHit.collider.GetComponentInParent<EnemyManager>();

        // Right-clicked an enemy.
        if (enemy != null)
        {
            playerCombat.SetAttackTarget(enemy);
            return;
        }

        // Right-clicked the ground.
        playerCombat.CancelAttack();
        playerMovement.MoveToPosition(inputManager.MouseWorldPosition);
    }
}