using UnityEngine;

[CreateAssetMenu]
public class RewardConfigSO : ScriptableObject
{
	[Header("Movement")]
	public float idlePenalty = -0.01f;
	public float movingReward = 0.001f;
	public float explorationReward = 0.2f;
	public float jumpPenalty = -0.01f;
	public float dashPenalty = -0.01f;

	[Header("Goals")]
	public float coinReward = 1.0f;
	public float checkpointReward = 5.0f;

	[Header("Hazards")]
	public float hazardPenalty = -3.0f;

	[Header("Enemy Interaction")]
	public float enemyDamageReward = 1.0f;
	public float enemyKillReward = 5.0f;
	public float hitByEnemyPenalty = -2.0f;

	[Header("Completion Bonus")]
	public float coinCompletionBonus = 2.0f;
	public float checkpointCompletionBonus = 2.0f;

	[Header("Behavior Shaping Weight")]
	public float coinShapingWeight = 0.01f;
}
