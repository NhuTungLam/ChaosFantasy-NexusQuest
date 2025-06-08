using UnityEngine;

public class PickupPassiveSkill : MonoBehaviour, IInteractable
{
    public PassiveSkillCard skillData;
    private bool isPicked = false;

    public bool CanInteract()
    {
        return !isPicked && skillData != null;
    }

    public void Interact()
    {
        if (isPicked) return;
        isPicked = true; // ??t ngay ? ??u ?? tránh double trigger

        CharacterHandler player = FindObjectOfType<CharacterHandler>();
        if (player != null && skillData != null)
        {
            player.ApplyPassiveSkill(this); // Apply vào ng??i ch?i

            // Disable collider ?? không va ch?m l?i n?a
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            player.ClearCurrentInteractable(this); // Xóa kh?i h? th?ng t??ng tác hi?n t?i

        }
    }
}
