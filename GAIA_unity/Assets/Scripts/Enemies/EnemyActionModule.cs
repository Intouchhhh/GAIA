using UnityEngine;

public class EnemyActionModule : MonoBehaviour
{
	public Enemy enemyCombat;
	public RewardConfigSO rewardConfigSO;

	private void Awake()
	{
		if (enemyCombat == null)
			enemyCombat = GetComponent<Enemy>();
	}

	public void TakeDamage(int amount, Vector2 hitSource)
	{
		AgentController agent = FindFirstObjectByType<AgentController>();
		if (agent != null)
		{
			agent.AddReward(rewardConfigSO.enemyKillReward);
		}
		if (enemyCombat != null)
			enemyCombat.TakeDamage(amount, hitSource);
	}

	public void Die()
	{
		AgentController agent = FindFirstObjectByType<AgentController>();
		if (agent != null)
		{
			agent.AddReward(rewardConfigSO.enemyDamageReward);
		}

		if (enemyCombat != null)
			enemyCombat.Die();
	}

	// For future extension:
	// public void MoveTo(Vector2 position)
	// public void Attack()
}
