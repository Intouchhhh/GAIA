using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class AgentController : Agent
{
	#region Fields & Components
	[SerializeField] private PlayerActionModules playerActionModules;
	[SerializeField] private PlayerManager playerManager;
	[SerializeField] private List<GameObject> spawnPointsList;
	private Vector2 lastPosition;
	private HashSet<Vector2Int> visitedAreas;
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
	}
	#endregion

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

		// transform.position = spawnPointsList[Random.Range(0, spawnPointsList.Count)].transform.position;

		lastPosition = transform.position;
		visitedAreas = new HashSet<Vector2Int>();
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
		if (jumpAction) playerActionModules.Jump();
		if (dashAction) playerActionModules.Dash();
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
			AddReward(0.05f * distanceMoved);
		else
			AddReward(-0.01f);

		lastPosition = transform.position;

		// Exploration Reward
		Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
		if (!visitedAreas.Contains(gridPos))
		{
			AddReward(0.2f);
			visitedAreas.Add(gridPos);
		}
	}
	#endregion

	#region Collisions
	private void OnTriggerEnter2D(Collider2D collision)
	{
		switch (collision.tag)
		{
			case "Coins":
				AddReward(1.0f);
				break;

			case "Hazard":
				AddReward(-3.0f);
				EndEpisode();
				break;

			case "Enemy":
				AddReward(-2.0f);
				break;

			case "Checkpoint":
				AddReward(5.0f);
				break;
		}
	}
	#endregion

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
