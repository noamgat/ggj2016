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
    private enum PendingActions{ StartGame, StartRound, PlayWin, PlayLoose, GameComplete };
    private States _state;

    private PendingActions _pendingActions;

    public PatternProxy PatternProxyInst;

    private IGameClient _networkClient;

    public bool UseFakeNetworClient;

    private bool _pendingMainThreadAction = false;

    public Animator CamAnimator;
    public Animator TempleAnimator;

    public Button StartButton;
    public Text CountText;

    private int _numPlayers;
	private int _numLevelsWon = 0;

    // Use this for initialization
    void Start () {
        
        
        

        InitMainMenu();
    }

    private void InitMainMenu() {

        if (_networkClient != null) {
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

    internal void EdgeFilledByPlayer(int playerID, int edgeId) {
        PatternProxyInst.EdgeFilledByPlayer(playerID, edgeId);

    }

    internal void NotifyFilledEdge(int edgeIndex) {
        _networkClient.NotifyFilledEdge(edgeIndex);
    }

    public void PlayStartAnim() {
        CamAnimator.SetTrigger("StartIntro");
        TempleAnimator.SetTrigger("StartIntro");
        MainMenuUI.SetActive(false);
        _networkClient.RequestStartGame();
    }

    internal void StartRound() {
        _networkClient.RequestStartLevel();
    }
    

    void Update() {
        if (_pendingMainThreadAction) {

            switch (_pendingActions) {
                case PendingActions.StartGame:
                    MainMenuUI.SetActive(false);
                    
                    break;
                case PendingActions.StartRound:

                    IngameUI.SetActive(true);
					SoundManager.Instance.PlayBackgroundMusic ();
                    CamAnimator.SetTrigger("StartLevel");

                    PatternProxyInst.StartRound();

                    _pendingMainThreadAction = false;
                    break;
				case PendingActions.PlayWin:
					IngameUI.SetActive (false);
					SoundManager.Instance.PlayLevelWinClip (_numLevelsWon);
                    Invoke("StartRound", 3);
                    PatternProxyInst.EndRound(true);
                    break;
				case PendingActions.PlayLoose:
					SoundManager.Instance.PlayLevelLoseClip ();
                    Invoke("InitMainMenu", 3);
                    PatternProxyInst.EndRound(false);
                    break;
                case PendingActions.GameComplete:
                    Invoke("InitMainMenu", 3);
                    
                    break;
            }
        }

        StartButton.interactable = _numPlayers > 0;
        CountText.text = "Souks Connected " + _numPlayers + "/4";
    }

   
}
