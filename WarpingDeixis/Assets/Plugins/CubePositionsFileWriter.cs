using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CubePositionsFileWriter {

    private List<string> _lines;

    public CubePositionsFileWriter()
    {
        _lines = new List<string>();
    }

    public void clear()
    {
        _lines.Clear();
    }

    public void addLine(string line)
    {
        _lines.Add(line);
    }

    public void tryFlush(string filename, out bool saved)
    {
        saved = false;
        if (_lines.Count > 0)
        {
            File.WriteAllLines(filename, _lines.ToArray());
            saved = true;
        }
    }
}
