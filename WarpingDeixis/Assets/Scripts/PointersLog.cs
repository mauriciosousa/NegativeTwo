using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class PointersLog : MonoBehaviour
{

    private bool __inSession__ = false;

    private string _filename;

    private string CSVSeparator = "#";

    public EvaluationConfigProperties _config;

    public ServerConsole console;

    private List<string> _lines;

    private DateTime _lastRecording;

    private bool _logPointer = false;

    void Start()
    {
        _logPointer = _config.LogPointer;
    }

    public void StartRecordingSession(int trial)
    {
        if (_config.Peer != EvaluationPeer.SERVER) return;

        if (!_logPointer) return;

        _lines = new List<string>();

        _filename = Application.dataPath + System.IO.Path.DirectorySeparatorChar + "Recordings" + System.IO.Path.DirectorySeparatorChar;
        if (!Directory.Exists(_filename))
            Directory.CreateDirectory(_filename);

        _filename += "PointersLog_" + trial + "_" + DateTime.Now.ToString("yyyy MMMM d H-mm-ss") + ".txt";
        if (!File.Exists(_filename))
            File.Create(_filename).Dispose();

        string header = "";

        header += "TIMESTAMP" + CSVSeparator;
        header += "TASK" + CSVSeparator;
        header += "CONDITION" + CSVSeparator;

        header += "POINTING_ARM" + CSVSeparator;

        header += "HEAD_X" + CSVSeparator;
        header += "HEAD_Y" + CSVSeparator;
        header += "HEAD_Z" + CSVSeparator;

        header += "ELBOW_X" + CSVSeparator;
        header += "ELBOW_Y" + CSVSeparator;
        header += "ELBOW_Z" + CSVSeparator;

        header += "HANDTIP_X" + CSVSeparator;
        header += "HANDTIP_Y" + CSVSeparator;
        header += "HANDTIP_Z" + CSVSeparator;

        header += "pointerHeadIndexHit_X" + CSVSeparator;
        header += "pointerHeadIndexHit_Y" + CSVSeparator;
        header += "pointerHeadIndexHit_Z" + CSVSeparator;

        header += "pointerElbowIndexHit_X" + CSVSeparator;
        header += "pointerElbowIndexHit_Y" + CSVSeparator;
        header += "pointerElbowIndexHit_Z" + CSVSeparator;

        _lines.Add(header);

        _lastRecording = DateTime.Now;

        //console.writeLineRed("Recording...Start");

        __inSession__ = true;
    }

    public void Record(int task, string condition, PointingArm arm, Vector3 head, Vector3 elbow, Vector3 handtip, Vector3 pointerHeadIndexHit, Vector3 pointerElbowIndexHit, float pointerHeadIndexHit_toTarget, float pointerElbowIndexHit_toTarget)
    {
        if (!_logPointer) return;

        if (!__inSession__) return;


        string line = "";

        if (_lastRecording.AddMilliseconds(500) > DateTime.Now)
        {

            line += DateTime.Now.ToString("yyyy MMMM d H-mm-ss.fffff") + CSVSeparator;
            line += task + CSVSeparator;
            line += condition + CSVSeparator;
            line += arm.ToString() + CSVSeparator;

            line += head.x + CSVSeparator;
            line += head.y + CSVSeparator;
            line += head.z + CSVSeparator;

            line += elbow.x + CSVSeparator;
            line += elbow.y + CSVSeparator;
            line += elbow.z + CSVSeparator;

            line += handtip.x + CSVSeparator;
            line += handtip.y + CSVSeparator;
            line += handtip.z + CSVSeparator;

            line += pointerHeadIndexHit.x + CSVSeparator;
            line += pointerHeadIndexHit.y + CSVSeparator;
            line += pointerHeadIndexHit.z + CSVSeparator;

            line += pointerElbowIndexHit.x + CSVSeparator;
            line += pointerElbowIndexHit.y + CSVSeparator;
            line += pointerElbowIndexHit.z + CSVSeparator;

            line += pointerHeadIndexHit_toTarget + CSVSeparator;
            line += pointerElbowIndexHit_toTarget + CSVSeparator;
            

            _lines.Add(line);

            _lastRecording = DateTime.Now;
        }
    }

    public void EndRecordingSession()
    {
        if (!_logPointer) return;

        File.WriteAllLines(_filename, _lines.ToArray(), Encoding.UTF8);

        //console.writeLineRed("Recording...End");

        _lines.Clear();
        _filename = null;
        __inSession__ = false;
    }
}