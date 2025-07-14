using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class UIBounce : MonoBehaviour
{
    public RectTransform imageToMove;    // Assign your Image RectTransform
    public Image mask;
    public float moveDuration = 50f;
    public float padding = 20f;          // Distance to keep from edges

    private RectTransform canvasRect;

    private void Start()
    {
        canvasRect = imageToMove.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        StartBounce();
        StartAlpha();
    }

    void StartBounce()
    {
        float imageWidth = imageToMove.rect.width;
        float canvasWidth = canvasRect.rect.width;

        float minX = -canvasWidth / 2f + imageWidth / 2f + padding;
        float maxX = canvasWidth / 2f - imageWidth / 2f - padding;

        Sequence seq = DOTween.Sequence()
            .Append(imageToMove.DOAnchorPosX(maxX, moveDuration).SetEase(Ease.InOutSine))
            .Append(imageToMove.DOAnchorPosX(minX, moveDuration).SetEase(Ease.InOutSine))
            .SetLoops(-1, LoopType.Yoyo);
    }
    void StartAlpha()
    {
        Color startColor = mask.color;
        startColor.a = 0f;
        mask.color = startColor;

        mask.DOFade(1f, 5f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
}
