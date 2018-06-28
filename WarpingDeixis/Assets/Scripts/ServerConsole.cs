using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsoleLine
{
    public string line;
    public GUIStyle fontStyle;

    public ConsoleLine(string line, GUIStyle fontStyle)
    {
        this.line = line;
        this.fontStyle = fontStyle;
    } 
}

public class ServerConsole : MonoBehaviour {

    public GUIStyle white;
    public GUIStyle red;
    public GUIStyle green;

    public int consoleLineHeight = 25;
    private int maxLines = 10;

    private List<ConsoleLine> _consoleLines;

    private NetworkCommunication _network;

    private bool __init__ = false;

	void Start () {
        if (!__init__) _init();
	}

    void _init()
    {
        _consoleLines = new List<ConsoleLine>();
        _network = GetComponent<NetworkCommunication>();
        __init__ = true;
    }

	void OnGUI () {
        if (_network.evaluationPeerType == EvaluationPeertype.SERVER)
        {
            maxLines = Screen.height / consoleLineHeight - 1;

            GUI.Box(new Rect(0, 0, Screen.width / 4f, Screen.height), "");

            int top = 0;
            int left = 10;

            int i = _consoleLines.Count < maxLines ? 0 : _consoleLines.Count - maxLines;
            for (; i < _consoleLines.Count; i++)
            {
                top += consoleLineHeight;
                ConsoleLine cl = _consoleLines[i];
                GUI.Label(new Rect(left, top, Screen.width / 2, consoleLineHeight), cl.line, cl.fontStyle);
            }
        }
	}

    private void _writeLine(string line, GUIStyle style)
    {
        if (!__init__) _init();
        if (_network.evaluationPeerType == EvaluationPeertype.SERVER)
            _consoleLines.Add(new ConsoleLine("" + (_consoleLines.Count + 1) + ". " + line, style));
    }

    public void writeLine(string line)
    {
        _writeLine(line, white);
    }

    public void writeLineGreen(string line)
    {
        _writeLine(line, green);
    }

    public void writeLineRed(string line)
    {
        _writeLine(line, red);
    }

}
