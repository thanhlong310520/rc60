
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Raccoon.Utils
{
    public static class GameUtils
    {
        public static string GetFullByFilename(string fileName)
        {
            string path = Application.persistentDataPath + "/VoxelPlay";
            Directory.CreateDirectory(path);
            string fullName = path + "/" + fileName + GameConstants.SAVEGAMEDATA_EXTENSION;
            return fullName;
        }

        public static string GenerateUniqueID(int min = 7, int max = 11)
        {
            int length = Random.Range(min, max);
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            string unique_id = "";
            for (int i = 0; i < length; i++)
            {
                unique_id += chars[Random.Range(0, chars.Length - 1)];
            }
            return unique_id;
        }

        public static bool IsNullOrEmpty(this string txt)
        {
            if (string.IsNullOrEmpty(txt)) return true;
            if (string.IsNullOrWhiteSpace(txt)) return true;

            return false;
        }

        public static string LogString(this List<string> list)
        {
            return string.Join(" - ", list);
        }

        public static RectTransform ClearAllChild(this RectTransform rt)
        {
            foreach (Transform child in rt)
            {
                GameObject.Destroy(child.gameObject);
            }
            return rt;
        }

        public static Transform ClearAllChild(this Transform rt)
        {
            foreach (Transform child in rt)
            {
                GameObject.Destroy(child.gameObject);
            }
            return rt;
        }

        public static Texture2D DecodeBase64(string base64String, int sizeX, int sizeY)
        {
            if (string.IsNullOrEmpty(base64String))
            {
                Debug.LogWarning("Base64 string is empty.");
                return null;
            }

            try
            {
                byte[] imageBytes = Convert.FromBase64String(base64String);
                Texture2D texture = new Texture2D(sizeX, sizeY); // Initial size, will be updated later

                if (texture.LoadImage(imageBytes))
                {
                    // Apply changes to the texture
                    texture.Apply();
                    return texture;
                }
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError("Error decoding Base64 string: " + e.Message);
                return null;
            }
        }

        public static void ForceBuildLayout(this RectTransform root)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(root);
            root.ForceUpdateRectTransforms();
        }

        public static string ConvertCurrencyToString(this long value)
        {
            if (value < 1000)
                return value.ToString();

            if (value < 1_000_000)
                return (value / 1000f).ToString("0.#") + "K";

            if (value < 1_000_000_000)
                return (value / 1_000_000f).ToString("0.#") + "M";

            if (value < 1_000_000_000_000)
                return (value / 1_000_000_000f).ToString("0.#") + "B";

            return (value / 1_000_000_000_000f).ToString("0.#") + "T";
        }
        public static Vector3 GetPoint(this Vector3 origin, Vector3 dir, float d)
        {
            return origin + dir.normalized * d;
        }
        //public static void SetRarityText(RarityType rarity, TextMeshProUGUI text)
        //{
        //    if (text == null) return;
        //    text.enableVertexGradient = false;
        //    switch (rarity)
        //    {
        //        case RarityType.Common:
        //            text.text = "Common";
        //            text.color = Color.white;
        //            break;

        //        case RarityType.Rare:
        //            text.text = "Rare";
        //            text.color = new Color(0.2f, 0.6f, 1f); // blue
        //            break;

        //        case RarityType.Epic:
        //            text.text = "Epic";
        //            text.color = new Color(0.9f, 0.9f, 0f); // yellow
        //            break;

        //        case RarityType.Legendary:
        //            text.text = "Legendary";
        //            text.color = new Color(1f, 0.6f, 0.2f); // orange
        //            break;

        //        case RarityType.Mythic:
        //            text.text = "Mythic";
        //            text.color = new Color(0.6f, 0.2f, 1f); // purple
        //            break;
        //        case RarityType.Godly:
        //            text.text = "Godly";
        //            text.color = new Color(0.1f, 0.2f, 0.5f); // red
        //            break;
        //        case RarityType.Secret:
        //            text.text = "Secret";
        //            text.color = new Color(0.5f, 0.5f, 0.5f); // gray
        //            break;
        //    }
        //}

        //public static Color GetColorRarity(RarityType rarity)
        //{
        //    switch (rarity)
        //    {
        //        case RarityType.Rare:
        //            return new Color(0.2f, 0.6f, 1f); // blue

        //        case RarityType.Epic:
        //            return new Color(0.9f, 0.9f, 0f); // yellow

        //        case RarityType.Legendary:
        //            return new Color(1f, 0.6f, 0.2f); // orange

        //        case RarityType.Mythic:
        //            return new Color(0.6f, 0.2f, 1f); // purple

        //        case RarityType.Secret:
        //            return new Color(0.5f, 0.5f, 0.5f); // gray

        //        default:
        //            return Color.white;
        //    }
        //}

        //public static Color GetColorByColorType(ColorType colorType)
        //{
        //    if (colorType == ColorType.Golden)
        //        return Color.yellow;
        //    if (colorType == ColorType.Diamond)
        //        return new Color(0f, 1f, 1f);
        //    if (colorType == ColorType.Rainbow)
        //        return new Color(1f, 0.5f, 0f);
        //    return Color.white;
        //}

        public static Color ApplyColorTint(Color baseColor, Color tint, float bias = 0.65f)
        {
            var a = baseColor.a;

            // 1. Multiply (preserve texture detail)
            var c = new Color(
                baseColor.r * tint.r,
                baseColor.g * tint.g,
                baseColor.b * tint.b,
                a
            );

            // 2. Add small bias so dark colors still get tinted
            c.r += tint.r * bias;
            c.g += tint.g * bias;
            c.b += tint.b * bias;

            // 3. Clamp safely
            c.r = Mathf.Clamp01(c.r);
            c.g = Mathf.Clamp01(c.g);
            c.b = Mathf.Clamp01(c.b);
            c.a = a;

            return c;
        }


        public static bool IsFirstLoginToday(string lastLoginDate)
        {
            string todayDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
            return string.IsNullOrEmpty(lastLoginDate) || lastLoginDate != todayDate;
        }

        public static Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            float u = 1 - t;
            return u * u * p0 + 2 * u * t * p1 + t * t * p2;
        }

        public static void SafeSetTexture(RawImage image, Texture texture)
        {
            if (image != null && image.isActiveAndEnabled)
            {
                image.texture = texture;
            }
            else
            {
                Debug.LogWarning("[GameUtils] Tried to set texture on a null or inactive RawImage.");
            }
        }

        public static void SafeSetText(Text text, string value)
        {
            if (text != null && text.isActiveAndEnabled)
            {
                text.text = value;
            }
            else
            {
                Debug.LogWarning("[GameUtils] Tried to set text on a null or inactive Text component.");
            }
        }

        //public static long CostMergeBotByColor(BotDataSO botDataSO)
        //{
        //    if(botDataSO.colorType == ColorType.Normal) return 0;
        //    var multi = 0.2f;
        //    var sum = (botDataSO.cost * 2);
        //    if (botDataSO.colorType == ColorType.Diamond)
        //        multi = 0.4f;
        //    sum = (long)(sum * multi);
        //    return sum;
        //}
        
        //public static ItemInfo GetRandomValueSeed(PlantDataSO seed)
        //{
        //    var randScale = GameData.GetRandomSizeScaleAndDamg();
        //    var plant = GameData.GetPlantGrownBySeed(seed);
        //    var itemInfo = new ItemInfo(plant.GetId, randScale.x, randScale.y, plant.time_grow, ItemInfo.ItemTypeInfo.Seed);
        //    return itemInfo;
        //}

        public static List<T> GetRandomUniqueElements<T>(this List<T> source, int count)
        {
            if (source == null || source.Count < count)
            {
                Debug.LogError("Not enough elements in the list!");
                return new List<T>();
            }

            return source.OrderBy(x => Random.value).Take(count).ToList();
        }

        public static void ForceKnockback(Transform originForce, Transform target, float force)
        {
            if (originForce == null || target == null) return;

            var rig = target.GetComponent<Rigidbody>();
            if (rig == null) return;

            // direction from character to target (horizontal only)
            Vector3 pushDir = (rig.position - originForce.position).normalized;
            pushDir.y = 0f; // keep push horizontal

            // clear existing velocity for consistent push
            // rig.linearVelocity = Vector3.zero;

            // apply impulse force
            rig.AddForce(pushDir * force, ForceMode.Impulse);
        }

        public static void ForceKnockback(Rigidbody rig, Vector3 target, Vector3 source, bool isPush, float multiForce = 25f)
        {
            // Calculate push direction
            Vector3 dir = (target - source).normalized;
            if (!isPush)
                dir *= -1;
            dir.y = 0;

            // Add small random deviation (horizontal only)
            float angle = UnityEngine.Random.Range(-15f, 15f); // random rotation angle in degrees
            Quaternion rot = Quaternion.Euler(0f, angle, 0f);
            dir = rot * dir;


            dir.Normalize();
            // Apply extra physics force
            //rig.linearVelocity = Vector3.zero; // reset velocity
            rig.AddForce(dir * multiForce, ForceMode.Impulse);
        }


        public static Vector3 CalculateArcVelocity(Vector3 start, Vector3 end, float scaleForceY)
        {
            Vector3 displacementXZ = new Vector3(end.x - start.x, 0, end.z - start.z);
            float displacementY = end.y - start.y;
            float height = displacementXZ.magnitude * scaleForceY;

            float timeUp = Mathf.Sqrt(2 * height / 9.8f);
            float timeDown = Mathf.Sqrt(2 * Mathf.Abs(height - displacementY) / 9.8f);
            float totalTime = timeUp + timeDown;

            Vector3 velocityXZ = displacementXZ / totalTime;
            float velocityY = Mathf.Sqrt(2 * 9.8f * height);
            Vector3 result = velocityXZ;
            result.y = velocityY;

            return result;
        }
        public static void AddForceRig(Rigidbody rig, Vector3 force, float scale = 1)
        {
            rig.linearVelocity = force * scale;
        }

        public static bool IsTagNameShop(this string tag)
        {
            if (tag == TagName.Tag_ShopDefensive)
                return true;
            
            return false;
        }

        public static bool IsTagNameHouse(this string tag)
        {
            if (tag == TagName.Tag_House)
                return true;

            return false;
        }
        public static bool IsTagGateHouse(this string tag)
        {
            if (tag == TagName.Tag_Gate_House)
                return true;

            return false;
        }

        public static bool IsTagNameBot(this string tag)
        {
            if (tag == TagName.Tag_Bot)
                return true;

            return false;
        }

        public static bool IsTagNameSlot(this string tag)
        {
            if (tag == TagName.Tag_Slot)
                return true;

            return false;
        }

        public static bool IsTagNameItem(this string tag)
        {
            if (tag == TagName.Tag_Item)
                return true;

            return false;
        }


        //public static List<ItemConfig> FindByIdContains(List<ItemConfig> items, string keyword)
        //{
        //    if (string.IsNullOrEmpty(keyword)) return new List<ItemConfig>();

        //    return items
        //        .Where(x => !string.IsNullOrEmpty(x.id) &&
        //                    x.id.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
        //        .ToList();
        //}

        /// <summary>
        /// Format seconds into string "hh:mm:ss" or "mm:ss"
        /// </summary>
        public static string FormatTimeSecond(this float totalSeconds, bool alwaysShowHours = false)
        {
            int secondsInt = Mathf.FloorToInt(totalSeconds);

            int hours = secondsInt / 3600;
            int minutes = (secondsInt % 3600) / 60;
            int seconds = secondsInt % 60;

            if (alwaysShowHours || hours > 0)
            {
                return $"{hours:00}:{minutes:00}:{seconds:00}"; // hh:mm:ss
            }
            else
            {
                return $"{minutes:00}:{seconds:00}"; // mm:ss
            }
        }
    }

    public static class UniqueRandomPicker
    {
        // Dictionary stores the remaining items per list (identified by a key)
        private static Dictionary<string, List<int>> indexMap = new Dictionary<string, List<int>>();

        // Returns a random item from the list without repetition
        public static T GetItem<T>(List<T> list, string listId)
        {
            if (!indexMap.ContainsKey(listId) || indexMap[listId].Count == 0)
            {
                // Initialize or reset
                indexMap[listId] = new List<int>();
                for (int i = 0; i < list.Count; i++)
                    indexMap[listId].Add(i);
            }

            // Randomly pick and remove an index
            int randomIndexInPool = Random.Range(0, indexMap[listId].Count);
            int actualIndex = indexMap[listId][randomIndexInPool];
            indexMap[listId].RemoveAt(randomIndexInPool);

            return list[actualIndex];
        }
    }

    public class GameConstants
    {
        public const float TIME_REFESH_HAFT_SEC = 0.5f;
        public const float TIME_REFESH_SEC = 1f;
        public const float TIME_WAIT_REWARD = 0.5f;
        public const float FORCE_KNOCKBACK = 500f;
        public const string SAVEGAMEDATA_EXTENSION = ".bytes";

        public const string AVATAR_MALE_DEFAULT = "male";
        public const string AVATAR_FEMALE_DEFAULT = "female";
    }

    public class SceneName
    {
        public const string LoadingScene = "LoadingScene";
        //public const string SingletonScene = "SingletonScene";
        public const string MainScene = "MainScene";
    }

    public class EventTracking
    {
        public readonly static string EVENT_START_MAP = "event_start_map";
        public readonly static string EVENT_REWARD_ADS_REVENUE = "event_reward_ads_revenue";
    }

    public class TagName
    {
        public readonly static string Tag_Player = "Player";
        public readonly static string Tag_House = "House";
        public readonly static string Tag_Item = "Item";
        public readonly static string Tag_Slot = "Slot";
        public readonly static string Tag_Bot = "Bot";
        public readonly static string Tag_Gate_House = "Gate";
        public readonly static string Tag_Npc = "Npc";
        public readonly static string Tag_ShopDefensive = "ShopDefensive";
    }

    public class PlayerPrefsKey
    {
        public readonly static string LAST_SAVE = "last_save";
    }
    public class LayerName
    {
        public readonly static string LAYER_FLOOR = "Floor";
        public readonly static string LAYER_NPC = "Npc";
    }
    public class AnimationName
    {
        public readonly static string ANIM_ATTACKING = "Attacking";
        public readonly static string ANIM_BUILD = "Build";
        public readonly static string ANIM_DEAD = "Dead";
        public readonly static string ANIM_DANCE = "Dance";
        public readonly static string ANIM_CHEER = "Cheer";
        public readonly static string ANIM_SWIM = "InWater";

    }
    public class Click_Feature
    {
        public readonly static string AVATAR_HOME = "avatar_home";
        public readonly static string SETTING_HOME = "setting_home";
        public readonly static string SHOP_HOME = "shop_home";
        public readonly static string DAILY_LOGIN_HOME = "daily_login_home";
        public readonly static string BACK_CREATE_MODE = "back_create_mode";
        public readonly static string AVATAR_CREATE_MODE = "avatar_create_mode";
        public readonly static string SETTING_CREATE_MODE = "setting_create_mode";
        public readonly static string BACK_IN_GAME = "back_in_game";
        public readonly static string SETTING_IN_GAME = "setting_in_game";
        public readonly static string INVENTORY_IN_GAME = "inventory_in_game";
        public readonly static string BOT_IN_GAME = "bot_in_game";
        public readonly static string QUEST_IN_GAME = "quest_in_game";
        public readonly static string BUILD_IN_GAME = "build_in_game";
        public readonly static string FLY_IN_GAME = "fly_in_game";
        public readonly static string JUMP_IN_GAME = "jump_in_game";
        public readonly static string WALK_RUN_IN_GAME = "walk_run_in_game";
        public readonly static string UN_RIDE_ITEM_IN_GAME = "un_ride_item_in_game";
        public readonly static string SHOP_IN_GAME = "shop_in_game";
        public readonly static string GO_QUEST_IN_GAME = "go_quest_in_game";
        public readonly static string COLLECT_QUEST_IN_GAME = "collect_quest_in_game";
        public readonly static string AVATAR_IN_GAME = "avatar_in_game";
        public readonly static string SHOW_LIST_BLOCK = "show_list_block";
        public readonly static string HOOK_IN_GAME = "hook_in_game";
        public readonly static string SCOPE_IN_GAME = "scope_in_game";

    }
    public class Event_Firebase
    {
        public readonly static string EVENT_CLICK_FEATURE = "click_feature";
        public readonly static string EVENT_CLICK_MENU = "click_menu";
        public readonly static string EVENT_CLICK_BLOCK = "click_block";
        public readonly static string EVENT_CLICK_ITEM = "click_item";
        public readonly static string EVENT_REWARD_ADS = "click_reward";
        public readonly static string EVENT_GAME = "event_game";
        public readonly static string EVENT_PURCHASE_IAP = "purchase_iap";
    }
}