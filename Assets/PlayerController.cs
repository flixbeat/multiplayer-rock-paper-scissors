using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    public string NickName
    {
        get => name.text;
        set => name.text = value;
    }

    public int Hp
    {
        get => int.Parse(hp.text);
        set => hp.text = value.ToString();
    }

    [HideInInspector] public string picked;
    
    [SerializeField] private Text hp;
    [SerializeField] private Text name;

    private const int damage = 1;
    private Transform spawnPointA;
    private Transform spawnPointB;
    private GameManager gm;

    private void Awake()
    {
        gm = GameManager.Instance;
    }

    private void Start()
    {
        int playerCount = PhotonNetwork.PlayerList.Length;
        transform.parent = gm.canvas.transform;
        transform.position = playerCount == 1 ? gm.spawnPointA.position : gm.spawnPointB.position;
    }

    [PunRPC]
    public void SetInfo(string nickname, int hp)
    {
        Hp = hp;
        NickName = nickname;
    }

    [PunRPC]
    public void DeductLife(string winnerName)
    {
        if (winnerName.Equals(string.Empty))
            return;
        
        if(!winnerName.Equals(NickName))
            Hp -= 1;
    }

    [PunRPC]
    public void Pick(string type)
    {
        picked = type;
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        info.Sender.TagObject = this;
    }
}
