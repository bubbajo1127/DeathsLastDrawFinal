using UnityEngine;

public class PlayerDashAbility : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DashAbilityData dashData;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerAnimator playerAnimation;
    [SerializeField] private PlayerStats playerStats;

    private float nextUseTime;

    private void Update()
    {
        if (!inputManager.Ability1Pressed)
            return;

        TryDash();
    }

    private void TryDash()
    {
        if (playerStats.IsDead())
            return;

        if (playerMovement.IsDashing)
            return;

        if (Time.time < nextUseTime)
            return;

        Vector3 mousePosition;

        if (inputManager.HasMouseHit)
        {
            mousePosition = inputManager.MouseWorldPosition;
        }
        else if (!inputManager.TryGetMousePositionOnPlane(transform.position.y, out mousePosition))
        {
            return;
        }

        Vector3 direction = mousePosition - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        playerAnimation.PlayDash();
        playerMovement.Dash(direction, dashData.DashDistance, dashData.DashDuration);

        nextUseTime = Time.time + dashData.Cooldown;
    }
}