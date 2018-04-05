using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
public enum WhackAMoleSessionType
{
    FOUR = 4,
    EIGHT = 8, 
    SIXTEEN = 16
}
*/
public enum EvaluationConditionType
{
    NONE,
    SIMULATED_SIDE_SIDE, 
    MIRRORED_PERSON,
    SIMULATED_SIDE_SIDE_Depth,
    MIRRORED_PERSON_Depth
}

public class MicroTaskData
{
    private DateTime _startTime;
    public DateTime startTime { get { return _startTime; } }

    private DateTime endTime;

    private DateTime _correctSelectionTimeStamp;
    private bool _success = false;
    public bool success
    {
        get
        {
            return _success;
        }

        set
        {
            _success = value;
            if (_success)
            {
                _correctSelectionTimeStamp = DateTime.Now;
            }
        }
    }

    private int _numberOfErrors = 0;
    public int NumberOfErrors { get { return _numberOfErrors; } } 

    public EvaluationConditionType condition;
    public int taskType;
    public int microtaskID;

    private List<bool> _usingDeictics;


    public float usingDeictics
    {
        get
        {
            double u = 0;
            foreach (bool b in _usingDeictics)
            {
                if (b) u++;
            }
            return (float) u / _usingDeictics.Count;
        }
    }

    public void incErrors()
    {
        if (!success) _numberOfErrors++;
    }

    public MicroTaskData(EvaluationConditionType condition, int tasktype, int microtask)
    {
        this.condition = condition;
        this.taskType = tasktype;
        this.microtaskID = microtask;
        _usingDeictics = new List<bool>();
    }

    public void addPointing(bool v)
    {
        if (!success)
            _usingDeictics.Add(v);
    }

    public void START()
    {
        _startTime = DateTime.Now;
        Debug.Log("[microtask]     START        " + condition + " " + taskType + " " + microtaskID);
    }
    public void END()
    {
        endTime = DateTime.Now;
        Debug.Log("[microtask]    END    total:" + this.TimeSpanMilliseconds + " using deictics %:" + usingDeictics); /// ????????????????????
    }

    public double TimeSpanMilliseconds 
    {
        get
        {
            return (endTime - startTime).TotalMilliseconds;
            //return ((TimeSpan)(endTime - startTime)).Milliseconds;
        }
    }

    public double TimeToCorrectSelection
    {
        get
        {
            return success ? (_correctSelectionTimeStamp - startTime).TotalMilliseconds : -1;
        }
    }
}

public class WhackAMole : MonoBehaviour {


    public GameObject selectedCube = null;
    public GameObject targetCube = null;
    public GameObject highlightedCube = null;
    private string lastTargetedCube = "";
    private string lastWronglySelectedCube = "";


    private bool _init = false;
    public bool IsInit { get { return _init; } }

    private Transform _workspace;
    private NegativeSpace _negativeSpace;
    private Main _main;

    public GameObject cubePrefab;
    public Vector3 CubesScale;

    private List<GameObject> _availableCubes;
    private Dictionary<int, GameObject> _cubesNumbers;

    private EvaluationConditionType evaluationCondition = EvaluationConditionType.NONE;

    public bool trialInProgress = false;
    public bool habituationTaskInProgress = false;

    public int taskType = 1;

    private int task = 1;
    private int microTask = 1;
    private bool microtasking = false;
    private int MaxRepetitions = 10;
    private double timelimit = 5 * 1000;  //ms

    private string LogFileDir
    {
        get
        {
            string filedir = Application.dataPath + "/TASKLOGS";
            if (!System.IO.Directory.Exists(filedir)) System.IO.Directory.CreateDirectory(filedir);
            return filedir;
        }
    }

    void Start()
    {
        _availableCubes = new List<GameObject>();
        _cubesNumbers = new Dictionary<int, GameObject>();
    }

    internal void Init()
    {
        _negativeSpace = GameObject.Find("Main").GetComponent<NegativeSpace>();
        transform.position = (_negativeSpace.LocalSurface.SurfaceBottomLeft + _negativeSpace.RemoteSurface.SurfaceBottomRight) * 0.5f;
        transform.rotation = _negativeSpace.NegativeSpaceCenter.transform.rotation;
        _main = GameObject.Find("Main").GetComponent<Main>();

        _init = true;
    }

    private Stack<MicroTaskData> _microtaskData = new Stack<MicroTaskData>(); //never pop please... just push and peek
    private List<string> _instructorIsPointing_LogLines = new List<String>();
    private List<bool> _instructorIsPointing = new List<bool>();
    private bool _storingInstructorData = false;
    void Update ()
    {
        if (!_init) return;

        EvaluationClient client = GetComponent<EvaluationClient>();

        if (habituationTaskInProgress)
        {
            if (_main.location == Location.Assembler)
            {
                selectedCube = _checkButtons();

                if (selectedCube != null)
                {
                    if (selectedCube == targetCube)
                    {
                        selectedCube.GetComponent<CubeSelection>().correctSelection();
                        client.reportToInstructorCubeSelected(selectedCube, true);
                    }
                    else
                    {
                        selectedCube.GetComponent<CubeSelection>().wrongSelection();
                        lastWronglySelectedCube = selectedCube.name;
                        client.reportToInstructorCubeSelected(selectedCube, false);
                    }
                }
            }
        }

        if (trialInProgress)
        {
            if (_main.location == Location.Assembler)
            {
                if (microTask <= MaxRepetitions)
                {
                    if (microTask == 1 && !microtasking)
                    {
                        //STARTMICRO
                        microtasking = true;

                        MicroTaskData m = new MicroTaskData(evaluationCondition, taskType, microTask); // START FIRST MICROTASK
                        _microtaskData.Push(m);
                        _selectTargetCube();
                        client.reportToInstructorMicroTaskStarted(_microtaskData.Peek().microtaskID, task, targetCube.name);
                        m.START();
                    }

                    #region MICROTASK
                    _microtaskData.Peek().addPointing(_negativeSpace.isUsingDeitics);
                    //print(_microtaskData.Count);

                    if (!_microtaskData.Peek().success) selectedCube = _checkButtons();

                    if (selectedCube != null)
                    {
                        if (selectedCube == targetCube && !_microtaskData.Peek().success)
                        {
                            _microtaskData.Peek().success = true;
                            selectedCube.GetComponent<CubeSelection>().correctSelection();
                            print("SELECTED CORRECTLY                  " + selectedCube + "  ----   " + targetCube);
                            client.reportToInstructorCubeSelected(selectedCube, true);
                        }
                        else
                        {
                            _microtaskData.Peek().incErrors();
                            selectedCube.GetComponent<CubeSelection>().wrongSelection();
                            lastWronglySelectedCube = selectedCube.name;
                            print("SELECTED WRONGLYYY                  " + selectedCube + "  ----   " + targetCube);
                            client.reportToInstructorCubeSelected(selectedCube, false);
                        }
                    }
                    selectedCube = null;
                    #endregion

                    if (_microtaskData.Peek().startTime.AddMilliseconds(timelimit) < DateTime.Now)
                    {
                        //END micro
                        if (microTask == MaxRepetitions)
                        {
                            microtasking = false;
                            microTask = 1;
                            trialInProgress = false;

                            client.reportToInstructorMicroTaskEnded(_microtaskData.Peek().microtaskID);
                            _microtaskData.Peek().END(); // end last one
                            print("END EVERythings");

                            _processMicroTaskData();
                            _cleanCubes();
                        }
                        else
                        {
                            // end current task
                            client.reportToInstructorMicroTaskEnded(_microtaskData.Peek().microtaskID);
                            _microtaskData.Peek().END();
                            _removeCubeColors();

                            microTask += 1;

                            // start new task
                            MicroTaskData m = new MicroTaskData(evaluationCondition, taskType, microTask); // START OTHER MICROTASKS
                            _microtaskData.Push(m);
                            _selectTargetCube();
                            client.reportToInstructorMicroTaskStarted(_microtaskData.Peek().microtaskID, task, targetCube.name);
                            m.START();
                        }
                    } 
                }
            }
            else if (_main.location == Location.Instructor)
            {
                if (_storingInstructorData)
                {
                    _instructorIsPointing.Add(_negativeSpace.isUsingDeitics);
                }
            }
        }

        /*
        if(Input.GetKeyDown(KeyCode.F1))
        {
            _distributeCubes(1);
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            _distributeCubes(2);
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            _distributeCubes(3);
        }
        if (Input.GetKeyDown(KeyCode.F4))
        {
            _distributeCubes(4);
        }
        */
    }

    private void _removeCubeColors()
    {
        foreach(GameObject o in _availableCubes)
        {
            o.GetComponent<CubeSelection>().state = CubeSTATE.NONE;
        }
    }

    private GameObject _checkButtons()
    {
        if (Input.GetKeyDown(KeyCode.F1) || Input.GetKeyDown(KeyCode.Keypad1)) return (_cubesNumbers.ContainsKey(1) ? _cubesNumbers[1] : null);
        if (Input.GetKeyDown(KeyCode.F2) || Input.GetKeyDown(KeyCode.Keypad2)) return (_cubesNumbers.ContainsKey(2) ? _cubesNumbers[2] : null);
        if (Input.GetKeyDown(KeyCode.F3) || Input.GetKeyDown(KeyCode.Keypad3)) return (_cubesNumbers.ContainsKey(3) ? _cubesNumbers[3] : null);
        if (Input.GetKeyDown(KeyCode.F4) || Input.GetKeyDown(KeyCode.Keypad4)) return (_cubesNumbers.ContainsKey(4) ? _cubesNumbers[4] : null);
        if (Input.GetKeyDown(KeyCode.F5) || Input.GetKeyDown(KeyCode.Keypad5)) return (_cubesNumbers.ContainsKey(5) ? _cubesNumbers[5] : null);
        if (Input.GetKeyDown(KeyCode.F6) || Input.GetKeyDown(KeyCode.Keypad6)) return (_cubesNumbers.ContainsKey(6) ? _cubesNumbers[6] : null);
        if (Input.GetKeyDown(KeyCode.F7) || Input.GetKeyDown(KeyCode.Keypad7)) return (_cubesNumbers.ContainsKey(7) ? _cubesNumbers[7] : null);
        if (Input.GetKeyDown(KeyCode.F8) || Input.GetKeyDown(KeyCode.Keypad8)) return (_cubesNumbers.ContainsKey(8) ? _cubesNumbers[8] : null);
        if (Input.GetKeyDown(KeyCode.F9) || Input.GetKeyDown(KeyCode.Keypad9)) return (_cubesNumbers.ContainsKey(9) ? _cubesNumbers[9] : null);
        if (Input.GetKeyDown(KeyCode.F10) || Input.GetKeyDown(KeyCode.KeypadDivide)) return (_cubesNumbers.ContainsKey(10) ? _cubesNumbers[10] : null);
        if (Input.GetKeyDown(KeyCode.F11) || Input.GetKeyDown(KeyCode.KeypadMultiply)) return (_cubesNumbers.ContainsKey(11) ? _cubesNumbers[11] : null);
        if (Input.GetKeyDown(KeyCode.F12) || Input.GetKeyDown(KeyCode.KeypadMinus)) return (_cubesNumbers.ContainsKey(12) ? _cubesNumbers[12] : null);

        return null;
    }

    private void _processMicroTaskData()
    {
        List<MicroTaskData> data = new List<MicroTaskData>(_microtaskData.ToArray());
        data.Reverse();
        if (data.Count > 0)
        {
            List<String> lines = new List<string>();
            lines.Add("TASKID" + "#" + "MICROTASKID" + "#" + "CONDITION" + "#" + "SUCCESS" + "#" + "NUMBER_OF_ERRORS" + "#" + "TIME_TILL_SUCCESS" + "#" + "TOTAL_TIME" + "#" + "%_DEICTICS");

            foreach (MicroTaskData m in data)
            {
                string line = "";
                line += m.taskType + "#";
                line += m.microtaskID + "#";
                line += m.condition + "#";
                line += m.success + "#";
                line += m.NumberOfErrors + "#";
                line += m.TimeToCorrectSelection + "#";
                line += m.TimeSpanMilliseconds + "#";
                line += m.usingDeictics;
                lines.Add(line);
            }
            Logger.save(LogFileDir + "/" + DateTime.Now.ToString("yyyy MMMM dd HH mm ss") + ".txt", lines.ToArray());
        }
        _microtaskData.Clear();
    }

    private void _setWorkspace(int session)
    {
        _cleanCubes();

        Vector3 origin = _negativeSpace.RemoteSurface.SurfaceBottomRight;
        Vector3 length = _negativeSpace.RemoteSurface.SurfaceBottomLeft - origin;
        Vector3 depth = _negativeSpace.LocalSurface.SurfaceBottomRight - origin;
        Vector3 up = _negativeSpace.LocalSurface.SurfaceTopRight - _negativeSpace.LocalSurface.SurfaceBottomRight;

        _distributeCubes(session);
    }

    private void _selectTargetCube()
    {
        if (_main.location == Location.Assembler)
        {
            lastTargetedCube = targetCube == null ? "" : targetCube.name;
            lastWronglySelectedCube = "";

            // select target cube
            _availableCubes.Shuffle();
            foreach (GameObject cube in _availableCubes)
            {
                if (cube.name != lastTargetedCube && cube.name != lastWronglySelectedCube)
                {
                    print(lastTargetedCube + " ------> " + cube.name);
                    targetCube = cube;
                    break;
                }
            }
            //print("TARGET CUBE = " + targetCube.name);
        }
    }

    private void _cleanCubes()
    {
        _cubesNumbers.Clear();

        if (_availableCubes != null)
        {
            foreach(GameObject o in _availableCubes)
            {
                Destroy(o);
            }
            _availableCubes.Clear();
        }
    }

    internal void PointingEvent(Vector3 origin, Vector3 direction)
    {
        if (!_init) return;

        if(highlightedCube != null)
        {
            highlightedCube.GetComponent<CubeSelection>().Highlighted = false;
            highlightedCube = null;
        }

        Transform screenCenter = GameObject.Find("localScreenCenter").transform;
        Plane surface = new Plane(screenCenter.forward, screenCenter.position);

        Ray ray = new Ray(origin, direction);

        float distance = float.PositiveInfinity;

        bool didHit = false;
        Vector3 hitPoint = Vector3.zero;

        /*if (surface.Raycast(ray, out distance))
        {
            hitPoint = ray.GetPoint(distance);

            Debug.DrawLine(origin, hitPoint, Color.cyan);

            Vector3 pixel = Camera.main.WorldToScreenPoint(hitPoint);
            ray = Camera.main.ScreenPointToRay(pixel);

            didHit = true;
        }

        if (didHit)*/
        {         
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, float.PositiveInfinity))
            {
                Debug.DrawLine(ray.origin, hit.point, Color.red);

                //print("Curte este OBJ: " + hit.collider.gameObject.name);

                distance = float.PositiveInfinity;
                
                foreach (GameObject cube in _availableCubes)
                {
                    float newDistance = Vector3.Distance(hit.point, cube.transform.position);
                    if (newDistance < distance)
                    {
                        highlightedCube = cube;
                        distance = newDistance;
                    }
                }

                if (highlightedCube != null)
                {
                    highlightedCube.GetComponent<CubeSelection>().Highlighted = true;

                    print("                         SELECTED CUBE: " + highlightedCube.ToString());
                }
            }
        }
    }

    /*public bool IAmPointing(Ray ray, bool click, out Vector3 hitPoint) // who dis?
    {
        print("POINT_NIG YEAHHH");

        hitPoint = Vector3.zero;
        bool didHit = false;

        if (!_init) return didHit;

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, float.PositiveInfinity))
        {
            didHit = true;
            hitPoint = hit.point;

            Highlight h = hit.transform.gameObject.GetComponent<Highlight>();
            if (h != null)
            {
                print("I pointed to a " + hit.transform.gameObject.name + " and I liked it!!!");
            }
        }
        else
        {

        }

        if (click)
        {
            
        }
        return didHit;
    }*/

    private GameObject _instantiateNewCube(string cubeName)
    {
        GameObject newCube = Instantiate(cubePrefab, transform) as GameObject;
        newCube.name = cubeName;
        newCube.transform.localScale = CubesScale;
        newCube.transform.localPosition = new Vector3(0f, 0f + CubesScale.y / 2, 0f);
        newCube.transform.localRotation = Quaternion.identity;
        return newCube;
    }

    private void _upTheCubes()
    {
        foreach (GameObject c in _availableCubes)
        {
            c.transform.position += c.transform.up * (c.transform.lossyScale.y / 2);
        }
    }

    private void _distributeCubes(int task)
    {
        float cubeSize = 0.05f;
        int numberOfCubes;

        if (task == 1) numberOfCubes = 4;
        else if (task == 2) numberOfCubes = 6;
        else if(task == 3) numberOfCubes = 9;
        else numberOfCubes = 12;

        Vector3 origin = _negativeSpace.RemoteSurface.SurfaceBottomRight;
        Vector3 length = _negativeSpace.RemoteSurface.SurfaceBottomLeft - origin;
        Vector3 depth = _negativeSpace.LocalSurface.SurfaceBottomRight - origin;
        Vector3 up = _negativeSpace.LocalSurface.SurfaceTopRight - _negativeSpace.LocalSurface.SurfaceBottomRight;

        _cleanCubes();

        List<int> numbers = new List<int>();

        int i;
        for (i = 1; i <= numberOfCubes; i++)
        {
            numbers.Add(i);
        }
        numbers.Shuffle();

        for (i = 1; i <= numberOfCubes; i++)
        {
            GameObject cube = _instantiateNewCube("cube_" + i);
            cube.transform.LookAt(cube.transform.position - depth, up);
            _availableCubes.Add(cube);

            if(_main.location == Location.Assembler)
            {
                _cubesNumbers.Add(numbers[i - 1], cube);
                cube.GetComponentInChildren<TextMesh>().text = "" + numbers[i-1];
            }
        }

        i = 0;
        _availableCubes[i++].transform.position = origin + (length.normalized + depth.normalized) * cubeSize;
        _availableCubes[i++].transform.position = origin + length + (-length.normalized + depth.normalized) * cubeSize;
        _availableCubes[i++].transform.position = origin + depth + (length.normalized - depth.normalized) * cubeSize;
        _availableCubes[i++].transform.position = origin + depth + length + (-length.normalized - depth.normalized) * cubeSize;

        if(task == 2 || task == 3)
        {
            _availableCubes[i++].transform.position = origin + depth * 0.5f + (length.normalized) * cubeSize;
            _availableCubes[i++].transform.position = origin + depth * 0.5f + length + (-length.normalized) * cubeSize;
        }

        if(task == 3)
        {
            _availableCubes[i++].transform.position = origin + depth * 0.5f + length * 0.5f;
        }

        if(task == 3 || task == 4)
        {
            _availableCubes[i++].transform.position = origin + length * 0.5f + (depth.normalized) * cubeSize;
            _availableCubes[i++].transform.position = origin + depth + length * 0.5f + (-depth.normalized) * cubeSize;
        }

        if (task == 4)
        {
            _availableCubes[i++].transform.position = origin + depth / 3.0f + (length.normalized) * cubeSize;
            _availableCubes[i++].transform.position = origin + depth / 3.0f + length * 0.5f;
            _availableCubes[i++].transform.position = origin + depth / 3.0f + length + (-length.normalized) * cubeSize;

            _availableCubes[i++].transform.position = origin + depth * 2.0f / 3.0f + (length.normalized) * cubeSize;
            _availableCubes[i++].transform.position = origin + depth * 2.0f / 3.0f + length * 0.5f;
            _availableCubes[i++].transform.position = origin + depth * 2.0f / 3.0f + length + (-length.normalized) * cubeSize;
        }

        _upTheCubes();
    }

    private void _distribute2x2(Vector3 o, Vector3 length, Vector3 depth)
    {
        _availableCubes[0].transform.position = o + (length / 8) + (depth / 4);
        _availableCubes[1].transform.position = o + (9 * length / 8) + (depth / 4);
        _availableCubes[2].transform.position = o + (length / 8) + (3 * depth / 4);
        _availableCubes[3].transform.position = o + (9 * length / 8) + (3 * depth / 4);
        _upTheCubes();
    }

    private void _distribute4x2(Vector3 o, Vector3 length, Vector3 depth)
    {
        _availableCubes[0].transform.position = o + (length / 5) + (depth / 4);
        _availableCubes[1].transform.position = o + (2 * length / 5) + (depth / 4);
        _availableCubes[2].transform.position = o + (3 * length / 5) + (depth / 4);
        _availableCubes[3].transform.position = o + (4 * length / 5) + (depth / 4);
        _availableCubes[4].transform.position = o + (length / 5) + (3 * depth / 4);
        _availableCubes[5].transform.position = o + (2 * length / 5) + (3 * depth / 4);
        _availableCubes[6].transform.position = o + (3 * length / 5) + (3 * depth / 4);
        _availableCubes[7].transform.position = o + (4 * length / 5) + (3 * depth / 4);
        _upTheCubes();
    }

    private void _distribute4x4(Vector3 o, Vector3 length, Vector3 depth)
    {
        _availableCubes[0].transform.position = o + (length / 5) + (depth / 5);
        _availableCubes[1].transform.position = o + (2 * length / 5) + (depth / 5);
        _availableCubes[2].transform.position = o + (3 * length / 5) + (depth / 5);
        _availableCubes[3].transform.position = o + (4 * length / 5) + (depth / 5);

        _availableCubes[4].transform.position = o + (length / 5) + (2 * depth / 5);
        _availableCubes[5].transform.position = o + (2 * length / 5) + (2 * depth / 5);
        _availableCubes[6].transform.position = o + (3 * length / 5) + (2 * depth / 5);
        _availableCubes[7].transform.position = o + (4 * length / 5) + (2 * depth / 5);

        _availableCubes[8].transform.position = o + (length / 5) + (3 * depth / 5);
        _availableCubes[9].transform.position = o + (2 * length / 5) + (3 * depth / 5);
        _availableCubes[10].transform.position = o + (3 * length / 5) + (3 * depth / 5);
        _availableCubes[11].transform.position = o + (4 * length / 5) + (3 * depth / 5);

        _availableCubes[12].transform.position = o + (length / 5) + (4 * depth / 5);
        _availableCubes[13].transform.position = o + (2 * length / 5) + (4 * depth / 5);
        _availableCubes[14].transform.position = o + (3 * length / 5) + (4 * depth / 5);
        _availableCubes[15].transform.position = o + (4 * length / 5) + (4 * depth / 5);
        _upTheCubes();
    }

    internal void clearBoard() // reset button (server)
    {
        evaluationCondition = EvaluationConditionType.NONE;
        trialInProgress = false;
        habituationTaskInProgress = false;
        targetCube = null;
        _cleanCubes();
    }

    internal void startTrial(int condition, int taskType)
    {
        habituationTaskInProgress = false;

        evaluationCondition = (EvaluationConditionType)condition;
        this.taskType = taskType;
        trialInProgress = true;
        _setWorkspace(taskType);
    }

    internal void startHabituationTask(int condition)
    {
        evaluationCondition = (EvaluationConditionType)condition;
        habituationTaskInProgress = true;
        taskType = 1;
        _setWorkspace(taskType);

        //_setWorkspace(WhackAMoleSessionType.FOUR);
        if (_main.location == Location.Assembler)
        {
            _selectTargetCube();
            GetComponent<EvaluationClient>().HabituationReportToInstructor_targetCube(targetCube);
        }
    }

    internal void updateCube(string gameObjectName, int state)
    {
        if (_availableCubes == null) return;

        foreach (GameObject cube in _availableCubes)
        {
            if (cube.name == gameObjectName)
            {
                cube.GetComponent<CubeSelection>().state = (CubeSTATE) state;
                break;
            }
        }
    }

    internal void INSTRUCTOR_microtaskStarted(int microtask, int task, string targetCubeName)
    {
        if (_main.location == Location.Instructor)
        {
            microTask = microtask;
            targetCube = GameObject.Find(targetCubeName);

            this.task = task;

            targetCube.GetComponent<CubeSelection>().state = CubeSTATE.SELECT;

            if (microtask == 1)
            {
                _instructorIsPointing.Clear();
                _instructorIsPointing_LogLines.Clear();
                _instructorIsPointing_LogLines.Add("TASKID#MICROTASK#EVALUATION_CONDITION#%_DEICTICS");
            }
            _storingInstructorData = true;
        }
    }

    internal void INSTRUCTOR_microtaskEnded(int microtask)
    {
        if (_main.location == Location.Instructor)
        {
            foreach (GameObject cube in _availableCubes)
            {
                cube.GetComponent<CubeSelection>().state = CubeSTATE.NONE;
            }

            _storingInstructorData = false;

            double u = 0;
            foreach (bool b in _instructorIsPointing)
            {
                if (b) u++;
            }
            u = (float)u / _instructorIsPointing.Count;
            _instructorIsPointing_LogLines.Add("" + task + "#" + microTask + "#" + evaluationCondition + "#" + u);
            _instructorIsPointing.Clear();

            _removeCubeColors();

            if (microtask == MaxRepetitions) // micro task ended
            {
                trialInProgress = false;
                Logger.save(LogFileDir + "/instructorLog " + DateTime.Now.ToString("yyyy MMMM dd HH mm ss") + ".txt", _instructorIsPointing_LogLines.ToArray());
                _instructorIsPointing_LogLines.Clear();
                _cleanCubes();
            }
        }
    }

    internal void INSTRUCTOR_assemblerSelectedACube(string selectedCubeName, bool isItTargetCube)
    {
        if (_main.location == Location.Instructor)
        {
            GameObject cube = GameObject.Find(selectedCubeName);
            if (cube != null)
            {
                if (isItTargetCube)
                {
                    cube.GetComponent<CubeSelection>().correctSelection();
                    _storingInstructorData = false;
                }
                else
                {
                    cube.GetComponent<CubeSelection>().wrongSelection();
                }
            }
        }
    }

    internal void INSTRUCTOR_setTargetCube(string targetCubeName)
    {
        targetCube = GameObject.Find(targetCubeName);
        targetCube.GetComponent<CubeSelection>().state = CubeSTATE.SELECT;
        print("I have target cube!!!");
    }

    internal void click()
    {
        Debug.LogError("[Click Event] " + new NotImplementedException().ToString());
    }
}
