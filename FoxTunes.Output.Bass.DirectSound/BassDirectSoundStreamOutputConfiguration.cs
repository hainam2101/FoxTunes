﻿using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class BassDirectSoundStreamOutputConfiguration
    {
        public const string MODE_DS_OPTION = "AAAA1348-069B-4763-89CF-5ACBE53E9F75";

        public const string ELEMENT_DS_DEVICE = "BBBBD4A5-4DD5-4985-A373-565335084B80";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(BassOutputConfiguration.SECTION, "Output")
                .WithElement(new SelectionConfigurationElement(BassOutputConfiguration.MODE_ELEMENT, "Mode")
                    .WithOptions(new[] { new SelectionConfigurationOption(MODE_DS_OPTION, "Direct Sound").Default() }))
                .WithElement(new SelectionConfigurationElement(ELEMENT_DS_DEVICE, "Device", path: "Direct Sound")
                    .WithOptions(GetDSDevices()));
        }

        public static int GetDsDevice(SelectionConfigurationOption option)
        {
            if (!string.Equals(option.Id, Bass.DefaultDevice.ToString()))
            {
                for (int a = 0, b = Bass.DeviceCount; a < b; a++)
                {
                    var deviceInfo = default(DeviceInfo);
                    BassUtils.OK(Bass.GetDeviceInfo(a, out deviceInfo));
                    if (string.Equals(deviceInfo.Name, option.Id, StringComparison.OrdinalIgnoreCase))
                    {
                        return a;
                    }
                }
            }
            return Bass.DefaultDevice;
        }

        private static IEnumerable<SelectionConfigurationOption> GetDSDevices()
        {
            yield return new SelectionConfigurationOption(Bass.DefaultDevice.ToString(), "Default Device");
            for (int a = 0, b = Bass.DeviceCount; a < b; a++)
            {
                var deviceInfo = default(DeviceInfo);
                BassUtils.OK(Bass.GetDeviceInfo(a, out deviceInfo));
                LogManager.Logger.Write(typeof(BassDirectSoundStreamOutputConfiguration), LogLevel.Debug, "DS Device: {0} => {1} => {2}", a, deviceInfo.Name, deviceInfo.Driver);
                if (!deviceInfo.IsEnabled)
                {
                    continue;
                }
                yield return new SelectionConfigurationOption(deviceInfo.Name, deviceInfo.Name, deviceInfo.Driver);
            }
        }
    }
}
