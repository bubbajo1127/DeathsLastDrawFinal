using UnityEngine;

public class PlayerDashAbility : PlayerAbility
{
    [Header("References")]
    [SerializeField] private DashAbilityData dashData;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerAnimator playerAnimation;
    [SerializeField] private PlayerStats playerStats;

    private float nextUseTime;

    public override AbilityData Data => dashData;

    public override bool TryUse()
    {
        if (!IsUnlocked)
            return false;

        if (playerStats.IsDead())
            return false;

        if (playerMovement.IsDashing)
            return false;

        if (Time.time < nextUseTime)
            return false;

        Vector3 mousePosition;

        if (inputManager.HasMouseHit)
        {
            mousePosition = inputManager.MouseWorldPosition;
        }
        else if (!inputManager.TryGetMousePositionOnPlane(transform.position.y, out mousePosition))
        {
            return false;
        }

        Vector3 direction = mousePosition - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return false;

        playerAnimation.PlayDash();
        playerMovement.Dash(direction, dashData.DashDistance, dashData.DashDuration);

        nextUseTime = Time.time + dashData.Cooldown;

        return true;
    }
}