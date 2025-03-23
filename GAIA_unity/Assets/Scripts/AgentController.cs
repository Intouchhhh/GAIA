using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using static Unity.Cinemachine.CinemachineTriggerAction.ActionSettings;

public class AgentController : Agent
{
	[SerializeField] private ActionModules actionModules;
	[SerializeField] private List<Transform> spawnPoints;
	private Vector2 lastPosition;
	private HashSet<Vector2Int> visitedAreas;

	public override void Initialize()
	{
		Time.timeScale = 1;

		if (actionModules == null)
			actionModules = GetComponent<ActionModules>();

		if (actionModules == null)
		{
			Debug.LogError("ActionModules not found on " + gameObject.name);
			return;
		}

		spawnPoints = new List<Transform>();
		foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Spawn"))
		{
			spawnPoints.Add(obj.transform);
		}
	}

	public override void OnEpisodeBegin()
	{
		visitedAreas = new HashSet<Vector2Int>();
		lastPosition = transform.position;

		if (actionModules == null)
			actionModules = GetComponent<ActionModules>();

		if (actionModules == null)
		{
			Debug.LogError("ActionModules not found on " + gameObject.name);
			return;
		}

		if (spawnPoints.Count > 0)
		{
			transform.position = spawnPoints[Random.Range(0, spawnPoints.Count)].position;
		}

		if (actionModules != null)
		{
			actionModules.basicPlayerMovement.inputEnabled = false;
			actionModules.playerAttack.inputEnabled = false;
		}
		else
		{
			Debug.LogError("ActionModules not found on " + gameObject.name);
		}
	}

	public override void CollectObservations(VectorSensor sensor)
	{
		sensor.AddObservation(transform.position.x);
		sensor.AddObservation(transform.position.y);
		sensor.AddObservation(actionModules.basicPlayerMovement.IsGrounded ? 1 : 0);
		sensor.AddObservation(actionModules.basicPlayerMovement.IsJumping ? 1 : 0);
		sensor.AddObservation(actionModules.basicPlayerMovement.IsFacingRight ? 1 : 0);
		sensor.AddObservation(actionModules.basicPlayerMovement.IsDashing ? 1 : 0);
		sensor.AddObservation(actionModules.basicPlayerMovement.IsDropping ? 1 : 0);
		sensor.AddObservation(actionModules.basicPlayerMovement.IsOnWall ? 1 : 0);
		sensor.AddObservation(actionModules.basicPlayerMovement.IsWallJumping ? 1 : 0);
	}

	public override void OnActionReceived(ActionBuffers actions)
	{
		float moveX = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
		bool jumpAction = actions.DiscreteActions[0] == 1;
		bool dashAction = actions.DiscreteActions[1] == 1;
		bool attackAction = actions.DiscreteActions[2] == 1;
		bool dropAction = actions.DiscreteActions[3] == 1;

		actionModules.Move(moveX);
		if (jumpAction) actionModules.Jump();
		if (dashAction) actionModules.Dash();
		if (attackAction) actionModules.Attack();
		if (dropAction) actionModules.Drop();

		EvaluateRewards();
	}

	private void EvaluateRewards()
	{
		float distanceMoved = Vector2.Distance(transform.position, lastPosition);
		lastPosition = transform.position;
		if (distanceMoved > 0.1f) AddReward(0.05f * distanceMoved);

		Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
		if (!visitedAreas.Contains(gridPos))
		{
			AddReward(0.2f);
			visitedAreas.Add(gridPos);
		}
	}

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
}
