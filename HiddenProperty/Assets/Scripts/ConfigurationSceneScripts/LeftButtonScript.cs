using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 



public class LeftButtonScript : MonoBehaviour
{
    public GameObject num_of_people_object;
    public GameObject configManager;

    // ボタンが押された場合、今回呼び出される関数
   public void OnClick()
   {
       Text numOfPeopleText = num_of_people_object.GetComponent<Text>();
       int numOfPeople = int.Parse(numOfPeopleText.text);
       if(4 <= numOfPeople && numOfPeople <= 8)
       {
           numOfPeople -= 1;
           PlayerManager script = configManager.GetComponent<PlayerManager>();  
           List<GameObject> list = script.playerList;
           GameObject lastPlayer = list[numOfPeople];
           Destroy(lastPlayer);
           list.RemoveAt(numOfPeople);
       }
       numOfPeopleText.text = numOfPeople.ToString();
   }
}
