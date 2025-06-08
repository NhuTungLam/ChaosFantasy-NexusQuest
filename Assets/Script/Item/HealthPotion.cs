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
            player.currentHealth += healAmount;
            if (player.currentHealth > player.characterData.MaxHealth)
                player.currentHealth = player.characterData.MaxHealth;

            Debug.Log($"[HealthPotion] Player healed +{healAmount} HP");

            isPicked = true;
            Destroy(gameObject);
        }
    }
}
