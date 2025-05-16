using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    protected Transform player;
    protected EnemyHandler enemyHandler;
    protected Rigidbody2D rb;
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        enemyHandler = GetComponent<EnemyHandler>();
        rb = GetComponent<Rigidbody2D>();

    }
    private void Update()
    {
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
