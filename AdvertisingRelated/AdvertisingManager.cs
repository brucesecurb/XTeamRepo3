using UnityEngine;
using System.Collections;

// This advertising manager implements ads using Appodeal
// It shouldn't be too hard to replace, although it may need to be adjusted

public enum VideoResult
{
    RewardPlayer,
    DoNotRewardPlayer
}

public interface AdStatusCallback
{
    void onVideoAdWatched(VideoResult result);
    void onVideoFailed();
}

/*
public interface AdReadyCallback
{
    void onVideoAdIsReady();
    void onVideoAdIsNotReady();
}
*/

public interface AdvertisingManager
{
    bool isVideoReady();

    void ShowRewardedAd(AdStatusCallback objectToCallback);
}
