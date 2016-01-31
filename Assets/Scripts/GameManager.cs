using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Holoville.HOTween;
using System;

public class GameManager : MonoBehaviour {

    public GameObject MainMenuUI;
    public GameObject IngameUI;
    public Camera cam;

    // Current game state
    private enum States { Menu, Connecting, Ingame, Loss, Win }
    private enum PendingActions{ StartGame, StartRound, PlayWin, PlayLoose, GameComplete };
    private States _state;

    private PendingActions _pendingActions;

    public PatternProxy PatternProxyInst;

    private IGameClient _networkClient;

    public bool UseFakeNetworClient;

    private bool _pendingMainThreadAction = false;

    public Animator CamAnimator;
    public Animator Altar1;
    public Animator Altar2;
    public Animator Satan;

    public Button StartButton;
    public Text CountText;

    public Text FloorText;

    private int _numPlayers;
	private int _numLevelsWon = 0;


    public Texture[] LevelTextures;
    public GameObject LevelBackground;

    private Queue<Action> _mainThreadActions;
    private int _currentLevel;

    public int MinPlayers;

    // Use this for initialization
    void Start () {

        _mainThreadActions = new Queue<Action>();
        InitMainMenu();
    }

    private void InitMainMenu() {

        PatternProxyInst.Stopcandles();

        if (UseFakeNetworClient) {
            _networkClient = new LocalGameClient();
        } else {
            _networkClient = new GameClient("wss://ggj2016-server.herokuapp.com/room");
        }


        _networkClient.onConnected += ServerConnected;
        _networkClient.onLevelStarted += ServerLevelStarted;
        _networkClient.onGameStarted += ServerGameStarted;
        _networkClient.onEdgeFilled += EdgeFilledByPlayer;
        _networkClient.onLevelWon += ServerLevelWon;
        _networkClient.onClientError += ServerHadError;
        _networkClient.onGameCompleted += ServerCompletedGame;
        _networkClient.onLevelLost += ServerLostLevel;
        _networkClient.onNumberOfPlayersChanged += ServerNumberOfPlayersChanged;

        _networkClient.Connect();

        IngameUI.SetActive(false);
        MainMenuUI.SetActive(true);

        _currentLevel = -1;
    }

    private void KillNetwork() {
        
        _networkClient.onConnected -= ServerConnected;
        _networkClient.onLevelStarted -= ServerLevelStarted;
        _networkClient.onGameStarted -= ServerGameStarted;
        _networkClient.onEdgeFilled -= EdgeFilledByPlayer;
        _networkClient.onLevelWon -= ServerLevelWon;
        _networkClient.onClientError -= ServerHadError;
        _networkClient.onGameCompleted -= ServerCompletedGame;
        _networkClient.onLevelLost -= ServerLostLevel;
        _networkClient.onNumberOfPlayersChanged -= ServerNumberOfPlayersChanged;

        (_networkClient as GameClient).Dispose();
        
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
        _pendingActions = PendingActions.StartRound;
        
}


    private void ServerGameStarted() {
        _pendingMainThreadAction = true;
        _pendingActions = PendingActions.StartGame;
		_numLevelsWon = 0;
    }



    private void ServerLevelWon() {
        _pendingActions = PendingActions.PlayWin;
        _pendingMainThreadAction = true;
		_numLevelsWon++;
    }

    void ServerLostLevel() {
        _pendingMainThreadAction = true;
        _pendingActions = PendingActions.PlayLoose;
    }

    void ServerCompletedGame() {
        _pendingActions = PendingActions.GameComplete;
        _pendingMainThreadAction = true;
    }

    void ServerHadError(string obj) {

    }

    private void PerformOnMainThread(System.Action action) {
        lock (_mainThreadActions) {
            _mainThreadActions.Enqueue(action);
        }
    }

    internal void EdgeFilledByPlayer(int playerID, int edgeId) {
        PerformOnMainThread(() => PatternProxyInst.EdgeFilledByPlayer(playerID, edgeId));
    }

    internal void NotifyFilledEdge(int edgeIndex) {
        _networkClient.NotifyFilledEdge(edgeIndex);
    }

    public void PlayStartAnim() {
        
        _networkClient.RequestStartGame();
    }

    internal void StartRound() {
        _networkClient.RequestStartLevel();
    }
    
    private float PlayInerLEvelAnim() {
        if (_currentLevel == 0) {
            Altar1.SetTrigger("SwitchLevel");
        } else if (_currentLevel == 1) {

            Altar2.SetTrigger("SwitchLevel");
        }else if (_currentLevel == 2) {
            
            Satan.SetTrigger("SwitchLevel");
        }
        
        CamAnimator.SetTrigger("SwitchLevel");
        
        
        return 4;
    }


    void Update() {
        if (_pendingMainThreadAction) {

            switch (_pendingActions) {
                case PendingActions.StartGame:
                    MainMenuUI.SetActive(false);
                    CamAnimator.SetTrigger("StartLevel");
                    FloorText.text = "";
                    Invoke("StartRound", 1);
                    break;
                case PendingActions.StartRound:
                    _currentLevel++;
                    LevelBackground.GetComponent<Renderer>().material.mainTexture = LevelTextures[_currentLevel];
                    IngameUI.SetActive(true);
					SoundManager.Instance.PlayBackgroundMusic ();
                    

                    PatternProxyInst.StartRound();
                    FloorText.text = "Engrave";
                    
                    break;
				case PendingActions.PlayWin:
					IngameUI.SetActive (false);
					SoundManager.Instance.PlayLevelWinClip (_numLevelsWon);
                    Invoke("StartRound", PlayInerLEvelAnim());
                    PatternProxyInst.EndRound(true);

                    FloorText.text = "DARKNESS!";

                    break;
				case PendingActions.PlayLoose:
					SoundManager.Instance.PlayLevelLoseClip ();
                    Invoke("InitMainMenu", 3);
                    PatternProxyInst.EndRound(false);
                    FloorText.text = "You are weak";
                    break;
                case PendingActions.GameComplete:
                    KillNetwork();
                    FloorText.text = "RITUAL\nCOMPLETE";
                    Invoke("InitMainMenu", 3);
                    
                    break;
            }

            _pendingMainThreadAction = false;
        }

        StartButton.interactable = _numPlayers >= MinPlayers;
        CountText.text = "Souls Connected " + _numPlayers + "/4";

        lock (_mainThreadActions) {
            while (_mainThreadActions.Count > 0) {
                _mainThreadActions.Dequeue()();
            }
        }
    }

   
    public void DoDebugMng() {
        ServerLevelWon();
    }


}
