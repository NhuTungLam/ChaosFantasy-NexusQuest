using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradePanel : MonoBehaviour
{
    private static UpgradePanel Instance;
    private enum Stat
    {
        Hp = 0,
        Might = 1,
        Haste = 2,
        Crit = 3
    }

    private void Awake()
    {
        Instance = this;
        ResetAllStat();

        cells[Stat.Hp] = hpCells;
        cells[Stat.Might] = mightCells;
        cells[Stat.Haste] = hasteCells;
        cells[Stat.Crit] = critCells;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            ShowUpgrade(5);
        }
    }
    [Header("References")]
    [SerializeField] private List<Button> plusButton = new List<Button>();
    [SerializeField] private Button refreshButton;
    [SerializeField] private List<Image> icons = new List<Image>();
    [SerializeField] private TextMeshProUGUI pointTxt;

    [SerializeField] private List<Image> hpCells = new();
    [SerializeField] private List<Image> mightCells = new();
    [SerializeField] private List<Image> hasteCells = new();
    [SerializeField] private List<Image> critCells = new();

    [Header("Sprites")]
    [SerializeField] private Sprite normalCell; 
    [SerializeField] private Sprite filledCell;

    private int maxPoint;
    private int currentPoint;
    private Dictionary<Stat, int> currentStats = new Dictionary<Stat, int>();
    private Dictionary<Stat, List<Image>> cells = new();
    public static void ShowUpgrade(int point)
    {
        Instance.maxPoint = point;
        Instance.currentPoint = point;
        Instance.pointTxt.color = Color.green;
        Instance.pointTxt.text = "" + point;
        for (int i = 0; i < 4; i++)
        {
            int index = i;
            Instance.plusButton[index].onClick.RemoveAllListeners();
            Instance.plusButton[index].onClick.AddListener(() => Instance.Upgrade((Stat)index));
        }
        foreach (var item in Instance.icons)
        {
            item.DOKill();
            Instance.StartHueCycle(item);
        }
        Instance.refreshButton.onClick.RemoveAllListeners();
        Instance.refreshButton.onClick.AddListener(() => Instance.ResetAllStat());

        GameManager.Instance.ShowUpgradePanel();
    }

    private void Upgrade(Stat stat)
    {
        if (currentPoint == 0)
        {
            pointTxt.DOColor(Color.red, 0.1f)
                .OnComplete(() => pointTxt.DOColor(Color.white, 0.1f));
            return;
        }

        int index = currentStats[stat];
        cells[stat][index].sprite = filledCell;

        index++;
        currentStats[stat] = index;

        currentPoint--;
        pointTxt.text = "" + currentPoint;
        if (currentPoint == 0)
        {
            pointTxt.color = Color.white;
        }
    }
    private void ResetAllStat()
    {
        currentStats[Stat.Hp] = 0;
        currentStats[Stat.Haste] = 0;
        currentStats[Stat.Might] = 0;
        currentStats[Stat.Crit] = 0;
        foreach (var item in cells)
        {
            foreach (var cell in item.Value)
            {
                cell.sprite = normalCell;
            }
        }
        pointTxt.text = "" + maxPoint;
        currentPoint = maxPoint;
        pointTxt.color = Color.green;
    }

    private void StartHueCycle(Image img)
    {
        float hue = 0f;
        DOTween.To(() => hue, x => {
            hue = x;
            Color rgb = Color.HSVToRGB(hue % 1f, 0.5f, 1f);
            img.color = rgb;
        }, 1f, 5f)
        .SetLoops(-1, LoopType.Restart)
        .SetEase(Ease.Linear);
    }
}
