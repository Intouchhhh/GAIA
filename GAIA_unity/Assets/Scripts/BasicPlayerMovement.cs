using UnityEngine;
using System.Collections;
using Unity.Sentis;

public class BasicPlayerMovement : MonoBehaviour
{
	#region VARS

	[Header("Movement Settings")]
	public float moveSpeed = 11f;
	public float acceleration = 13f;
	public float decceleration = 16f;
	public float velPower = 0.9f;
	public float gravityScale = 5.0f;
	public float frictionAmount = 0.5f;

	[Header("Jump Settings")]
	public float jumpForce = 18f;
	public float coyoteTime = 0.15f;
	public float jumpBufferTime = 0.1f;
	public float jumpHangGravityMultiplier = 0.5f;
	public float fallMultiplier = 2f;
	public float maxFallSpeed;

	[Header("Wall Jump Settings")]
	public Vector2 wallJumpForce = new Vector2(15.0f, 25.0f);
	public float wallJumpLerp = 10f;
	public float wallSlideSpeed = 2f;
	public LayerMask _wallLayer;

	[Header("Dash Settings")]
	public float dashSpeed = 14f;
	public float dashDuration = 0.5f;
	public float dashCooldown = 1f;

	[Header("Checks")]
	[SerializeField] private Transform _wallCheckPointLeft;
	[SerializeField] private Transform _wallCheckPointRight;
	[SerializeField] private Vector2 _wallCheckSize = new Vector2(0.11f, 0.46f);
	[SerializeField] private Transform _groundCheckPoint;
	[SerializeField] private Vector2 _groundCheckSize = new Vector2(0.5f, 0.03f);
	public string _playerLayer = "Player";
	public LayerMask _groundLayer;
	public string _oneWayPlatLayer = "OneWayPlatform";

	[Header("Serialize")]
	[SerializeField] private Rigidbody2D rb;

	[SerializeField] private float lastGroundedTime;

	[SerializeField] private float lastJumpPressedTime;
	[SerializeField] private int wallJumpDir;

	[SerializeField] private float lastDashTime;
	[SerializeField] private float lastDashPressedTime;
	[SerializeField] private Vector2 dashingDirection;

	[SerializeField] private Vector2 moveInput;

	[Header("Flags")]
	[SerializeField] private bool isGrounded;
	[SerializeField] private bool isOnRightWall;
	[SerializeField] private bool isOnLeftWall;
	[SerializeField] private bool isOnWall;
	[SerializeField] private bool isDashing;
	[SerializeField] private bool isJumping;
	[SerializeField] private bool isWallJumping;
	[SerializeField] private bool isFacingRight;

	public bool IsGrounded => isGrounded;
	public bool IsOnWall => isOnWall;
	public bool IsDashing => isDashing;
	public bool IsJumping => isJumping;
	public bool IsWallJumping => isWallJumping;

	#endregion

	private void Awake()
	{
		lastDashPressedTime = -2.0f; // Prevent dash on first frame
		lastJumpPressedTime = -2.0f; // Prevent jump on first frame
		isFacingRight = true;

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
		HandleMovement(moveInput.x);
		HandleFriction();
		HandleWallSlide();
		HandleGrounded();
		HandleWallDetection();
		ApplyCustomGravity();
	}

	#region INPUT

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
			Dash();
		}
	}

	#endregion

	#region WALK

	private void HandleMovement(float direction)
	{
		if (isDashing) return;

		if (direction > 0 && !isFacingRight)
			Flip();
		else if (direction < 0 && isFacingRight)
			Flip();

		float targetSpeed = direction * moveSpeed;
		float speedDifference = targetSpeed - rb.linearVelocity.x;
		float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : decceleration;
		float movement = Mathf.Pow(Mathf.Abs(speedDifference) * accelRate, velPower) * Mathf.Sign(speedDifference);

		if (!isWallJumping)
		{
			rb.AddForce(movement * Vector2.right);
		}
		else
		{
			rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, (new Vector2(direction * moveSpeed, rb.linearVelocity.y)), wallJumpLerp * Time.deltaTime);
		}
	}

	#endregion

	#region JUMP
	private void Jump()
	{
		lastJumpPressedTime = Time.time;
	}

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
				wallJumpDir = (isOnRightWall) ? -1 : 1;
				PerformWallJump(wallJumpDir);
			}
		}
	}

	private bool CanJump() => isGrounded || Time.time - lastGroundedTime <= coyoteTime;
	private bool CanWallJump() => isOnWall && !isGrounded;

	private void PerformJump()
	{
		lastGroundedTime = 0;
		lastJumpPressedTime = 0;
		rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);

		rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
		isJumping = true;
	}

	private void PerformWallJump(int dir)
	{
		lastGroundedTime = 0;
		lastJumpPressedTime = 0;

		Vector2 force = new Vector2(wallJumpForce.x, wallJumpForce.y);
		force.x *= dir;

		if (Mathf.Sign(rb.linearVelocity.x) != Mathf.Sign(force.x))
			force.x -= rb.linearVelocity.x;

		if (rb.linearVelocity.y < 0)
			force.y -= rb.linearVelocity.y;

		rb.AddForce(force, ForceMode2D.Impulse);
		isWallJumping = true;
	}

	#endregion

	#region DASH
	private void Dash()
	{
		if (Time.time >= lastDashTime + dashCooldown)
		{
			lastDashPressedTime = Time.time;
		}
	}

	private void HandleDash()
	{
		if (CanDash() && lastDashPressedTime > lastDashTime)
		{
			StartCoroutine(PerformDash());
		}
	}
	private bool CanDash() => !isDashing && (Time.time >= lastDashTime + dashCooldown);

	private IEnumerator PerformDash()
	{
		Debug.Log("Dash");
		isDashing = true;

		lastDashPressedTime = Time.time;

		Vector2 dashDirection = isFacingRight ? Vector2.right : Vector2.left;

		rb.gravityScale = 0;
		rb.linearVelocity = dashDirection * dashSpeed;

		yield return new WaitForSeconds(dashDuration);

		rb.gravityScale = gravityScale;
		isDashing = false;
		lastDashTime = Time.time;
	}

	#endregion

	#region WALLSLIDE

	private void HandleWallSlide()
	{
		if (isOnWall && !isGrounded && rb.linearVelocity.y < 0)
		{
			rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
		}
	}

	#endregion

	#region CHECK

	private void HandleGrounded()
	{
		isGrounded = Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer) != null;

		if (isGrounded)
		{
			lastGroundedTime = Time.time;
			isJumping = false;
			isWallJumping = false;
		}
	}

	private void HandleWallDetection()
	{
		if (isGrounded)
		{
			isOnWall = isOnLeftWall = isOnRightWall = false;
			return;
		}

		Transform leftCheck = isFacingRight ? _wallCheckPointLeft : _wallCheckPointRight;
		Transform rightCheck = isFacingRight ? _wallCheckPointRight : _wallCheckPointLeft;

		isOnLeftWall = Physics2D.OverlapBox(leftCheck.position, _wallCheckSize, 0, _wallLayer) != null;
		isOnRightWall = Physics2D.OverlapBox(rightCheck.position, _wallCheckSize, 0, _wallLayer) != null;

		isOnWall = isOnLeftWall || isOnRightWall;
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("OneWayPlatCheck"))
        {
            Debug.Log("One Way (trigger)!");
			if (rb.linearVelocity.y > 0)
			{
				Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer(_playerLayer), LayerMask.NameToLayer(_oneWayPlatLayer), true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("OneWayPlatCheck"))
        {
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer(_playerLayer), LayerMask.NameToLayer(_oneWayPlatLayer), false);
        }
    }

    #endregion

    #region CUSTOM PHYSICS

    private void ApplyCustomGravity()
	{
		if (isJumping && Mathf.Abs(rb.linearVelocity.y) < 0.1f)
		{
			rb.gravityScale = gravityScale * jumpHangGravityMultiplier;
		}
		else if (!isDashing && rb.linearVelocity.y < 0) // Apply increased gravity when falling
		{
			rb.gravityScale = gravityScale * fallMultiplier;
			rb.linearVelocity = new Vector2(rb.linearVelocity.x,Mathf.Max(rb.linearVelocity.y, -maxFallSpeed));
		}
		else if (!isDashing)
		{
			rb.gravityScale = gravityScale;
		}
	}

	private void HandleFriction()
	{
		if (lastGroundedTime > 0 && Mathf.Abs(moveInput.x) < 0.01f)
		{
			float amount = Mathf.Min(Mathf.Abs(rb.linearVelocity.x), Mathf.Abs(frictionAmount));

			amount *= Mathf.Sign(rb.linearVelocity.x);

			rb.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
		}
	}

	#endregion

	#region HELPER

	private void Flip()
	{
		Vector3 scale = transform.localScale;
		scale.x *= -1;
		transform.localScale = scale;

		isFacingRight = !isFacingRight;
	}

	#endregion

	#region GIZMOS

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(_groundCheckPoint.position, _groundCheckSize);
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(_wallCheckPointLeft.position, _wallCheckSize);
		Gizmos.DrawWireCube(_wallCheckPointRight.position, _wallCheckSize);
	}

	#endregion
}