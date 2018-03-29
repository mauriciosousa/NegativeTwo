using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvaluationClient : MonoBehaviour {

    private Main _main;
    private NetworkView _networkView;
    private NegativeSpace _negativeSpace;
    private WhackAMole _whackAMole;

    public bool Connected { get { return Network.peerType == NetworkPeerType.Client; } }

    void Start()
    {
        _main = GameObject.Find("Main").GetComponent<Main>();
        _negativeSpace = GameObject.Find("Main").GetComponent<NegativeSpace>();
        _whackAMole = GameObject.Find("WhackAMole").GetComponent<WhackAMole>();
        _networkView = GetComponent<NetworkView>();
    }

    public void Init()
    {
        int port = int.Parse(_main.properties.localSetupInfo.rpcPort);
        string address = _main.properties.serverAddress;

        Debug.Log(this.ToString() + "[RPC]: trying to connect to " + address + ":" + port);
        Network.Connect(address, port);
    }

    #region RPC Calls
    [RPC]
    void RPC_receiveMessage(string s)
    {
        Debug.Log("[RPC_receiveMessage]: " + s);
    }

    [RPC]
    void RPC_startHabituationTask(int condition)
    {
        Debug.Log("[RPC_startHabituationTask]: condition " + (EvaluationConditionType)condition);
        _whackAMole.startHabituationTask(condition);
    }

    [RPC]
    void RPC_startTrial(int condition, int task)
    {
        Debug.Log("[RPC_startTrial]: condition " + (EvaluationConditionType)condition);
        _whackAMole.startTrial(condition, task);
    }

    [RPC]
    void RPC_updateCube(string gameObjectName, int state)
    {
        if (_main.location == Location.Instructor)
        {
            _whackAMole.updateCube(gameObjectName, state);
        }
    }
    public void updateCube(string gameObjectName, int state)
    {
        _networkView.RPC("RPC_updateCube", RPCMode.Others, gameObjectName, state);
    }

    [RPC]
    void RPC_clearBoard()
    {
        Debug.Log("[RPC_clearBoard]");
        _whackAMole.clearBoard();
    }

    [RPC]
    void RPC_microtaskStarted(int microtask, int task)
    {
        if (_main.location == Location.Instructor) _whackAMole.INSTRUCTOR_microtaskStarted(microtask, task);
    }
    internal void reportToInstructorMicroTaskStarted(int microTask, int task)
    {
        _networkView.RPC("RPC_microtaskEnded", RPCMode.Others, microTask, task);
    }

    [RPC]
    void RPC_microtaskEnded(int microtask)
    {
        if (_main.location == Location.Instructor) _whackAMole.INSTRUCTOR_microtaskEnded(microtask);
    }
    internal void reportToInstructorMicroTaskEnded(int microTask)
    {
        _networkView.RPC("RPC_microtaskStarted", RPCMode.Others, microTask);
    }
    #endregion
}
