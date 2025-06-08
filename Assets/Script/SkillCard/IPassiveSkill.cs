public interface IPassiveSkill
{
    void Initialize(CharacterHandler player);
    void Tick(); // call each frame in update

}
