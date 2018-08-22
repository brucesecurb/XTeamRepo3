using UnityEngine;
using System.Collections;
using GameSparks.Api.Messages;
using GameSparks.Core;
using System.Collections.Generic;

public class GameSparksMessageManager : MonoBehaviour {

    public GameSparksTeamChatManager teamChatManager;
    public PlatoonManager platoonManager;
    public PanelSwitch platoonKickedPanel;
	public PlatoonMembersManager platoonMembersManager;

    public void Awake()
    {
        GameSparksInitialConfig();
    }

    public void GameSparksInitialConfig()
    {
        ObjectsForGamesparks objectsNeeded = GameObject.Find("ObjectsForGamesparks").GetComponent<ObjectsForGamesparks>();
        teamChatManager = objectsNeeded.teamChatManager;
        platoonMembersManager = objectsNeeded.platoonMembersManager;
        platoonKickedPanel = objectsNeeded.platoonKickedPanel;
        platoonManager = objectsNeeded.platoonManager;
    }

    public void ProcessGenericMessages()
    {
        Debug.Log("Register to message listener");
        ScriptMessage.Listener += ListenerForGenericMessages;
    }

    private void ListenerForGenericMessages(ScriptMessage message)
    {
        Debug.Log("message received: " + message.ExtCode);
        switch(message.ExtCode){
            case CloudRelatedStringDefinitions.JoinTeamMessage:
                if (teamChatManager.gameObject.activeSelf)
                {
                    teamChatManager.GetMyTeamsRequest();
                }
                break;
            case CloudRelatedStringDefinitions.LeaveTeamMessage:
                if(teamChatManager.gameObject.activeSelf){
                    teamChatManager.GetMyTeamsRequest();
                }
                break;
            case CloudRelatedStringDefinitions.KickTeamMessage:
                {
                    GSData messageData = message.Data;
                    IDictionary<string, object> receivedData = GSJson.From(messageData.JSON) as IDictionary<string, object>;
                    string playerID = receivedData["playerKickedID"] as string;
                    if (playerID == GameSparksPlayerData.playerID){
                        platoonKickedPanel.switchGameObjects();
                        platoonManager.GetClanList();
                    }else{
                        teamChatManager.GetMyTeamsRequest();
                    }
                }
                break;
            case CloudRelatedStringDefinitions.TeamMemberRequestJoin:
			{
                    GSData messageData = message.Data;
                    IDictionary<string, object> receivedData = GSJson.From(messageData.JSON) as IDictionary<string, object>;
					string ownerID = receivedData["platoonOwner"] as string;
					if (ownerID.Equals (GameSparksPlayerData.playerID))
					{
						platoonMembersManager.GetPlatoonWaitingListRequest ();
					}
                }
                break;
            case CloudRelatedStringDefinitions.TeamMemberRequestDenied:
                {
                    GSData messageData = message.Data;
                    IDictionary<string, object> receivedData = GSJson.From(messageData.JSON) as IDictionary<string, object>;
                    string playerID = receivedData["memberName"] as string;
					if (playerID.Equals (GameSparksPlayerData.playerName))
					{
						platoonManager.GetMyPlatoonRequest ();
					}
                }
                break;
            case CloudRelatedStringDefinitions.TeamMemberRequestAccepted:
                {
                    GSData messageData = message.Data;
                    IDictionary<string, object> receivedData = GSJson.From(messageData.JSON) as IDictionary<string, object>;
					string playerID = receivedData["memberID"] as string;
					if (playerID.Equals (GameSparksPlayerData.playerID))
					{
						platoonManager.RemoveFromWaitingList ();
						platoonManager.switchToTeamPanels.switchGameObjects();
                        teamChatManager.GetMyTeamsRequest();
					}
                }
                break;

            default:
                break;
        }

        /*GSData data = message.Data;
        string extCode = message.ExtCode;
        string messageId = message.MessageId;
        bool? notification = message.Notification;
        //GSEnumerable<GSData> scriptData = message.ScriptData;
        string subTitle = message.SubTitle;
        string summary = message.Summary;
        string title = message.Title;*/
    }
}
