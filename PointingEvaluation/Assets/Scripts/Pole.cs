using System.Collections.Generic;
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

        numberOfPoleCells = (int) ((h1 - h0) / poleCellSize);
        for (int i = 0; i < numberOfPoleCells; i++)
        {
            GameObject cell = Instantiate(poleCellPrefab, transform, false);
            
            cell.transform.localScale = new Vector3(poleCellSize, poleCellSize, poleCellSize);
            cell.transform.position += cell.transform.up * poleCellSize * (i + 1);
            cell.GetComponent<PoleNumber>().textMesh.text = "" + i + 1;
        }

        print("CELLLLSSSSS  " + numberOfPoleCells);
	}
	
	void Update () {
		
	}
}
