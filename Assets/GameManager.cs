using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;
    
    public Canvas canvas;
    public Transform spawnPointA;
    public Transform spawnPointB;

    [SerializeField] private Transform leftHandPos;
    [SerializeField] private Transform rightHandPos;
    
    [SerializeField] private GameObject leftHand;
    [SerializeField] private GameObject rightHand;
    [SerializeField] private GameObject playerPrefab;
    
    [SerializeField] private Canvas canvasPick;
    [SerializeField] private List<Sprite> rpsSprites = new List<Sprite>();
    [SerializeField] private Text waiting;
    
    private PlayerController playerController;
    private GameObject leftHandGameObject;
    private GameObject rightHandGameObject;
    private string whosTurn;
    private string whosWinner;
    private string myNickname;

    private void Awake()
    {
        if (!Instance)
            Instance = this;

        myNickname = PhotonNetwork.NickName;
    }

    private void Start()
    {
        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity);
        playerController = player.GetComponent<PlayerController>();
        
        photonView.RPC("ShowPlayerInfo",RpcTarget.AllBuffered);
    }

    public override void OnJoinedRoom()
    {
        photonView.RPC("SetTurn",RpcTarget.All);
    }

    [PunRPC]
    private void ShowPlayerInfo()
    {
        playerController.photonView.RPC("SetInfo",RpcTarget.All,myNickname,3);
    }

    [PunRPC]
    private void ShowHand(int lIndex, int rIndex)
    {
        leftHandGameObject = PhotonNetwork.Instantiate(leftHand.name, Vector2.zero, leftHand.transform.rotation);
        leftHandGameObject.transform.parent = leftHandPos;
        leftHandGameObject.transform.localPosition = Vector2.zero;

        rightHandGameObject = PhotonNetwork.Instantiate(rightHand.name, Vector2.zero, rightHand.transform.rotation);
        rightHandGameObject.transform.parent = rightHandPos;
        rightHandGameObject.transform.localPosition = Vector2.zero;
        
        Hand lHand = leftHandGameObject.GetComponent<Hand>();
        lHand.SetSprite(rpsSprites[lIndex]);
        
        Hand rHand = rightHandGameObject.GetComponent<Hand>();
        rHand.SetSprite(rpsSprites[rIndex]);
    }
    
    [PunRPC]
    private void InflictDamage()
    {
        PhotonView photonView = playerController.GetComponent<PhotonView>();
        photonView.RPC("DeductLife",RpcTarget.All,whosWinner);
    }

    [PunRPC]
    private void SetTurn()
    {
        if (whosTurn == String.Empty)
        {
            int rand = Random.Range(0, 2);
            whosTurn = PhotonNetwork.PlayerList[rand].NickName;
        }
        else
        {
            whosTurn = GetOtherPlayer().NickName;
        }

        canvasPick.gameObject.SetActive(myNickname.Equals(whosTurn));
        waiting.gameObject.SetActive(!myNickname.Equals(whosTurn));
    }
    
    // get the player who's not turn right now
    private Player GetOtherPlayer()
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (!player.NickName.Equals(whosTurn))
                return player;
        }

        return null;
    }

    private void GetPlayerObjects()
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            PlayerController playerController = (PlayerController) player.TagObject;
        }
    }

    public void Pick(string type)
    {
        PhotonView playerPhotonView = playerController.GetComponent<PhotonView>();
        playerPhotonView.RPC("Pick",RpcTarget.All,type);
        Fight();
    }

    private void Fight()
    {
        PlayerController otherPlayer = (PlayerController) GetOtherPlayer().TagObject;
        string otherPlayerPicked = otherPlayer.picked;
        int lIndex = -1;
        int rIndex = -1;
        
        // check if the other player haven't picked yet
        if (otherPlayerPicked == String.Empty)
        {
            photonView.RPC("SetTurn",RpcTarget.All);
            return;
        }
        
        // if both players have picked
        if (playerController.picked.Equals("rock") && otherPlayerPicked.Equals("rock"))
        {
            print("Tie!");
            photonView.RPC("SetWinner",RpcTarget.All,string.Empty);
            lIndex = 0;
            rIndex = 0;
        }
        else if (playerController.picked.Equals("rock") && otherPlayerPicked.Equals("paper"))
        {
            print($"Winner: {otherPlayer.NickName}");
            photonView.RPC("SetWinner",RpcTarget.All,otherPlayer.NickName);
            lIndex = 0;
            rIndex = 1;
        }
        else if (playerController.picked.Equals("rock") && otherPlayerPicked.Equals("scissors"))
        {
            print($"Winner: {playerController.NickName}");
            photonView.RPC("SetWinner",RpcTarget.All,playerController.NickName);
            lIndex = 0;
            rIndex = 2;
        }
        
        if (playerController.picked.Equals("paper") && otherPlayerPicked.Equals("rock"))
        {
            print($"Winner: {playerController.NickName}");
            photonView.RPC("SetWinner",RpcTarget.All,playerController.NickName);
            lIndex = 1;
            rIndex = 0;
        }
        else if (playerController.picked.Equals("paper") && otherPlayerPicked.Equals("paper"))
        {
            print($"Tie!");
            photonView.RPC("SetWinner",RpcTarget.All,string.Empty);
            lIndex = 1;
            rIndex = 1;
        }
        else if (playerController.picked.Equals("paper") && otherPlayerPicked.Equals("scissors"))
        {
            print($"Winner: {otherPlayer.NickName}");
            photonView.RPC("SetWinner",RpcTarget.All,otherPlayer.NickName);
            lIndex = 1;
            rIndex = 2;
        }
        
        if (playerController.picked.Equals("scissors") && otherPlayerPicked.Equals("rock"))
        {
            print($"Winner: {otherPlayer.NickName}");
            photonView.RPC("SetWinner",RpcTarget.All,otherPlayer.NickName);
            lIndex = 2;
            rIndex = 0;
        }
        else if (playerController.picked.Equals("scissors") && otherPlayerPicked.Equals("paper"))
        {
            print($"Winner: {playerController.NickName}");
            photonView.RPC("SetWinner",RpcTarget.All,playerController.NickName);
            lIndex = 2;
            rIndex = 1;
        }
        else if (playerController.picked.Equals("scissors") && otherPlayerPicked.Equals("scissors"))
        {
            print($"Tie!");
            photonView.RPC("SetWinner",RpcTarget.All,string.Empty);
            lIndex = 2;
            rIndex = 2;
        }
        
        playerController.GetComponent<PhotonView>().RPC("Pick", RpcTarget.All, String.Empty);
        otherPlayer.GetComponent<PhotonView>().RPC("Pick", RpcTarget.All, String.Empty);
        photonView.RPC("ShowHand",RpcTarget.All,lIndex,rIndex);
        photonView.RPC("PostFight",RpcTarget.All);
        photonView.RPC("InflictDamage",RpcTarget.All);
    }

    [PunRPC]
    private void SetWinner(string nickname)
    {
        whosWinner = nickname;
    }

    [PunRPC]
    private void PostFight()
    {
        canvasPick.gameObject.SetActive(false);
        waiting.gameObject.SetActive(true);
        waiting.text = whosWinner.Equals(string.Empty) ? "TIE!" : $"{whosWinner } won!";
        
        StartCoroutine(DestroyHands());
    }

    private IEnumerator DestroyHands()
    {
        yield return new WaitForSeconds(3);
        photonView.RPC("SetWinner",RpcTarget.All,String.Empty);
        PhotonNetwork.Destroy(leftHandGameObject);
        PhotonNetwork.Destroy(rightHandGameObject);
        waiting.text = "Waiting for the opponent to choose...";
        photonView.RPC("SetTurn",RpcTarget.All);
    }
}
