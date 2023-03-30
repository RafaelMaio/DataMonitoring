// ===============================
// AUTHOR     : Rafael Maio (rafael.maio@ua.pt)
// PURPOSE     : Handles the navegation between menus
// SPECIAL NOTES: X
// ===============================

using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenuHandler : MonoBehaviour
{
    /// <summary>
    /// Main menu window.
    /// </summary>
    public GameObject mainMenu;

    /// <summary>
    /// Configuration window.
    /// </summary>
    public GameObject configurationMenu;

    /// <summary>
    /// Visualization window.
    /// </summary>
    public GameObject lineMonitoringMenu;

    /// <summary>
    /// The current menu opened.
    /// </summary>
    private GameObject currentMenu;

    /// <summary>
    /// All menus to navigate (main, configuration and visualization).
    /// </summary>
    private List<GameObject> allMenus = new List<GameObject>();

    /// <summary>
    /// Manager game object.
    /// </summary>
    public GameObject manager;

    /// <summary>
    /// Dialog warning for saving the configuration.
    /// </summary>
    public GameObject saveConfDialog;

    /// <summary>
    /// Button to filter visualization.
    /// </summary>
    public GameObject filterButton;

    /// <summary>
    /// Button to cancel the filtering menu.
    /// </summary>
    public GameObject cancelButton;

    /// <summary>
    /// Collection with the possible filter buttons.
    /// </summary>
    public GameObject filterCollection;

    /// <summary>
    /// Bar for indicating the task for user tests purposes. 
    /// </summary>
    public GameObject usabilityTaskBar;

    /// <summary>
    /// The number of the current task for user tests purposes.
    /// </summary>
    private int taskCounter;

    /// <summary>
    /// Communication with the Usability Test script.
    /// </summary>
    public UsabilityTest usabilityTest;

    /// <summary>
    /// Adaptation window.
    /// </summary>
    public GameObject adaptationMenu;

    /// <summary>
    /// The number of the current task for adaptation purposes.
    /// </summary>
    private int adaptationTaskCounter;

    /// <summary>
    /// Communitcation with the Adaptation script.
    /// </summary>
    public AdaptationPeriod adaptationPeriod;

    /// <summary>
    /// Unity Start function.
    /// </summary>
    private void Start()
    {
        currentMenu = mainMenu;
        allMenus.Add(mainMenu);
        allMenus.Add(configurationMenu);
        allMenus.Add(lineMonitoringMenu);
        allMenus.Add(adaptationMenu);

        taskCounter = 1;
        adaptationTaskCounter = 1;
    }

    /// <summary>
    /// Enable menus when the left hand palm is in view.
    /// </summary>
    public void enableCurrentMenu()
    {
        currentMenu.SetActive(true);
    }

    /// <summary>
    /// Disable menus when the left hand palm is out of view.
    /// </summary>
    public void disableMenus()
    {
        foreach (var menu in allMenus)
        {
            menu.SetActive(false);
        }
    }

    /// <summary>
    /// Change menu when buttons are clicked.
    /// </summary>
    /// <param name="menu">Menu to chage.</param>
    public void changeMenu(string menu)
    {
        if (menu.Equals("main"))
        {
            currentMenu = mainMenu; 
            if (manager.GetComponent<ConfigurationHandller>().enabled)
            {
                if (!manager.GetComponent<ConfigurationHandller>().getSavedStatus())
                {
                    Dialog saveDialog = Dialog.Open(saveConfDialog, DialogButtonType.Yes | DialogButtonType.No, "Configuration warning!", "The configuration is not saved. Do you wish to save the configuraion?", true);
                    if (saveDialog != null)
                    {
                        saveDialog.OnClosed += OnClosedSaveDialogEvent;
                    }
                }
                else
                {
                    manager.GetComponent<ConfigurationHandller>().enabled = false;
                }

                if (manager.GetComponent<ConfigurationHandller>().IsInvoking())
                {
                    manager.GetComponent<ConfigurationHandller>().CancelInvoke();
                }
            }
            else
            {
                manager.GetComponent<LineMonitoringHandler>().enabled = false;
                if (manager.GetComponent<LineMonitoringHandler>().IsInvoking())
                {
                    manager.GetComponent<LineMonitoringHandler>().CancelInvoke();
                }
            }
        }
        else if (menu.Equals("configuration"))
        {
            currentMenu = configurationMenu;
        }
        else if (menu.Equals("lineMonitoring"))
        {
            currentMenu = lineMonitoringMenu;
        }

        else if (menu.Equals("adaptation"))
        {
            currentMenu = adaptationMenu;
        }
        disableMenus();
        enableCurrentMenu();
    }

    /// <summary>
    /// Close the dialog warning for saving configuration.
    /// </summary>
    /// <param name="obj">Yes or No (to save the configuration).</param>
    private void OnClosedSaveDialogEvent(DialogResult obj)
    {
        if (obj.Result == DialogButtonType.Yes)
        {
            manager.GetComponent<ConfigurationHandller>().saveConfiguration();
        }
        manager.GetComponent<ConfigurationHandller>().enabled = false;
    }

    /// <summary>
    /// Open the filtering collection.
    /// </summary>
    public void FilterPressed()
    {
        filterButton.SetActive(false);
        cancelButton.SetActive(true);
        filterCollection.SetActive(true);
    }

    /// <summary>
    /// Close the filtering collection.
    /// </summary>
    public void CancelFilterPressed()
    {
        filterButton.SetActive(true);
        cancelButton.SetActive(false);
        filterCollection.SetActive(false);
    }

    /// <summary>
    /// Show/Hide the usability task bar presenting the current task.
    /// </summary>
    /// <param name="showHide">Show or hide the bar.</param>
    public void showHideUsabilityTaskBar(bool showHide)
    {
        usabilityTaskBar.SetActive(showHide);
    }

    /// <summary>
    /// Task completed.
    /// Move to the next task.
    /// For user tests purposes.
    /// </summary>
    public void nextTask()
    {
        switch (taskCounter)
        {
            case 1:
                staticFunctions.FindChildByRecursion(usabilityTaskBar.transform, "TaskText").GetComponent<TextMeshPro>().text =
                    "2 - Verify which KPIs are below the target in station 10.";
                break;
            case 2:
                staticFunctions.FindChildByRecursion(usabilityTaskBar.transform, "TaskText").GetComponent<TextMeshPro>().text =
                    "3 - Walk to the station causing the line bottlenck.";
                break;
            case 3:
                staticFunctions.FindChildByRecursion(usabilityTaskBar.transform, "TaskText").GetComponent<TextMeshPro>().text =
                    "4 - Summarize task 1-3 in the paper.";
                break;
            case 4:
                staticFunctions.FindChildByRecursion(usabilityTaskBar.transform, "TaskText").GetComponent<TextMeshPro>().text =
                    "5 - Approximately, what is the minimum and maximum PartCount value of station 70 in the last 60 shifts?";
                break;
            case 5:
                staticFunctions.FindChildByRecursion(usabilityTaskBar.transform, "TaskText").GetComponent<TextMeshPro>().text =
                    "6 - Indicate the process time value of station 90 10 shifts ago.";
                break;
            case 6:
                staticFunctions.FindChildByRecursion(usabilityTaskBar.transform, "TaskText").GetComponent<TextMeshPro>().text =
                    "7 - Which station had the best total cycle time in the last 10 samples? (stations 20, 70, 71 and 100).";
                break;
            case 7:
                staticFunctions.FindChildByRecursion(usabilityTaskBar.transform, "TaskText").GetComponent<TextMeshPro>().text =
                    "8 - Observe the OEE between 20 and 30 samples, from station 110, and tell which ones are above the target.";
                break;
            case 8:
                staticFunctions.FindChildByRecursion(usabilityTaskBar.transform, "TaskText").GetComponent<TextMeshPro>().text =
                    "User test completed!";
                break;
        }
        usabilityTest.stopTime(taskCounter);
        taskCounter += 1;
    }

    /// <summary>
    /// Task completed.
    /// Move to the next task.
    /// For adaptation purposes.
    /// </summary>
    public void adaptionNextTask()
    {
        switch (adaptationTaskCounter)
        {
            case 1:
                staticFunctions.FindChildByRecursion(adaptationMenu.transform, "TaskText").GetComponent<TextMeshPro>().text =
                    "2 - Open the partCount menu from KPIs.";
                break;
            case 2:
                staticFunctions.FindChildByRecursion(adaptationMenu.transform, "TaskText").GetComponent<TextMeshPro>().text =
                    "3 - Try the slider. Close the panel after trying the slider.";
                break;
            case 3:
                adaptationPeriod.destroyPanels();
                staticFunctions.FindChildByRecursion(adaptationMenu.transform, "TaskText").GetComponent<TextMeshPro>().text =
                    "Adaptation done, you are ready! Go back.";
                break;
        }
        adaptationTaskCounter += 1;
        adaptationPeriod.nextAdaptationTask(adaptationTaskCounter);
    }
}