using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class PlayerLogic : NetworkBehaviour {

	// This should only be handled by the host player.
	// NOTE:
	// 1 Round = when all players complete the number sequence
	// 1 Turn = one turn of player's correct button press

	public enum GameState {
		None,
		Init,
		WaitingForPlayers,
		WaitingForNextRound,
		PlayingRound,
		GameOver,
		RestartRound
	}

	[SyncVar]
	GameState gameState;

	[SyncVar]
	int playerScore;

	static int playerCount = 0;

	float nextRoundCountdown = 0;
	public float currentTime;
	int currentTurn;
	int currentRound;
	int totalTurns;
	int totalTurnPerPlayer;

	public int currentPlayer;
	List<NetworkInstanceId> playerIdQueue = new List<NetworkInstanceId>();
	List<int> playerNumbers = new List<int>(); // player's numbers, e.g. [1,3,5,6,7]
	List<int> availableIdArray = new List<int>();



	void Start ()
	{
		playerCount++;

		if (GetComponent<NetworkIdentity>().isServer)
	    {
			Debug.Log("LOGGED IN AS SERVER");
			SetGameState(GameState.Init);
	    }
	    else
	    {
			Debug.Log("Logged in as client");
	    }
	}

	void OnDestroy ()
	{
		playerCount--;
	}

	void Update () 
	{
		if (!isLocalPlayer)
            return;

		if (playerCount < 2 && gameState != GameState.WaitingForPlayers)
			SetGameState(GameState.WaitingForPlayers);

		switch (gameState)
		{
		case GameState.WaitingForPlayers:

			if (playerCount > 1)
				SetGameState(GameState.WaitingForNextRound);

			break;

		case GameState.WaitingForNextRound:

			nextRoundCountdown -= Time.deltaTime;

			if (nextRoundCountdown <= 0f)
				SetGameState(GameState.RestartRound);

			break;

		case GameState.PlayingRound:

			Debug.Log("PlayingRound waiting for :" + currentTurn);

			if (Input.GetButtonDown("Fire1"))
			{
				CmdPlayerPress(netId, playerNumbers[0]);
			}

			currentTime -= Time.deltaTime;
	        if (currentTime <= 0)
	        {
	            currentTime = 5f;
	            if (playerScore <= 0)
	            {
	                playerScore = 0;
	            }
	            else
	            {
	                playerScore -= 5;
	            }
	        }

			HUD.instance.UpdateTimer(currentTime);
			HUD.instance.UpdateScore(playerScore);

			break;
		}
	}

	void SetGameState(GameState newState)
	{
		Debug.Log("SetGameState : " + newState);

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

			GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
			totalTurns = players.Length * totalTurnPerPlayer;

			// Create numbers for every player
			//! Assign available id to each player
	        CreateAvailableId(totalTurns);

			for (int i = 0; i < players.Length; i++)
	        {
	        	Debug.Log("===== Assigning for player : " + i);

				players[i].GetComponent<PlayerLogic>().ClearNumbers();

				for (int j = 0; j < totalTurnPerPlayer; j++)
		        {
					int number = j+(i*totalTurnPerPlayer);
					players[i].GetComponent<PlayerLogic>().AddNumber(availableIdArray[number]);

					Debug.Log("assigned number : " + i + " , " + availableIdArray[number]);
		        }

				Debug.Log(playerNumbers.ToString());
		        Debug.Log("===== Done assigning : " + i);
				

				players[i].GetComponent<PlayerLogic>().SortNumbers();
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

	public bool CheckNumber (int numberToCheck)
	{
		if (currentTurn == numberToCheck)
		{
			// If correct number, move to next turn (reset turn)
			currentTurn++;
			currentTime = 5f;

			if (currentTurn >= totalTurns)
				SetGameState(GameState.GameOver);

			return true;
		}
		else
			return false;
	}

	public void ClearNumbers ()
	{
		playerNumbers.Clear();
	}

	public void AddNumber (int num)
	{
		playerNumbers.Add(num);
	}

	public void SortNumbers ()
	{
		Debug.Log("Sort : " + playerNumbers);
		playerNumbers.Sort((int x, int y) => { return x.CompareTo(y); });
	}

	void AddScore(int score)
	{
		playerScore += score;
		if (playerScore <= 0)
			playerScore = 0;
	}



	[Command]
	void CmdPlayerPress (NetworkInstanceId nid, int number)
	{
		Debug.Log("CmdPlayerPress : " + currentTurn + " , " + number);

		if (isServer)
		{
			if (currentTurn == number)
			{

				int score = Mathf.FloorToInt(currentTime);
				RpcCorrect(nid, score);
			}
			else
			{
				RpcWrong(nid);
			}
		}
	}

	[ClientRpc]
	void RpcWrong (NetworkInstanceId nid)
	{
		Debug.Log("RpcWrong");
		if (netId == nid)
		{
			AddScore(-5);
		}
	}

	[ClientRpc]
	void RpcCorrect (NetworkInstanceId nid, int score)
	{
		Debug.Log("RpcCorrect");
		if (netId == nid)
		{
			AddScore(score);
		}
	}
}