using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
namespace Raccoon
{
    public class GameData : MonoBehaviour
    {
        private static GameData instance;
        public static GameData Get => instance;

        [Header("Danh sách toàn bộ map trong game")]
        [SerializeField] private List<MapData> mapDataList = new List<MapData>();


        public List<AudioClip> audioBg;
        [SerializeField] List<SoundData> listSoundFX;

        static string bgId = "BGmusic";


        public static float scaleAds;
        public static long refreshTimeAds;

        public bool isShowVip;

        public MapData currentMap;
        private void Awake()
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            PlayerData.LoadLast();
        }

        private void Start()
        {
            isShowVip = false;
            //if (PlayerData.Get.onMusic) PlayBgMusic();
            //ObserverEventManager.Instance.Subscribe<SoundType>(EventObserverName.PlaySfx.ToString(),PlaySFX);
            CheckSubscribe();
            GetCurrentMap();
        }

        void GetCurrentMap()
        {
            string mapId = PlayerData.Get.currentMap;
            Debug.Log("[GameData]  mapid = " + mapId);

            currentMap = mapDataList.FirstOrDefault(c => c.mapId == mapId);
            if(currentMap == null)
            {
                Debug.Log("[GameData] current map = null");
                SetCurrentMap(mapDataList[0]);
            }
            Debug.Log("[GameData] list map " + PlayerData.Get.listDataMap.Count);
            
        }

        public void SetCurrentMap(MapData newmap)
        {
            if (newmap == null) return;
            currentMap = newmap;
            PlayerData.Get.SetCurrentMap(currentMap.mapId);
        }
        public void CheckSubscribe()
        {
            //foreach (var sub in GameStoreController.Get.lstProduct)
            //{
            //    //if (sub.productType != UnityEngine.Purchasing.ProductType.Subscription) continue;
            //    GameStoreController.Get.CheckRestoreProductById(sub.id, (restore, id) =>
            //    {
            //        if (restore)
            //        {
            //            sub.OnSendCheckButton(true);
            //            if (sub.has_noads)
            //            {
            //                PlayerData.Get.RemoveAds();
            //            }
            //            if (sub.vip)
            //            {
            //                PlayerData.Get.OnVip();
            //            }
            //        }
            //        else
            //        {
            //            if (sub.has_noads)
            //            {
            //                PlayerData.Get.OnAds();
            //            }
            //            if (sub.vip)
            //            {
            //                PlayerData.Get.RemoveVip();
            //            }
            //        }
            //    });
            //}
        }

        //public CharacterData GetCharacterData(string id)
        //{
        //    CharacterData result = PlayerData.Get.GetCharacterData(id);
        //    return result;  
        //}
        public bool SaveCheckPoint(string idMap, string idCheckPoin)
        {
            return PlayerData.Get.SaveCheckPoint(idMap, idCheckPoin);  
        }

        public void SetWinMap(string idMap)
        {
            DataMap dt = PlayerData.Get.GetDataMap(idMap);
            dt.WinMap();
        }

        public void ResetMap(string idMap)
        {
            Debug.Log("[GameData] reset map " + idMap);

            DataMap dt = PlayerData.Get.GetDataMap(idMap);
            dt.Reset();
        }

        public DataMap GetDataMap(string idMap)
        {
            return PlayerData.Get.GetDataMap(idMap);
        }

        public bool GetWinMap(string idMap)
        {
            var dataMap = PlayerData.Get.GetDataMap(idMap);
            return dataMap != null && dataMap.won;
        }
        #region Audio
        public void ChangeOnSound(UnityAction<bool> actionChange)
        {
            PlayerData.Get.onSound = ! PlayerData.Get.onSound;
            actionChange?.Invoke(PlayerData.Get.onSound);

            if (!PlayerData.Get.onSound) StopSound();
        }
        public void ChangeOnMusic(UnityAction<bool> actionChange)
        {
            PlayerData.Get.onMusic = !PlayerData.Get.onMusic;
            actionChange?.Invoke(PlayerData.Get.onMusic);

            if (PlayerData.Get.onMusic) PlayBgMusic();
            else PauseBgMusic();
        }

        void PlayBgMusic()
        {
            AudioClip clip = GetClipBgMusic();
            GameAudio.Get.PlayMusic(bgId, clip);
        }
        void PauseBgMusic()
        {
            GameAudio.Get.PauseMusic(bgId);
        }

        public void StopSound()
        {
            GameAudio.Get.StopAllSoundFX();
        }
        private void OnApplicationQuit()
        {
            if (PlayerData.Get != null) PlayerData.Get.Save();
        }
        private void OnApplicationPause(bool pause)
        {
            if (PlayerData.Get != null) PlayerData.Get.Save();
        }

        public void PlaySFX(string idAs, AudioClip clip, float vol = 0.7f)
        {
            if (!PlayerData.Get.onSound) return;

            GameAudio.Get.PlaySFX(idAs, clip, vol);
        }
        public void PlaySFX(SoundType type)
        {
            if (!PlayerData.Get.onSound) return;
            AudioClip clip = GetClipBySoundType(type);
            float vol = GetVolBySoundType(type);
            if (clip == null) return;

            GameAudio.Get.PlaySFX(type.ToString(), clip, vol);
        }
        AudioClip GetClipBySoundType(SoundType type)
        {
            var sd = listSoundFX.FirstOrDefault(e =>  e.type == type);

            return sd.clip;
        }
        float GetVolBySoundType(SoundType type)
        {
            var sd = listSoundFX.FirstOrDefault(e => e.type == type);

            return sd.vol;
        }
        AudioClip GetClipBgMusic()
        {
            int index = UnityEngine.Random.Range(0, audioBg.Count);
            return audioBg[index];
        }
        #endregion


        #region Sub

        public void BuyVipIAP()
        {
            PlayerData.Get.OnVip();
            //GameAds.Get.RemoveAds();
        }

        public void BuyRemoveAdsIAP()
        {
            PlayerData.Get.RemoveAds();
            //GameAds.Get.RemoveAds();

        }
        #endregion
    }

    [Serializable] struct SoundData
    {
        public SoundType type;
        public AudioClip clip;
        public float vol;
    }
    public enum SoundType 
    {
        Button, ContactSlot, CollectCoin, ContactWave, BaseBatAttack, SaveLoot, UpgradeSuccess, LootItem, FootStep, ChangeMap, 
        Jump, MapSound, Steal, BuyFall, Shield, ChangeMap1,
    }
}
