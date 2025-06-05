using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviourPunCallbacks, IMovementController
{

    public float currentMoveSpeed { get; set; } = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 lastMoveDirection = Vector2.right;

    private Camera mainCamera;
    public Animator animator;

    void Start()
    {
        

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        mainCamera = Camera.main;


    }

    void Update()
    {
        if (!photonView.IsMine) return;

        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput.Normalize();

        if (moveInput != Vector2.zero)
        {
            lastMoveDirection = moveInput;
        }

        if (animator)
        {
            animator.SetFloat("isMoving", moveInput != Vector2.zero ? 1f : 0f);
            animator.SetFloat("Speed", currentMoveSpeed / 5f);
        }

        RotatePlayerToMouse();
    }

    void FixedUpdate()
    {
        if (!photonView.IsMine) return;

        var handler = GetComponent<CharacterHandler>();
        Vector2 move = moveInput * currentMoveSpeed;

        if (handler != null && handler.isDashing)
        {
            move = handler.dashDirection * handler.dashSpeed; 
        }
        else
        {
            move = moveInput * currentMoveSpeed;
        }


        rb.MovePosition(rb.position + (move * Time.fixedDeltaTime));

        Transform canvas = transform.Find("Canvas");
        if (canvas != null)
        {
            Vector3 scale = canvas.localScale;
            scale.x = Mathf.Abs(scale.x);
            canvas.localScale = scale;
        }
    }

    public Vector2 GetMoveDirection()
    {
        return lastMoveDirection;
    }

    void RotatePlayerToMouse()
    {
        if (!photonView.IsMine) return;

        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        transform.localScale = new Vector3(mousePos.x < transform.position.x ? -1 : 1, 1, 1);
    }


    public void SetAnimator(RuntimeAnimatorController controller)
    {
        if (!animator)
            animator = GetComponent<Animator>();

        animator.runtimeAnimatorController = controller;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            rb.velocity = Vector2.zero;
        }
    }
    public void PlayDashAnimation()
    {
        if (animator) animator.SetTrigger("Dash");
    }

    public void PlayDieAnimation()
    {
        if (animator) animator.SetTrigger("Die");
    }

}
