using UnityEngine;
using System.Collections;
using GameSparks.Api.Responses;
using GameSparks.Core;
using System;
using Crosstales.BWF;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameSparksDeviceLogin : MonoBehaviour {

    public GameObject inputText;
    public GameObject platoonPanel;
    public LoginPanelData loginPanel;
	public GameSparksErrorManager socialErrorManager;

    public GameObject updateButton;
    public GameObject updatePanel;

    public static bool alreadyLoggedIn = false;

	private string newName;

    public void Awake()
    {
        GameSparksInitialConfig();
    }

    public void GameSparksInitialConfig(){
        ObjectsForGamesparks objectsNeeded = GameObject.Find("ObjectsForGamesparks").GetComponent<ObjectsForGamesparks>();
        inputText = objectsNeeded.inputText;
        platoonPanel = objectsNeeded.platoonPanel;
        loginPanel = objectsNeeded.loginPanel;
    }

    public void registerButtonCallback(){
        string userName = loginPanel.loginInput.text;
        if(userName != ""){
            if(!BWFManager.Contains(userName) && userName != "Rookie"){
                loginPanel.ErrorString = "";
                loginPanel.loginStarted();

                Debug.Log("Updating Player...");
//                new GameSparks.Api.Requests.RegistrationRequest()
//                              .SetDisplayName(userName)
//                              .SetUserName(SystemInfo.deviceUniqueIdentifier)
//                              .SetPassword("")
//                              .Send(RegisterGameSparksResponse);
				newName = userName;
				new GameSparks.Api.Requests.ChangeUserDetailsRequest ()
					.SetDisplayName (userName)
					.Send (ChangeDisplayNameResponse);
            }
			else
			{
				loginPanel.ErrorString = socialErrorManager.GetErrorMessage ("BAD_WORD");
            }
        }
		else
		{
			loginPanel.ErrorString = socialErrorManager.GetErrorMessage ("EMPTY_INPUT_LOGIN");
        }
    }

	public void ShowLoginPanel()
	{
		loginPanel.gameObject.SetActive (true);
        loginPanel.loginInput.text = GetRandomPlayerName();
		loginPanel.loginFailed ();
	}

    void Start()
    {
        Debug.Log("Authenticating Device...");
        loginPanel.loginStarted();
        new GameSparks.Api.Requests.AuthenticationRequest()
                      .SetUserName(SystemInfo.deviceUniqueIdentifier)
                      .SetPassword("")
                      .Send(DeviceAuthGameSparksResponse);
    }
   

    private void RegisterGameSparksResponse(RegistrationResponse response)
    {

        if (!response.HasErrors)
        {
            Debug.Log("Player Authenticated... \n User Name: " + response.DisplayName);
            GameSparksPlayerData.playerID = response.UserId;
            GameSparksPlayerData.playerName = response.DisplayName;

            loginPanel.gameObject.SetActive(false);
            GameSparksDeviceLogin.alreadyLoggedIn = true;
            platoonPanel.SetActive(true);
            ServiceLocator.SaveSystem.GetPlayerDataFromServer();
            this.GetComponent<GameSparksMessageManager>().ProcessGenericMessages();
			ServiceLocator.AnalyticsDispatcher.DispatchAnalytics ("playerRegistered");

			if (ABTest_SkipTutorial.flag && ServiceLocator.tutorialManager.skipButton != null)
			{
				ServiceLocator.tutorialManager.skipButton.GetComponent<Button> ().interactable = true;
			}
        }
        else
        {
            loginPanel.loginFailed();
			loginPanel.ErrorString = "Check your internet connection";
			IDictionary<string, object> errorDictionary = GSJson.From (response.Errors.JSON.ToString ()) as IDictionary<string, object>;
			loginPanel.ErrorString = socialErrorManager.GetErrorMessage (errorDictionary ["error"].ToString());
            Debug.Log("Error Authenticating Player... \n " + response.Errors.JSON.ToString());
        }
    }

	private void ChangeDisplayNameResponse(ChangeUserDetailsResponse response)
	{
		if (!response.HasErrors)
		{
			GameSparksPlayerData.playerName = newName;
			loginPanel.gameObject.SetActive(false);
			ServiceLocator.SaveSystem.SetPlayerName (newName);
			ServiceLocator.AnalyticsDispatcher.DispatchAnalytics ("playerNameUpdated");
			Invoke ("UpdateName", 0.5f);
		}
		else
		{
			Debug.Log("Error while changing the Player info... \n " + response.Errors.JSON.ToString());
		}
	}

    public void DeviceAuthGameSparksResponse(AuthenticationResponse response)
    {
        if (!response.HasErrors)
        {
            Debug.Log("Player Authenticated... \n User Name: " + response.DisplayName);
            GameSparksPlayerData.playerID = response.UserId;
            GameSparksPlayerData.playerName = response.DisplayName;
            GameSparksDeviceLogin.alreadyLoggedIn = true;

			PlayerPrefsXGameSparksSaveData.GetSavedDecks ();
			PlayerPrefsXGameSparksSaveData.GetSavedCards ();
			PlayerPrefsXGameSparksSaveData.GetSelectedCharacter ();

            loginPanel.gameObject.SetActive(false);
            platoonPanel.SetActive(true);
            ServiceLocator.SaveSystem.GetPlayerDataFromServer();
            this.GetComponent<GameSparksMessageManager>().ProcessGenericMessages();
			ServiceLocator.AnalyticsDispatcher.DispatchAnalytics ("playerAuthenticated");
            CheckForUpdates();

			if (ABTest_SkipTutorial.flag && ServiceLocator.tutorialManager.skipButton != null)
			{
				ServiceLocator.tutorialManager.skipButton.GetComponent<Button> ().interactable = true;
			}
        }
        else
        {
            loginPanel.loginFailed();
			RegisterPlayerAutomatically ();
            Debug.Log("Error Authenticating Player... \n " + response.Errors.JSON.ToString());
        }
    }

    public void CheckForUpdates(){
        new GameSparks.Api.Requests.LogEventRequest()
                      .SetEventKey("GET_GAME_VERSION")
                      .Send(CheckForUpdatesResponse);
    }

    public void SetLastLoginTime() {
        new GameSparks.Api.Requests.LogEventRequest()
                      .SetEventKey("SET_LAST_LOGIN_TIME ")
                      .Send(SetLastLoginTimeResponse);
    }

    private void SetLastLoginTimeResponse(LogEventResponse response) {
        if (!response.HasErrors) {
            Debug.Log("LoginSet correctly");
        } else {
            Debug.Log("Error while getting the update version... \n " + response.Errors.JSON.ToString());
        }
    }

    public void CheckForUpdatesResponse(LogEventResponse response)
    {
        if (!response.HasErrors)
        {
            float versionNumber = response.ScriptData.GetFloat("versionNumber").Value;
            bool invasive = response.ScriptData.GetBoolean("invasive").Value;

            if(float.Parse(Application.version) < versionNumber){
                updateButton.SetActive(true);
                if (invasive)
                {
                    updatePanel.SetActive(true);
                }
            }
        }
        else
        {
            Debug.Log("Error while getting the update version... \n " + response.Errors.JSON.ToString());
        }
    }

	private void UpdateName()
	{
		MainMenuV2.UpdateUI (true);
	}

	// Temporary workaround for register players automatically

	private string[] nameArray1 = new string[]{"Ninja", "Eagle", "Hotdog", "The King", "Shiny", "User", "Mr awesome", "Random", "Robo", "Ghost", "Lord", "Bubblegum"};
	private string[] nameArray2 = new string[]{" Pyjamas", " Fights", " Night", " City", " Hero", " Man", " Forest", " Jungle", " Toy", ""};


	private void RegisterPlayerAutomatically()
	{
//		string temporalName = GetRandomPlayerName ();
//
//		#if UNITY_ANDROID
//			temporalName = GameSparksPlayerData.playerName;
//		#endif
//
//		if (temporalName.Equals ("") || temporalName.Equals (null))
//		{
//			temporalName = GetRandomPlayerName ();
//		}

		new GameSparks.Api.Requests.RegistrationRequest()
			.SetDisplayName("Rookie")
			.SetUserName(SystemInfo.deviceUniqueIdentifier)
			.SetPassword("")
			.Send(RegisterGameSparksResponse);
	}

	private string GetRandomPlayerName()
	{
		UnityEngine.Random.InitState( System.Environment.TickCount);
		string randomName = nameArray1 [UnityEngine.Random.Range(0, nameArray1.Length)];
		randomName += nameArray2 [UnityEngine.Random.Range (0, nameArray2.Length)];

		return randomName;
	}


}
