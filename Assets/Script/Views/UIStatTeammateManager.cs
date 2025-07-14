using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class UIStatTeammateManager : MonoBehaviour
{
    public static UIStatTeammateManager Instance;
    private void Awake()
    {
        Instance = this;
    }

    public float uiTransitDuration = 0.5f;
    public List<RectTransform> teammateUIs = new List<RectTransform>();
    private Queue<RectTransform> standbyQueue = new Queue<RectTransform>();
    private List<RectTransform> inUseList = new List<RectTransform>();

    // Start is called before the first frame update
    void Start()
    {
        foreach (var t in teammateUIs)
        {
            t.anchoredPosition = new Vector2(-500, 0); // off screen
            standbyQueue.Enqueue(t);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public static RectTransform Assign()
    {
        if (Instance.standbyQueue.Count == 0)
        {
            Debug.LogWarning("No more object queued!");
            return null;
        }
        var rt = Instance.standbyQueue.Dequeue();
        Instance.inUseList.Add(rt);
        var queue = Instance.inUseList.Count - 1;
        Instance.Entrance(rt, queue);

        return rt;
    }
    public static void UnAssign(RectTransform rt)
    {
        if (!Instance.inUseList.Contains(rt))
        {
            Debug.LogWarning("Duplicate call on 1 object!");
            return;
        }

        Instance.inUseList.Remove(rt);
        Instance.standbyQueue.Enqueue(rt);

        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true);
        seq.AppendCallback(() => Instance.Exit(rt));
        seq.AppendInterval(Instance.uiTransitDuration);
        seq.AppendCallback(() => Instance.Reposition());
    }
    private void Entrance(RectTransform rt, int queue)
    {
        float yPos = -120f * queue;
        rt.anchoredPosition = new Vector2(-500, yPos);
        rt.DOAnchorPosX(0, uiTransitDuration).SetUpdate(true);
    }
    private void Exit(RectTransform rt)
    {
        rt.DOAnchorPosX(-500, uiTransitDuration).SetUpdate(true);
    }
    private void Reposition()
    {
        for (int i = 0; i < inUseList.Count; i++)
        {
            float targetY = -120f * i;
            inUseList[i].DOKill();
            inUseList[i].DOAnchorPosY(targetY, uiTransitDuration).SetUpdate(true);
        }
    }
}
