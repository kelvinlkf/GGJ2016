using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Timer : MonoBehaviour {

    public Text timer;
    public Text score;
    public Text round;
    public Text currentNumber;

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

    List<int> idArray;
    List<int> availableIdArray;

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
        idArray = new List<int>();
        //!if is server
        WaitingGame();
    }
	
	void FixedUpdate () 
    {
        GameUpdate();
	}

    void GameUpdate()
    {
        if (isWaiting)
        {
            //if (playerCount > 1)
                StartGame();
        }

        if (isPlaying)
        {
            scored = false;
            if (!scored)
            {
                currentTime -= Time.deltaTime;
                if (currentTime <= 0)
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

                if (Input.GetButtonDown("Fire1"))
                    Scoring(playerID);
            }
            timer.text = "" + currentTime.ToString("f0");
            score.text = "" + playerScore.ToString("f0");
            round.text = "" + currentRound.ToString("f0");
            //TODO get player current number
            //currentNumber.text = "" + currentRound.ToString("f0");
        }
    }

    void SetGameState()
    {
        
    }

    void WaitingGame()
    {
        isWaiting = true;
        currentTurn = 0;
        currentRound = 0;
        playerCount = 11;
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

        //! Set up total turn for each player count
        if (playerCount < 5)
            totalTurn = Random.Range(5, 10);
        else if (playerCount < 10)
            totalTurn = Random.Range(3, 5);
        else if (playerCount >= 10)
            totalTurn = Random.Range(2, 3);

        totalRound = playerCount * totalTurn;
        CreateAvailableId(totalRound);

        /*
        for (int i = 0; i < totalRound; i++)
        {
            Debug.Log(availableIdArray[i]);
        }*/

        //! Assign available id to each player
        for (int i = 0; i < totalTurn; i++)
        {
            
        }
    }

    void NextRound()
    {
        isTurnEnd = true;
        currentTurn++;
        RoundEnd();
    }

    void CreateAvailableId(int length)
    {
        availableIdArray = new List<int>();
        //! Assign 0 - length
        for (int i = 0; i < length; i++)
        {
            availableIdArray.Add(i);
        }

        //! Random swtich the index
        for (int i = length-1; i > 0; i--)
        {
            // Randomize a number between 0 and i (so that the range decreases each time)
            int rnd = Random.Range(0,i);

            // Save the value of the current i, otherwise it'll overright when we swap the values
            int temp = availableIdArray[i];

            // Swap the new and old values
            availableIdArray[i] = availableIdArray[rnd];
            availableIdArray[rnd] = temp;
        }
    }

    void AddID(int id)
    {
        idArray.Add(id);
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
