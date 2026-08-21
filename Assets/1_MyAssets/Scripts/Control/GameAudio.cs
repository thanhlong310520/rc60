using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Raccoon
{
    /// <summary>
    /// Manager script to play audio on different channels, two sounds can't play on the same chanel.
    /// Useful to avoid having sounds play on top of each other
    /// </summary>

    public class GameAudio : MonoBehaviour
    {
        private static GameAudio instance;
        public static GameAudio Get => instance;
        
        private bool isSoundEnabled = true;

        private Dictionary<string, AudioSource> channels_sfx = new Dictionary<string, AudioSource>();
        private Dictionary<string, AudioSource> channels_music = new Dictionary<string, AudioSource>();
        private Dictionary<string, float> channels_volume = new Dictionary<string, float>();

        void Awake()
        {
            if(instance != null)
            {
                Destroy(gameObject); return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        //channel: Two sounds on the same channel will never play at the same time, sounds on different channel will play at the same time.
        //priority: if false, will not play if a sound is already playing on the channel, if true, will replace current sound playing on channel
        public void PlaySFX(string channel, AudioClip sound, float vol = 0.7f, bool priority = true, bool loop = false)
        {
            if (string.IsNullOrEmpty(channel) || sound == null)
                return;

            AudioSource source = GetChannel(channel);
            // float volume = PlayerData.Get.sfx_volume;
            float volume = 1;
            channels_volume[channel] = vol;

            if (source == null)
            {
                source = CreateChannel(channel); //Create channel if doesnt exist, for optimisation put the channel in preload_channels so its created at start instead of here
                channels_sfx[channel] = source;
            }

            if (source != null)
            {
                if (priority || !source.isPlaying)
                {
                    source.clip = sound;
                    source.volume = vol * volume;
                    source.loop = loop;
                    source.Play();
                }
            }
        }

        //channel: Two sounds on the same channel will never play at the same time, sounds on different channel will play at the same time.
        //If music is already playing on the same channel, new music will be played unless its the same one.(Won't restart in that case)
        public void PlayMusic(string channel, AudioClip music, float vol = 0.11f, bool loop = true)
        {
            if (string.IsNullOrEmpty(channel) || music == null)
                return;

            AudioSource source = GetMusicChannel(channel);
            float volume = 1;

            channels_volume[channel] = vol;

            if (source == null)
            {
                source = CreateChannel(channel); //Create channel if doesnt exist, for optimisation put the channel in preload_channels so its created at start instead of here
                channels_music[channel] = source;
            }

            if (source != null)
            {
                if (!source.isPlaying || source.clip != music)
                {
                    source.clip = music;
                    source.volume = vol * volume;
                    source.loop = loop;
                    source.Play();
                }
            }
        }
        
        public void PauseMusic(string channel)
        {
            if (string.IsNullOrEmpty(channel))
                return;

            AudioSource source = GetMusicChannel(channel);

            if (source != null)
            {
                source.Pause();
            }
        }
        public void UnPauseMusic(string channel)
        {
            if (string.IsNullOrEmpty(channel))
                return;

            AudioSource source = GetMusicChannel(channel);

            if (source != null)
            {
                source.UnPause();
            }
        }

        public void StopMusic(string channel)
        {
            if (string.IsNullOrEmpty(channel))
                return;
            AudioSource source = GetMusicChannel(channel);
            if (source != null)
            {
                source.Stop();
            }
        }
        public void StopAllMusic()
        {
            foreach (var source in channels_music.Values)
            {
                if (source != null)
                    source.Stop();
            }
        }

        public void StopSoundFX(string channel)
        {
            if (string.IsNullOrEmpty(channel))
                return;
            if (DoesChannelExist(channel) && channels_sfx[channel] != null)
            {
                channels_sfx[channel].Stop();
            }
        }

        public void RefreshVolume()
        {
            foreach (KeyValuePair<string, AudioSource> pair in channels_sfx)
            {
                if (pair.Value != null)
                {
                    float vol = channels_volume.ContainsKey(pair.Key) ? channels_volume[pair.Key] : 0.8f;
                    // pair.Value.volume = vol * PlayerData.Get.sfx_volume;
                    pair.Value.volume = vol * 1;

                }
            }

            foreach (KeyValuePair<string, AudioSource> pair in channels_music)
            {
                if (pair.Value != null)
                {
                    float vol = channels_volume.ContainsKey(pair.Key) ? channels_volume[pair.Key] : 0.4f;
                    // pair.Value.volume = vol * PlayerData.Get.music_volume;
                    pair.Value.volume = vol * 1;
                }
            }
        }

        public bool IsMusicPlaying(string channel)
        {
            AudioSource source = GetMusicChannel(channel);
            if (source != null)
                return source.isPlaying;
            return false;
        }

        public AudioSource GetChannel(string channel)
        {
            if (channels_sfx.ContainsKey(channel))
                return channels_sfx[channel];
            return null;
        }

        public AudioSource GetMusicChannel(string channel)
        {
            if (channels_music.ContainsKey(channel))
                return channels_music[channel];
            return null;
        }

        public bool DoesChannelExist(string channel)
        {
            return channels_sfx.ContainsKey(channel);
        }

        public bool DoesMusicChannelExist(string channel)
        {
            return channels_music.ContainsKey(channel);
        }

        public AudioSource CreateChannel(string channel, int priority = 128)
        {
            if (string.IsNullOrEmpty(channel))
                return null;

            GameObject cobj = new GameObject("AudioChannel-" + channel);
            cobj.transform.parent = transform;
            AudioSource caudio = cobj.AddComponent<AudioSource>();
            caudio.playOnAwake = false;
            caudio.loop = false;
            caudio.priority = priority;
            return caudio;
        }

        //Shortcuts
        public static void Music(string channel, AudioClip audio, float volume = 1f) { instance?.PlayMusic(channel, audio, volume); }
        public static void SFX(string channel, AudioClip audio, float volume = 0.8f, bool loop = false) { instance?.PlaySFX(channel, audio, volume, loop: loop); }
        public static void Stop(string channel) { instance?.StopMusic(channel); } //Stops music
        public static void StopSFX(string channel) { instance?.StopSoundFX(channel); } //Stops music

        public void StopAllSound()
        {
            StopAllMusic();
            StopAllSoundFX();
        }
        public void StopAllSoundFX()
        {
            foreach (var source in channels_sfx.Values)
            {
                if (source != null)
                    source.Stop();
            }
        }

        public void SetEnableSound(bool enable)
        {
            //PlayerData.Get.master_volume = enable ? 1f : 0f;
        }
        
    }

}
