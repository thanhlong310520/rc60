using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Music
{
    Login,
    Main,
}

public class MusicManager : MonoSingletonGlobal<MusicManager>
{
    [System.Serializable]
    public class MusicTable
    {
        public Music music;
        public AudioClip clip;
    }

    [SerializeField] private MusicTable[] musics;
    [SerializeField] AudioSource audioSource;
    private Dictionary<Music, AudioClip> musicDics = new Dictionary<Music, AudioClip>();

    protected override void Awake()
    {
        base.Awake();
        foreach (var _s in musics)
        {
            musicDics.Add(_s.music, _s.clip);
        }
    }

    private IEnumerator Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        audioSource.mute = !RuntimeStorageData.Sound.isSound;

        if (musics.Length == 0)
            yield break;

        PlaySound(Music.Login);
    }

    public void PlaySound(Music sound, float _volume = 1.0f)
    {
        PauseSound();
        audioSource.clip = ConverToClip(sound);
        audioSource.loop = true;
        audioSource.volume = _volume;
        audioSource.Play();
    }

    public void PauseSound()
    {
        audioSource.Pause();
    }

    public void UnPauseSound()
    {
        audioSource.UnPause();
    }

    AudioClip ConverToClip(Music sound)
    { 
        if (musicDics.ContainsKey(sound))
            return musicDics[sound];
        return null;
    }

    public void Turn(bool isEnble)
    {
        audioSource.mute = !isEnble;
    }
}
