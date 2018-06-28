using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour {

    public GameObject wall;

	void Start () {
        hideWall();
	}

    public void createWall(EvaluationTask task)
    {
        wall.GetComponent<MeshRenderer>().enabled = true;

    }

    public void hideWall()
    {
        wall.GetComponent<MeshRenderer>().enabled = false;
    }

    void Update () {
		
	}
}
