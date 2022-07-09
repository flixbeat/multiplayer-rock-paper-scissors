using System;
using Photon.Pun;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;
    
    public Canvas canvas;
    public Transform spawnPointA;
    public Transform spawnPointB;
    
    [SerializeField] private GameObject player;

    private GameObject localPlayer;
    
    private void Awake()
    {
        if (!Instance)
            Instance = this;
    }

    private void Start()
    {
        localPlayer = PhotonNetwork.Instantiate(player.name,Vector3.zero, Quaternion.identity);
        localPlayer.name = PhotonNetwork.NickName;
    }
}
