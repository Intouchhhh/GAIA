using UnityEngine;

public class Enemy : MonoBehaviour
{
	[SerializeField] private int maxHealth = 3;
	[SerializeField] private float damageCooldown = 0.5f;
	[SerializeField] private float knockbackForce = 5f;

	[SerializeField] private int currentHealth;
	[SerializeField] private bool isInvincible;
	[SerializeField] private float invincibilityTimer;
	private Rigidbody2D rb;

	public EnemyActionModule actionModule;

	private void Awake()
	{
		currentHealth = maxHealth;
		rb = GetComponent<Rigidbody2D>();
		if (actionModule == null)
			actionModule = GetComponent<EnemyActionModule>();
	}

	private void Update()
	{
		if (isInvincible)
		{
			invincibilityTimer -= Time.deltaTime;
			if (invincibilityTimer <= 0)
				isInvincible = false;
		}
	}

	public void TakeDamage(int amount, Vector2 hitSource)
	{
		if (isInvincible) return;

		currentHealth -= amount;
		isInvincible = true;
		invincibilityTimer = damageCooldown;

		ApplyKnockback(hitSource);

		if (currentHealth <= 0)
		{
			actionModule.Die();
		}
	}

	private void ApplyKnockback(Vector2 hitSource)
	{
		Debug.LogWarning("Knockback");
		if (rb == null) return;

		Vector2 dir = ((Vector2)transform.position - hitSource).normalized;
		rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
	}

	public void Die()
	{
		Destroy(gameObject);
	}
}
