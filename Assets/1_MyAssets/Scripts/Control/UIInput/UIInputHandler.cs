using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UIInputHandler : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IDragHandler
{
    [Header("Rotation")]
    public float rotationSpeed = 1f;
    public float dragThreshold = 5f;

    private Vector2 lastPos;
    private Vector2 eventPos;

    private bool dragged;

    private float pressTime;

    private float mouseX;
    private float mouseY;

    // Finger đang điều khiển
    private int activePointerId = -999;

    public UnityEvent<float, float> rotateOutputEvent;
    public UnityEvent clickOutputEvent;

    // =========================================
    // POINTER DOWN
    // =========================================
    public void OnPointerDown(PointerEventData eventData)
    {
        // Touch mới luôn giành quyền
        activePointerId = eventData.pointerId;

        mouseX = 0;
        mouseY = 0;

        dragged = false;

        pressTime = Time.time;

        lastPos = eventData.position;
        eventPos = eventData.position;
    }

    // =========================================
    // DRAG
    // =========================================
    public void OnDrag(PointerEventData eventData)
    {
        // Ignore finger cũ
        if (eventData.pointerId != activePointerId)
            return;

        eventPos = eventData.position;

        Vector2 delta = eventPos - lastPos;

        // Chỉ tính drag khi đủ lớn
        if (delta.magnitude > 3f)
            dragged = true;
    }

    // =========================================
    // POINTER UP
    // =========================================
    public void OnPointerUp(PointerEventData eventData)
    {
        // Ignore finger cũ
        if (eventData.pointerId != activePointerId)
            return;

        mouseX = 0;
        mouseY = 0;

        float holdTime = Time.time - pressTime;

        if (!dragged && holdTime < 0.3f)
        {
            clickOutputEvent?.Invoke();
        }

        dragged = false;

        // Không trả quyền cho finger cũ
        activePointerId = -999;
    }

    // =========================================
    // UPDATE
    // =========================================
    private void Update()
    {
        if (activePointerId == -999)
        {
            mouseX = 0;
            mouseY = 0;

            rotateOutputEvent?.Invoke(0, 0);
            return;
        }

        if (dragged)
        {
            Vector2 delta = eventPos - lastPos;

            // ========================= X
            float deltaX = ApplyThreshold(delta.x);

            deltaX *= rotationSpeed * 3000f / Screen.width;

            mouseX = Mathf.Lerp(mouseX, deltaX, 0.25f);

            // ========================= Y
            float deltaY = ApplyThreshold(delta.y);

            deltaY *= rotationSpeed * 1500f / Screen.height;

            mouseY = Mathf.Lerp(mouseY, deltaY, 0.25f);

            lastPos = eventPos;
        }
        else
        {
            mouseX = 0;
            mouseY = 0;
        }

        rotateOutputEvent?.Invoke(mouseX, mouseY);
    }

    // =========================================
    // THRESHOLD
    // =========================================
    private float ApplyThreshold(float value)
    {
        if (value > 0)
        {
            value -= dragThreshold;

            if (value < 0)
                value = 0;
        }
        else
        {
            value += dragThreshold;

            if (value > 0)
                value = 0;
        }

        return value;
    }
}