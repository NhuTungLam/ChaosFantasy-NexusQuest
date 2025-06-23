using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using System.Collections;
using static UnityEngine.GraphicsBuffer;

public class BossEnemy : EnemyMovement
{
    public enum BossPattern
    {
        ChasePlayer,
        KeepDistance,
        GoToCenter
    }

    public BossPattern currentPattern = BossPattern.ChasePlayer;
    private Animator animator;

    private float attackCD;
    private float switchInterval;
    private bool isMoving;
    private void SignalMovingChange(bool canMove)
    {
        isMoving = canMove;
        if (isMoving)
        {
            animator.Play("boss_walk");
        }
        else
        {
            string idle = Random.Range(0, 2) == 0 ? "boss_idle_1" : "boss_idle_2";
            animator.Play(idle);
        }
    }

    //strafe
    private float moveSpeed = 1.5f;
    private float preferredDistance = 4f;

    //center
    private float centerThreshold = 0.2f;
    private Vector2 roomCenter = Vector2.zero;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        roomCenter = transform.position;
    }
    protected override void Attack()
    {
        if (attackCD >= 0)
        {
            attackCD -= Time.deltaTime;
        }
        switch (currentPattern)
        {
            case BossPattern.ChasePlayer:
                switchInterval -= Time.deltaTime;
                if (player == null || transform == null) break;
                if (attackCD < 0)
                {
                    StartCoroutine(Pattern1Seq(player, switchInterval <= 0));
                    attackCD = enemyHandler.enemyData.AtkSpeed;
                }
                break;
            case BossPattern.KeepDistance:
                switchInterval -= Time.deltaTime;
                if (attackCD < 0)
                {
                    if (player != null)
                    {
                        var dir = player.position - transform.position;
                        StartCoroutine(Pattern2Seq(dir, switchInterval <= 0));
                        attackCD = enemyHandler.enemyData.AtkSpeed;
                    }
                }
                break;
            case BossPattern.GoToCenter:
                if (!IsInCenter()) break;
                switchInterval -= Time.deltaTime;
                if (attackCD < 0)
                {
                    StartCoroutine(Pattern3Seq(switchInterval <= 0));
                    attackCD = enemyHandler.enemyData.AtkSpeed;
                }
                break;
        }
    }
    private IEnumerator Pattern1Seq(Transform target, bool toChangePattern)
    {
        int i = 8;
        SignalMovingChange(false);
        while (i > 0)
        {
            Vector2 dir = (target.position - transform.position).normalized;
            float speed = 8f;

            photonView.RPC("RPC_FireProjectile", RpcTarget.All,
                "boss_fireball",
                transform.position,
                dir,
                speed,
                1f,
                enemyHandler.currentDamage,
                2
            );

            i--;
            yield return new WaitForSeconds(0.15f);
        }
        if (toChangePattern)
            SignalPatternChange();
        SignalMovingChange(true);
        yield return null;
    }
    private IEnumerator Pattern2Seq(Vector2 dir, bool toChangePattern)
    {
        dir.Normalize();
        SignalMovingChange(false);

        float speed = 6f;

        photonView.RPC("RPC_FireProjectile", RpcTarget.All,
            "boss_firepillar_base",
            transform.position,
            dir,
            speed,
            2f,
            enemyHandler.currentDamage,
            2
        );

        yield return new WaitForSeconds(0.2f);
      
        if (toChangePattern)
            SignalPatternChange();
        SignalMovingChange(true);
        yield return null;
    }
    private IEnumerator Pattern3Seq(bool toChangePattern)
    {
        float speed = 6f;
        float delay = 1f;

        // First burst - orthogonal directions
        Vector2[] orthogonalDirs = new Vector2[]
        {
        Vector2.up,
        Vector2.down,
        Vector2.left,
        Vector2.right
        };

        foreach (var dir in orthogonalDirs)
        {
            photonView.RPC("RPC_FireProjectile", RpcTarget.All,
                "boss_slash",
                transform.position,
                dir,
                speed,
                1.5f,
                enemyHandler.currentDamage,
                2
            );
        }

        yield return new WaitForSeconds(delay);

        // Second burst - diagonal directions
        Vector2[] diagonalDirs = new Vector2[]
        {
        new Vector2(1, 1).normalized,
        new Vector2(-1, 1).normalized,
        new Vector2(1, -1).normalized,
        new Vector2(-1, -1).normalized
        };

        foreach (var dir in diagonalDirs)
        {
            photonView.RPC("RPC_FireProjectile", RpcTarget.All,
                "boss_slash",
                transform.position,
                dir,
                speed,
                1.5f,
                enemyHandler.currentDamage,
                2
            );
        }

        if (toChangePattern)
            SignalPatternChange();
    }

    public void SignalPatternChange()
    {
        // Change to a random new pattern different from the current one
        BossPattern newPattern;
        do
        {
            newPattern = (BossPattern)Random.Range(0, 3);
        } while (newPattern == currentPattern);

        currentPattern = newPattern;

        SignalMovingChange(true);
        switchInterval = 10f;
    }

    protected override void Movement()
    {
        if (player != null)
        {
            var playerDir = player.position - transform.position; 
            if (playerDir.x < 0)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
            else if (playerDir.x > 0)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
        }

        if (!isMoving) return;

        Vector2 targetPos = transform.position;

        switch (currentPattern)
        {
            case BossPattern.ChasePlayer:
                if (player != null)
                    targetPos = player.position;
                break;

            case BossPattern.KeepDistance:
                if (player != null)
                {
                    Vector2 toPlayer = player.position - transform.position;
                    float distance = toPlayer.magnitude;

                    // Too close, move away
                    if (distance < preferredDistance * 0.8f)
                        targetPos = (Vector2)transform.position - toPlayer.normalized;

                    // Too far, move closer
                    else if (distance > preferredDistance * 1.2f)
                        targetPos = player.position;

                    // In range, strafe perpendicular
                    else
                    {
                        Vector2 perp = Vector2.Perpendicular(toPlayer.normalized);
                        float direction = Mathf.Sin(Time.time * 2f); // oscillate left/right
                        targetPos = (Vector2)transform.position + perp * direction;
                    }
                }
                break;

            case BossPattern.GoToCenter:
                if (IsInCenter() && isMoving)
                {
                    SignalMovingChange(false);
                }
                targetPos = roomCenter;
                break;
        }

        // Move toward target
        Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
        rb.MovePosition(rb.position + dir * moveSpeed * Time.fixedDeltaTime);

        if (dir.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (dir.x > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    public bool IsInCenter()
    {
        float distanceToCenter = Vector2.Distance(transform.position, roomCenter);
        return distanceToCenter <= centerThreshold;
    }
}
