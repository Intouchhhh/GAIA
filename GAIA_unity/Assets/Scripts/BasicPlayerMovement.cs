using UnityEngine;

public class BasicPlayerMovement : MonoBehaviour
{
	[Header("Movement Settings")]
	public float moveSpeed = 12f;
	public float acceleration = 7f;
	public float decceleration = 7f;
	public float velPower = 0.9f;
	public float gravityScale = 2.0f;

	[Header("Jump Settings")]
	public float jumpForce = 15f;
	public float coyoteTime = 0.15f;
	public float jumpBufferTime = 0.1f;
	public float fallMultiplier = 2f;

	[Header("Wall Jump Settings")]
	public float wallJumpForce = 15f;
	public float wallSlideSpeed = 2f;
	public LayerMask wallLayer;

	[Header("Dash Settings")]
	public float dashSpeed = 20f;
	public float dashDuration = 0.2f;
	public float dashCooldown = 1f;

	[Header("Checks")]
	public float wallCheckLength = 1.0f;
	public float groundCheckLength = 1.0f;
	public LayerMask groundLayer;

	[Header("Serialize")]
	[SerializeField] private Rigidbody2D rb;
	[SerializeField] private float lastDashTime;
	[SerializeField] private int airJumpsRemaining;
	[SerializeField] private float lastGroundedTime;
	[SerializeField] private float lastJumpPressedTime;
	[SerializeField] private Vector2 moveInput;

	[Header("Flags")]
	[SerializeField] private bool isGrounded;
	[SerializeField] private bool isOnWall;
	[SerializeField] private bool isDashing;
	[SerializeField] private bool isJumping;

	public bool IsGrounded => isGrounded;
	public bool IsOnWall => isOnWall;
	public bool IsDashing => isDashing;
	public bool IsJumping => isJumping;


	[Header("Inputs")]
	public bool dashInput;

	private void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
	}

	private void Update()
	{
		HandleInput();
		HandleJump();
		HandleDash();
	}

	private void FixedUpdate()
	{
		HandleMovement();
		HandleWallSlide();
		HandleGrounded();
		HandleWallDetection();
		ApplyCustomGravity();
	}

	private void HandleInput()
	{
		// Movement Input
		moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

		// Jump Pressed (For Jump Buffering)
		if (Input.GetKeyDown(KeyCode.Space))
		{
			Jump();
		}

		// Dash Input (Single press detection)
		if (Input.GetKeyDown(KeyCode.LeftShift))
		{
			dashInput = true;
		}
	}

	private void HandleMovement()
	{
		if (isDashing) return;

		float targetSpeed = moveInput.x * moveSpeed;
		float speedDifference = targetSpeed - rb.velocity.x;
		float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : decceleration;
		float movement = Mathf.Pow(Mathf.Abs(speedDifference) * accelRate, velPower) * Mathf.Sign(speedDifference);

		rb.AddForce(movement * Vector2.right);

		if (Mathf.Abs(moveInput.x) < 0.1f)	
		{
			rb.velocity = new Vector2(0, rb.velocity.y);
		}
	}

	#region JUMP

	private void HandleJump()
	{
		if (Time.time - lastJumpPressedTime <= jumpBufferTime)
		{
			if (CanJump())
			{
				PerformJump();
			}
			else if (CanWallJump())
			{
				WallJump();
			}
		}
	}

	private bool CanJump() => isGrounded || Time.time - lastGroundedTime <= coyoteTime;
	private bool CanWallJump() => isOnWall && !isGrounded;

	private void Jump()
	{
		lastJumpPressedTime = Time.time;
	}

	private void PerformJump()
	{
		rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
		lastGroundedTime = 0;
		lastJumpPressedTime = 0;
		isJumping = true;
	}

	private void WallJump()
	{
		int wallDirection = isOnWall ? (int)Mathf.Sign(transform.localScale.x) : 0;
		rb.velocity = new Vector2(wallJumpForce * -wallDirection, jumpForce);
		lastJumpPressedTime = 0;
	}

	#endregion

	#region DASH

	private void HandleDash()
	{
		// Check if the dash input is pressed and the dash is not on cooldown
		if (dashInput && Time.time - lastDashTime >= dashCooldown)
		{
			// Start the dash
			isDashing = true;
			lastDashTime = Time.time;
			rb.velocity = new Vector2(moveInput.x * dashSpeed, 0);
		}

		// End the dash after the duration
		if (isDashing && Time.time - lastDashTime >= dashDuration)
		{
			isDashing = false;
			rb.velocity = new Vector2(rb.velocity.x * 0.5f, rb.velocity.y); // Slow down after dash
		}
	}

	#endregion

	private void HandleWallSlide()
	{
		if (isOnWall && !isGrounded && rb.velocity.y < 0)
		{
			rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -wallSlideSpeed));
		}
	}

	private void HandleGrounded()
	{
		Vector2 rayOriginLeft = transform.position - new Vector3(0.25f, 0, 0);
		Vector2 rayOriginMid = transform.position;
		Vector2 rayOriginRight = transform.position + new Vector3(0.25f, 0, 0);

		RaycastHit2D hitLeft = Physics2D.Raycast(rayOriginLeft, Vector2.down, groundCheckLength, groundLayer);
		RaycastHit2D hitMid = Physics2D.Raycast(rayOriginMid, Vector2.down, groundCheckLength, groundLayer);
		RaycastHit2D hitRight = Physics2D.Raycast(rayOriginRight, Vector2.down, groundCheckLength, groundLayer);

		bool wasGrounded = isGrounded;
		isGrounded = hitLeft.collider != null || hitMid.collider != null || hitRight.collider != null;

		// Reset isJumping when landing
		if (!wasGrounded && isGrounded)
		{
			isJumping = false;
		}

		if (isGrounded)
		{
			lastGroundedTime = Time.time;
		}
	}

	private void HandleWallDetection()
	{
		Vector2 rayOrigin = transform.position;

		RaycastHit2D hitLeft = Physics2D.Raycast(rayOrigin, Vector2.left, wallCheckLength, wallLayer);
		RaycastHit2D hitRight = Physics2D.Raycast(rayOrigin, Vector2.right, wallCheckLength, wallLayer);

		isOnWall = hitLeft.collider != null || hitRight.collider != null;
	}

	private void ApplyCustomGravity()
	{
		if (rb.velocity.y < 0) // Apply increased gravity when falling
		{
			rb.gravityScale = gravityScale * fallMultiplier;
		}
		else
		{
			rb.gravityScale = gravityScale;
		}
	}

	private void OnDrawGizmos()
	{
		// Draw ground detection rays
		Vector2 rayOriginLeft = transform.position - new Vector3(0.25f, 0, 0);
		Vector2 rayOriginMid = transform.position;
		Vector2 rayOriginRight = transform.position + new Vector3(0.25f, 0, 0);

		Gizmos.color = Color.green;
		Gizmos.DrawLine(rayOriginLeft, rayOriginLeft + Vector2.down * groundCheckLength);
		Gizmos.DrawLine(rayOriginMid, rayOriginMid + Vector2.down * groundCheckLength);
		Gizmos.DrawLine(rayOriginRight, rayOriginRight + Vector2.down * groundCheckLength);

		// Draw wall detection ray
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(rayOriginMid, rayOriginMid + Vector2.left * wallCheckLength);
		Gizmos.DrawLine(rayOriginMid, rayOriginMid + Vector2.right * wallCheckLength);
	}
}