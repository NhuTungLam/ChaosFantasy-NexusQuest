using UnityEngine;

public class PickupActiveSkillCard : MonoBehaviour, IInteractable
{
    public ActiveSkillCard skillData;
    private bool isPicked = false;

    public bool CanInteract()
    {
        return !isPicked && skillData != null;
    }

    public void Interact(CharacterHandler user = null)
    {
        if (isPicked || skillData == null) return;

        CharacterHandler player = FindObjectOfType<CharacterHandler>();
        if (player != null)
        {
            player.SetActiveSkill(skillData);
            isPicked = true;
            Debug.Log($"[SkillPickup] Player picked up: {skillData.skillName}");

            Destroy(gameObject);
        }
    }
}
