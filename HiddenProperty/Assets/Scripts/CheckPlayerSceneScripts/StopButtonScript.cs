using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StopButtonScript : MonoBehaviour
{
   public void OnClick()
   {
       SceneManager.LoadScene("ConfigurationScene");
   }
}
