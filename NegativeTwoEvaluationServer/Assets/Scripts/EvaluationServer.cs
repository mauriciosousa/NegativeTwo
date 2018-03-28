﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvaluationServer : MonoBehaviour {

    public int port;

    private NetworkView _networkView;

    public GUIStyle HugeStyle;
    public GUIStyle ValueStyle;
    public GUIStyle MEGAHUGESTYLE;

    private int condition = 1;

    void Start ()
    {
        Application.runInBackground = true;

        Screen.SetResolution(800, 800, true);
        Screen.fullScreen = false;

        _networkView = GetComponent<NetworkView>();

        Network.InitializeServer(2, port, false);
	}

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _networkView.RPC("RPC_receiveMessage", RPCMode.Others, "Oiiiiiiiiii");
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(2 * Screen.width / 3, Screen.height - 3.9f * Screen.height / 4, 50, 1000), "" + condition, MEGAHUGESTYLE);


        int left = 10;
        int top = 10;
        int lineSize = 30;

        GUI.Label(new Rect(left, top, 50, lineSize), "Network Port: ", HugeStyle); left += 160;
        GUI.Label(new Rect(left, top, 50, lineSize), "" + port, ValueStyle); left = 10;

        top += lineSize;

        GUI.Label(new Rect(left, top, 50, lineSize), "Server running: ", HugeStyle); left += 160;
        GUI.Label(new Rect(left, top, 50, lineSize), "" + Network.peerType, ValueStyle); left = 10;

        top += lineSize;

        GUI.Label(new Rect(left, top, 50, lineSize), "Number of clients: ", HugeStyle); left += 160;
        GUI.Label(new Rect(left, top, 50, lineSize), "" + Network.connections.Length, ValueStyle); left = 10;

        top += 2 * lineSize;

        GUI.Label(new Rect(left, top, 50, lineSize), "Condition: ", HugeStyle); left += 160;
        GUI.Label(new Rect(left, top, 50, lineSize), "" + condition, ValueStyle); left = 10;

        top += lineSize;


        GUI.Label(new Rect(left, top, 50, lineSize), "Choose Condition: ", HugeStyle); left += 160;

        if (GUI.Button(new Rect(left, top, 40, 40), "1"))
        {
            condition = 1;
        }
        left += 40;
        if (GUI.Button(new Rect(left, top, 40, 40), "2"))
        {
            condition = 2;
        }
        left += 40;
        if (GUI.Button(new Rect(left, top, 40, 40), "3"))
        {
            condition = 3;
        }
        left += 40;
        if (GUI.Button(new Rect(left, top, 40, 40), "4"))
        {
            condition = 4;
        }
        left += 40;

        top += 4 * lineSize;

        left = 10;
        GUI.Label(new Rect(left, top, 50, lineSize), "Start stuff: ", HugeStyle); left += 160;
        top += 2 * lineSize;

        if (GUI.Button(new Rect(left, top, 300, lineSize * 2), "START HABITUATION TASK"))
        {
            _networkView.RPC("RPC_startHabituationTask", RPCMode.Others, condition);
        }
        top += 6 * lineSize;

        left += 50;

        if (GUI.Button(new Rect(left, top, 300, lineSize * 2), "START TASK 1"))
        {
            _networkView.RPC("RPC_startTrial", RPCMode.Others, condition, 1);
        }

        top += 3 * lineSize;

        left += 50;

        if (GUI.Button(new Rect(left, top, 300, lineSize * 2), "START TASK 2"))
        {
            _networkView.RPC("RPC_startTrial", RPCMode.Others, condition, 2);
        }

        top += 3 * lineSize;

        left += 50;

        if (GUI.Button(new Rect(left, top, 300, lineSize * 2), "START TASK 3"))
        {
            _networkView.RPC("RPC_startTrial", RPCMode.Others, condition, 3);
        }

        top += 3 * lineSize;

        left += 50;

        /*
        if (GUI.Button(new Rect(left, top, 300, lineSize * 2), "CLEAR BOARD"))
        {
            _networkView.RPC("RPC_clearBoard", RPCMode.Others);
        }
        */

    }

    #region RPC Calls
    [RPC]
    void RPC_receiveMessage(string s) { }

    [RPC]
    void RPC_startHabituationTask(int condition) { }

    [RPC]
    void RPC_startTrial(int condition, int task) { }

    [RPC]
    void RPC_clearBoard() { }

    #endregion
}