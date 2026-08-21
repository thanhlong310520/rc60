using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Raccoon.Utils;
#if USE_FIREBASE
using Firebase.Analytics;
using Firebase.Crashlytics;
using Firebase.Extensions;
#endif
using UnityEngine;

namespace Raccoon.Controller
{
    public class GameFirebase : MonoBehaviour
    {
#if USE_FIREBASE
        private static GameFirebase api;
        public static GameFirebase Get => api;

        private static bool firebaseInitialized = false;
        private Firebase.FirebaseApp appFirebase;

        // Start is called before the first frame update
        void Awake()
        {
            if(api != null)
            {
                Destroy(gameObject);
                return;
            }
            api = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            InitFirebase();
        }

        private void InitFirebase()
        {
            if (firebaseInitialized) return;
            //Debug.Log("start init");
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
                var dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    // Create and hold a reference to your FirebaseApp,
                    // where app is a Firebase.FirebaseApp property of your application class.
                    InitializeFirebase();
                }
                else
                {
                    UnityEngine.Debug.LogError(System.String.Format(
                      "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                    // Firebase Unity SDK is not safe to use here.
                }
            });
        }

        // Handle initialization of the necessary firebase modules:
        void InitializeFirebase()
        {
            //DebugLog("Enabling data collection.");
            //FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);

            //DebugLog("Set user properties.");
            // Set default session duration values.
            Crashlytics.ReportUncaughtExceptionsAsFatal = true;
            appFirebase = Firebase.FirebaseApp.DefaultInstance;
            Application.logMessageReceived += HandleLog;

            firebaseInitialized = true;

            //HandleLog("send test ", "stack trace ", LogType.Exception);
            Debug.Log("Initialize Firebase.");

            LoadRemoteConfig();

            SetConsentData();
        }

        private void SetConsentData()
        {
            try
            {
                Dictionary<ConsentType, ConsentStatus> consentMap = new Dictionary<ConsentType, ConsentStatus>();
                consentMap.Add(ConsentType.AnalyticsStorage, ConsentStatus.Granted);
                consentMap.Add(ConsentType.AdPersonalization, ConsentStatus.Granted);
                consentMap.Add(ConsentType.AdUserData, ConsentStatus.Granted);
                consentMap.Add(ConsentType.AdStorage, ConsentStatus.Granted);
                Firebase.Analytics.FirebaseAnalytics.SetConsent(consentMap);
            }
            catch
            {
                Debug.Log("SetConsentData error");

            }
        }

        private void LoadRemoteConfig()
        {
            Debug.Log("Load remote config");

            System.Threading.Tasks.Task fetchTask =
            Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.FetchAsync(
                TimeSpan.Zero);
            fetchTask.ContinueWithOnMainThread(FetchDataComplete);
        }

        void FetchDataComplete(Task fetchTask)
        {
            if (fetchTask.IsCanceled)
            {
                Debug.Log("Fetch canceled.");
            }
            else if (fetchTask.IsFaulted)
            {
                Debug.Log("Fetch encountered an error.");
            }
            else if (fetchTask.IsCompleted)
            {
                Debug.Log("Fetch completed successfully!");
            }


            var info = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.Info;
            switch (info.LastFetchStatus)
            {
                case Firebase.RemoteConfig.LastFetchStatus.Success:
                    Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.ActivateAsync()
                    .ContinueWithOnMainThread(task => {
                        //Debug.Log(string.Format("Remote data loaded and ready (last fetch time {0}).",
                        //           info.FetchTime));

                        //how to get value
                        var refreshTime = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance
                                                  .GetValue("refresh_time").LongValue;

                        var scale = (float)Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance
                                                  .GetValue("scale").DoubleValue;

                        GameData.refreshTimeAds = refreshTime; GameData.scaleAds = scale;
                        Debug.Log("Config refreshTime " + GameData.refreshTimeAds);
                        Debug.Log("Config Scale " + GameData.scaleAds);

                        //var remote_time_inter = (float)Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue("time_inter_ads").DoubleValue;
                        //GameAds.Get.time_inter_ad = remote_time_inter;

                        //GameAds.Get._adEnable = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue("ad_enable").BooleanValue;

                        //var item_tag_new = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance
                        //                          .GetValue("item_tag_new").StringValue;
                        //GameData.Get.SetDataItemTagNew(item_tag_new);

                        //var category_tag_new = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance
                        //                          .GetValue("category_tag_new").StringValue;
                        //Debug.Log("category_tag_new  " + category_tag_new);
                        //GameData.Get.SetDataCategoryTagNew(category_tag_new);
                    });

                    break;
                case Firebase.RemoteConfig.LastFetchStatus.Failure:
                    switch (info.LastFetchFailureReason)
                    {
                        case Firebase.RemoteConfig.FetchFailureReason.Error:
                            Debug.Log("Fetch failed for unknown reason");
                            break;
                        case Firebase.RemoteConfig.FetchFailureReason.Throttled:
                            Debug.Log("Fetch throttled until " + info.ThrottledEndTime);
                            break;
                    }
                    break;
                case Firebase.RemoteConfig.LastFetchStatus.Pending:
                    Debug.Log("Latest Fetch call still pending.");
                    break;
            }
            //GameManager.Get.textMeshProToast.text += "-> FetchComplete ";
        }

        private static void SendEventFirebase(string nameEvent, params string[] parameters)
        {
#if UNITY_EDITOR
            var txt = $"send firebase :{nameEvent}: ";
            for (int i = 0; i < parameters.Length; i += 2)
            {
                txt += $"{parameters[i]}: {parameters[i + 1].Replace(".0", "")},";
            }
            //Debug.Log(txt);
#endif
            if (!firebaseInitialized) return;

            var arr = new List<Firebase.Analytics.Parameter>();
            if (parameters.Length % 2 != 0) return;
            for (int i = 0; i < parameters.Length; i += 2)
            {
                arr.Add(new Firebase.Analytics.Parameter(parameters[i], parameters[i + 1].Replace(".0", "")));
            }
            Firebase.Analytics.FirebaseAnalytics.LogEvent(nameEvent, arr.ToArray());
        }

        public static void SendEvent(string nameEvent, params string[] parameters)
        {
            try
            {
#if UNITY_EDITOR
                SendEventFirebase(nameEvent, parameters);
#endif

                if (Get == null || Get.appFirebase == null)
                {
                    Debug.LogWarning("Firebase not initialized yet. Cannot use feature.");
                    return;
                }
                if (firebaseInitialized)
                    SendEventFirebase(nameEvent, parameters);
            }
            catch(Exception ex)
            {

            }
        }

        public static void SendEventGame(string content)
        {
            SendEvent(Event_Firebase.EVENT_GAME, Event_Firebase.EVENT_GAME, content);
        }

        // Log a caught exception.
        public void LogCaughtException(Exception ex)
        {
            Crashlytics.LogException(ex);
        }

        void HandleLog(string logString, string stackTrace, LogType type)
        {
            // Check if the log type is an error or exception
            if (type == LogType.Exception || type == LogType.Error)
            {
                Exception exception = new Exception(logString + "\n" + stackTrace);
                // Call your methods when the application crashes
                LogCaughtException(exception);
            }
        }

        void OnDestroy()
        {
            // Unsubscribe from the application's crash event
            Application.logMessageReceived -= HandleLog;
        }
#else
        private static void SendEventFirebase(string nameEvent, params string[] parameters)
        {
            var txt = $"send firebase :{nameEvent}: ";
            for (int i = 0; i < parameters.Length; i += 2)
            {
                txt += $"{parameters[i]}: {parameters[i + 1].Replace(".0", "")},";
            }
            Debug.Log(txt);
        }

        public static void SendEvent(string nameEvent, params string[] parameters)
        {
            try
            {
#if UNITY_EDITOR
                SendEventFirebase(nameEvent, parameters);
#endif

            }
            catch 
            {

            }
        }

        public static void SendEventGame(string content)
        {
            SendEvent(Event_Firebase.EVENT_GAME, Event_Firebase.EVENT_GAME, content);
        }
#endif
    }
}
