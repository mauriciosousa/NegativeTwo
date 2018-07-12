using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeixisNetwork : MonoBehaviour {

    private int _port;
    private string _address;
    private EvaluationPeer _peer;

    private NetworkView _networkView;
    private EvaluationConfigProperties _config;
    private DeixisEvaluation _evaluation;
    private Wall _wall;
    private ServerConsole _console;

    public NetworkPeerType networkPeerType;

    public bool Connected
    {
        get
        {
            return Network.peerType != NetworkPeerType.Disconnected;
        }
    }

    void Start()
    {
        Application.runInBackground = true;

        _networkView = GetComponent<NetworkView>();
        _config = GetComponent<EvaluationConfigProperties>();
        _evaluation = GetComponent<DeixisEvaluation>();
        _wall = GameObject.Find("Wall").GetComponent<Wall>();
        _console = GetComponent<ServerConsole>();

        _port = _config.port;
        _address = _config.address;
        _peer = _config.Peer;

        if (_peer == EvaluationPeer.SERVER)
        {
            Network.InitializeServer(4, _port, false);
        }
        else
        {
            Network.Connect(_address, _port);
        }
    }

    private void Update()
    {
        networkPeerType = Network.peerType;
    }

    void OnPlayerConnected(NetworkPlayer player)
    {
        _console.writeLine("New Player @ " + player.ipAddress);
    }

    void OnPlayerDisconnected(NetworkPlayer player)
    {
        _console.writeLine("Player Disconnected @ " + player.ipAddress);
    }

    [RPC]
    void RPC_start(int trial, int condition)
    {
        if (_peer != EvaluationPeer.SERVER)
        {
            _evaluation.start(trial, (WarpingCondition) condition);
        }
    }

    public void StartMessage(int trial, WarpingCondition condition)
    {
        _networkView.RPC("RPC_start", RPCMode.Others, trial, (int) condition);
    }

    [RPC]
    void RPC_end()
    {
        if (_peer != EvaluationPeer.SERVER)
        {
            _evaluation.end();
        }
    }

    public void EndMessage()
    {
        _networkView.RPC("RPC_end", RPCMode.Others);
    }

    [RPC]
    void RPC_calibrateHumans(int evaluationPosition)
    {
        if (_peer != EvaluationPeer.SERVER)
        {
            _evaluation.calibrateHumans((EvaluationPosition)Enum.ToObject(typeof(EvaluationPosition), evaluationPosition));
        }
    }

    public void CalibrateHumans(int evaluationPosition)
    {
        _networkView.RPC("RPC_calibrateHumans", RPCMode.Others, evaluationPosition);
    }

    [RPC]
    void RPC_reset()
    {
        if (_peer != EvaluationPeer.SERVER)
        {
            _evaluation.reset();
        }
    }

    public void reset()
    {
        _networkView.RPC("RPC_reset", RPCMode.Others);
    }

    [RPC]
    void RPC_UpdateWallCursor(float x, float y)
    {
        if (_peer == EvaluationPeer.SERVER)
        {
            _wall.updateWallCursor(x, y);
        }
    }

    public void UpdateWallCursor(float x, float y)
    {
        _networkView.RPC("RPC_UpdateWallCursor", RPCMode.Others, x, y);
    }
}
