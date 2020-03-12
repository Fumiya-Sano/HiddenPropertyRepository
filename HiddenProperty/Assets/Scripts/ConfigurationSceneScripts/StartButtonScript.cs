using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartButtonScript : MonoBehaviour
{
    public GameObject playerConfig;
    public static Dictionary<int, string> playerTable = new Dictionary<int, string>();
    int a;


    public void OnClick()
    {
        playerTable.Clear();
        a = 0;
        PlayerManager script = playerConfig.GetComponent<PlayerManager>();
        List<GameObject> list = script.playerList;
        foreach(GameObject player in list)
        {
            a += 1;
            GameObject textObject = player.transform.Find("Text").gameObject;
            Text playerName = textObject.GetComponent<Text>();
            if(playerName.text == "")
            {
                GameObject placeholderObject = player.transform.Find("Placeholder").gameObject;
                Text placeholderName = placeholderObject.GetComponent<Text>();
                playerTable.Add(a, placeholderName.text);
            }
            else
            {
                playerTable.Add(a, playerName.text);
            }
        }
        foreach(var pair in playerTable)
        {
            Debug.Log(pair.Value);
            Debug.Log(pair.Key);
        }
        TurnPlayerManagerScript.setTurnPlayerNum(0);
        TurnPlayerManagerScript.setRoundNum(1);
        TurnPlayerManagerScript.setTotalPlayers(a);
        GameScript.IsInitial = 1;
        GameScript.OnlyPositionList.Clear();
        GameScript.posVirusTable.Clear();
        SceneManager.LoadScene("CheckPlayerScene");
    }

    public static Dictionary<int, string> getPlayerDict()
    {
        return playerTable;
    }
}
