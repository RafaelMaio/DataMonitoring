// ===============================
// AUTHOR     : Rafael Maio (rafael.maio@ua.pt)
// PURPOSE     : Script for debugging in HoloLens.
// SPECIAL NOTES: Not useful for the application well-functioning.
// ===============================

using TMPro;
using UnityEngine;

public class DebugWindow : MonoBehaviour
{
    /// <summary>
    /// Where the text is written.
    /// </summary>
    public TMP_Text textMesh;

    /// <summary>
    /// Unity's OnEnable function.
    /// </summary>
    void OnEnable()
    {
        Application.logMessageReceived += LogMessage;
    }

    /// <summary>
    /// Unity's OnDisable function.
    /// </summary>
    void OnDisable()
    {
        Application.logMessageReceived -= LogMessage;
    }

    /// <summary>
    /// Writes the Debug.Log(message) message.
    /// </summary>
    public void LogMessage(string message, string stackTrace, LogType type)
    {
        if (textMesh.text.Length > 500)
        {
            textMesh.text = message + "\n";
        }
        else
        {
            textMesh.text += message + "\n";
        }
    }
}