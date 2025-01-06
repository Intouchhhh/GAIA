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
    public Transform player;
    public GameObject detected;

	[Header("State Machine")]
    public EnemyState currentState = EnemyState.Patrol;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.freezeRotation = true;

        if (patrolPoints.Length == 0)
        {
            Debug.LogWarning("No patrol points assigned!");
        }
        if (player == null)
        {
            Debug.LogError("Player Transform is not assigned!");
        }
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
            currentPointIndex++;
            if (currentPointIndex >= patrolPoints.Length)
            {
                currentPointIndex = 0;
            }
        }

        FlipSprite(direction.x);
    }

    void ChasePlayer()
    {
        if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;

        rb.linearVelocity = new Vector2(direction.x * chaseSpeed, rb.linearVelocity.y);

        FlipSprite(direction.x);
    }

    void FlipSprite(float moveDirection)
    {
        if (moveDirection != 0 && spriteRenderer != null)
        {
            spriteRenderer.flipX = moveDirection < 0;
        }
    }

    void CheckForPlayer()
    {
        if (player == null) return;

        if (Vector2.Distance(transform.position, player.position) <= detectionRange)
        {
            Debug.LogWarning("Player Found!");
            detected.SetActive(true);
            currentState = EnemyState.Chase;
        }
    }

    void CheckForPlayerOutOfRange()
    {
        if (player == null) return;

        if (Vector2.Distance(transform.position, player.position) > detectionRange)
        {
			Debug.LogWarning("Player Gone!");
			detected.SetActive(false);
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

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, detectionRange);
	}
}