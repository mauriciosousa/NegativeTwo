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
    public Role role = Role.NONE;
    public EvaluationPosition evaluationPosition = EvaluationPosition.NONE;

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

    public BoxCollider wallCollider;

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

        _clientPosition = _config.clientPosition;

        condition = EvaluationCondition.NO_DEICTICS_CORRECTION;

        _task = 1;

		if (_network.evaluationPeerType == EvaluationPeertype.SERVER) {
			_log.Init (_numberOfRepetitions);
		}

        leftTarget.SetActive(false);
        rightTarget.SetActive(false);

    }
	
	void Update () {

        if (_network.evaluationPeerType == EvaluationPeertype.CLIENT && _taskInProgress)
        {

        }
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

                        _network.startTrial(_task);
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
                        Vector3 left_hitpoint = Vector3.zero;

                        Role leftHumanRole = getRole(_task, EvaluationPosition.ON_THE_LEFT);
                        PointingArm leftHuman_PointingArm = _isPointing(leftHuman, EvaluationPosition.ON_THE_LEFT, out left_hitpoint);


                        Vector3 right_hitpoint = Vector3.zero;

                        Role rightHumanRole = getRole(_task, EvaluationPosition.ON_THE_RIGHT);
                        PointingArm rightHuman_PointingArm = _isPointing(rightHuman, EvaluationPosition.ON_THE_RIGHT, out right_hitpoint);

                        Vector3 pointer_head = Vector3.zero;
                        Vector3 mimic_head = Vector3.zero;

                        Vector3 origin = Vector3.zero;
                        Vector3 pointer_hit = Vector3.zero;
                        Vector3 mimic_hit = Vector3.zero;

                        if (leftHumanRole == Role.POINTING_PERSON)
                        {
                            pointer_head = leftHuman.body.Joints[BodyJointType.head];
                            mimic_head = rightHuman.body.Joints[BodyJointType.head];

                            origin = pointer_head;
                            pointer_hit = left_hitpoint;
                            mimic_hit = right_hitpoint;
                        }
                        else
                        {
                            pointer_head = rightHuman.body.Joints[BodyJointType.head];
                            mimic_head = leftHuman.body.Joints[BodyJointType.head];

                            origin = pointer_head;
                            pointer_hit = right_hitpoint;
                            mimic_hit = left_hitpoint;
                        }

                        Vector3 pointer_vector = (pointer_hit - origin);
                        Vector3 mimic_vector = (mimic_hit - origin);

                        float error_angle = Vector3.Angle(pointer_vector, mimic_vector);
                        float error_distance = Vector3.Distance(pointer_hit, mimic_hit);



                        _log.recordSnapshot(_task, condition, leftHuman, leftHumanRole, leftHuman_PointingArm, rightHuman, rightHumanRole, rightHuman_PointingArm, error_angle, error_distance, pointer_hit, pointer_head, mimic_hit, mimic_head);

                        _continue = true;
                    }
                    catch (Exception e)
                    {
                        _console.writeLineRed(e.Message);
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
                        }
                        _incTask();
                    }
                }
            }
        }
        else if (_network.evaluationPeerType == EvaluationPeertype.CLIENT)
        {
            GUI.Box(new Rect(Screen.width / 2, 30, 300, 30), "in progress: " + taskInProgress);
        }
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
            _bodies.calibrateHumans(evaluationPosition);
        }
    }

    internal void OnRPC_setupTrial(EvaluationPosition evaluationPosition, Role role, EvaluationCondition evaluationCondition)
    {
        throw new NotImplementedException();
    }

	internal void OnRPC_startTrial(int trialID, EvaluationCondition condition)
    {
		if (_network.evaluationPeerType == EvaluationPeertype.CLIENT) {

			this.condition = condition;
			// setup condition
			_taskInProgress = true;
		}
	}

	internal void OnRPC_endTrial()
	{
		if (_network.evaluationPeerType == EvaluationPeertype.CLIENT && _taskInProgress) {
			_taskInProgress = false;
		}
	}

    internal void OnRPC_reset()
    {
        throw new NotImplementedException();
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
        return getRole(_task, evaluationPosition);
    }

    public static bool evenNumber(int n)
    {
        return Convert.ToBoolean((n % 2 == 0 ? true : false));
    }

}
