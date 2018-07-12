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

    public Properties properties;
    
    private UdpBodiesListener _udpLocalBodiesListener;
    private BodiesManager _bodies;
    public bool correctingPointing = true;

    //public PointingDistortionInfo rightPointingInfo;
    //public PointingDistortionInfo leftPointingInfo;

    public bool mirrorPessoa = false;
    public float headSize;
    Vector3 lastRightHandPos;
    Vector3 lastLeftHandPos;
    Vector3 remoteLeftHit;
    Vector3 remoteRightHit;
    public float maxHandVelocity = 1.0f;


    public EvaluationConfigProperties config;

    public Vector3 getLocalHead()
    {
        return _bodies.getLocalHead();
    }

    public int remoteHead()
    {
        return _bodies.remoteHead();
    }

    void Awake()
    {
        Application.runInBackground = true;

        properties = GetComponent<Properties>();
        try
        {
            properties.load();
        }
        catch (NegativeRuntimeException e)
        {
            Debug.LogException(e);
            strategicExit();
        }

        UdpBodiesListener[] udpListeners = GameObject.Find("BodiesManager").GetComponents<UdpBodiesListener>();
        foreach (UdpBodiesListener l in udpListeners)
        {
            if (l.humansType == HumansType.Local)
            {
                _udpLocalBodiesListener = l;
            }
        }

        _bodies = GameObject.Find("BodiesManager").GetComponent<BodiesManager>();
        _udpLocalBodiesListener.startListening(int.Parse(properties.info.trackerBroadcastPort));


		//if (GameObject.Find ("DeixisEvaluation").GetComponent<EvaluationConfigProperties> ().Peer != EvaluationPeer.SERVER) {
        
        if (config.Peer != EvaluationPeer.SERVER)
        {
			GameObject.Find("RavatarManager").GetComponent<TcpDepthListener>().Init(int.Parse(properties.info.avatarListenPort));
			GameObject.Find("RavatarManager").GetComponent<Tracker>().Init(int.Parse(properties.info.avatarListenPort), int.Parse(properties.info.trackerListenPort));
		}
	
	}

	void Start ()
    {
    }

    private static void strategicExit()
    {
        if (Application.isEditor)
        {
            Debug.Break();
        }
    }
}
