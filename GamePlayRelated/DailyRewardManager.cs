using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Analytics;

public class DailyRewardManager : MonoBehaviour {

	[Header("Rewards")] 
	[SerializeField]
	private int day1Reward;
	[SerializeField]
	private int day2Reward;
	[SerializeField]
	private int day3Reward;
	[SerializeField]
	private int day4Reward;
	[SerializeField]
	private int day5Reward;
	[SerializeField]
	private int day6Reward;
	[SerializeField]
	private int day7Reward;

	[Header("UI elements")]
	[SerializeField]
	private GameObject ui;
	[SerializeField]
	private GameObject dailyRewardPopUp;
	[SerializeField]
	private Text rewardText;
	[SerializeField]
	private Text earnedCubesText;

	[Header("Reward schedule")]
	[SerializeField]
	private Image[] days;
	[SerializeField]
	private Text[] dayText;
	[SerializeField]
	private Text[] dayAmountText;
	[SerializeField]
	private Sprite dailyPrevious;
	[SerializeField]
	private Sprite dailyCurrent;
	[SerializeField]
	private Sprite dailyNext;
	[SerializeField]
	private Sprite dailyUpcoming;

	private int totalCubes;

	//
	// U n i t y
	//

	void Start () {
		InitializePlayerPrefs ();
		dayAmountText [0].text = day1Reward.ToString ();
		dayAmountText [1].text = day2Reward.ToString ();
		dayAmountText [2].text = day3Reward.ToString ();
		dayAmountText [3].text = day4Reward.ToString ();
		dayAmountText [4].text = day5Reward.ToString ();
		dayAmountText [5].text = day6Reward.ToString ();
		dayAmountText [6].text = day7Reward.ToString ();
		CheckDay ();
	}

	//
	// U s e r
	//

	void InitializePlayerPrefs()
	{
		if (!PlayerPrefs.HasKey ("rewardedDay"))
		{
			PlayerPrefs.SetString ("rewardedDay", "");
		}

		if (!PlayerPrefs.HasKey ("rewardedDayNumber"))
		{
			PlayerPrefs.SetInt ("rewardedDayNumber", 0);
		}

		if (!PlayerPrefs.HasKey ("dayCounter"))
		{
			PlayerPrefs.SetInt ("dayCounter", 0);
		}
	}
		
	void CheckDay()
	{
		// We ask if the current day is the same as the saved one
		if (PlayerPrefs.GetString ("rewardedDay").Equals (DateTime.Now.DayOfWeek.ToString()))
		{
			//Debug.Log ("SAME DAY");
			//Debug.Log (DateTime.Now.DayOfWeek.ToString ());
			SameDay ();
		}
		else
		{
			//Debug.Log ("DIFFERENT DAY");
			//Debug.Log (DateTime.Now.DayOfWeek.ToString ());
			DifferentDay ();
		}
	}

	void SameDay()
	{
		// If the day counter equals to 7, means that the rewards have to restart
		if (PlayerPrefs.GetInt ("dayCounter") == 7)
		{
			RestartRewards ();
		}
	}

	void DifferentDay()
	{
		PlayerPrefs.SetString ("rewardedDay", DateTime.Now.DayOfWeek.ToString ());

		// We know that the current day is the next one of the rewarded
		if (DateTime.Now.DayOfYear == PlayerPrefs.GetInt ("rewardedDayNumber") + 1
		    || PlayerPrefs.GetInt ("rewardedDayNumber") == 0)
		{
			if (PlayerPrefs.GetInt ("dayCounter") == 7)
			{
				RestartRewards ();
			}
			else
			{
				PlayerPrefs.SetInt ("rewardedDayNumber", DateTime.Now.DayOfYear);
				int counter = PlayerPrefs.GetInt ("dayCounter");
				PlayerPrefs.SetInt ("dayCounter", ++counter);
				ChangeDays (counter);
				GiveReward (counter);
			}
		}
		else
		{
			RestartRewards ();
		}
	}

	void RestartRewards()
	{
		PlayerPrefs.SetString ("rewardedDay", DateTime.Now.DayOfWeek.ToString ());
		PlayerPrefs.SetInt ("rewardedDayNumber", DateTime.Now.DayOfYear);
		PlayerPrefs.SetInt ("dayCounter", 1);
		int currentDay = PlayerPrefs.GetInt ("dayCounter");
		ChangeDays (currentDay);
		GiveReward (currentDay);
	}

	void ChangeDays(int rewardedDay)
	{
		bool isPrevious = true;
		bool isNext = false;
		bool isUpcoming = false;
		for (int i = 0; i < days.Length; i++)
		{
			if (i == rewardedDay - 1)
			{
				days [i].sprite = dailyCurrent;
				dayText [i].text = "Today";
				isPrevious = false;
				isNext = true;
			}
			else
			{
				dayText [i].text = "Day " + (i + 1);

				if (isPrevious)
				{
					days [i].sprite = dailyPrevious;
				}
				else if (isNext)
				{
					days [i].sprite = dailyNext;
					isNext = false;
					isUpcoming = true;
				}
				else if (isUpcoming)
				{
					days [i].sprite = dailyUpcoming;
				}
			}

		}
	}

	void GiveReward(int rewardedDay)
	{
		totalCubes = PlayerPrefs.GetInt("CubesPickup");
		switch (rewardedDay)
		{
		case 1:
			rewardText.text = "+ " + day1Reward + " cubes";
			totalCubes += day1Reward;
			break;
		case 2:
			rewardText.text = "+ " + day2Reward + " cubes";
			totalCubes += day2Reward;
			break;
		case 3:
			rewardText.text = "+ " + day3Reward + " cubes";
			totalCubes += day3Reward;
			break;
		case 4:
			rewardText.text = "+ " + day4Reward + " cubes";
			totalCubes += day4Reward;
			break;
		case 5:
			rewardText.text = "+ " + day5Reward + " cubes";
			totalCubes += day5Reward;
			break;
		case 6:
			rewardText.text = "+ " + day6Reward + " cubes";
			totalCubes += day6Reward;
			break;
		case 7:
			rewardText.text = "+ " + day7Reward + " cubes";
			totalCubes += day7Reward;
			break;
		default:
			totalCubes = PlayerPrefs.GetInt("CubesPickup");
			break;
		}
        if (ui != null)
        {
            ui.SetActive(false);
        }
		dailyRewardPopUp.SetActive (true);
		// Analytics
		Analytics.CustomEvent ("Reward Day " + rewardedDay, new Dictionary<string, object>{ });
		PlayerPrefs.SetInt ("CubesPickup", totalCubes);
	}

	//
	// U s e r - P u b l i c
	//

	public void ClosePopUp()
	{
		ui.SetActive (true);
		dailyRewardPopUp.SetActive (false);
		earnedCubesText.text = "x " + PlayerPrefs.GetInt ("CubesPickup");
	}
}

