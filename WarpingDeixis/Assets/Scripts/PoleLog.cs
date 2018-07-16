using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class PoleLog : MonoBehaviour {

    private bool __inSession__ = false;

    private string _filename;

    private string CSVSeparator = "#";

    public EvaluationConfigProperties _config;

    public ServerConsole console;

    public void StartRecordingSession()
    {

        if (_config.Peer != EvaluationPeer.SERVER) return;

        _filename = Application.dataPath + System.IO.Path.DirectorySeparatorChar + "Recordings" + System.IO.Path.DirectorySeparatorChar;
        if (!Directory.Exists(_filename))
            Directory.CreateDirectory(_filename);

        _filename += "Pole_" + DateTime.Now.ToString("yyyy MMMM d H-mm-ss") + ".txt";
        if (!File.Exists(_filename))
            File.Create(_filename).Dispose();

        //console.writeLine("Saving PoleLogs to " + "Pole_" + DateTime.Now.ToString("yyyy MMMM d H-mm-ss") + ".txt");

        string header = "";

        header += "TIMESTAMP" + CSVSeparator;
        header += "TASK" + CSVSeparator;
        header += "CONDITION" + CSVSeparator;
        header += "OBSERVER" + CSVSeparator;
        header += "TARGET" + CSVSeparator;
        header += "OBSERVED_TARGET" + CSVSeparator;
        header += "ERROR_DISTANCE" + CSVSeparator;

        _writeLine(header, _filename);

        __inSession__ = true;
    }

    public void Record(int task, string condition, string observer, int target, int observedPoleTarget)
    {
        if (!__inSession__) return;

        string line = "";

        line += DateTime.Now.ToString("yyyy MMMM d H-mm-ss.fffff") + CSVSeparator;
        line += task + CSVSeparator;
        line += condition + CSVSeparator;
        line += observer + CSVSeparator;
        line += target + CSVSeparator;
        line += observedPoleTarget + CSVSeparator;

        Vector3 pTarget = GameObject.Find("pole" + target).transform.position;
        Vector3 oTarget = GameObject.Find("pole" + observedPoleTarget).transform.position;

        line += Vector3.Distance(pTarget, oTarget);

        console.writeLine("target = " + target);
        console.writeLine("observer target = " + observedPoleTarget);
        console.writeLine("error distance = " + Vector3.Distance(pTarget, oTarget));

        _writeLine(line, _filename);
    }

    public void EndRecordingSession()
    {
        _filename = null;
        __inSession__ = false;
    }

    private void _writeLine(string line, string file)
    {
        using (StreamWriter w = File.AppendText(file))
        {
            w.WriteLine(line);
            w.Close();
        }
    }

}
