using System;

[Serializable]
public class PlayerProfile
{
    public string userId;              
    public string username;
    public string password;
    public string @class;               
    public int level = 1;            
    public int exp = 0;              
    public int gold = 0;            
    public int winCount = 0;        
    public int totalMatches = 0;      
    public float totalPlayTime = 0f;   
}
