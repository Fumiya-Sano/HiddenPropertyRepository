using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI; 
public class PlayerManager : MonoBehaviour

{
    public List<GameObject> playerList = new List<GameObject>();
    public GameObject player1;
    public GameObject player2;
    public GameObject player3;

    void Start()
    {
        playerList.Add(player1);
        playerList.Add(player2);
        playerList.Add(player3);
    }

}
