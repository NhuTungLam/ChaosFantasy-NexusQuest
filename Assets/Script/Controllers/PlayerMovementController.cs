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

                animator.SetBool("isMoving", moveInput != Vector2.zero);
            }
        }

        void FixedUpdate()
        {
            rb.velocity = moveInput * moveSpeed;
        }
    }
}
