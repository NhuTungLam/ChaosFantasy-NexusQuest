using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>(); // THÊM DÒNG NÀY
    }

    void Update()
    {
        Vector2 move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        animator.SetFloat("Speed", move == Vector2.zero?0:1);
        animator.SetFloat("isMoving", move == Vector2.zero ? 0 : 1);

        // Xử lý Flip khi sang trái
        if (move.x > 0)
            spriteRenderer.flipX = false;
        else if (move.x < 0)
            spriteRenderer.flipX = true;
    }
}
