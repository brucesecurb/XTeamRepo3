using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// OptionsPanel.
/// This script shows and updates the options buttons.
/// </summary> 

public class OptionsPanel : MonoBehaviour {

	public GameObject optionsButtons;
	public GameObject settingsButton;
	public GameObject helpButton;
	public GameObject audioToggleButton;
	private Image audioToggleIcon;
	public GameObject fullscreenToggleButton;
	private Image fullscreenToggleIcon;
	public GameObject effectsQualityToggleButton;
	private Image effectsQualityToggleIcon;
	public GameObject leaderboardsButton;

	private Atlas hammer2OptionsAtlas;

	void Awake () {

		// [MOBILE] We scale the options (top-right) buttons a bit larger so they're easier to tap!
		if (GameData.mobile) optionsButtons.GetComponent<RectTransform>().localScale = Vector3.one;

		audioToggleIcon = audioToggleButton.transform.Find("Icon").GetComponent<Image>();
		fullscreenToggleIcon = fullscreenToggleButton.transform.Find("Icon").GetComponent<Image>();
		effectsQualityToggleIcon = effectsQualityToggleButton.transform.Find("Icon").GetComponent<Image>();

		hammer2OptionsAtlas = AtlasManager.hammer2OptionsAtlas;

		// [MOBILE] Since we don't have leaderboards, fullscreen or a quality toggle we hide them
		// In theory you could have multiple quality settings, but we will let you decide on that!
		if (GameData.mobile){
			leaderboardsButton.SetActive(false);
			fullscreenToggleButton.SetActive(false);
			effectsQualityToggleButton.SetActive(false);
		}

	}

	// As long as this panel and script is active,
	// some buttons need to be changed depending on whether conditions have changed.
	// So that is why we have stuff running on Update.
	void Update ()
	{
		AudioToggleSet(); // toggle audio button
		// [MOBILE] Only run these functions when you're not a mobile version.
		if(!GameData.mobile){
			ButtonsSet();
			FullscreenToggleSet();
			EffectsQualitySet();
		}
	}

	private void ButtonsSet()
	{
		leaderboardsButton.SetActive(false); // (DG): No leaderboards yet.
	}

	private void AudioToggleSet()
	{
		if (Data.muteAllSound || !Data.sfx && !Data.music) audioToggleIcon.sprite =  hammer2OptionsAtlas.Get("SoundOff");
		else audioToggleIcon.sprite = hammer2OptionsAtlas.Get("SoundOn");
	}
	
	private void FullscreenToggleSet()
	{
		if (Screen.fullScreen)	fullscreenToggleIcon.sprite = hammer2OptionsAtlas.Get("Windowed");
		else fullscreenToggleIcon.sprite =  hammer2OptionsAtlas.Get("Fullscreen");
	}
	
	private void EffectsQualitySet()
	{
		switch (Data.quality)
		{
		case "Fantastic":
			effectsQualityToggleIcon.sprite = hammer2OptionsAtlas.Get("QualityHigh");
			break;
		case "Simple":
			effectsQualityToggleIcon.sprite = hammer2OptionsAtlas.Get("QualityMedium");
			break;
		case "Fastest":
			effectsQualityToggleIcon.sprite = hammer2OptionsAtlas.Get("QualityLow");
			break;
		}
	}
}
