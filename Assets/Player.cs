using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Player : MonoBehaviourPun, IPunObservable, IPunInstantiateMagicCallback
{
    [SerializeField] private Text hp;
    [SerializeField] private Text name;
    
    private int damage = 10;

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
        SetHp(100);
    }
    
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        Hashtable prop = new Hashtable();
        x = Random.Range(0, 99);
        prop["test"] = x;
        info.Sender.TagObject = gameObject;
        name.text = info.Sender.NickName;
        print($"{x} generated for {info.Sender.NickName}");
    }

    private void SetHp(int val)
    {
        hp.text = $"HP: {val}";
    }

    [PunRPC]
    private void Damage()
    {
        if (!photonView.IsMine)
            return;
        
        string[] hpInfo = hp.text.Split(":");
        int curVal = int.Parse(hpInfo[1]);
        SetHp(curVal - damage);
    }
    
    // sync hp
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

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {
            if (!photonView.IsMine)
                return;
            
            foreach (var p in PhotonNetwork.CurrentRoom.Players)
            {
                var player = p.Value;
                GameObject playerGameObject = (GameObject) player.TagObject;
                var nickname = player.NickName;
                var view = playerGameObject.GetComponent<PhotonView>();
                var playerScript = playerGameObject.GetComponent<Player>();
                
                print($"{nickname} has prop {playerScript.x} and view id of {view.ViewID}");

                if (view.ViewID != photonView.ViewID)
                    view.RPC(nameof(Damage),RpcTarget.All);
            }
            
            print($"sender ({PhotonNetwork.NickName}) view id is: {photonView.ViewID}");
        }
    }

}
