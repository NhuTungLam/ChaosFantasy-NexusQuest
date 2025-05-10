using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviourPunCallbacks
{
    public float currentMoveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;

    private Camera mainCamera;
    public Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        mainCamera = Camera.main;

        if (!photonView.IsMine)
        {
            Destroy(rb); // tránh điều khiển đối tượng không phải của mình
            enabled = false;
        }
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput.Normalize();
        // Gán animation "isMoving"
        if (animator)
        {
            if (moveInput != Vector2.zero)
            {
                animator.SetFloat("isMoving", 1f);
                animator.SetFloat("Speed", currentMoveSpeed / 5f);
            }
            else
            {
                animator.SetFloat("isMoving", 0f);
                animator.SetFloat("Speed", 1f);
            }
        }

        RotatePlayerToMouse();
    }

    void FixedUpdate()
    {
        if (!photonView.IsMine) return;
        rb.MovePosition(rb.position + (moveInput * currentMoveSpeed * Time.fixedDeltaTime));
        Transform canvas = transform.Find("Canvas");
        if (canvas != null)
        {
            Vector3 scale = canvas.localScale;
            scale.x = Mathf.Abs(scale.x);
            canvas.localScale = scale;
        }
    }

    void RotatePlayerToMouse()
    {
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        // Kiểm tra hướng của chuột so với người chơi
        if (mousePos.x < transform.position.x)
        {
            transform.localScale = new Vector3(-1, 1, 1); 
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1); 
        }
        //
        

    }
    public void SetAnimator(RuntimeAnimatorController controller)
    {
        if (!animator)
            animator = GetComponent<Animator>();

        animator.runtimeAnimatorController = controller;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag.Equals("Enemy")){
            rb.velocity = Vector2.zero;       
        }
    }
}
