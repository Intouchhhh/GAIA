using UnityEngine;

public class FlyingEnemy : MonoBehaviour
{
    [Header("Flying Patrol Settings")]
    public float speed = 2f; // Movement speed
    public Transform[] patrolPoints; // Waypoints for patrol
    private int currentPointIndex = 0;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Ensure gravity is disabled for flying
        rb.gravityScale = 0;

        // Freeze rotation to prevent physics interference
        rb.freezeRotation = true;

        if (patrolPoints.Length == 0)
        {
            Debug.LogWarning("No patrol points assigned!");
        }
    }

    void Update()
    {
        Patrol();
    }

    void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        // Get the current patrol point
        Transform targetPoint = patrolPoints[currentPointIndex];
        Vector2 direction = (targetPoint.position - transform.position).normalized;

        // Move towards the target patrol point
        rb.linearVelocity = direction * speed;

        // Check if the enemy has reached the target point
        if (Vector2.Distance(transform.position, targetPoint.position) < 0.2f)
        {
            currentPointIndex++;
            if (currentPointIndex >= patrolPoints.Length)
            {
                currentPointIndex = 0; // Loop back to the first point
            }
        }

        // Flip the sprite to face the movement direction
        FlipSprite(direction.x);
    }

    void FlipSprite(float moveDirection)
    {
        if (moveDirection != 0 && spriteRenderer != null)
        {
            spriteRenderer.flipX = moveDirection < 0;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If the player collides with the flying enemy, trigger player death
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.Die();
            }
        }
    }
}