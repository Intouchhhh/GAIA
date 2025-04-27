using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerAttack : MonoBehaviour
{
	[Header("Attack Settings")]
	[SerializeField] private float attackCooldown = 0.5f;
	[SerializeField] private int attackDamage;
	[SerializeField] private LayerMask enemyLayer;
	[SerializeField] private Transform attackPoint;
	[SerializeField] private Vector2 attackCapsuleSize = new Vector2(1.2f, 0.6f);
	[SerializeField] private CapsuleDirection2D capsuleDirection = CapsuleDirection2D.Horizontal;
	[SerializeField] private GameObject attackVisual;

	[SerializeField] private PlayerManager playerManager;

	public bool isAttacking { get; private set; }

	private float lastAttackTime;
	public bool attackInput;
	public bool inputEnabled = true;

	private void Awake()
	{
		playerManager = GetComponent<PlayerManager>();
		attackDamage = playerManager.attackDamage;
	}

	void Update()
	{
		HandleInput();
	}

	private void HandleInput()
	{
		if (!inputEnabled) return;

		attackInput = Input.GetMouseButtonDown(0);

		if (attackInput)
		{
			PerformAttack();
		}
	}

	public void PerformAttack()
	{
		if (Time.time < lastAttackTime + attackCooldown) return;

		lastAttackTime = Time.time;
		isAttacking = true;

		// Enable attack visual
		if (attackVisual != null)
		{
			attackVisual.SetActive(true);
			Invoke(nameof(DisableAttackVisual), 0.1f); // Hide after a short duration
		}

		// Detect enemies in range
		Collider2D[] hitEnemies = Physics2D.OverlapCapsuleAll(attackPoint.position, attackCapsuleSize, capsuleDirection, 0, enemyLayer);

		foreach (Collider2D enemyCollider in hitEnemies)
		{
			EnemyActionModule enemy = enemyCollider.GetComponent<EnemyActionModule>();
			if (enemy != null)
			{
				enemy.TakeDamage(attackDamage, transform.position); // Apply knockback from player position
				Debug.Log("Enemy hit and damaged!");
			}
		}
	}

	private void DisableAttackVisual()
	{
		isAttacking = false;

		if (attackVisual != null)
		{
			attackVisual.SetActive(false);
		}
	}

	private void OnDrawGizmos()
	{
		if (attackPoint == null) return;

		Gizmos.color = Color.red;

		Vector3 center = attackPoint.position;
		float capsuleRadius = attackCapsuleSize.y / 2;

		Vector3 capsuleStart, capsuleEnd;

		if (capsuleDirection == CapsuleDirection2D.Horizontal)
		{
			capsuleStart = center - Vector3.right * (attackCapsuleSize.x / 2 - capsuleRadius);
			capsuleEnd = center + Vector3.right * (attackCapsuleSize.x / 2 - capsuleRadius);
		}
		else
		{
			capsuleStart = center - Vector3.up * (attackCapsuleSize.y / 2 - capsuleRadius);
			capsuleEnd = center + Vector3.up * (attackCapsuleSize.y / 2 - capsuleRadius);
		}

		// Draw capsule end circles
		Gizmos.DrawWireSphere(capsuleStart, capsuleRadius);
		Gizmos.DrawWireSphere(capsuleEnd, capsuleRadius);

		// Connect circles with lines
		if (capsuleDirection == CapsuleDirection2D.Horizontal)
		{
			Gizmos.DrawLine(capsuleStart + Vector3.up * capsuleRadius, capsuleEnd + Vector3.up * capsuleRadius);
			Gizmos.DrawLine(capsuleStart - Vector3.up * capsuleRadius, capsuleEnd - Vector3.up * capsuleRadius);
		}
		else
		{
			Gizmos.DrawLine(capsuleStart + Vector3.right * capsuleRadius, capsuleEnd + Vector3.right * capsuleRadius);
			Gizmos.DrawLine(capsuleStart - Vector3.right * capsuleRadius, capsuleEnd - Vector3.right * capsuleRadius);
		}
	}
}
