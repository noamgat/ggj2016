﻿using UnityEngine;
using System.Collections;

public class GameClientWrapper : MonoBehaviour {

	//Tester...
	IGameClient gameClient;

	public string url = "ws://localhost:8000/room";

	// Use this for initialization
	void Start () {
		if (url.StartsWith ("local")) {
			gameClient = new LocalGameClient ();
		} else {
			gameClient = new GameClient (url);
		}
		gameClient.onConnected += GameClient_onConnected;
		gameClient.onLevelWon += GameClient_onLevelWon;
		gameClient.onEdgeFilled += GameClient_onEdgeFilled;
		gameClient.onNumberOfPlayersChanged += GameClient_onNumberOfPlayersChanged;
		gameClient.onClientError += GameClient_onClientError;
		gameClient.onLevelStarted += GameClient_onLevelStarted;
		gameClient.onLevelLost += GameClient_onLevelLost;
		gameClient.onGameCompleted += GameClient_onGameCompleted;
		gameClient.onGameStarted += GameClient_onGameStarted;
		Debug.Log ("Calling load");
		gameClient.Connect ();
	}

	void GameClient_onGameStarted ()
	{
		Debug.Log ("Game started");
	}

	void GameClient_onGameCompleted ()
	{
		Debug.Log ("Game completed");
	}

	void GameClient_onLevelLost ()
	{
		Debug.Log ("Level lost");
	}

	private PatternModel pattern;

	void GameClient_onLevelStarted (PatternModel obj)
	{
		this.pattern = obj;
		Debug.Log ("Level started");
	}

	void GameClient_onClientError (string obj)
	{
		Debug.Log ("Client error : " + obj);
	}

	void GameClient_onNumberOfPlayersChanged (int obj)
	{
		Debug.Log ("Number of players changed : " + obj);
	}

	void GameClient_onEdgeFilled (int arg1, int arg2)
	{
		Debug.Log ("Edge filled");
	}

	void GameClient_onLevelWon ()
	{
		Debug.Log ("Level won");
	}

	void GameClient_onConnected (int arg2)
	{
		Debug.Log ("Connected as player : " + arg2);
	}

	[ContextMenu("Send filled all edges")]
	public void SendFilledEdgeZero(){
		for (int i = 0; i < this.pattern.edges.Count; i++) {
			gameClient.NotifyFilledEdge (i);
		}
	}


	[ContextMenu("Send request start game")]
	public void RequestStartGame() {
		gameClient.RequestStartGame ();
	}

	[ContextMenu("Send request start level")]
	public void RequestStartLevel() {
		gameClient.RequestStartLevel ();
	}


}
