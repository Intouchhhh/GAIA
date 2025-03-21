using UnityEngine;
using System.Collections;
using Unity.Sentis;
using JetBrains.Annotations;

public class BasicPlayerMovement : MonoBehaviour
{
	#region VARS

	[Header("Movement Settings")]
	[SerializeField] private float moveSpeed = 11f;
	[SerializeField] private float acceleration = 13f;
	[SerializeField] private float decceleration = 16f;
	[SerializeField] private float velPower = 0.9f;
	[SerializeField] private float gravityScale = 5.0f;
	[SerializeField] private float frictionAmount = 0.5f;

	[Header("Jump Settings")]
	[SerializeField] private float jumpForce = 18f;
	[SerializeField] private float coyoteTime = 0.15f;
	[SerializeField] private float jumpBufferTime = 0.1f;
	[SerializeField] private float jumpHangGravityMultiplier = 0.5f;
	[SerializeField] private float fallMultiplier = 2f;
	[SerializeField] private float maxFallSpeed;

	[Header("Wall Jump Settings")]
	[SerializeField] private Vector2 wallJumpForce = new Vector2(15.0f, 25.0f);
	[SerializeField] private float wallJumpLerp = 10f;
	[SerializeField] private float wallSlideSpeed = 2f;
	[SerializeField] private LayerMask _wallLayer;

	[Header("Dash Settings")]
	[SerializeField] private float dashSpeed = 14f;
	[SerializeField] private float dashDuration = 0.5f;
	[SerializeField] private float dashCooldown = 1f;

	[Header("Checks")]
	[SerializeField] private Transform _wallCheckPointLeft;
	[SerializeField] private Transform _wallCheckPointRight;
	[SerializeField] private Vector2 _wallCheckSize = new Vector2(0.11f, 0.46f);
	[SerializeField] private Transform _groundCheckPoint;
	[SerializeField] private Vector2 _groundCheckSize = new Vector2(0.5f, 0.03f);
	[SerializeField] private string _playerLayer = "Player";
	[SerializeField] private LayerMask _groundLayer;
	[SerializeField] private string _oneWayPlatLayer = "OneWayPlatform";

	[Header("Serialize")]
	[SerializeField] private Rigidbody2D rb;

	[SerializeField] private float lastGroundedTime;

	[SerializeField] private float lastJumpPressedTime;
	[SerializeField] private int wallJumpDir;

	[SerializeField] private float lastDashTime;
	[SerializeField] private float lastDashPressedTime;
	[SerializeField] private Vector2 dashDirection;

	[Header("Inputs (Public)")]
	public Vector2 moveInput;
	public float dropInput;
	public float moveDir;
	public bool jumpInput;
	public bool dashInput;

    [Header("Flags")]
	private bool isGrounded;
	private bool isOnRightWall;
	private bool isOnLeftWall;
	private bool isOnWall;
	private bool isDashing;
	private bool isJumping;
	private bool isDropping;
	private bool isWallJumping;
	private bool isFacingRight;

	#region PUBLIC GETTERS

	[Header("Public getters")]
	// Flags (Read-only)
	public bool IsGrounded => isGrounded;
	public bool IsOnRightWall => isOnRightWall;
	public bool IsOnLeftWall => isOnLeftWall;
	public bool IsOnWall => isOnWall;
	public bool IsDashing => isDashing;
	public bool IsJumping => isJumping;
	public bool IsWallJumping => isWallJumping;
	public bool IsFacingRight => isFacingRight;
	public bool IsDropping => isDropping;

	// Movement settings (Read-only)
	public float MoveSpeed => moveSpeed;
	public float Acceleration => acceleration;
	public float Decceleration => decceleration;
	public float VelPower => velPower;
	public float GravityScale => gravityScale;
	public float FrictionAmount => frictionAmount;

	// Jump settings (Read-only)
	public float JumpForce => jumpForce;
	public float CoyoteTime => coyoteTime;
	public float JumpBufferTime => jumpBufferTime;
	public float JumpHangGravityMultiplier => jumpHangGravityMultiplier;
	public float FallMultiplier => fallMultiplier;
	public float MaxFallSpeed => maxFallSpeed;

	// Wall jump settings (Read-only)
	public Vector2 WallJumpForce => wallJumpForce;
	public float WallJumpLerp => wallJumpLerp;
	public float WallSlideSpeed => wallSlideSpeed;
	public LayerMask WallLayer => _wallLayer;

	// Dash settings (Read-only)
	public float DashSpeed => dashSpeed;
	public float DashDuration => dashDuration;
	public float DashCooldown => dashCooldown;

	// Checks (Read-only)
	public Transform WallCheckPointLeft => _wallCheckPointLeft;
	public Transform WallCheckPointRight => _wallCheckPointRight;
	public Vector2 WallCheckSize => _wallCheckSize;
	public Transform GroundCheckPoint => _groundCheckPoint;
	public Vector2 GroundCheckSize => _groundCheckSize;
	public string PlayerLayer => _playerLayer;
	public LayerMask GroundLayer => _groundLayer;
	public string OneWayPlatLayer => _oneWayPlatLayer;

	// Serialized references (Read-only)
	public Rigidbody2D Rb => rb;

	// Last action times (Read-only)
	public float LastGroundedTime => lastGroundedTime;
	public float LastJumpPressedTime => lastJumpPressedTime;
	public int WallJumpDir => wallJumpDir;
	public float LastDashTime => lastDashTime;
	public float LastDashPressedTime => lastDashPressedTime;
	public Vector2 DashDirection => dashDirection;

	#endregion

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
		HandleMovement(moveDir);
		HandleFriction();
		HandleWallSlide();
		HandleGrounded();
		HandleWallDetection();
		ApplyCustomGravity();
	}

	#region INPUT

	private void HandleInput()
	{
        #region Agent Notes
        // Input:
        // 0: moveDir = moveInput.x;						0 = still, 1 = left, 2 = right
        // 1: dropInput = moveInput.y;						0 = still, 1 = down, 2 = up
        // 2: jump = Input.GetKeyDown(KeyCode.Space)		0 = still, 1 = jump
        // 3: dash = Input.GetKeyDown(KeyCode.LeftShift)	0 = still, 1 = dash
        //
        // switch (xInput)
        // {
        //		case 0: moveDir =  0f; break;
        //		case 1: moveDir = -1f; break;
        //		case 2: moveDir = +1f; break;
        // }
        //
        // switch (yInput)
        // {
        //		case 0: dropInput =  0f; break;
        //		case 1: dropInput = -1f; break;
        //		case 2: dropInput = +1f; break;
        // }
        //
        // if (jump)
        // {
        //		Jump();
        // }
        //
        // if (dash)
        // {
        //		Dash();
        // }
        #endregion

        // Movement Input
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        moveDir = moveInput.x;
        dropInput = moveInput.y;
		jumpInput = Input.GetKeyDown(KeyCode.Space);
		dashInput = Input.GetKeyDown(KeyCode.LeftShift);

		// Jump Pressed (For Jump Buffering)
		if (jumpInput)
		{
			Jump();
		}

		// Dash Input (Single press detection)
		if (dashInput)
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
		if (dropInput < 0)
		{
			Drop();
			return;
		}
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
		isDashing = true;

		lastDashPressedTime = Time.time;

		dashDirection = isFacingRight ? Vector2.right : Vector2.left;

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

    #region DROP

	private void Drop()
	{
		if (isGrounded)
		{
			Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer(_playerLayer), LayerMask.NameToLayer(_oneWayPlatLayer), true);
			isDropping = true;
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
        if (collision.CompareTag("OneWayJumpCheck"))
        {
            Debug.LogError("One Way (trigger)!");
			Debug.LogError(rb.linearVelocity.y);
			if (rb.linearVelocity.y > 0)
			{
				Debug.LogError("Ignore!");
				Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer(_playerLayer), LayerMask.NameToLayer(_oneWayPlatLayer), true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("OneWayDropCheck"))
		{
			Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer(_playerLayer), LayerMask.NameToLayer(_oneWayPlatLayer), false);
			isDropping = false;
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