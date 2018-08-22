using System;
using UnityEngine;
using UnityEngine.UI;

public class IronSourceAdManager : MonoBehaviour, AdvertisingManager
{
    [Serializable]
    public class perPlatformAppKey
    {
        public string androidKey;
        public string iosKey;
        //public string amazonKey;  // not yet obvious how to tell android and amazon apart
    };

    private static IronSourceAdManager instance;
    public perPlatformAppKey appKey;

    public bool testing;
    private bool isInitialized = false;
    private bool isBannerReady = false;
    private bool isBannerNeededAsSoonAsItIsReady = false;
    public bool autoReloadBannerAfterDismissed = true;
    private IronSourceBannerSize lastBannerSize;
    private IronSourceBannerPosition lastBannerPosition;
    private AdStatusCallback objectToCallback;


    private bool isShowingFakeBanner = false;

    public Action OnRewardedNeeded;

    //bool rewardedVideoAvaliable;
    bool interstitialVideoAvaliable;

    public static IronSourceAdManager Instance
    {
        get
        {
            return instance;
        }
    }

    void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        InitializeIronSourceForCurrentPlatform();
    }

    void InitializeIronSourceForCurrentPlatform()
    {
        //Initialization
        IronSource.Agent.reportAppStarted();

        IronSourceConfig.Instance.setClientSideCallbacks(true);
        IronSource.Agent.setUserId(IronSource.Agent.getAdvertiserId());

        string key = null;
#if UNITY_IOS
	    key = appKey.iosKey;

		IronSource.Agent.init(key, new string[] { IronSourceAdUnits.REWARDED_VIDEO, IronSourceAdUnits.BANNER, IronSourceAdUnits.INTERSTITIAL });
        

        //Rewarded Callbacks
        IronSourceEvents.onRewardedVideoAdOpenedEvent += RewardedVideoAdOpenedEvent;
        IronSourceEvents.onRewardedVideoAdClosedEvent += RewardedVideoAdClosedEvent;
        IronSourceEvents.onRewardedVideoAvailabilityChangedEvent += RewardedVideoAvailabilityChangedEvent;
        IronSourceEvents.onRewardedVideoAdStartedEvent += RewardedVideoAdStartedEvent;
        IronSourceEvents.onRewardedVideoAdEndedEvent += RewardedVideoAdEndedEvent;
        IronSourceEvents.onRewardedVideoAdRewardedEvent += RewardedVideoAdRewardedEvent;
        IronSourceEvents.onRewardedVideoAdShowFailedEvent += RewardedVideoAdShowFailedEvent;

        //Banner Callbacks
        IronSourceEvents.onBannerAdLoadedEvent += BannerAdLoadedEvent;
        IronSourceEvents.onBannerAdLoadFailedEvent += BannerAdLoadFailedEvent;        
        IronSourceEvents.onBannerAdClickedEvent += BannerAdClickedEvent; 
        IronSourceEvents.onBannerAdScreenPresentedEvent += BannerAdScreenPresentedEvent; 
        IronSourceEvents.onBannerAdScreenDismissedEvent += BannerAdScreenDismissedEvent;
        IronSourceEvents.onBannerAdLeftApplicationEvent += BannerAdLeftApplicationEvent;

        //Interstiial Callbakcs
        IronSourceEvents.onInterstitialAdReadyEvent += InterstitialAdReadyEvent;
        IronSourceEvents.onInterstitialAdLoadFailedEvent += InterstitialAdLoadFailedEvent;
        IronSourceEvents.onInterstitialAdShowSucceededEvent += InterstitialAdShowSucceededEvent;
        IronSourceEvents.onInterstitialAdShowFailedEvent += InterstitialAdShowFailedEvent;
        IronSourceEvents.onInterstitialAdClickedEvent += InterstitialAdClickedEvent;
        IronSourceEvents.onInterstitialAdOpenedEvent += InterstitialAdOpenedEvent;
        IronSourceEvents.onInterstitialAdClosedEvent += InterstitialAdClosedEvent;

        IronSource.Agent.loadInterstitial();

#elif UNITY_ANDROID
        key = appKey.androidKey;

        IronSource.Agent.init(key, new string[] { IronSourceAdUnits.REWARDED_VIDEO, IronSourceAdUnits.BANNER, IronSourceAdUnits.INTERSTITIAL });

        //Rewarded Callbacks
        IronSourceEvents.onRewardedVideoAdOpenedEvent += RewardedVideoAdOpenedEvent;
        IronSourceEvents.onRewardedVideoAdClosedEvent += RewardedVideoAdClosedEvent;
        IronSourceEvents.onRewardedVideoAvailabilityChangedEvent += RewardedVideoAvailabilityChangedEvent;
        IronSourceEvents.onRewardedVideoAdStartedEvent += RewardedVideoAdStartedEvent;
        IronSourceEvents.onRewardedVideoAdEndedEvent += RewardedVideoAdEndedEvent;
        IronSourceEvents.onRewardedVideoAdRewardedEvent += RewardedVideoAdRewardedEvent;
        IronSourceEvents.onRewardedVideoAdShowFailedEvent += RewardedVideoAdShowFailedEvent;

        //Banner Callbacks
        IronSourceEvents.onBannerAdLoadedEvent += BannerAdLoadedEvent;
        IronSourceEvents.onBannerAdLoadFailedEvent += BannerAdLoadFailedEvent;
        IronSourceEvents.onBannerAdClickedEvent += BannerAdClickedEvent;
        IronSourceEvents.onBannerAdScreenPresentedEvent += BannerAdScreenPresentedEvent;
        IronSourceEvents.onBannerAdScreenDismissedEvent += BannerAdScreenDismissedEvent;
        IronSourceEvents.onBannerAdLeftApplicationEvent += BannerAdLeftApplicationEvent;

        //Interstiial Callbakcs
        IronSourceEvents.onInterstitialAdReadyEvent += InterstitialAdReadyEvent;
        IronSourceEvents.onInterstitialAdLoadFailedEvent += InterstitialAdLoadFailedEvent;
        IronSourceEvents.onInterstitialAdShowSucceededEvent += InterstitialAdShowSucceededEvent;
        IronSourceEvents.onInterstitialAdShowFailedEvent += InterstitialAdShowFailedEvent;
        IronSourceEvents.onInterstitialAdClickedEvent += InterstitialAdClickedEvent;
        IronSourceEvents.onInterstitialAdOpenedEvent += InterstitialAdOpenedEvent;
        IronSourceEvents.onInterstitialAdClosedEvent += InterstitialAdClosedEvent;

        IronSource.Agent.loadInterstitial();

#endif
        isInitialized = true;
    }

    #region AdvertisingManager implementation

    public bool isVideoReady()
    {
        return IronSource.Agent.isRewardedVideoAvailable();
    }

    public void ShowRewardedAd(AdStatusCallback _objectToCallback)
    {
        objectToCallback = _objectToCallback;

#if UNITY_EDITOR
        doRewardUser();
#else
        if (!isVideoReady())
        {
            
        }
        else
        {
            IronSource.Agent.showRewardedVideo("VideoPlacement");
        }
#endif
    }

    public void ShowInterstitial()
    {
        if(IronSource.Agent.isInterstitialReady())
        {
            IronSource.Agent.showInterstitial();
            IronSource.Agent.loadInterstitial();
        }
    }

    private void doRewardUser()
    {
        if (objectToCallback != null)
        {
            objectToCallback.onVideoAdWatched(VideoResult.RewardPlayer);
        }
    }

    private void videoCancelled()
    {
        if (objectToCallback != null)
        {
            objectToCallback.onVideoFailed();
        }
    }

    #endregion

    #region Banner Related
    public bool IsBannerReady()
    {
        return isBannerReady;
    }

    public void ShowBanner(IronSourceBannerSize desiredSize, IronSourceBannerPosition desiredPosition)
    {
        Debug.LogFormat("Trying to load Banner: {0} {1}. IsInitialized:{2}", desiredSize, desiredPosition, isInitialized);

#if !UNITY_EDITOR
        //BANNER->Best for Phones & Tablets	320X50	Admob, Facebook, InMobi 
        //TABLET_BANNER-> Best for Tablets and Larger Devices	728X90  Admob, Facebook, InMobi
        //LARGE_BANNER->Best for Tablets and Larger Devices 320X90  Admob, Facebook,
        // RECTANGLE_BANNER->Best for Scrollable Feeds or Between Game Levels 300X250 Admob, Facebook, InMobi

        //TOP->Banner will be positioned at the top center of the screen
        //BOTTOM->Banner will be positioned at the bottom center of the screen
        lastBannerPosition = desiredPosition;
        lastBannerSize = desiredSize;
        if(!isInitialized)
        {
            isBannerNeededAsSoonAsItIsReady = true;
        }
        else
        {
            IronSource.Agent.loadBanner(desiredSize, desiredPosition);
        }
#else
        lastBannerPosition = desiredPosition;
        lastBannerSize = desiredSize;
        //show fake banner
        isShowingFakeBanner = true;
#endif
    }

    public void HideBanner()
    {
        isShowingFakeBanner = false;
        IronSource.Agent.hideBanner();
    }

    public void DisplayBanner()
    {
        isShowingFakeBanner = true;
        IronSource.Agent.displayBanner();
    }

    public void DestroyCurrentBannerAndGetAnother(IronSourceBannerSize desiredSize, IronSourceBannerPosition desiredPosition)
    {
        Debug.Log("Destroying current Banner");
        IronSource.Agent.destroyBanner();
        ShowBanner(desiredSize, desiredPosition);
    }

    #endregion

    #region Rewarded Video Callbacks

    //Invoked when the RewardedVideo ad view has opened.
    //Your Activity will lose focus. Please avoid performing heavy 
    //tasks till the video ad will be closed.
    void RewardedVideoAdOpenedEvent()
    {
    }
    //Invoked when the RewardedVideo ad view is about to be closed.
    //Your activity will now regain its focus.
    void RewardedVideoAdClosedEvent()
    {
        //Ad was closed, reset the flag.
        // rewardedVideoAvaliable = false;

    }
    //Invoked when there is a change in the ad availability status.
    //@param - available - value will change to true when rewarded videos are available. 
    //You can then show the video by calling showRewardedVideo().
    //Value will change to false when no videos are available.
    void RewardedVideoAvailabilityChangedEvent(bool available)
    {
        //Change the in-app 'Traffic Driver' state according to availability.
        //Ad is available, turn on flag.
        //rewardedVideoAvaliable = available;
    }
    //Invoked when the video ad starts playing.
    void RewardedVideoAdStartedEvent()
    {
#if UNITY_IOS
            Time.timeScale = 0;
#endif
    }
    //Invoked when the video ad finishes playing.
    void RewardedVideoAdEndedEvent()
    {
#if UNITY_IOS
            Time.timeScale = 1;
#endif

    }
    //Invoked when the user completed the video and should be rewarded. 
    //If using server-to-server callbacks you may ignore this events and wait for 
    //the callback from the ironSource server.
    //@param - placement - placement object which contains the reward data
    void RewardedVideoAdRewardedEvent(IronSourcePlacement placement)
    {
        /*Debug.Log("Finished video:" + finished);
        if (finished)*/

        //Reward Player Function
        doRewardUser();

#if UNITY_IOS
        Time.timeScale = 1;
#endif
        if (OnRewardedNeeded != null)
        {
            OnRewardedNeeded();
        }

    }
    //Invoked when the Rewarded Video failed to show
    //@param description - string - contains information about the failure.
    void RewardedVideoAdShowFailedEvent(IronSourceError error)
    {
        videoCancelled();
        // Couldn't display
        Debug.LogFormat("<color=red>IronSource Rewarded Ad Show Failed: </color>" + error.ToString());
#if UNITY_IOS
            Time.timeScale = 1;
#endif
    }

    #endregion

    #region Banner Callbacks
    //Invoked once the banner has loaded
    void BannerAdLoadedEvent()
    {
        Debug.Log("Banner Loaded");
        isBannerReady = true;
        if (isBannerNeededAsSoonAsItIsReady)
        {
            isBannerNeededAsSoonAsItIsReady = false;
            ShowBanner(lastBannerSize, lastBannerPosition);
        }
    }
    //Invoked when the banner loading process has failed.
    //@param description - string - contains information about the failure.
    void BannerAdLoadFailedEvent(IronSourceError error)
    {
        Debug.Log("Banner Failed: " + error.getDescription());
        isBannerReady = false;
    }
    // Invoked when end user clicks on the banner ad
    void BannerAdClickedEvent()
    {
    }
    //Notifies the presentation of a full screen content following user click
    void BannerAdScreenPresentedEvent()
    {
    }
    //Notifies the presented screen has been dismissed
    void BannerAdScreenDismissedEvent()
    {
        Debug.Log("Banner AdScreenDismissed. AutoReloadNewBanner: " + autoReloadBannerAfterDismissed);
        if (autoReloadBannerAfterDismissed)
        {
            DestroyCurrentBannerAndGetAnother(lastBannerSize, lastBannerPosition);
        }
    }
    //Invoked when the user leaves the app
    void BannerAdLeftApplicationEvent()
    {
    }
    #endregion

    #region Interstitial Callbacks

    //Invoked when the initialization process has failed.
    //@param description - string - contains information about the failure.
    void InterstitialAdLoadFailedEvent(IronSourceError error)
    {
    }
    //Invoked right before the Interstitial screen is about to open.
    void InterstitialAdShowSucceededEvent()
    {
    }
    //Invoked when the ad fails to show.
    //@param description - string - contains information about the failure.
    void InterstitialAdShowFailedEvent(IronSourceError error)
    {
    }
    // Invoked when end user clicked on the interstitial ad
    void InterstitialAdClickedEvent()
    {
    }
    //Invoked when the interstitial ad closed and the user goes back to the application screen.
    void InterstitialAdClosedEvent()
    {
        IronSource.Agent.loadInterstitial();
    }
    //Invoked when the Interstitial is Ready to shown after load function is called
    void InterstitialAdReadyEvent()
    {
    }
    //Invoked when the Interstitial Ad Unit has opened
    void InterstitialAdOpenedEvent()
    {
    }

    #endregion

    #region Utils
    public float GetDeviceInches()
    {
        float screenWidth = Screen.width / Screen.dpi;
        float screenHeight = Screen.height / Screen.dpi;
        float diagonalInches = Mathf.Sqrt(screenWidth * screenWidth + screenHeight * screenHeight);

        return diagonalInches;
    }
    #endregion

 

#if UNITY_EDITOR
    private void OnGUI()
    {
        if (isShowingFakeBanner)
        {
            Rect bannerRect;
            if (lastBannerPosition == IronSourceBannerPosition.TOP)
            {
                bannerRect = new Rect(0, 0, Screen.width, Screen.height * 0.1f);
            }
            else
            {
                bannerRect = new Rect(0, Screen.height * 0.1f, Screen.width, Screen.height * 0.1f);
            }
            GUI.Box(bannerRect, "I am a banner");
        }
    }
#endif
}
