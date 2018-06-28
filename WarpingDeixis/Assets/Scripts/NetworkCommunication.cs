using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EvaluationPeertype
{
    SERVER,
    CLIENT
}

public class NetworkCommunication : MonoBehaviour {

    public EvaluationPeertype evaluationPeerType;

    private int _port;
    private string _address;

    private NetworkView _networkView;
    private EvaluationConfigProperties _config;
    private ServerConsole _console;
    private Evaluation _evaluation;

    public bool Connected
    {
        get
        {
            return Network.peerType != NetworkPeerType.Disconnected;
        }
    }

	void Start () {
        Application.runInBackground = true;
        _networkView = GetComponent<NetworkView>();
        _config = GetComponent<EvaluationConfigProperties>();
        _console = GetComponent<ServerConsole>();
        _evaluation = GetComponent<Evaluation>();

        _port = _config.port;
        _address = _config.address;
        //evaluationPeerType = _config.evaluatoinPeerType;

        Debug.Log("network started");

        if (evaluationPeerType == EvaluationPeertype.SERVER)
        {
            Network.InitializeServer(2, _port, false);

            _console.writeLine("" + _address + ":" + _port);
            _console.writeLine("server started");
        }
        else
        {
            Network.Connect(_address, _port);
        }
	}
	
	void Update () {
		
	}

    void OnPlayerConnected(NetworkPlayer player)
    {

        _console.writeLineGreen("New Player @ " + player.ipAddress);
    }

    void OnPlayerDisconnected(NetworkPlayer player)
    {
        _console.writeLineRed("Player Disconnected @ " + player.ipAddress);
    }

    [RPC]
    void RPC_calibrateHumans(int evaluationPosition)
    {
        if (evaluationPeerType == EvaluationPeertype.CLIENT)
        {
            _evaluation.OnRPC_calibrateHumans((EvaluationPosition)Enum.ToObject(typeof(EvaluationPosition), evaluationPosition));
        }
    }

    public void calibrateHumans(int evaluationPosition)
    {
        _networkView.RPC("RPC_calibrateHumans", RPCMode.Others, evaluationPosition);
    }

    public void setupTrial(int evaluationPosition, int role, int condition)
    {
        _networkView.RPC("RPC_setupTrial", RPCMode.Others, evaluationPosition, role, condition);
    }

    [RPC]
	void RPC_startTrial(int trialID, int condition)
    {
        if (evaluationPeerType == EvaluationPeertype.CLIENT)
        {
			_evaluation.OnRPC_startTrial(trialID, (EvaluationCondition)Enum.ToObject(typeof(EvaluationCondition), condition));
        }
    }

    public void startTrial(int trialID, int condition)
    {
        _networkView.RPC("RPC_startTrial", RPCMode.Others, trialID, condition);
    }

	[RPC]
	void RPC_endTrial()
	{
		if (evaluationPeerType == EvaluationPeertype.CLIENT)
		{
			_evaluation.OnRPC_endTrial();
		}
	}

	public void endTrial()
	{
		_networkView.RPC("RPC_endTrial", RPCMode.Others);
	}

    [RPC]
    void RPC_reset()
    {
        if (evaluationPeerType == EvaluationPeertype.CLIENT)
        {
            _evaluation.reset();
        }
    }

    public void reset()
    {
        _networkView.RPC("RPC_endTrial", RPCMode.Others);
    }
}
