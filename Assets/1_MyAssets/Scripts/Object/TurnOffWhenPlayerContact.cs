using Raccoon.Controller;
using UnityEngine;

public class TurnOffWhenPlayerContact : MonoBehaviour
{

    public bool playerContact = false;
    public bool isTurnOff = false;

    public float timePlayerIn = 2f;
    float currentTimePlayerIn = 0f;

    public float timeToTurnOff = 3f;
    float currentTimeToTurnOff = 0f;

    public Renderer rend;
    public Collider colli;
    private void Start()
    {
        playerContact = false;
        isTurnOff = false;
    }
    private void Reset()
    {
        rend = GetComponent<Renderer>();
        colli = GetComponent<Collider>();
    }
    private void Update()
    {
        if (playerContact)
        {
            CountTimePlayerIn();
        }
        if (isTurnOff)
        {
            CountTimeReset();
        }
    }

    void CountTimePlayerIn()
    {
        if(currentTimePlayerIn < timePlayerIn)
        {
            currentTimePlayerIn += Time.deltaTime;
        }
        else
        {
            TurnOff();
        }
    }
    void CountTimeReset()
    {
        if (currentTimeToTurnOff < timeToTurnOff)
        {
            currentTimeToTurnOff += Time.deltaTime;
        }
        else
        {
            TurnOn();
        }
    }
    void TurnOff()
    {
        rend.enabled = false;
        colli.enabled = false;
        isTurnOff = true;
        playerContact = false;
    }   
    void TurnOn()
    {
        rend.enabled = true;
        colli.enabled = true;
        isTurnOff = false;
        ResetTime();
    }

    public void ResetTime()
    {
        currentTimePlayerIn = 0f;
        currentTimeToTurnOff = 0f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform == PlayerController.instance.transform)
        {
            playerContact = true;
            ResetTime();

        }
    }
    //private void OnCollisionExit(Collision collision)
    //{
    //    if (collision.transform == PlayerController.instance.transform)
    //    {
    //        playerContact = false;
    //        ResetTime();
    //    }
    //}
}
