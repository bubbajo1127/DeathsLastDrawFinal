using UnityEngine;
using Microlight.MicroBar;

public class PlayerHealthBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private MicroBar healthBar;

    private float previousHealth;

    private void Start()
    {
        if (playerStats == null || healthBar == null)
        {
            Debug.LogError("PlayerHealthBarUI is missing references.");
            return;
        }

        healthBar.Initialize(playerStats.MaxHealth);

        healthBar.UpdateBar(
            playerStats.CurrentHealth
        );

        previousHealth = playerStats.CurrentHealth;

        playerStats.OnHealthChanged += HandleHealthChanged;
    }

    private void OnDestroy()
    {
        if (playerStats != null)
        {
            playerStats.OnHealthChanged -= HandleHealthChanged;
        }
    }

    private void HandleHealthChanged(float currentHealth,float maxHealth)
    {
        if (currentHealth < previousHealth)
        {
            healthBar.UpdateBar(
                currentHealth,
                UpdateAnim.Damage
            );
        }
        else
        {
            healthBar.UpdateBar(
                currentHealth,
                UpdateAnim.Heal
            );
        }

        previousHealth = currentHealth;
    }
}
