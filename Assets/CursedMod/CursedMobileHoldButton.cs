using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// A held mobile action is released on pointer-up/cancel, not when a finger
/// drifts a few pixels outside the button.
/// </summary>
public sealed class CursedMobileHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, ICancelHandler
{
    public InputAction action;
    private int activePointerId = int.MinValue;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (activePointerId != int.MinValue) return;
        activePointerId = eventData.pointerId;
        CursedMobileInput.SetTouchAction(action, true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId == activePointerId) Release();
    }

    public void OnCancel(BaseEventData eventData)
    {
        Release();
    }

    private void OnDisable()
    {
        Release();
    }

    private void Release()
    {
        if (activePointerId == int.MinValue) return;
        CursedMobileInput.SetTouchAction(action, false);
        activePointerId = int.MinValue;
    }
}
