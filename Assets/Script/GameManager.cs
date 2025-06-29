using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    private bool isPaused = false;

    private RectTransform SettingPausePanel;
    private void Start()
    {
        SettingPausePanel = GameObject.FindGameObjectWithTag("Setting").GetComponent<RectTransform>();
        SettingPausePanel.transform.Find("home").gameObject.SetActive(true);
        SettingPausePanel.transform.Find("home").GetComponent<Button>().onClick.RemoveAllListeners();
        SettingPausePanel.transform.Find("home").GetComponent<Button>().onClick.AddListener(OnExitToNexusButton);
        SettingPausePanel.transform.Find("close").GetComponent<Button>().onClick.RemoveAllListeners();
        SettingPausePanel.transform.Find("close").GetComponent<Button>().onClick.AddListener(OnResumeButton);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    public void TogglePauseMenu()
    {
        isPaused = !isPaused;
        //pauseMenuUI.SetActive(isPaused);
        Time.timeScale = isPaused ? 0 : 1;
        ShowPanel(SettingPausePanel);
    }

    public void OnResumeButton()
    {
        isPaused = false;
        HidePanel(SettingPausePanel);
        Time.timeScale = 1;
    }

    public void OnExitToNexusButton()
    {
        OnResumeButton();
        StartCoroutine(LeaveRoomAndLoadNexus());
    }

    private IEnumerator LeaveRoomAndLoadNexus()
    {
        Time.timeScale = 1; // Reset nếu đang pause
        BlackScreen.Instance.BlackIn();
        yield return new WaitForSecondsRealtime(1.2f);
        DungeonRestorerManager.Instance.ResetState();
        // ✅ Save progress trước khi rời phòng
        if (PlayerManager.Instance != null)
        {
            Transform myPlayer = PlayerManager.Instance.GetMyPlayer();
            if (myPlayer != null && PlayerProfileFetcher.CurrentProfile != null )
            {
                if (RoomSessionManager.Instance.IsRoomOwner())
                {
                    MessageBoard.Show("Saving progress...");
                    yield return StartCoroutine(DungeonApiClient.Instance.SaveProgressAfterSpawn(myPlayer,PlayerManager.Instance.GetOtherPlayer()));
                }
                else 
                
                {
                    int ownerId = PlayerManager.Instance.GetOwnerPlayerId();
                    if (ownerId == -1)
                    {
                        MessageBoard.Show("Owner in guest mode, cannot save");
                    }
                    else
                        StartCoroutine(DungeonApiClient.Instance.LoadDungeonProgress(ownerId, (progressid) =>
                        {
                            DungeonRestorerManager.Instance.dungeoninfo = null;
                            var myid = PlayerProfileFetcher.CurrentProfile.userId;
                            var mydto = PlayerManager.Instance.GetPlayerProgress(myid);
                            
                            StartCoroutine(DungeonApiClient.Instance.SaveTeammateProgress(myid, progressid, mydto));
                        }));
                }    
            }
            else
            {
                MessageBoard.Show("Saving is not available in Guest mode");
            }
        }

        // ✅ Rời phòng
        if (PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.LocalPlayer.TagObject is GameObject go)
            {
                PhotonNetwork.Destroy(go);
                PhotonNetwork.LocalPlayer.TagObject = null;
            }

            PhotonNetwork.LeaveRoom();
            while (PhotonNetwork.InRoom)
                yield return null;

            yield return new WaitForSeconds(0.2f);
        }

        PhotonRoomManager.autoCreateRoom = true;
        Debug.Log("🔁 Returning to Nexus...");
        SceneManager.LoadScene("Nexus");
    }

    private void OnApplicationQuit()
    {
        if (PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.LocalPlayer.TagObject is GameObject go)
            {
                PhotonNetwork.Destroy(go);
                PhotonNetwork.LocalPlayer.TagObject = null;
            }

            PhotonNetwork.LeaveRoom();

        }
    }


    [Header("Animation Settings")]
    public float duration = 0.5f;
    public float offsetY = 150f;
    private CanvasGroup GetOrAddCanvasGroup(RectTransform rt)
    {
        var cg = rt.GetComponent<CanvasGroup>();
        if (cg == null) cg = rt.gameObject.AddComponent<CanvasGroup>();
        return cg;
    }
    private void ShowPanel(RectTransform panel)
    {
        var cg = GetOrAddCanvasGroup(panel);
        cg.interactable = true;
        cg.alpha = 0;
        panel.anchoredPosition = new Vector2(0, -300);

        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true);

        seq.Append(cg.DOFade(1, duration));
        seq.Join(panel.DOAnchorPosY(offsetY, duration).SetEase(Ease.OutCubic));
    }

    private Tween HidePanel(RectTransform panel)
    {
        var cg = GetOrAddCanvasGroup(panel);
        cg.interactable = false;
        panel.anchoredPosition = new Vector2(0, offsetY);

        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true);

        seq.Append(cg.DOFade(0, duration));
        seq.Join(panel.DOAnchorPosY(panel.anchoredPosition.y + 100, duration).SetEase(Ease.InCubic));
        seq.OnComplete(() => panel.anchoredPosition= new Vector2(-2000,-2000));
        return seq;
    }
}
