using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class PlayerLogic : NetworkBehaviour {

	// This should only be handled by the host player.
	// NOTE:
	// 1 Round = when all players complete the number sequence
	// 1 Turn = one turn of player's correct button press

	[SyncVar]
	int playerScore = 100;

	[SyncVar]
	bool waitForNextRound = false;

	List<int> playerNumbers = new List<int>(); // player's numbers, e.g. [1,3,5,6,7]
	List<int> availableIdArray = new List<int>();



	void Start ()
	{
		Game.playerCount++;

		Random.seed = 12;

		if (isServer)
	    {
			Debug.Log("LOGGED IN AS SERVER");
			SetGameState(GameState.Init);
	    }
	    else
	    {
			Debug.Log("Logged in as client");
			CmdNewPlayerJoined();
	    }
	}

	void OnDestroy ()
	{
		Game.playerCount--;
	}

	void Update () 
	{
		if (!isLocalPlayer)
            return;

		if (Game.playerCount < 2 && Game.gameState != GameState.WaitingForPlayers)
			SetGameState(GameState.WaitingForPlayers);

		switch (Game.gameState)
		{
		case GameState.WaitingForPlayers:

			if (Game.playerCount > 1)
				SetGameState(GameState.WaitingForNextRound);

			break;

		case GameState.WaitingForNextRound:

			Game.nextRoundCountdown -= Time.deltaTime;

			if (Game.nextRoundCountdown <= 0f)
				SetGameState(GameState.RestartRound);

			break;

		case GameState.PlayingRound:

			Debug.Log("PlayingRound waiting for :" + Game.currentTurn);

			if (Input.GetButtonDown("Fire1"))
			{
				string str = "";
				for (int i = 0; i < playerNumbers.Count; i++)
					str += playerNumbers[i] + ", ";
				Debug.Log(str);

				Debug.Log("Send number : " + playerNumbers[0]);

				if (isServer)
					RpcPlayerPress(netId, playerNumbers[0]);
				else
					CmdPlayerPress(netId, playerNumbers[0]);
			}

			Game.currentTime -= Time.deltaTime;
			if (Game.currentTime <= 0)
	        {
				Game.currentTime = 5f;
	            if (playerScore <= 0)
	            {
	                playerScore = 0;
	            }
	            else
	            {
	                playerScore -= 5;
	            }
	        }

			HUD.instance.UpdateTimer(Game.currentTime);
			HUD.instance.UpdateScore(playerScore);

			break;
		}
	}

	void SetGameState(GameState newState)
	{
		Debug.Log("SetGameState : " + newState);

		Game.gameState = newState;

		switch (Game.gameState)
	    {
	    case GameState.Init:

	    	// This is when the the first player (host) is created,
	    	// so setup the server variables.

			Game.currentTurn = 0;
			Game.currentRound = 0;
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
			Game.nextRoundCountdown = 2f;

			break;

		case GameState.RestartRound:

			CmdDoRestartRound();

			break;

	    case GameState.PlayingRound:

	    	// Init variables before starting the round

	    	Game.currentTime = 5f;

			break;

	    case GameState.GameOver:

			if (waitForNextRound)
			{
				waitForNextRound = false;
				tag = "Player";
			}

			break;

		default:
			Debug.Log("State not found : " + newState);
			break;
	    }
	}

	void CreateAvailableId()
	{
		int length = Game.totalTurns;

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
		if (Game.currentTurn == numberToCheck)
		{
			// If correct number, move to next turn (reset turn)
			Game.currentTurn++;
			Game.currentTime = 5f;

			if (Game.currentTurn >= Game.totalTurns)
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
	void CmdDoRestartRound ()
	{
		Debug.Log("CmdDoRestartRound");
		RpcDoRestartRound();
	}

	[ClientRpc]
	void RpcDoRestartRound ()
	{
		Debug.Log("RpcDoRestartRound");

		Game.currentTurn = 0;
		Game.currentRound++;
		int totalTurnPerPlayer = 0;

		// Set up total turn for each player count
        if (Game.playerCount < 5)
            totalTurnPerPlayer = Random.Range(5, 10);
		else if (Game.playerCount < 10)
			totalTurnPerPlayer = Random.Range(3, 5);
		else if (Game.playerCount >= 10)
			totalTurnPerPlayer = Random.Range(2, 3);

		GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

		Game.totalTurns = players.Length * totalTurnPerPlayer;

		// Create numbers for every player
		//! Assign available id to each player
        CreateAvailableId();

        /*string str = "";
		for (int i = 0; i < availableIdArray.Count; i++)
			str += availableIdArray[i] + ", ";
		HUD.instance.SetScore(str);*/

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
	}

	[Command]
	void CmdPlayerPress (NetworkInstanceId nid, int number)
	{
		Debug.Log("=====1 " + isServer + " , " + isClient + " , " + isLocalPlayer);
		Debug.Log("CmdPlayerPress");

		RpcPlayerPress(nid, number);
		PlayerPress(nid, number);
	}

	[ClientRpc]
	void RpcPlayerPress (NetworkInstanceId nid, int number)
	{
		Debug.Log("RpcPlayerPress");
		PlayerPress(nid, number);
	}

	void PlayerPress (NetworkInstanceId nid, int number)
	{
		Debug.Log("=====2 " + isServer + " , " + isClient + " , " + isLocalPlayer);
		Debug.Log("PlayerPress : " + nid + " = turn " + Game.currentTurn + " , number " + number);

		string str = "";
		for (int i = 0; i < playerNumbers.Count; i++)
			str += playerNumbers[i] + ", ";
		Debug.Log(str);

		if (Game.currentTurn == number)
		{
			int score = Mathf.FloorToInt(Game.currentTime);

			Debug.Log("PlayerPress right");
			Game.currentTurn++;
			Game.currentTime = 5f;
			playerNumbers.RemoveAt(0);
			AddScore(score);
		}
		else
		{
			Debug.Log("PlayerPress wrong");
			AddScore(-5);
		}
	}

	/*[Command]
	void CmdGotCorrect (int val)
	{
		RpcGotCorrect(val);
		GotCorrect(val);
	}

	[ClientRpc]
	void RpcGotCorrect (int val)
	{
		GotCorrect(val);
	}

	void GotCorrect (int val)
	{
		Game.currentTurn++;
		Game.currentTime = 5f;
		playerNumbers.RemoveAt(0);
		AddScore(val);
	}*/

	[Command]
	void CmdNewPlayerJoined ()
	{
		RpcNewPlayerJoined();
	}

	[ClientRpc]
	void RpcNewPlayerJoined ()
	{
		if (Game.gameState == GameState.WaitingForNextRound)
		{
			Game.nextRoundCountdown = 2f;
		}
		else if (Game.gameState == GameState.PlayingRound)
		{
			// Change tag to make sure that this player does
			// not get listed during CmdDoRestartRound()
			waitForNextRound = true;
			tag = "PlayerInQueue";
		}
	}

    public bool GetWaitNextRound()
    {
        return waitForNextRound;
    }
}