using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{

    // public Button levelOneButton;
    // public Button levelTwoButton;
    // public Button levelThreeButton;

    private void Start()
    {
        // levelOneButton = GetComponent<Button>();
        // levelTwoButton = GetComponent<Button>();
        // levelThreeButton = GetComponent<Button>();

        // levelOneButton.onClick.AddListener(LoadLevelOne);
        // levelTwoButton.onClick.AddListener(LoadLevelTwo);
        // levelThreeButton.onClick.AddListener(LoadLevelThree);
    }

    public void LoadLevelOne()
    {
        SceneManager.LoadScene(1);
    }
 
    public void LoadLevelTwo()
    {
        SceneManager.LoadScene(2);
    }

    public void LoadLevelThree()
    {
        SceneManager.LoadScene(3);
    }

}