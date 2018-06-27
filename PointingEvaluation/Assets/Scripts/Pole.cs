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

	void Start ()
    {
        poleCells = new List<GameObject>();
        List<int> numbers = new List<int>();

        numberOfPoleCells = (int) Mathf.Floor((h1 - h0) / poleCellSize);
        for (int i = 1; i <= numberOfPoleCells; i++)
        {
            numbers.Add(i);

            GameObject cell = Instantiate(poleCellPrefab, transform, false);
            
            cell.transform.localScale = new Vector3(poleCellSize, poleCellSize, poleCellSize);
            cell.transform.localPosition = Vector3.zero;
            cell.transform.localPosition = cell.transform.up * h0 + cell.transform.up * (poleCellSize * i - poleCellSize / 2);
            cell.GetComponent<PoleNumber>().Number = numberOfPoleCells - i;
            cell.GetComponent<PoleNumber>().textMesh.text = "" + (numberOfPoleCells - i);
            cell.name = "pole" + i;
            poleCells.Add(cell);
        }
    }
	
	void Update () {
		
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
