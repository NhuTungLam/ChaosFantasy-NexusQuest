using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class QuickTutorial : MonoBehaviour, IInteractable
{
    public Transform target; // Assign your target Transform
    public float zoomInScale = 1.5f;
    public float zoomOutScale = 1f;
    public float duration = 0.3f;

    private Tween zoomTween;
    public void CancelInRangeAction(CharacterHandler player = null)
    {
        // Kill any existing tween but don't reset the scale
        zoomTween?.Kill();
        zoomTween = target.DOScale(zoomOutScale, duration).SetEase(Ease.OutCubic).SetUpdate(true);
    }

    public bool CanInteract()
    {
        return true;
    }

    public void InRangeAction(CharacterHandler player = null)
    {
        zoomTween?.Kill();
        zoomTween = target.DOScale(zoomInScale, duration).SetEase(Ease.OutCubic).SetUpdate(true);
    }

    public void Interact(CharacterHandler player = null)
    {

    }

}
