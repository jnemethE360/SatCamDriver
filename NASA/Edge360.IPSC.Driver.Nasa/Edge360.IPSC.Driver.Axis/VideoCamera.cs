using System;
using System.ServiceModel;
using CNL.IPSecurityCenter.Driver;
using CNL.IPSecurityCenter.Driver.Ptz;

namespace Edge360.IPSC.Driver.Axis
{
    /// <summary>
    /// Represents the VideoCamera class, implementing the IVideoCamera contract.
    /// </summary>
    [Serializable]
    [ServiceBehaviorAttribute(InstanceContextMode = InstanceContextMode.Single)]
    public class VideoCamera : VideoCameraBase, IVideoCamera
    {
        // -- constructors

        public VideoCamera()
            : base()
        {
            presets = new PresetCollection();
        }

        // -- fields

        private PresetCollection presets;

        // -- public methods

        /// <summary>
        /// Gets the presets for the device.
        /// </summary>
        /// <returns></returns>
        public override PresetCollection GetPresets()
        {
            return presets;
        }

        /// <summary>
        /// Selects the specified preset position.
        /// </summary>
        /// <param name="number">The preset number to select.</param>
        public override void SelectPreset(int number)
        {
            Preset preset;

            if (PresetsSupported)
            {
                if (presets.TryGetValue(number, out preset))
                {
                    OnPresetSelected(new PresetSelectedEventArgs(preset, this.Identifier));
                }
            }
        }

        /// <summary>
        /// Updates or adds the specified preset.
        /// </summary>
        /// <param name="preset">The preset to update or add.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="preset"/> is <c>null</c>.</exception>
        public override void Update(Preset preset)
        {
            if (preset == null)
                throw new ArgumentNullException("preset");

            presets.Update(preset);
        }

        /// <summary>
        /// Removes the specified preset.
        /// </summary>
        /// <param name="preset">The preset to remove.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="preset"/> is <c>null</c>.</exception>
        public override void Remove(Preset preset)
        {
            if (preset == null)
                throw new ArgumentNullException("preset");

            presets.Remove(preset);
        }
    }
}
