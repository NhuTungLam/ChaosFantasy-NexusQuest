using UnityEngine;

public class DashSkill : ActiveSkillBase
{
    public float dashSpeed = 30f;
    public float dashDuration = 0.1f;

    public override void Activate(CharacterHandler player)
    {
        player.StartCoroutine(DashRoutine(player)); 
        player.GetComponent<PlayerController>()?.PlayDashAnimation();
    }

    private System.Collections.IEnumerator DashRoutine(CharacterHandler player)
    {
        Vector2 dir = player.movement.GetMoveDirection();

        if (dir == Vector2.zero)
            dir = Vector2.right;

        float originalSpeed = player.movement.currentMoveSpeed;

        player.SetInvincible(true);
        player.isDashing = true;
        player.dashDirection = dir.normalized;
        player.movement.currentMoveSpeed = dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        player.SetInvincible(false);
        player.isDashing = false;
        player.movement.currentMoveSpeed = originalSpeed;
    }
}
