using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    public enum Options
    {
        WaitingRoom,
        Shape
    }
    [SerializeField]
    public Options dropdown;
    public TextMeshPro progress;
    public float waitingTime;
    GameObject managerObj;
    SceneDirector sceneDirector;

    // Jalynn: Talk to experimenter before timer
    public bool updated = false;

    // Jalynn: Experiment Log
    public GameObject manager;

    // Start is called before the first frame update
    void Start()
    {
        // Jalynn: Talk to experimenter before timer
        // StartCoroutine(updateBar());

        // Jalynn: Experiment Log
        manager = GameObject.FindWithTag("Manager");
    }

    // Update is called once per frame
    void Update()
    {
        // Jalynn: Talk to experimenter before timer - Experimenter has to press space bar
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (updated == false) // Only can start timer once
            {
                StartCoroutine(updateBar());
                updated = true;
            }
        }
    }
    IEnumerator updateBar()
    {

        // Jalynn: Experiment Log
        manager.GetComponent<ExperimentLog>().AddData(this.gameObject.name, "Rest Start");

        if (dropdown == Options.Shape)
        {
           progress.text = waitingTime.ToString() + " s";
        }
        
        managerObj = GameObject.FindWithTag("Manager");
        sceneDirector = managerObj.GetComponent<SceneDirector>();

        /*if (sceneDirector.firstWait && dropdown == Options.WaitingRoom)
        {
            progress.text = "Please talk to the experimenter.";
            sceneDirector.firstWait = false;
            yield break;
        }*/

        while (waitingTime > 0)
        {

            yield return new WaitForSeconds(1f);
            waitingTime--;
            if (dropdown == Options.WaitingRoom)
            {
                progress.text = "Please wait for " + waitingTime.ToString() + " seconds";
            }
            else if (dropdown == Options.Shape)
            {
                progress.text = waitingTime.ToString() + " s";
            }
        }

        /*if (dropdown == Options.WaitingRoom)
        {
            progress.text = "Please talk to the experimenter.";
        }
        else if (dropdown == Options.Shape)
        {
            
            progress.enabled = false;
        }*/

        // Jalynn: Experiment Log
        manager.GetComponent<ExperimentLog>().AddData(this.gameObject.name, "Rest Stop");
    }
}
