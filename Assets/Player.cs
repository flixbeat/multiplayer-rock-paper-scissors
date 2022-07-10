using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;

public class Player : MonoBehaviourPun, IPunObservable, IPunInstantiateMagicCallback
{
    [SerializeField] private Text hp;
    [SerializeField] private Text name;
    
    private int damage = 1;

    [HideInInspector] public int x;
    
    void Start()
    {
        Canvas canvas = GameManager.Instance.canvas;
        Transform spawnPointA = GameManager.Instance.spawnPointA;
        Transform spawnPointB = GameManager.Instance.spawnPointB;
        
        int playersCount = PhotonNetwork.CurrentRoom.Players.Count;
        Vector2 position = playersCount == 1 ? spawnPointA.transform.position : spawnPointB.transform.position;
        transform.parent = canvas.transform;
        transform.position = position;
        SetHp(3);
    }
    
    // invoked when a player has been instantiated using PhotonNetwork.Instantiate
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        // create a custom property (to be shared and read by other players)
        Hashtable prop = new Hashtable();
        x = Random.Range(0, 99);
        prop["test"] = x;

        // set name via nickname
        name.text = info.Sender.NickName;
        
        // set the TagObject to this game object for other player to reference this object
        // can be accessed by looping all players in the room, eg. PhotonNetwork.CurrentRoom.Players
        info.Sender.TagObject = gameObject;
    }

    private void SetHp(int val)
    {
        // automatically synced to the network due to OnPhotonSerializeView
        hp.text = $"HP: {val}";
    }

    // inflict damage to self
    [PunRPC]
    private void Damage()
    {
        // if not owned, don't do anything
        if (!photonView.IsMine)
            return;
        
        // get current hp and deduct damage
        string[] hpInfo = hp.text.Split(":");
        int curVal = int.Parse(hpInfo[1]);
        SetHp(curVal - damage);
    }
    
    // sync values over the network continually like Update(), whenever there is a change
    // in the value of the variable it will be sent over the network and read by their remote copies
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // client original copy (sender/send values)
        if (stream.IsWriting)
        {
            stream.SendNext(hp.text);
            stream.SendNext(name.text);
        }
        // remote copy (receiver/update values)
        else if (stream.IsReading)
        {
            hp.text = (string) stream.ReceiveNext();
            name.text = (string) stream.ReceiveNext();
        }
    }
}
