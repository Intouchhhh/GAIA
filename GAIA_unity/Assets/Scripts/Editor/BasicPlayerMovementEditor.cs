using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BasicPlayerMovement))]
public class BasicPlayerMovementEditor : Editor
{
	private bool showMovementSettings = false;
	private bool showJumpSettings = false;
	private bool showWallJumpSettings = false;
	private bool showDashSettings = false;
	private bool showChecks = false;
	private bool showSerialize = false;
	private bool showInputs = false;
	private bool showFlags = false;

	public override void OnInspectorGUI()
	{
		BasicPlayerMovement script = (BasicPlayerMovement)target;

		EditorGUI.BeginChangeCheck();

		showMovementSettings = EditorGUILayout.Foldout(showMovementSettings, "Movement Settings");
		if (showMovementSettings)
		{
			EditorGUILayout.PropertyField(serializedObject.FindProperty("moveSpeed"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("acceleration"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("decceleration"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("velPower"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("gravityScale"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("frictionAmount"));
		}

		showJumpSettings = EditorGUILayout.Foldout(showJumpSettings, "Jump Settings");
		if (showJumpSettings)
		{
			EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpForce"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("coyoteTime"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpBufferTime"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpHangGravityMultiplier"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("fallMultiplier"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("maxFallSpeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpCooldown"));
        }

		showWallJumpSettings = EditorGUILayout.Foldout(showWallJumpSettings, "Wall Jump Settings");
		if (showWallJumpSettings)
		{
			EditorGUILayout.PropertyField(serializedObject.FindProperty("wallJumpForce"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("wallJumpLerp"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("wallSlideSpeed"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_wallLayer"));
		}

		showDashSettings = EditorGUILayout.Foldout(showDashSettings, "Dash Settings");
		if (showDashSettings)
		{
			EditorGUILayout.PropertyField(serializedObject.FindProperty("dashSpeed"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("dashDuration"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("dashCooldown"));
		}

		showChecks = EditorGUILayout.Foldout(showChecks, "Checks");
		if (showChecks)
		{
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_wallCheckPointLeft"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_wallCheckPointRight"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_wallCheckSize"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_groundCheckPoint"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_groundCheckSize"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_playerLayer"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_groundLayer"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_oneWayPlatLayer"));
		}

		showSerialize = EditorGUILayout.Foldout(showSerialize, "Serialize");
		if (showSerialize)
		{
			EditorGUILayout.PropertyField(serializedObject.FindProperty("rb"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("lastGroundedTime"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("lastJumpPressedTime"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("wallJumpDir"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("lastDashTime"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("lastDashPressedTime"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("dashDirection"));
		}

		showInputs = EditorGUILayout.Foldout(showInputs, "Inputs (Public)");
		if (showInputs)
		{
			EditorGUILayout.PropertyField(serializedObject.FindProperty("moveInput"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("dropInput"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("moveDir"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpInput"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("dashInput"));
		}

		showFlags = EditorGUILayout.Foldout(showFlags, "Flags");
		if (showFlags)
		{
			EditorGUILayout.PropertyField(serializedObject.FindProperty("isGrounded"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("isOnRightWall"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("isOnLeftWall"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("isOnWall"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("isDashing"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("isJumping"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("isDropping"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("isWallJumping"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("isFacingRight"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("canJump"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("canWallJump"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("hasJumped"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("hasWallJumped"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("wasGrounded"));
        }

		serializedObject.ApplyModifiedProperties();
	}
}
