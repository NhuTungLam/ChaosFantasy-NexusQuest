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

        CharacterHandler player = FindObjectOfType<CharacterHandler>();
        if (player != null && skillData != null)
        {
            player.ApplyPassiveSkill(skillData);
            isPicked = true;
            Destroy(gameObject);
        }
    }
}
