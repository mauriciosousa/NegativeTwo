using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EvaluationCondition
{
    NONE = 0,
    NO_DEICTICS_CORRECTION = 1,
    DEICTICS_CORRECTION = 2
}

public enum Role
{
    NONE = 0,
    POINTING_PERSON = 1,
    MIMIC = 2
}

public enum EvaluationPosition
{
    NONE = 0,
    ON_THE_LEFT = 1,
    ON_THE_RIGHT = 2
}

public enum TypeOfHuman
{
    LeftHuman = 1,
    RightHuman = 2
}

public enum PointingArm
{
    NONE,
    LEFT,
    RIGHT
}

public class Evaluation : MonoBehaviour {

    public EvaluationCondition condition = EvaluationCondition.NONE;

    private NetworkCommunication _network;
    private ServerConsole _console;
    private EvaluationConfigProperties _config;
	private LogEvaluation _log;

	private int _numberOfRepetitions = 14;
	public int NumberOfRepetitions
	{
		get 
		{
			return _numberOfRepetitions;
		}
	}

	private int _task = 0;
	public int Task
	{
		get
		{ 
			return _task;
		}
	}

    private EvaluationPosition _clientPosition;
    public EvaluationPosition clientPosition
    {
        get
        {
            return _clientPosition;
        }
    }

    private BodiesManager _bodies;

    public GUIStyle titleFontStyle;
    public GUIStyle conditionFontStyle;
    public Texture backIcon;
    public Texture forwardIcon;
	private GUIStyle redBox;
	private GUIStyle greenBox;

    private bool _taskInProgress = false;
	public bool taskInProgress
	{
		get
		{ 
			return _taskInProgress;
		}
	}

	public GameObject leftTarget;
    public GameObject rightTarget;

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

    private Instructions _instructionsPanel;

    public BoxCollider wallCollider;

    public bool showClientGUI;

    public GameObject TargetGameobject;

    void Start () {

		redBox = new GUIStyle ();
		redBox.normal.background = MakeTex( 2, 2, new Color(255f / 255f, 59f  / 255f, 48f / 255f));
		greenBox = new GUIStyle ();
		greenBox.normal.background = MakeTex( 2, 2, new Color(76f / 255f, 217f / 255f, 100f / 255f));

        _network = GetComponent<NetworkCommunication>();
        _console = GetComponent<ServerConsole>();
        _config = GetComponent<EvaluationConfigProperties>();
        _bodies = GameObject.Find("BodiesManager").GetComponent<BodiesManager>();
		_log = GetComponent<LogEvaluation> ();

        _instructionsPanel = GameObject.Find("InstructionsPanel").GetComponent<Instructions>();

        _clientPosition = _config.clientPosition;

        Debug.Log(_clientPosition);

        condition = EvaluationCondition.NO_DEICTICS_CORRECTION;

        _task = 1;

		if (_network.evaluationPeerType == EvaluationPeertype.SERVER) {
			_log.Init (_numberOfRepetitions);
		}

        leftTarget.SetActive(false);
        rightTarget.SetActive(false);

        _instructionsPanel.gameObject.SetActive(false);
    }
	

    void OnGUI()
    {
        if (_network.evaluationPeerType == EvaluationPeertype.SERVER)
        {
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

            if (GUI.Button(new Rect(Screen.width - width + 10, top, 40, 40), backIcon, GUIStyle.none))
            {
                _swapCondition();
            }
            left += 10;
            GUI.Label(new Rect(Screen.width - width / 2 - 100, top - 5, 200, 2 * lineHeight), condition.ToString().Replace('_', ' '), conditionFontStyle);
            if (GUI.Button(new Rect(Screen.width - 40 - 10, top, 40, 40), forwardIcon, GUIStyle.none))
            {
                _swapCondition();
            }

            // TASK
            top += 4 * lineHeight; left = Screen.width - width + 10;
            GUI.Label(new Rect(left, top, width, lineHeight), "Task:", titleFontStyle);
            top += 2 * lineHeight; left += 10;

            if (GUI.Button(new Rect(Screen.width - width + 10, top, 40, 40), backIcon, GUIStyle.none))
            {
                _decTask();
            }
            left += 10;
            GUI.Label(new Rect(Screen.width - width / 2 - 100, top - 5, 200, 2 * lineHeight), "Task " + _task, conditionFontStyle);
            if (GUI.Button(new Rect(Screen.width - 40 - 10, top, 40, 40), forwardIcon, GUIStyle.none))
            {
                _incTask();
            }

            // ACTIONS
            top += 4 * lineHeight; left = Screen.width - width + 10;
            GUI.Label(new Rect(left, top, width, lineHeight), "Actions:", titleFontStyle);

            top += 2 * lineHeight;
            left = Screen.width - width;

            if (!_taskInProgress)
            {
                if (GUI.Button(new Rect(left + 10, top, width - 20, 1.5f * lineHeight), "Start Task"))
                {
                    if (_checkHumans())
                    {
                        _taskInProgress = true;

                        _network.startTrial(_task, (int) condition);
                        _console.writeLineGreen("TASK " + _task + " started...");
                    }
                }
            }
            else
            {
                if (GUI.Button(new Rect(left + 10, top, width - 20, 1.5f * lineHeight), "End Task"))
                {
                    bool _continue = false;

                    try
                    {
                        // task
                        // condition
                        Vector3 leftHuman_Hitpoint = Vector3.zero;
                        PointingArm leftHuman_PointingArm = _isPointing(leftHuman, EvaluationPosition.ON_THE_LEFT, out leftHuman_Hitpoint);

                        Vector3 rightHuman_Hitpoint = Vector3.zero;
                        PointingArm rightHuman_PointingArm = _isPointing(rightHuman, EvaluationPosition.ON_THE_RIGHT, out rightHuman_Hitpoint);

                        Role leftHuman_Role = getRole(_task, EvaluationPosition.ON_THE_LEFT);
                        Role rightHuman_Role = getRole(_task, EvaluationPosition.ON_THE_RIGHT);

                        Vector3 target = TargetGameobject.transform.position;

                        Vector3 Phead = Vector3.zero;
                        Vector3 PElbow = Vector3.zero;
                        Vector3 PHandTip = Vector3.zero;
                        Vector3 PhHead = Vector3.zero;
                        Vector3 PhElbow = Vector3.zero;

                        Vector3 Mhead = Vector3.zero;
                        Vector3 MElbow = Vector3.zero;
                        Vector3 MHandTip = Vector3.zero;
                        Vector3 MhHead = Vector3.zero;
                        Vector3 MhElbow = Vector3.zero;


                        GameObject LeftBody = _bodies.LeftHumanGO;
                        GameObject RightBody = _bodies.RightHumanGO;

                        if (leftHuman_Role == Role.POINTING_PERSON)
                        {
                            Phead = LeftBody.transform.Find("HEAD").transform.position;
                            string str = leftHuman_PointingArm == PointingArm.LEFT ? "LEFTELBOW" : "RIGHTELBOW";
                            PhElbow = LeftBody.transform.Find(str).transform.position;
                            str = leftHuman_PointingArm == PointingArm.LEFT ? "LEFTHANDTIP" : "RIGHTHANDTIP";
                            PHandTip = LeftBody.transform.Find(str).transform.position;
                            PhHead = _getHitpoint(Phead, PHandTip);
                            PhElbow = _getHitpoint(PElbow, PHandTip);

                            Mhead = RightBody.transform.Find("HEAD").transform.position;
                            str = rightHuman_PointingArm == PointingArm.LEFT ? "LEFTELBOW" : "RIGHTELBOW";
                            MhElbow = RightBody.transform.Find(str).transform.position;
                            str = rightHuman_PointingArm == PointingArm.LEFT ? "LEFTHANDTIP" : "RIGHTHANDTIP";
                            MHandTip = RightBody.transform.Find(str).transform.position;
                            MhHead = _getHitpoint(Mhead, MHandTip);
                            MhElbow = _getHitpoint(MElbow, MHandTip);
                        }
                        else
                        {
                            Mhead = LeftBody.transform.Find("HEAD").transform.position;
                            string str = leftHuman_PointingArm == PointingArm.LEFT ? "LEFTELBOW" : "RIGHTELBOW";
                            MhElbow = LeftBody.transform.Find(str).transform.position;
                            str = leftHuman_PointingArm == PointingArm.LEFT ? "LEFTHANDTIP" : "RIGHTHANDTIP";
                            MHandTip = LeftBody.transform.Find(str).transform.position;
                            MhHead = _getHitpoint(Mhead, MHandTip);
                            MhElbow = _getHitpoint(MElbow, MHandTip);

                            Phead = RightBody.transform.Find("HEAD").transform.position;
                            str = rightHuman_PointingArm == PointingArm.LEFT ? "LEFTELBOW" : "RIGHTELBOW";
                            PhElbow = RightBody.transform.Find(str).transform.position;
                            str = rightHuman_PointingArm == PointingArm.LEFT ? "LEFTHANDTIP" : "RIGHTHANDTIP";
                            PHandTip = RightBody.transform.Find(str).transform.position;
                            PhHead = _getHitpoint(Phead, PHandTip);
                            PhElbow = _getHitpoint(PElbow, PHandTip);
                        }

                        float d_PhHead_Target = Vector3.Distance(PhHead, target);
                        float d_MhHead_Target = Vector3.Distance(MhHead, target);
                        float d_PhHead_MhHead = Vector3.Distance(PhHead, MhHead);

                        float d_PhElbow_Target = Vector3.Distance(PhElbow, target);
                        float d_MhElbow_Target = Vector3.Distance(MhElbow, target);
                        float d_PhElbow_MhElbow = Vector3.Distance(PhElbow, MhElbow);

                        float d_PhHead_MhElbow = Vector3.Distance(PhHead, MhElbow);
                        float d_MhHead_PhElbow = Vector3.Distance(MhHead, PhElbow);

                        if (_task != 1 && _task != 8)
                        {
                            _log.recordSnapshot(_task,
                                                condition,

                                                leftHuman_PointingArm,
                                                rightHuman_PointingArm,

                                                leftHuman_Role,
                                                rightHuman_Role,

                                                target,

                                                Phead, PElbow, PHandTip, PhHead, PhElbow,

                                                Mhead, MElbow, MHandTip, MhHead, MhElbow,

                                                d_PhHead_Target, d_MhHead_Target, d_PhHead_MhHead,

                                                d_PhElbow_Target, d_MhHead_Target, d_PhElbow_MhElbow,

                                                d_PhHead_MhElbow, d_MhHead_PhElbow
                                                );



                        }

                        _continue = true;
                    }
                    catch (Exception e)
                    {
                        _console.writeLineRed(e.Message);
                        Debug.Log(e.Message);
                        Debug.LogError(e.StackTrace);
                    }

                    if (_continue)
                    {
                        _taskInProgress = false;
                        _network.endTrial();

                        _console.writeLine("TASK " + _task + " ended...");

                        if (_task == _numberOfRepetitions)
                        {
                            _console.writeLineGreen(" ");
                            _console.writeLineGreen("########################################");
                            _console.writeLineGreen(" ");
                            _console.writeLineGreen("                Session ended");
                            _console.writeLineGreen(" ");
                            _console.writeLineGreen("########################################");
                            _console.writeLineGreen(" ");
                            reset();
                        }
                        _incTask();

                        if (_task < _numberOfRepetitions)
                        {
                            _network.startTrial(_task, (int) condition);
                        }
                    }
                }
            }

			if (GUI.Button(new Rect(left + 10, Screen.height - 19, width - 20, 19), "Reset"))
			{
				reset();
                _network.reset();
			}
        }
        else if (_network.evaluationPeerType == EvaluationPeertype.CLIENT && showClientGUI)
        {
            GUI.Box(new Rect(0, Screen.height / 2 - 150, Screen.width, 300), "");
            GUI.Label(new Rect(10, 10, 1000, 500), "CLIENT: " + clientPosition.ToString(), conditionFontStyle);
            GUI.Label(new Rect(10, 60, 1000, 500), "IN PROGRESS: " + taskInProgress, conditionFontStyle);
            GUI.Label(new Rect(10, 110, 1000, 500), "TASK: " + _task, conditionFontStyle);
            GUI.Label(new Rect(10, 160, 1000, 500), "CONDITION: " + condition, conditionFontStyle);
        }
    }

    private Vector3 _getHitpoint(Vector3 a, Vector3 b)
    {
        Vector3 hitpoint = Vector3.zero;

        Ray leftRay = new Ray(a, b - a);
        RaycastHit leftHit;
        if (wallCollider.Raycast(leftRay, out leftHit, float.PositiveInfinity))
        {
            hitpoint = leftHit.point;
        }
        return hitpoint;
    }

    private PointingArm _isPointing(Human human, EvaluationPosition position, out Vector3 hitpoint)
    {
        Vector3 head = human.body.Joints[BodyJointType.head];
        Vector3 leftHand = human.body.Joints[BodyJointType.leftHandTip];
        Vector3 rightHand = human.body.Joints[BodyJointType.rightHandTip];

        Debug.DrawRay(head, (leftHand - head).normalized, Color.cyan);
        Debug.DrawRay(head, (rightHand - head).normalized, Color.cyan);


        hitpoint = Vector3.zero;

        bool leftPointing = false;
        Ray leftRay = new Ray(head, leftHand - head);
        RaycastHit leftHit;
        if (wallCollider.Raycast(leftRay, out leftHit, float.PositiveInfinity))
        {
            hitpoint = leftHit.point;
            leftPointing = true;
        }

        bool rightPointing = false;
        Ray rightRay = new Ray(head, rightHand - head);
        RaycastHit rightHit;
        if (wallCollider.Raycast(rightRay, out rightHit, float.PositiveInfinity))
        {
            hitpoint = rightHit.point; 
            rightPointing = true;
        }



        if (leftPointing && !rightPointing)
        {
            return PointingArm.LEFT;
        }

        if (!leftPointing && rightPointing)
        {
            return PointingArm.RIGHT;
        }

        if (leftPointing && rightPointing)
        {
            throw new Exception("USER " + position.ToString().Replace('_', ' ') + " USED BOTH HANDS >:(");
        }

        if (!leftPointing && !rightPointing)
        {
            throw new Exception("USER " + position.ToString().Replace('_', ' ') + " NOT POINTING uhgg");
        }

        return PointingArm.NONE;
    }

    private bool _checkHumans()
	{
		bool ret = false;

		if (leftHuman != null) {
			ret = true;
		} else {
			_console.writeLineRed ("Undefined LEFT HUMAN!!");
		}

		if (rightHuman != null) {
			ret = ret && true;
		} else {
			_console.writeLineRed ("Undefined RIGHT HUMAN!!");
		}

		return ret;
	}

    private void _swapCondition()
    {
        if (condition == EvaluationCondition.DEICTICS_CORRECTION)
            condition = EvaluationCondition.NO_DEICTICS_CORRECTION;
        else
            condition = EvaluationCondition.DEICTICS_CORRECTION;
    }

    private void _incTask()
    {
        _task++;
        if (_task > _numberOfRepetitions) _task = 1;
    }

    private void _decTask()
    {
        _task--;
        if (_task < 1) _task = _numberOfRepetitions;
    }

    private void _setupHumans(TypeOfHuman human)
    {
        string id = "None";

        try
        {
            if (human == TypeOfHuman.LeftHuman)
            {
                id = _bodies.calibrateLeftHuman();
                _network.calibrateHumans((int)TypeOfHuman.LeftHuman);
            }
            else
            {
                id = _bodies.calibrateRightHuman();
                _network.calibrateHumans((int)TypeOfHuman.RightHuman);
            }
            _console.writeLineGreen(human.ToString() + " calibrated with ID: " + id);
        }
        catch (Exception e)
        {
            _console.writeLineRed(e.Message);
        }
    }

    internal void OnRPC_calibrateHumans(EvaluationPosition evaluationPosition)
    {

        if (_network.evaluationPeerType == EvaluationPeertype.CLIENT)
        {
            Debug.Log("[RPC] OnRPC_calibrateHumans");
            _bodies.calibrateHumans(evaluationPosition);
        }
    }

	internal void OnRPC_startTrial(int trialID, EvaluationCondition condition)
    {
		if (_network.evaluationPeerType == EvaluationPeertype.CLIENT)
        {
            Debug.Log("[RPC] OnRPC_startTrial");
            this.condition = condition;
            // setup condition
            _task = trialID;
            _taskInProgress = true;

            _instructionsPanel.gameObject.SetActive(true);


            if (clientPosition == EvaluationPosition.ON_THE_LEFT)
            {
                if (_task == 1) leftTarget.SetActive(true);
            }

            if (clientPosition == EvaluationPosition.ON_THE_RIGHT)
            {
                if (_task == 1 || _task == 8) rightTarget.SetActive(true);

            }

        }
	}

	internal void OnRPC_endTrial()
	{
        if (_network.evaluationPeerType == EvaluationPeertype.CLIENT && _taskInProgress) {
            Debug.Log("[RPC] OnRPC_endTrial");
            leftTarget.SetActive(false);
            rightTarget.SetActive(false);
            _taskInProgress = false;
            _instructionsPanel.gameObject.SetActive(false);


            if (_task == NumberOfRepetitions)
            {
                _task = 0;
                reset();
            }
        }
	}

    public void reset()
    {
        _taskInProgress = false;
        _instructionsPanel.gameObject.SetActive(false);
        leftTarget.SetActive(false);
        rightTarget.SetActive(false);
    }

    private Texture2D MakeTex( int width, int height, Color col )
	{
		Color[] pix = new Color[width * height];
		for( int i = 0; i < pix.Length; ++i )
		{
			pix[ i ] = col;
		}
		Texture2D result = new Texture2D( width, height );
		result.SetPixels( pix );
		result.Apply();
		return result;
	}

    public Role getRole(int task, EvaluationPosition position)
    {
        if (task == 1) return Role.POINTING_PERSON;

        if (position == EvaluationPosition.ON_THE_LEFT) return evenNumber(task) ? Role.POINTING_PERSON : Role.MIMIC;

        if (position == EvaluationPosition.ON_THE_RIGHT) return !evenNumber(task) ? Role.POINTING_PERSON : Role.MIMIC;

        return Role.NONE;
    }

    public Role getMyRole()
    {
        return getRole(_task, clientPosition);
    }

    public static bool evenNumber(int n)
    {
        return Convert.ToBoolean((n % 2 == 0 ? true : false));
    }

}
