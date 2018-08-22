using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using GameSparks.Core;
using GameSparks.Api;
using GameSparks.Api.Requests;
using GameSparks.Api.Responses;
using GameSparks.Api.Messages;
using System;
using System.Collections.Generic;
using Crosstales.BWF;

public class GameSparksTeamChatManager : MonoBehaviour {

    private GSEnumerable<CreateTeamResponse._Player> membersCreate;
    private GSEnumerable<JoinTeamResponse._Player> membersJoin;
    private CreateTeamResponse._Player ownerCreate;
    private JoinTeamResponse._Player ownerJoin;
    private GSEnumerable<ListTeamChatResponse._ChatMessage> messages;
    private GSData scriptData;
    private string teamId;
    private string teamName;
    private string messageForChat;
    private string teamType = "blocky_clan";
    


    [SerializeField]
    private InputField messageArea;
    [SerializeField]
    private int messageAmount;
    [SerializeField]
    private Text clanBlocks;

    public int teamMaxMembers = 30;
    public Text teamTitle;
    public Text teamMembersNumber;
    public MemberTableLogic membersTable;
    public ChatTableLogic chatTable;
    public PlatoonManager platoonManager;
    public PlatoonMembersManager platoonMembersManagers;
    void Start(){
        platoonMembersManagers = this.GetComponent<PlatoonMembersManager>();
        GetMyTeamsRequest();
    }

    public void GetMyTeamsRequest(){
        List<string> teams = new List<string>();
        teams.Add(teamType);
        new GetMyTeamsRequest()
            .SetTeamTypes(teams)
            .Send(GetMyTeamsRequestResponse);
    }

    private void GetMyTeamsRequestResponse(GetMyTeamsResponse response)
    {
        if(response.Teams != null){
            int memberCount = 0;
            foreach (GetMyTeamsResponse._Team team in response.Teams)
            {

                GameSparksPlayerData.playerTeam = team.TeamId;
                GameSparksPlayerData.playerTeamName = team.TeamName;

                Debug.Log(membersTable.cells.Length);
                foreach (GetMyTeamsResponse._Team._Player player in team.Members)
                {
                    membersTable.cells[memberCount].TeamMember = player.DisplayName;
					membersTable.cells [memberCount].MemberID = player.Id;
                    memberCount++;
                }
                for (int i = memberCount; i < membersTable.cells.Length; i++){
                    membersTable.cells[i].TeamMember = "";
                    membersTable.cells[i].gameObject.SetActive(false);
                }
                teamMembersNumber.text = memberCount + "/" + teamMaxMembers;
                Debug.Log("----TEAM_ "+team.TeamName);
                teamTitle.text = team.TeamName;
                Debug.Log("----TEAM SISI_ " + teamTitle.text);

                //Sorted member list code begisn here

                GSData scriptData = response.ScriptData;
                IDictionary<string, object> receivedData = GSJson.From(scriptData.JSON) as IDictionary<string, object>;
                List<object> data = receivedData["teamArray"] as List<object>;

                for (int i = 0; i < data.Count; i++)
                {
                    IDictionary<string, object> dictionaryHelper = data[i] as IDictionary<string, object>;
                    Debug.Log("Sorted Member List " + i + ": " + dictionaryHelper["playerID"].ToString());
                    membersTable.cells[i].TeamMember = dictionaryHelper["displayName"].ToString();
                    membersTable.cells[i].MemberID = dictionaryHelper["playerID"].ToString();
                    membersTable.cells[i].EtherBlocks = dictionaryHelper["cubes"].ToString();
                }

                clanBlocks.text = receivedData["clanTotalCubes"].ToString();
            }
            platoonMembersManagers.GetPlatoonOwnership();
            membersTable.UpdateTable();
            GetMessages();
            ProcessChatMessages();
        }

    }


    public void SendMessageToChat()
    {
        string messageToSend = BWFManager.ReplaceAll(messageArea.text);
        if(messageToSend != ""){
            new SendTeamChatMessageRequest()
                .SetMessage(messageToSend)
            //Player data Team ID
                .SetTeamId(GameSparksPlayerData.playerTeam)
            //Player data Team Type
                .SetTeamType(teamType)
            .Send(SendMessageToChatCallback);
        }
    }

    private void SendMessageToChatCallback(SendTeamChatMessageResponse response)
    {
        Debug.Log("Message Sent");
    }

    // Get an array of X number of messages
    public void GetMessages()
    {
        new ListTeamChatRequest()
        .SetEntryCount(messageAmount)
        //Player data Team ID
            .SetTeamId(GameSparksPlayerData.playerTeam)
        //Player data Team Type
            .SetTeamType(teamType)
        .Send(GetMessageResponseCallBack);
    }

    private void GetMessageResponseCallBack(ListTeamChatResponse response)
    {
        messages = response.Messages;
        //int count = 0;
        List<ChatTableLogic.PlayerMessageData> messageData = new List<ChatTableLogic.PlayerMessageData>();
        foreach (ListTeamChatResponse._ChatMessage msg in messages)
        {
            ChatTableLogic.PlayerMessageData data = new ChatTableLogic.PlayerMessageData();
            data.messages = msg.Message;
            data.player = msg.Who;
            data.playerID = msg.FromId;
            messageData.Add(data);
            //Debug.Log("Message " + count + ": " + msg.Message);
        }
        chatTable.setChatMessage(messageData.ToArray());
    }

    //...

    // Get the last message sent by anyone
    void ProcessChatMessages()
    {
        TeamChatMessage.Listener = ListenerForMessages;
    }

    private void ListenerForMessages(TeamChatMessage message)
    {
        string chatMessageId = message.ChatMessageId;
        string fromId = message.FromId;
        messageForChat = message.Message;
        string who = message.Who;

        chatTable.updateChatMessage(who, messageForChat, fromId);

    }
    //...
}
