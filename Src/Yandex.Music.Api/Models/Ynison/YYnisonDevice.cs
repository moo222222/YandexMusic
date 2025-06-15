namespace Yandex.Music.Api.Models.Ynison
{
    public class YYnisonDevice
    {
        public YYnisonDeviceInfo Info { get; set; }
        public YYnisonDeviceCapabilities Capabilities { get; set; } = new YYnisonDeviceCapabilities();
        public YYnisonDeviceVolumeInfo VolumeInfo { get; set; } = new YYnisonDeviceVolumeInfo();
        public bool IsShadow { get; set; }
    }
}