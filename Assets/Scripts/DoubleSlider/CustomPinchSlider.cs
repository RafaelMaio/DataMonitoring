// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.Toolkit.Input;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.UI
{
    /// <summary>
    /// A slider that can be moved by grabbing / pinching a slider thumb
    /// </summary>
    [HelpURL("https://docs.microsoft.com/windows/mixed-reality/mrtk-unity/features/ux-building-blocks/sliders")]
    [AddComponentMenu("Scripts/MRTK/SDK/CustomPinchSlider")]
    public class CustomPinchSlider : MonoBehaviour, IMixedRealityPointerHandler, IMixedRealityFocusHandler, IMixedRealityTouchHandler
    {
        #region Serialized Fields and Public Properties
        [Tooltip("The gameObject that contains the slider thumb.")]
        [SerializeField]
        private GameObject thumbRootMax = null;
        public GameObject ThumbRootMax
        {
            get
            {
                return ThumbRootMax;
            }
            set
            {
                ThumbRootMax = value;
                InitializeSliderThumb();
            }
        }
        [SerializeField]
        private GameObject thumbRootMin = null;
        public GameObject ThumbRootMin
        {
            get
            {
                return thumbRootMin;
            }
            set
            {
                thumbRootMin = value;
                InitializeSliderThumb();
            }
        }


        [SerializeField]
        [Tooltip("Whether or not this slider is controllable via touch events")]
        private bool isTouchable;

        /// <summary>
        /// Property accessor of isTouchable. Determines whether or not this slider is controllable via touch events
        /// </summary>
        public bool IsTouchable
        {
            get { return isTouchable; }
            set { isTouchable = value; }
        }

        [SerializeField]
        [Tooltip("Whether or not this slider snaps to the designated position on the slider")]
        private bool snapToPosition;

        /// <summary>
        /// Property accessor of snapToPosition. Determines whether or not this slider snaps to the designated position on the slider
        /// </summary>
        public bool SnapToPosition
        {
            get { return snapToPosition; }
            set
            {
                snapToPosition = value;
                if (!touchCollider.IsNull())
                {
                    touchCollider.enabled = value;
                }
                if (!thumbColliderMax.IsNull())
                {
                    thumbColliderMax.enabled = !value;
                }
            }
        }

        [SerializeField]
        /// <summary>
        /// White-Gray on the root object.
        /// </summary>
        public Material default_material;

        [SerializeField]
        /// <summary>
        /// White-Gray on the root object.
        /// </summary>
        public Material dragging_material;

        public maintainLineRenderer maintainLine;

        [SerializeField]
        /// <summary>
        /// Used to control the slider on the track when snapToPosition is false
        /// </summary>
        private Collider thumbColliderMax = null;

        /// <summary>
        /// Property accessor of thumbCollider. Used to control the slider on the track when snapToPosition is false
        /// </summary>
        public Collider ThumbColliderMax
        {
            get { return thumbColliderMax; }
            set { thumbColliderMax = value; }
        }

        [SerializeField]
        /// <summary>
        /// Used to control the slider on the track when snapToPosition is false
        /// </summary>
        private Collider thumbColliderMin = null;

        /// <summary>
        /// Property accessor of thumbCollider. Used to control the slider on the track when snapToPosition is false
        /// </summary>
        public Collider ThumbColliderMin
        {
            get { return thumbColliderMin; }
            set { thumbColliderMin = value; }
        }
        [SerializeField]
        /// <summary>
        /// Used to determine the position we snap the slider do when snapToPosition is true
        /// </summary>
        private Collider touchCollider = null;

        /// <summary>
        /// Property accessor of touchCollider. Used to determine the position we snap the slider do when snapToPosition is true
        /// </summary>
        public Collider TouchCollider
        {
            get { return touchCollider; }
            set { touchCollider = value; }
        }

        [Range(minVal, maxVal)]
        [SerializeField]
        private float sliderValueMax = 0.5f;
        public float SliderValueMax
        {
            get { return sliderValueMax; }
            set
            {
                var oldSliderValue = sliderValueMax;
                sliderValueMax = value;
                UpdateUI();
                OnValueUpdated.Invoke(new CustomSliderEventData(oldSliderValue, value, ActivePointer, this));
            }
        }

        [Range(minVal, maxVal)]
        [SerializeField]
        private float sliderValueMin = 0.4f;
        public float SliderValueMin
        {
            get { return sliderValueMin; }
            set
            {
                var oldSliderValue = sliderValueMin;
                sliderValueMin = value;
                UpdateUI();
                OnValueUpdated.Invoke(new CustomSliderEventData(oldSliderValue, value, ActivePointer, this));
            }
        }

        [SerializeField]
        [Tooltip("Controls whether this slider is increments in steps or continuously")]
        private bool useSliderStepDivisions;

        /// <summary>
        /// Property accessor of useSliderStepDivisions, it determines whether the slider steps according to subdivisions
        /// </summary>
        public bool UseSliderStepDivisions
        {
            get { return useSliderStepDivisions; }
            set { useSliderStepDivisions = value; }
        }

        [SerializeField]
        [Min(1)]
        [Tooltip("Number of subdivisions the slider is split into.")]
        private int sliderStepDivisions = 1;

        /// <summary>
        /// Property accessor of sliderStepDivisions, it holds the number of subdivisions the slider is split into.
        /// </summary>
        public int SliderStepDivisions
        {
            get { return sliderStepDivisions; }
            set { sliderStepDivisions = value; }
        }

        [Header("Slider Axis Visuals")]

        [Tooltip("The gameObject that contains the trackVisuals. This will get rotated to match the slider axis")]
        [SerializeField]
        private GameObject trackVisuals = null;
        /// <summary>
        /// Property accessor of trackVisuals, it contains the desired track Visuals. This will get rotated to match the slider axis.
        /// </summary>
        public GameObject TrackVisuals
        {
            get
            {
                return trackVisuals;
            }
            set
            {
                if (trackVisuals != value)
                {
                    trackVisuals = value;
                    UpdateTrackVisuals();
                }
            }
        }

        [Tooltip("The gameObject that contains the tickMarks.  This will get rotated to match the slider axis")]
        [SerializeField]
        private GameObject tickMarks = null;
        /// <summary>
        /// Property accessor of tickMarks, it contains the desired tick Marks.  This will get rotated to match the slider axis.
        /// </summary>
        public GameObject TickMarks
        {
            get
            {
                return tickMarks;
            }
            set
            {
                if (tickMarks != value)
                {
                    tickMarks = value;
                    UpdateTickMarks();
                }
            }
        }

        [Tooltip("The gameObject that contains the thumb visuals.  This will get rotated to match the slider axis.")]
        [SerializeField]
        private GameObject thumbVisuals = null;
        /// <summary>
        /// Property accessor of thumbVisuals, it contains the desired tick marks.  This will get rotated to match the slider axis.
        /// </summary>
        public GameObject ThumbVisuals
        {
            get
            {
                return thumbVisuals;
            }
            set
            {
                if (thumbVisuals != value)
                {
                    thumbVisuals = value;
                    UpdateThumbVisuals();
                }
            }
        }


        [Header("Slider Track")]

        [Tooltip("The axis the slider moves along")]
        [SerializeField]
        private SliderAxis sliderAxis = SliderAxis.XAxis;
        /// <summary>
        /// Property accessor of sliderAxis. The axis the slider moves along.
        /// </summary>
        public SliderAxis CurrentSliderAxis
        {
            get { return sliderAxis; }
            set
            {
                sliderAxis = value;
                UpdateVisualsOrientation();
            }
        }

        /// <summary>
        /// Previous value of slider axis, is used in order to detect change in current slider axis value
        /// </summary>
        private SliderAxis? previousSliderAxis = null;
        /// <summary>
        /// Property accessor for previousSliderAxis that is used also to initialize the property with the current value in case of null value.
        /// </summary>
        private SliderAxis PreviousSliderAxis
        {
            get
            {
                if (previousSliderAxis == null)
                {
                    previousSliderAxis = CurrentSliderAxis;
                }
                return previousSliderAxis.Value;
            }
            set
            {
                previousSliderAxis = value;
            }
        }

        [SerializeField]
        [Tooltip("Where the slider track starts, as distance from center along slider axis, in local space units.")]
        private float sliderStartDistance = -.5f;
        public float SliderStartDistance
        {
            get { return sliderStartDistance; }
            set { sliderStartDistance = value; }
        }

        [SerializeField]
        [Tooltip("Where the slider track ends, as distance from center along slider axis, in local space units.")]
        private float sliderEndDistance = .5f;
        public float SliderEndDistance
        {
            get { return sliderEndDistance; }
            set { sliderEndDistance = value; }
        }

        /// <summary>
        /// Gets the start position of the slider, in world space, or zero if invalid.
        /// Sets the start position of the slider, in world space, projected to the slider's axis.
        /// </summary>
        public Vector3 SliderStartPosition
        {
            get { return transform.TransformPoint(GetSliderAxis() * sliderStartDistance); }
            set { sliderStartDistance = Vector3.Dot(transform.InverseTransformPoint(value), GetSliderAxis()); }
        }

        /// <summary>
        /// Gets the end position of the slider, in world space, or zero if invalid.
        /// Sets the end position of the slider, in world space, projected to the slider's axis.
        /// </summary>
        public Vector3 SliderEndPosition
        {
            get { return transform.TransformPoint(GetSliderAxis() * sliderEndDistance); }
            set { sliderEndDistance = Vector3.Dot(transform.InverseTransformPoint(value), GetSliderAxis()); }
        }

        /// <summary>
        /// Returns the vector from the slider start to end positions
        /// </summary>
        public Vector3 SliderTrackDirection
        {
            get { return SliderEndPosition - SliderStartPosition; }
        }

        #endregion

        #region Event Handlers
        [Header("Events")]
        public CustomSliderEvent OnValueUpdated = new CustomSliderEvent();
        public CustomSliderEvent OnInteractionStarted = new CustomSliderEvent();
        public CustomSliderEvent OnInteractionEnded = new CustomSliderEvent();
        public CustomSliderEvent OnHoverEntered = new CustomSliderEvent();
        public CustomSliderEvent OnHoverExited = new CustomSliderEvent();
        #endregion

        #region Private Fields

        /// <summary>
        /// Position offset for slider handle in world space.
        /// </summary>
        private Vector3 sliderThumbOffsetMax = Vector3.zero;

        /// <summary>
        /// Position offset for slider handle in world space.
        /// </summary>
        private Vector3 sliderThumbOffsetMin = Vector3.zero;

        private bool moveMaxOrMin = false;


        /// <summary>
        /// Private member used to adjust slider values
        /// </summary>
        private float sliderStepVal => (maxVal - minVal) / sliderStepDivisions;

        #endregion

        #region Protected Properties

        /// <summary>
        /// Float value that holds the starting value of the slider.
        /// </summary>
        protected float StartSliderValueMax { get; private set; }
        protected float StartSliderValueMin { get; private set; }

        /// <summary>
        /// Starting position of mixed reality pointer in world space
        /// Used to track pointer movement
        /// </summary>
        protected Vector3 StartPointerPosition { get; private set; }

        /// <summary>
        /// Interface for handling pointer being used in UX interaction.
        /// </summary>
        protected IMixedRealityPointer ActivePointer { get; private set; }

        #endregion

        #region Constants
        /// <summary>
        /// Minimum distance between start and end of slider, in world space
        /// </summary>
        private const float MinSliderLength = 0.001f;

        /// <summary>
        /// The minimum value that the slider can take on
        /// </summary>
        private const float minVal = 0.0f;

        /// <summary>
        /// The maximum value that the slider can take on
        /// </summary>
        private const float maxVal = 1.0f;

        #endregion  

        #region Unity methods
        protected virtual void Start()
        {
            if (useSliderStepDivisions)
            {
                InitializeStepDivisions();
            }

            if (thumbRootMax == null || thumbRootMin == null)
            {
                throw new Exception($"Slider thumb on gameObject {gameObject.name} is not specified. Did you forget to set it?");
            }

            SnapToPosition = snapToPosition;
            InitializeSliderThumb();
            OnValueUpdated.Invoke(new CustomSliderEventData(sliderValueMax, sliderValueMax, null, this));
            OnValueUpdated.Invoke(new CustomSliderEventData(sliderValueMin, sliderValueMin, null, this));
        }

        private void OnDisable()
        {
            if (ActivePointer != null)
            {
                EndInteraction();
            }
        }

        private void OnValidate()
        {
            CurrentSliderAxis = sliderAxis;
        }

        #endregion

        #region Private Methods
        private void InitializeSliderThumb()
        {
            var startToThumbMax = thumbRootMax.transform.position - SliderStartPosition;
            var thumbProjectedOnTrackMax = SliderStartPosition + Vector3.Project(startToThumbMax, SliderTrackDirection);
            sliderThumbOffsetMax = thumbRootMax.transform.position - thumbProjectedOnTrackMax;

            var startToThumbMin = thumbRootMin.transform.position - SliderStartPosition;
            var thumbProjectedOnTrackMin = SliderStartPosition + Vector3.Project(startToThumbMin, SliderTrackDirection);
            sliderThumbOffsetMin = thumbRootMin.transform.position - thumbProjectedOnTrackMin;

            UpdateUI();
        }

        /// <summary> 
        /// Private method used to adjust initial slider value to stepwise values
        /// </summary>
        private void InitializeStepDivisions()
        {
            SliderValueMax = SnapSliderToStepPositions(SliderValueMax);
            SliderValueMin = SnapSliderToStepPositions(SliderValueMin);
        }

        /// <summary>
        /// Update orientation of track visuals based on slider axis orientation
        /// </summary>
        private void UpdateTrackVisuals()
        {
            if (TrackVisuals)
            {
                TrackVisuals.transform.localPosition = Vector3.zero;

                switch (sliderAxis)
                {
                    case SliderAxis.XAxis:
                        TrackVisuals.transform.localRotation = Quaternion.identity;
                        break;
                    case SliderAxis.YAxis:
                        TrackVisuals.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 90.0f);
                        break;
                    case SliderAxis.ZAxis:
                        TrackVisuals.transform.localRotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);
                        break;
                }
            }
        }

        /// <summary>
        /// Update orientation of tick marks based on slider axis orientation
        /// </summary>
        private void UpdateTickMarks()
        {
            if (TickMarks)
            {
                TickMarks.transform.localPosition = Vector3.zero;
                TickMarks.transform.localRotation = Quaternion.identity;

                var grid = TickMarks.GetComponent<Utilities.GridObjectCollection>();
                if (grid)
                {
                    // Update cellwidth or cellheight depending on what was the previous axis set to
                    var previousAxis = grid.Layout;
                    if (previousAxis == Utilities.LayoutOrder.Vertical)
                    {
                        grid.CellWidth = grid.CellHeight;
                    }
                    else
                    {
                        grid.CellHeight = grid.CellWidth;
                    }

                    grid.Layout = (sliderAxis == SliderAxis.YAxis) ? Utilities.LayoutOrder.Vertical : Utilities.LayoutOrder.Horizontal;
                    grid.UpdateCollection();
                }

                if (sliderAxis == SliderAxis.ZAxis)
                {
                    TickMarks.transform.localRotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);
                }
            }
        }

        /// <summary>
        /// Update orientation of thumb visuals based on slider axis orientation
        /// </summary>
        private void UpdateThumbVisuals()
        {
            if (ThumbVisuals)
            {
                ThumbVisuals.transform.localPosition = Vector3.zero;

                switch (sliderAxis)
                {
                    case SliderAxis.XAxis:
                        ThumbVisuals.transform.localRotation = Quaternion.identity;
                        break;
                    case SliderAxis.YAxis:
                        ThumbVisuals.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 90.0f);
                        break;
                    case SliderAxis.ZAxis:
                        ThumbVisuals.transform.localRotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);
                        break;
                }
            }
        }

        /// <summary>
        /// Update orientation of the visual components of pinch slider
        /// </summary>
        private void UpdateVisualsOrientation()
        {
            if (PreviousSliderAxis != sliderAxis)
            {
                UpdateThumbVisuals();
                UpdateTrackVisuals();
                UpdateTickMarks();
                PreviousSliderAxis = sliderAxis;
            }
        }

        private Vector3 GetSliderAxis()
        {
            switch (sliderAxis)
            {
                case SliderAxis.XAxis:
                    return Vector3.right;
                case SliderAxis.YAxis:
                    return Vector3.up;
                case SliderAxis.ZAxis:
                    return Vector3.forward;
                default:
                    throw new ArgumentOutOfRangeException("Invalid slider axis");
            }
        }

        private void UpdateUI()
        {
            var newSliderPosMax = SliderStartPosition + sliderThumbOffsetMax + SliderTrackDirection * sliderValueMax;
            thumbRootMax.transform.position = newSliderPosMax;

            var newSliderPosMin = SliderStartPosition + sliderThumbOffsetMin + SliderTrackDirection * sliderValueMin;
            thumbRootMin.transform.position = newSliderPosMin;

            if (maintainLine.transform.parent.name.Equals("CycleTimeSpecific"))
            {
                maintainLine.sampleNumberChange();
            }
            else
            {
                maintainLine.shiftNumberChange();
            }
        }

        private void EndInteraction()
        {
            if (OnInteractionEnded != null)
            {
                OnInteractionEnded.Invoke(new CustomSliderEventData(sliderValueMax, sliderValueMax, ActivePointer, this));
                OnInteractionEnded.Invoke(new CustomSliderEventData(sliderValueMin, sliderValueMin, ActivePointer, this));
            }
            ActivePointer = null;
        }


        private float SnapSliderToStepPositions(float value)
        {
            var stepCount = value / sliderStepVal;
            var snappedValue = sliderStepVal * Mathf.RoundToInt(stepCount);
            Mathf.Clamp(snappedValue, minVal, maxVal);
            return snappedValue;
        }

        private void CalculateSliderValueBasedOnTouchPoint(Vector3 touchPoint)
        {
            var sliderTouchPoint = touchPoint - SliderStartPosition;
            var sliderVector = SliderEndPosition - SliderStartPosition;

            // If our touch point goes off the start side of the slider, set it's value to minVal and return immediately
            // Explanation of the math here: https://www.quora.com/Can-scalar-projection-be-negative
            if (Vector3.Dot(sliderTouchPoint, sliderVector) < 0)
            {
                SliderValueMax = minVal;
                SliderValueMin = minVal;
                return;
            }

            float sliderProgress = Vector3.Project(sliderTouchPoint, sliderVector).magnitude;
            float result = sliderProgress / sliderVector.magnitude;
            float clampedResult = result;
            if (UseSliderStepDivisions)
            {
                clampedResult = SnapSliderToStepPositions(result);
            }
            clampedResult = Mathf.Clamp(clampedResult, minVal, maxVal);

            SliderValueMax = clampedResult;
            SliderValueMin = clampedResult;
        }


        #endregion

        #region IMixedRealityFocusHandler
        public void OnFocusEnter(FocusEventData eventData)
        {
            OnHoverEntered.Invoke(new CustomSliderEventData(sliderValueMax, sliderValueMax, eventData.Pointer, this));
            OnHoverEntered.Invoke(new CustomSliderEventData(sliderValueMin, sliderValueMin, eventData.Pointer, this));
        }

        public void OnFocusExit(FocusEventData eventData)
        {
            OnHoverExited.Invoke(new CustomSliderEventData(sliderValueMax, sliderValueMax, eventData.Pointer, this));
            OnHoverExited.Invoke(new CustomSliderEventData(sliderValueMin, sliderValueMin, eventData.Pointer, this));
        }
        #endregion

        #region IMixedRealityPointerHandler

        public void OnPointerUp(MixedRealityPointerEventData eventData)
        {
            if (eventData.Pointer == ActivePointer && !eventData.used)
            {
                EndInteraction();
                if(maintainLine.transform.parent.name.Equals("CycleTimeSpecific"))
                {
                    maintainLine.changeNumberOfPointsTime_v2();
                }
                else
                {
                    maintainLine.changeNumberOfPointsKPI_v2();
                }
                if (moveMaxOrMin)
                {
                    Material[] materials = thumbRootMax.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().materials;
                    materials[0] = default_material;
                    thumbRootMax.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().materials = materials;
                }
                else
                {
                    Material[] materials = thumbRootMin.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().materials;
                    materials[0] = default_material;
                    thumbRootMin.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().materials = materials;
                }
                

                // Mark the pointer data as used to prevent other behaviors from handling input events
                eventData.Use();

            }
        }

        public void OnPointerDown(MixedRealityPointerEventData eventData)
        {
            if (ActivePointer == null && !eventData.used)
            {

                ActivePointer = eventData.Pointer;
                StartPointerPosition = ActivePointer.Position;

                if (SnapToPosition)
                {
                    CalculateSliderValueBasedOnTouchPoint(ActivePointer.Result.Details.Point);
                }

                if (OnInteractionStarted != null)
                {
                    float max = staticFunctions.calculateDistance(ActivePointer.Result.Details.Point, thumbRootMax.transform.position);
                    float min = staticFunctions.calculateDistance(ActivePointer.Result.Details.Point, thumbRootMin.transform.position);
                    moveMaxOrMin = max < min;
                    OnInteractionStarted.Invoke(new CustomSliderEventData(sliderValueMax, sliderValueMax, ActivePointer, this));
                    OnInteractionStarted.Invoke(new CustomSliderEventData(sliderValueMin, sliderValueMin, ActivePointer, this));


                    if (moveMaxOrMin)
                    {
                        Material[] materials = thumbRootMax.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().materials;
                        materials[0] = dragging_material;
                        thumbRootMax.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().materials = materials;
                    }
                    else
                    {
                        Material[] materials = thumbRootMin.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().materials;
                        materials[0] = dragging_material;
                        thumbRootMin.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().materials = materials;
                    }

                }
                StartSliderValueMax = sliderValueMax;
                StartSliderValueMin = sliderValueMin;

                // Mark the pointer data as used to prevent other behaviors from handling input events
                eventData.Use();
            }
        }

        public virtual void OnPointerDragged(MixedRealityPointerEventData eventData)
        {
            if (eventData.Pointer == ActivePointer && !eventData.used)
            {
                /*
                Debug.Log("Pointer: " + ActivePointer.Position);
                Debug.Log("Max: " + thumbRootMax.transform.position);
                Debug.Log("Min: " + thumbRootMin.transform.position);
                */
                var delta = ActivePointer.Position - StartPointerPosition;
                var handDelta = Vector3.Dot(SliderTrackDirection.normalized, delta);
                /*
                float max = staticFunctions.calculateDistance(handDelta, thumbRootMax.transform.position);
                float min = staticFunctions.calculateDistance(handDelta, thumbRootMin.transform.position);
                if (max < min) Debug.Log("Max: " + handDelta + " ## " + thumbRootMax.transform.position + " ## " + thumbRootMin.transform.position);
                else Debug.Log("Min: " + handDelta + " ## " + thumbRootMax.transform.position + " ## " + thumbRootMin.transform.position);
                */
                if (useSliderStepDivisions)
                {
                    var stepVal = (handDelta / SliderTrackDirection.magnitude > 0) ? sliderStepVal : (sliderStepVal * -1);
                    var stepMag = Mathf.Floor(Mathf.Abs(handDelta / SliderTrackDirection.magnitude) / sliderStepVal);
                    if (moveMaxOrMin)
                    {
                        SliderValueMax = Mathf.Clamp(StartSliderValueMax + (stepVal * stepMag), 0, 1);
                        if (SliderValueMax <= SliderValueMin + maxVal / sliderStepDivisions)
                        {
                            SliderValueMax = SliderValueMin + maxVal / sliderStepDivisions;
                        }
                    }
                    else
                    {
                        SliderValueMin = Mathf.Clamp(StartSliderValueMin + (stepVal * stepMag), 0, 1);
                        if (SliderValueMin >= SliderValueMax - maxVal / sliderStepDivisions)
                        {
                            SliderValueMin = SliderValueMax - maxVal / sliderStepDivisions;
                        }
                    }
                }
                else
                {
                    SliderValueMax = Mathf.Clamp(StartSliderValueMax + handDelta / SliderTrackDirection.magnitude, 0, 1);
                    SliderValueMin = Mathf.Clamp(StartSliderValueMin + handDelta / SliderTrackDirection.magnitude, 0, 1);
                }

                // Mark the pointer data as used to prevent other behaviors from handling input events
                eventData.Use();
            }
        }

        public void OnPointerClicked(MixedRealityPointerEventData eventData) { }

        #endregion


        #region IMixedRealityTouchHandler
        public void OnTouchStarted(HandTrackingInputEventData eventData)
        {
            if (isTouchable)
            {
                if (OnInteractionStarted != null)
                {
                    OnInteractionStarted.Invoke(new CustomSliderEventData(sliderValueMax, sliderValueMax, ActivePointer, this));
                    OnInteractionStarted.Invoke(new CustomSliderEventData(sliderValueMin, sliderValueMin, ActivePointer, this));
                }
                eventData.Use();
            }
        }


        public void OnTouchCompleted(HandTrackingInputEventData eventData)
        {
            if (isTouchable)
            {
                if (!eventData.used)
                {
                    EndInteraction();

                    // Mark the pointer data as used to prevent other behaviors from handling input events
                    eventData.Use();
                }
            }
        }

        /// <summary>b  
        /// When the collider is touched, use the touch point to Calculate the Slider value
        /// </summary>
        public void OnTouchUpdated(HandTrackingInputEventData eventData)
        {
            if (isTouchable)
            {
                CalculateSliderValueBasedOnTouchPoint(eventData.InputData);
            }
        }

        #endregion IMixedRealityTouchHandler
    }
}
