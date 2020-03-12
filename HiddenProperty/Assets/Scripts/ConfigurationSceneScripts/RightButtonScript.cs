using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 



public class RightButtonScript : MonoBehaviour
{
   public GameObject num_of_people_object;
   public GameObject content;
   public GameObject inputPrefab;
   public GameObject configManager;

   // ボタンが押された場合、今回呼び出される関数
   public void OnClick()
   {
       Text numOfPeopleText = num_of_people_object.GetComponent<Text>();
       int numOfPeople = int.Parse(numOfPeopleText.text);
       if(3 <= numOfPeople && numOfPeople <= 7)
       {
            numOfPeople += 1;
            GameObject inputField = Instantiate(inputPrefab, content.transform);
            GameObject placeHolder = inputField.transform.Find("Placeholder").gameObject;
            Text playerName = placeHolder.GetComponent<Text>();
            playerName.text = "プレイヤー" + numOfPeople;
            PlayerManager script = configManager.GetComponent<PlayerManager>();  
            List<GameObject> list = script.playerList;
            list.Add(inputField);
       }
       numOfPeopleText.text = numOfPeople.ToString();
   }
}
