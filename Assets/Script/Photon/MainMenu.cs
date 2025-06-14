using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public static MainMenu Instance { get; private set; }
    public SceneController sceneController;
    private void Awake()
    {
        Instance = this;
    }
    public RectTransform LoginPanel, RegisterPanel, CharacterSelectionPanel;
    public RectTransform PlayerProfilePanel;

    public void StartAction()
    {
        if (PlayerProfileFetcher.CurrentProfile == null)
        {
            ShowLogin();
        }
        else
        {
            sceneController.StartGame();
        }
    }
    private void Start()
    {
        SettingPanel = GameObject.FindGameObjectWithTag("Setting").GetComponent<RectTransform>();
        SettingPanel.transform.Find("home").gameObject.SetActive(false);
        SettingPanel.transform.Find("close").GetComponent<Button>().onClick.RemoveAllListeners();
        SettingPanel.transform.Find("close").GetComponent<Button>().onClick.AddListener(() => HideSetting());
    }
    private RectTransform SettingPanel;
    public void ShowSetting()
    {
        if (SettingPanel != null)
            ShowPanel(SettingPanel);
    }
    public void HideSetting()
    {
        if (SettingPanel != null) 
            HidePanel(SettingPanel);
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
    public void ShowPlayerProfile(PlayerProfile profile = null)
    {
        if (profile == null)
        {
            PlayerProfilePanel.DOAnchorPosY(200, 1).SetEase(Ease.InCubic);   
        }
        else
        {
            PlayerProfilePanel.DOAnchorPosY(-50, 1).SetEase(Ease.OutCubic);
            PlayerProfilePanel.transform.Find("name").GetComponent<TextMeshProUGUI>().text= profile.username;
            PlayerProfilePanel.transform.Find("level").GetComponent<TextMeshProUGUI>().text = $"lvl {profile.level}";
            PlayerProfilePanel.transform.Find("gold").GetComponent<TextMeshProUGUI>().text = $"gold {profile.gold}";
        }
    }
    public void ShowLogin()
    {
        ShowPanel(LoginPanel);
    }
    public void HideLogin()
    {
        HidePanel(LoginPanel);
    }
    public void HideRegister()
    {
        HidePanel(RegisterPanel);
    }
    public void ShowCharacter()
    {
        ShowPanel(CharacterSelectionPanel);
    }
    public void HideCharacter()
    {
        HidePanel(CharacterSelectionPanel);
    }
    public void ShowRegisterFromLogin()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(HidePanel(LoginPanel));
        seq.AppendCallback(() => ShowPanel(RegisterPanel));
    }
    public void ShowLoginFromRegister()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(HidePanel(RegisterPanel));
        seq.AppendCallback(() => ShowPanel(LoginPanel));
    }

    /*public void ShowCharacterFromLogin()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(HidePanel(LoginPanel));
        seq.AppendCallback(() => ShowPanel(CharacterSelectionPanel));
    }*/

    private void ShowPanel(RectTransform panel)
    {


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
        seq.OnComplete(() => panel.anchoredPosition = new Vector2(-2000, -2000));
        return seq;
    }
}



