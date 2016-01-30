using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Holoville.HOTween;
using System;

public class GameManager : MonoBehaviour {

    public GameObject MainMenuUI;
    public GameObject IngameUI;
    public Camera cam;

    // Current game state
    private enum States { Menu, Connecting, Ingame, Loss, Win }
    private States _state;

    public PatternProxy PatternProxyInst;

    private IGameClient _networkClient;

    public bool UseFakeNetworClient;

    private bool _pendingMainThreadAction = false;

    public Animator CamAnimator;
    public Animator TempleAnimator;

    public Button StartButton;
    public Text CountText;

    private int _numPlayers;

    // Use this for initialization
    void Start () {
        // 
        IngameUI.SetActive(false);
        MainMenuUI.SetActive(true);


        if (UseFakeNetworClient) {
            _networkClient = new LocalGameClient();
        } else {
            _networkClient = new GameClient("wss://ggj2016-server.herokuapp.com/room");
        }


        _networkClient.onConnected += ServerConnected;
        _networkClient.onLevelStarted += ServerLevelStarted;
        _networkClient.onEdgeFilled += EdgeFilledByPlayer;
        _networkClient.onLevelWon += ServerLevelWon;
        _networkClient.onClientError += ServerHadError;
        _networkClient.onGameCompleted += ServerCompletedGame;
        _networkClient.onLevelLost += ServerLostLevel;
        _networkClient.onNumberOfPlayersChanged += ServerNumberOfPlayersChanged;

        _networkClient.Connect();
    }

    private void ServerConnected(int myID) {
        PatternProxyInst.myID = myID;
    }

    void ServerNumberOfPlayersChanged(int players) {
        _numPlayers = players;
        
    }

    private void ServerLevelStarted(PatternModel patternModel) {
        PatternProxyInst.UpdatePattern(patternModel);

        _pendingMainThreadAction = true;

       
    }

    private void ServerLevelWon() {
        print("Won");
    }

    void ServerLostLevel() {

    }

    void ServerCompletedGame() {

    }

    void ServerHadError(string obj) {

    }

    internal void EdgeFilledByPlayer(int playerID, int edgeId) {
        PatternProxyInst.EdgeFilledByPlayer(playerID, edgeId);

    }

    internal void NotifyFilledEdge(int edgeIndex) {
        _networkClient.NotifyFilledEdge(edgeIndex);
    }

    public void PlayStartAnim() {
        MainMenuUI.SetActive(false);
        CamAnimator.SetTrigger("StartIntro");
        TempleAnimator.SetTrigger("StartIntro");
    }

    internal void StartAnimComplete() {
        _networkClient.RequestStartGame();
    }
    

    void Update() {
        if (_pendingMainThreadAction) {

            IngameUI.SetActive(true);
            
            CamAnimator.SetTrigger("StartLevel");
            
            PatternProxyInst.StartGame();
            
            _pendingMainThreadAction = false;
        }

        StartButton.interactable = _numPlayers > 0;
        CountText.text = "Souks Connected " + _numPlayers + "/4";
    }

   
}
