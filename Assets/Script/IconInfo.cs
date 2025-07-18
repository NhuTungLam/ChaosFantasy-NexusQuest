using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IconInfo : MonoBehaviour
{
    private static IconInfo Instance;
    [SerializeField] RectTransform infoPanel;
    [SerializeField] TextMeshProUGUI title;
    [SerializeField] TextMeshProUGUI description;

    [SerializeField] RectTransform canvasRectTransform; // Assign the root canvas
    [SerializeField] Camera uiCamera; // Assign the camera used by the Canvas

    private void Awake()
    {
        Instance = this;
    }
    public static void Assign(ButtonUI bt, SkillCardBase skillCard)
    {
        bt.onPointerEnter.AddListener((p) =>
        {
            Show(skillCard);
        });
        bt.onPointerExit.AddListener((p) => Hide());
    }
    private static void Show(SkillCardBase skillCard)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            Instance.canvasRectTransform,
            Input.mousePosition,
            Instance.uiCamera,
            out var localPoint
        );
        Show(skillCard.CardName, skillCard.CardDescription, localPoint);
    }
    public static void Show(string title, string description, Vector2 position)
    {
        Instance.infoPanel.anchoredPosition = position;
        Instance.title.text = title;
        Instance.description.text = description;
    }
    public static void Hide()
    {
        Instance.infoPanel.anchoredPosition = new Vector2(-2000, 0);
    }
}
