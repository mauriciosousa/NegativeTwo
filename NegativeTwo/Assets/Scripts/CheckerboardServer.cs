using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MyExtensions
{
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        System.Random rnd = new System.Random();
        while (n > 1)
        {
            int k = (rnd.Next(0, n) % n);
            n--;
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}

public class CheckerboardServer : MonoBehaviour {

    public int port = 5000;
    private NetworkView _networkView;

    List<string> positions; 
    List<string> cubes;

    
    private int puzzle = 0;

    
    private int condition = 1;

    public GUIStyle HugeStyle;
    public GUIStyle ValueStyle;


    void Start ()
    {
        Application.runInBackground = true;

        _networkView = GetComponent<NetworkView>();

        _getInitialPositions();

        Network.InitializeServer(2, port, false);
    }

    private void _getInitialPositions()
    {
        positions = new List<string>();
        cubes = new List<string>();

        string path = Application.dataPath + "/initPositions.txt";
        if (System.IO.File.Exists(path))
        {
            String[] lines = System.IO.File.ReadAllLines(path);
            foreach (string line in lines)
            {
                if (line.Contains(" ontopof "))
                {
                    string[] s = line.Split(' ');
                    //putObjectOnTopOf(GameObject.Find(s[0]), GameObject.Find(s[2]));
                    cubes.Add(s[0]);
                    positions.Add(s[2]);
                }
            }
        }
        else
        {
            cubes.Add("RedCube");
            cubes.Add("GreenCube");
            cubes.Add("BlueCube");
            cubes.Add("YellowCube");
            cubes.Add("PinkCube");

            positions.Add("box(5,3)");
            positions.Add("box(4,0)");
            positions.Add("box(4,4)");
            positions.Add("box(4,2)");
            positions.Add("box(5,1)");
        }

        cubes.Shuffle();
        positions.Shuffle();

        for (int i = 0; i < cubes.Count; i++)
        {
            print(cubes[i] + " " + positions[i]);
        }
    }

    void Update ()
    {
        	
	}

    void OnGUI()
    {
        int left = 10;
        int top = 10;
        int lineSize = 30;

        GUI.Label(new Rect(left, top, 50, lineSize), "Puzzle: ", HugeStyle); left += 160;
        GUI.Label(new Rect(left, top, 50, lineSize), "" + puzzle, ValueStyle);

        left = 10; top += 30;
        GUI.Label(new Rect(left, top, 50, lineSize), "Condition: ", HugeStyle); left += 160;
        GUI.Label(new Rect(left, top, 50, lineSize), "" + condition, ValueStyle);


        left = 10; top += 3 * 30;
        GUI.Label(new Rect(left, top, 50, lineSize), "Choose Condition: ", HugeStyle); left += 160;

        left = 10; top += 30;
        if (GUI.Button(new Rect(left, top, 40, 40), "1"))
        {
            condition = 1;
        }
        left += 40;
        if (GUI.Button(new Rect(left, top, 40, 40), "2"))
        {
            condition = 2;
        }
        left += 40;
        if (GUI.Button(new Rect(left, top, 40, 40), "3"))
        {
            condition = 3;
        }
        left += 40;
        if (GUI.Button(new Rect(left, top, 40, 40), "4"))
        {
            condition = 4;
        }
        left += 40;


        left = 10; top += 2 * 30;
        GUI.Label(new Rect(left, top, 50, lineSize), "Choose Puzzle: ", HugeStyle); left += 160;

        left = 10; top += 30;
        if (GUI.Button(new Rect(left, top, 40, 40), "0"))
        {
            puzzle = 0;
        }
        left += 40;
        if (GUI.Button(new Rect(left, top, 40, 40), "1"))
        {
            puzzle = 1;
        }
        left += 40;
        if (GUI.Button(new Rect(left, top, 40, 40), "2"))
        {
            puzzle = 2;
        }
        left += 40;
        if (GUI.Button(new Rect(left, top, 40, 40), "3"))
        {
            puzzle = 3;
        }
        left += 40;
        if (GUI.Button(new Rect(left, top, 40, 40), "4"))
        {
            puzzle = 4;
        }
        left += 40;
        if (GUI.Button(new Rect(left, top, 40, 40), "5"))
        {
            puzzle = 5;
        }
        left += 40;
        if (GUI.Button(new Rect(left, top, 40, 40), "6"))
        {
            puzzle = 6;
        }
        left += 40;
        if (GUI.Button(new Rect(left, top, 40, 40), "7"))
        {
            puzzle = 7;
        }
        left += 40;
        if (GUI.Button(new Rect(left, top, 40, 40), "8"))
        {
            puzzle = 8;
        }
        left += 40;



        int newLeft = Screen.width / 2 + 50;
        left = newLeft;
        top = 10;

        GUI.Label(new Rect(left, top, 50, lineSize), "Server Running: ", HugeStyle); left += 160;
        GUI.Label(new Rect(left, top, 50, lineSize), "" + Network.peerType, ValueStyle);

        left = newLeft; top += 30;
        GUI.Label(new Rect(left, top, 50, lineSize), "#Clients: ", HugeStyle); left += 160;
        GUI.Label(new Rect(left, top, 50, lineSize), "" + Network.connections.Length, ValueStyle);

        left = newLeft; top += 2 * 30;
        GUI.Label(new Rect(left, top, 50, lineSize), "Actions: ", HugeStyle);

        top += 30;
        /*
        if (GUI.Button(new Rect(left, top, 200, 40), "Ping"))
        {
            _networkView.RPC("RPC_receiveMessage", RPCMode.Others, "Ping");
        }
        top += 45;
        if (GUI.Button(new Rect(left, top, 200, 40), "Set Initial Position"))
        {
            for (int i = 0; i < cubes.Count; i++)
            {
                //print(cubes[i] + " " + positions[i]);
                _networkView.RPC("RPC_putObjectOnTopOf_Init", RPCMode.Others, cubes[i], positions[i]);
            }
        }*/
        top += 45;
        if (GUI.Button(new Rect(left, top, 200, 40), "Start"))
        {
            Debug.Log("Condition={" + condition + "}, puzzle={" + puzzle + "}");
            _networkView.RPC("RPC_Start", RPCMode.Others, condition, puzzle);
        }

        top += 45;
        if (GUI.Button(new Rect(left, top, 200, 40), "Show/Hide Screen"))
        {
            _networkView.RPC("RPC_ShowHide", RPCMode.Others);
        }



    }

    #region RPC Calls
    [RPC]
    void RPC_ShowHide()
    {
        
    }

    [RPC]
    void RPC_Start(int condition, int puzzle)
    {

    }

    [RPC]
    void RPC_UpdateCube(string cube, string status)
    {

    }

    [RPC]
    void RPC_receiveMessage(string s)
    {
        Debug.Log(this.ToString() + ": " + s);
    }

    [RPC]
    void RPC_putObjectOnTopOf(string that, string there)
    {

    }
    [RPC]
    void RPC_putObjectOnTopOf_Init(string that, string there)
    {

    }
    #endregion
}
