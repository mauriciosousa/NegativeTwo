using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class WallLog : MonoBehaviour {

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

        _filename += "Wall_" + DateTime.Now.ToString("yyyy MMMM d H-mm-ss") + ".txt";
        if (!File.Exists(_filename))
            File.Create(_filename).Dispose();

        //console.writeLine("Saving WallLogs to " + "Wall_" + DateTime.Now.ToString("yyyy MMMM d H-mm-ss") + ".txt");

        string header = "";

        header += "TIMESTAMP" + CSVSeparator;
        header += "TASK" + CSVSeparator;
        header += "CONDITION" + CSVSeparator;
        header += "OBSERVER" + CSVSeparator;

        header += "TARGET_X" + CSVSeparator;
        header += "TARGET_Y" + CSVSeparator;
        header += "TARGET_Z" + CSVSeparator;

        header += "CURSOR_X" + CSVSeparator;
        header += "CURSOR_Y" + CSVSeparator;
        header += "CURSOR_Z" + CSVSeparator;

        header += "DIST_TARGET_CURSOR" + CSVSeparator;

        _writeLine(header, _filename);

        __inSession__ = true;
    }

    public void Record(int task, string condition, string observer, Vector3 target, Vector3 cursor)
    {
        if (!__inSession__) return;

        string line = "";

        line += DateTime.Now.ToString("yyyy MMMM d H-mm-ss.fffff") + CSVSeparator;
        line += task + CSVSeparator;
        line += condition + CSVSeparator;
        line += observer + CSVSeparator;
        line += target.x + CSVSeparator + target.y + CSVSeparator + target.z + CSVSeparator;
        line += cursor.x + CSVSeparator + cursor.y + CSVSeparator + cursor.z + CSVSeparator;
        line += Vector3.Distance(target, cursor);


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
