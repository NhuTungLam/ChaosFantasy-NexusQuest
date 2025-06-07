using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MainMenu : MonoBehaviour
{
    public static MainMenu Instance { get; private set; }
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
            ShowCharacter();
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
    public void ShowPlayerProfile(PlayerProfile profile = null)
    {
        PlayerProfilePanel.gameObject.SetActive(profile != null);
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

    public void ShowCharacterFromLogin()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(HidePanel(LoginPanel));
        seq.AppendCallback(() => ShowPanel(CharacterSelectionPanel));
    }

    private void ShowPanel(RectTransform panel)
    {
        panel.gameObject.SetActive(true);

        var cg = GetOrAddCanvasGroup(panel);
        cg.alpha = 0;
        panel.anchoredPosition = new Vector2(0, -300);

        Sequence seq = DOTween.Sequence();
        seq.Append(cg.DOFade(1, duration));
        seq.Join(panel.DOAnchorPosY(offsetY, duration).SetEase(Ease.OutCubic));
    }

    private Tween HidePanel(RectTransform panel)
    {
        var cg = GetOrAddCanvasGroup(panel);
        panel.anchoredPosition = new Vector2(0, offsetY);

        Sequence seq = DOTween.Sequence();
        seq.Append(cg.DOFade(0, duration));
        seq.Join(panel.DOAnchorPosY(panel.anchoredPosition.y + 100, duration).SetEase(Ease.InCubic));
        seq.OnComplete(() => panel.gameObject.SetActive(false));
        return seq;
    }
}



