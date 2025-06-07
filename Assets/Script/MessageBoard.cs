using UnityEngine;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;

public class MessageBoard : MonoBehaviour
{
    public static MessageBoard Instance;

    [Header("References")]
    public RectTransform messageContainer; // Parent Canvas or Panel (anchor top)
    public GameObject messagePrefab;       // The TextMeshProUGUI prefab

    [Header("Settings")]
    public float messageDuration = 2f;
    public float fadeDuration = 0.5f;
    public float verticalSpacing = 40f;

    private List<RectTransform> activeMessages = new();

    void Awake()
    {
        Instance = this;
    }
    private void Start()
    {

    }
    public static void Show(string text)
    {
        Instance?.SpawnMessage(text);
    }

    private void SpawnMessage(string message)
    {
        GameObject msgObj = Instantiate(messagePrefab, messageContainer);
        RectTransform msgRect = msgObj.GetComponent<RectTransform>();
        TextMeshProUGUI tmp = msgObj.GetComponentInChildren<TextMeshProUGUI>();
        CanvasGroup cg = msgObj.GetComponent<CanvasGroup>();

        tmp.text = message;

        // Set initial position (under top, depending on count)
        float yOffset = -activeMessages.Count * verticalSpacing;
        msgRect.anchoredPosition = new Vector2(0, yOffset);
        cg.alpha = 0;

        activeMessages.Add(msgRect);

        // Entry animation
        Sequence seq = DOTween.Sequence();
        seq.Append(cg.DOFade(1, fadeDuration));
        seq.Join(msgRect.DOAnchorPosY(yOffset + 10f, fadeDuration));

        // Stay for duration
        seq.AppendInterval(messageDuration);

        // Exit animation
        seq.Append(cg.DOFade(0, fadeDuration));
        seq.Join(msgRect.DOAnchorPosY(yOffset + 30f, fadeDuration));

        // Cleanup
        seq.OnComplete(() =>
        {
            activeMessages.Remove(msgRect);
            Destroy(msgObj);
            RepositionMessages();
        });
    }

    private void RepositionMessages()
    {
        for (int i = 0; i < activeMessages.Count; i++)
        {
            RectTransform rect = activeMessages[i];
            float targetY = -i * verticalSpacing;

            rect.DOAnchorPosY(targetY, 0.3f).SetEase(Ease.OutQuad);
        }
    }
}
