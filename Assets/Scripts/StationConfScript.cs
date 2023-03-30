// ===============================
// AUTHOR     : Rafael Maio (rafael.maio@ua.pt)
// PURPOSE     : Station information during configuration.
// SPECIAL NOTES: X
// ===============================


using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StationConfScript : MonoBehaviour
{
    /// <summary>
    /// Slider to associate the station to the virtual sphere.
    /// </summary>
    public PinchSlider stationPinchSlider;

    /// <summary>
    /// Existing stations in the line.
    /// </summary>
    private int[] existingStations = { 10, 20, 30, 60, 70, 71, 72, 80, 90, 100, 110, 120, 140, 160, 170, 200, 210, 215, 220, 225, 230, 235, 240, 250, 260, 270, 273, 290, 300, 301, 310, 320, 330, 420, 510, 520, 710, 810, 830 };

    /// <summary>
    /// Unity Start function.
    /// </summary>
    private void Start()
    {
        stationPinchSlider.SliderStepDivisions = existingStations.Length;
    }

    /// <summary>
    /// Change the station number when the slider changes.
    /// </summary>
    public void changeStationNumber()
    {
        transform.GetChild(0).GetComponent<ToolTip>().ToolTipText = "Station: " + existingStations[(int)(stationPinchSlider.SliderValue * (existingStations.Length - 1))].ToString();
    }

    /// <summary>
    /// A new station has been selected.
    /// </summary>
    public void changeSelected()
    {
        FindObjectOfType<ConfigurationHandller>().updateSelected(this.gameObject);
        FindObjectOfType<ConfigurationHandller>().sceneChanged();
    }

    /// <summary>
    /// Set slider to a normalized value.
    /// </summary>
    /// <param name="station_number">Station being configured.</param>
    public void setSlider(int station_number)
    {
        if ((float)Array.IndexOf(existingStations, station_number) < existingStations.Length / 2)
        {
            stationPinchSlider.SliderValue = (float)Array.IndexOf(existingStations, station_number) / (existingStations.Length - 2);
        }
        else
        {
            stationPinchSlider.SliderValue = (float)Array.IndexOf(existingStations, station_number) / (existingStations.Length - 1);
        }
    }
}