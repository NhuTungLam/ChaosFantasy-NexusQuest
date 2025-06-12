using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NexusChangeCharacter : MonoBehaviour, IInteractable
{
    public static NexusChangeCharacter Instance;
    public CharacterData[] availableCharacters;
    private int currentCharIndex = 0;

    public TextMeshProUGUI charSelTitle;
    public Image charSelPortrait;
    public CharacterData CurrentCharacterData;
    public RectTransform CharacterChangePanel;
    //public RectTransform RoomJoinUI;
    [Header("Animation Settings")]
    public float duration = 0.5f;
    public float offsetY = 150f;

    private CharacterHandler currentUser;
    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        currentCharIndex = 0;
        ChangeCurrentClass();

        SettingPausePanel = GameObject.FindGameObjectWithTag("Setting").GetComponent<RectTransform>();
        SettingPausePanel.transform.Find("home").gameObject.SetActive(true);
        SettingPausePanel.transform.Find("home").GetComponent<Button>().onClick.RemoveAllListeners();
        //SettingPausePanel.transform.Find("home").GetComponent<Button>().onClick.AddListener(OnExitToNexusButton);
        SettingPausePanel.transform.Find("close").GetComponent<Button>().onClick.RemoveAllListeners();
        SettingPausePanel.transform.Find("close").GetComponent<Button>().onClick.AddListener(OnResumeButton);
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
        panel.gameObject.SetActive(true);

        var cg = GetOrAddCanvasGroup(panel);
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
        panel.anchoredPosition = new Vector2(0, offsetY);

        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true);
        seq.Append(cg.DOFade(0, duration));
        seq.Join(panel.DOAnchorPosY(panel.anchoredPosition.y + 100, duration).SetEase(Ease.InCubic));
        seq.OnComplete(() => panel.gameObject.SetActive(false));
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
}
