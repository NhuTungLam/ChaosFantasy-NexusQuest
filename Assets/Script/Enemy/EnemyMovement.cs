using Photon.Pun;
using UnityEngine;

public class EnemyMovement : MonoBehaviourPun
{
    protected Transform player;
    protected EnemyHandler enemyHandler;
    protected Rigidbody2D rb;

    public float detectionRange = 5f;

    // Internal caching
    private Transform cachedPlayer;
    private float playerCacheTime = -1f;

    private void Start()
    {
        enemyHandler = GetComponent<EnemyHandler>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (!photonView.IsMine) return;
        if (!this || !transform) return;

        player = GetCachedPlayer();
        if (player == null) return;

        Attack();
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine) return;
        if (!this || !transform) return;

        player = GetCachedPlayer();
        if (player == null) return;

        Movement();
    }

    /// <summary>
    /// Only checks for player once per frame
    /// </summary>
    private Transform GetCachedPlayer()
    {
        if (Time.frameCount != playerCacheTime)
        {
            cachedPlayer = PlayerManager.Instance.GetPlayer(transform.position, detectionRange);
            playerCacheTime = Time.frameCount;
        }
        return cachedPlayer;
    }

    protected virtual void Attack() { }
    protected virtual void Movement() { }
}
