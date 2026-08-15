using Raccoon;
using UnityEngine;
using UnityEngine.Audio;

public class PlayerSoundFootStep : MonoBehaviour
{

    public float stepInterval = 0.45f;

    private float timer;

    bool playAudio = false;

    public void SetPlayAudio(bool playAudio)
    {
        this.playAudio = playAudio;
    }
    float scaleTime = 1;
    public void SetScaleTime(float scale)
    {
        scaleTime = scale;
    }

    void Update()
    {
        if (playAudio)
        {
            timer += Time.deltaTime;

            if (timer >= stepInterval/scaleTime)
            {
                timer = 0;
                // play
                ObserverEventManager.Instance.Publish<SoundType>(EventObserverName.PlaySfx.ToString(), SoundType.FootStep);

            }
        }
        else
        {
            timer = stepInterval;
        }
    }
}
