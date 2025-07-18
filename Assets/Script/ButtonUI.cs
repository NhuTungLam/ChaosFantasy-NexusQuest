using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[AddComponentMenu("UI/ButtonUI")]
public class ButtonUI : Selectable,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    [Serializable]
    public class DragEvent : UnityEvent<PointerEventData> { }

    [Serializable]
    public class PointerEvent : UnityEvent<PointerEventData> { }

    [Header("Drag Events")]
    public DragEvent onBeginDrag = new DragEvent();
    public DragEvent onDrag = new DragEvent();
    public DragEvent onEndDrag = new DragEvent();

    [Header("Hover Events")]
    public PointerEvent onPointerEnter = new PointerEvent();
    public PointerEvent onPointerExit = new PointerEvent();

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!IsActive() || !IsInteractable())
            return;

        onBeginDrag?.Invoke(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!IsActive() || !IsInteractable())
            return;

        onDrag?.Invoke(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!IsActive() || !IsInteractable())
            return;

        onEndDrag?.Invoke(eventData);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData); // Retain highlight visuals
        if (!IsActive() || !IsInteractable()) return;
        onPointerEnter?.Invoke(eventData);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData); // Retain unhighlight visuals
        if (!IsActive() || !IsInteractable()) return;
        onPointerExit?.Invoke(eventData);
    }

}
