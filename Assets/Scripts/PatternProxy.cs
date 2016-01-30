using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class PatternProxy : MonoBehaviour {
    
    public Image ProgBar;

    public Vector2[] TargetPoints;
    public int[] TargetSegments;

    public Segment SegmentPF;
    public Vertex VertexPF;

    private List<Vertex> _verteces = new List<Vertex>();
    private List<Segment> _segments = new List<Segment>();

    private Vertex _lastContact;

    internal int _myID;

    public float SegmentLifetime;
    public List<int> Players;

    //private IGameClient _networkClient = new LocalGameClient();
    //private IGameClient _networkClient = new GameClient("wss://ggj2016-server.herokuapp.com/room");
    private IGameClient _networkClient;

    private bool _patternNedsUpdate = false;
    private PatternModel _patternModel;

    public bool UseLocal;

    private bool _isInGame = false;
    



    // Use this for initialization
    void Start () {

        if (UseLocal) {
            _networkClient = new LocalGameClient();
        } else {
            _networkClient = new GameClient("wss://ggj2016-server.herokuapp.com/room");
        }
        

		_networkClient.onConnected += ServerConnected;
		_networkClient.onLevelStarted += ServerLevelStarted;
        _networkClient.onEdgeFilled += EdgeFilledByPlayer;
        _networkClient.onLevelWon += LevelWon;
		_networkClient.onClientError += ServerHadError;
		_networkClient.onGameCompleted += ServerCompletedGame;
		_networkClient.onLevelLost += ServerLostLevel;
		_networkClient.onNumberOfPlayersChanged += ServerNumberOfPlayersChanged;

        _networkClient.Connect();
    }

    void ServerNumberOfPlayersChanged (int obj)
    {
		
    }

    void ServerLostLevel ()
    {
		
    }

    void ServerCompletedGame ()
    {
		
    }

    void ServerHadError (string obj)
    {
		
    }

    internal void StartGame() {
        _isInGame = true;
        ShowInitialSplash();
    }
    
    private void LevelWon() {
        print("Won");
    }

    private void EdgeFilledByPlayer(int playerID, int edgeId) {
        if (playerID != _myID) {
            _segments[edgeId].AddPlayer(Players.IndexOf(playerID));
        }
        
    }

    private void ServerConnected(int myID) {
        _myID = myID;
        
        _patternModel = patternModel;
        _patternNedsUpdate = true;

    }

	private void ServerLevelStarted(PatternModel patternModel) {
		_serverStatuses = ServerStatuses.Ready;
		_patternModel = patternModel;

	}

    private void AdjustCollidersSize() {
        foreach (Vertex verA in _verteces) {
            foreach (Vertex verB in _verteces) {
                if (verA != verB) {
                    float dist = (verA.location - verB.location).magnitude;
                    if (dist < 0.09f) {
                        verA.SetColliderSize(dist * 0.35f);
                        verB.SetColliderSize(dist * 0.35f);
                    }
                }
            }
        }
    }

    private void CreatePattern(PatternModel patternModel) {
        for (int i = 0; i < patternModel.points.Length; i++) {
            _verteces.Add(Instantiate(VertexPF));
            _verteces[_verteces.Count - 1].location = patternModel.points[i];
            _verteces[_verteces.Count - 1].Place();
        }


        for (int i = 0; i < patternModel.edges.Count; i++) {
            Segment seg = Instantiate(SegmentPF);

            _segments.Add(seg);
            seg.Init(Players.Count, SegmentLifetime);
            seg.VertexA = _verteces[patternModel.edges[i][0]];
            seg.VertexB = _verteces[patternModel.edges[i][1]];
            seg.Place();
        }
    }
    
    public void CreatePattern() {

        //stub place

        for (int i = 0; i < TargetPoints.Length; i++) {
            _verteces.Add(Instantiate(VertexPF));
            _verteces[_verteces.Count - 1].location = TargetPoints[i];
            _verteces[_verteces.Count - 1].Place();
        }


        for (int i = 0; i < TargetSegments.Length / 3; i++) {

            int numOfDivisions = TargetSegments[i * 3 + 2];

            Vertex absFrom = _verteces[TargetSegments[i * 3]];
            Vertex absTo = _verteces[TargetSegments[i * 3 + 1]];
            Vertex from;
            Vertex to = absFrom;

            for (int j = 0; j < numOfDivisions; j++) {

                from = to;

                if (j < numOfDivisions - 1) {

                    to = Instantiate(VertexPF);

                    _verteces.Add(to);

                    Vector2 trg = (absTo.location - absFrom.location);
                    trg = (trg / (float)numOfDivisions);
                    trg = trg * (float)(j + 1);

                    to.location = absFrom.location + trg;
                    to.Place();

                    
                } else {
                    to = absTo;
                }
                
                Segment seg = Instantiate(SegmentPF);

                _segments.Add(seg);
                seg.Init(Players.Count, SegmentLifetime);
                seg.VertexA = from;
                seg.VertexB = to;
                seg.Place();
                
            }

            
        }

            
    }

    internal void CheckCollision(Collider collider) {
        if (!_isInGame) return;

        Vertex vertex = collider.GetComponent<Vertex>();
        if (vertex != null && _lastContact != vertex && _verteces.IndexOf(vertex) > -1) {

            foreach (Segment seg in _segments) {
                if ((seg.VertexA == vertex || seg.VertexB == vertex) && (seg.VertexA == _lastContact || seg.VertexB == _lastContact)) {
                    seg.AddPlayer(Players.IndexOf(_myID));
                    //seg.AddPlayer(Players.IndexOf(UnityEngine.Random.Range(1, 3)));
                    _networkClient.NotifyFilledEdge(_segments.IndexOf(seg));
                }
            }

            _lastContact = vertex;
            
        }
    }

    void Update() {
        if (_patternNedsUpdate) {

            foreach(Segment seg in _segments) { 
                Destroy(seg.gameObject);
            }

            _segments.Clear();


            foreach (Vertex ver in _verteces) {
                Destroy(ver.gameObject);
            }

            _verteces.Clear();

            //CreatePattern(_patternModel);
            CreatePattern();
            AdjustCollidersSize();
            _patternNedsUpdate = false;
        }

        int workngSegments = 0;

        foreach (Segment seg in _segments) {
            if (seg.IsWorking) workngSegments++;
        }
        
        
        if (_segments.Count > 0) ProgBar.fillAmount = (float)workngSegments / _segments.Count;
    }

    private void ShowInitialSplash() {
        foreach (Segment seg in _segments) {
            seg.Splash(0.5f * (seg.VertexA.location.y + seg.VertexB.location.y));
        }
    }

    public void DebugAction() {
        /*
        string s = "{\n";
        s += "\"points\": [\n";
        
        foreach (Vertex ver in _verteces) {
            s += "[" + ver.location.x + ',' + ver.location.y + "],\n";
        }
        s += "]\n";
        s += "\"edges\": [\n";

        foreach (Segment seg in _segments) {
            s += "[" + _verteces.IndexOf(seg.VertexA) + ',' + _verteces.IndexOf(seg.VertexB) + "],\n";
        }
        s += "]\n";

        s += "}\n";

        print( s);
        */
        _patternNedsUpdate = true;

    }
}
