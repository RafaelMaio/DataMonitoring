// ===============================
// AUTHOR     : Rafael Maio (rafael.maio@ua.pt)
// PURPOSE     : Handles the visualization module.
// SPECIAL NOTES: Its basically the main script of the application.
// ===============================

using Microsoft.MixedReality.WorldLocking.Core;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using Microsoft.MixedReality.Toolkit.UI;
using System.Linq;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;

public class LineMonitoringHandler : MonoBehaviour
{
    /// <summary>
    /// Path to application persisent data +
    /// specific file for the line selected.
    /// </summary>
    private string path;

    /// <summary>
    /// Parent of the stations configured in the line.
    /// </summary>
    public GameObject contentStations;

    /// <summary>
    /// Prefab for each station - Main view.
    /// </summary>
    public GameObject visStationPrefab;

    /// <summary>
    /// Prefab for each station - Filtered by bottleneck.
    /// </summary>
    public GameObject specificVisStationPrefabBottleneck;

    /// <summary>
    /// Prefab for each station - Filtered by cycle time.
    /// </summary>
    public GameObject specificVisStationPrefabCycle;

    /// <summary>
    /// Prefab for each station - Filtered by KPIs.
    /// </summary>
    public GameObject specificVisStationPrefabKPIs;

    /// <summary>
    /// Personal menu main window.
    /// </summary>
    public GameObject generalStationInfo;

    /// <summary>
    /// Personal menu cycle time window.
    /// </summary>
    public GameObject cycleTimesMain;

    /// <summary>
    /// Personal menu kpis window.
    /// </summary>
    public GameObject kpisMain;

    /// <summary>
    /// Personal menu specific cycle time window.
    /// </summary>
    public GameObject cycleTimesSpecific;

    /// <summary>
    /// Personal menu specific kpi window.
    /// </summary>
    public GameObject kpisSpecific;

    /// <summary>
    /// Representation of the HMD.
    /// To access its pose.
    /// </summary>
    public GameObject head;

    /// <summary>
    /// Access to the hand joints.
    /// </summary>
    private IMixedRealityHandJointService handJointService;

    /// <summary>
    /// Access to the hand joints.
    /// </summary>
    private IMixedRealityHandJointService HandJointService => 
        handJointService ?? (handJointService = CoreServices.GetInputSystemDataProvider<IMixedRealityHandJointService>());

    /// <summary>
    /// Communication with the ValuesHandler script.
    /// To access the API
    /// </summary>
    public ValuesHandler valuesHandler;

    /// <summary>
    /// Object for deleting purposes.
    /// </summary>
    private GameObject nullGameObject;

    /// <summary>
    /// Sound when opening the personal window.
    /// </summary>
    public AudioSource soundOpenPersonal;

    /// <summary>
    /// Arrow for indication where the bottleneck station is located.
    /// </summary>
    public GameObject bottleneckArrow;

    /// <summary>
    /// The last station defined as the bottleneck.
    /// </summary>
    private int lastBottleneckStation = -1;

    /// <summary>
    /// Prefab for dialogs
    /// </summary>
    public GameObject dialogWarningPrefab;

    /// <summary>
    /// Sound when the dialog window opens.
    /// </summary>
    public AudioSource soundDialogWarning;

    /// <summary>
    /// Dialog for indicting that the previous bottleneck has changed.
    /// </summary>
    private Dialog bottleneckWarningDialog;

    /// <summary>
    /// Dialog for indicating if stations don't have data in the API.
    /// </summary>
    private Dialog noDataDialog;

    /// <summary>
    /// All verifications are done and all stations have data in the API.
    /// </summary>
    private bool dataAvailable = false;

    /// <summary>
    /// Prefab for the overview window.
    /// </summary>
    public GameObject overviewStationPrefab;

    /// <summary>
    /// Overview window.
    /// </summary>
    public GameObject stationOverview;

    /// <summary>
    /// Communication with the Usability Test script.
    /// </summary>
    public UsabilityTest usabilityTest;

    /// <summary>
    /// Communication with the script for accessing vegalite.
    /// </summary>
    public newChartGen newChartG;

    /// <summary>
    /// Panel confirming that a panel was opened (or that is already opened).
    /// </summary>
    public GameObject openWarning;

    /// <summary>
    /// Unity Start function.
    /// </summary>
    private void Start()
    {
        nullGameObject = new GameObject();

        // Add event to every button
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        UsabilityTest usabilityTest = FindObjectOfType<UsabilityTest>();
        foreach (GameObject go in allObjects)
        {
            if (go.GetComponent<PressableButtonHoloLens2>() != null)
            {
                //UnityEditor.Events.UnityEventTools.RemovePersistentListener(go.GetComponent<Interactable>().OnClick, go.GetComponent<Interactable>().OnClick.GetPersistentEventCount() - 1);
                //UnityEditor.Events.UnityEventTools.AddStringPersistentListener(go.GetComponent<Interactable>().OnClick, usabilityTest.writeEvent, getPathName(go));
                go.GetComponent<Interactable>().OnClick.AddListener(delegate { usabilityTest.writeEvent(staticFunctions.getPathName(go)); });
            }
            else if (go.GetComponent<CustomPinchSlider>() != null)
            {
                //UnityEditor.Events.UnityEventTools.RemovePersistentListener(go.GetComponent<PinchSlider>().OnInteractionEnded, go.GetComponent<PinchSlider>().OnInteractionEnded.GetPersistentEventCount() - 1);
                //UnityEditor.Events.UnityEventTools.AddStringPersistentListener(go.GetComponent<PinchSlider>().OnInteractionEnded, usabilityTest.writeEvent, getPathName(go));
                if (staticFunctions.getPathName(go).Contains("KPI")){
                    go.GetComponent<CustomPinchSlider>().OnInteractionEnded.AddListener(delegate { usabilityTest.writeEventSlider(staticFunctions.getPathName(go), (go.GetComponent<CustomPinchSlider>().SliderValueMin * 60).ToString(), (go.GetComponent<CustomPinchSlider>().SliderValueMax * 60).ToString()); });
                }
                else
                {
                    go.GetComponent<CustomPinchSlider>().OnInteractionEnded.AddListener(delegate { usabilityTest.writeEventSlider(staticFunctions.getPathName(go), (go.GetComponent<CustomPinchSlider>().SliderValueMin * 100).ToString(), (go.GetComponent<CustomPinchSlider>().SliderValueMax * 100).ToString()); });
                }
            }
        }
    }

    /// <summary>
    /// Unity Update function.
    /// For selecting station and calling methods with a rate of 5 seconds.
    /// </summary>
    private void Update()
    {
        if (dataAvailable) {
            bool isPointin = staticFunctions.isPointing(nullGameObject).Item1;
            GameObject hitObject = staticFunctions.isPointing(null).Item2;
            if (isPointin && staticFunctions.isPinching(Handedness.Right))
            {
                if (!alreadyOpened(hitObject))
                {
                    openPersonalWindow(hitObject);
                    usabilityTest.writeEvent("Open:" + staticFunctions.FindChildByRecursion(hitObject.transform, "Title").GetComponent<TMP_Text>().text.Split("/ Station:")[1]);
                    openWarning.transform.GetChild(0).GetComponent<TextMeshPro>().text = "Station " + staticFunctions.FindChildByRecursion(hitObject.transform, "Title").GetComponent<TMP_Text>().text.Split("/ Station:")[1] + " opened";
                }
                else
                {
                    if (!IsInvoking("closeOpenWarning")){
                        openWarning.transform.GetChild(0).GetComponent<TextMeshPro>().text = "Station " + staticFunctions.FindChildByRecursion(hitObject.transform, "Title").GetComponent<TMP_Text>().text.Split("/ Station:")[1] + " already opened";
                    }
                }
                openWarning.SetActive(true);
                Invoke("closeOpenWarning", 1f);
            }
            if (!IsInvoking("callSimulation"))
            {
                Invoke("callSimulation", 5f);
            }
            if (!IsInvoking("paintLoadCircles"))
            {
                Invoke("paintLoadCircles", 5f);
            }
            if (!IsInvoking("UpdatePersonalValues"))
            {
                Invoke("UpdatePersonalValues", 5f);
            }

            if (bottleneckArrow.activeSelf)
                bottleneckHandler();

            if (valuesHandler.getAllSet() == contentStations.transform.childCount * 3)
            {
                paintLoadCircles();
                UpdatePersonalValues();
                valuesHandler.setAllSet();
            }
            if (getOpenedPersonal() != null) {
                if (staticFunctions.betweenDistance(getOpenedPersonal(), head, getOpenedPersonal().GetComponent<RadialView>().MaxDistance)){
                    getOpenedPersonal().GetComponent<RadialView>().enabled = false;
                }
                else
                {
                    getOpenedPersonal().GetComponent<RadialView>().enabled = true;
                }
            }
        }
    }

    /// <summary>
    /// Set the line scenario for acessing configuration.
    /// </summary>
    /// <param name="scenario">Line number.</param>
    public void setLineScenario(int scenario)
    {
        path = Path.Combine(Application.persistentDataPath, "scene_content" + scenario.ToString() + ".txt");
    }

    /// <summary>
    /// Unity OnEnable function.
    /// Creates one visualization panel for each existing station in the line.
    /// </summary>
    private void OnEnable()
    {
        dataAvailable = false;
        noDataDialog = Dialog.Open(dialogWarningPrefab, DialogButtonType.OK, "Data verification.", "Verifying if all stations have API access.", true);
        string line_number = path.Split("scene_content")[1].Split(".txt")[0];
        WorldLockingManager.GetInstance().Load();
        if (File.Exists(path))
        {
            var source = new StreamReader(path);
            var fileContents = source.ReadToEnd();
            source.Close();
            var lines = fileContents.Split("\n"[0]);
            foreach (var line in lines)
            {
                if (line.Contains(";"))
                {
                    string[] line_splitted = line.Split(";");
                    Vector3 pos = new Vector3(float.Parse(line_splitted[0], CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(line_splitted[1], CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(line_splitted[2], CultureInfo.InvariantCulture.NumberFormat));
                    Vector3 rot = new Vector3(float.Parse(line_splitted[3], CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(line_splitted[4], CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(line_splitted[5], CultureInfo.InvariantCulture.NumberFormat));
                    GameObject spawnedStation = Instantiate(visStationPrefab, pos, Quaternion.Euler(rot));
                    spawnedStation.transform.parent = contentStations.transform;
                    int station_number = Int32.Parse(line_splitted[6]);
                    staticFunctions.FindChildByRecursion(spawnedStation.transform, "Title").GetComponent<TMP_Text>().text =
                        "Line: " + line_number + " / Station: " + station_number.ToString();
                    valuesHandler.verifyDataInStation(Int32.Parse(line_number), station_number);
                }
            }
        }
        fillOverview(line_number);
    }

    /// <summary>
    /// Unity OnDisable function.
    /// Destroys game objects that are not useful outside the visualization module.
    /// </summary>
    private void OnDisable()
    {
        for (int i = 0; i < contentStations.transform.childCount; i++)
        {
            Destroy(contentStations.transform.GetChild(i).gameObject);
        }
        if (getOpenedPersonal() != null) getOpenedPersonal().SetActive(false);
        for (int i = 3; i < stationOverview.transform.childCount; i++)
        {
            Destroy(stationOverview.transform.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// Filter the line stations.
    /// </summary>
    /// <param name="by">Filter by: Cyle Times, KPIs or Bottleneck.</param>
    public void filterBy(string by)
    {
        List<GameObject> filteredStations = new List<GameObject>();
        for (int i = 0; i < contentStations.transform.childCount; i++)
        {
            GameObject spawnedStation = new GameObject();
            string station_number = staticFunctions.FindChildByRecursion(contentStations.transform.GetChild(i).transform, "Title").GetComponent<TMP_Text>().text.Split("Station: ")[1];
            if (by.Equals("Cycle Times"))
            {
                spawnedStation = Instantiate(specificVisStationPrefabCycle, contentStations.transform.GetChild(i).position, contentStations.transform.GetChild(i).rotation);
                newChartG.GetSample(station_number, "totalCycleTime", 1, 10, staticFunctions.FindChildByRecursion(spawnedStation.transform, "VegaLiteIntegration").gameObject, true);
            }
            else if (by.Equals("KPIs"))
            {
                spawnedStation = Instantiate(specificVisStationPrefabKPIs, contentStations.transform.GetChild(i).position, contentStations.transform.GetChild(i).rotation);
                newChartG.GetSample(station_number, "fpy", 1, 10, staticFunctions.FindChildByRecursion(spawnedStation.transform, "VegaLiteIntegration").gameObject, true);
            }
            else
            {
                spawnedStation = Instantiate(specificVisStationPrefabBottleneck, contentStations.transform.GetChild(i).position, contentStations.transform.GetChild(i).rotation);
            }
            filteredStations.Add(spawnedStation);
            staticFunctions.FindChildByRecursion(spawnedStation.transform, "Title").GetComponent<TMP_Text>().text =
                        staticFunctions.FindChildByRecursion(contentStations.transform.GetChild(i).transform, "Title").GetComponent<TMP_Text>().text;
            Destroy(contentStations.transform.GetChild(i).gameObject);
        }
        foreach (var newStation in filteredStations)
        {
            newStation.transform.parent = contentStations.transform;
        }
        paintLoadCircles();
        UpdatePersonalValues();
    }

    /// <summary>
    /// Filter the existing stations by cycle times.
    /// </summary>
    public void filterByCycleTime()
    {
        filterBy("Cycle Times");
        clearBottleneckHandler(false);
    }

    /// <summary>
    /// Filter the existing stations by bottleneck.
    /// </summary>
    public void filterByBottleneck()
    {
        filterBy("Bottleneck");
        clearBottleneckHandler(true);
    }

    /// <summary>
    /// Filter the existing stations by KPIs.
    /// </summary>
    public void filterByKPIs()
    {
        filterBy("KPIs");
        clearBottleneckHandler(false);
    }

    /// <summary>
    /// Open the personal window selected by user.
    /// </summary>
    /// <param name="personalObj">Personal panel window selected.</param>
    private void openPersonalWindow(GameObject personalObj)
    {
        GameObject nullPersonal = new GameObject();
        nullPersonal.transform.position = head.transform.position + head.transform.forward * 0.8f;
        nullPersonal.transform.rotation = Quaternion.LookRotation(nullPersonal.transform.position - head.transform.position);

        GameObject openedWindow = getOpenedPersonal();
        if (openedWindow != null) openedWindow.SetActive(false);
        if (personalObj.tag.Equals("SpecificInfo"))
        {
            if(staticFunctions.FindChildByRecursion(personalObj.transform, "SpecificTitle").GetComponent<TMP_Text>().text.Equals("Cycle Time"))
            {
                cycleTimesMain.SetActive(true);
                staticFunctions.FindChildByRecursion(cycleTimesMain.transform, "Title").GetComponent<TMP_Text>().text =
                    staticFunctions.FindChildByRecursion(personalObj.transform, "Title").GetComponent<TMP_Text>().text;
                if (openedWindow != null) staticFunctions.maintainTransform(openedWindow, cycleTimesMain);
                else staticFunctions.maintainTransform(nullPersonal, cycleTimesMain);
            }
            else if(staticFunctions.FindChildByRecursion(personalObj.transform, "SpecificTitle").GetComponent<TMP_Text>().text.Equals("FPY"))
            {
                kpisMain.SetActive(true);
                staticFunctions.FindChildByRecursion(kpisMain.transform, "Title").GetComponent<TMP_Text>().text =
                    staticFunctions.FindChildByRecursion(personalObj.transform, "Title").GetComponent<TMP_Text>().text;
                if (openedWindow != null) staticFunctions.maintainTransform(openedWindow, kpisMain);
                else staticFunctions.maintainTransform(nullPersonal, kpisMain);
            }
            else if (staticFunctions.FindChildByRecursion(personalObj.transform, "SpecificTitle").GetComponent<TMP_Text>().text.Equals("Bottleneck"))
            {
                generalStationInfo.SetActive(true);
                staticFunctions.FindChildByRecursion(generalStationInfo.transform, "Title").GetComponent<TMP_Text>().text =
                    staticFunctions.FindChildByRecursion(personalObj.transform, "Title").GetComponent<TMP_Text>().text;
                if (openedWindow != null) staticFunctions.maintainTransform(openedWindow, generalStationInfo);
                else staticFunctions.maintainTransform(nullPersonal, generalStationInfo);
            }
        }
        else
        {
            generalStationInfo.SetActive(true);
            staticFunctions.FindChildByRecursion(generalStationInfo.transform, "Title").GetComponent<TMP_Text>().text =
                staticFunctions.FindChildByRecursion(personalObj.transform, "Title").GetComponent<TMP_Text>().text;
            if (openedWindow != null) staticFunctions.maintainTransform(openedWindow, generalStationInfo);
            else staticFunctions.maintainTransform(nullPersonal, generalStationInfo);
        }
        UpdatePersonalValues();
        soundOpenPersonal.GetComponent<AudioSource>().Play();

        if (personalObj.tag.Equals("SpecificInfo"))
        {
            if (!staticFunctions.FindChildByRecursion(personalObj.transform, "SpecificTitle").GetComponent<TMP_Text>().text.Equals("Bottleneck"))
            {
                clearBottleneckHandler(false);
            }
        }

        Destroy(nullPersonal);
    }

    /// <summary>
    /// Open the personal window from the overview menu.
    /// </summary>
    /// <param name="station_number">Station number to open.</param>
    private void openPersonalWindowFromOverview(string station_number)
    {
        GameObject nullPersonal = new GameObject();
        nullPersonal.transform.position = head.transform.position + head.transform.forward * 0.8f;
        nullPersonal.transform.rotation = Quaternion.LookRotation(nullPersonal.transform.position - head.transform.position);

        GameObject openedWindow = getOpenedPersonal();
        if (openedWindow != null) openedWindow.SetActive(false);
        generalStationInfo.SetActive(true);
        int line_number = Int32.Parse(path.Split("scene_content")[1].Split(".txt")[0]);
        staticFunctions.FindChildByRecursion(generalStationInfo.transform, "Title").GetComponent<TMP_Text>().text = 
            "Line: " + line_number.ToString() + " / Station: " + station_number;
        if (openedWindow != null) staticFunctions.maintainTransform(openedWindow, generalStationInfo);

        UpdatePersonalValues();
        soundOpenPersonal.GetComponent<AudioSource>().Play();

        if (openedWindow != null) staticFunctions.maintainTransform(openedWindow, stationOverview);
        else staticFunctions.maintainTransform(nullPersonal, stationOverview);

        Destroy(nullPersonal);
    }

    /// <summary>
    /// Verify if the the personal menu window is already opened.
    /// </summary>
    /// <param name="personalObj">The personal menu window.</param>
    /// <returns>If the personal menu window is opened.</returns>
    private bool alreadyOpened(GameObject personalObj)
    {
        if (personalObj.tag.Equals("SpecificInfo"))
        {
            if (staticFunctions.FindChildByRecursion(personalObj.transform, "SpecificTitle").GetComponent<TMP_Text>().text.Equals("Cycle Times"))
            {
                if (cycleTimesMain.activeSelf)
                {
                    if (staticFunctions.FindChildByRecursion(cycleTimesMain.transform, "Title").GetComponent<TMP_Text>().text
                        .Equals(staticFunctions.FindChildByRecursion(personalObj.transform, "Title").GetComponent<TMP_Text>().text))
                    {
                        return true;
                    }
                }
            }
            else if (staticFunctions.FindChildByRecursion(personalObj.transform, "SpecificTitle").GetComponent<TMP_Text>().text.Equals("KPIs"))
            {
                if (kpisMain.activeSelf)
                {
                    if (staticFunctions.FindChildByRecursion(kpisMain.transform, "Title").GetComponent<TMP_Text>().text
                        .Equals(staticFunctions.FindChildByRecursion(personalObj.transform, "Title").GetComponent<TMP_Text>().text))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        else if (personalObj.tag.Equals("GeneralInfo"))
        {
            if (generalStationInfo.activeSelf)
            {
                if (staticFunctions.FindChildByRecursion(generalStationInfo.transform, "Title").GetComponent<TMP_Text>().text
                    .Equals(staticFunctions.FindChildByRecursion(personalObj.transform, "Title").GetComponent<TMP_Text>().text))
                {
                    return true;
                }
            }
            return false;
        }
        else return false;
    }

    /// <summary>
    /// Get the personal window opened.
    /// </summary>
    /// <returns>The personal window opened.</returns>
    private GameObject getOpenedPersonal()
    {
        if (generalStationInfo.activeSelf) return generalStationInfo;
        if (cycleTimesMain.activeSelf) return cycleTimesMain;
        if (kpisMain.activeSelf) return kpisMain;
        if (cycleTimesSpecific.activeSelf) return cycleTimesSpecific;
        if (kpisSpecific.activeSelf) return kpisSpecific;
        if (stationOverview.activeSelf) return stationOverview;
        return null;
    }

    /// <summary>
    /// Remove the filtering from the stations.
    /// "Show all" button.
    /// </summary>
    public void removeFilter()
    {
        List<GameObject> completedStations = new List<GameObject>();
        for (int i = 0; i < contentStations.transform.childCount; i++)
        {
            GameObject spawnedStation = Instantiate(visStationPrefab, contentStations.transform.GetChild(i).position, contentStations.transform.GetChild(i).rotation);
            completedStations.Add(spawnedStation);
            staticFunctions.FindChildByRecursion(spawnedStation.transform, "Title").GetComponent<TMP_Text>().text =
                        staticFunctions.FindChildByRecursion(contentStations.transform.GetChild(i).transform, "Title").GetComponent<TMP_Text>().text;
            Destroy(contentStations.transform.GetChild(i).gameObject);
        }
        foreach (var newStation in completedStations)
        {
            newStation.transform.parent = contentStations.transform;
        }
        clearBottleneckHandler(false);
        paintLoadCircles();
        UpdatePersonalValues();
    }

    /// <summary>
    /// Write target values from the station in the personal main cycle times and personal main kpis.
    /// </summary>
    /// <param name="station">Corresponding station.</param>
    public void WriteTargetValues(int station)
    {
        // Write target values in fields - Cycle Times
        staticFunctions.FindChildByRecursion(cycleTimesMain.transform, "CycleTimeTitle").GetChild(0).GetComponent<TMP_Text>().text =
            "Target: " + Math.Round(valuesHandler.allStations.targetCycles[station].totalCycleTime, 1).ToString() + "s";
        staticFunctions.FindChildByRecursion(cycleTimesMain.transform, "ProcessTimeTitle").GetChild(0).GetComponent<TMP_Text>().text =
            "Target: " + Math.Round(valuesHandler.allStations.targetCycles[station].processTime, 1).ToString() + "s";
        staticFunctions.FindChildByRecursion(cycleTimesMain.transform, "ExitTimeTitle").GetChild(0).GetComponent<TMP_Text>().text =
            "Target: " + Math.Round(valuesHandler.allStations.targetCycles[station].exitTime, 1).ToString() + "s";
        staticFunctions.FindChildByRecursion(cycleTimesMain.transform, "WaitingTimeTitle").GetChild(0).GetComponent<TMP_Text>().text =
            "Target: " + Math.Round(valuesHandler.allStations.targetCycles[station].changeTime, 1).ToString() + "s";

        // Write target values in fields - KPIs
        staticFunctions.FindChildByRecursion(kpisMain.transform, "OEETitle").GetChild(0).GetComponent<TMP_Text>().text =
            "Target: " + Math.Round(valuesHandler.allStations.targetKPIs[station].oee, 1).ToString() + "%";
        staticFunctions.FindChildByRecursion(kpisMain.transform, "FPYTitle").GetChild(0).GetComponent<TMP_Text>().text =
            "Target: " + Math.Round(valuesHandler.allStations.targetKPIs[station].fpy, 1).ToString() + "%";
        staticFunctions.FindChildByRecursion(kpisMain.transform, "PartCountTitle").GetChild(0).GetComponent<TMP_Text>().text =
            "Target: " + Math.Round(valuesHandler.allStations.targetKPIs[station].partCount, 1).ToString();
        staticFunctions.FindChildByRecursion(kpisMain.transform, "PartCountNIOTitle").GetChild(0).GetComponent<TMP_Text>().text =
            "Target: " + Math.Round(valuesHandler.allStations.targetKPIs[station].countNIO, 1).ToString();
        staticFunctions.FindChildByRecursion(kpisMain.transform, "ProductivityTitle").GetChild(0).GetComponent<TMP_Text>().text =
            "Target: " + Math.Round(valuesHandler.allStations.targetKPIs[station].productivity, 1).ToString();
    }

    /// <summary>
    /// Update the values in the personal main windows.
    /// </summary>
    public void UpdatePersonalValues()
    {
        GameObject openedWindow = getOpenedPersonal();
        if (openedWindow != null && openedWindow != stationOverview)
        {
            //Get line and station number
            int line_number = Int32.Parse(
                staticFunctions.FindChildByRecursion(openedWindow.transform, "Title").GetComponent<TMP_Text>().text.Split(" /")[0].Split(": ")[1]
            );
            int station_number = Int32.Parse(
                staticFunctions.FindChildByRecursion(openedWindow.transform, "Title").GetComponent<TMP_Text>().text.Split("Station: ")[1]
            );

            // Write values in fields - Cycle Times
            staticFunctions.FindChildByRecursion(cycleTimesMain.transform, "CycleTimeTitle").GetComponent<TMP_Text>().text =
                "Cycle Time: " + Math.Round(valuesHandler.allStations.cycles[station_number].totalCycleTime, 1).ToString() + "s";
            staticFunctions.FindChildByRecursion(cycleTimesMain.transform, "ProcessTimeTitle").GetComponent<TMP_Text>().text =
                "Process Time: " + Math.Round(valuesHandler.allStations.cycles[station_number].processTime, 1).ToString() + "s";
            staticFunctions.FindChildByRecursion(cycleTimesMain.transform, "ExitTimeTitle").GetComponent<TMP_Text>().text =
                "Exit Time: " + Math.Round(valuesHandler.allStations.cycles[station_number].exitTime, 1).ToString() + "s";
            staticFunctions.FindChildByRecursion(cycleTimesMain.transform, "WaitingTimeTitle").GetComponent<TMP_Text>().text =
                "Waiting Time: " + Math.Round(valuesHandler.allStations.cycles[station_number].changeTime, 1).ToString() + "s";

            // Write values in fields - KPIs
            staticFunctions.FindChildByRecursion(kpisMain.transform, "OEETitle").GetComponent<TMP_Text>().text =
                "OEE: " + Math.Round(valuesHandler.allStations.kpis[station_number].oee, 1).ToString() + "%";
            staticFunctions.FindChildByRecursion(kpisMain.transform, "FPYTitle").GetComponent<TMP_Text>().text =
                "FPY: " + Math.Round(valuesHandler.allStations.kpis[station_number].fpy, 1).ToString() + "%";
            staticFunctions.FindChildByRecursion(kpisMain.transform, "PartCountTitle").GetComponent<TMP_Text>().text =
                "PartCount: " + Math.Round(valuesHandler.allStations.kpis[station_number].partCount, 1).ToString();
            staticFunctions.FindChildByRecursion(kpisMain.transform, "PartCountNIOTitle").GetComponent<TMP_Text>().text =
                "PartCount NOk: " + Math.Round(valuesHandler.allStations.kpis[station_number].countNIO, 1).ToString();
            staticFunctions.FindChildByRecursion(kpisMain.transform, "ProductivityTitle").GetComponent<TMP_Text>().text =
                "Productivity: " + Math.Round(valuesHandler.allStations.kpis[station_number].productivity, 1).ToString();

            UpdatePersonalCircles(station_number);

            WriteTargetValues(station_number);
        }
    }

    /// <summary>
    /// Call for the functions to paint every required circle.
    /// </summary>
    /// <param name="station_number">Station to paint the circles.</param>
    public void UpdatePersonalCircles(int station_number)
    {
        // Paint circles - Cycle Times
        float c1 = paintCircle(station_number, cycleTimesMain.transform, "totalCycleTime", true, "CycleTimeCircle", true);
        float c2 = paintCircle(station_number, cycleTimesMain.transform, "exitTime", true, "ExitTimeCircle", true);
        float c3 = paintCircle(station_number, cycleTimesMain.transform, "changeTime", true, "WaitingTimeCircle", true);
        float c4 = paintCircle(station_number, cycleTimesMain.transform, "processTime", true, "ProcessTimeCircle", true);

        // Paint circles - KPIs
        float k1 = paintCircle(station_number, kpisMain.transform, "oee", false, "OEECircle", false);
        float k2 = paintCircle(station_number, kpisMain.transform, "fpy", false, "FPYCircle", false);
        float k3 = paintCircle(station_number, kpisMain.transform, "partCount", false, "PartCountCircle", false);
        float k4 = paintCircle(station_number, kpisMain.transform, "countNIO", false, "PartCountNIOCircle", true);
        float k5 = paintCircle(station_number, kpisMain.transform, "productivity", false, "ProductivityCircle", false);

        float[] c = { c1, c2, c3, c4 };
        float[] k = { k1, k2, k3, k4, k5 };

        // Paint general circles
        paintGeneralCircles(c.Max(), "CycleTimesCircle", generalStationInfo.transform, 0);
        paintGeneralCircles(k.Max(), "KPIsCircle", generalStationInfo.transform, 0);

        // Paint bottleneck
        paintBottleneckStation(station_number, valuesHandler.getBottleneckStation(), generalStationInfo.transform, "BottleneckCircle", false);
    }

    /// <summary>
    /// Paint the bottleneck station.
    /// </summary>
    /// <param name="station_num">Station number.</param>
    /// <param name="bottleneck_station">Station that is the bottleneck.</param>
    /// <param name="t">Panel window.</param>
    /// <param name="circle">Circle to paint.</param>
    /// <param name="overview_size">If is the overview window.</param>
    private void paintBottleneckStation(int station_num, int bottleneck_station, Transform t, string circle, bool overview_size)
    {
        Vector3 small, big;
        if (!overview_size)
        {
            small = new Vector3(0.066f, 0.066f, 0.0000005f);
            big = new Vector3(0.2f, 0.2f, 0.0000005f);
        }
        else
        {
            small = new Vector3(0.0066f, 0.0066f, 0.0000005f);
            big = new Vector3(0.02f, 0.02f, 0.0000005f);
        }
        if (station_num != bottleneck_station)
        {
            staticFunctions.FindChildByRecursion(t, circle).GetComponent<Renderer>().material.color = new Color(0, 1, 0, 1);
            staticFunctions.FindChildByRecursion(t, circle).transform.localScale = small;
        }
        else
        {
            staticFunctions.FindChildByRecursion(t, circle).GetComponent<Renderer>().material.color = new Color(1, 0, 0, 1);
            staticFunctions.FindChildByRecursion(t, circle).transform.localScale = big;
        }
    }

    /// <summary>
    /// Paint the circles in the personal main cycle_times/kpis window.
    /// </summary>
    /// <param name="station_number">Station number.</param>
    /// <param name="t">Panel window.</param>
    /// <param name="varName">The indicator corresponding to the circle.</param>
    /// <param name="cycle_or_kpi">Is it cycle-times or kpis?</param>
    /// <param name="circle">The circle to paint.</param>
    /// <param name="positive">Is the indicator supposed to be higher or lower than the target value.</param>
    /// <returns></returns>
    private float paintCircle(int station_number, Transform t, string varName, bool cycle_or_kpi, string circle, bool positive)
    {
        Vector3 small = new Vector3(0.033f, 0.033f, 0.0000005f);
        float veredict = valuesHandler.checkThereshold(station_number, varName, cycle_or_kpi, positive);
        /*
        float r = ((255 * (veredict - 1)) / 100);
        float g = (255 * (100 - veredict)) / 100;
        float b = 0;
        */
        float r, g, b;
        if ((veredict - 1) <= 50)
        {
            r = 255 * (veredict - 1) / 100;
            g = 255;
        }
        else
        {
            r = 255;
            g = 255 - 255 * (veredict - 1) / 100;
        }
        b = 0;
        staticFunctions.FindChildByRecursion(t, circle).GetComponent<Renderer>().material.color = new Color(r/255, g/255, b/255, 1);
        staticFunctions.FindChildByRecursion(t, circle).transform.localScale = small * (((veredict * 2) / 100) + 1);
        return veredict;
    }

    /// <summary>
    /// Paint the general circles (main views).
    /// Paints red when one ore more of the indicators is red.
    /// Paints yellow when one or more of the indicators is yellow and none is red.
    /// Paints green otherwise.
    /// </summary>
    /// <param name="v1">First indicator.</param>
    /// <param name="v2">Second indicator.</param>
    /// <param name="v3">Third indicator.</param>
    /// <param name="v4">Forth indicator.</param>
    /// <param name="circle">Circle to be painted.</param>
    /// <param name="station">Station number.</param>
    /// <param name="size">To difine the size of the circle (it depends of the panel).</param>
    /// <param name="v5">Fifth indicator.</param>
    private void paintGeneralCircles(float veredict, string circle, Transform station, int size)
    {
        Vector3 small = Vector3.zero;
        switch (size)
        {
            case 0:
                small = new Vector3(0.066f, 0.066f, 0.0000005f);
                break;
            case 1:
                small = new Vector3(0.0066f, 0.0066f, 0.0000005f);
                break;
            case 2:
                small = new Vector3(0.02f, 0.02f, 0.0000005f);
                break;
            case 3:
                small = new Vector3(0.04f, 0.04f, 0.0000005f);
                break;
        }
        float r, g, b;
        if ((veredict - 1) <= 50)
        {
            r = 255 * (veredict - 1) / 100;
            g = 255;
        }
        else
        {
            r = 255;
            g = 255 - 255 * (veredict - 1) / 100;
        }
        b = 0;
        staticFunctions.FindChildByRecursion(station, circle).GetComponent<Renderer>().material.color = new Color(r/255, g/255, b/255, 1);
        staticFunctions.FindChildByRecursion(station, circle).transform.localScale = small * (((veredict * 2) / 100) + 1);
    }

    /// <summary>
    /// Fetch the values from the API.
    /// </summary>
    private void callSimulation()
    {
        //Simulate values
        //valuesHandler.simulateValues();
        for (int i = 0; i < contentStations.transform.childCount; i++)
        {
            Transform currentWindow = contentStations.transform.GetChild(i);

            int line_number = Int32.Parse(
                staticFunctions.FindChildByRecursion(currentWindow, "Title").GetComponent<TMP_Text>().text.Split(" /")[0].Split(": ")[1]
            );
            int station_number = Int32.Parse(
                staticFunctions.FindChildByRecursion(currentWindow, "Title").GetComponent<TMP_Text>().text.Split("Station: ")[1]
            );
            valuesHandler.fillDynamicDataAsset(line_number, station_number);
        }
    }

    /// <summary>
    /// Paint the circles situated in each station.
    /// Communicates with the paint general circles function.
    /// </summary>
    public void paintLoadCircles()
    {

        for (int i = 0; i < contentStations.transform.childCount; i++)
        {
            Transform currentWindow = contentStations.transform.GetChild(i);
            //Get line and station number
            int line_number = Int32.Parse(
                staticFunctions.FindChildByRecursion(currentWindow, "Title").GetComponent<TMP_Text>().text.Split(" /")[0].Split(": ")[1]
            );
            int station_number = Int32.Parse(
                staticFunctions.FindChildByRecursion(currentWindow, "Title").GetComponent<TMP_Text>().text.Split("Station: ")[1]
            );

            // Paint circles - Cycle Times
            //TODO:
            /*
            int c1 = valuesHandler.checkThereshold(station_number, "totalCycleTime", true, true);
            int c2 = valuesHandler.checkThereshold(station_number, "exitTime", true, true);
            int c3 = valuesHandler.checkThereshold(station_number, "changeTime", true, true);
            int c4 = valuesHandler.checkThereshold(station_number, "processTime", true, true);
            */

            float c1 = valuesHandler.checkThereshold(station_number, "totalCycleTime", true, true);
            float c2 = valuesHandler.checkThereshold(station_number, "exitTime", true, true);
            float c3 = valuesHandler.checkThereshold(station_number, "changeTime", true, true);
            float c4 = valuesHandler.checkThereshold(station_number, "processTime", true, true);

            // Paint circles - KPIs
            //TODO:
            /*
            int k1 = valuesHandler.checkThereshold(station_number, "oee", false, false);
            int k2 = valuesHandler.checkThereshold(station_number, "fpy", false, false);
            int k3 = valuesHandler.checkThereshold(station_number, "partCount", false, false);
            int k4 = valuesHandler.checkThereshold(station_number, "countNIO", false, true);
            int k5 = valuesHandler.checkThereshold(station_number, "productivity", false, false);
            */
            float k1 = valuesHandler.checkThereshold(station_number, "oee", false, false);
            float k2 = valuesHandler.checkThereshold(station_number, "fpy", false, false);
            float k3 = valuesHandler.checkThereshold(station_number, "partCount", false, false);
            float k4 = valuesHandler.checkThereshold(station_number, "countNIO", false, true);
            float k5 = valuesHandler.checkThereshold(station_number, "productivity", false, false);

            float[] c = { c1, c2, c3, c4 };
            float[] k = { k1, k2, k3, k4, k5 };

            if (currentWindow.tag.Equals("SpecificInfo"))
            {
                if (staticFunctions.FindChildByRecursion(currentWindow.transform, "SpecificTitle").GetComponent<TMP_Text>().text.Equals("Cycle Time"))
                {
                    paintGeneralCircles(c1, "CycleTimeCircle", currentWindow, 3);
                    paintGeneralCircles(c2, "ProcessTimeCircle", currentWindow, 2);
                    paintGeneralCircles(c3, "ExitTimeCircle", currentWindow, 2);
                    paintGeneralCircles(c4, "WaitingTimeCircle", currentWindow, 2);
                }
                else if (staticFunctions.FindChildByRecursion(currentWindow.transform, "SpecificTitle").GetComponent<TMP_Text>().text.Equals("FPY"))
                {
                    paintGeneralCircles(k1, "OEECircle", currentWindow, 2);
                    paintGeneralCircles(k2, "FPYCircle", currentWindow, 3);
                    paintGeneralCircles(k3, "PartCountCircle", currentWindow, 2);
                    paintGeneralCircles(k4, "PartCountNIOCircle", currentWindow, 2);
                    paintGeneralCircles(k5, "ProductivityCircle", currentWindow, 2);
                }
                else if (staticFunctions.FindChildByRecursion(currentWindow.transform, "SpecificTitle").GetComponent<TMP_Text>().text.Equals("Bottleneck"))
                {
                    paintBottleneckStation(station_number, valuesHandler.getBottleneckStation(), currentWindow, "BottleneckCircle", false);
                }
            }
            else
            {
                paintGeneralCircles(c.Max(), "CycleTimesCircle", currentWindow, 0);
                paintGeneralCircles(k.Max(), "KPIsCircle", currentWindow, 0);
                paintBottleneckStation(station_number, valuesHandler.getBottleneckStation(), currentWindow, "BottleneckCircle", false);
            }

            //Paint overview circles
            for(int j = 3; j < stationOverview.transform.childCount; j++)
            {
                if (!stationOverview.transform.GetChild(j).name.Equals("rigRoot")){
                    if (Int32.Parse(staticFunctions.FindChildByRecursion(stationOverview.transform.GetChild(j), "StationNumberText").GetComponent<TMP_Text>().text) ==
                        station_number)
                    {
                        paintGeneralCircles(c.Max(), "CycleTimesCircle", stationOverview.transform.GetChild(j), 1);
                        paintGeneralCircles(k.Max(), "KPIsCircle", stationOverview.transform.GetChild(j), 1);
                        paintBottleneckStation(station_number, valuesHandler.getBottleneckStation(), stationOverview.transform.GetChild(j), "BottleneckCircle", true);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Handles the bottleneck logic.
    /// An arrow will point to the bottleneck station.
    /// If the bottleneck changes the arrow will change accordingly and a dialog warning appears.
    /// </summary>
    private void bottleneckHandler()
    {
        int currentBottleneckStation = valuesHandler.getBottleneckStation();

        if (lastBottleneckStation != -1 && lastBottleneckStation != currentBottleneckStation)
        {
            bottleneckWarningDialog = Dialog.Open(dialogWarningPrefab, DialogButtonType.OK, "Bottleneck changed!", "Station " + valuesHandler.getBottleneckStation() + " is the current bottleneck.", true);
            Invoke("CloseBottleneckWarningDialog", 1.5f);
            soundDialogWarning.GetComponent<AudioSource>().Play();
        }

        lastBottleneckStation = currentBottleneckStation;

        for (int i = 0; i < contentStations.transform.childCount; i++)
        {
            Transform currentWindow = contentStations.transform.GetChild(i);
            //Get line and station number
            int line_number = Int32.Parse(
                staticFunctions.FindChildByRecursion(currentWindow, "Title").GetComponent<TMP_Text>().text.Split(" /")[0].Split(": ")[1]
            );
            int station_number = Int32.Parse(
                staticFunctions.FindChildByRecursion(currentWindow, "Title").GetComponent<TMP_Text>().text.Split("Station: ")[1]
            );

            if (station_number == currentBottleneckStation)
            {
                /*
                if (getOpenedPersonal() != null)
                {
                    getOpenedPersonal().SetActive(false);
                }*/
                bottleneckArrow.transform.position = head.transform.position + head.transform.forward * 0.8f;
                bottleneckArrow.transform.rotation = Quaternion.LookRotation(bottleneckArrow.transform.position - currentWindow.transform.position);
                if (Vector3.Distance(bottleneckArrow.transform.position, currentWindow.transform.position) < 1f)
                {
                    bottleneckArrow.transform.GetChild(0).gameObject.SetActive(false);
                }
                else
                {
                    bottleneckArrow.transform.GetChild(0).gameObject.SetActive(true);
                }
            }
        }
    }

    /// <summary>
    /// Close the bottleneck warning (indicating that the bottleneck was changed).
    /// Is called after a few seconds after the dialog opens.
    /// </summary>
    private void CloseBottleneckWarningDialog()
    {
        bottleneckWarningDialog.DismissDialog();
    }

    /// <summary>
    /// Clear the bottleneck auxiliar material.
    /// </summary>
    /// <param name="active">Is the bottleneck filtering active.</param>
    private void clearBottleneckHandler(bool active)
    {
        bottleneckArrow.SetActive(active);
        bottleneckArrow.transform.GetChild(0).gameObject.SetActive(true);
        lastBottleneckStation = -1;
    }

    /// <summary>
    /// Indicates that a station does not have data in the API.
    /// </summary>
    /// <param name="line">Line number.</param>
    /// <param name="station">Station number.</param>
    public void noDataInStation(int line, int station)
    {
        if (noDataDialog != null)
        {
            noDataDialog.DismissDialog();
            noDataDialog = Dialog.Open(dialogWarningPrefab, DialogButtonType.OK, "Station without data!", "Station " + station + " from line " + line + " has no data in the API.", true);
            Invoke("CloseDialogWarning2", 6f);
            soundDialogWarning.GetComponent<AudioSource>().Play();
            Invoke("cancelInv", 0.5f);
        }
    }

    /// <summary>
    /// Cancel the invocation of data availability function.
    /// </summary>
    public void cancelInv()
    {
        if (IsInvoking("dataAvailability"))
        {
            CancelInvoke("dataAvailability");
        }
    }
    
    /// <summary>
    /// Close the dialog warning for verifying the data availability after one second.
    /// </summary>
    public void CloseDialogWarning()
    {
        if (noDataDialog != null)
        {
            if (!IsInvoking("dataAvailability"))
            {
                Invoke("dataAvailability", 1f);
            }
        }
    }

    /// <summary>
    /// Verifies if every station in the line has data in the API.
    /// </summary>
    public void dataAvailability()
    {
        dataAvailable = true;
        for (int i = 0; i < contentStations.transform.childCount; i++)
        {
            Transform currentWindow = contentStations.transform.GetChild(i);

            int line_number = Int32.Parse(
                staticFunctions.FindChildByRecursion(currentWindow, "Title").GetComponent<TMP_Text>().text.Split(" /")[0].Split(": ")[1]
            );
            int station_number = Int32.Parse(
                staticFunctions.FindChildByRecursion(currentWindow, "Title").GetComponent<TMP_Text>().text.Split("Station: ")[1]
            );
            valuesHandler.fillDynamicDataAsset(line_number, station_number);
        }
        noDataDialog.DismissDialog();
        usabilityTest.userTestSetup();
        
    }

    /// <summary>
    /// Closes the warning saying that is no data in station X after a few seconds.
    /// </summary>
    public void CloseDialogWarning2()
    {
        if (noDataDialog != null)
        {
            noDataDialog.DismissDialog();
        }
    }

    /// <summary>
    /// Fill the overview panel with every station.
    /// </summary>
    /// <param name="line_number">Line number.</param>
    private void fillOverview(string line_number)
    {
        staticFunctions.FindChildByRecursion(stationOverview.transform, "Title").GetComponent<TMP_Text>().text = "Line " + line_number;
        for (int i = 0; i < contentStations.transform.childCount; i++)
        {
            GameObject spawnedButton = Instantiate(overviewStationPrefab, stationOverview.transform);
            string station_number = staticFunctions.FindChildByRecursion(contentStations.transform.GetChild(i), "Title").GetComponent<TMP_Text>().text.Split("Station: ")[1];
            staticFunctions.FindChildByRecursion(spawnedButton.transform, "StationNumberText").GetComponent<TMP_Text>().text = station_number;
            spawnedButton.SetActive(true);
        }
        stationOverview.gameObject.GetComponent<GridObjectCollection>().Rows = contentStations.transform.childCount + 2;
        stationOverview.gameObject.GetComponent<GridObjectCollection>().UpdateCollection();
    }

    /// <summary>
    /// Open the overview panel.
    /// </summary>
    public void openOverview()
    {
        GameObject nullPersonal = new GameObject();
        nullPersonal.transform.position = head.transform.position + head.transform.forward * 0.8f;
        nullPersonal.transform.rotation = Quaternion.LookRotation(nullPersonal.transform.position - head.transform.position);
        GameObject openedWindow = getOpenedPersonal();
        bool activo = stationOverview.activeSelf;
        if (openedWindow != null) openedWindow.SetActive(false);
        if (!activo)
        {
            stationOverview.SetActive(true);
        }
        if (openedWindow != null) staticFunctions.maintainTransform(openedWindow, stationOverview);
        else staticFunctions.maintainTransform(nullPersonal, stationOverview);

        Destroy(nullPersonal);
    }

    /// <summary>
    /// Open the personal station menu from the overview menu.
    /// </summary>
    /// <param name="button">Button pressed (to access the station to open).</param>
    public void openPersonalByOverview(GameObject button)
    {
        openPersonalWindowFromOverview(staticFunctions.FindChildByRecursion(button.transform, "StationNumberText").GetComponent<TMP_Text>().text);
    }

    /// <summary>
    /// Closes the station opening warning after 1.5seconds.
    /// </summary>
    private void closeOpenWarning()
    {
        openWarning.SetActive(false);
    }
}