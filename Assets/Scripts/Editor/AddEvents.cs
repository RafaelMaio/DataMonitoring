// ===============================
// AUTHOR     : Rafael Maio (rafael.maio@ua.pt)
// PURPOSE     : Add events to buttons automatically.
// SPECIAL NOTES: This way, no button is forgoten.
// ===============================

using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class AddEvents : Editor
{
    /// <summary>
    /// Add event to buttons and sliders.
    /// </summary>
    [MenuItem("UserTests/Add scripts")]
    static void addScripts()
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        UsabilityTest usabilityTest = FindObjectOfType<UsabilityTest>();
        foreach (GameObject go in allObjects)
        {
            if (go.GetComponent<PressableButtonHoloLens2>() != null)
            {
                //UnityEditor.Events.UnityEventTools.RemovePersistentListener(go.GetComponent<Interactable>().OnClick, go.GetComponent<Interactable>().OnClick.GetPersistentEventCount() - 1);
                UnityEditor.Events.UnityEventTools.AddStringPersistentListener(go.GetComponent<Interactable>().OnClick, usabilityTest.writeEvent, getPathName(go));
            }
            else if(go.GetComponent<PinchSlider>() != null)
            {
                //UnityEditor.Events.UnityEventTools.RemovePersistentListener(go.GetComponent<PinchSlider>().OnInteractionEnded, go.GetComponent<PinchSlider>().OnInteractionEnded.GetPersistentEventCount() - 1);
                UnityEditor.Events.UnityEventTools.AddStringPersistentListener(go.GetComponent<PinchSlider>().OnInteractionEnded, usabilityTest.writeEvent, getPathName(go));
            }
        }
    }

    /// <summary>
    /// Gets the path for the child gameobject.
    /// </summary>
    /// <param name="go">Game Object to obtain the full path from.</param>
    /// <returns>The full path for the child gameobject.</returns>
    static string getPathName(GameObject go)
    {
        string pathName = go.name;
        GameObject auxGo = go;
        while (auxGo.transform.parent != null)
        {
            pathName = auxGo.transform.parent.name + "/" + pathName;
            auxGo = auxGo.transform.parent.gameObject;
        }
        return pathName;
    }
}