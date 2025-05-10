using UnityEngine;

public class Pickup : MonoBehaviour, IKnockback
{

    protected CharacterHandler target;
    protected float speed;
    
    Vector3 knockbackVelocity;
    float knockbackDuration;

    [Header("Bonuses")]
    public int expGranted;
    public int healthGranted;

    public virtual bool Collect(CharacterHandler target, float speed)
    {
        if (!this.target)
        {
            this.target = target;
            this.speed = speed;
            Knockback(10f, 0.1f);
            return true;
        }
        return false;
    }

    protected virtual void Update()
    {
        // If there is target for the pick-up to move toward
        if (target)
        {
            if (knockbackDuration > 0)
            {
                transform.position += knockbackVelocity * Time.deltaTime;
                knockbackDuration -= Time.deltaTime;
            }
            else
            {
                // Move toward that target
                Vector2 distance = target.transform.position - transform.position;
                // Check the distance between the pick-up and the target, destroy pick-up if they touch each other
                // Using square instead of square root because it is faster
                // Delta time is accounting for the lag because we are running this every frame
                if (distance.sqrMagnitude > speed * speed * Time.deltaTime)
                {
                    transform.position += speed * Time.deltaTime * (Vector3)distance.normalized;
                } else
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    // Now this will be used for subsequent types of pick-ups like exp, health
    protected virtual void OnDestroy()
    {
        if (!target) return;
    }

    public void Knockback(float force, float duration)
    {
        Vector3 direction = transform.position - target.transform.position;

        knockbackVelocity = direction.normalized * force;
        knockbackDuration = duration;
    }
}
