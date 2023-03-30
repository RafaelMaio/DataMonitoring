// ===============================
// AUTHOR     : Rafael Maio (rafael.maio@ua.pt)
// PURPOSE     : Handles the initial menu logic.
// SPECIAL NOTES: X
// ===============================

using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainMenuHandler : MonoBehaviour
{
    /// <summary>
    /// Slider for choosing the line.
    /// </summary>
    public PinchSlider linePinchSlider;

    /// <summary>
    /// Text where the line number selected currently appears.
    /// </summary>
    public TMP_Text lineNumberText;

    /// <summary>
    /// Communication with the Configuration Handler script.
    /// </summary>
    public ConfigurationHandller configurationHandller;

    /// <summary>
    /// Communication with the Line Monitoring Handler script.
    /// </summary>
    public LineMonitoringHandler lineMonitoringHandler;

    /// <summary>
    /// Change the line scenario using the slider.
    /// </summary>
    public void changeLineScenario()
    {
        lineNumberText.text = ((int)(linePinchSlider.SliderValue * 10)).ToString();
    }

    /// <summary>
    /// Set the line scenario for configuration.
    /// </summary>
    public void setConfLineScenario()
    {
        configurationHandller.setLineScenario((int)(linePinchSlider.SliderValue * 10));
        configurationHandller.enabled = true;
    }

    /// <summary>
    /// Set the line scenario for visualization.
    /// </summary>
    public void setVisLineScenario()
    {
        lineMonitoringHandler.setLineScenario((int)(linePinchSlider.SliderValue * 10));
        lineMonitoringHandler.enabled = true;
    }
}