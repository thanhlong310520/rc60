using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public enum Sound
{
    Spawned,
    RagdollHit,
    AK47,
    Sniper,
    Shotgun,
    Pistol,
    Bow,
    RobotHit,
    SpawnedRagdoll,
    SpawnedRobot,
    TankT90,
    Cyberpunk,
    Click,
    Wall,
    Laser,
    Scifi,
    LaserLoop,
    SwordSlash,
    BlackHole,
    Catapult,
    AcidExplosion,
    Liquid,
    Tap
}

public class SoundManager : MonoSingleton<SoundManager>
{
    [System.Serializable]
    public class SoundTable
    {
        public Sound sound;
        public AudioClip clip;
    }

    //private Coroutine corChangeVolume;
    [SerializeField] AudioSource audioSourceNormal;
    [SerializeField] AudioSource audioSourceSpecial;

    [SerializeField] private SoundTable[] sounds;
    private Dictionary<Sound, AudioClip> soundDics = new Dictionary<Sound, AudioClip>();
    private Queue<Audio3D> queue3d = new Queue<Audio3D>();

    protected override void Awake()
    {
        base.Awake();
        foreach (var _s in sounds)
        {
            soundDics.Add(_s.sound, _s.clip);
        }
    }

    private void Start()
    {
        //audioSourceNormal = GetComponent<AudioSource>();
        audioSourceNormal.volume = PlayerPrefsOverride.GetFloat("VolumeNormal", 1.0f);
        audioSourceSpecial.volume = PlayerPrefsOverride.GetFloat("VolumeNormal", 1.0f);
        //StartCoroutine(TurnOnSoundAwake());

        queue3d.Clear();
    }

    public void Play(AudioClip audioClip, bool isActive = true, bool isLoop = false)
    {
        if (isActive)
        {
            DisableLoopSound();
            if (audioSourceNormal.isPlaying)
                audioSourceNormal.Stop();

            audioSourceNormal.time = 0;
            audioSourceNormal.loop = isLoop;
            audioSourceNormal.clip = audioClip;
            audioSourceNormal.Play();
        }
        else
            Stop();
    }

    public void PlaySpecial(AudioClip audioClip, bool isActive = true, bool isLoop = false)
    {
        if (isActive)
        {
            DisableLoopSound();
            if (audioSourceSpecial.isPlaying)
                audioSourceSpecial.Stop();

            audioSourceSpecial.time = 0;
            audioSourceSpecial.loop = isLoop;
            audioSourceSpecial.clip = audioClip;
            audioSourceSpecial.Play();
        }
        else
            audioSourceSpecial.Stop();
    }

    public void PlaySoundUpdateVolume(AudioClip audioClip, float minVolume = 0.4f, float maxVolume = 1f)
    {
        DisableLoopSound();
        Play(audioClip, true, false);
        float volume = minVolume;
        audioSourceNormal.volume = volume;
        DOTween.To(() => volume, x => volume = x, maxVolume, 3f)
            .OnUpdate(() => {
                audioSourceNormal.volume = volume;
            });
    }

    public void VolumeChange(float toVolume)
    {
        float volume = audioSourceNormal.volume;
        DOTween.To(() => volume, x => volume = x, toVolume, 3f)
        .OnUpdate(() => {
            audioSourceNormal.volume = volume;
            audioSourceSpecial.volume = volume;
        });
    }    


    private Coroutine LoopSound;

    private void DisableLoopSound()
    {
        if (LoopSound != null)
            StopCoroutine(LoopSound);
    }

    public void PlaySoundLoopWithTime(AudioClip audioClip, float startTime = 2f, float endTime = 4f)
    {
        DisableLoopSound();
        audioSourceNormal.loop = false;
        audioSourceNormal.clip = audioClip;
        audioSourceNormal.time = startTime;
        audioSourceNormal.Play();

        LoopSound = CoroutineUtils.PlayCoroutine(() =>
        {
            PlaySoundLoopWithTime(audioClip, startTime, endTime);
        }, endTime);
    }

    public void PlayOnShot(Sound sound, float volume = 1f)
    {
        AudioClip clip = ConvertToClip(sound);
        audioSourceNormal.PlayOneShot(clip);
    }

    public void PlayOnShot(AudioClip sound, float volume = 1f)
    {
        audioSourceNormal.PlayOneShot(sound);
    }

    public void PlayOneShotSpecial(Sound sound, float volume = 1f)
    {
        AudioClip clip = ConvertToClip(sound);
        audioSourceSpecial.PlayOneShot(clip);
    }
        

    public IEnumerator PlayOnShotCustom(Sound sound, float volume = 1f, float after = 0, int numberPlay = 1)
    {
        yield return WaitForSecondCache.GetWFSCache(after);
        for (int i = 0; i < numberPlay; i++)
        {
            AudioClip clip = ConvertToClip(sound);
            audioSourceNormal.PlayOneShot(clip);

            yield return WaitForSecondCache.GetWFSCache(clip.length);
        }
    }

    public void PlayLoopInfinity(Sound sound)
    {
        AudioClip clip = ConvertToClip(sound);
        audioSourceNormal.clip = clip;
        audioSourceNormal.Play();
    }

    public void Stop()
    {
        audioSourceNormal.clip = null;
        audioSourceNormal.Stop();
        DisableLoopSound();
    }

    public void PlaySoundWithPlaying(Sound sound)
    {
        if (audioSourceNormal.isPlaying == true)
            return;

        CoroutineUtils.PlayCoroutine(() =>
        {
            AudioClip clip = ConvertToClip(sound);
            audioSourceNormal.clip = clip;
            audioSourceNormal.Play();
        }, 0.2f);
    }    

    public void PlaySoundAsync(Sound sound)
    {
        if (!isPlayingAsync)
        {
            AudioClip clip = ConvertToClip(sound);
            float length = GetSoundLength(sound);
            StartCoroutine(PlaySoundWithUpdate(clip, length));
        }
    }

    public void PlaySoundAsyncWithDelay(Sound sound, float delay)
    {
        if (!isPlayingAsync)
        {
            AudioClip clip = ConvertToClip(sound);
            float length = GetSoundLength(sound);
            StartCoroutine(PlaySoundWithUpdate(clip, length + delay));
        }
    }

    private bool isPlayingAsync = false;
    IEnumerator PlaySoundWithUpdate(AudioClip clip, float length, float volume = 1f)
    {
        isPlayingAsync = true;
        audioSourceNormal.PlayOneShot(clip);
        yield return WaitForSecondCache.GetWFSCache(length);
        isPlayingAsync = false;
    }

    public void PlaySoundWithCounter(Sound sound, int counter)
    {
        AudioClip clip = ConvertToClip(sound);
        float length = GetSoundLength(sound);
        StartCoroutine(PlaySoundWithDelay(clip, length, counter));
    }

    IEnumerator PlaySoundWithDelay(AudioClip clip, float sLength, int counter)
    {
        int t = 0;
        while (t < counter)
        {
            audioSourceNormal.PlayOneShot(clip);
            t += 1;
            yield return WaitForSecondCache.GetWFSCache(sLength);
        }
    }

    AudioClip ConvertToClip(Sound sound)
    {
        if (soundDics.ContainsKey(sound))
            return soundDics[sound];
        return null;
    }

    public void Turn(bool isEnable)
    {
        audioSourceSpecial.mute = !isEnable;
        audioSourceNormal.mute = !isEnable;
    }

    public float GetSoundLength(Sound sound)
    {
        AudioClip clip = ConvertToClip(sound);
        return clip.length;
    }

    public IEnumerator PlayOnShotSpecial(Sound sound, float volume = 1f, float after = 0, int numberPlay = 1)
    {
        yield return WaitForSecondCache.GetWFSCache(after);
        for (int i = 0; i < numberPlay; i++)
        {
            AudioClip clip = ConvertToClip(sound);
            audioSourceSpecial.PlayOneShot(clip);
            yield return WaitForSecondCache.GetWFSCache(clip.length);
        }
    }

    public void PlayLoopSpecial(Sound sound)
    {
        AudioClip clip = ConvertToClip(sound);
        audioSourceSpecial.clip = clip;
        audioSourceSpecial.loop = false;
        audioSourceSpecial.Play();
    }

    public void PlayLoopSpecial(AudioClip clip)
    {
        audioSourceSpecial.clip = clip;
        audioSourceSpecial.loop = true;
        audioSourceSpecial.Play();
    }

    public void StopSpecial()
    {
        audioSourceSpecial.clip = null;
        audioSourceSpecial.Stop();
    }

    //private IEnumerator TurnOnSoundAwake()
    //{
    //    audioSourceNormal.enabled = false;
    //    audioSourceSpecial.enabled = false;
    //    yield return WaitForSecondCache.GetWFSCache(1f);
    //    audioSourceNormal.enabled = true;
    //    audioSourceSpecial.enabled = true;
    //}

    public void PlaySound(Sound id, float volumeMultiply = 1)
    {
        PlayOnShot(id, volumeMultiply);
    }

    public void PlaySoundAtLocation(Sound id, Vector3 worldPosition, float volumeMultiply = 1)
    {
        if (queue3d.Count == 0)
        {
            var _obj = new GameObject("AudioSource", typeof(Audio3D), typeof(AudioSource));
            _obj.transform.parent = transform;
            queue3d.Enqueue(_obj.GetComponent<Audio3D>());
        }

        var clip = ConvertToClip(id);
        var audio3D = queue3d.Dequeue();
        audio3D.SpawnAudio3D(clip, worldPosition, volumeMultiply,
            () =>
            {
                queue3d.Enqueue(audio3D);
            });
    }
}
