using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class Solution
{
    public string Name { get; private set; }

    private Dictionary<int, string> _solutionThat;
    private Dictionary<int, string> _solutionThere;

    private int Steps { get { return _solutionThat.Count; } }

    public int Step { get; private set; }

    public bool finished = false;

    public int wrongSelections;
    public int wrongMoves;

    public Solution(string filename)
    {
        _solutionThat = new Dictionary<int, string>();
        _solutionThere = new Dictionary<int, string>();

        wrongSelections = 0;
        wrongMoves = 0;

    string[] dirs = filename.Split(Path.DirectorySeparatorChar);
        Name = dirs[dirs.Length - 1].Split('.')[0];
        if (File.Exists(filename))
        {
            string[] lines = File.ReadAllLines(filename);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(" ontopof "))
                {
                    string[] s = lines[i].Split(' ');
                    _solutionThat[i] = s[0];
                    _solutionThere[i] = s[2];
                }
            }
            Debug.Log(this.ToString() + ": new solution " + Name);
        }

        if (_solutionThat.Count > 0)
        {
            for (int i = 0; i < _solutionThat.Count; i++)
            {
                //Debug.Log("" + i + ". " + _solutionThat[i] + " -> " + _solutionThere[i]);
            }
        }



        reset();
    }

    public string GetNextSelect()
    {
        if (finished) return "None";
        return _solutionThat[Step];
    }

    public string GetNextMove()
    {
        if (finished)
        {
            return "None";
        }
        return _solutionThere[Step];
    }

    public void reset()
    {
        Step = 0;
        finished = false;
    }

    public bool Select(string that)
    {
        if (that == GetNextSelect())
        {
            return true;
        }

        return false;
    }

    public bool Move(string that, string there)
    {
        if (Select(that) && there == GetNextMove())
        {
            Step += 1;
            if (Step == Steps)
            {

                Debug.Log("FINISSSSSSSSSSSSSSSSSSSSSS");
                finished = true;
                // END Session

            }
            return true;
        }
        return false;
    }
}

/*
public class SolutionEvaluator : MonoBehaviour
{

    private string _solutionsFolder;

    private List<Solution> _solutions;

    void Start()
    {
        _solutionsFolder = Application.dataPath + "/Solutions/";
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

        string solname = "1";
        Solution solution = GetSolutionByName(solname);
        if (solution == null)
        {
            Debug.LogError(this.ToString() + ": no such solution " + solname);
        }
        else
        {
            solution.reset();

            //Debug.Log(solution.GetNextSelect());
            Debug.Log(solution.Select("RedCube"));
            Debug.Log(solution.Move("RedCube", "box(5,3)"));

            //Debug.Log(solution.GetNextSelect());
            Debug.Log(solution.Select("GreenCube"));
            Debug.Log(solution.Move("GreenCube", "box(4,0)"));

            //Debug.Log(solution.GetNextSelect());
            Debug.Log(solution.Select("BlueCube"));
            Debug.Log(solution.Move("BlueCube", "box(4,4)"));

            //Debug.Log(solution.GetNextSelect());
            Debug.Log(solution.Select("BlueCube"));
            Debug.Log(solution.Select("YellowCube"));
            Debug.Log(solution.Move("YellowCube", "box(4,2)"));

            //Debug.Log(solution.GetNextSelect());
            Debug.Log(solution.Select("YellowCube"));
            Debug.Log(solution.Move("YellowCube", "box(4,3)"));

            //Debug.Log(solution.Step);
            //Debug.Log(solution.GetNextSelect());
            Debug.Log(solution.Select("PinkCube"));
            Debug.Log(solution.Move("PinkCube", "box(5,1)"));

            Debug.Log(solution.Select("PinkCube"));
            Debug.Log(solution.Move("PinkCube", "box(5,5)"));

            Debug.Log(solution.Select("PinkCube"));
            Debug.Log(solution.Move("PinkCube", "box(5,1)"));

            // assert 5 falses
        }

    }

    public Solution GetSolutionByName(string name)
    {
        foreach (Solution s in _solutions)
        {
            if (s.Name == name) return s;
        }
        return null;
    }
    
}
*/

