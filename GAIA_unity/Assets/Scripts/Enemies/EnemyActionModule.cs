using UnityEngine;

public class EnemyActionModule : MonoBehaviour
{
	public Enemy enemyCombat;

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
			agent.AddReward(1.0f);
		}
		if (enemyCombat != null)
			enemyCombat.TakeDamage(amount, hitSource);
	}

	public void Die()
	{
		AgentController agent = FindFirstObjectByType<AgentController>();
		if (agent != null)
		{
			agent.AddReward(5.0f);
		}

		if (enemyCombat != null)
			enemyCombat.Die();
	}
}
