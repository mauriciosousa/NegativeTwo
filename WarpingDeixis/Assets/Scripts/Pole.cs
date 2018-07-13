using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using UnityEngine;


public class Pole : MonoBehaviour {

    public GameObject poleCellPrefab;

    public float poleCellSize; // (4cm)
    public float h0; // bottom (32cm)
    public float h1; // top (284 cm)

    public int numberOfPoleCells;

    private List<GameObject> poleCells;

    public int task;
    public bool observer;

    public DeixisEvaluation deixisEvaluation;
    private EvaluationConfigProperties _config;
    private EvaluationPeer peer;

    public Transform leftHumanPosition;
    public Transform rightHumanPosition;

    void Start ()
    {
        poleCells = new List<GameObject>();

        _config = deixisEvaluation.GetComponent<EvaluationConfigProperties>();
        peer = _config.Peer;
    }



    public void createAPole(int task, float distance, int target, bool observer, WarpingCondition condition)
    {
        transform.position = Vector3.zero;

        EvaluationPeer pointer = deixisEvaluation.getObserver(task);
        Transform t;
        if (pointer == EvaluationPeer.LEFT) t = leftHumanPosition;
        else t = rightHumanPosition;
        Vector3 pos = transform.position;
        transform.position = new Vector3(t.position.x, pos.y, pos.z);


        if (peer == EvaluationPeer.SERVER) observer = _config.serverIsObserver;
        

        distance += GameObject.Find("leftHumanPosition").transform.position.z; 

        print("Creating Pole for " + task.ToString() + " at " + distance);

        if (poleCells.Count > 0) destroyCurrent();

        poleCells = new List<GameObject>();

        this.task = task;
        this.observer = observer;

        numberOfPoleCells = (int)Mathf.Floor((h1 - h0) / poleCellSize);
        for (int i = 1; i <= numberOfPoleCells; i++)
        {
            GameObject cell = Instantiate(poleCellPrefab, transform, false);

            cell.transform.localScale = new Vector3(poleCellSize, poleCellSize, poleCellSize);
            cell.transform.localPosition = Vector3.zero;
            cell.transform.localPosition = cell.transform.up * h0 + cell.transform.up * (poleCellSize * i - poleCellSize / 2);

            int index = numberOfPoleCells - i + 1;

            cell.GetComponent<PoleNumber>().Number = index;

            //try
            {
                cell.GetComponent<PoleNumber>().textMesh.text = "" + PoleNumbersDics.translate(index, task, condition);
            }
            //catch (Exception e)
            {
            //    Debug.Log(e.StackTrace);
            }
                //cell.GetComponent<PoleNumber>().textMesh.text = "" + index;
            cell.name = "pole" + index;

            PoleNumber pl = cell.GetComponent<PoleNumber>();
            if (observer)
            {
                pl.state = PoleCellStateType.OBSERVER_SIDE;
            }
            else pl.state = PoleCellStateType.POINTER_SIDE;

            if (!observer && index == target)
            {
                pl.state = PoleCellStateType.POINTER_SIDE_TARGET;
            }

            if (observer && index == target && peer == EvaluationPeer.SERVER)
            {
                Debug.Log("indicator true for target " + index);
                pl.showIndicator();
            }

            poleCells.Insert(0, cell);
        }
        transform.position += Vector3.forward * distance;
    }

    public void destroyCurrent()
    {
        foreach (GameObject o in poleCells)
        {
            Destroy(o);
        }
        poleCells = new List<GameObject>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F9))
        {
            string filep = Application.dataPath + "/out.txt";

            List<string> lines = new List<string>();

            List<int> numbers = new List<int>();
            foreach (GameObject go in poleCells)
            {
                numbers.Add(go.GetComponent<PoleNumber>().Number);
            }

            string line = "";
            foreach (int n in numbers)
            {
                line += " " + n;
            }
            lines.Add(line);

            for (int i = 1; i <= 5; i++)
            {
                numbers.Shuffle();
                line = "{ ";
                for (int n = 0; n < numbers.Count; n++)
                {
                    line += "{" + (n + 1) + ", " + numbers[n] + "},";
                }
                line = line.Remove(line.Length - 1);
                line += "};";
                lines.Add(line);
            }
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filep, false))
            {
                foreach (string l in lines)
                {
                    print(l);
                    file.WriteLine(l);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.F10))
        {
            List<string> lines = new List<string>();

            lines.Add("Pole Number, LeftObserverBaseline, RightObserverBaseline, LeftObserverWarp, RightObserverWarp");

            int n = PoleNumbersDics.shuffledA.Count;

            for (int i = 1; i <= n; i++)
            {
                int sA = PoleNumbersDics.shuffledA[i];
                int sB = PoleNumbersDics.shuffledB[i];
                int sC = PoleNumbersDics.shuffledC[i];
                int sD = PoleNumbersDics.shuffledD[i];

                string line;
                line = "" + i + "#";
                line += "" + sA + "#";
                line += "" + sB + "#";
                line += "" + sC + "#";
                line += "" + sD;

                print(line);

                lines.Add(line);
            }

            string filep = Application.dataPath + "/conversion-sheet.txt";

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filep, false))
            {
                foreach (string l in lines)
                {
                    file.WriteLine(l);
                }
            }
        }
    }
}

public static class ThreadSafeRandom
{
    [ThreadStatic] private static System.Random Local;

    public static System.Random ThisThreadsRandom
    {
        get { return Local ?? (Local = new System.Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
    }
}

static class MyExtensions
{
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}

public static class PoleNumbersDics
{
    public static Dictionary<int, int> shuffledA = new Dictionary<int, int>()
    {
        {1, 9},{2, 27},{3, 11},{4, 34},{5, 15},{6, 23},{7, 13},{8, 29},{9, 8},{10, 31},{11, 37},{12, 21},{13, 20},{14, 32},{15, 4},{16, 19},{17, 26},{18, 30},{19, 5},{20, 35},{21, 6},{22, 1},{23, 33},{24, 24},{25, 7},{26, 18},{27, 28},{28, 3},{29, 16},{30, 10},{31, 22},{32, 36},{33, 14},{34, 2},{35, 12},{36, 17},{37, 25}
    };
    
    public static Dictionary<int, int> shuffledB = new Dictionary<int, int>()
    {
        {1, 6},{2, 11},{3, 25},{4, 18},{5, 36},{6, 1},{7, 13},{8, 16},{9, 8},{10, 29},{11, 26},{12, 14},{13, 17},{14, 15},{15, 10},{16, 4},{17, 32},{18, 5},{19, 27},{20, 9},{21, 37},{22, 3},{23, 34},{24, 30},{25, 35},{26, 23},{27, 28},{28, 31},{29, 22},{30, 7},{31, 24},{32, 33},{33, 21},{34, 19},{35, 12},{36, 2},{37, 20}
    };

    public static Dictionary<int, int> shuffledC = new Dictionary<int, int>()
    {
        {1, 33},{2, 30},{3, 6},{4, 32},{5, 31},{6, 23},{7, 36},{8, 3},{9, 25},{10, 1},{11, 17},{12, 18},{13, 21},{14, 22},{15, 7},{16, 15},{17, 28},{18, 4},{19, 20},{20, 13},{21, 14},{22, 10},{23, 16},{24, 12},{25, 24},{26, 37},{27, 11},{28, 19},{29, 35},{30, 29},{31, 9},{32, 8},{33, 2},{34, 34},{35, 5},{36, 27},{37, 26}
    };

    public static Dictionary<int, int> shuffledD = new Dictionary<int, int>()
    {
        {1, 21},{2, 23},{3, 19},{4, 20},{5, 27},{6, 37},{7, 25},{8, 28},{9, 1},{10, 22},{11, 7},{12, 18},{13, 12},{14, 26},{15, 30},{16, 24},{17, 14},{18, 6},{19, 15},{20, 17},{21, 13},{22, 10},{23, 34},{24, 5},{25, 16},{26, 33},{27, 29},{28, 4},{29, 32},{30, 35},{31, 3},{32, 31},{33, 11},{34, 8},{35, 36},{36, 2},{37, 9}
    };

    internal static int translate(int index, int trial, WarpingCondition condition)
    {

        if (new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 }.Contains(trial) && condition == WarpingCondition.BASELINE)
        {
            return shuffledA[index];
        }

        if (new List<int>() { 15, 16, 17, 18, 19, 20, 21, 22, 23 }.Contains(trial) && condition == WarpingCondition.BASELINE)
        {
            return shuffledB[index];
        }

        if (new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 }.Contains(trial) && condition == WarpingCondition.WARPING)
        {
            return shuffledC[index];
        }

        if (new List<int>() { 15, 16, 17, 18, 19, 20, 21, 22, 23 }.Contains(trial) && condition == WarpingCondition.WARPING)
        {
            return shuffledD[index];
        }

        return -1;
    }
}

