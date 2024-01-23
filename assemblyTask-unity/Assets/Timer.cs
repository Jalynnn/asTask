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
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(updateBar());
    }

    // Update is called once per frame
    void Update()
    {

    }
    IEnumerator updateBar()
    {
        while (waitingTime >= 0)
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
        if (dropdown == Options.WaitingRoom)
        {
            progress.text = "Please talk to the experimenter.";
        }
        else if (dropdown == Options.Shape)
        {
            progress.enabled = false;
        }
    }
}
