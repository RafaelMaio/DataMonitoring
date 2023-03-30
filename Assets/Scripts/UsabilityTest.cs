// ===============================
// AUTHOR     : Rafael Maio (rafael.maio@ua.pt)
// PURPOSE     : Script for handling the user study.
// SPECIAL NOTES: X
// ===============================

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UsabilityTest : MonoBehaviour
{
    /// <summary>
    /// Is the user test enable or disable?
    /// </summary>
    private bool OnFlag = false;

    /// <summary>
    /// Communication with Menu Handler script.
    /// </summary>
    public MenuHandler menuHandler;

    /// <summary>
    /// Usability test date since the beggining of the task.
    /// </summary>
    private DateTime date;

    /// <summary>
    /// Date when the user test started.
    /// </summary>
    private DateTime initialDate;

    /// <summary>
    /// File path for saving the usability tests results.
    /// </summary>
    private string filePathName;

    /// <summary>
    /// File path for saving the usability tests events.
    /// </summary>
    private string filePathEvents;

    /// <summary>
    /// Participant identifier.
    /// </summary>
    private int participantId = 0;

    /// <summary>
    /// Number of clicks to reach the goal.
    /// </summary>
    private int accuracy = 0;

    /// <summary>
    /// Number of the current task;
    /// </summary>
    private int currentTaskNumber = 1;

    /// <summary>
    /// Enable/disable the usability test.
    /// </summary>
    public void changeOnFlag()
    {
        OnFlag = !OnFlag;
        menuHandler.showHideUsabilityTaskBar(OnFlag);
    }

    /// <summary>
    /// Get if the usability tests are enabled or disabled.
    /// </summary>
    /// <returns>The flag indicating if the user tests are on or off.</returns>
    public bool getOnFlag()
    {
        return OnFlag;
    }

    /// <summary>
    /// Set the path for the file, participant id and initial date.
    /// </summary>
    public void userTestSetup()
    {
        if (OnFlag)
        {
            date = DateTime.Now;
            filePathName = Application.persistentDataPath + "/userTestMonitoringAR.txt";
            filePathEvents = Application.persistentDataPath + "/userTestEvents.txt";
            if (File.Exists(filePathName))
            {
                if (File.ReadAllLines(filePathName).Length == 0)
                {
                    participantId = 1;
                }
                else
                {
                    participantId = Int32.Parse(File.ReadAllLines(filePathName)[File.ReadAllLines(filePathName).Length - 1].Split(',')[0]) + 1;
                }
            }
            else
            {
                File.Create(filePathName);
                File.Create(filePathEvents);
                participantId = 1;
            }
        }
    }

    /// <summary>
    /// Stop the task timer and accuracy.
    /// </summary>
    public void stopTime(int taskCount)
    {
        TimeSpan time = DateTime.Now - date;
        using (StreamWriter sw = File.AppendText(filePathName))
        {
            sw.WriteLine(participantId.ToString() + "," + taskCount.ToString() + "," + time.ToString() + "," + accuracy);
        }
        accuracy = 0;
        currentTaskNumber = taskCount + 1;
        date = DateTime.Now;
    }

    /// <summary>
    /// A component (ex: button) was pressed, increase the number of clicks to reach the goal.
    /// </summary>
    public void componentClicked()
    {
        accuracy += 1;
    }

    /// <summary>
    /// Writes event in file.
    /// </summary>
    /// <param name="eventName">The name of button pressed.</param>
    public void writeEvent(string eventName)
    {
        if (participantId > 0)
        {
            TimeSpan time = DateTime.Now - initialDate;
            componentClicked();
            using (StreamWriter sw = File.AppendText(filePathEvents))
            {
                sw.WriteLine(participantId.ToString() + "," + currentTaskNumber.ToString() + "," + time.ToString() + "," + accuracy + "," + eventName);
            }
        }
    }

    /// <summary>
    /// Writes event in file.
    /// </summary>
    /// <param name="eventName">The name of button pressed.</param>
    public void writeEventSlider(string eventName, string valueMin, string valueMax)
    {
        if (participantId > 0)
        {
            TimeSpan time = DateTime.Now - initialDate;
            componentClicked();
            using (StreamWriter sw = File.AppendText(filePathEvents))
            {
                if (!valueMin.Equals("0"))
                {
                    sw.WriteLine(participantId.ToString() + "," + currentTaskNumber.ToString() + "," + time.ToString() + "," + accuracy + "," + eventName + "," + valueMin + "," + valueMax);
                }
                else
                {
                    sw.WriteLine(participantId.ToString() + "," + currentTaskNumber.ToString() + "," + time.ToString() + "," + accuracy + "," + eventName + "," + "1" + "," + valueMax);
                }
            }
        }
    }
}