using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Unity.Mathematics;
using Unity.Collections;

public class Patterns 
{
    public List<HashSet<int2>> list = new List<HashSet<int2>>();

    public Patterns()
    {
        string path = "Assets/Resources/Patterns";
        string[] files = Directory.GetFiles(path, "*.txt");
        foreach (string sourceFile in files)
        {
            string fileName = Path.GetFileName(sourceFile);
            list.Add(getPatternFromFile("Resources/Patterns/" + fileName));
        }
    }

    //***************************
    // getPatternFromFile
    // - Opens text file in Resources/Patterns folder and returns the pattern
    //***************************
    static HashSet<int2> getPatternFromFile(string filepath)
    {
        HashSet<int2> returnPattern = new HashSet<int2>();

        var sr = new StreamReader(Application.dataPath + "/" + filepath);
        var fileContents = sr.ReadToEnd();
        sr.Close();

        var lines = fileContents.Split("\n"[0]);


        int row = 0;
        foreach (string line in lines)
        {
            int col = 0;
            foreach (char c in line)
            {
                if (c.ToString() == "x")
                {
                    returnPattern.Add(new int2(row, col));
                }
                col++;
            }
            row++;
        }

        return returnPattern;
    }

}
