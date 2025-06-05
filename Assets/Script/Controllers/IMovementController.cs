using UnityEngine;

public interface IMovementController
{
    Vector2 GetMoveDirection();
    float currentMoveSpeed { get; set; }
    void SetAnimator(RuntimeAnimatorController controller);
    void PlayDashAnimation();
    void PlayDieAnimation();

}
