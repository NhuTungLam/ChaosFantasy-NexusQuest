using UnityEngine;

// Ensuring that whenever this class is initiated, component CircleCollider2D is also initiated for this class
[RequireComponent(typeof(CircleCollider2D))]
public class PlayerCollector : MonoBehaviour
{
    private CharacterHandler player;
    private CircleCollider2D detector;
    public float pullSpeed;

    void Start()
    {
        // Can change to GetComponentInParent for multiplayer
        CharacterSelector selector = FindObjectOfType<CharacterSelector>();

    }

    public void SetRadius(float r)
    {
        if (!detector)
        {
            detector = GetComponent<CircleCollider2D>();
        }
        detector.radius = r;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the collided gameObject has the class Pickup
        if (collision.gameObject.TryGetComponent(out Pickup collectible))
        {
            // Call that object Collect method
            collectible.Collect(player, pullSpeed);
        }
    }
}
