using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool defeated = false;
    public void Awake()
    {
        Instance = this;
    }
    private bool isPaused = false;

    private RectTransform SettingPausePanel;
    private RectTransform SummaryPanel;
    private int finalGold;
    private int finalExp;

    //public RectTransform UpgradeStatPanel;

    private void Start()
    {
        SettingPausePanel = GameObject.FindGameObjectWithTag("Setting").GetComponent<RectTransform>();
        SettingPausePanel.transform.Find("home").gameObject.SetActive(true);
        SettingPausePanel.transform.Find("home").GetComponent<Button>().onClick.RemoveAllListeners();
        SettingPausePanel.transform.Find("home").GetComponent<Button>().onClick.AddListener(OnExitToNexusButton);
        SettingPausePanel.transform.Find("close").GetComponent<Button>().onClick.RemoveAllListeners();
        SettingPausePanel.transform.Find("close").GetComponent<Button>().onClick.AddListener(OnResumeButton);

        SummaryPanel = GameObject.FindGameObjectWithTag("SummaryPanel").GetComponent<RectTransform>();
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
        Time.timeScale = 1;
        BlackScreen.Instance.BlackIn();
        yield return new WaitForSecondsRealtime(1.2f);
        DungeonRestorerManager.Instance.ResetState();

        MessageBoard.Show("Leaving dungeon...");

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


    public void ShowSummaryPanel()
    {
        defeated=true;
        int kills = PlayerStatTracker.Instance?.enemyKillCount ?? 0;
        int deaths = PlayerStatTracker.Instance?.deathCount ?? 0;
        int rooms = 1; // Replace with actual cleared room count if available

        finalGold = PlayerProfileFetcher.Instance.CalculateGold(
            new DungeonApiClient.PlayerProgressDTO { enemyKills = kills, deathCount = deaths }, rooms);

        finalExp = PlayerProfileFetcher.Instance.CalculateExp(
            new DungeonApiClient.PlayerProgressDTO { enemyKills = kills, deathCount = deaths }, rooms);

        SummaryPanel.transform.Find("enemy_kill").GetComponent<TextMeshProUGUI>().text = $"{kills}";
        SummaryPanel.transform.Find("room_clear").GetComponent<TextMeshProUGUI>().text = $"{rooms}";
        SummaryPanel.transform.Find("death_count").GetComponent<TextMeshProUGUI>().text = $"{deaths}";
        SummaryPanel.transform.Find("gold_earn").GetComponent<TextMeshProUGUI>().text = $"{finalGold}";
        SummaryPanel.transform.Find("exp_earn").GetComponent<TextMeshProUGUI>().text = $"{finalExp}";

        var returnBtn = SummaryPanel.transform.Find("return_nexus").GetComponent<Button>();
        returnBtn.onClick.RemoveAllListeners();
        returnBtn.onClick.AddListener(() => StartCoroutine(SendRewardAndReturn()));

        ShowPanel(SummaryPanel);
    }

    private IEnumerator SendRewardAndReturn()
    {
        Time.timeScale = 1;

        PlayerProfileFetcher.UpdateReward(finalGold, finalExp);
        PlayerProfileFetcher.Instance.UpdateProfile();

        yield return StartCoroutine(LeaveRoomAndLoadNexus());
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

        DG.Tweening.Sequence seq = DG.Tweening.DOTween.Sequence();
        seq.SetUpdate(true);

        seq.Append(cg.DOFade(1, duration));
        seq.Join(panel.DOAnchorPosY(offsetY, duration).SetEase(Ease.OutCubic));
    }

    private Tween HidePanel(RectTransform panel)
    {
        var cg = GetOrAddCanvasGroup(panel);
        cg.interactable = false;
        panel.anchoredPosition = new Vector2(0, offsetY);

        DG.Tweening.Sequence seq = DG.Tweening.DOTween.Sequence();
        seq.SetUpdate(true);

        seq.Append(cg.DOFade(0, duration));
        seq.Join(panel.DOAnchorPosY(panel.anchoredPosition.y + 100, duration).SetEase(Ease.InCubic));
        seq.OnComplete(() => panel.anchoredPosition = new Vector2(-2000, -2000));
        return seq;
    }
}
