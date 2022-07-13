using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class Launcher : MonoBehaviourPunCallbacks
{
    [SerializeField] private InputField nickname;
    [SerializeField] private InputField roomName;
    [SerializeField] private Button btnJoin;
    [SerializeField] private Button btnCreate;
    [SerializeField] private Text message;

    private bool isCreate;
    
    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = "1";
    }

    private void Start()
    {
        if (PlayerPrefs.HasKey("username"))
            nickname.text = PlayerPrefs.GetString("username");
        if (PlayerPrefs.HasKey("roomName"))
            roomName.text = PlayerPrefs.GetString("roomName");
    }


    public void JoinOrCreateRoom(bool isCreate)
    {
        PhotonNetwork.NickName = nickname.text;
        btnCreate.interactable = false;
        btnJoin.interactable = false;
        this.isCreate = isCreate;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnCreatedRoom()
    {
        print($"Room {roomName.text} has been created.");
    }

    public override void OnConnectedToMaster()
    {
        if (isCreate)
            PhotonNetwork.CreateRoom(roomName.text, new RoomOptions {MaxPlayers = 2});
        else
            PhotonNetwork.JoinRoom(roomName.text);
    }

    public override void OnJoinedRoom()
    {
        string roomName = PhotonNetwork.CurrentRoom.Name;
        print($"joined room: {roomName}");
        PhotonNetwork.LoadLevel("InGame");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        StartCoroutine(ShowMesasge(returnCode, message));
    }
    
    IEnumerator ShowMesasge(short returnCode, string message)
    {
        this.message.text = $"Error {returnCode}: {message}";
        yield return new WaitForSeconds(3);
        this.message.text = "";
        btnCreate.interactable = true;
        btnJoin.interactable = true;
    }

    public void SetUsername(string username)
    {
        PlayerPrefs.SetString("username",username);
    }

    public void SetRoomName(string roomName)
    {
        PlayerPrefs.SetString("roomName", roomName);
    }
}
