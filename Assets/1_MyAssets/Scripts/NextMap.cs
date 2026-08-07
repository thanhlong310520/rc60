using Raccoon.Controller;
using UnityEngine;
using UnityEngine.Events;

public class NextMap : MonoBehaviour
{
    public Transform holder;
    Transform user;


    public float timeDelayOpen = 0.5f;
    protected float currentTime = 0f;

    protected bool waitOpen = false;


    protected UnityAction<bool> ActionShowWaitOpen;
    protected UnityAction<float> ActionChangeAmountWait;


    protected virtual void Start()
    {
        waitOpen = false;
        user = PlayerController.instance.transform;
    }
    public virtual void HandleStartInteract(Transform contactor)
    {
        if (contactor != user) { return; }

        Debug.Log("start contact " + contactor);
        waitOpen = true;
        currentTime = 0;
        ShowUIWait(true);
    }

    public virtual void HandleStopInteract(Transform contactor)
    {

        if (contactor != user) { return; }
        Debug.Log("end contact " + contactor);
        if (!waitOpen) return;
        waitOpen = false;
        ShowUIWait(false);
    }

    public virtual void Raise()
    {
        ChangeAmountWait(currentTime, timeDelayOpen);
        currentTime += Time.deltaTime;
        if (currentTime > timeDelayOpen)
        {
            waitOpen = false;
            ShowUIWait(false);

            // Thực hiện hành động mở map tiếp theo

            GamePlayController.instance.NextLevel();
        }

    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (waitOpen)
        {
            Raise();
        }
    }

    void ShowUIWait(bool isShow)
    {
        ActionShowWaitOpen?.Invoke(isShow);
    }
    void ChangeAmountWait(float curent, float max)
    {
        float amount = Mathf.Clamp01(curent / max);
        ActionChangeAmountWait?.Invoke(amount);
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleStartInteract(other.transform);
    }

    private void OnTriggerExit(Collider other)
    {
        HandleStopInteract(other.transform);
    }


    private void OnDisable()
    {
        waitOpen = false;
    }
    private void OnEnable()
    {

    }
}
