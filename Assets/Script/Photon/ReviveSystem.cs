using UnityEngine;

public class ReviveSystem : MonoBehaviour, IInteractable
{
    public CharacterHandler downedPlayer;
    public float clickInterval = 0f;
    public float percent = 0f;
    void Update()
    {
        if (clickInterval > 0f)
        {
            clickInterval -= Time.deltaTime;
        }
    }
    public void Interact(CharacterHandler user = null)
    {
        if (user == downedPlayer) return;
        
        if (user == null || downedPlayer == null || downedPlayer.isDowned == false) return;

        if (clickInterval > 0f)
        {
            return;
        }  
        clickInterval = 0.5f;
        percent += 0.25f;
        DungeonPickup.ShowPickup($"Reviving... {(int)(percent * 100)}%", downedPlayer.transform.position);

        if (percent >= 1f)
        {
            downedPlayer.photonView.RPC("RPC_Revive", downedPlayer.photonView.Owner);
            DungeonPickup.HidePickup();
            percent = 0f;
        }
       
    }

    public bool CanInteract() => downedPlayer != null && downedPlayer.isDowned;

    public void InRangeAction(CharacterHandler user = null)
    {
        if (user != downedPlayer)
        {
            DungeonPickup.ShowPickup("Revive", transform.position);
        }

    }

    public void CancelInRangeAction(CharacterHandler user = null)
    {
        if (user != downedPlayer)
        {
            DungeonPickup.HidePickup();
        } 
    }
}
