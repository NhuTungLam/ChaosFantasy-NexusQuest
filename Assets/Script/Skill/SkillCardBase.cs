using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillCardBase : MonoBehaviour,IInteractable
{
    [Header("Data card")]
    public bool isActiveCard = false;
    public string CardName;
    public Sprite Icon;
    public string CardDescription;
    public float cooldown = 2f;
    [Header("Runtime Data")]
    public bool hasPick = false;


    public virtual void Initialize(CharacterHandler player)
    {
        hasPick = true;
        if (isActiveCard == true)
        {
            player.SetActiveSkill(this);
        }
        else
        {
            player.SetPassiveSkill(this);
        }
    }
    public virtual void Activate(CharacterHandler player) 
    { 
    }
    public void Interact(CharacterHandler player) 
    {
        Initialize(player);
    }
    public bool CanInteract()
    {
        return !hasPick;
    }
    public void InRangeAction(CharacterHandler user = null)
    {
        DungeonPickup.ShowPickup(CardName, transform.position);
    }
    public void CancelInRangeAction(CharacterHandler user = null)
    {
        DungeonPickup.HidePickup();
    }
}
