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
    internal string localSurfaceListen;
    internal Location location;
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
    public NegativeSpaceInfo localSetupInfo;
    public NegativeSpaceInfo remoteSetupInfo;

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

    public void load(Location myLocation)
    {
        _location = myLocation;
        if (System.IO.File.Exists(configFilenameFullPath))
        {
            if (_location == Location.Instructor)
            {
                localSetupInfo = _retrieve(Location.Instructor);
                remoteSetupInfo = _retrieve(Location.Assembler);
            }
            else
            {
                localSetupInfo = _retrieve(Location.Assembler);
                remoteSetupInfo = _retrieve(Location.Instructor);
            }
        }
        else throw new NegativeRuntimeException("Cannot find config file (" + configFilenameFullPath + ")");

    }

    private NegativeSpaceInfo _retrieve(Location location)
    {
        NegativeSpaceInfo info = new NegativeSpaceInfo();

        info.location = location;
        info.machineAddress = load(location.ToString() + ".machine.address");
        info.rpcPort = load(location.ToString() + ".rpc.port");
        info.receiveHandheldPort = load(location.ToString() + ".rcv.handheld.port");
        info.trackerBroadcastPort = load(location.ToString() + ".tracker.broadcast.port");
        info.trackerListenPort = load(location.ToString() + ".tracker.listen.port");
        info.avatarListenPort = load(location.ToString() + ".avatar.listen.port");
        info.localSurfaceListen = load(location.ToString() + ".local.surface.listen");
        info.remoteSurfaceListen = load(location.ToString() + ".remote.surface.listen");
        negativeSpaceLength = float.Parse(load("ns.length"));
        serverAddress = load("rpc.server.address");


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
