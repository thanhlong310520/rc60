using Raccoon.EnumHolder;
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
        [SerializeField] public List<SoSkin> listSkinSO;
        [SerializeField] public List<SoDailyReward> listDailyRewardSO;
        [SerializeField] List<UiCurrencyType> listUICurrency;
        [SerializeField] List<SoSkin> defaultSkin;

        public DailyRewardTimeChecker dailyRewardTimeChecker = new DailyRewardTimeChecker();


        public List<AudioClip> audioBg;
        [SerializeField] List<SoundData> listSoundFX;

        static string bgId = "BGmusic";


        public static float scaleAds;
        public static long refreshTimeAds;

        public bool isShowVip;

        public MapData currentMap;


        public List<SoSkin> currentSkinSOs;


        private void Awake()
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            PlayerData.LoadLast();
        }

        private void Start()
        {
            isShowVip = false;
            ObserverEventManager.Instance.Subscribe<SoundType>(EventObserverName.PlaySfx.ToString(),PlaySFX);
            CheckSubscribe();
            GetCurrentMap();
            currentSkinSOs = GetListCurrentSkinUserUse();
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

        public CharacterData GetCharacterData()
        {
            CharacterData result = PlayerData.Get.GetCharacterData();
            return result;
        }
        public bool CanClaimReward()
        {
            string lastReward = PlayerData.Get.GetCharacterData().lastTimeClaimReward;
            if (string.IsNullOrEmpty(lastReward)) return true;

            if (dailyRewardTimeChecker.GetElapsedDays(lastReward) >= 1)
            {
                return true;
            }
            return false;
        }
        public int GetDayReward()
        {
            int exDay = PlayerData.Get.GetCharacterData().dayReward;

            if (exDay < 0) exDay = 0;
            if (exDay >= listDailyRewardSO.Count)
            {
                if(CanClaimReward()) exDay = 0;
            }
            return exDay + 1;
        }


        public void ClaimReward(List<SoDailyReward> listSO)
        {
            int day = 0;
            foreach (var so in listSO)
            {
                if (so.day > day) day = so.day;
                /// add

            }
            print("Day " + day);
            PlayerData.Get.GetCharacterData().SetDayReward(day);
            string timeClaim = dailyRewardTimeChecker.ConvertTimeToString(DateTime.UtcNow);
            PlayerData.Get.GetCharacterData().SetLastTimeClaimReward(timeClaim);
        }
        #region Map
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

            print("[GameData] reset map " + idMap + " done");
            print("[GameData] list checkpoint " + dt.listCheckPoint.Count); 
        }

        public DataMap GetDataMap(string idMap)
        {
            return PlayerData.Get.GetDataMap(idMap);
        }

        public bool GetIsCheckpointSaved(string idMap, string idCheckpoint)
        {
            return PlayerData.Get.GetIsCheckpointSaved(idMap, idCheckpoint);
        }
        public bool GetWinMap(string idMap)
        {
            var dataMap = PlayerData.Get.GetDataMap(idMap);
            return dataMap != null && dataMap.won;
        }
        public void NextMap()
        {
            int index = mapDataList.FindIndex(c => c.mapId == currentMap.mapId);
            if (index < 0) index = 0;
            else index++;
            if (index >= mapDataList.Count) index = 0;

            SetCurrentMap(mapDataList[index]);

            var dataMap = GetDataMap(currentMap.mapId);
            if (dataMap.won)
            {
                ResetMap(dataMap.map_id);
            }
        }

        #endregion

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


        public List<SoSkin> GetListCurrentSkinUserUse()
        {
            List<SoSkin> soSkins = new List<SoSkin>();
            foreach (TypeSkin type in Enum.GetValues(typeof(TypeSkin)))
            {
                string idSkin = GetCharacterData().GetIdCurrentSkin(type);
                if(idSkin != null || !string.IsNullOrEmpty(idSkin))
                {
                    var so = GetSkin(type, idSkin);
                    if (so != null) soSkins.Add(so);
                }
                else
                {
                    foreach (var s in defaultSkin)
                    {
                        if(s.typeSkin == type) soSkins.Add(s);
                    }
                }
            }

            return soSkins;
        }

        public SoSkin GetSkin(TypeSkin type,string id)
        {
            SoSkin result = listSkinSO.FirstOrDefault(so => (so.typeSkin == type && so.id == id));
            return result;
        }

        public void ChangeSkin(SoSkin so)
        {
            if (so == null) return;
            var removeSo = currentSkinSOs.FirstOrDefault(s => s.typeSkin == so.typeSkin);
            if(removeSo!= null) currentSkinSOs.Remove(removeSo);
            currentSkinSOs.Add(so);
            GetCharacterData().ChangeFashion(so.typeSkin, so.id);
            GetCharacterData().AddOwnSkin(so.typeSkin, so.id);
        }
        public Sprite GetBgDailyRewardCurrencyByType(TypeCurrency type)
        {
            return listUICurrency.FirstOrDefault(ui => ui.type == type).bgDailyReward;
        }
    }

    [Serializable] struct SoundData
    {
        public SoundType type;
        public AudioClip clip;
        public float vol;
    }
    [Serializable]
    struct UiCurrencyType
    {
        public TypeCurrency type;
        public Sprite bgDailyReward;
    }



    public enum SoundType 
    {
        Button, ContactSlot, CollectCoin, ContactWave, BaseBatAttack, SaveLoot, UpgradeSuccess, LootItem, FootStep, ChangeMap, 
        Jump, MapSound, Steal, BuyFall, Shield, ChangeMap1,
    }
}
