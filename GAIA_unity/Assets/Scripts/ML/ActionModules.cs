using UnityEngine;

public class ActionModules : MonoBehaviour
{
	public BasicPlayerMovement basicPlayerMovement;
	public PlayerAttack playerAttack;

	private void Awake()
	{
		if (basicPlayerMovement == null)
			basicPlayerMovement = GetComponent<BasicPlayerMovement>();

		if (playerAttack == null)
			playerAttack = GetComponent<PlayerAttack>();
	}

	public void Move(float direction)
	{
		if (basicPlayerMovement != null)
			basicPlayerMovement.moveDir = direction;
	}

	public void Jump()
	{
		if (basicPlayerMovement != null)
			basicPlayerMovement.Jump();
	}

	public void Dash()
	{
		if (basicPlayerMovement != null)
			basicPlayerMovement.Dash();
	}

	public void Drop()
	{
		if (basicPlayerMovement != null)
		{
			basicPlayerMovement.dropInput = -1f;
			basicPlayerMovement.Jump();
			basicPlayerMovement.dropInput = 0f;
		}
	}

	public void Attack()
	{
		if (playerAttack != null)
			playerAttack.PerformAttack();
	}
}
