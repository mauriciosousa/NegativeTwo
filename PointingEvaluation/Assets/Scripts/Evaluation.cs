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
    OBSERVER = 2
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

public class Evaluation : MonoBehaviour {

    public EvaluationCondition condition = EvaluationCondition.NONE;
    public Role role = Role.NONE;
    public EvaluationPosition evaluationPosition = EvaluationPosition.NONE;

    private NetworkCommunication _network;
    private ServerConsole _console;
    private EvaluationConfigProperties _config;
	private LogEvaluation _log;

	private int _numberOfRepetitions = 10;
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

	public GameObject targetGO;

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

		if (targetGO != null)
			targetGO.SetActive (false);

	}
	
	void Update () {

    }

    void OnGUI()
    {
        if (_network.evaluationPeerType == EvaluationPeertype.SERVER)
        {
            int width = (int) Math.Max(Math.Ceiling(Screen.width / 3.5f), 300);
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
			GUI.Box (new Rect(left + 5, top + 1.5f*lineHeight, width / 2 - 10, 5), "", (leftHuman != null) ? greenBox : redBox);

			if (GUI.Button (new Rect (left + width / 2, top, width / 2 - 5, 1.5f * lineHeight), "Human RIGHT"))
            {
                _setupHumans(TypeOfHuman.RightHuman);
            }
			GUI.Box (new Rect(left + width / 2, top + 1.5f*lineHeight, width / 2 - 5, 5), "", (leftHuman != null) ? greenBox : redBox);


            // CONDITION
            top += 4 * lineHeight; left += 10;
            GUI.Label(new Rect(left, top, width, lineHeight), "Condition:", titleFontStyle);
            top += 2 * lineHeight; left += 10;

            if (GUI.Button(new Rect(Screen.width - width + 10, top, 40, 40), backIcon, GUIStyle.none))
            {
                _swapCondition();
            }
            left += 10;
            GUI.Label(new Rect(Screen.width - width / 2 - 100, top-5, 200, 2 * lineHeight), condition.ToString().Replace('_', ' '), conditionFontStyle);
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
					if (_checkHumans ()) {
						_taskInProgress = true;
						if (targetGO != null) {
							targetGO.SetActive (_task == 1);
						}
						_network.startTrial (_task);
						_console.writeLineGreen ("TASK " + _task + " started...");
					}
                }
            }
            else
            {
                if (GUI.Button(new Rect(left + 10, top, width - 20, 1.5f * lineHeight), "End Task"))
                {
					//_log.recordSnapshot (_task, (int) condition, leftHuman, rightHuman);

					if (targetGO != null) {
						targetGO.SetActive (false);
					}
                    _taskInProgress = false;
					_network.endTrial ();
					_console.writeLine ("TASK " + _task + " ended...");
					if (_task == _numberOfRepetitions)
						_console.writeLineGreen ("(( Session ended ))");
                    _incTask();
                }
            }
        }
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
        try
        {
            if (human == TypeOfHuman.LeftHuman)
            {
                _bodies.calibrateLeftHuman();
                _network.calibrateHumans((int)TypeOfHuman.LeftHuman);
            }
            else
            {
                _bodies.calibrateRightHuman();
                _network.calibrateHumans((int)TypeOfHuman.RightHuman);
            }
            _console.writeLineGreen(human.ToString() + " calibrated");
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
			if (targetGO != null && role == Role.POINTING_PERSON) {
				targetGO.SetActive (_task == 1);
			}
		}
	}

	internal void OnRPC_endTrial()
	{
		if (_network.evaluationPeerType == EvaluationPeertype.CLIENT && _taskInProgress) {
			_taskInProgress = false;

			if (targetGO != null) {
				targetGO.SetActive (false);
			}
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

}
