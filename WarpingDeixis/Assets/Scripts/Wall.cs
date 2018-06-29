using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour {

    public GameObject wall;
    public GameObject cursor;

    public GameObject Target_7;
    public GameObject Target_8;
    public GameObject Target_9;
    public GameObject Target_10;
    public GameObject Target_11;
    public GameObject Target_18;
    public GameObject Target_19;
    public GameObject Target_20;
    public GameObject Target_21;
    public GameObject Target_22;

    private GameObject _lastEnabled = null;

    void Start () {
        hideWall();

        float distanceToAdd = GameObject.Find("leftHumanPosition").transform.position.z;
        this.transform.position += Vector3.forward * distanceToAdd;
    }

    public void createWall(int trial, bool observer)
    {
        wall.GetComponent<MeshRenderer>().enabled = true;
        if (observer)
        {
            cursor.GetComponent<MeshRenderer>().enabled = true;
        }
        else
        {
            _lastEnabled = _getTarget(trial);
            _lastEnabled.GetComponent<MeshRenderer>().enabled = true;
        }
    }

    public void hideWall()
    {
        wall.GetComponent<MeshRenderer>().enabled = false;
        cursor.GetComponent<MeshRenderer>().enabled = false;
        if (_lastEnabled != null) _lastEnabled.GetComponent<MeshRenderer>().enabled = false;
        _lastEnabled = null;
    }

    void Update () {
		
	}

    internal void destroyCurrent()
    {
        hideWall();
    }

    private GameObject _getTarget(int trial)
    {
        if (trial == 7) return Target_7;
        if (trial == 8) return Target_8;
        if (trial == 9) return Target_9;
        if (trial == 10) return Target_10;
        if (trial == 11) return Target_11;

        if (trial == 18) return Target_18;
        if (trial == 19) return Target_19;
        if (trial == 20) return Target_20;
        if (trial == 21) return Target_21;
        if (trial == 22) return Target_22;

        return null;
    }
}
