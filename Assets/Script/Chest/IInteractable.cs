public interface IInteractable 
{
    void Interact(CharacterHandler player=null);
    bool CanInteract();
    void InRangeAction(CharacterHandler player=null);
    void CancelInRangeAction(CharacterHandler player=null);
}
