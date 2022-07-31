using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "tipsList", menuName = "tips")]
public class TipsList : ScriptableObject
{
    public List<string> Tips;

    [ContextMenu("record")]
    public void Record()
    {
        string path = Path.Combine(Application.persistentDataPath, "Content/Texts.txt");

        StreamWriter writer = new StreamWriter(path, false);
       
        writer.WriteLine(String.Concat(Tips.Select(t=>t + "\n")));
        writer.Close();

        BuddaSettings bs = new BuddaSettings(); 
    }
}
