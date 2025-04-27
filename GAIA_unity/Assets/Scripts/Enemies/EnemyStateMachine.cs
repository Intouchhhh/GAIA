using UnityEngine;

public class EnemyStateMachine : MonoBehaviour
{
	public enum EnemyState { Patrol, Idle, Chase }

	[Header("Patrol Settings")]
	public float speed = 2f;
	public Transform[] patrolPoints;
	private int currentPointIndex = 0;

	[Header("Chase Settings")]
	public float chaseSpeed = 3f;
	public float detectionRange = 5f;
	public BasicPlayerMovement playerObj;
	public Transform playerPosition;
	public GameObject detected;

	[Header("State Machine")]
	public EnemyState currentState = EnemyState.Patrol;

	private Rigidbody2D rb;
	public GameObject sprite;

	void Start()
	{
		rb = GetComponent<Rigidbody2D>();
		rb.freezeRotation = true;

		playerObj = FindFirstObjectByType<BasicPlayerMovement>();
		if (playerObj != null)
			playerPosition = playerObj.transform;

		if (patrolPoints.Length == 0)
			Debug.LogWarning("No patrol points assigned!");
	}

	void Update()
	{
		switch (currentState)
		{
			case EnemyState.Patrol:
				Patrol();
				CheckForPlayer();
				break;

			case EnemyState.Chase:
				ChasePlayer();
				CheckForPlayerOutOfRange();
				break;

			case EnemyState.Idle:
				rb.linearVelocity = Vector2.zero;
				break;
		}
	}

	void Patrol()
	{
		if (patrolPoints.Length == 0) return;

		Transform targetPoint = patrolPoints[currentPointIndex];
		Vector2 direction = (targetPoint.position - transform.position).normalized;
		rb.linearVelocity = new Vector2(direction.x * speed, rb.linearVelocity.y);

		if (Vector2.Distance(transform.position, targetPoint.position) < 0.2f)
		{
			currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
		}

		FlipSprite(direction.x);
	}

	void ChasePlayer()
	{
		if (playerPosition == null) return;

		Vector2 direction = (playerPosition.position - transform.position).normalized;
		rb.linearVelocity = new Vector2(direction.x * chaseSpeed, rb.linearVelocity.y);

		FlipSprite(direction.x);
	}

	void FlipSprite(float moveDirection)
	{
		if (moveDirection != 0 && sprite != null)
		{
			Vector3 scale = sprite.transform.localScale;
			if ((moveDirection < 0 && scale.x > 0) || (moveDirection > 0 && scale.x < 0))
			{
				scale.x *= -1;
				sprite.transform.localScale = scale;
			}
		}
	}

	void CheckForPlayer()
	{
		if (playerPosition == null) return;

		if (Vector2.Distance(transform.position, playerPosition.position) <= detectionRange)
		{
			detected?.SetActive(true);
			currentState = EnemyState.Chase;
		}
	}

	void CheckForPlayerOutOfRange()
	{
		if (playerPosition == null) return;

		if (Vector2.Distance(transform.position, playerPosition.position) > detectionRange)
		{
			detected?.SetActive(false);
			currentState = EnemyState.Patrol;
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.gameObject.CompareTag("Player"))
		{
			PlayerManager playerManager = collision.gameObject.GetComponent<PlayerManager>();
			if (playerManager != null)
			{
				playerManager.Die();
			}
		}
	}
}
