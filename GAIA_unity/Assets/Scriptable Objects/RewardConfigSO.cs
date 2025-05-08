using UnityEngine;

[CreateAssetMenu]
public class RewardConfigSO : ScriptableObject
{
	[Header("Movement")]
	public float idlePenalty = -0.02f;
	public float movingReward = 0.001f;
	public float explorationReward = 0.05f;
	public float jumpPenalty = -0.03f;
	public float dashPenalty = -0.05f;

	[Header("Goals")]
	public float coinReward = 1.5f;
	public float checkpointReward = 2.0f;

	[Header("Hazards")]
	public float hazardPenalty = -3.0f;

	[Header("Enemy Interaction")]
	public float enemyDamageReward = 0.5f;
	public float enemyKillReward = 1.5f;
	public float hitByEnemyPenalty = -2.5f;

	[Header("Completion Rewards")]
	public float coinCompletionBonus = 2.0f;
	public float checkpointCompletionBonus = 2.0f;
	public float jumpCountTax = 0.01f;
	public float dashCountTax = 0.02f;

	[Header("Behavior Shaping Weight")]
	public float coinShapingWeight = 0.01f;
}
