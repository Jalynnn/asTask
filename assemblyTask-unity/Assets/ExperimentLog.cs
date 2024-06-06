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

// Jalynn: LSL Requires this section
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
    public bool testing = true;
    public GameObject DebugLog;
    public bool toggleLog = false;

    // Jalynn: LSL Requires this section
    string StreamName = "LSL4Unity.Samples.SimpleCollisionEvent";
    string StreamType = "Markers";
    private StreamOutlet outlet;
    private string[] sample = { "" };
    private int eventNumber = 0; // TEMP

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
        DebugLog = GameObject.FindWithTag("DebugWindow");
        var rnd = new System.Random();

#if UNITY_EDITOR
        filePath = Application.dataPath + "/Records";
#else
        filePath = Application.persistentDataPath + "/Records";

#endif
        if (!Directory.Exists(filePath))
            Directory.CreateDirectory(filePath);

        // Make this GameObject persistent across scene loads.
        if (instance == this) DontDestroyOnLoad(transform.gameObject);
        // activate this for testing
        if (SceneManager.GetActiveScene().name != "Tutorial Video" && instance == this && testing)
        {
            SetParticipantNumber(rnd.Next(1, 60));
            if (DebugLog && toggleLog)
                DebugLog.SetActive(true);
        }

        // Jalynn: LSL Requires this section
        var hash = new Hash128();
        hash.Append(StreamName);
        hash.Append(StreamType);
        hash.Append(gameObject.GetInstanceID());
        StreamInfo streamInfo = new StreamInfo(StreamName, StreamType, 1, LSL.LSL.IRREGULAR_RATE,
            channel_format_t.cf_string, hash.ToString());
        outlet = new StreamOutlet(streamInfo);
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

        if (manager.experimentType == SceneDirector.ExperimentType.ExpB)
        {
            if (testing)
            {
                manager.schedule = manager.GetNumbersFromCSV(true, participantNumber);
            }
            else
            {
                manager.schedule = manager.GetNumbersFromCSV(false, participantNumber);
            }
        }
        if (manager.experimentType == SceneDirector.ExperimentType.Usability)
        {
            manager.schedule = manager.GetNumbersFromCSV(false, 111);

        }


        manager.participantID = participantNumber;
        UnityEngine.Debug.Log(manager.schedule[0]); // Jalynn: Added UnityEngine
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
    public void AddData(string category = "n/a", string action = "n/a", string step = "n/a", string errorType = "n/a", string expected = "n/a", string actual = "n/a")
    {
        Scene scene = SceneManager.GetActiveScene();
        string sceneName = scene.name;
        string[] splitSceneName = sceneName.Split('_');

        float miliS = time_s * 1000;
        int seconds = ((int)time_s % 60);
        int minutes = ((int)time_s / 60);
        string timeString = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, miliS);
        float timeSinceLastEvent = miliS - tempTime;

        // Jalynn: Changed all of the ";" to ","
        string newLine = participantNumber.ToString();
        if (splitSceneName.Length == 3)
        {
            newLine += "," + splitSceneName[0];
            newLine += "," + splitSceneName[1];
            newLine += "," + splitSceneName[2];
        }
        else
        {
            newLine += "," + sceneName;
            newLine += "," + "n/a";
            newLine += "," + "n/a";
        }
        newLine += "," + manager.trialNumber.ToString();
        newLine += "," + DateTime.Now.ToString("HH:mm.ss");
        newLine += "," + Mathf.Round(miliS).ToString();
        newLine += "," + category;
        newLine += "," + action;
        newLine += "," + errorType;
        newLine += "," + expected;
        newLine += "," + actual;
        newLine += "," + step;
        newLine += "," + Mathf.Round(timeSinceLastEvent).ToString();
        tempTime = miliS;
        using (writer = File.AppendText(filePath))
        {
            // Jalynn: LSL Requires this section
            // Every new line written should have an associated trigger event sent
            int nominal = NominalData(eventNumber, sceneName, category, action, errorType);
            lslStuff(nominal);
            newLine += "," + nominal.ToString();

            writer.WriteLine(newLine);
        }

        // Jalynn: Temp - Is LSL triggering every AddData
        eventNumber++;
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

        // Jalynn: Changed all of the ";" to ","
        if (counter == 1)
        {
            newLine = participantNumber.ToString() + ",";
        }

        if (splitSceneName.Length == 3)
        {
            newLine += splitSceneName[0];
            newLine += "," + splitSceneName[1];
            newLine += "," + splitSceneName[2];
        }
        else
        {
            newLine += sceneName;
            newLine += "," + "n/a";
            newLine += "," + "n/a";
        }
        newLine += "," + manager.shapeNumber.ToString();
        newLine += "," + trialNumber.ToString();
        newLine += "," + Mathf.Round(miliSW).ToString();
        newLine += "," + mistakesMade.ToString() + ",";

        string[] lines = File.ReadAllLines(filePathW);

        if (counter == 1)
        {
            lines[0] += "Participant_Number,Shape" + counter + ",Condition" + counter + ",Adaptivity" + counter + ",PositionInExp" + counter + ",Trial" + counter + ",TotalTime" + counter + ",MistakesMade" + counter + ",";
            UnityEngine.Debug.Log("Added Lines"); // Jalynn: Added UnityEngine.
        }
        else
        {
            lines[0] += "Shape" + counter + ",Condition" + counter + ",Adaptivity" + counter + ",PositionInExp" + counter + ",Trial" + counter + ",TotalTime" + counter + ",MistakesMade" + counter + ",";
            UnityEngine.Debug.Log("Added Lines 2"); // Jalynn: Added UnityEngine.
        }
        lines[^1] += newLine;
        counter++;
        File.WriteAllLines(filePathW, lines);

    }

    // Jalynn: LSL Requires this section
    public int NominalData(int eventNumber, string sceneName = "n/a", string category = "n/a", string action = "n/a", string errorType = "n/a")
    {
        UnityEngine.Debug.Log("Jalynn: This is the scene name: " + sceneName);
        string[] splitSceneName = sceneName.Split('_');
        string shape = splitSceneName[0];
        UnityEngine.Debug.Log("Jalynn: This is the shape: " + shape);

        int nominal = 222; // Default is error

        UnityEngine.Debug.Log("Jalynn: EventNumber: " + eventNumber);
        UnityEngine.Debug.Log("Jalynn: This is the category: " + category);
        UnityEngine.Debug.Log("Jalynn: This is the action: " + action);
        UnityEngine.Debug.Log("Jalynn: This is the errorType: " + errorType);

        switch (category, action, errorType)
        {
            case ("Trial", "complete", "n/a"):
                nominal = 10;
                break;
            case ("Trial", "continued", "n/a"):
                nominal = 20;
                break;
            case ("Trial", "loaded", "n/a"):
                nominal = 30;
                break;
            case ("Trial", "started", "n/a"):
                nominal = 40;
                break;
            case ("ShortBar", "Correct placement", "n/a"):
                nominal = 50;
                break;
            case ("ShortBar", "Error", "placement"):
                nominal = 60;
                break;
            case ("ShortBar", "Error", "length"):
                nominal = 70;
                break;
            case ("ShortBar", "Error", "color"):
                nominal = 80;
                break;
            case ("ShortBar", "Error", "distance"):
                nominal = 90;
                break;
            case ("MediumBar", "Correct placement", "n/a"):
                nominal = 100;
                break;
            case ("MediumBar", "Error", "placement"):
                nominal = 110;
                break;
            case ("MediumBar", "Error", "length"):
                nominal = 120;
                break;
            case ("MediumBar", "Error", "color"):
                nominal = 130;
                break;
            case ("MediumBar", "Error", "distance"):
                nominal = 140;
                break;
            case ("LongBar", "Correct placement", "n/a"):
                nominal = 150;
                break;
            case ("LongBar", "Error", "placement"):
                nominal = 160;
                break;
            case ("LongBar", "Error", "length"):
                nominal = 170;
                break;
            case ("LongBar", "Error", "color"):
                nominal = 180;
                break;
            case ("LongBar", "Error", "distance"):
                nominal = 190;
                break;
            case ("WaitingRoomManager", "Rest Start", "n/a"):
                nominal = 200;
                break;
            case ("WaitingRoomManager", "Rest Stop", "n/a"):
                nominal = 210;
                break;
            default:
                nominal = 0;
                break;
        }

        switch (shape)
        {
            case ("A"):
                nominal += 1;
                break;
            case ("B"):
                nominal += 2;
                break;
            case ("C"):
                nominal += 3;
                break;
            case ("D"):
                nominal += 4;
                break;
            case ("E"):
                nominal += 5;
                break;
            case ("F"):
                nominal += 6;
                break;
            case ("G"):
                nominal += 7;
                break;
            case ("H"):
                nominal += 8;
                break;
            case ("Practice"):
                nominal += 9;
                break;
            case ("PracticeColor"):
                nominal += 0;
                break;
        }

        sceneName = "n/a";
        category = "n/a";
        action = "n/a";
        errorType = "n/a";
        return nominal;
    }

    // Jalynn: LSL Requires this section
    public void lslStuff(int nominal)
    {
        UnityEngine.Debug.Log("Jalynn: This is the nominal: " + nominal.ToString());
        if (outlet != null)
        {
            sample[0] = nominal.ToString();
            outlet.push_sample(sample);
            UnityEngine.Debug.Log("Jalynn: Trigger sent");
        }
    }
}
