public interface IInteractable 
{
    void Interact(CharacterHandler player=null);
    bool CanInteract();
}
