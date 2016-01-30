using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Server : NetworkBehaviour {

	// This should only be handled by the host player.
	// NOTE:
	// 1 Round = when all players complete the number sequence
	// 1 Turn = one turn of player's correct button press

	enum GameState {
		None,
		Init,
		WaitingForPlayers,
		WaitingForNextRound,
		PlayingRound,
		GameOver,
		RestartRound
	}

	public Text timer;
    public Text score;

	[SyncVar]
	GameState gameState;

	float nextRoundCountdown = 0;
	float currentTime;
    int currentTurn;
    int currentRound;
    int totalRound;
	int totalTurnPerPlayer;
    int playerCount;

	List<int> playerNumbers = new List<int>(); // player's numbers, e.g. [1,3,5,6,7]
	List<int> availableIdArray = new List<int>();

    [SyncVar]
    int playerScore;



    void Awake()
    {
		timer = timer.GetComponent<Text>();
        score = score.GetComponent<Text>();
    }

    void Start ()
    {
		if (GetComponent<NetworkIdentity>().isServer)
        {
			Debug.Log("LOGGED IN AS SERVER");
			SetGameState(GameState.Init);
        }
        else
        {
			Debug.Log("Not logged in as server");
        }
    }
	
	void Update () 
    {
		switch (gameState)
    	{
		case GameState.WaitingForNextRound:

			nextRoundCountdown -= Time.deltaTime;

			if (nextRoundCountdown <= 0f)
				SetGameState(GameState.RestartRound);

			break;

		case GameState.PlayingRound:

			if (Input.GetButtonDown("Fire1"))
				CmdPressButton(netId, playerNumbers[0]);

			timer.text = "" + currentTime.ToString("f0");
        	score.text = "" + playerScore.ToString("f0");

			break;
    	}
	}

    void SetGameState(GameState newState)
    {
    	gameState = newState;

        switch (gameState)
        {
        case GameState.Init:

        	// This is when the the first player (host) is created,
        	// so setup the server variables.

	        currentTurn = 0;
	        currentRound = 0;
	        SetGameState(GameState.WaitingForPlayers);

        	break;

        case GameState.WaitingForPlayers:

        	// This only happens when there's only one player,
        	// either on init or all other players left.

        	// In this state, the game doesn't start/countdown
        	// until another player joins.

			break;

        case GameState.WaitingForNextRound:

        	// This is when there are enough players, and we
        	// wait for a while before starting next round

        	// TODO display the countdown to let everyone get ready
			nextRoundCountdown = 2f;

			break;

		case GameState.RestartRound:

			currentTurn = 0;
			currentRound++;

			// Set up total turn for each player count
	        if (playerCount < 5)
	            totalTurnPerPlayer = Random.Range(5, 10);
	        else if (playerCount < 10)
				totalTurnPerPlayer = Random.Range(3, 5);
	        else if (playerCount >= 10)
				totalTurnPerPlayer = Random.Range(2, 3);

			// totalRound = playerCount * totalTurnPerPlayer;
	        CreateAvailableId(totalRound);

			//! Assign available id to each player
			playerNumbers.Clear();
	        for (int i = 0; i < playerCount; i++)
	        {
				for (int j = 0; j < totalTurnPerPlayer; j++)
		        {
					int number = j+(i*totalTurnPerPlayer);
		            playerNumbers.Add(availableIdArray[number]);

					Debug.Log("### assigned number : " + availableIdArray[number]);
		        }
	        }

			SetGameState(GameState.PlayingRound);

			break;

        case GameState.PlayingRound:

        	// Init variables before starting the round

        	currentTime = 5f;

			break;

        case GameState.GameOver:

        	// TODO

			break;

		default:
			Debug.Log("State not found : " + newState);
			break;
        }
    }

    void OnServerConnected (NetworkConnection conn)
    {
		Debug.Log("OnServerConnected");
    }

	void OnServerDisconnect ()
    {
		Debug.Log("OnServerDisconnect");
    }

	void OnConnectedToServer ()
    {
		Debug.Log("OnConnectedToServer");
    }

	void OnPlayerConnected(NetworkPlayer player)
	{
		playerCount++;

		Debug.Log("Player " + playerCount + " connected from " + player.ipAddress + ":" + player.port);

		if (gameState == GameState.WaitingForPlayers)
			SetGameState(GameState.WaitingForNextRound);
	}

	void OnPlayerDisconnected(NetworkPlayer player)
	{
		Debug.Log("Player " + playerCount + " disconnected from " + player.ipAddress + ":" + player.port);

		playerCount--;

		// Cleanup stuff, from http://docs.unity3d.com/ScriptReference/MonoBehaviour.OnPlayerDisconnected.html
		Network.RemoveRPCs(player);
        Network.DestroyPlayerObjects(player);
	}

	[Command]
	void CmdPressButton(NetworkInstanceId playerNetId, int playerNumber)
    {
    	// Command functions mean client will invoke this
    	// function, but only the server will execute it

    	Debug.Log("CmdPressButton : " + playerNetId + " , " + playerNumber);

    	// TODO
    	// server checks whether playerNetId + playerNumber is correct.
		// if true: reset currentTime = 5f, move to next required number/turn
		// If false: minus score only

		/*scored = true;
        if (currentTurn == playerNumber)
        {
            AddScore((int)currentTime);
            NextRound();
            playerNumbers.Remove(0);
            Debug.Log("Success, correct!");
        }
        else
        {
            AddScore(-5);
            Debug.Log("Failed, wrong answer");
        }
        currentTime = timeCountdown;*/





		/*scored = false;
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


        }
        */
    }

	void AddScore(int score)
    {
        playerScore += score;
        if (playerScore <= 0)
            playerScore = 0;
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
}
