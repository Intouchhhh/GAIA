using UnityEngine;

public class ActionModules : MonoBehaviour
{
    public PlayerMovement playerMovement;

    public void Jump()
    {
        playerMovement.OnJumpInput();
    }

    public void JumpRelease()
    {
        playerMovement.OnJumpUpInput();
    }

    public void Dash()
    {
        playerMovement.OnDashInput();
    }

    public void Drop()
    {
        playerMovement.HandleDropThroughPlatform();
    }
}
