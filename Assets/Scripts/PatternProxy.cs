using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PatternProxy : MonoBehaviour {

    
    public Vector2[] TargetPoints;
    public int[] TargetSegments;

    public Segment SegmentPF;
    public Vertex VertexPF;

    private List<Vertex> _verteces = new List<Vertex>();
    private List<Segment> _segments = new List<Segment>();

    private Vertex _lastContact;

    public int MyID;

    public float SegmentLifetime;
    public List<int> Players;

    private IGameClient _networkClient = new LocalGameClient();


    // Use this for initialization
    void Start () {

        _networkClient.onLoaded += ServerLoaded;
        _networkClient.onEdgeFilled += EdgeFilledByPlayer;
        _networkClient.onLevelWon += LevelWon;

        _networkClient.Load();
    }

    private void LevelWon() {
        print("Won");
    }

    private void EdgeFilledByPlayer(int playerID, int edgeId) {
        if (playerID != MyID) {
            _segments[edgeId].AddPlayer(Players.IndexOf(MyID));
        }
        
    }

    private void ServerLoaded(PatternModel patternModel, int myID) {
        MyID = myID;
        //CreatePattern(patternModel);
        CreatePattern();
        AdjustCollidersSize();
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
        Vertex vertex = collider.GetComponent<Vertex>();
        if (vertex != null && _lastContact != vertex && _verteces.IndexOf(vertex) > -1) {

            foreach (Segment seg in _segments) {
                if ((seg.VertexA == vertex || seg.VertexB == vertex) && (seg.VertexA == _lastContact || seg.VertexB == _lastContact)) {
                    seg.AddPlayer(Players.IndexOf(MyID));
                    //seg.AddPlayer(Players.IndexOf(UnityEngine.Random.Range(1, 3)));
                    _networkClient.NotifyFilledEdge(_segments.IndexOf(seg));
                }
            }

            _lastContact = vertex;
            
        }
    }
}
