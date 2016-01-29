using UnityEngine;
using System.Collections;
using WebSocketSharp;

public class GameClient : MonoBehaviour {

	public string url = "ws://localhost:8000/";
	private WebSocket webSocket;

	// Use this for initialization
	void Start () {
		webSocket = new WebSocket (url);
		webSocket.OnOpen += WebSocket_OnOpen;
		webSocket.OnMessage += WebSocket_OnMessage;
		webSocket.OnClose += WebSocket_OnClose;
		webSocket.OnError += WebSocket_OnError;

		// To change the logging level.
		webSocket.Log.Level = LogLevel.Trace;

		// To change the wait time for the response to the Ping or Close.
		webSocket.WaitTime = System.TimeSpan.FromSeconds (10);

		Debug.Log ("About to connect");
		webSocket.Connect ();
	}

	void WebSocket_OnError (object sender, ErrorEventArgs e)
	{
		Debug.Log("Websocket error : " + e.Message);
	}

	void WebSocket_OnClose (object sender, CloseEventArgs e)
	{
		Debug.Log ("Websocket close : " + e.Reason);
	}

	void WebSocket_OnOpen (object sender, System.EventArgs e)
	{
		Debug.Log ("Websocket open");
	}

	void WebSocket_OnMessage (object sender, MessageEventArgs e)
	{
		Debug.Log ("Websocket message: " + e.Data);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
