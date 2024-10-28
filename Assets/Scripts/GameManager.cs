using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // static reference

    public GameObject backgroundPanel; //gray background
    public GameObject victoryPanel;
    public GameObject lossPanel;
    public TMP_Text congratulationsText;
    public TMP_Text unfortunateText;

    public int goal; //the amount of points you need to get to win.
    public int moves; //the number of turns you can take
    public int points; //the current points you have earned
    public int coins; //the current coins you have earned

    public bool isGameEnded;

    public TMP_Text pointsTxt;
    public TMP_Text movesTxt;
    public TMP_Text goalTxt;
    public TMP_Text coinsText;

    private void Awake()
    {
        Instance = this;
    }

    public void Initialize(int _moves, int _goal, int _coins)
    {
    moves = _moves;
    goal = _goal;
    coins = _coins;
    }

    // Update is called once per frame
    void Update()
    {
    pointsTxt.text = "Points: " + points.ToString();        
    movesTxt.text = "Moves: " + moves.ToString();        
    goalTxt.text = "Goal: " + goal.ToString();
    coinsText.text = "Coins: " + coins.ToString();
    congratulationsText.text = "Congratulations you won the game in " + moves.ToString() + " moves and scored " + points.ToString() + " points. ";
    unfortunateText.text = "Unfortunately you only got  " + points.ToString() + " points in " + moves.ToString()  + " moves. Better luck next time!";
    }

    public void ProcessTurn(int _pointsToGain, bool _subtractMoves, int _coinsToGain)
    {
        points += _pointsToGain;
        coins += _coinsToGain;
        if(_subtractMoves)
        moves--;

        if(points >= goal)
        {
            //you've won the game
            isGameEnded = true;
            //Display a victory screen
            backgroundPanel.SetActive(true);
            victoryPanel.SetActive(true);
            PotionBoard.Instance.potionParent.SetActive(false);
            return;
        }
        if(moves == 0)
        {
            //lose the game
            isGameEnded = true;
            backgroundPanel.SetActive(true);
            lossPanel.SetActive(true);
            PotionBoard.Instance.potionParent.SetActive(false);
            return;
        }
    }

    //attached to a button to change scene when winning
    public void WinGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);        
    }

    //attached to a button to change scene when losing
    public void LoseGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


}
