using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NexusChangeCharacter : MonoBehaviour, IInteractable
{
    public static NexusChangeCharacter Instance;
    public CharacterData[] availableCharacters;
    private int currentCharIndex = 0;
    private PlayerController playerController;
    public TextMeshProUGUI charSelTitle;
    public Image charSelPortrait;
    public CharacterData CurrentCharacterData;
    public RectTransform CharacterChangePanel;
    public RectTransform PlayerProfilePanel;
    //public RectTransform RoomJoinUI;
    [Header("Animation Settings")]
    public float duration = 0.5f;
    public float offsetY = 150f;

    public DamagePopUp popUpPrefab;
    private CharacterHandler currentUser;
    void Awake()
    {
        Instance = this;
        ObjectPools.SetupPool(popUpPrefab, 10, "DamagePopUp");
    }
    void Start()
    {
        currentCharIndex = 0;
        ChangeCurrentClass();

        SettingPausePanel = GameObject.FindGameObjectWithTag("Setting").GetComponent<RectTransform>();
        SettingPausePanel.transform.Find("home").gameObject.SetActive(true);
        SettingPausePanel.transform.Find("home").GetComponent<Button>().onClick.RemoveAllListeners();
        SettingPausePanel.transform.Find("home").GetComponent<Button>().onClick.AddListener(OnExitToNexusButton);
        SettingPausePanel.transform.Find("close").GetComponent<Button>().onClick.RemoveAllListeners();
        SettingPausePanel.transform.Find("close").GetComponent<Button>().onClick.AddListener(OnResumeButton);

        ShowPlayerProfile(PlayerProfileFetcher.CurrentProfile);
    }
    private bool hasExited = false;
    void OnExitToNexusButton()
    {
        if (hasExited) return;
        hasExited = true;
        //Time.timeScale = 1f;
        OnResumeButton();
        StartCoroutine(LeaveRoom());
    }
    IEnumerator LeaveRoom()
    {
        BlackScreen.Instance.BlackIn();
        PhotonRoomManager.autoCreateRoom = false;

        if (PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.LocalPlayer.TagObject is GameObject go)
            {
                PhotonNetwork.Destroy(go);
                PhotonNetwork.LocalPlayer.TagObject = null;
            }

            PhotonNetwork.LeaveRoom();

            float timeout = 5f;
            while (PhotonNetwork.InRoom && timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }
        }
        float waitMasterTimeout = 5f;
        while (PhotonNetwork.NetworkClientState != ClientState.ConnectedToMasterServer && waitMasterTimeout > 0f)
        {
            waitMasterTimeout -= Time.deltaTime;
            yield return null;
        }

        MessageBoard.Show("Leaving Nexus...");
        SceneManager.LoadScene("Login");
    }
    // also add the setting 
    public bool CanInteract()
    {
        return true;
    }
    public void Interact(CharacterHandler user = null)
    {
        currentUser = user;
        ShowCharacter();
        playerController = user.transform.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.CanMove = false;
        }
    }
    public void ShowCharacter()
    {
        ShowPanel(CharacterChangePanel);
    }
    public void HideCharacter()
    {
        HidePanel(CharacterChangePanel);
        currentUser = null;
    }
    public void InRangeAction(CharacterHandler user = null) 
    {
        DungeonPickup.ShowPickup("Change Character", transform.position);
    }
    public void CancelInRangeAction(CharacterHandler user = null)
    {
        DungeonPickup.HidePickup();
    }

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
        seq.OnComplete(() => panel.anchoredPosition = new Vector2(-2000, -2000));
        return seq;
    }

    public void GoLeft()
    {
        currentCharIndex--;
        if (currentCharIndex < 0)
            currentCharIndex = availableCharacters.Length - 1;
        ChangeCurrentClass();
    }
    public void GoRight()
    {
        currentCharIndex++;
        if (currentCharIndex >= availableCharacters.Length)
            currentCharIndex = 0;
        ChangeCurrentClass();
    }
    public void ChangeCurrentClass()
    {
        CurrentCharacterData = availableCharacters[currentCharIndex];
        charSelTitle.text = CurrentCharacterData.name;
        charSelPortrait.sprite = CurrentCharacterData.PlayerSprite;
    }
    public void SelectClass()
    {
        currentUser.Init(CurrentCharacterData);
        CharacterSelector.Instance.characterData = CurrentCharacterData;
        HideCharacter();
        if (playerController != null)
        {
            playerController.CanMove = true;
            playerController = null;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    private RectTransform SettingPausePanel;
    private bool isPaused = false;
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
    public void ShowPlayerProfile(PlayerProfile profile = null)
    {
        if (profile == null)
        {
            PlayerProfilePanel.DOAnchorPosY(200, 1).SetEase(Ease.InCubic).SetUpdate(true);
        }
        else
        {
            PlayerProfilePanel.DOAnchorPosY(-50, 1).SetEase(Ease.OutCubic).SetUpdate(true);
            PlayerProfilePanel.transform.Find("name").GetComponent<TextMeshProUGUI>().text = profile.username;
            PlayerProfilePanel.transform.Find("level").GetComponent<TextMeshProUGUI>().text = $"lvl {profile.level}";
            PlayerProfilePanel.transform.Find("gold").GetComponent<TextMeshProUGUI>().text = $"gold {profile.gold}";
        }
    }
}
