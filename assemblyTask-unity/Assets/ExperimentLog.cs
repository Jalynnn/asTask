using System;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEditor.Build.Content;

// Jalynn: LSL requires
using LSL;
using System.Diagnostics;

public class ExperimentLog : MonoBehaviour
{
    public static ExperimentLog instance;
    public string filePath;
    public string filePathW;
    public int participantNumber = 0;
    StreamWriter writer;
    StreamWriter writerW;
    SceneDirector manager;
    public float time_s = 0;
    float tempTime = 0f;
    int counter = 1;

    // Jalynn: LSL Requires
    string StreamName = "LSL4Unity.Samples.SimpleCollisionEvent";
    string StreamType = "Markers";
    private StreamOutlet outlet;
    private string[] sample = {""};
    private int[] samples = {0}; // TEMP

    // Start is called before the first frame update
    void Start()
    {
        // Ensure that only one instance of ExperimentLog exists.
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            //Debug.Log("Destroyed"+SceneManager.GetActiveScene().name);
        }
        else
        {
            instance = this;
        }

        manager = this.gameObject.GetComponent<SceneDirector>();
        var rnd = new System.Random();
        filePath = Application.dataPath + "/Records";
        if (!Directory.Exists(filePath))
            Directory.CreateDirectory(filePath);

        // Make this GameObject persistent across scene loads.
        if (instance == this) DontDestroyOnLoad(transform.gameObject);
// activate this for testing
        //if (SceneManager.GetActiveScene().name != "Tutorial Video" && instance == this)
          //  SetParticipantNumber(rnd.Next(1000, 9999)); 
    }
    // Update is called once per frame
    void Update()
    {
        time_s += Time.deltaTime;

    }
    // This method sets the participant number and initializes the log files.
    public void SetParticipantNumber(int pNum)
    {

        participantNumber = pNum;
        string temp = filePath;
        filePath = filePath + "/Participant" + participantNumber.ToString() + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmssf") + ".csv";
        filePathW = temp + "/WideParticipant" + participantNumber.ToString() + "_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".csv";
        using (writer = File.CreateText(filePath))
        {
            writer.WriteLine("Participant_Number;Shape;Condition;Adaptivity;Trial;Timestamp;Time_in_trial;Category;Action;Errortype;Expected;Actual;Step;TimeSinceLastEvent");
        }
        using (writerW = File.CreateText(filePathW))
        {
            //Debug.Log("Creating Wide File");
            writerW.WriteLine("");
            writerW.WriteLine("");
        }

    }
    // This method adds a new line to the log file.
    public void AddData(string category = "n/a", string action = "n/a", string step = "n/a", string errorType = "n/a",string expected = "n/a", string actual = "n/a")
    {
        Scene scene = SceneManager.GetActiveScene();
        string sceneName = scene.name;
        string[] splitSceneName = sceneName.Split('_');

        float miliS = time_s * 1000;
        int seconds = ((int)time_s % 60);
        int minutes = ((int)time_s / 60);
        string timeString = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, miliS);
        float timeSinceLastEvent = miliS - tempTime;

        string newLine = participantNumber.ToString();
        if (splitSceneName.Length == 3)
        {
            newLine += ";" + splitSceneName[0];
            newLine += ";" + splitSceneName[1];
            newLine += ";" + splitSceneName[2];
        }
        else
        {
            newLine += ";" + sceneName;
            newLine += ";" + "n/a";
            newLine += ";" + "n/a";
        }
        newLine += ";" + manager.trialNumber.ToString();
        newLine += ";" + DateTime.Now.ToString("HH:mm.ss");
        newLine += ";" + Mathf.Round(miliS).ToString();
        newLine += ";" + category;
        newLine += ";" + action;
        newLine += ";" + errorType;
        newLine += ";" + expected;
        newLine += ";" + actual;
        newLine += ";" + step;
        newLine += ";" + Mathf.Round(timeSinceLastEvent).ToString();
        tempTime = miliS;
        using (writer = File.AppendText(filePath))
        {
            writer.WriteLine(newLine);
        }

        // Jalynn: LSL Required
        int nominal = NominalData(sceneName, category, action, errorType);
        lslStuff(nominal);
    }

    // This method adds a new line to the wide log file. Francisco wanted this as a sort of summary of the experiment data.
    public void AddWideData(int trialNumber, int mistakesMade)
    {
        //        Debug.Log("Adding Wide Data");
        //Participant_Number;Shape;Condition;Adaptivity;PositionInExp;Trial;TotalTime;MistakesMade";
        Scene scene = SceneManager.GetActiveScene();
        string sceneName = scene.name;
        string[] splitSceneName = sceneName.Split('_');
        string newLine = "";
        float miliSW = time_s * 1000;
        if (counter == 1)
        {
            newLine = participantNumber.ToString() + ";";
        }

        if (splitSceneName.Length == 3)
        {
            newLine += splitSceneName[0];
            newLine += ";" + splitSceneName[1];
            newLine += ";" + splitSceneName[2];
        }
        else
        {
            newLine += sceneName;
            newLine += ";" + "n/a";
            newLine += ";" + "n/a";
        }
        newLine += ";" + manager.shapeNumber.ToString();
        newLine += ";" + trialNumber.ToString();
        newLine += ";" + Mathf.Round(miliSW).ToString();
        newLine += ";" + mistakesMade.ToString() + ";";

        string[] lines = File.ReadAllLines(filePathW);

        if (counter == 1)
        {
            lines[0] += "Participant_Number;Shape" + counter + ";Condition" + counter + ";Adaptivity" + counter + ";PositionInExp" + counter + ";Trial" + counter + ";TotalTime" + counter + ";MistakesMade" + counter + ";";
            UnityEngine.Debug.Log("Added Lines"); // Jalynn: Added UnityEngine.
        }
        else
        {
            lines[0] += "Shape" + counter + ";Condition" + counter + ";Adaptivity" + counter + ";PositionInExp" + counter + ";Trial" + counter + ";TotalTime" + counter + ";MistakesMade" + counter + ";";
            UnityEngine.Debug.Log("Added Lines 2"); // Jalynn: Added UnityEngine.
        }
        lines[^1] += newLine;
        counter++;
        File.WriteAllLines(filePathW, lines);

    }

    // Jalynn: LSL Required
    public int NominalData(string sceneName = "n/a", string category = "n/a", string action = "n/a", string errorType = "n/a")
    {
        UnityEngine.Debug.Log("This is the scene name: " + sceneName);
        string[] splitSceneName = sceneName.Split('_');
        string shape = splitSceneName[0];
        UnityEngine.Debug.Log("This is the shape: " + shape);
        /*switch (shape)
        {
            case ("A"): return 1;
            case ("B"): return 2;
            case ("C"): return 3;
            case ("D"): return 4;
            case ("E"): return 5;
            case ("F"): return 6;
            case ("G"): return 7;
            case ("H"): return 8;
            case("Practice"): return 9;
            case("PracticeColor"): return 99;
        }*/

        UnityEngine.Debug.Log("This is the category: " + category);
        UnityEngine.Debug.Log("This is the action: " + action);
        UnityEngine.Debug.Log("This is the errorType: " + errorType);
        switch (category, action, errorType)
        {
            case ("Trial", "complete", "n/a"):
                return 10;
            case ("Trial", "continued", "n/a"):
                return 20;
            case ("Trial", "loaded", "n/a"):
                return 30;
            case ("Trial", "started", "n/a"):
                return 40;
            case ("ShortBar", "Correct placement", "n/a"):
                return 50;
            case ("ShortBar", "Error", "placement"): // distance
                return 60;
            case ("ShortBar", "Error", "length"):
                return 70;
            case ("ShortBar", "Error", "color"):
                return 80;
            case ("MediumBar", "Correct placement", "n/a"):
                return 90;
            case ("MediumBar", "Error", "placement"):
                return 100;
            case ("MediumBar", "Error", "length"):
                return 110;
            case ("MediumBar", "Error", "color"):
                return 120;
            case ("LongBar", "Correct placement", "n/a"):
                return 130;
            case ("LongBar", "Error", "placement"):
                return 140;
            case ("LongBar", "Error", "length"):
                return 150;
            case ("LongBar", "Error", "color"):
                return 160;
        }

        return 222; // Error
    }

    // Jalynn: LSL Required
    public void lslStuff(int nominal) {
        UnityEngine.Debug.Log("This is the nominal: " + nominal.ToString());
        if (outlet != null)
        {
            UnityEngine.Debug.Log("Outlet doesn't equal null");
            // Original: Will I still have problems since I am using .ToString()?
            sample[0] = nominal.ToString();
            outlet.push_sample(sample);

            // What if we just try without the conversion
            // outlet.push_sample(nominal);

            // Or maybe it has to be in an []
            // samples[0] = nominal;
            // outlet.push_sample(samples);
        }
    }

}
