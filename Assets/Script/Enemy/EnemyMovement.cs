using Photon.Pun;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class EnemyMovement : MonoBehaviourPun
{
    
    protected Transform player;
    protected EnemyHandler enemyHandler;
    protected Rigidbody2D rb;

    private void Start()
    {
        
        enemyHandler = GetComponent<EnemyHandler>();
        rb = GetComponent<Rigidbody2D>();

    }
    private void Update()
    {
        if (!photonView.IsMine) return;
        if (!this || !transform) return;
        player = PlayerManager.Instance.GetPlayer(transform.position);
        if (player == null) return;

        Attack();
        Movement();
    }
    protected virtual void Attack()
    {

    }
    protected virtual void Movement()
    {
    } 
}
