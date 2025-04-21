using UnityEngine;
using ChaosFantasy.Models;

namespace ChaosFantasy.Controllers
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovementController : MonoBehaviour
    {
        public float moveSpeed = 5f;

        private Rigidbody2D rb;
        private Animator animator;
        private Vector2 moveInput;

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
        }

        void Update()
        {
            moveInput.x = Input.GetAxisRaw("Horizontal");
            moveInput.y = Input.GetAxisRaw("Vertical");
            moveInput.Normalize();

            // Cập nhật animator (View)
            if (animator)
            {
                if (moveInput != Vector2.zero)
                {
                    animator.SetFloat("isMoving", 1f);
                    animator.SetFloat("Speed", moveSpeed/5f);
                }
                else
                {
                    animator.SetFloat("isMoving", 0f);
                    animator.SetFloat("Speed", 1f);
                }
            }
        }

        void FixedUpdate()
        {
            rb.velocity = moveInput * moveSpeed;
        }
    }
}
