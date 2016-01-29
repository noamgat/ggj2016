using UnityEngine;
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
		gameClient.onLoaded += GameClient_onLoaded;
		gameClient.onLevelWon += GameClient_onLevelWon;
		gameClient.onEdgeFilled += GameClient_onEdgeFilled;
		Debug.Log ("Calling load");
		gameClient.Load ();
	}

	void GameClient_onEdgeFilled (int arg1, int arg2)
	{
		Debug.Log ("Edge filled");
	}

	void GameClient_onLevelWon ()
	{
		Debug.Log ("Level won");
	}

	void GameClient_onLoaded (PatternModel arg1, int arg2)
	{
		Debug.Log ("Level loaded");
	}

	[ContextMenu("Send filled edge zero")]
	public void SendFilledEdgeZero(){
		gameClient.NotifyFilledEdge (0);
	}
}
