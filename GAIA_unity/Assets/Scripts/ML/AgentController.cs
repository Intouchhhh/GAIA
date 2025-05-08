using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class AgentController : Agent
{
	#region Fields & Components
	[SerializeField] private PlayerActionModules playerActionModules;
	[SerializeField] private PlayerManager playerManager;
	[SerializeField] private RewardConfigSO rewardConfigSO;
	[SerializeField] private List<GameObject> spawnPointsList;
	private int totalCoins;
	private int totalCheckpoints;
	private int collectedCoins;
	private int collectedCheckpoints;
	private Vector2 lastPosition;
	private HashSet<Vector2Int> visitedAreas;

	private bool episodeCompleted = false;

	private HashSet<GameObject> visitedCheckpoints;
	private List<GameObject> allCoins;
	private List<GameObject> allEnemies;

	private int jumpCount = 0;
	private int dashCount = 0;
	private static int episodeCounter = 0;
	private static int totalStepsAcrossEpisodes = 0;
	private static int cumulativeDeaths = 0;

	public event System.Action OnEpisodeEnded;
	#endregion

	#region Expose
	public int JumpCount => jumpCount;
	public int DashCount => dashCount;
	public int CollectedCoins => collectedCoins;
	public int CollectedCheckpoints => collectedCheckpoints;
	public int TotalCoins => totalCoins;
	public int TotalCheckpoints => totalCheckpoints;
	public RewardConfigSO Config => rewardConfigSO;
	public int EpisodeNumber => episodeCounter;
	public int AverageSteps => episodeCounter == 0 ? 0 : totalStepsAcrossEpisodes / episodeCounter;
	public int DeathCount => cumulativeDeaths;

	#endregion

	#region Initialization
	public override void Initialize()
	{
		Time.timeScale = 1;

		if (playerActionModules == null)
			playerActionModules = GetComponent<PlayerActionModules>();

		if (playerManager == null)
			playerManager = GetComponent<PlayerManager>();

		spawnPointsList = new List<GameObject>(GameObject.FindGameObjectsWithTag("Spawn"));

		allCoins = new List<GameObject>(GameObject.FindGameObjectsWithTag("Coins"));

		allEnemies = new List<GameObject>(GameObject.FindGameObjectsWithTag("Enemy"));
	}
	#endregion

	private void FixedUpdate()
	{
		if (!episodeCompleted && StepCount >= MaxStep && MaxStep > 0)
		{
			Debug.LogError("Max Step Reached !!!");
			CompleteEpisode();
		}
	}

	#region Episode Handling
	public override void OnEpisodeBegin()
	{
		if (playerActionModules == null)
			playerActionModules = GetComponent<PlayerActionModules>();

		DisablePlayerInput();

		// Reset velocity and position
		if (playerActionModules?.basicPlayerMovement != null)
		{
			playerActionModules.basicPlayerMovement.rb = GetComponent<Rigidbody2D>();
			playerActionModules.basicPlayerMovement.Rb.linearVelocity = Vector2.zero;
		}

		if (Academy.Instance.IsCommunicatorOn)
		{
			transform.position = spawnPointsList[Random.Range(0, spawnPointsList.Count)].transform.position;
		}

		playerManager.currentHealth = playerManager.maxHealth;

		lastPosition = transform.position;
		visitedAreas = new HashSet<Vector2Int>();
		visitedCheckpoints = new HashSet<GameObject>();

		totalCoins = GameObject.FindGameObjectsWithTag("Coins").Length;
		totalCheckpoints = GameObject.FindGameObjectsWithTag("Checkpoint").Length;
		collectedCoins = 0;
		collectedCheckpoints = 0;

		episodeCompleted = false;

		foreach (var coin in allCoins)
		{
			if (coin != null)
				coin.SetActive(true);
		}

		foreach (var enemy in allEnemies)
		{
			if (enemy != null)
			{
				enemy.SetActive(true);
				var enemyScript = enemy.GetComponent<Enemy>();
				if (enemyScript != null)
				{
					enemyScript.ResetEnemy();
				}
			}
		}

		jumpCount = 0;
		dashCount = 0;
	}

	private void DisablePlayerInput()
	{
		if (playerActionModules == null) return;

		if (playerActionModules.basicPlayerMovement == null)
			playerActionModules.basicPlayerMovement = GetComponent<BasicPlayerMovement>();

		if (playerActionModules.playerAttack == null)
			playerActionModules.playerAttack = GetComponent<PlayerAttack>();

		playerActionModules.basicPlayerMovement.inputEnabled = false;
		playerActionModules.playerAttack.inputEnabled = false;
	}
	#endregion

	#region Observations
	public override void CollectObservations(VectorSensor sensor)
	{
		var movement = playerActionModules?.basicPlayerMovement;

		if (movement == null)
		{
			sensor.AddObservation(0f); // fallback values
			return;
		}

		// Position
		sensor.AddObservation(transform.position.x);
		sensor.AddObservation(transform.position.y);

		// Movement States
		sensor.AddObservation(movement.IsGrounded ? 1f : 0f);
		sensor.AddObservation(movement.IsJumping ? 1f : 0f);
		sensor.AddObservation(movement.IsFacingRight ? 1f : 0f);
		sensor.AddObservation(movement.IsDashing ? 1f : 0f);
		sensor.AddObservation(movement.IsDropping ? 1f : 0f);
		sensor.AddObservation(movement.IsOnWall ? 1f : 0f);
		sensor.AddObservation(movement.IsWallJumping ? 1f : 0f);

		// Health
		sensor.AddObservation(playerManager.currentHealth);

		// Attacking States
		sensor.AddObservation(playerActionModules.playerAttack.isAttacking ? 1f : 0f);
	}

	private GameObject GetNearestCoin()
	{
		return allCoins
			.Where(c => c.activeSelf)
			.OrderBy(c => Vector2.Distance(transform.position, c.transform.position))
			.FirstOrDefault();
	}

	public void CompleteEpisode()
	{
		episodeCounter++;
		totalStepsAcrossEpisodes += StepCount;

		if (playerManager.currentHealth <= 0 || transform == null)
		{
			cumulativeDeaths++;
		}

		if (episodeCompleted) return;
		episodeCompleted = true;

		HandleCompletionRewards();
		EndEpisode();
	}
	#endregion

	#region Actions
	public override void OnActionReceived(ActionBuffers actions)
	{
		float moveX = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
		bool jumpAction = actions.DiscreteActions[0] == 1;
		bool dashAction = actions.DiscreteActions[1] == 1;
		bool attackAction = actions.DiscreteActions[2] == 1;
		bool dropAction = actions.DiscreteActions[3] == 1;

		// Delegate input
		playerActionModules.Move(moveX);
		if (jumpAction)
		{
			AddReward(rewardConfigSO.jumpPenalty);
			playerActionModules.Jump();
			jumpCount++;
		}
		if (dashAction)
		{
			AddReward(rewardConfigSO.dashPenalty);
			playerActionModules.Dash();
			dashCount++;
		}
		if (attackAction) playerActionModules.Attack();
		if (dropAction) playerActionModules.Drop();

		EvaluateRewards();
	}
	#endregion

	#region Reward System
	private void EvaluateRewards()
	{
		// Distance Reward
		float distanceMoved = Vector2.Distance(transform.position, lastPosition);
		if (distanceMoved > 0.1f)
			AddReward(rewardConfigSO.movingReward * distanceMoved); // Moving Reward
		else
			AddReward(rewardConfigSO.idlePenalty); // Idle Penalty

		lastPosition = transform.position;

		// Exploration Reward
		Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
		if (!visitedAreas.Contains(gridPos))
		{
			AddReward(rewardConfigSO.explorationReward); // Explore Reward
			visitedAreas.Add(gridPos);
		}

		var nearestCoin = GetNearestCoin();
		if (nearestCoin != null)
		{
			float distance = Vector2.Distance(transform.position, nearestCoin.transform.position);
			float reward = Mathf.Clamp01(1f / (distance + 1f)); // Smaller distance = higher reward
			AddReward(rewardConfigSO.coinShapingWeight * reward); // e.g. 0.01f
		}
	}
	#endregion

	#region Collisions
	private void OnTriggerEnter2D(Collider2D collision)
	{
		switch (collision.tag)
		{
			case "Coins":
				Debug.LogError("Coins Hit");
				AddReward(rewardConfigSO.coinReward); // Coin Reward
				collectedCoins++;
				break;

			case "Hazard":
				Debug.LogError("Hazard Hit");
				AddReward(rewardConfigSO.hazardPenalty); // Hazard Penalty
				break;

			case "Checkpoint":
				if (!visitedCheckpoints.Contains(collision.gameObject))
				{
					Debug.LogError("Checkpoint Hit");
					AddReward(rewardConfigSO.checkpointReward);
					collectedCheckpoints++;
					visitedCheckpoints.Add(collision.gameObject);
				}
				break;
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		switch (collision.gameObject.tag)
		{
			case "Enemy":
				Debug.LogError("Enemy Hit");
				AddReward(rewardConfigSO.hitByEnemyPenalty);
				break;
		}
	}
	#endregion

	public void HandleCompletionRewards()
	{
		if (totalCoins > 0)
		{
			float coinCompletion = (float)collectedCoins / totalCoins;
			Debug.Log($"Coin Completion: {coinCompletion * 100f}%");
			AddReward(rewardConfigSO.coinCompletionBonus * coinCompletion);
		}

		if (totalCheckpoints > 0)
		{
			float checkpointCompletion = (float)collectedCheckpoints / totalCheckpoints;
			Debug.Log($"Checkpoint Completion: {checkpointCompletion * 100f}%");
			AddReward(rewardConfigSO.checkpointCompletionBonus * checkpointCompletion);
		}

		AddReward(rewardConfigSO.jumpCountTax * jumpCount);
		AddReward(rewardConfigSO.dashCountTax * dashCount);
		OnEpisodeEnded?.Invoke();
		EndEpisode();
	}


	#region Heuristics (for manual testing)
	public override void Heuristic(in ActionBuffers actionsOut)
	{
		var continuousActions = actionsOut.ContinuousActions;
		var discreteActions = actionsOut.DiscreteActions;

		continuousActions[0] = Input.GetAxisRaw("Horizontal");
		discreteActions[0] = Input.GetKeyDown(KeyCode.Space) ? 1 : 0;
		discreteActions[1] = Input.GetKeyDown(KeyCode.LeftShift) ? 1 : 0;
		discreteActions[2] = Input.GetMouseButtonDown(0) ? 1 : 0;
		discreteActions[3] = Input.GetKey(KeyCode.S) ? 1 : 0;
	}
	#endregion
}
