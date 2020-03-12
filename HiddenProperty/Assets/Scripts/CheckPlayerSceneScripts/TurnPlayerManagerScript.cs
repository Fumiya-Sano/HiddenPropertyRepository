using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnPlayerManagerScript : MonoBehaviour
{
 
    public Dictionary<int, string> playerDictionary;
    public static int turnPlayerNum;
    public static int roundNum;
    public static int totalPlayers;
    public GameObject raunndoObject;

    // Start is called before the first frame update
    void Start()
    {
        turnPlayerNum += 1;
        if(turnPlayerNum > totalPlayers)
        {
            turnPlayerNum = 1;
            roundNum += 1;
        }
        // プレイヤーリストの取得
        playerDictionary = StartButtonScript.getPlayerDict();
        foreach(var pair in playerDictionary)
        {
            Debug.Log(pair.Key);
        }
        Text turnPlayerText = GetComponent<Text>();
        turnPlayerText.text = playerDictionary[turnPlayerNum];
        Text raunndoText = raunndoObject.GetComponent<Text>();
        raunndoText.text = roundNum.ToString();
        Debug.Log(totalPlayers);
    }

    public static int getTurnPlayerNum()
    {
        return turnPlayerNum;
    }

    public static void setTurnPlayerNum(int a)
    {
        turnPlayerNum = a;
    }

    public static int getRoundNum()
    {
        return roundNum;
    }

    public static void setRoundNum(int a)
    {
        roundNum = a;
    }

    public static int getTotalPlayers()
    {
        return totalPlayers;
    }

    public static void setTotalPlayers(int a)
    {
        totalPlayers = a;
    }
}
