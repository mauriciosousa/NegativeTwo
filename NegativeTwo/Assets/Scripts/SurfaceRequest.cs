using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class Sensor
{
    public string id = null;
    public Vector3 position = Vector3.zero;
    public Quaternion rotation = Quaternion.identity;
}

public class SurfaceRectangle
{
    private Vector3 _bl;
    public Vector3 SurfaceBottomLeft { get { return _bl; } }
    private Vector3 _br;
    public Vector3 SurfaceBottomRight { get { return _br; } }
    private Vector3 _tl;
    public Vector3 SurfaceTopLeft { get { return _tl; } }
    private Vector3 _tr;
    public Sensor[] sensors = null;

    public Vector3 SurfaceTopRight { get { return _tr; } }

    public Vector3 Center { get { return (_bl + _tr) * 0.5f; } }

    public Quaternion Perpendicular
    {
        get
        {
            Vector3 up = _tl - _bl;
            Vector3 right = _br - _bl;
            Vector3 forward = Vector3.Cross(up, right);

            return Quaternion.LookRotation(forward, up);
        }
    }

    public GameObject CenterGameObject { get; internal set; }

    public SurfaceRectangle(Vector3 BL, Vector3 BR, Vector3 TL, Vector3 TR)
    {
        _bl = BL;
        _br = BR;
        _tl = TL;
        _tr = TR;
    }

    public SurfaceRectangle(string value)
    {
        string[] values = value.Split(MessageSeparators.L0)[1].Split(MessageSeparators.L1);
        string name = values[0];
        sensors = new Sensor[0];

        _bl = CommonUtils.networkStringToVector3(values[1], MessageSeparators.L3);
        _br = CommonUtils.networkStringToVector3(values[2], MessageSeparators.L3);
        _tl = CommonUtils.networkStringToVector3(values[3], MessageSeparators.L3);
        _tr = CommonUtils.networkStringToVector3(values[4], MessageSeparators.L3);
    }

    public override string ToString()
    {
        return "BL" + _bl.ToString() + ", BR" + _br.ToString() + ", TL" + _tl.ToString() + ", TR" + _tr.ToString();
    }
}

public class SurfaceMessage
{
    public static string createRequestMessage(int port)
    {
        return "SurfaceMessage" + MessageSeparators.L0 + Network.player.ipAddress + MessageSeparators.L1 + port;
    }

    public static bool isMessage(string value)
    {
        if (value.Split(MessageSeparators.L0)[0] == "SurfaceMessage")
        {
            return true;
        }
        return false;
    }
}

public class SurfaceRequest : MonoBehaviour
{
    public SurfaceRectangle localSurface = null;
    public SurfaceRectangle remoteSurface = null;

    private Properties _properties;
    private Main _main;

    private DateTime lastTry;
    public int requestInterval = 100;

    public void request()
    {
        _properties = GetComponent<Properties>();
        _main = GetComponent<Main>();

        Debug.Log(this.ToString() + ": Requesting local surface to " + _properties.localSetupInfo.machineAddress + ":" + _properties.localSetupInfo.trackerListenPort + " to receive in " + _properties.localSetupInfo.localSurfaceListen);
        Debug.Log(this.ToString() + ": Requesting remote surface to " + _properties.remoteSetupInfo.machineAddress + ":" + _properties.remoteSetupInfo.trackerListenPort + " to receive in " + _properties.localSetupInfo.remoteSurfaceListen);

        _requestLocal();
        _requestRemote();
        lastTry = DateTime.Now;
    }

    private void _requestLocal()
    {
        //Debug.Log(this.ToString() + ": Request Local surface");
        _request(_properties.localSetupInfo.trackerListenPort, _properties.localSetupInfo.localSurfaceListen);
    }

    private void _requestRemote()
    {
        //Debug.Log(this.ToString() + ": Request Remote surface");
        _request(_properties.remoteSetupInfo.trackerListenPort, _properties.localSetupInfo.remoteSurfaceListen);
    }

    private void _request(string trackerPort, string receivePort)
    {
        UdpClient udp = new UdpClient();
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Broadcast, int.Parse(trackerPort));
        string message = SurfaceMessage.createRequestMessage(int.Parse(receivePort));
        byte[] data = Encoding.UTF8.GetBytes(message);
        udp.Send(data, data.Length, remoteEndPoint);
    }

    void Update()
    {
        /*
        if (_main.LocalSurfaceReceived && _main.RemoteSurfaceReceived) return;

        if (lastTry != null && DateTime.Now > lastTry.AddMilliseconds(requestInterval))
        {
            if (!_main.LocalSurfaceReceived)
            {
                _requestLocal();
            }

            if (!_main.RemoteSurfaceReceived)
            {
                _requestRemote();
            }

            lastTry = DateTime.Now;
        }
        */
    }
}
