using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ToConfigButtonScript : MonoBehaviour
{
    public void OnClick()
    {
        SceneManager.LoadScene("ConfigurationScene");
    }
}
