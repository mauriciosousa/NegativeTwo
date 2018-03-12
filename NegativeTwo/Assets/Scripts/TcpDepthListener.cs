﻿using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using System;

public class DepthStream
{
    private TcpClient _client;
    internal string name;
    internal byte[] colorData;
    internal byte[] depthData;

    internal int sizec;
    internal int sized;

    internal uint lastID;
    internal bool dirty;
    public int BUFFER = 868352;
    public int DBUFFER = 868352;
    public bool compressed;
    public int scale;

    public DepthStream(TcpClient client)
    {
        compressed = false;
        name = "Depth Kinect Stream";
        _client = client;
        colorData = new byte[BUFFER];
        depthData = new byte[DBUFFER];
        dirty = false;
        scale = 1;
    }

    public void stopStream()
    {
        _client.Close();
    }
}


public class TcpDepthListener : MonoBehaviour
{


    public bool showNetworkDetails = true;

    private int TcpListeningPort;
    private TcpListener _server;

    private bool _running;

    private List<DepthStream> _depthStreams;
    private object myLock;
    



    public void Init(int tcpListenPort)
    {
        myLock = new object();
        //_threads = new List<Thread>();


        _depthStreams = new List<DepthStream>();

        TcpListeningPort = tcpListenPort;
        _server = new TcpListener(IPAddress.Any, TcpListeningPort);

        _running = true;
        _server.Start();
        Debug.Log("Accepting clients at port " + TcpListeningPort);
        Thread acceptLoop = new Thread(new ParameterizedThreadStart(AcceptClients));
        //_threads.Add(acceptLoop);
        acceptLoop.Start();
    }

    void AcceptClients(object o)
    {

        while (_running)
        {
            TcpClient newclient = _server.AcceptTcpClient();
            Thread clientThread = new Thread(new ParameterizedThreadStart(clientHandler));
            //_threads.Add(clientThread);
            clientThread.Start(newclient);
        }
    }


    void clientHandler(object o)
    {
        byte[] _buffer = new byte[868352];
        byte[] _dbuffer = new byte[868352];
        int SIZEHELLO = 200;
        TcpClient client = (TcpClient)o;
        DepthStream kstream = new DepthStream(client);

        _depthStreams.Add(kstream);

        using (NetworkStream ns = client.GetStream())
        {

            byte[] message = new byte[SIZEHELLO];
            int bytesRead = 0;
            try
            {
                bytesRead = ns.Read(message, 0, SIZEHELLO);
            }
            catch
            {
                Debug.Log("Connection Lost from " + kstream.name);
                client.Close();
                _depthStreams.Remove(kstream); ;
            }

            if (bytesRead == 0)
            {
                Debug.Log("Connection Lost from " + kstream.name);
                client.Close();
                _depthStreams.Remove(kstream); ;
            }

            //Login
            string s = System.Text.Encoding.Default.GetString(message);
            string[] l = s.Split('/');

            if (l.Length == 3 && l[0] == "k")
            {
                kstream.name = l[1];
                Debug.Log("New stream from " + l[1]);
            }


            bool colorFrame = false;
            while (_running)
            {
                try
                {
                    bytesRead = ns.Read(message, 0, 13);
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                   // _running = false;
                    break;
                }
                if (bytesRead == 0)
                {
                  //  _running = false;
                    break;
                }

                byte[] idb = { message[0], message[1], message[2], message[3] };
                uint id = BitConverter.ToUInt32(idb, 0);
                kstream.lastID = id;
                byte[] sizeb = { message[4], message[5], message[6], message[7] };
                int size = BitConverter.ToInt32(sizeb, 0);
                if(colorFrame)
                    kstream.sizec = size;
                else
                    kstream.sized = size;

                if (message[8] == 1)
                {
                    kstream.compressed = true;
                }
                else
                {
                    kstream.compressed = false;
                }

                byte[] sc = { message[9], message[10], message[11], message[12] };
                int scale = BitConverter.ToInt32(sc, 0);
                while (size > 0)
                {
                    try
                    {   
                        if(colorFrame)
                            bytesRead = ns.Read(_buffer, 0, size);
                        else
                            bytesRead = ns.Read(_dbuffer, 0, size);
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message + "color frame " + colorFrame + " size " + size);
                        //_running = false;
                        break;
                    }
                    if (bytesRead == 0)
                    {
                       // _running = false;
                        break;
                    }
                    //save because can't update from outside main thread
                    if (colorFrame) {
                        lock (myLock) {
                            Array.Copy(_buffer, 0, kstream.colorData, kstream.sizec - size, bytesRead);
                        }
                    }
                    else {
                        lock (myLock) { 
                            Array.Copy(_dbuffer, 0, kstream.depthData, kstream.sized - size, bytesRead);
                        }
                    }

                    size -= bytesRead;
                }
                if (colorFrame) {
                    kstream.scale = scale;
                    kstream.dirty = true;
                }
                colorFrame = !colorFrame;
            }
        }

    }

    private int convert2BytesToInt(byte b1, byte b2)
    {
        return (int)b1 + (int)(b2 * 256);
    }

    public void closeTcpConnections()
    {
        foreach (DepthStream ks in _depthStreams)
        {
            ks.stopStream();
        }
        _depthStreams = new List<DepthStream>();
    }

    void OnApplicationQuit()
    {
        _running = false;
        closeTcpConnections();
    }

    void Update()
    {
        if (_depthStreams == null) return;

        foreach (DepthStream k in _depthStreams)
        {
            lock (myLock) { 
                if (k.dirty)
                {
                    k.dirty = false;
                    gameObject.GetComponent<Tracker>().setNewDepthCloud(k.name, k.colorData,k.depthData, k.lastID,k.compressed,k.sizec,k.scale);
                }
            }
        }
    }

    void OnQuit()
    {
        OnApplicationQuit();
    }
}
