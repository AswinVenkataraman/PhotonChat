using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Chat;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
public class PhotonChat : MonoBehaviour, IChatClientListener
{

    //Photon Chat Related Settings
    ChatClient photonChat;
    const string chatAppId = "ab959705-ce5f-4a0b-88dc-3dbd0f4f5205";
    const string chatAppVersion = "0.1";

    //Unique ID Of The User
    string userID;
    //Channel ID
    string channelID = "1"; 

    public GameObject chatObject;
    public GameObject bgOverlay;
    TMP_InputField inputText;
    Text sendButtonText;

    //Message Related
    string lastSenderName = "";
    const int Max_Messages = 50;
    public GameObject messageObj;
    public GameObject scrollContent;
    RectTransform scrollRect;

    //List for mutedPlayers during gameplay.
    List<string> mutedPlayers;

    //Print debug statements if enabled
    public bool isDebugEnabled;

    void Start()
    {

        mutedPlayers = new List<string>();
        userID = "User_" + Random.Range(0, 1000);

        photonChat = new ChatClient(this);
        photonChat.ChatRegion = "ASIA";
        photonChat.Connect(chatAppId, chatAppVersion, new AuthenticationValues(userID));

        inputText = chatObject.transform.Find("InputField").GetComponent<TMP_InputField>();
        sendButtonText = chatObject.transform.Find("Send").GetComponentInChildren<Text>();

        chatObject.SetActive(false);
        bgOverlay.SetActive(false);

        scrollRect = scrollContent.GetComponent<RectTransform>();
        inputText.onValueChanged.AddListener(OnChatTextEntered);

    }

    void Update()
    {
        if(photonChat!=null)
        photonChat.Service();
    }

    public void OnChatButtonClicked()
    {
       chatObject.SetActive(!chatObject.activeInHierarchy);
       bgOverlay.SetActive(!bgOverlay.activeInHierarchy);
    }

    public void OnChatSendClicked()
    {
        if (string.IsNullOrEmpty(inputText.text))
            return;

        if(photonChat.PublishMessage(channelID, inputText.text))
        inputText.text = string.Empty;
    }

    public void OnChatTextEntered(string message)
    {
        sendButtonText.text = "Send\n" + message.Length + "/100";
    }

    public void OnPlayerMuted()
    {
       string blockedUserName = EventSystem.current.currentSelectedGameObject.transform.parent.transform.Find("UserName").GetComponent<Text>().text;
       mutedPlayers.Add(blockedUserName);
    }

    public void OnChatStateChange(ChatState state)
    {
        if(isDebugEnabled)
        Debug.Log(" OnChatStateChange = " + state);
    }

    public void OnConnected()
    {
        if (isDebugEnabled)
            Debug.Log("OnConnected");
        photonChat.Subscribe(channelID);
    }

    public void OnDisconnected()
    {
        if (isDebugEnabled)
            Debug.Log("OnDisconnected Reason = " + photonChat.DisconnectedCause);
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {        
        int i = 0;

        foreach (string sender in senders)
        {

            if (mutedPlayers.Contains(sender))
                continue;

            GameObject msgObj;

            if (scrollContent.transform.childCount < Max_Messages)
                msgObj = GameObject.Instantiate(messageObj, scrollContent.transform);
            else
            {
                msgObj = scrollContent.transform.GetChild(0).gameObject;
                msgObj.transform.SetAsLastSibling();
            }

            msgObj.transform.Find("Message").GetComponent<Text>().text = messages[i].ToString();
            msgObj.transform.Find("UserName").GetComponent<Text>().text = sender;

            if (string.Equals(sender, userID))
                msgObj.transform.Find("Mute").gameObject.SetActive(false);
            else
                msgObj.transform.Find("Mute").GetComponent<Button>().onClick.AddListener(OnPlayerMuted);

            lastSenderName = sender;
            i++;
        }

        if(scrollContent.transform.childCount > 4)
            scrollRect.sizeDelta = new Vector2(scrollRect.sizeDelta.x, (scrollContent.transform.childCount * 40) + 5);

    }

    public void OnPrivateMessage(string sender, object message, string channelName)
    {

    }

    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {

    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        if (isDebugEnabled)
            Debug.Log("OnSubscribed To " + channels[0]);
    }

    public void OnUnsubscribed(string[] channels)
    {
        if (isDebugEnabled)
            Debug.Log("OnUnsubscribed To " + channels[0]);

    }

    public void OnUserSubscribed(string channel, string user)
    {
        if (isDebugEnabled)
            Debug.Log("OnUserSubscribed " + channel + " User = "+user);

    }

    public void OnUserUnsubscribed(string channel, string user)
    {
        if (isDebugEnabled)
            Debug.Log("OnUserUnsubscribed " + channel + " User = " + user);
    }

    public void DebugReturn(DebugLevel level, string message)
    {

    }

}
