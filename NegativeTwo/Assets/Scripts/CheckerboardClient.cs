using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CheckerboardClient : MonoBehaviour {

    private Main _main;
    private NetworkView _networkView;
    private Checkerboard _board;






    public bool Connected { get { return Network.peerType == NetworkPeerType.Client; } }

	void Start ()
    {
        _main = GameObject.Find("Main").GetComponent<Main>();
        _board = this.GetComponent<Checkerboard>();
        _networkView = GetComponent<NetworkView>();
    }

    public void Init()
    {

        int port = int.Parse(_main.properties.localSetupInfo.rpcPort);
        string address = _main.properties.serverAddress;

        Debug.Log(this.ToString() + "[RPC]: trying to connect to " + address + ":" + port);
        Network.Connect(address, port);
    }

   

    void Update ()
    {
		
	}

    #region RPC Calls

    [RPC]
    void RPC_UpdateCube(string cube, bool selected)
    {
        if (_main.location == Location.Instructor)
        {
            GameObject cubego = GameObject.Find(cube);
            cubego.GetComponent<Highlight>().setSelected(selected);
        }
    }

    [RPC]
    void RPC_receiveMessage(string s)
    {
        Debug.Log(this.ToString() + ": " + s);
    }

    [RPC]
    void RPC_putObjectOnTopOf(string that, string there)
    {
        Debug.Log(that + "   " + there);
        if (_main.location == Location.Instructor)
        {
            Debug.Log(that + "   " + there);
            _board.putObjectOnTopOf(that, there);
        }
    }

    public void putObjectOnTopOf(GameObject that, GameObject there)
    {
        if (_main.location == Location.Assembler)
        {
            _networkView.RPC("RPC_putObjectOnTopOf", RPCMode.Others, that.name, there.name);
        }
    }

    [RPC]
    void RPC_putObjectOnTopOf_Init(string that, string there)
    {
        if (Network.peerType != NetworkPeerType.Server)
        {
            _board.putObjectOnTopOf(that, there);
        }
    }

    internal void callHighlightUpdate(string name, bool highlighted)
    {
        _networkView.RPC("RPC_UpdateCube", RPCMode.Others, name, highlighted);
    }

    [RPC]
    void RPC_Start(int condition, int puzzle)
    {
        if (Network.peerType != NetworkPeerType.Server)
        {
            // TODO: SET CONDITION ANd PUZZLE
            Debug.Log(this.ToString() + ": Start: condition " + condition + ", puzzle " + puzzle);
            if (_main.location == Location.Assembler)
            {
                _board.StartEvaluation(condition, puzzle);
            }
            else
            {
                _main.mirrorPessoa = true;
            }
        }
    }

    public bool hideScreen = false;
    public Texture hideTexture;

    [RPC]
    void RPC_ShowHide()
    {
        if (Network.peerType != NetworkPeerType.Server)
        {
            //GameObject hideScreenGo = GameObject.Find("HideScreen");
            //Hide hideScript = hideScreenGo.GetComponent<Hide>();
            //hideScript.setEnabled(!hideScript.Enabled);
            hideScreen = !hideScreen;
        }
    }
    #endregion

    void OnGUI()
    {
        if (hideScreen)
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), hideTexture);
        }
    }

    internal void showHide()
    {
        _networkView.RPC("RPC_ShowHide", RPCMode.All);
    }
}
