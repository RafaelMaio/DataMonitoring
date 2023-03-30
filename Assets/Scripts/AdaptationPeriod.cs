// ===============================
// AUTHOR     : Rafael Maio (rafael.maio@ua.pt)
// PURPOSE     : Script for application adaptation before the user test.
// SPECIAL NOTES: X
// ===============================

using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AdaptationPeriod : MonoBehaviour
{
    /// <summary>
    /// Dummy station 100.
    /// </summary>
    public GameObject stationinfoload;

    /// <summary>
    /// Dummy station 200.
    /// </summary>
    public GameObject stationinfoload2;

    /// <summary>
    /// Dummy general main panel.
    /// </summary>
    public GameObject GeneralMain;

    /// <summary>
    /// Dummy KPIs main panel.
    /// </summary>
    public GameObject KPIsMain;

    /// <summary>
    /// Dummy specific KPI panel.
    /// </summary>
    public GameObject KPIspecific;

    /// <summary>
    /// Object for deleting purposes.
    /// </summary>
    private GameObject nullGameObject;

    /// <summary>
    /// Representation of the HMD.
    /// To access its pose.
    /// </summary>
    public GameObject head;

    /// <summary>
    /// Unity Start function.
    /// </summary>
    void Start()
    {
        nullGameObject = new GameObject();
    }

    /// <summary>
    /// Unity Update function.
    /// For pinch detection.
    /// </summary>
    void Update()
    {
        bool isPointin = staticFunctions.isPointing(nullGameObject).Item1;
        GameObject hitObject = staticFunctions.isPointing(nullGameObject).Item2;
        if (isPointin && staticFunctions.isPinching(Handedness.Right))
        {
            string station_number = staticFunctions.FindChildByRecursion(hitObject.transform, "Title").GetComponent<TMP_Text>().text;
            staticFunctions.FindChildByRecursion(GeneralMain.transform, "Title").GetComponent<TMP_Text>().text = station_number;
            /*
            staticFunctions.FindChildByRecursion(KPIsMain.transform, "OEETitle").GetComponent<TMP_Text>().text = "OEE: " + station_number + "%";
            staticFunctions.FindChildByRecursion(KPIsMain.transform, "FPYTitle").GetComponent<TMP_Text>().text = "FPY: " + station_number + "%";
            staticFunctions.FindChildByRecursion(KPIsMain.transform, "PartCountTitle").GetComponent<TMP_Text>().text = "PartCount: " + station_number;
            staticFunctions.FindChildByRecursion(KPIsMain.transform, "PartCountNIOTitle").GetComponent<TMP_Text>().text = "PartCount NOk: " + station_number;
            staticFunctions.FindChildByRecursion(KPIsMain.transform, "ProductivityTitle").GetComponent<TMP_Text>().text = "Productivity: " + station_number ;
            */
            if (!GeneralMain.activeSelf)
            {
                GeneralMain.transform.position = head.transform.position + head.transform.forward * 0.8f;
                GeneralMain.transform.rotation = Quaternion.LookRotation(GeneralMain.transform.position - head.transform.position);
            }
            GeneralMain.SetActive(true);
        }
    }

    /// <summary>
    /// Move to the next task.
    /// Making the next adaptation objects appear.
    /// </summary>
    /// <param name="taskCounter">Number of the adaptation task.</param>
    public void nextAdaptationTask(int taskCounter)
    {
        switch (taskCounter)
        {
            case 2:
                staticFunctions.FindChildByRecursion(KPIspecific.transform, "FollowMeButton").gameObject.SetActive(true);
                break;
            case 3:
                staticFunctions.FindChildByRecursion(KPIspecific.transform, "FollowMeButton").gameObject.SetActive(true);
                staticFunctions.FindChildByRecursion(KPIspecific.transform, "CloseButton").gameObject.SetActive(true);
                staticFunctions.FindChildByRecursion(KPIspecific.transform, "MoveButton").gameObject.SetActive(true);
                staticFunctions.FindChildByRecursion(KPIspecific.transform, "BackButton").gameObject.SetActive(true);
                staticFunctions.FindChildByRecursion(KPIspecific.transform, "CustomPinchSlider").gameObject.SetActive(true);
                break;
        }
    }

    /// <summary>
    /// Open KPIs specific menu.
    /// Second task.
    /// </summary>
    public void openPartCount()
    {
        KPIsMain.SetActive(false);
        KPIspecific.SetActive(true);
        KPIspecific.transform.position = KPIsMain.transform.position;
        KPIspecific.transform.rotation = KPIsMain.transform.rotation;
        string station_number = staticFunctions.FindChildByRecursion(KPIsMain.transform, "Title").GetComponent<TMP_Text>().text;
        staticFunctions.FindChildByRecursion(KPIspecific.transform, "Title").GetComponent<TMP_Text>().text = station_number;
    }

    /// <summary>
    /// Open KPIs menu.
    /// Second task.
    /// </summary>
    public void openKPIs()
    {
        GeneralMain.SetActive(false);
        KPIsMain.SetActive(true);
        KPIsMain.transform.position = GeneralMain.transform.position;
        KPIsMain.transform.rotation = GeneralMain.transform.rotation;
        string station_number = staticFunctions.FindChildByRecursion(GeneralMain.transform, "Title").GetComponent<TMP_Text>().text;
        staticFunctions.FindChildByRecursion(KPIsMain.transform, "Title").GetComponent<TMP_Text>().text = station_number;
    }

    /// <summary>
    /// Destroy station 100 and station 200.
    /// </summary>
    public void destroyPanels()
    {
        Destroy(KPIspecific);
        Destroy(stationinfoload);
        Destroy(stationinfoload2);
    }
}