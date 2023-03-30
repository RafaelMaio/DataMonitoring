#region Assembly Microsoft.MixedReality.Toolkit.SDK, Version=2.8.2.0, Culture=neutral, PublicKeyToken=null
// C:\Users\RafaelMaio\Desktop\Bolsa\Line_monitoring\UnityApps\LineMonitoringTestFeatures\Library\ScriptAssemblies\Microsoft.MixedReality.Toolkit.SDK.dll
#endregion

using Microsoft.MixedReality.Toolkit.Input;

namespace Microsoft.MixedReality.Toolkit.UI
{
    public class CustomSliderEventData
    {
        public CustomSliderEventData(float o, float n, IMixedRealityPointer pointer, CustomPinchSlider slider) {
            OldValue = o;
            NewValue = n;
            Slider = slider;
            Pointer = pointer;
        }

        public float OldValue { get; }
        public float NewValue { get; }
        public CustomPinchSlider Slider { get; }
        public IMixedRealityPointer Pointer { get; set; }
    }
}