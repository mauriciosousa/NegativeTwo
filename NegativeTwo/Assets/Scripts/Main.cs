using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable()]
public class NegativeRuntimeException : System.Exception
{
    public NegativeRuntimeException() : base() { }
    public NegativeRuntimeException(string message) : base(message) { }
    public NegativeRuntimeException(string message, System.Exception inner) : base(message, inner) { }
    protected NegativeRuntimeException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
}

public class Main : MonoBehaviour {

    public Location location;

    public Properties properties;

    private SurfaceRequestListener _surfaceRequestListener;
    private UdpBodiesListener _udpLocalBodiesListener;
    private UdpBodiesListener _udpRemoteBodiesListener;
    private BodiesManager _bodies;

    private GameObject _localOrigin = null;
    public GameObject LocalOrigin { get { return _localOrigin; } }

    private GameObject _remoteOrigin = null;
    public GameObject RemoteOrigin { get { return _remoteOrigin; } }

    private SurfaceRectangle _localSurface = null;
    private SurfaceRectangle _remoteSurface = null;

    public bool __localSurfaceReceived = false;
    public bool __remoteSurfaceReceived = false;
    private bool __everythingIsNiceAndWellConfigured = false;

    private NegativeSpace _negativeSpace;
    private PerspectiveProjection _projection;

    public bool mirrorPessoa = false;

    void Awake()
    {
        Application.runInBackground = true;

        properties = GetComponent<Properties>();
        try
        {
            properties.load(location);
        }
        catch (NegativeRuntimeException e)
        {
            Debug.LogException(e);
            strategicExit();
        }

        _negativeSpace = GetComponent<NegativeSpace>();
        _projection = Camera.main.GetComponent<PerspectiveProjection>();
        _surfaceRequestListener = GetComponent<SurfaceRequestListener>();
        _surfaceRequestListener.StartReceive();
        GetComponent<SurfaceRequest>().request();

        UdpBodiesListener[] udpListeners = GameObject.Find("BodiesManager").GetComponents<UdpBodiesListener>();
        foreach (UdpBodiesListener l in udpListeners)
        {
            if (l.humansType == HumansType.Local)
            {
                _udpLocalBodiesListener = l;
            }
            if (l.humansType == HumansType.Remote)
            {
                _udpRemoteBodiesListener = l;
            }
        }
        

        _bodies = GameObject.Find("BodiesManager").GetComponent<BodiesManager>();
        _udpLocalBodiesListener.startListening(int.Parse(properties.localSetupInfo.trackerBroadcastPort));
        _udpRemoteBodiesListener.startListening(int.Parse(properties.remoteSetupInfo.trackerBroadcastPort));
    }

	void Start ()
    {
    }

    void Update ()
    {

        if (__localSurfaceReceived && __remoteSurfaceReceived)
        {
            if (!__everythingIsNiceAndWellConfigured)
            {
                Debug.Log("XXX  " + this.ToString() + ": Creating the negative world!!!!! XXX");

                GameObject localOrigin = new GameObject("LocalOrigin");
                localOrigin.transform.rotation = Quaternion.identity;
                localOrigin.transform.position = Vector3.zero;

                GameObject remoteOrigin = new GameObject("RemoteOrigin");
                remoteOrigin.transform.rotation = Quaternion.identity;
                remoteOrigin.transform.position = Vector3.zero;

                GameObject localScreenCenter = new GameObject("localScreenCenter");
                localScreenCenter.transform.position = _localSurface.Center;
                localScreenCenter.transform.rotation = _localSurface.Perpendicular;

                Vector3 BLp = _calculateRemoteProxy(_localSurface.SurfaceBottomLeft, localScreenCenter, properties.negativeSpaceLength);
                Vector3 BRp = _calculateRemoteProxy(_localSurface.SurfaceBottomRight, localScreenCenter, properties.negativeSpaceLength);
                Vector3 TRp = _calculateRemoteProxy(_localSurface.SurfaceTopRight, localScreenCenter, properties.negativeSpaceLength);
                Vector3 TLp = _calculateRemoteProxy(_localSurface.SurfaceTopLeft, localScreenCenter, properties.negativeSpaceLength);

                SurfaceRectangle remoteSurfaceProxy = new SurfaceRectangle(BLp, BRp, TLp, TRp);

                /*GameObject lbl = new GameObject("lbl");
                lbl.transform.position = _localSurface.SurfaceBottomLeft;
                lbl.transform.rotation = _localSurface.Perpendicular;
                GameObject lbr = new GameObject("lbr");
                lbr.transform.position = _localSurface.SurfaceBottomRight;
                lbr.transform.rotation = _localSurface.Perpendicular;
                GameObject ltr = new GameObject("ltr");
                ltr.transform.position = _localSurface.SurfaceTopRight;
                ltr.transform.rotation = _localSurface.Perpendicular;
                GameObject rbl = new GameObject("rbl");
                rbl.transform.position = _remoteSurface.SurfaceBottomLeft;
                rbl.transform.rotation = _remoteSurface.Perpendicular;
                GameObject rbr = new GameObject("rbr");
                rbr.transform.position = _remoteSurface.SurfaceBottomRight;
                rbr.transform.rotation = _remoteSurface.Perpendicular;
                GameObject rtr = new GameObject("rtr");
                rtr.transform.position = _remoteSurface.SurfaceTopRight;
                rtr.transform.rotation = _remoteSurface.Perpendicular;
                */

                GameObject remoteScreenCenter = new GameObject("remoteScreenCenter");
                remoteScreenCenter.transform.position = _remoteSurface.Center;
                remoteScreenCenter.transform.rotation = _remoteSurface.Perpendicular;

                localOrigin.transform.parent = localScreenCenter.transform;
                remoteOrigin.transform.parent = remoteScreenCenter.transform;
                remoteScreenCenter.transform.position = localScreenCenter.transform.position;
                remoteScreenCenter.transform.rotation = Quaternion.LookRotation(-localScreenCenter.transform.forward, localScreenCenter.transform.up);

                remoteScreenCenter.transform.position = remoteSurfaceProxy.Center;

                _localSurface.CenterGameObject = localScreenCenter;
                _remoteSurface.CenterGameObject = remoteScreenCenter;

                _localOrigin = localOrigin;
                _remoteOrigin = remoteOrigin;

                //Transform mist = GameObject.Find("mist").transform;
                //mist.position = new Vector3(0, _remoteOrigin.transform.position.y - mist.localScale.x, 0);

                foreach (Sensor sensor in _localSurface.sensors)
                {
                    GameObject g = new GameObject(sensor.id);
                    g.transform.parent = _localOrigin.transform;
                    g.transform.localPosition = sensor.position;
                    g.transform.localRotation = sensor.rotation;
                    
                }

                foreach (Sensor sensor in _remoteSurface.sensors)
                {
                    GameObject g = new GameObject(sensor.id);
                    g.transform.parent = _remoteOrigin.transform;
                    g.transform.localPosition = sensor.position;
                    g.transform.localRotation = sensor.rotation;
                }

                _negativeSpace.create(location, _localSurface, remoteSurfaceProxy, properties.negativeSpaceLength);

                _projection.init(_localSurface);


                __everythingIsNiceAndWellConfigured = true;

                GameObject.Find("EvaluationClient").GetComponent<EvaluationClient>().Init();
                //GameObject.Find("Projector").GetComponent<SimpleProjector>().init(remoteScreenCenter.transform);



                GameObject.Find("WhackAMole").GetComponent<WhackAMole>().Init();

                GameObject localbody = GameObject.Find("LocalBody");
                localbody.transform.parent = _localOrigin.transform;
                localbody.transform.localPosition = Vector3.zero;
                localbody.transform.localRotation = Quaternion.identity;

                GameObject remoteBody = GameObject.Find("RemoteBody");
                remoteBody.transform.parent = _remoteOrigin.transform;
                remoteBody.transform.localPosition = Vector3.zero;
                remoteBody.transform.localRotation = Quaternion.identity;

                GameObject.Find("RavatarManager").GetComponent<TcpDepthListener>().Init(int.Parse(properties.remoteSetupInfo.avatarListenPort));

                GameObject.Find("RavatarManager").GetComponent<Tracker>().Init(int.Parse(properties.remoteSetupInfo.avatarListenPort), int.Parse(properties.remoteSetupInfo.trackerListenPort));
            }
        }
	}

    private static void strategicExit()
    {
        if (Application.isEditor)
        {
            Debug.Break();
        }
    }

    public void setLocalSurface(SurfaceRectangle surfaceRectangle)
    {
        Debug.Log("Received Local Surface: " + surfaceRectangle.ToString());
        _localSurface = surfaceRectangle;
        __localSurfaceReceived = true;
    }

    public void setRemoteSurface(SurfaceRectangle surfaceRectangle)
    {
        Debug.Log("Received Remote Surface: " + surfaceRectangle.ToString());
        _remoteSurface = surfaceRectangle;
        __remoteSurfaceReceived = true;
    }

    private Vector3 _calculateRemoteProxy(Vector3 point, GameObject localScreenCenter, float negativeSpaceLength)
    {
        return point + negativeSpaceLength * localScreenCenter.transform.forward;
    }
}
