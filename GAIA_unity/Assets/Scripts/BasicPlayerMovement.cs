using UnityEngine;

public class BasicPlayerMovement : MonoBehaviour
{
    private float horizontal;
    private bool isFacingRight = true;

    [Header("Movement Settings")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpingPower = 10f;
    [SerializeField] private float dashForce = 15f;
    [SerializeField] private float dashCooldown = 1f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [Header("Temp Variables")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private bool isGrounded;
    [SerializeField] private bool canDash = true;
    [SerializeField] private bool isDashing;
    [SerializeField] private float lastDashTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        isGrounded = IsGrounded();
        horizontal = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower);
        }

        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Dash();
        }

        if ((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) && isGrounded)
        {
            DropThroughPlatform();
        }

        Flip();
    }

    private void FixedUpdate()
    {
        if (!isDashing)
        {
            rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);
        }
    }

    private void Dash()
    {
        if (canDash && Time.time - lastDashTime > dashCooldown)
        {
            canDash = false;
            isDashing = true;
            lastDashTime = Time.time;

            rb.linearVelocity = new Vector2(transform.localScale.x * dashForce, 0);
            Invoke(nameof(ResetDash), 0.2f);
        }
    }

    private void DropThroughPlatform()
    {
        Collider2D platform = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
        if (platform != null)
        {
            Physics2D.IgnoreCollision(platform, GetComponent<Collider2D>(), true);
            Invoke(nameof(RestoreCollision), 0.3f);
        }
    }

    private void ResetDash()
    {
        canDash = true;
        isDashing = false;
    }

    private void RestoreCollision()
    {
        Collider2D platform = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
        if (platform != null)
        {
            Physics2D.IgnoreCollision(platform, GetComponent<Collider2D>(), false);
        }
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    private void Flip()
    {
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }
}
