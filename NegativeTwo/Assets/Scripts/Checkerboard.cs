using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class CheckerboardLogger
{
    private List<String> _lines;
    private int _size;

    public string[] Lines
    {
        get
        {
            return _lines.ToArray();
        }
    }

    public CheckerboardLogger(int size)
    {
        _lines = new List<string>();
        _size = size;
    }

    public void write(string line)
    {
        if (_lines != null)
        {
            if (_lines.Count == _size)
            {
                _lines.RemoveAt(0);
            }
            _lines.Add(line);
        }
    }
}

public class Checkerboard : MonoBehaviour {
    
    public int boardWidth = 6;
    public int boardDepth = 7;

    private bool _instantiateDark = false;

    public GameObject boardDarkPiece;
    public GameObject boardLightPiece;

    public GameObject blueCube;
    public GameObject greenCube;
    public GameObject pinkCube;
    public GameObject redCube;
    public GameObject yellowCube;

    private Highlight _lastHighlighted = null;
    private GameObject _currentlyHighlighted = null;

    public GameObject[,] boardMap;



    public GameObject selectedObject = null;


    public Material colorBlindMaterial;
    public Material colorBlindHighlightMaterial;
    public Material colorBlindSelected;
    public bool colorBlind = false;

    private bool _init = false;

    public static float moveUp = 0.5f;
    public float scaleFactor = 0.05f;

    private CheckerboardClient client;

    private Main _main;


    private string _solutionsFolder;


    private bool _showDebug = false;
    private CheckerboardLogger _checkerboardLogger;
    private List<Solution> _solutions;


    public Solution currentSolution;
    public bool inAEvaluationSession = false;

    private string _evalResultsDir;
    private System.IO.StreamWriter _evalSessionFile = null;

    private DateTime startTime;

    

    public void Init()
    {
        _evalResultsDir = Application.dataPath + "/Results";

        _loadSolutions();
        _checkerboardLogger = new CheckerboardLogger(10);
        _main = GameObject.Find("Main").GetComponent<Main>();
        colorBlind = _main.location == Location.Assembler ? true : false;
        if (colorBlind)
        {
            blueCube.GetComponent<Highlight>().setMaterials(colorBlindMaterial, colorBlindHighlightMaterial, colorBlindSelected);
            greenCube.GetComponent<Highlight>().setMaterials(colorBlindMaterial, colorBlindHighlightMaterial, colorBlindSelected);
            pinkCube.GetComponent<Highlight>().setMaterials(colorBlindMaterial, colorBlindHighlightMaterial, colorBlindSelected);
            redCube.GetComponent<Highlight>().setMaterials(colorBlindMaterial, colorBlindHighlightMaterial, colorBlindSelected);
            yellowCube.GetComponent<Highlight>().setMaterials(colorBlindMaterial, colorBlindHighlightMaterial, colorBlindSelected);
        }


        boardMap = new GameObject[boardWidth, boardDepth];

        for (int i = 0; i < boardWidth; i++)
        {
            for (int j = 0; j < boardDepth; j++)
            {
                if (i >= boardWidth) i = 0;
                if (j >= boardDepth) j = 0;
                GameObject go = Instantiate(_instantiateDark ? boardDarkPiece : boardLightPiece, transform); // hehe isto só funciona com boardDepth impares :D
                boardMap[i, j] = go;
                go.name = "box(" + i + "," + j + ")";
                _instantiateDark = !_instantiateDark;
                go.transform.position = new Vector3(j - boardDepth / 2, 0, i - boardWidth / 2);
            }
        }

        moveUp = blueCube.transform.localScale.y / 2f;

        

        GameObject negativeSpaceCenter = GameObject.Find("NegativeSpaceCenter");
        NegativeSpace negativeSpace = GameObject.Find("Main").GetComponent<NegativeSpace>();

        transform.position = negativeSpace.bottomCenterPosition + 0.001f * negativeSpaceCenter.transform.up;
        transform.rotation = negativeSpaceCenter.transform.rotation;

        moveUp = moveUp * scaleFactor;
        Debug.Log(this.ToString() + ": Scaling to " + scaleFactor);
        transform.localScale = transform.localScale * scaleFactor;

        string path = Application.dataPath + "/initPositions.txt";
        if (System.IO.File.Exists(path))
        {
            String[] lines = System.IO.File.ReadAllLines(path);
            foreach (string line in lines)
            {
                if (line.Contains(" ontopof "))
                {
                    string[] s = line.Split(' ');
                    putObjectOnTopOf(GameObject.Find(s[0]), GameObject.Find(s[2]));
                }
            }
        }
        else
        {
            putObjectOnTopOf(blueCube, 0, 0);
            putObjectOnTopOf(greenCube, 1, 0);
            putObjectOnTopOf(pinkCube, 2, 0);
            putObjectOnTopOf(redCube, 3, 0);
            putObjectOnTopOf(yellowCube, 4, 0);
        }

        if (!System.IO.Directory.Exists(_evalResultsDir))
        {
            System.IO.Directory.CreateDirectory(_evalResultsDir);
        }


        _init = true;

        client = this.GetComponent<CheckerboardClient>();
        client.Init();
    }

    internal void StartEvaluation(int condition, int puzzle)
    {
        /**
         *   EVALUATION START 
         */
        currentSolution = null;
        if (puzzle != 0)
        {
            currentSolution = GetSolutionByName("" + puzzle);

            SkipFirstStep();

            currentSolution.wrongMoves = 0;
            currentSolution.wrongSelections = 0;

            inAEvaluationSession = true;

            startTime = DateTime.Now;
            Debug.Log("[EVALUATION START] cond={" + condition + "}, puzzle={" + puzzle + "} at " + startTime.ToString("yy/MM/dd-H:mm:ss zzz"));


            string filename = _evalResultsDir + '/' + startTime.ToString("yyMMdd-Hmmss") + ".txt";

            if(_evalSessionFile != null)
                _evalSessionFile.Close();

            _evalSessionFile = new StreamWriter(filename);

            _evalSessionFile.WriteLine("STARTTIME=" + startTime.ToString("yy/MM/dd-H:mm:ss zzz"));
            _evalSessionFile.WriteLine("CONDITION=" + condition);
            _evalSessionFile.WriteLine("PUZZLE=" + puzzle);

        }
        else
        {
            setUpTranningTask();
        }

        _applyCondition(condition);

    }

    private void _applyCondition(int condition)
    {
        switch (condition)
        {

            // ATENCAOO ----- o stream do kinect já está mirror
            case 1:
                // mirror avatar
                // rotate workspace
                transform.localScale = new Vector3(scaleFactor * -1.0f, scaleFactor, scaleFactor * -1.0f);
                _main.mirrorPessoa = true;
                break;
            case 2:
                // mirror avatar
                transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
                _main.mirrorPessoa = true;
                break;
            case 3:
                transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
                _main.mirrorPessoa = false;
                break;
            case 4:
                // mirror avatar
                transform.localScale = new Vector3(scaleFactor * -1.0f, scaleFactor, scaleFactor);
                _main.mirrorPessoa = true;
                break;
        }
    }

    private void setUpTranningTask()
    {
        List<string> cubes = new List<string>();
        cubes.Add("RedCube");
        //cubes.Add("GreenCube");
        cubes.Add("BlueCube");
        cubes.Add("YellowCube");
        cubes.Add("PinkCube");
        cubes.Shuffle();

        List<string> positions = new List<string>();
        positions.Add("box(0,0)");
        positions.Add("box(4,0)");
        positions.Add("box(4,6)");
        positions.Add("box(0,6)");
        positions.Shuffle();

        for (int i = 0; i < cubes.Count; i++)
        {
            putObjectOnTopOf(cubes[i], positions[i]);
        }
        putObjectOnTopOf("GreenCube", "box(3,3)");
    }

    private void SkipFirstStep()
    {
        GameObject that = GameObject.Find(currentSolution.GetNextSelect());
        GameObject there = GameObject.Find(currentSolution.GetNextMove());

        List<string> cubes = _listOfCubes(that.name);
        cubes.Shuffle();

        List<string> positions = new List<string>();
        positions.Add("box(0,0)");
        positions.Add("box(4,0)");
        positions.Add("box(4,6)");
        positions.Add("box(0,6)");
        positions.Shuffle();

        for (int i = 0; i < cubes.Count; i++)
        {
            putObjectOnTopOf(cubes[i], positions[i]);
        }

        putObjectOnTopOf(that, there);
        currentSolution.Move(that.name, there.name);
    }

    private void SkipStep()
    {
        GameObject that = GameObject.Find(currentSolution.GetNextSelect());
        GameObject there = GameObject.Find(currentSolution.GetNextMove());

        putObjectOnTopOf(that, there);
        currentSolution.Move(that.name, there.name);
    }

    private List<string> _listOfCubes(string firstCube)
    {
        List<string> cubes = new List<string>();
        cubes.Add("RedCube");
        cubes.Add("GreenCube");
        cubes.Add("BlueCube");
        cubes.Add("YellowCube");
        cubes.Add("PinkCube");

        cubes.Remove(firstCube);
        
        return cubes;
    }

    private void _loadSolutions()
    {
        currentSolution = null;
        _solutionsFolder = Application.dataPath + "/Solutions/";
        Debug.Log(_solutionsFolder);
        _solutions = new List<Solution>();
        if (Directory.Exists(_solutionsFolder))
        {
            FileInfo[] files = new DirectoryInfo(_solutionsFolder).GetFiles("*.txt");
            if (files.Length > 0)
            {
                foreach (FileInfo f in files)
                {
                    _solutions.Add(new Solution(f.FullName));
                }
            }
            else Debug.LogError(this.ToString() + ": No files in Directory - " + _solutionsFolder);


        }
        else
        {
            Debug.LogError(this.ToString() + ": Directory does not exists - " + _solutionsFolder);
        }

        //string solname = "1";
        //Solution solution = GetSolutionByName(solname);
        //if (solution == null)
        //{
            
        //    Debug.LogError(this.ToString() + ": no such solution " + solname);
        //}
    }

    public Solution GetSolutionByName(string name)
    {
        foreach (Solution s in _solutions)
        {
            Debug.Log("SOLUNITON: " + s.Name);
            if (s.Name == name) return s;
        }
        return null;
    }

    void Update()
    {
        
        if (!_init) return;
        /*
        if (_main.location == Location.Assembler)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Input.GetMouseButtonDown(0))
            {
                IAmPointing(ray, true);
            }
            else
            {
                IAmPointing(ray, false);
            }
        }
        */
        if (client.Connected)
        {
            client.callHighlightUpdate(blueCube.name, blueCube.GetComponent<Highlight>().selected);
            client.callHighlightUpdate(greenCube.name, greenCube.GetComponent<Highlight>().selected);
            client.callHighlightUpdate(pinkCube.name, pinkCube.GetComponent<Highlight>().selected);
            client.callHighlightUpdate(redCube.name, redCube.GetComponent<Highlight>().selected);
            client.callHighlightUpdate(yellowCube.name, yellowCube.GetComponent<Highlight>().selected);
        }




        // check finishzzz

        if (currentSolution != null)
        {
            if (currentSolution.finished && _evalSessionFile != null)
            {
                Debug.Log("FINISSSSS");


                client.showHide();
                

                DateTime now = DateTime.Now;
                _evalSessionFile.WriteLine("ENDTIME=" + now.ToString("yy/MM/dd-H:mm:ss zzz"));

                TimeSpan elapsed = now.Subtract(startTime);

                _evalSessionFile.WriteLine("DURATION=" + elapsed.ToString());

                _evalSessionFile.WriteLine("WRONGSELECTIONS=" + currentSolution.wrongSelections);
                _evalSessionFile.WriteLine("WRONGMOVES=" + (currentSolution.wrongMoves - currentSolution.wrongSelections));

                _evalSessionFile.Close();
                _evalSessionFile = null;

                currentSolution = null;
            }
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            _showDebug = !_showDebug;
        }

        if(Input.GetKeyDown(KeyCode.S))
        {
            SkipStep();
        }
    }

    public bool IAmPointing(Ray ray, bool click, out Vector3 hitPoint)
    {

        hitPoint = Vector3.zero;
        bool didHit = false;

        if (!_init) return didHit;

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, float.PositiveInfinity))
        {
            //Debug.Log("Danieu diz: " + hit.transform.gameObject.name);

            didHit = true;
            hitPoint = hit.point;

            Highlight h = hit.transform.gameObject.GetComponent<Highlight>();
            if (h != null)
            {
                //Debug.Log(this.ToString() + "[IAmPointing]: " + h.name);
                h.setHighlight(true);
                if (_lastHighlighted != null && _lastHighlighted != h) _lastHighlighted.setHighlight(false);
                _lastHighlighted = h;
                _currentlyHighlighted = hit.transform.gameObject;
            }
        }
        else
        {
            if (_lastHighlighted != null) _lastHighlighted.setHighlight(false);
            _lastHighlighted = null;
        }

        if (click)
        {
            if (_currentlyHighlighted != null)
            {
                _selection(_currentlyHighlighted);
            }
        }
        _currentlyHighlighted = null;

        return didHit;
    }

    private bool isNextSelectionIfEvaluation(string objectName)
    {
        if (inAEvaluationSession && currentSolution != null)
        {
            string nextSelection = currentSolution.GetNextSelect();
            Debug.Log("[SELECTION] selected={" + objectName + "}, next={" + nextSelection + "}");
            return objectName == nextSelection;
        }
        return true;
    }

    private bool isNextMoveIfEvaluation(string that, string there)
    {
        if (inAEvaluationSession && currentSolution != null)
        {
            string nextThat = currentSolution.GetNextSelect();
            string nextThere = currentSolution.GetNextMove();
            return that == nextThat && there == nextThere;
        }
        return true;
    }

    private void _selection(GameObject o)
    {
        if (selectedObject != null) selectedObject.GetComponent<Highlight>().setSelected(false);

        if (selectedObject == null && isCube(o)) // SELECT OBJECT
        {
            if (inAEvaluationSession)
            {
                if (isNextSelectionIfEvaluation(o.name))
                {
                    // CORRECT SELECTION
                    _checkerboardLogger.write("[SELECTION] CORRECT");
                }
                else
                {
                    // WRONG SELECTION
                    _checkerboardLogger.write("[SELECTION] WRONG");
                    if (currentSolution != null) currentSolution.wrongSelections += 1;
                }
            }

            selectedObject = o;
            selectedObject.GetComponent<Highlight>().setSelected(true);
            if (inAEvaluationSession && currentSolution != null) currentSolution.Select(o.name);
           
        }
        else if (selectedObject != null) // MOVE OBJECT
        {
            selectedObject.GetComponent<Highlight>().setSelected(false);

            if (inAEvaluationSession)
            {
                if (isNextMoveIfEvaluation(selectedObject.name, o.name))
                {
                    // CORRECT 
                    _checkerboardLogger.write("[MOVE] CORRECT");
                }
                else
                {
                    // WRONG 
                    _checkerboardLogger.write("[MOVE] WRONG");
                    if (currentSolution != null) currentSolution.wrongMoves += 1;
                }
            }


            putObjectOnTopOf(selectedObject, o);
            if (inAEvaluationSession && currentSolution != null) currentSolution.Move(selectedObject.name, o.name);

            selectedObject = null;
        }
    }

    public void putObjectOnTopOf(string that, string there)
    {
        GameObject thatgo = GameObject.Find(that);
        GameObject therego = GameObject.Find(there);
        putObjectOnTopOf(thatgo, therego);
    }

    void putObjectOnTopOf(GameObject that, int thereWidth, int thereDepth)
    {
        if (thereWidth <= boardMap.GetLength(0) && thereWidth >= 0
            && thereDepth <= boardMap.GetLength(1) && thereDepth >= 0)
        {
            putObjectOnTopOf(that, boardMap[thereWidth, thereDepth]);
        }
    }

    void putObjectOnTopOf(GameObject that, GameObject there)
    {

        if (that == there) return;

        float d = moveUp;
        if (isCube(there))
        {
            d = 2*d;
        }

        that.GetComponent<CubeBehaviour>().onTopOf = there;
        that.GetComponent<CubeBehaviour>().GoTo(there.transform.position + d * (GameObject.Find("NegativeSpaceCenter").transform.up));

        if (_init && _main.location == Location.Assembler)
        {
            client.putObjectOnTopOf(that, there);
        }

        //Debug.Log(createInstruction(that, there));
    }

    public static bool isCube(GameObject v)
    {
        return v.name.EndsWith("Cube");
    }

    private bool isBoard(GameObject v)
    {
        return v.name.StartsWith("Box");
    }

    public string createInstruction(GameObject that, GameObject there)
    {
        if (!isCube(there))
        {
            string s = there.name.TrimStart("box(".ToCharArray());
            s = s.TrimEnd(')');
            string[] values = s.Split(',');

            return that.name + " " + values[0] + " " + values[1];
        }
        else return that.name + " " + there.name;
    }

    void __OnGUI()
    {
        int width = Screen.width / 10;
        int height = width / 2;
        int bezel = 10;

        Rect interactionArea = new Rect(Screen.width - width, 0, width, height);
        if (Input.mousePosition.x > interactionArea.x && Screen.height - Input.mousePosition.y < interactionArea.height)
        {
            GUI.Box(interactionArea, "");
            if (GUI.Button(new Rect(interactionArea.x + bezel, interactionArea.y + bezel, interactionArea.width - 2 * bezel, interactionArea.height - 2 * bezel), "Save"))
            {
                CubePositionsFileWriter f = new CubePositionsFileWriter();
                f.addLine(redCube.GetComponent<CubeBehaviour>().initPosition());
                f.addLine(greenCube.GetComponent<CubeBehaviour>().initPosition());
                f.addLine(blueCube.GetComponent<CubeBehaviour>().initPosition());
                f.addLine(yellowCube.GetComponent<CubeBehaviour>().initPosition());
                f.addLine(pinkCube.GetComponent<CubeBehaviour>().initPosition());

                bool saved;
                f.tryFlush(Application.dataPath + "/initPositions.txt", out saved);
                if (saved) Debug.Log("init positions saved");
            }
        }

        
    }

    private void OnGUI()
    {
        if (_showDebug)
        {
            int lineHeight = 25;
            int top = 10;
            GUI.Label(new Rect(10, top, 1000, lineHeight), "in EVALUATION: " + inAEvaluationSession);


            top += lineHeight;
            string[] lines = _checkerboardLogger.Lines;
            if (lines.Length > 0)
            {    
                int left = 10;
                foreach (string line in lines)
                {
                    GUI.Label(new Rect(left, top, 1000, lineHeight), line);
                    top += lineHeight;
                }
            }
            else
            {
                GUI.Label(new Rect(10, top, 1000, lineHeight), "No info to show!!!");
            }
        }
    }
}
