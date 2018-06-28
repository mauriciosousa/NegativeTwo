using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EvaluationConfigProperties : MonoBehaviour {

    public string configFilename = "evaluationConfig.txt";

    public string configFilenameFullPath
    {
        get
        {
            return Application.dataPath + System.IO.Path.DirectorySeparatorChar + configFilename;
        }
    }

    /*public EvaluationPeertype evaluatoinPeerType
    {
        get
        {
            return (EvaluationPeertype) Enum.Parse(typeof(EvaluationPeertype), _load("peer.type"));
        }
    }*/

    public EvaluationPeer Peer
    {
        get
        {
            return (EvaluationPeer)Enum.Parse(typeof(EvaluationPeer), _load("deixis.peer"));
        }
    }

    public int port
    {
        get
        {
            return int.Parse(_load("server.port"));
        }
    }

    public string address
    {
        get
        {
            return _load("server.address");
        }
    }

    public EvaluationPosition clientPosition
    {
        get
        {
            return (EvaluationPosition)Enum.Parse(typeof(EvaluationPosition), _load("client.position"));
        }
    }

    private string _load(string property)
    {
        if (File.Exists(configFilenameFullPath))
        {
            List<string> lines = new List<string>(File.ReadAllLines(configFilenameFullPath));
            foreach (string line in lines)
            {
                if (line.Split('=')[0] == property)
                {
                    //Debug.Log("Found: " + property + " - " + line.Split('=')[1]);
                    return line.Split('=')[1];
                }
            }
            throw new Exception(property + ": Not Found");
        }
        else
            throw new Exception(property + ": Not Found");
    }
}
