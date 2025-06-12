using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class A_DashSkill : SkillCardBase
{
    public float dashSpeed = 30f;
    public float dashDuration = 0.1f;
    private float interval;
    private void Update()
    {
        interval += Time.deltaTime;
    }
    public override void Activate(CharacterHandler player)
    {
        if (interval >= cooldown)
        {
            player.StartCoroutine(DashRoutine(player));
            player.GetComponent<PlayerController>()?.PlayDashAnimation();
            interval = 0;
        }
    }
   
    private IEnumerator DashRoutine(CharacterHandler player)
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
