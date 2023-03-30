// ===============================
// AUTHOR     : Rafael Maio (rafael.maio@ua.pt)
// PURPOSE     : Script for deleting the anchor storage.
// SPECIAL NOTES: X
// ===============================

using Microsoft.MixedReality.WorldLocking.Core;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.XR.WSA;

public class AnchorManager : MonoBehaviour
{
    /// <summary>
    /// Reset the anchor storage of all lines.
    /// </summary>
    public void anchorReset()
    {
        WorldLockingManager.GetInstance().Reset();
        if (Directory.Exists(Application.persistentDataPath))
        {
            string worldsFolder = Application.persistentDataPath;

            DirectoryInfo d = new DirectoryInfo(worldsFolder);
            foreach (var file in d.GetFiles("scene_content*"))
            {
                File.Delete(file.FullName);
            }
        }
        WorldLockingManager.GetInstance().Save();
    }
}