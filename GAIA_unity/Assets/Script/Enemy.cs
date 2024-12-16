using UnityEngine;

public class EnemyStateMachine : MonoBehaviour
{
    public enum EnemyState { Patrol, Idle }

    [Header("Patrol Settings")]
    public float speed = 2f; // Movement speed
    public Transform[] patrolPoints; // Waypoints for patrol
    private int currentPointIndex = 0;

    [Header("State Machine")]
    public EnemyState currentState = EnemyState.Patrol;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        if (patrolPoints.Length == 0)
        {
            Debug.LogWarning("No patrol points assigned!");
        }
    }

    void Update()
    {
        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                break;

            case EnemyState.Idle:
                // Enemy can stop moving or perform another behavior.
                rb.linearVelocity = Vector2.zero;
                break;
        }
    }

    void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        Transform targetPoint = patrolPoints[currentPointIndex];
        Vector2 direction = (targetPoint.position - transform.position).normalized;

        // Move enemy towards the current patrol point
        rb.linearVelocity = new Vector2(direction.x * speed, rb.linearVelocity.y);

        // Check if the enemy is close enough to switch to the next point
        if (Vector2.Distance(transform.position, targetPoint.position) < 0.2f)
        {
            currentPointIndex++;
            if (currentPointIndex >= patrolPoints.Length)
            {
                currentPointIndex = 0; // Loop back to the first point
            }

            // Flip the enemy sprite when changing direction
            FlipSprite(direction.x);
        }
    }

    void FlipSprite(float moveDirection)
    {
        if (moveDirection != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(moveDirection), 1, 1);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the player touches the enemy
        if (collision.gameObject.CompareTag("Player"))
        {
            // Call a "Die" function on the player
            collision.gameObject.GetComponent<PlayerController>().Die();
        }
    }
}