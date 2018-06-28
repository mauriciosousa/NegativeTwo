using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EvaluationPeer
{
    SERVER,
    LEFT_VR_CLIENT,
    RIGHT_VR_CLIENT
}

public enum EvaluationTask
{
    POLE_T1_C1,
    POLE_T2_C1,
    POLE_T1_C2,
    POLE_T2_C2,
    WALL_C1,
    WALL_C2
}

public enum WarpingCondition
{
    BASELINE = 1,
    WARPING = 2
}

public class DeixisEvaluation : MonoBehaviour {

    public EvaluationPeer peer;

    private EvaluationConfigProperties _config;
    private ServerConsole _console;
    private DeixisNetwork _network;


    public Pole pole;
    public Wall wall;


    public WarpingCondition condition = WarpingCondition.BASELINE;
    public int trial = 0;

    private BodiesManager _bodies;

    public Human leftHuman
    {
        get
        {
            return _bodies.LeftHuman;
        }
    }

    public Human rightHuman
    {
        get
        {
            return _bodies.RightHuman;
        }
    }

    public GUIStyle titleFontStyle;
    public GUIStyle conditionFontStyle;
    private GUIStyle redBox;
    private GUIStyle greenBox;
    public Texture backIcon;
    public Texture forwardIcon;

    void Start () {
        _config = GetComponent<EvaluationConfigProperties>();
        peer = _config.Peer;
        _bodies = GameObject.Find("BodiesManager").GetComponent<BodiesManager>();
        _network = GetComponent<DeixisNetwork>();
        _console = GetComponent<ServerConsole>();

        redBox = new GUIStyle();
        redBox.normal.background = MakeTex(2, 2, new Color(255f / 255f, 59f / 255f, 48f / 255f));
        greenBox = new GUIStyle();
        greenBox.normal.background = MakeTex(2, 2, new Color(76f / 255f, 217f / 255f, 100f / 255f));
    }

    void Update () {
		
	}

    internal void start(int trial, WarpingCondition condition)
    {
        this.trial = trial;
        this.condition = condition;
    }

    internal void end()
    {
        trial = 0;
    }

    void OnGUI()
    {
        if (peer != EvaluationPeer.SERVER) return;

        int width = (int)Math.Max(Math.Ceiling(Screen.width / 3.5f), 300);
        int left = Screen.width - width;
        int top = 0;
        int lineHeight = 25;

        GUI.Box(new Rect(left, top, width, Screen.height), "");

        // CALIBRATION
        top += 10; left += 10;
        GUI.Label(new Rect(left, top, width, lineHeight), "Calibration:", titleFontStyle);

        top += 2 * lineHeight;
        left = Screen.width - width;

        if (GUI.Button(new Rect(left + 5, top, width / 2 - 10, 1.5f * lineHeight), "Human LEFT"))
        {
            _setupHumans(TypeOfHuman.LeftHuman);
        }
        GUI.Box(new Rect(left + 5, top + 1.5f * lineHeight, width / 2 - 10, 5), "", (leftHuman != null) ? greenBox : redBox);

        if (GUI.Button(new Rect(left + width / 2, top, width / 2 - 5, 1.5f * lineHeight), "Human RIGHT"))
        {
            _setupHumans(TypeOfHuman.RightHuman);
        }
        GUI.Box(new Rect(left + width / 2, top + 1.5f * lineHeight, width / 2 - 5, 5), "", (rightHuman != null) ? greenBox : redBox);

        // CONDITION
        top += 4 * lineHeight; left += 10;
        GUI.Label(new Rect(left, top, width, lineHeight), "Condition:", titleFontStyle);
        top += 2 * lineHeight; left += 10;

        if ((trial < 1 || trial > 8) && GUI.Button(new Rect(Screen.width - width + 10, top, 40, 40), backIcon, GUIStyle.none))
        {
            _swapCondition();
        }
        left += 10;

        //GUI.Label(new Rect(Screen.width - width / 2 - 100, top - 5, 200, 2 * lineHeight), condition.ToString().Replace('_', ' '), conditionFontStyle);
        string cond = condition == WarpingCondition.WARPING ? "WARP" : "BASELINE";
        GUI.Label(new Rect(Screen.width - width / 2 - 100, top - 5, 200, 2 * lineHeight), cond, conditionFontStyle);


        if ((trial < 1 || trial > 8) && GUI.Button(new Rect(Screen.width - 40 - 10, top, 40, 40), forwardIcon, GUIStyle.none))
        {
            _swapCondition();
        }


        // Trial
        left = Screen.width - width;
        top += 4 * lineHeight; left += 10;
        GUI.Label(new Rect(left, top, width, lineHeight), "Trial :", titleFontStyle);
        left += 20;
        GUI.Label(new Rect(left, top, width, lineHeight), trial == 0 ? "None" : "" + trial, conditionFontStyle);

        left = Screen.width - width;
        top += 4 * lineHeight;

        if (trial > 0 && trial < 9)
        {
            if (GUI.Button(new Rect(left + 10, top + 2 * lineHeight, width - 20, 1.5f * lineHeight), "NEXT"))
            {
                if (trial == 9)
                {
                    trial = 0;
                }
                else
                {
                    _network.EndMessage();
                    trial += 1;
                    _setupTrial(trial, condition);
                }
            }
        }
        else
        {
            if (GUI.Button(new Rect(left + 10, top, width - 20, 1.5f * lineHeight), "START"))
            {
                trial = 1;
                _setupTrial(trial, condition);
            }
        }

        left = Screen.width - width;
        if (GUI.Button(new Rect(left + 10, Screen.height - 19, width - 20, 19), "Reset"))
        {
            reset();
            _network.reset();
        }
    }

    public void reset()
    {
        trial = 0;
    }

    private void _setupTrial(int trial, WarpingCondition condition)
    {
        Debug.Log("STARTING: " + trial + " " + condition);

        _network.StartMessage(trial, condition);
    }

    private void _swapCondition()
    {
        if (condition == WarpingCondition.BASELINE)
            condition = WarpingCondition.WARPING;
        else
            condition = WarpingCondition.BASELINE;
    }

    internal void calibrateHumans(EvaluationPosition evaluationPosition)
    {
        if (peer != EvaluationPeer.SERVER)
        {
            Debug.Log("[RPC] OnRPC_calibrateHumans");
            _bodies.calibrateHumans(evaluationPosition);
        }
    }

    private void _setupHumans(TypeOfHuman human)
    {
        string id = "None";

        try
        {
            if (human == TypeOfHuman.LeftHuman)
            {
                id = _bodies.calibrateLeftHuman();
                _network.CalibrateHumans((int)TypeOfHuman.LeftHuman);
            }
            else
            {
                id = _bodies.calibrateRightHuman();
                _network.CalibrateHumans((int)TypeOfHuman.RightHuman);
            }
            _console.writeLineGreen(human.ToString() + " calibrated with ID: " + id);
        }
        catch (Exception e)
        {
            _console.writeLineRed(e.Message);
        }
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; ++i)
        {
            pix[i] = col;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}
