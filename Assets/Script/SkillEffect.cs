using UnityEngine;

public class SkillEffect : MonoBehaviour
{
    public float duration = 1f;
    public float damage = 20f;

    void Start()
    {
        Destroy(gameObject, duration);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy")) // hoặc thử với Player
        {
            Debug.Log("Skill hit for " + damage);
        }
    }
}
