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

    public PointersLog pointersLog;

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

    public GameObject leftHumanGameObject;
    public GameObject rightHumanGameObject;

    

    public GUIStyle titleFontStyle;
    public GUIStyle conditionFontStyle;
    public GUIStyle instructionStyle;
    private GUIStyle redBox;
    private GUIStyle greenBox;
    public Texture backIcon;
    public Texture forwardIcon;

    public Vector3 pointerHeadIndexHit;
    public Vector3 pointerElbowIndexHit;
    public Transform pointerHeadIndexTransform;
    public Transform pointerElbowIndexTransform;
    public float vDistance = 0f;

    private string observedPoleTarget = "";

    void Start() {
        _config = GetComponent<EvaluationConfigProperties>();
        peer = _config.Peer;
        _bodies = GameObject.Find("BodiesManager").GetComponent<BodiesManager>();
        _network = GetComponent<DeixisNetwork>();
        _console = GetComponent<ServerConsole>();
        pointersLog = GetComponent<PointersLog>();

        redBox = new GUIStyle();
        redBox.normal.background = MakeTex(2, 2, new Color(255f / 255f, 59f / 255f, 48f / 255f));
        greenBox = new GUIStyle();
        greenBox.normal.background = MakeTex(2, 2, new Color(76f / 255f, 217f / 255f, 100f / 255f));


        //pointerHeadIndexTransform.gameObject.GetComponent<Renderer>().enabled = peer == EvaluationPeer.SERVER;
        //pointerElbowIndexTransform.gameObject.GetComponent<Renderer>().enabled = peer == EvaluationPeer.SERVER;

    }

    void Update() {

        pointerHeadIndexHit = Vector3.zero;
        pointerElbowIndexHit = Vector3.zero;

        if (leftHuman != null && rightHuman != null && trial > 0)
        {
            EvaluationPeer pointer = getObserver(trial) == EvaluationPeer.LEFT ? EvaluationPeer.LEFT : EvaluationPeer.RIGHT;
            GameObject pointerGO = pointer == EvaluationPeer.LEFT ? leftHumanGameObject : rightHumanGameObject;

            Vector3 head_index_hitpoint = Vector3.zero;
            PointingArm pointingArm = getPointingArm(pointer, out head_index_hitpoint);

            if (pointingArm != PointingArm.BOTH && pointingArm != PointingArm.NONE)
            {

                Vector3 elbow_index_hitpoint = Vector3.zero;
                Vector3 elbow;
                Vector3 index;

                if (pointingArm == PointingArm.LEFT)
                {
                    elbow = pointerGO.transform.Find(BodyJointType.leftElbow.ToString()).transform.position;
                    index = pointerGO.transform.Find(BodyJointType.leftHandTip.ToString()).transform.position;
                }
                else
                {
                    elbow = pointerGO.transform.Find(BodyJointType.rightElbow.ToString()).transform.position;
                    index = pointerGO.transform.Find(BodyJointType.rightHandTip.ToString()).transform.position;
                }
             

                Ray ray = new Ray(index, index - elbow);
                RaycastHit hit;
                if (getCollider().Raycast(ray, out hit, 1000.0f))
                {
                    elbow_index_hitpoint = hit.point;
                }

                pointerHeadIndexHit = head_index_hitpoint;
                pointerElbowIndexHit = elbow_index_hitpoint;

                Vector3 target = Vector3.zero;

                if (_getExercise(trial) == PointingExercise.POLE)
                {
                    int t = _getPoleTarget(trial, getObserver(trial), condition);
                    target = GameObject.Find("pole" + t).transform.position;
                }
                else
                {
                    target = GameObject.Find("TARGET").transform.position;
                }


                pointersLog.Record(trial, condition.ToString(), pointingArm,
                    pointerGO.transform.Find(BodyJointType.head.ToString()).transform.position, elbow, index, pointerHeadIndexHit, pointerElbowIndexHit, Vector3.Distance(pointerHeadIndexHit, target), Vector3.Distance(pointerElbowIndexHit, target));
            }
            else
            {
                pointersLog.Record(trial, condition.ToString(), pointingArm,
                    Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, 0f, 0f);
            }

            pointerHeadIndexTransform.position = pointerHeadIndexHit;
            pointerElbowIndexTransform.position = pointerElbowIndexHit;
            
            vDistance = Vector3.Distance(pointerHeadIndexHit, pointerElbowIndexHit);

            
        }
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



    public bool isHumanPointing(EvaluationPeer peer, PointingArm arm, out Vector3 hitpoint)
    {
        hitpoint = Vector3.zero;

        GameObject humanGO;
        if (peer == EvaluationPeer.LEFT)
        {
            if (leftHuman == null) return false;

            humanGO = leftHumanGameObject;
        }
        else
        {
            if (rightHuman == null) return false;

            humanGO = rightHumanGameObject;
        }

        Vector3 head = humanGO.transform.Find(BodyJointType.head.ToString()).position;
        Vector3 tip;
        if (arm == PointingArm.LEFT)
        {
            tip = humanGO.transform.Find(BodyJointType.leftHandTip.ToString()).position;
        }
        else
        {
            tip = humanGO.transform.Find(BodyJointType.rightHandTip.ToString()).position;
        }

        Ray ray = new Ray(tip, tip - head);
        RaycastHit hit;
        if (getCollider().Raycast(ray, out hit, 1000.0f))
        {
            hitpoint = hit.point;
            return true;
        }

        return false;
    }

    public PointingArm getPointingArm(EvaluationPeer peer, out Vector3 hitpoint)
    {
        hitpoint = Vector3.zero;
        Vector3 leftHit = hitpoint;
        Vector3 rightHit = hitpoint;

        bool left = isHumanPointing(peer, PointingArm.LEFT, out leftHit);
        bool right = isHumanPointing(peer, PointingArm.RIGHT, out rightHit);

        if (left && right)
            return PointingArm.BOTH;

        if (left)
        {
            hitpoint = leftHit;
            return PointingArm.LEFT;
        }

        if (right)
        {
            hitpoint = rightHit;
            return PointingArm.RIGHT;
        }

        return PointingArm.NONE;
    }

    private bool _isPointing(Ray ray, out Vector3 hitpoint)
    {
        hitpoint = Vector3.zero;
        RaycastHit hit;
        if (getCollider().Raycast(ray, out hit, 1000.0f))
        {
            hitpoint = hit.point;
            return true;
        }

        return false;
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

        // pointing data

        Vector3 hit;
        bool leftHumanLeftHandPointing = isHumanPointing(EvaluationPeer.LEFT, PointingArm.LEFT, out hit);
        bool leftHumanRightHandPointing = isHumanPointing(EvaluationPeer.LEFT, PointingArm.RIGHT, out hit);
        bool rightHumanLeftHandPointing = isHumanPointing(EvaluationPeer.RIGHT, PointingArm.LEFT, out hit);
        bool rightHumanRightHandPointing = isHumanPointing(EvaluationPeer.RIGHT, PointingArm.RIGHT, out hit);

        top += 6;
        GUI.Box(new Rect(left + 5, top + 1.5f * lineHeight, width / 4 - 5, 5), "", leftHumanLeftHandPointing ? greenBox : redBox);
        GUI.Box(new Rect(left + width / 4, top + 1.5f * lineHeight, width / 4 - 5, 5), "", leftHumanRightHandPointing ? greenBox : redBox);
        GUI.Box(new Rect(left + width / 2, top + 1.5f * lineHeight, width / 4 - 5, 5), "", rightHumanLeftHandPointing ? greenBox : redBox);
        GUI.Box(new Rect(left + width / 2 + width / 4 - 5, top + 1.5f * lineHeight, width / 4, 5), "", rightHumanRightHandPointing ? greenBox : redBox);


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
            bool canShowNEXT = true;
            if (_getExercise(trial) == PointingExercise.POLE)
            {
                observedPoleTarget = GUI.TextField(new Rect(left + 10, top + 2 * lineHeight, 50, lineHeight), observedPoleTarget);
                top += 2 * lineHeight;

                int oTarget = 0;
                bool parsed = int.TryParse(observedPoleTarget, out oTarget);
                canShowNEXT = parsed && oTarget > 0 && oTarget < 38;
            }

            if (canShowNEXT)
            {
                if (GUI.Button(new Rect(left + 10, top + 2 * lineHeight, width - 20, 1.5f * lineHeight), "NEXT"))
                {

                    if (peer == EvaluationPeer.SERVER)
                    {
                        pointersLog.EndRecordingSession();
                    }

                    if (_getExercise(trial) == PointingExercise.POLE)
                    {
                        poleLog.Record(trial, condition.ToString(), getObserver(trial).ToString(), _getPoleTarget(trial, getObserver(trial), condition), PoleNumbersDics.revert(int.Parse(observedPoleTarget), trial, condition));
                        observedPoleTarget = "";
                    }

                    if (_getExercise(trial) == PointingExercise.WALL)
                    {
                        wallLog.Record(trial, condition.ToString(), getObserver(trial).ToString(), wall.target.transform.position, wall.cursor.transform.position);
                    }

                    if (trial == maxTrials)
                    {
                        trial = 0;
                        reset();
                        _network.reset();

                        poleLog.EndRecordingSession();
                        wallLog.EndRecordingSession();

                        _console.writeLineRed("------------------------------------------------------------------------");
                        _console.writeLineRed("   condition " + condition + ": DONE");
                        _console.writeLineRed("------------------------------------------------------------------------");
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
        bool observer = peer == getObserver(trial);

        Debug.Log("STARTING: " + trial + " " + condition);
        //Debug.Log("trial = " + trial + ", Observer = " + _getObserver(trial) + ", Exercise = " + _getExercise(trial) + ", Arm = " + _getArm(trial) + ", distance = " + _getDistance(trial) + "m" + ", target = " + _getPoleTarget(trial, _getObserver(trial), condition));

        if (peer == EvaluationPeer.SERVER)
        {
            _console.writeLineGreen("t = " + trial + ", Observer is " + getObserver(trial));

            pointersLog.StartRecordingSession(trial);
        }


        if (_getExercise(trial) == PointingExercise.POLE)
        {
            pole.createAPole(trial, _getDistance(trial), _getPoleTarget(trial, getObserver(trial), condition), observer, condition);

            if (trial == 1 && peer == EvaluationPeer.SERVER)
            {
                poleLog.StartRecordingSession();
            }
        }
        if (_getExercise(trial) == PointingExercise.WALL)
        {
            wall.createWall(trial, observer, getObserver(trial), condition);

            if (trial == 10 && peer == EvaluationPeer.SERVER)
            {
                wallLog.StartRecordingSession();
            }
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

    public EvaluationPeer getObserver(int trial)
    {
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
        if (new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 }.Contains(trial))
        {
            return "<Nome da Direita>, aponta para o alvo no poste.\n\n<Nome da Esquerda>, diz-me em voz alta o numero para onde o teu colega está a apontar.";
        }

        if (new List<int>() { 15, 16,17,18,19,20,21,22,23 }.Contains(trial))
        {
            return "<Nome da Esquerda>, aponta para o alvo no poste.\n\n<Nome da Direita>, diz-me em voz alta o numero para onde o teu colega está a apontar.";
        }

        if (new List<int>() { 10, 11, 12, 13, 14 }.Contains(trial))
        {
            return "<Nome da Direita>, aponta para o alvo na parede.\n\n<Nome da Esquerda>, com o cursor, indica para onde o teu colega está a apontar.";
        }

        if (new List<int>() { 24, 25, 26, 27, 28 }.Contains(trial))
        {
            return "<Nome da Direita>, aponta para o alvo na parede.\n\n<Nome da Esquerda>, com o cursor, indica para onde o teu colega está a apontar.";
        }




        return "lol";
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
