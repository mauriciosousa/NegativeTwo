using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;


public class Tracker : MonoBehaviour
{

	private Dictionary<string, PointCloudDepth> _clouds;
    private Dictionary<string, GameObject> _cloudGameObjects;

    private int _listenPort;
    private int _trackerPort;

    public void Init (int listenPort, int trackerPort)
	{
        Debug.Log("Hello Tracker");
		_clouds = new Dictionary<string, PointCloudDepth> ();
        _cloudGameObjects = new Dictionary<string, GameObject>();

        _listenPort = listenPort;
        _trackerPort = trackerPort;
        resetListening();

        UdpClient udp = new UdpClient();
        string message = AvatarMessage.createRequestMessage(1, listenPort);
        byte[] data = Encoding.UTF8.GetBytes(message);
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Broadcast, trackerPort);
        Debug.Log("Sent request to port" + trackerPort + " with content " + message); 
        udp.Send(data, data.Length, remoteEndPoint);

        
    }



    //FOR TCP DEPTH
    internal void setNewDepthCloud(string KinectID, byte[] colorData, byte[] depthData, uint id,bool compressed,int sizec,int scale)
    {
       
        // tirar o id da mensagem que é um int
        if (_clouds.ContainsKey(KinectID))
        {
            _clouds[KinectID].setPoints(colorData,depthData,compressed, sizec,scale);
            _clouds[KinectID].show();
        }
    }
    
	public void resetListening ()
	{
		gameObject.GetComponent<UdpListener> ().udpRestart (_listenPort);
	}

	public void hideAllClouds ()
	{
		foreach (PointCloudDepth s in _clouds.Values) {
			s.hide ();
		}
		UdpClient udp = new UdpClient ();
		string message = CloudMessage.createRequestMessage (2,Network.player.ipAddress, _listenPort); 
		byte[] data = Encoding.UTF8.GetBytes(message);
		IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Broadcast, _listenPort + 1);
		udp.Send(data, data.Length, remoteEndPoint);
	}


    public void processAvatarMessage(AvatarMessage av)
    {
        foreach (string s in av.calibrations)
        {
            //if (s == "") continue;
            string[] chunks = s.Split(';');
            string id = chunks[0];
            float px = float.Parse(chunks[1]);
            float py = float.Parse(chunks[2]);
            float pz = float.Parse(chunks[3]);
            float rx = float.Parse(chunks[4]);
            float ry = float.Parse(chunks[5]);
            float rz = float.Parse(chunks[6]);
            float rw = float.Parse(chunks[7]);

            GameObject cloudobj = new GameObject(id);
         

            cloudobj.transform.localPosition = new Vector3(px,py,pz);
            cloudobj.transform.localRotation = new Quaternion(rx,ry,rz,rw);
            cloudobj.transform.localScale = new Vector3(-1, 1, 1);
            cloudobj.AddComponent<PointCloudDepth>();
            PointCloudDepth cloud = cloudobj.GetComponent<PointCloudDepth>();
            _clouds.Add(id, cloud);
            _cloudGameObjects.Add(id, cloudobj);

        }
    }
}
