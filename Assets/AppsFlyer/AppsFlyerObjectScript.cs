using System; // for EventArgs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AppsFlyerSDK;
using UnityEngine.SceneManagement; // for SceneManager

// This class is intended to be used the the AppsFlyerObject.prefab

public class AppsFlyerObjectScript : MonoBehaviour, IAppsFlyerConversionData
{

    // These fields are set from the editor so do not modify!
    //******************************//
    public string devKey;
    public string appID;
    public string UWPAppID;
    public string macOSAppID;
    public bool isDebug;
    public bool getConversionData;
    public string deeplinkURL;
    //******************************//


    void Start()
    {
        // These fields are set from the editor so do not modify!
        //******************************//
        AppsFlyer.setIsDebug(isDebug);
#if UNITY_WSA_10_0 && !UNITY_EDITOR
        AppsFlyer.initSDK(devKey, UWPAppID, getConversionData ? this : null);
#elif UNITY_STANDALONE_OSX && !UNITY_EDITOR
        AppsFlyer.initSDK(devKey, macOSAppID, getConversionData ? this : null);
#else
        AppsFlyer.initSDK(devKey, appID, getConversionData ? this : null);
#endif
        //******************************/
        AppsFlyer.OnDeepLinkReceived += OnDeepLink;
        AppsFlyer.startSDK();
    }

    void OnDeepLink(object sender, EventArgs args)
    {
        var deepLinkEventArgs = args as DeepLinkEventsArgs;
        AppsFlyer.AFLog("DeepLink Status", deepLinkEventArgs.status.ToString());
        AppsFlyer.AFLog("OnDeepLink Application.absoluteURL", Application.absoluteURL);
        if (!string.IsNullOrEmpty(Application.absoluteURL))
        {
            HandlerDLScene(Application.absoluteURL);
        }

        switch (deepLinkEventArgs.status)
        {
            case DeepLinkStatus.FOUND:

                if (deepLinkEventArgs.isDeferred())
                {
                    AppsFlyer.AFLog("OnDeepLink", "This is a deferred deep link");
                }
                else
                {
                    AppsFlyer.AFLog("OnDeepLink", "This is a direct deep link");
                }

                // deepLinkParamsDictionary contains all the deep link parameters as keys
                Dictionary<string, object> deepLinkParamsDictionary = null;
#if UNITY_IOS && !UNITY_EDITOR
              if (deepLinkEventArgs.deepLink.ContainsKey("click_event") && deepLinkEventArgs.deepLink["click_event"] != null)
              {
                  deepLinkParamsDictionary = deepLinkEventArgs.deepLink["click_event"] as Dictionary<string, object>;
              }
#elif UNITY_ANDROID && !UNITY_EDITOR
                  deepLinkParamsDictionary = deepLinkEventArgs.deepLink;
#endif

                break;
            case DeepLinkStatus.NOT_FOUND:
                AppsFlyer.AFLog("OnDeepLink", "Deep link not found");
                break;
            default:
                AppsFlyer.AFLog("OnDeepLink", "Deep link error");
                break;
        }
    }

    private void HandlerDLScene(string url)
    {
        // Update DeepLink Manager global variable, so URL can be accessed from anywhere.
        deeplinkURL = url;
        
// Decode the URL to determine action. 
// In this example, the application expects a link formatted like this:
// unitydl://mylink?scene1
        string sceneName = url.Split('?')[1].Split('&')[0];
        Debug.Log("sceneName: " + sceneName);

        bool validScene;
        switch (sceneName)
        {
            case "scene1":
                validScene = true;
                break;
            case "SampleScene":
                validScene = true;
                break;
            default:
                validScene = false;
                break;
        }
        if (validScene) SceneManager.LoadScene(sceneName);
    }
    
    void Update()
    {

    }

    // Mark AppsFlyer CallBacks
    public void onConversionDataSuccess(string conversionData)
    {
        AppsFlyer.AFLog("didReceiveConversionData", conversionData);
        Dictionary<string, object> conversionDataDictionary = AppsFlyer.CallbackStringToDictionary(conversionData);
        // add deferred deeplink logic here


    }

    public void onConversionDataFail(string error)
    {
        AppsFlyer.AFLog("didReceiveConversionDataWithError", error);
    }

    public void onAppOpenAttribution(string attributionData)
    {
        AppsFlyer.AFLog("onAppOpenAttribution", attributionData);
        Dictionary<string, object> attributionDataDictionary = AppsFlyer.CallbackStringToDictionary(attributionData);
        // add direct deeplink logic here
    }

    public void onAppOpenAttributionFailure(string error)
    {
        AppsFlyer.AFLog("onAppOpenAttributionFailure", error);
    }

}