using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class Logger// : MonoBehaviour
{

    private static int i = 0;
    public static void save(string filename, string[] lines)
    {
        if (!File.Exists(filename))
        {
            File.WriteAllLines(filename, lines);
            i = 0;
        }
        else
        {
            i++;
            save(filename + i, lines);
        }
    }
}
