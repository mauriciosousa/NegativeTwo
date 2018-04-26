using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Location
{
    Instructor,
    Assembler
}

public class NegativeSpaceInfo
{
    internal string avatarListenPort;
    internal object machineAddress;
    internal string receiveHandheldPort;
    internal string remoteSurfaceListen;
    internal string rpcPort;
    internal string trackerBroadcastPort;
    internal string trackerListenPort;
}

public class Properties : MonoBehaviour
{
    private Location _location;
    public NegativeSpaceInfo info;

    public string configFilename;
    public float negativeSpaceLength = 0.1f;

    public string serverAddress = "127.0.0.1";

    public string configFilenameFullPath
    {
        get
        {
            return Application.dataPath + System.IO.Path.DirectorySeparatorChar + configFilename;
        }
    }

    public void load()
    {
        if (System.IO.File.Exists(configFilenameFullPath))
        {
            info = _retrieve();
        }
        else throw new NegativeRuntimeException("Cannot find config file (" + configFilenameFullPath + ")");

    }

    private NegativeSpaceInfo _retrieve()
    {
        NegativeSpaceInfo info = new NegativeSpaceInfo();

        info.machineAddress = load("tracker.address");
        info.trackerBroadcastPort = load("tracker.broadcast.port");
        info.trackerListenPort = load("tracker.listen.port");
        info.avatarListenPort = load("avatar.listen.port");
        
        return info;
    }

    private string load(string property)
    {
        if (File.Exists(configFilenameFullPath))
        {
            List<string> lines = new List<string>(File.ReadAllLines(configFilenameFullPath));
            foreach (string line in lines)
            {
                if (line.Split('=')[0] == property)
                {
                    Debug.Log("Found: " + property + " - " + line.Split('=')[1]);
                    return line.Split('=')[1];
                }
            }
            throw new NegativeRuntimeException(property + ": Not Found");
        }
        else
            throw new NegativeRuntimeException(property + ": Not Found");
    }
}
