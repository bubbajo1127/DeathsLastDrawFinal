using Microlight.MicroBar;
using TMPro;
using UnityEngine;

public class BossHealthBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MicroBar healthBar;
    [SerializeField] private TMP_Text bossNameText;

    private EnemyStats bossStats;
    private float previousHealth;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void ShowBoss(EnemyStats stats, string bossName)
    {
        if (stats == null)
            return;

        Unsubscribe();

        bossStats = stats;
        previousHealth = bossStats.CurrentHealth;

        bossStats.OnHealthChanged += HandleHealthChanged;

        healthBar.Initialize(bossStats.MaxHealth);
        healthBar.UpdateBar(bossStats.CurrentHealth);

        if (bossNameText != null)
            bossNameText.text = bossName;

        gameObject.SetActive(true);
    }

    public void HideBoss()
    {
        Unsubscribe();
        gameObject.SetActive(false);
    }

    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {
        if (currentHealth < previousHealth)
            healthBar.UpdateBar(currentHealth, UpdateAnim.Damage);
        else
            healthBar.UpdateBar(currentHealth, UpdateAnim.Heal);

        previousHealth = currentHealth;

        if (currentHealth <= 0f)
            HideBoss();
    }

    private void Unsubscribe()
    {
        if (bossStats != null)
            bossStats.OnHealthChanged -= HandleHealthChanged;

        bossStats = null;
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }
}