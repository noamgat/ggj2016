using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PatternProxy : MonoBehaviour {

    public Segment SegmentPF;

    public List<Vertex> Verteces;
    public List<Segment> Segments;

    // Use this for initialization
    void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void CreatePattern() {

        Segments.Add(Instantiate(SegmentPF));

        Segments[0].VertexA = Verteces[0];
        Segments[0].VertexB = Verteces[1];
        Segments[0].Place();
    }

    [Serializable]
    public class Vertex {
        public Vector2 location;
    }
    
}
