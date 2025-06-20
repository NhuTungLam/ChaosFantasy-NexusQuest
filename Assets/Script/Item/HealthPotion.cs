using UnityEngine;

public class HealthPotion : MonoBehaviour, IInteractable
{
    public int healAmount = 30;
    private bool isPicked = false;

    public bool CanInteract()
    {
        return !isPicked;
    }

    public void Interact(CharacterHandler user = null)
    {
        if (isPicked) return;

        CharacterHandler player = FindObjectOfType<CharacterHandler>();
        if (player != null)
        {
            player.TakeDamage(-healAmount);

            Debug.Log($"[HealthPotion] Player healed +{healAmount} HP");

            isPicked = true;
            Destroy(gameObject);
        }
    }
    public void InRangeAction(CharacterHandler user = null)
    {
        DungeonPickup.ShowPickup("HP potion", transform.position);
    }
    public void CancelInRangeAction(CharacterHandler user = null)
    {
        DungeonPickup.HidePickup();
    }
}
