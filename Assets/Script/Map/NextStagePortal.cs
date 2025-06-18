using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class NextStagePortal : MonoBehaviour, IInteractable
{
    private bool hasInteracted = false;
    

    public bool CanInteract()
    {
        return !hasInteracted;
    }

    public void Interact(CharacterHandler player = null)
    {
        hasInteracted = true;
        var roomOwnerId = PhotonNetwork.CurrentRoom.CustomProperties["roomOwner"]?.ToString();
        if (PhotonNetwork.LocalPlayer.UserId == roomOwnerId)
        {
            StartCoroutine(NextStageSeq(player));
            MessageBoard.Show("Going Deeper...");
        }
        else
        {
            MessageBoard.Show("Wait for the room owner to enter...");
        }
    }
    private IEnumerator NextStageSeq(CharacterHandler player)
    {
        BlackScreen.Instance.BlackIn();
        yield return new WaitForSecondsRealtime(1f);
        DungeonGenerator.Instance.GenerateDungeon();
        DungeonGenerator.Instance.stageLevel += 1;
        if (PlayerManager.Instance != null)
        {
            Transform myPlayer = PlayerManager.Instance.GetMyPlayer();
            if (myPlayer != null && PlayerProfileFetcher.CurrentProfile != null)
            {
                MessageBoard.Show("Saving progress...");
                yield return StartCoroutine(DungeonApiClient.Instance.SaveProgressAfterSpawn(myPlayer));
            }
            else
            {
                MessageBoard.Show("Saving is not available in Guest mode");
            }

            //TODO: saving other players' progress
            DungeonSyncManager.Instance.photonView.RPC("RPC_SpawnRoomPrefab", RpcTarget.Others, DungeonGenerator.Instance.SaveLayout());
            foreach (var p in PlayerManager.Instance.playerList)
            {
                p.position = new Vector2(Random.Range(-3f, 3f), Random.Range(-3f, 3f));
            }
        }
        yield return new WaitForSecondsRealtime(1f);
        BlackScreen.Instance.BlackOut();
        Destroy(gameObject);
    }
    public void InRangeAction(CharacterHandler user = null)
    {
        DungeonPickup.ShowPickup("Next Stage", transform.position);
    }
    public void CancelInRangeAction(CharacterHandler user = null)
    {
        DungeonPickup.HidePickup();
    }
}
