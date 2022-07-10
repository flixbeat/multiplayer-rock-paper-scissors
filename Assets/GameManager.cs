using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;
    
    public Canvas canvas;
    public Transform spawnPointA;
    public Transform spawnPointB;
    
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Canvas canvasPick;

    private int whosTurnViewId;

    private void Awake()
    {
        if (!Instance)
            Instance = this;
    }

    private void Start()
    {
        PhotonNetwork.Instantiate(playerPrefab.name,Vector3.zero, Quaternion.identity);
    }
    

    // invoked when other player joined (max players 2 - as defined in launcher)
    public override void OnJoinedRoom()
    {
        StartCoroutine(StartGame());
        
        // check for player data if set
        bool HasAllPlayersLoaded()
        {
            foreach (var p in PhotonNetwork.CurrentRoom.Players)
            {
                var player = p.Value;
                GameObject playerObject = (GameObject) player.TagObject;
                if (playerObject == null)
                    return false;
            }

            return true;
        }

        IEnumerator StartGame()
        {
            List<GameObject> players = new List<GameObject>();

            // continually check if all player data is set
            yield return new WaitUntil(HasAllPlayersLoaded);
            
            // add to list of players
            foreach (var p in PhotonNetwork.CurrentRoom.Players)
            {
                var player = p.Value;
                GameObject playerObject = (GameObject) player.TagObject;
                players.Add(playerObject);
            }

            // give turn to random player
            int randomPlayerIndex = Random.Range(0, players.Count);
            int viewId = players[1].GetComponent<PhotonView>().ViewID;
            photonView.RPC("SetTurn",RpcTarget.All,viewId);
        }
    }

    [PunRPC]
    private void SetTurn(int viewId, PhotonMessageInfo info)
    {
        whosTurnViewId = viewId;
        
        foreach (var p in PhotonNetwork.CurrentRoom.Players)
        {
            var player = p.Value;
            GameObject playerObject = (GameObject) player.TagObject;
            PhotonView playerPhotonView = playerObject.GetComponent<PhotonView>();
            
            if (playerPhotonView.ViewID == viewId)
            {
                ShowCanvas(playerPhotonView);
                print("turn given to: " + player.NickName);
                break;
            }
        }
    }

    private void ShowCanvas(PhotonView photonView)
    {
        if(photonView.IsMine)
            canvasPick.gameObject.SetActive(true);
    }

    // called via button onclick
    public void OnPickButtonClicked(string chosen)
    {
        photonView.RPC(nameof(Pick),RpcTarget.All,chosen);
    }
    
    [PunRPC]
    private void Pick(string chosen)
    {
        canvasPick.gameObject.SetActive(false);
        
        if (!photonView.IsMine)
            return;
        
        foreach (var p in PhotonNetwork.CurrentRoom.Players)
        {
            var player = p.Value;
            GameObject playerObject = (GameObject) player.TagObject;
            PhotonView playerPhotonView = playerObject.GetComponent<PhotonView>();

            if (playerPhotonView.ViewID == whosTurnViewId)
            {
                Hashtable prop = new Hashtable();
                prop["pick"] = chosen;
                player.CustomProperties = prop;
                print($"{playerPhotonView.Owner} choose {chosen}");
                photonView.RPC("SetTurn",RpcTarget.All,GetOtherPlayerView(playerPhotonView.ViewID).ViewID);
                break;
            }
        }

        if(!IsOtherPlayerWaitingToChoose())
            Fight();
    }

    private PhotonView GetOtherPlayerView(int referenceViewId)
    {
        foreach (var p in PhotonNetwork.CurrentRoom.Players)
        {
            var player = p.Value;
            GameObject playerObject = (GameObject) player.TagObject;
            PhotonView playerPhotonView = playerObject.GetComponent<PhotonView>();

            if (playerPhotonView.ViewID != referenceViewId)
                return playerPhotonView;
        }

        print($"unable to get other player photon view id");
        return null;
    }

    private void Fight()
    {
        List<Photon.Realtime.Player> players = new List<Photon.Realtime.Player>();
        
        foreach (var p in PhotonNetwork.CurrentRoom.Players)
        {
            var player = p.Value;
            players.Add(player);
        }

        PhotonView playerA = ((GameObject) players[0].TagObject).GetComponent<PhotonView>();
        PhotonView playerB = ((GameObject) players[1].TagObject).GetComponent<PhotonView>();
        
        string playerAPicked = (string) players[0].CustomProperties["pick"];
        string playerBPicked = (string) players[1].CustomProperties["pick"];
        
        if(playerAPicked.Equals("rock") && playerBPicked.Equals("rock"))
            print("tie");
        else if (playerAPicked.Equals("paper") && playerBPicked.Equals("rock"))
        {
            print($"{players[0]} winner!");
            playerB.RPC("Damage",RpcTarget.All);
        }
        else if (playerAPicked.Equals("scissors") && playerBPicked.Equals("rock"))
        {
            print($"{players[1]} winner!");
            playerA.RPC("Damage",RpcTarget.All);
        }

        else if (playerAPicked.Equals("rock") && playerBPicked.Equals("paper"))
        {
            print($"{players[1]} winner!");
            playerA.RPC("Damage",RpcTarget.All);
        }
        else if(playerAPicked.Equals("paper") && playerBPicked.Equals("paper"))
            print("tie!");
        else if (playerAPicked.Equals("scissors") && playerBPicked.Equals("paper"))
        {
            print($"{players[0]} winner!");
            playerB.RPC("Damage",RpcTarget.All);
        }
        
        else if (playerAPicked.Equals("rock") && playerBPicked.Equals("scissors"))
        {
            print($"{players[0]} winner!");
            playerB.RPC("Damage",RpcTarget.All);
        }
            
        else if (playerAPicked.Equals("paper") && playerBPicked.Equals("scissors"))
        {
            print($"{players[1]} winner!");
            playerA.RPC("Damage",RpcTarget.All);
        }
        else if(playerAPicked.Equals("scissors") && playerBPicked.Equals("scissors"))
            print("tie!");
        
        foreach (var p in PhotonNetwork.CurrentRoom.Players)
        {
            var player = p.Value;
            player.CustomProperties["pick"] = null;
        }
    }

    private bool IsOtherPlayerWaitingToChoose()
    {
        foreach (var p in PhotonNetwork.CurrentRoom.Players)
        {
            var player = p.Value;
            if( string.IsNullOrEmpty((string) player.CustomProperties["pick"]) )
                return true;
        }

        return false;
    }
    
}
