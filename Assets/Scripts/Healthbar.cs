using UnityEngine;

public class HealthBar : MonoBehaviour
{
    private Transform healthBar;
    private SpriteRenderer healthBarRenderer;
    private Vector3 originalScale;

    private void Awake()
    {
        healthBar = transform.GetChild(0);
        healthBarRenderer = healthBar.GetComponentInChildren<SpriteRenderer>();
    }

        public void SetHealth(float health, float maxHealth)
    {
        if (maxHealth <= 0f || healthBar == null)
            return;

        float percentage = Mathf.Clamp01(health / maxHealth);

        Vector3 scale = healthBar.localScale;

        scale.x = percentage;
        scale.y = 1.0f;

        healthBar.localScale = scale;

        if (percentage > 0.5f)
                healthBarRenderer.color = Color.green;
            else if (percentage > 0.25f)
                healthBarRenderer.color = Color.yellow;
            else
                healthBarRenderer.color = Color.red;
    }
}
