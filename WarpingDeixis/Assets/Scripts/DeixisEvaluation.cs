using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EvaluationPeer
{
    SERVER,
    LEFT,
    RIGHT
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
    public PoleLog poleLog;
    public BoxCollider poleCollider;

    public Wall wall;
    public WallLog wallLog;
    public BoxCollider wallCollider;


    public WarpingCondition condition = WarpingCondition.BASELINE;
    private int trial = 0;
    private int maxTrials = 28;

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
        Debug.Log("Start Trial");
        this.trial = trial;
        this.condition = condition;
        _setupTrial(trial, condition);
    }

    internal void end()
    {
        pole.destroyCurrent();
        wall.destroyCurrent();
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
                if (_getExercise(trial) == PointingExercise.POLE)
                {
                    poleLog.Record(trial, condition.ToString(), _getObserver(trial).ToString(), _getPoleTarget(trial, _getObserver(trial), condition));
                }

                if (_getExercise(trial) == PointingExercise.WALL)
                {
                    wallLog.Record(trial, condition.ToString(), _getObserver(trial).ToString(), wall.target.transform.position, wall.cursor.transform.position);
                }

                if (trial == maxTrials)
                {
                    trial = 0;
                    reset();
                    _network.reset();

                    poleLog.EndRecordingSession();
                    wallLog.EndRecordingSession();

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
            GUI.Box(new Rect(Screen.width / 3 - 20, 0, (Screen.width / 3) + 30, 80), " ");
            GUI.Box(new Rect(Screen.width / 3 - 20, 0, (Screen.width / 3) + 30, 80), " ");
            GUI.Box(new Rect(Screen.width / 3 - 20, 0, (Screen.width / 3) + 30, 80), " ");

            GUI.Label(new Rect(Screen.width / 3, 10, (Screen.width / 3), Screen.height - 10), _getInstruction(trial), instructionStyle);
        }
    }

    internal Collider getCollider()
    {
        if (_getExercise(trial) == PointingExercise.POLE)
        {
            return poleCollider;
        }
        return wallCollider;
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

        Debug.Log("STARTING: " + trial + " " + condition);
        //Debug.Log("trial = " + trial + ", Observer = " + _getObserver(trial) + ", Exercise = " + _getExercise(trial) + ", Arm = " + _getArm(trial) + ", distance = " + _getDistance(trial) + "m" + ", target = " + _getPoleTarget(trial, _getObserver(trial), condition));

        if (_getExercise(trial) == PointingExercise.POLE)
        {
            pole.createAPole(trial, _getDistance(trial), _getPoleTarget(trial, _getObserver(trial), condition), observer, condition);

            if (trial == 1 && peer == EvaluationPeer.SERVER) poleLog.StartRecordingSession();
        }
        if (_getExercise(trial) == PointingExercise.WALL)
        {
            wall.createWall(trial, observer, _getObserver(trial), condition);

            if (trial == 10 && peer == EvaluationPeer.SERVER) wallLog.StartRecordingSession();
        }

        if (peer == EvaluationPeer.SERVER) _network.StartMessage(trial, condition);
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
        //if (peer == EvaluationPeer.SERVER) return EvaluationPeer.LEFT_VR_CLIENT;


        if (trial <= 14)
        {
            return EvaluationPeer.LEFT;
        }
        return EvaluationPeer.RIGHT;
    }

    private PointingExercise _getExercise(int trial)
    {
        if ((trial >= 1 && trial <= 9) || (trial >= 15 && trial <= 23))
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
        if (new List<int>() {1, 2, 3, 15, 16, 17}.Contains(trial))
        {
            return 1;
        }

        if (new List<int>() {7, 8, 9, 21, 22, 23}.Contains(trial))
        {
            return 3;
        }

        return 2;
    }

    private string _getInstruction(int trial)
    {
        return "Pessoa Da Direita, aponta para o alvo e pessoa da Esquerda diz para onde o teu colega está a apontar em voz alta.";
    }

    private int _getPoleTarget(int trial, EvaluationPeer observer, WarpingCondition condition)
    {
        Dictionary<int, int> b1 = new Dictionary<int, int>()
        {
            {1, 26}, {2, 13}, {3, 16}, {4, 29}, {5, 21}, {6, 10}, {7, 19}, {8, 8}, {9, 31}
        };

        Dictionary<int, int> b2 = new Dictionary<int, int>()
        {
            {15, 19}, {16, 10}, {17, 29}, {18, 13}, {19, 21}, {20, 26}, {21, 16}, {22, 31}, {23, 8}
        };

        Dictionary<int, int> b3 = new Dictionary<int, int>()
        {
            {1, 13}, {2, 29}, {3, 19}, {4, 16}, {5, 26}, {6, 10}, {7, 31}, {8, 8}, {9, 21}
        };

        Dictionary<int, int> b4 = new Dictionary<int, int>()
        {
            {15, 16}, {16, 8}, {17, 29}, {18, 10}, {19, 26}, {20, 19}, {21, 13}, {22, 31}, {23, 21}
        };

        if (observer == EvaluationPeer.LEFT  && condition == WarpingCondition.BASELINE) return b1[trial];

        if (observer == EvaluationPeer.RIGHT && condition == WarpingCondition.BASELINE) return b2[trial];

        if (observer == EvaluationPeer.LEFT && condition == WarpingCondition.WARPING) return b3[trial];

        if (observer == EvaluationPeer.RIGHT && condition == WarpingCondition.WARPING) return b4[trial];

        return -1;
    }
}
