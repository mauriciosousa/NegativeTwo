﻿using System;
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

public enum PointingExercise
{
    POLE,
    WALL
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
    public int maxTrials = 22;

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
    public GUIStyle instructionStyle;
    private GUIStyle redBox;
    private GUIStyle greenBox;
    public Texture backIcon;
    public Texture forwardIcon;

    void Start() {
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

    void Update() {

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

        if ((trial < 1 || trial > maxTrials) && GUI.Button(new Rect(Screen.width - width + 10, top, 40, 40), backIcon, GUIStyle.none))
        {
            _swapCondition();
        }
        left += 10;

        //GUI.Label(new Rect(Screen.width - width / 2 - 100, top - 5, 200, 2 * lineHeight), condition.ToString().Replace('_', ' '), conditionFontStyle);
        string cond = condition == WarpingCondition.WARPING ? "WARP" : "BASELINE";
        GUI.Label(new Rect(Screen.width - width / 2 - 100, top - 5, 200, 2 * lineHeight), cond, conditionFontStyle);


        if ((trial < 1 || trial > maxTrials) && GUI.Button(new Rect(Screen.width - 40 - 10, top, 40, 40), forwardIcon, GUIStyle.none))
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

        if (trial > 0 && trial <= maxTrials)
        {
            if (GUI.Button(new Rect(left + 10, top + 2 * lineHeight, width - 20, 1.5f * lineHeight), "NEXT"))
            {
                if (trial == maxTrials)
                {
                    trial = 0;
                    reset();
                    _network.reset();
                }
                else
                {
                    _network.EndMessage();
                    pole.destroyCurrent();
                    wall.destroyCurrent();

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
            trial = 0;
        }

        // Instruction
        if (trial > 0 && trial <= maxTrials)
        {
            GUI.Box(new Rect(Screen.width / 3 - 20, 0, (Screen.width / 3) + 30, Screen.height / 2 + 20), " ");
            GUI.Label(new Rect(Screen.width / 3, 10, (Screen.width / 3), Screen.height - 10), _getInstruction(trial), instructionStyle);
        }
    }

    public void reset()
    {
        trial = 0;
        pole.destroyCurrent();
        wall.hideWall();
    }

    private void _setupTrial(int trial, WarpingCondition condition)
    {
        bool observer = peer == _getObserver(trial);
        print("OBSERVER = " + observer);

        Debug.Log("STARTING: " + trial + " " + condition);
        Debug.Log("trial = " + trial + ", Observer = " + _getObserver(trial) + ", Exercise = " + _getExercise(trial) + ", Arm = " + _getArm(trial) + ", distance = " + _getDistance(trial) + "m" + ", target = " + _getPoleTarget(trial));

        if (_getExercise(trial) == PointingExercise.POLE)
        {
            pole.createAPole(trial, _getDistance(trial), _getPoleTarget(trial),observer, condition);
        }
        if (_getExercise(trial) == PointingExercise.WALL)
        {
            wall.createWall(trial, observer);
        }

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

    private EvaluationPeer _getObserver(int trial)
    {
        if (trial <= 11)
        {
            return EvaluationPeer.RIGHT_VR_CLIENT;
        }
        return EvaluationPeer.LEFT_VR_CLIENT;
    }

    private PointingExercise _getExercise(int trial)
    {
        if ((trial >= 1 && trial <= 6) || (trial >= 12 && trial <= 17))
        {
            return PointingExercise.POLE;
        }
        return PointingExercise.WALL;
    }

    private PointingArm _getArm(int trial)
    {
        if (trial % 2 != 0) // is Odd
        {
            return PointingArm.LEFT;
        }
        return PointingArm.RIGHT;
    }

    private int _getDistance(int trial)
    {
        if (new List<int>() {1, 2, 12, 13}.Contains(trial))
        {
            return 1;
        }

        if (new List<int>() { 5, 6, 16, 17 }.Contains(trial))
        {
            return 3;
        }

        return 2;
    }

    private string _getInstruction(int trial)
    {
        return "Pessoa Da Direita, aponta para o alvo e pessoa da Esquerda diz para onde o teu colega está a apontar em voz alta.";
    }

    private int _getPoleTarget(int trial)
    {
        if (trial == 1) return 16;

        if (trial == 2) return 18;

        if (trial == 3) return 11;

        if (trial == 4) return 25;

        if (trial == 5) return 28;

        if (trial == 6) return 21;


        if (trial == 12) return 21;

        if (trial == 13) return 16;

        if (trial == 14) return 25;

        if (trial == 15) return 11;

        if (trial == 16) return 18;

        if (trial == 17) return 28;

        return -1;
    }
}
