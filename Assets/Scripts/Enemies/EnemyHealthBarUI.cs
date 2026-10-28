using UnityEngine;
using Microlight.MicroBar;

public class EnemyHealthBarUI : MonoBehaviour
{
[Header("References")]
    [SerializeField] private EnemyStats enemyStats;
    [SerializeField] private MicroBar healthBar;

    private float previousHealth;

    private void Start()
    {
        if (enemyStats == null || healthBar == null)
        {
            Debug.LogError($"{gameObject.name}: EnemyHealthBarUI is missing references.");

            return;
        }

        healthBar.Initialize(enemyStats.MaxHealth);

        healthBar.UpdateBar(
            enemyStats.CurrentHealth
        );

        previousHealth = enemyStats.CurrentHealth;

        enemyStats.OnHealthChanged += HandleHealthChanged;
    }

    private void OnDestroy()
    {
        if (enemyStats != null)
        {
            enemyStats.OnHealthChanged -= HandleHealthChanged;
        }
    }

    private void HandleHealthChanged(
        float currentHealth,
        float maxHealth
    )
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
