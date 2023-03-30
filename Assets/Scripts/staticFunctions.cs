// ===============================
// AUTHOR     : Rafael Maio (rafael.maio@ua.pt)
// PURPOSE     : Static function to be accessed by any script.
// SPECIAL NOTES: X
// ===============================

using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class staticFunctions : MonoBehaviour
{
    /// <summary>
    /// Pinch threshold.
    /// </summary>
    private const float PinchThreshold = 0.7f;

    /// <summary>
    /// Access to the hand joints.
    /// </summary>
    private static IMixedRealityHandJointService handJointService;

    /// <summary>
    /// Access to the hand joints.
    /// </summary>
    private static IMixedRealityHandJointService HandJointService =>
        handJointService ?? (handJointService = CoreServices.GetInputSystemDataProvider<IMixedRealityHandJointService>());

    /// <summary>
    /// Find a transform child from an object.
    /// </summary>
    /// <param name="aParent">Transform from the parent game object.</param>
    /// <param name="aName">Child game object name.</param>
    /// <returns>The child transform.</returns>
    static public Transform FindChildByRecursion(Transform aParent, string aName)
    {
        if (aParent == null) return null;
        var result = aParent.Find(aName);
        if (result != null)
            return result;
        foreach (Transform child in aParent)
        {
            result = FindChildByRecursion(child, aName);
            if (result != null)
                return result;
        }
        return null;
    }

    /// <summary>
    /// Verifies if the hand is pinching.
    /// </summary>
    /// <param name="trackedHand">The hand being tracked.</param>
    /// <returns>If the hand is pinching.</returns>
    public static bool IsPinching(Handedness trackedHand)
    {
        return HandPoseUtils.CalculateIndexPinch(trackedHand) > PinchThreshold;
    }

    /// <summary>
    /// Maintain the transform when chaning the personal window.
    /// </summary>
    /// <param name="closed">The window being closed.</param>
    /// <param name="opened">The window being opened.</param>
    public static void maintainTransform(GameObject closed, GameObject opened)
    {
        opened.transform.position = closed.transform.position;
        opened.transform.rotation = closed.transform.rotation;
        opened.transform.localScale = closed.transform.localScale;
    }

    /// <summary>
    /// Verify if the user is pointing to a station.
    /// </summary>
    /// <returns>If the user is pointing to a station, and which station is he pointing at.</returns>
    public static Tuple<bool, GameObject> isPointing(GameObject nullGameObject)
    {
        if (CoreServices.InputSystem.DetectedInputSources != null)
        {
            foreach (var source in CoreServices.InputSystem.DetectedInputSources)
            {
                // Ignore anything that is not a hand because we want articulated hands
                if (source.SourceType == InputSourceType.Hand)
                {
                    foreach (var p in source.Pointers)
                    {
                        if (p is IMixedRealityNearPointer)
                        {
                            // Ignore near pointers, we only want the rays
                            continue;
                        }
                        if (p.Result != null)
                        {
                            var hitObject = p.Result.Details.Object;
                            if (hitObject)
                            {
                                if (hitObject.tag.Equals("GeneralInfo") || hitObject.tag.Equals("SpecificInfo"))
                                {
                                    return Tuple.Create(true, hitObject);
                                }
                            }
                        }
                    }
                }
            }
        }
        return Tuple.Create(false, nullGameObject);
    }

    /// <summary>
    /// Verify if the user is doing the pinch gesture.
    /// </summary>
    /// <param name="hand">Users hand.</param>
    /// <returns>If the user is pincing or not.</returns>
    public static bool isPinching(Handedness hand)
    {
        if (HandJointService.IsHandTracked(hand) && IsPinching(hand))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Gets the path for the child gameobject.
    /// </summary>
    /// <param name="go">Game Object to obtain the full path from.</param>
    /// <returns>The full path for the child gameobject.</returns>
    public static string getPathName(GameObject go)
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

    public static float calculateDistance(Vector3 panel, Vector3 head)
    {
        return (float)Math.Sqrt(Math.Pow(panel.x - head.x, 2) + 
            Math.Pow(panel.y - head.y, 2) + 
            Math.Pow(panel.z - head.z, 2));
    }

    public static bool betweenDistance(GameObject panel, GameObject head, float distance)
    {
        return (calculateDistance(panel.transform.position, head.transform.position) <= distance);
    }
}