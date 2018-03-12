using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Highlight : MonoBehaviour {

    public Material normalMaterial;
    public Material highligtedMaterial;
    public Material selectedMaterial;

    public bool selected { get; private set; }
    public bool highlighted { get; private set; }

    void Start ()
    {
        highlighted = false;
        selected = false;
	}
	
	void Update ()
    {
		
	}

    public void setHighlight(bool setValue)
    {
        if (!selected)
        {
            Material m = setValue ? highligtedMaterial : normalMaterial;
            GetComponent<Renderer>().material = m;
            highlighted = setValue;
        }
    }

    internal void setSelected(bool v)
    {
        selected = v;
        Material m = v ? selectedMaterial : normalMaterial;
        GetComponent<Renderer>().material = m;
    }

    internal void setMaterials(Material colorBlindMaterial, Material colorBlindHighlightMaterial, Material colorBlindSelected)
    {
        normalMaterial = colorBlindMaterial;
        highligtedMaterial = colorBlindHighlightMaterial;
        selectedMaterial = colorBlindSelected;
        GetComponent<Renderer>().material = normalMaterial;
    }
}
