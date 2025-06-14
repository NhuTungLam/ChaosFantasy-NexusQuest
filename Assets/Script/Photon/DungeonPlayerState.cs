namespace DungeonSystem
{
    [System.Serializable]
    public class DungeonPlayerState
    {
        public string userId;    
        public string currentCard;  
        public string currentWeapon; 
        public string currentClass; 

        public float hp;          
        public float mana;     

        public int stageLevel;     
    }
}
