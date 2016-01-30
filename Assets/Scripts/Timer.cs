using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Timer : MonoBehaviour {

    public Text timer;
    public Text score;

    bool isWaiting;
    bool isPlaying;
    bool isTurnEnd;
    bool isRoundEnd;

    int currentTurn;
    int currentRound;
    int totalRound;
    int totalTurn;
    int playerCount;
    int playerScore;

    int[] idArray;

    int playerID;

    //Kelvin
    float timeCountdown;
    float currentTime;
    Color initialColour = Color.white;
    Color warningColour = Color.red;
    bool scored;

    void Awake()
    {
        timer = timer.GetComponent<Text>();
        score = score.GetComponent<Text>();
        WaitingGame();
        AddPlayer();
        AddPlayer();
    }
	
	void FixedUpdate () 
    {
        GameUpdate();
	}

    void GameUpdate()
    {
        if (isWaiting)
        {
            if (playerCount > 1)
                StartGame();
        }

        if (isPlaying)
        {
            scored = false;
            if(!scored)
            {
                currentTime -= Time.deltaTime;
                if(currentTime <= 0)
                {
                    currentTime = timeCountdown;
                    if (playerScore <= 0)
                    {
                        playerScore = 0;
                    }
                    else
                    {
                        playerScore -= 5;
                    }
                }

                if(Input.GetButtonDown("Fire1"))
                    Scoring(playerID);
            }
            timer.text = "Remaining time : " + currentTime.ToString("f0");
            score.text = "Score : " + playerScore.ToString("f0");
        }
    }

    void WaitingGame()
    {
        isWaiting = true;
        currentTurn = 0;
        currentRound = 0;
        playerCount = 0;
        timeCountdown = 0.0f;
        isTurnEnd = false;
        isRoundEnd = false;
    }

    void StartGame()
    {
        isPlaying = true;
        currentTurn = 0;
        currentRound = 0;
        timeCountdown = 5.0f;
        isWaiting = false;
        isTurnEnd = false;
        isRoundEnd = false;

        if (playerCount < 5)
            totalTurn = Random.Range(5, 10);
        else if (playerCount < 10)
            totalTurn = Random.Range(3, 5);
        else if (playerCount >= 10)
            totalTurn = Random.Range(2, 3);

        idArray = new int[totalTurn];
        totalRound = playerCount * totalTurn;
    }

    void NextRound()
    {
        isTurnEnd = true;
        currentTurn++;
        RoundEnd();
    }
        
    void SetId(int id)
    {
        playerID = id;
    }

    void AddPlayer()
    {
        playerCount++;
    }

    void RemovePlayer()
    {
        playerCount--;
    }

    void RoundEnd()
    {
        currentRound++;
        if (currentRound >= totalRound)
            GameOver();
    }

    void GameOver()
    {
        Debug.Log("Game Over");
    }

    public void Scoring(int id)
    {
        scored = true;
        if (currentTurn == id)
        {
            AddScore((int)currentTime);
            NextRound();
            Debug.Log("Success, correct!");
        }
        else
        {
            AddScore(-5);
            Debug.Log("Failed, wrong answer");
        }
        currentTime = timeCountdown;
    }

    void AddScore(int score)
    {
        playerScore += score;
        if (playerScore <= 0)
            playerScore = 0;
    }
}
