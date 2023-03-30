// ===============================
// AUTHOR     : Rafael Maio (rafael.maio@ua.pt)
// PURPOSE     : Handles the configuraion module.
// SPECIAL NOTES: X
// ===============================

using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.WorldLocking.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using TMPro;
using UnityEngine;

public class ConfigurationHandller : MonoBehaviour
{
    /// <summary>
    /// Sphere prefab.
    /// To represent the real-world station location.
    /// </summary>
    public GameObject stationPrefab;

    /// <summary>
    /// Representation of the HMD.
    /// To access its pose.
    /// </summary>
    public GameObject head;

    /// <summary>
    /// Material for coloring the station selected (green).
    /// </summary>
    public Material selectedMaterial;

    /// <summary>
    /// Material for coloring the stations that are not selected (gray).
    /// </summary>
    public Material notSelectedMaterial;

    /// <summary>
    /// Station object currently selected.
    /// </summary>
    private GameObject selectedStation;

    /// <summary>
    /// Parent from all stations.
    /// </summary>
    public GameObject contentStations;

    /// <summary>
    /// Path to application persisent data +
    /// specific file for the line selected.
    /// </summary>
    private string path;

    /// <summary>
    /// Is the current configuration already saved.
    /// </summary>
    private bool saved = false;

    /// <summary>
    /// Dialog prefab.
    /// </summary>
    public GameObject dialogWarningPrefab;

    /// <summary>
    /// Dialog for warning users that the stations configured don't have data in the API.
    /// </summary>
    private Dialog noDataDialog;

    /// <summary>
    /// Sound when the dialog appears.
    /// </summary>
    public AudioSource soundDialogWarning;

    /// <summary>
    /// Communication with the Values Handler script.
    /// For accessing the API (verifying if it has data).
    /// </summary>
    public ValuesHandler valuesHandler;

    /// <summary>
    /// Add station to the scene.
    /// </summary>
    public void addStation()
    {
        Vector3 headPos = head.transform.position;

        // Add station if front of the user
        Vector3 position = headPos + head.transform.forward * 0.6f;
        GameObject spawnedStation = Instantiate(stationPrefab, position, transform.rotation);
        spawnedStation.transform.parent = contentStations.transform;
        
        // Select the new station
        if(selectedStation != null)
        {
            selectedStation.GetComponent<Renderer>().material = notSelectedMaterial;
            for(int i = 0; i < selectedStation.transform.childCount; i++)
            {
                selectedStation.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
        selectedStation = spawnedStation;
        spawnedStation.GetComponent<Renderer>().material = selectedMaterial;
    }

    /// <summary>
    /// Delete the selected station.
    /// </summary>
    public void removeStation()
    {
        Destroy(selectedStation);
    }

    /// <summary>
    /// Save the current line configuration.
    /// Including the anchors.
    /// </summary>
    public void saveConfiguration()
    {
        noDataDialog = Dialog.Open(dialogWarningPrefab, DialogButtonType.OK, "Data verification.", "Verifying if all stations have API access.", true);
        WorldLockingManager.GetInstance().Save();
        File.WriteAllText(path, string.Empty);
        using (StreamWriter sw = File.AppendText(path))
        {
            for (int i = 0; i < contentStations.transform.childCount; i++)
            {
                sw.WriteLine(
                contentStations.transform.GetChild(i).position.x + ";" +
                contentStations.transform.GetChild(i).position.y + ";" +
                contentStations.transform.GetChild(i).position.z + ";" +
                contentStations.transform.GetChild(i).rotation.eulerAngles.x + ";" +
                contentStations.transform.GetChild(i).rotation.eulerAngles.y + ";" +
                contentStations.transform.GetChild(i).rotation.eulerAngles.z + ";" +
                contentStations.transform.GetChild(i).GetChild(0).GetComponent<ToolTip>().ToolTipText.Split(" ")[1]
                );

                string line = path.Split("scene_content")[1].Replace(".txt", "");
                valuesHandler.verifyDataInStation(Int32.Parse(line), Int32.Parse(contentStations.transform.GetChild(i).GetChild(0).GetComponent<ToolTip>().ToolTipText.Split(" ")[1]));
            }
        }
        saved = true;
    }

    /// <summary>
    /// The corresponding station does not have data in the API.
    /// </summary>
    /// <param name="line">Corresponding line.</param>
    /// <param name="station">Corresponding station.</param>
    public void noDataInStation(int line, int station)
    {
        if (noDataDialog != null)
        {
            noDataDialog.DismissDialog();
            noDataDialog = Dialog.Open(dialogWarningPrefab, DialogButtonType.OK, "Station without data!", "Station " + station + " from line " + line + " has no data in the API.", true);
            Invoke("CloseDialogWarning", 6f);
            soundDialogWarning.GetComponent<AudioSource>().Play();
        }
    }

    /// <summary>
    /// Clear the line configuration.
    /// </summary>
    public void clearLineConfiguration()
    {
        for (int i = 0; i < contentStations.transform.childCount; i++)
        {
            Destroy(contentStations.transform.GetChild(i).gameObject);
        }
        selectedStation = null;
        File.WriteAllText(path, string.Empty);
    }

    /// <summary>
    /// Set the path for the corresponding line.
    /// </summary>
    /// <param name="scenario">Line number.</param>
    public void setLineScenario(int scenario)
    {
        path = Path.Combine(Application.persistentDataPath, "scene_content" + scenario.ToString() + ".txt");
    }

    /// <summary>
    /// Unity's OnDisable function.
    /// Deletes the GameObjects not useful out of this scene.
    /// </summary>
    private void OnDisable()
    {
        for (int i = 0; i < contentStations.transform.childCount; i++)
        {
            Destroy(contentStations.transform.GetChild(i).gameObject);
        }
        selectedStation = null;
    }

    /// <summary>
    /// Unity's OnEnable function.
    /// Loads the GameObjects from the previous configuration in the corresponding line.
    /// </summary>
    private void OnEnable()
    {
        WorldLockingManager.GetInstance().Load();
        saved = false;
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
                    GameObject spawnedStation = Instantiate(stationPrefab, pos, Quaternion.Euler(rot));
                    spawnedStation.transform.parent = contentStations.transform;
                    spawnedStation.GetComponent<StationConfScript>().setSlider(Int32.Parse(line_splitted[6]));
                    spawnedStation.transform.GetChild(0).gameObject.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// Updates the current selected station.
    /// </summary>
    /// <param name="go">Station selected.</param>
    public void updateSelected(GameObject go)
    {
        if (!go.Equals(selectedStation))
        {
            if (selectedStation != null)
            {
                selectedStation.GetComponent<Renderer>().material = notSelectedMaterial;
                selectedStation.transform.GetChild(0).gameObject.SetActive(false);
            }
            selectedStation = go;
            selectedStation.GetComponent<Renderer>().material = selectedMaterial;
            selectedStation.transform.GetChild(0).gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Verify if the configuration is already saved.
    /// </summary>
    /// <returns>If the configuration is already saved or not.</returns>
    public bool getSavedStatus()
    {
        return saved;
    }

    /// <summary>
    /// If the scene changes, the configuration is not saved anymore.
    /// </summary>
    public void sceneChanged()
    {
        saved = false;
    }

    /// <summary>
    /// Close the Dialog Warning.
    /// Called a few seconds after opening.
    /// </summary>
    public void CloseDialogWarning()
    {
        if (noDataDialog != null)
        {
            if (!IsInvoking("CloseDialogWarning"))
            {
                noDataDialog.DismissDialog();
            }
        }
    }
}