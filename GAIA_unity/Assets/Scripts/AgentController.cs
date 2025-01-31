using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class AgentController : Agent
{
	[SerializeField] private PlayerManager playerManager;
	[SerializeField] private PlayerMovement playerMovement;
	[SerializeField] private StageManager stageManager;

	private List<GameObject> coins;
	private List<GameObject> spawnPointsList;
	private List<GameObject> checkPointsList;

	private bool wasJumpingLastFrame = false;
	private bool wasDashingLastFrame = false;

	private Vector2 previousPosition;
	private float totalDistanceTraveled;

	private bool jumpHeld = false;

	public override void Initialize()
	{
		Time.timeScale = 1;
		coins = new List<GameObject>(GameObject.FindGameObjectsWithTag("Coins"));
		spawnPointsList = new List<GameObject>(GameObject.FindGameObjectsWithTag("Spawn"));
		checkPointsList = new List<GameObject>(GameObject.FindGameObjectsWithTag("Checkpoint"));
	}

	public override void OnEpisodeBegin()
	{
		// Reset player velocity and state at the beginning of each episode
		playerMovement.RB.linearVelocity = Vector3.zero;

		// Reset player position and state at the beginning of each episode
		int randomIndex = Random.Range(0, spawnPointsList.Count);
		//transform.position = spawnPointsList[randomIndex].transform.position;

		transform.position = stageManager.spawnPoint.transform.position;

		previousPosition = transform.position;
		totalDistanceTraveled = 0f;

		foreach (GameObject coin in coins)
		{
			if (coin != null)
			{
				coin.SetActive(true);
			}
		}
	}

	public override void CollectObservations(VectorSensor sensor)
	{
		// Add player-related observations
		sensor.AddObservation(playerMovement.IsFacingRight ? 1 : -1); // Direction
		sensor.AddObservation(playerMovement.IsJumping ? 1 : 0); // Jumping state
		sensor.AddObservation(playerMovement.IsDashing ? 1 : 0); // Dashing state
		sensor.AddObservation(playerMovement.LastOnGroundTime); // Time since last grounded

		// Add velocity observations
		sensor.AddObservation(playerMovement.RB.linearVelocity.x);
		sensor.AddObservation(playerMovement.RB.linearVelocity.y);

		// Add position-related observations
		//sensor.AddObservation(transform.position.x);
		//sensor.AddObservation(transform.position.y);
	}

	public override void OnActionReceived(ActionBuffers actions)
	{
		// Map the actions to the player's inputs

		//int moveX = actions.DiscreteActions[0];
		//if (moveX == 0)
		//{
		//	playerMovement._moveInput.x = -1.00;
		//}
		//else if (moveX == 1)
		//{
		//	playerMovement._moveInput.x = 0.00;
		//}
		//else if (moveX == 2)
		//{
		//	playerMovement._moveInput.x = 1;
		//}

		float moveX = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
		
		bool jumpAction = actions.DiscreteActions[0] == 1;
		bool jumpCutAction = actions.DiscreteActions[1] == 1;
		bool dashAction = actions.DiscreteActions[2] == 1;


		// Call PlayerMovement methods based on actions
		playerMovement._moveInput.x = moveX;

		if (jumpAction & !wasJumpingLastFrame)
		{
			AddReward(-0.5f);
			playerMovement.OnJumpInput();
		}

		if (jumpCutAction)
		{
			playerMovement.OnJumpUpInput();
		}

		if (dashAction & !wasDashingLastFrame)
		{
			AddReward(-0.5f);
			playerMovement.OnDashInput();
		}

		wasJumpingLastFrame = jumpAction;
		wasDashingLastFrame = dashAction;

		#region DISTANCE REWARD
		float stepDistance = Vector2.Distance(transform.position, previousPosition);
		totalDistanceTraveled += stepDistance;

		AddReward(stepDistance * 0.05f);

		previousPosition = transform.position;
		#endregion

		#region STAYING STILL PENALTY
		if (stepDistance < 0.01f)
		{
			AddReward(-0.05f);
		}
		#endregion

		Debug.LogWarning("Cumulative Reward: " + GetCumulativeReward());
	}

	public override void Heuristic(in ActionBuffers actionsOut)
	{
		var continuousActions = actionsOut.ContinuousActions;
		var discreteActions = actionsOut.DiscreteActions;

		continuousActions[0] = Input.GetAxisRaw("Horizontal");
		discreteActions[0] = Input.GetKeyDown(KeyCode.Space) ? 1 : 0; // Jump

		if (Input.GetKey(KeyCode.Space))
		{
			jumpHeld = true;
		}
		if (!Input.GetKey(KeyCode.Space) && jumpHeld)
		{
			discreteActions[1] = 1; // JumpCut
			jumpHeld = false; // Reset the flag
		}
		else
		{
			discreteActions[1] = 0;
		}

		discreteActions[2] = Input.GetKeyDown(KeyCode.LeftShift) ? 1 : 0; // Dash
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.CompareTag("Coins"))
		{
			Debug.LogWarning("HIT Coin");
			other.gameObject.SetActive(false);
			AddReward(1.0f);
			Debug.LogWarning("Coin: Cumulative Reward: " + GetCumulativeReward());
		}
		else if (other.gameObject.CompareTag("Checkpoint"))
		{
			Debug.LogWarning("HIT Checkpoint");
			other.gameObject.SetActive(false);
			AddReward(10.0f);
			Debug.LogError("Checkpoint: Cumulative Reward: " + GetCumulativeReward());
		}
		else if (other.gameObject.CompareTag("Spike"))
		{
			Debug.LogWarning("HIT Spike");
			AddReward(-10.0f);
			Debug.LogError("DIED: Cumulative Reward: " + GetCumulativeReward());
			EndEpisode();
		}

	}
}
