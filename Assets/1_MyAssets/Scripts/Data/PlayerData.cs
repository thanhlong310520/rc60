using MessagePack;
using Raccoon.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Raccoon
{
    [MessagePackObject(keyAsPropertyName: true)]
    [System.Serializable]
    public class PlayerData
    {
        private static string file_loaded = "rc60";
        private static PlayerData player_data = null;
        public const string extension = ".raccoon";
        public bool onSound;
        public bool onMusic;
        public bool isOnAds = true;
        public string filename;
        public string version;
        public string currentMap;
        public List<CharacterData> charactorDatas;
        public List<string> actionTutDone;
        public Dictionary<string, DataMap> listDataMap;
        public bool isVipIAP;

        public PlayerData() { }

        public PlayerData(string name)
        {
            filename = name;
            version = Application.version;
            onMusic = true;
            onSound = true;
            isOnAds = true;
            isVipIAP = false;
            currentMap = "";
        }
        public List<CharacterData> GetCharacterDatas() => charactorDatas;

        public void FixData()
        {
            charactorDatas ??= new List<CharacterData>();
            actionTutDone ??= new List<string>();
            listDataMap ??= new Dictionary<string,DataMap>();
        }

        #region SAVE / LOAD

        public static PlayerData Get
        {
            get { return player_data; }
        }

        public bool IsVersionValid()
        {
            return version == Application.version;
        }

        public void Save()
        {
            Debug.Log("Save Game");
            Save(file_loaded, this);
        }

        public static void Save(string filename, PlayerData data)
        {
            if (!string.IsNullOrEmpty(filename) && data != null)
            {
                data.filename = filename;
                data.version = Application.version;
                player_data = data;
                file_loaded = filename;

                SaveTool.SaveFile<PlayerData>(filename + extension, data);
                SetLastSave(filename);
            }
        }

        public static void NewGame()
        {
            NewGame(GetLastSave()); //default name
        }

        //You should reload the scene right after NewGame
        public static PlayerData NewGame(string filename)
        {
            file_loaded = filename;
            player_data = new PlayerData(filename);
            player_data.FixData();
            return player_data;
        }

        public static PlayerData Load(string filename)
        {
            if (player_data == null || file_loaded != filename)
            {
                player_data = SaveTool.LoadFile<PlayerData>(filename + extension);
                if (player_data != null)
                {
                    file_loaded = filename;
                    player_data.FixData();
                }
            }
            return player_data;
        }

        public static PlayerData LoadLast()
        {
            return AutoLoad(GetLastSave());
        }

        //Load if found, otherwise new game
        public static PlayerData AutoLoad(string filename)
        {
            if (player_data == null)
                player_data = Load(filename);
            if (player_data == null)
                player_data = NewGame(filename);

            Debug.Log("LoadData");
            return player_data;
        }

        public static void SetLastSave(string filename)
        {
            if (SaveTool.IsValidFilename(filename))
            {
                PlayerPrefs.SetString(PlayerPrefsKey.LAST_SAVE, filename);
            }
        }

        public static string GetLastSave()
        {
            string name = PlayerPrefs.GetString(PlayerPrefsKey.LAST_SAVE, "");
            if (string.IsNullOrEmpty(name))
                name = "rc53"; //Default name
            return name;
        }

        public static bool HasLastSave()
        {
            return HasSave(GetLastSave());
        }

        public static bool HasSave(string filename)
        {
            return SaveTool.DoesFileExist(filename + extension);
        }

        public static void Unload()
        {
            player_data = null;
            file_loaded = "";
        }

        public static void Delete(string filename)
        {
            if (file_loaded == filename)
            {
                player_data = new PlayerData(filename);
                player_data.FixData();
            }

            SaveTool.DeleteFile(filename + extension);
        }

        public static bool IsLoaded()
        {
            return player_data != null && !string.IsNullOrEmpty(file_loaded);
        }

        #endregion

        #region USE_DATA
        #region Ads
        public void OnVip()
        {
            isVipIAP = true;
        }
        public void RemoveVip()
        {
            isVipIAP = false;
        }
        public void RemoveAds()
        {
            isOnAds = false;
        }
        public void OnAds()
        {
            isOnAds = true;
        }
        #endregion

        public void SetCurrentMap(string cm)
        {
            Get.currentMap = cm;
        }
        public DataMap GetDataMap(string idMap)
        {
            DataMap result = null;
            if(string.IsNullOrEmpty(idMap)) return null;
            if (Get.listDataMap.ContainsKey(idMap))
            {
                if(Get.listDataMap[idMap] == null)
                {
                    result = new DataMap(idMap);
                    Get.listDataMap[idMap] = result;
                }
                else
                {
                    result = Get.listDataMap[idMap];
                }
            }
            else
            {
                result = new DataMap(idMap);
                Get.listDataMap.Add(idMap, result);
            }
            return result;

        }

        public string GetLastCheckPointInMap(string idMap)
        {
            string result = "";
            DataMap dataM = GetDataMap(idMap);
            if(dataM != null && dataM.listCheckPoint.Count > 0)
            {
                result = dataM.listCheckPoint[dataM.listCheckPoint.Count - 1];
            }

            return result;
        }

        public bool SaveCheckPoint(string idMap, string idCheckPoint)
        {
            DataMap dM = GetDataMap(idMap);
            if(dM != null)
            {
                return dM.AddCheckPoint(idCheckPoint);
            }
            return false;
        }
        public CharacterData GetCharacterData(string id)
        {
            CharacterData result = Get.GetCharacterDatas().FirstOrDefault(c => c.id == id);
            if (result == null)
            {
                result = new CharacterData(id, id);
                Get.charactorDatas.Add(result);
            }
            return result;
        }

        public bool HaveActionTutDone(string action)
        {
            return actionTutDone.Contains(action);
        }
        public void AddActionTutDone(string action)
        {
            if(!actionTutDone.Contains(action)  && !string.IsNullOrEmpty(action))
                actionTutDone.Add(action);
        }
        #endregion

    }

    [MessagePackObject(keyAsPropertyName: true)]
    [System.Serializable]
    public class UserInfoData
    {
        public string user_id;
        public string user_name;
        public string avatar_name;
        public long totalExp;
        public string fashion_id;

        public UserInfoData() { }

        public UserInfoData(string id)
        {
            this.user_id = id;
        }
    }

}