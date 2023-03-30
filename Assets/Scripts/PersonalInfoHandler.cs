// ===============================
// AUTHOR     : Rafael Maio (rafael.maio@ua.pt)
// PURPOSE     : Handles the personal menus navigation.
// SPECIAL NOTES: X
// ===============================

using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PersonalInfoHandler : MonoBehaviour
{
    /// <summary>
    /// Main station panel.
    /// </summary>
    public GameObject generalStationInfo;

    /// <summary>
    /// Cycle times main station panel.
    /// </summary>
    public GameObject cycleTimeMain;

    /// <summary>
    /// Cycle times specific panel.
    /// </summary>
    public GameObject cycleTimeSpecific;

    /// <summary>
    /// KPIs main station panel.
    /// </summary>
    public GameObject kpiMain;

    /// <summary>
    /// KPIs specific panel.
    /// </summary>
    public GameObject kpiSpecific;

    /// <summary>
    /// Communication with the Line Monitoring Handler script
    /// </summary>
    public LineMonitoringHandler lineMonitoringHandler;

    /// <summary>
    /// Communication with the Maintain Line Renderer script for a specific cycle time.
    /// </summary>
    public maintainLineRenderer maintainLineOnCycle;

    /// <summary>
    /// Communication with the Maintain Line Renderer script for a specific kpi.
    /// </summary>
    public maintainLineRenderer maintainLineOnKPIs;

    /// <summary>
    /// Change to the cycle times main panel.
    /// </summary>
    public void changeToCycleTime()
    {
        generalStationInfo.SetActive(false);
        cycleTimeMain.SetActive(true);
        staticFunctions.FindChildByRecursion(cycleTimeMain.transform, "Title").GetComponent<TMP_Text>().text =
            staticFunctions.FindChildByRecursion(generalStationInfo.transform, "Title").GetComponent<TMP_Text>().text;
        staticFunctions.maintainTransform(generalStationInfo, cycleTimeMain);

        lineMonitoringHandler.paintLoadCircles();
        lineMonitoringHandler.UpdatePersonalValues();
    }

    /// <summary>
    /// Filter by bottleneck.
    /// </summary>
    public void changeToBottleneck()
    {
        generalStationInfo.SetActive(false);
        lineMonitoringHandler.filterByBottleneck();
    }

    /// <summary>
    /// Change to the kpis main panel.
    /// </summary>
    public void changeToKPIs()
    {
        generalStationInfo.SetActive(false);
        kpiMain.SetActive(true);
        staticFunctions.FindChildByRecursion(kpiMain.transform, "Title").GetComponent<TMP_Text>().text =
            staticFunctions.FindChildByRecursion(generalStationInfo.transform, "Title").GetComponent<TMP_Text>().text;
        staticFunctions.maintainTransform(generalStationInfo, kpiMain);
        lineMonitoringHandler.paintLoadCircles();
        lineMonitoringHandler.UpdatePersonalValues();
    }

    /// <summary>
    /// Change to the specific cycle time panel.
    /// </summary>
    /// <param name="specificTime">Specific cycle time.</param>
    public void changeToSpecificCycleTime(string specificTime)
    {
        cycleTimeMain.SetActive(false);
        cycleTimeSpecific.SetActive(true);
        staticFunctions.FindChildByRecursion(cycleTimeSpecific.transform, "Title").GetComponent<TMP_Text>().text =
            staticFunctions.FindChildByRecursion(cycleTimeMain.transform, "Title").GetComponent<TMP_Text>().text;
        staticFunctions.FindChildByRecursion(cycleTimeSpecific.transform, "CycleTimesTitle").GetComponent<TMP_Text>().text = specificTime;
        staticFunctions.FindChildByRecursion(cycleTimeSpecific.transform, "CustomPinchSlider").GetComponent<CustomPinchSlider>().SliderValueMin = 0f;
        staticFunctions.FindChildByRecursion(cycleTimeSpecific.transform, "CustomPinchSlider").GetComponent<CustomPinchSlider>().SliderValueMax = 1f;

        string specificTimeFixed = "";
        switch (specificTime)
        {
            case "Cycle Time":
                specificTimeFixed = "totalCycleTime";
                break;
            case "Waiting Time":
                specificTimeFixed = "changeTime";
                break;
            case "Process Time":
                specificTimeFixed = "processTime";
                break;
            case "Exit Time":
                specificTimeFixed = "exitTime";
                break;
        }

        string line = staticFunctions.FindChildByRecursion(cycleTimeSpecific.transform, "Title").GetComponent<TMP_Text>().text.Split(" /")[0].Split(": ")[1];
        string station = staticFunctions.FindChildByRecursion(cycleTimeSpecific.transform, "Title").GetComponent<TMP_Text>().text.Split("Station: ")[1];

        maintainLineOnCycle.setPersonalInfoCycleTime(Int32.Parse(line), Int32.Parse(station), specificTimeFixed);
        maintainLineOnCycle.changeNumberOfPointsTime_v2();
        staticFunctions.maintainTransform(cycleTimeMain, cycleTimeSpecific);

        lineMonitoringHandler.paintLoadCircles();
        lineMonitoringHandler.UpdatePersonalValues();
    }

    /// <summary>
    /// Change to the specific kpi panel.
    /// </summary>
    /// <param name="specificTime">Specific kpi.</param>
    public void changeToSpecificKPI(string specificKPI)
    {
        kpiMain.SetActive(false);
        kpiSpecific.SetActive(true);
        staticFunctions.FindChildByRecursion(kpiSpecific.transform, "Title").GetComponent<TMP_Text>().text =
            staticFunctions.FindChildByRecursion(kpiMain.transform, "Title").GetComponent<TMP_Text>().text;
        staticFunctions.FindChildByRecursion(kpiSpecific.transform, "KPITitle").GetComponent<TMP_Text>().text = specificKPI;
        staticFunctions.FindChildByRecursion(kpiSpecific.transform, "CustomPinchSlider").GetComponent<CustomPinchSlider>().SliderValueMin = 0f;
        staticFunctions.FindChildByRecursion(kpiSpecific.transform, "CustomPinchSlider").GetComponent<CustomPinchSlider>().SliderValueMax = 1f;
        string specificKPIFixed = "";
        if (specificKPI.Equals("OEE"))
        {
            staticFunctions.FindChildByRecursion(kpiSpecific.transform, "YLabel").GetComponent<TMP_Text>().text = "%";
            specificKPIFixed = "oee";
        }
        else if(specificKPI.Equals("FPY"))
        {
            staticFunctions.FindChildByRecursion(kpiSpecific.transform, "YLabel").GetComponent<TMP_Text>().text = "%";
            specificKPIFixed = "fpy";
        }
        else if (specificKPI.Equals("PartCount"))
        {
            staticFunctions.FindChildByRecursion(kpiSpecific.transform, "YLabel").GetComponent<TMP_Text>().text = "Pieces";
            specificKPIFixed = "partCount";
        }
        else if (specificKPI.Equals("PartCountNIO"))
        {
            staticFunctions.FindChildByRecursion(kpiSpecific.transform, "YLabel").GetComponent<TMP_Text>().text = "Pieces";
            specificKPIFixed = "countNIO";
        }
        else if (specificKPI.Equals("Productivity"))
        {
            staticFunctions.FindChildByRecursion(kpiSpecific.transform, "YLabel").GetComponent<TMP_Text>().text = "Pieces / Man * Hour";
            specificKPIFixed = "productivity";
        }
        string line = staticFunctions.FindChildByRecursion(kpiSpecific.transform, "Title").GetComponent<TMP_Text>().text.Split(" /")[0].Split(": ")[1];
        string station = staticFunctions.FindChildByRecursion(kpiSpecific.transform, "Title").GetComponent<TMP_Text>().text.Split("Station: ")[1];
        maintainLineOnKPIs.setPersonalInfoKPI(
            Int32.Parse(line),
            Int32.Parse(station),
            specificKPIFixed
        );
        //newChartG.GetSample(station, specificKPIFixed, 12, staticFunctions.FindChildByRecursion(kpiSpecific.transform, "VegaLiteIntegration").gameObject);
        //maintainLineOnKPIs.changeNumberOfPointsKPI();
        maintainLineOnKPIs.changeNumberOfPointsKPI_v2();
        staticFunctions.maintainTransform(kpiMain, kpiSpecific);
        lineMonitoringHandler.paintLoadCircles();
        lineMonitoringHandler.UpdatePersonalValues();
    }

    /// <summary>
    /// Specific cycle time panel indication that the data is being acccessed and the plot is being drawn.
    /// </summary>
    /// <param name="refreshing">Is it refreshing?</param>
    public void refreshingCycleTimes(bool refreshing)
    {
        if (refreshing)
        {
            staticFunctions.FindChildByRecursion(cycleTimeSpecific.transform, "CycleTimesTitle").GetComponent<TMP_Text>().text =
                staticFunctions.FindChildByRecursion(cycleTimeSpecific.transform, "CycleTimesTitle").GetComponent<TMP_Text>().text + " - Refreshing...";
        }
        else
        {
            staticFunctions.FindChildByRecursion(cycleTimeSpecific.transform, "CycleTimesTitle").GetComponent<TMP_Text>().text =
                staticFunctions.FindChildByRecursion(cycleTimeSpecific.transform, "CycleTimesTitle").GetComponent<TMP_Text>().text.Split(" -")[0];
        }
    }

    /// <summary>
    /// Specific kpi panel indication that the data is being acccessed and the plot is being drawn.
    /// </summary>
    /// <param name="refreshing">Is it refreshing?</param>
    public void refreshingKPIs(bool refreshing)
    {
        if (refreshing)
        {
            staticFunctions.FindChildByRecursion(kpiSpecific.transform, "KPITitle").GetComponent<TMP_Text>().text = 
                staticFunctions.FindChildByRecursion(kpiSpecific.transform, "KPITitle").GetComponent<TMP_Text>().text + " - Refreshing...";
        }
        else
        {
            staticFunctions.FindChildByRecursion(kpiSpecific.transform, "KPITitle").GetComponent<TMP_Text>().text =
                staticFunctions.FindChildByRecursion(kpiSpecific.transform, "KPITitle").GetComponent<TMP_Text>().text.Split(" -")[0];
        }
    }

    /// <summary>
    /// Return to the previous menu.
    /// </summary>
    /// <param name="lastMenu">Previous menu.</param>
    public void back(string lastMenu)
    {
        if (lastMenu.Equals("generalStationInfo_c"))
        {
            cycleTimeMain.SetActive(false);
            generalStationInfo.SetActive(true);
            staticFunctions.maintainTransform(cycleTimeMain, generalStationInfo);
            staticFunctions.FindChildByRecursion(generalStationInfo.transform, "Title").GetComponent<TMP_Text>().text =
                staticFunctions.FindChildByRecursion(cycleTimeMain.transform, "Title").GetComponent<TMP_Text>().text;
        }
        else if (lastMenu.Equals("generalStationInfo_k"))
        {
            kpiMain.SetActive(false);
            generalStationInfo.SetActive(true);
            staticFunctions.maintainTransform(kpiMain, generalStationInfo);
            staticFunctions.FindChildByRecursion(generalStationInfo.transform, "Title").GetComponent<TMP_Text>().text =
                staticFunctions.FindChildByRecursion(kpiMain.transform, "Title").GetComponent<TMP_Text>().text;
        }
        else if (lastMenu.Equals("cycleTimesMain"))
        {
            cycleTimeSpecific.SetActive(false);
            cycleTimeMain.SetActive(true);
            staticFunctions.maintainTransform(cycleTimeSpecific, cycleTimeMain);
        }
        else if (lastMenu.Equals("kpisMain"))
        {
            kpiSpecific.SetActive(false);
            kpiMain.SetActive(true);
            staticFunctions.maintainTransform(kpiSpecific, kpiMain);
        }

        lineMonitoringHandler.paintLoadCircles();
        lineMonitoringHandler.UpdatePersonalValues();
    }

    /// <summary>
    /// Handle the personal window mevement (Moving or fixed).
    /// </summary>
    public void handleMovement()
    {
        if(staticFunctions.FindChildByRecursion(generalStationInfo.transform, "MoveButton").GetComponent<ButtonConfigHelper>().MainLabelText.Equals("Move Panel"))
        {
            staticFunctions.FindChildByRecursion(generalStationInfo.transform, "MoveButton").GetComponent<ButtonConfigHelper>().MainLabelText = "Fixe Panel";
            staticFunctions.FindChildByRecursion(kpiMain.transform, "MoveButton").GetComponent<ButtonConfigHelper>().MainLabelText = "Fixe Panel";
            staticFunctions.FindChildByRecursion(cycleTimeMain.transform, "MoveButton").GetComponent<ButtonConfigHelper>().MainLabelText = "Fixe Panel";
            staticFunctions.FindChildByRecursion(cycleTimeSpecific.transform, "MoveButton").GetComponent<ButtonConfigHelper>().MainLabelText = "Fixe Panel";
            staticFunctions.FindChildByRecursion(kpiSpecific.transform, "MoveButton").GetComponent<ButtonConfigHelper>().MainLabelText = "Fixe Panel";
            generalStationInfo.GetComponent<ObjectManipulator>().enabled = true;
            kpiMain.GetComponent<ObjectManipulator>().enabled = true;
            cycleTimeSpecific.GetComponent<ObjectManipulator>().enabled = true;
            cycleTimeMain.GetComponent<ObjectManipulator>().enabled = true;
            kpiSpecific.GetComponent<ObjectManipulator>().enabled = true;
        }
        else
        {
            staticFunctions.FindChildByRecursion(generalStationInfo.transform, "MoveButton").GetComponent<ButtonConfigHelper>().MainLabelText = "Move Panel";
            staticFunctions.FindChildByRecursion(kpiMain.transform, "MoveButton").GetComponent<ButtonConfigHelper>().MainLabelText = "Move Panel";
            staticFunctions.FindChildByRecursion(cycleTimeMain.transform, "MoveButton").GetComponent<ButtonConfigHelper>().MainLabelText = "Move Panel";
            staticFunctions.FindChildByRecursion(cycleTimeSpecific.transform, "MoveButton").GetComponent<ButtonConfigHelper>().MainLabelText = "Move Panel";
            staticFunctions.FindChildByRecursion(kpiSpecific.transform, "MoveButton").GetComponent<ButtonConfigHelper>().MainLabelText = "Move Panel";
            generalStationInfo.GetComponent<ObjectManipulator>().enabled = false;
            kpiMain.GetComponent<ObjectManipulator>().enabled = false;
            cycleTimeSpecific.GetComponent<ObjectManipulator>().enabled = false;
            cycleTimeMain.GetComponent<ObjectManipulator>().enabled = false;
            kpiSpecific.GetComponent<ObjectManipulator>().enabled = false;
        }
    }
}