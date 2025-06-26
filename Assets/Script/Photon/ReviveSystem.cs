using UnityEngine;

public class ReviveSystem : MonoBehaviour, IInteractable
{
    public CharacterHandler downedPlayer;
    public float holdTimer = 0f;
    private bool isInteracting = false;

    public void Interact(CharacterHandler user = null)
    {
        if (user == downedPlayer) return;
        
        if (user == null || downedPlayer == null || downedPlayer.isDowned == false) return;

        isInteracting = true;

        holdTimer += Time.deltaTime;
        float percent = holdTimer / downedPlayer.reviveHoldTime;
        DungeonPickup.ShowPickup($"Reviving... {(int)(percent * 100)}%", downedPlayer.transform.position);

        if (percent >= 1f)
        {
            downedPlayer.photonView.RPC("RPC_Revive", downedPlayer.photonView.Owner);
            holdTimer = 0f;
            DungeonPickup.HidePickup();
            isInteracting = false;
        }
       
    }

    public bool CanInteract() => downedPlayer != null && downedPlayer.isDowned;

    public void InRangeAction(CharacterHandler user = null)
    {
        if (user != downedPlayer)
        {
            DungeonPickup.ShowPickup("Hold R to Revive", transform.position);
        }

    }

    public void CancelInRangeAction(CharacterHandler user = null)
    {
        if (user != downedPlayer)
        {
            DungeonPickup.HidePickup();
            holdTimer = 0f;
            isInteracting = false;
        } 
    }
}
