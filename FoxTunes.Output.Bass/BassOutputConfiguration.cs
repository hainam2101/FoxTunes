﻿using System.Collections.Generic;

namespace FoxTunes
{
    public static class BassOutputConfiguration
    {
        public const string SECTION = "8399D051-81BC-41A6-940D-36E6764213D2";

        public const string RATE_ELEMENT = "AAAAA558-F1ED-41B1-A3DC-95158E01003C";

        public const string ENFORCE_RATE_ELEMENT = "JJJJ5B16-1B49-4C50-A8CF-BE3A6CCD4A87";

        public const string DEPTH_ELEMENT = "KKKKA2A6-DCA0-4E27-9812-498BB2A2C4BC";

        public const string DEPTH_16_OPTION = "LLLL8466-7582-4B1C-8687-7AB75D636CD8";

        public const string DEPTH_32_OPTION = "MMMM73F2-2E08-4F2B-B94E-DAA945D96497";

        public const string MODE_ELEMENT = "NNNN6B39-2F8A-4667-9C03-9742FF2D1EA7";

        public const string PLAY_FROM_RAM_ELEMENT = "OOOOBED1-7965-47A3-9798-E46124386A8D";

        public const string BUFFER_LENGTH_ELEMENT = "PPPP3629-1AE5-451F-A545-8B864FEAD038";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, "Output")
                .WithElement(new SelectionConfigurationElement(RATE_ELEMENT, "Rate", path: "Advanced").WithOptions(GetRateOptions()))
                .WithElement(new SelectionConfigurationElement(DEPTH_ELEMENT, "Depth", path: "Advanced").WithOptions(GetDepthOptions()))
                .WithElement(new SelectionConfigurationElement(MODE_ELEMENT, "Mode"))
                .WithElement(new BooleanConfigurationElement(ENFORCE_RATE_ELEMENT, "Enforce Rate", path: "Advanced").WithValue(false))
                .WithElement(new BooleanConfigurationElement(PLAY_FROM_RAM_ELEMENT, "Play From Memory", path: "Advanced").WithValue(false))
                .WithElement(new IntegerConfigurationElement(BUFFER_LENGTH_ELEMENT, "Buffer Length", path: "Advanced").WithValue(500).WithValidationRule(new IntegerValidationRule(10, 5000))
            );
        }

        public static IEnumerable<SelectionConfigurationOption> GetRateOptions()
        {
            foreach (var rate in OutputRate.PCM)
            {
                yield return new SelectionConfigurationOption(rate.ToString(), rate.ToString());
            }
        }

        public static int GetRate(SelectionConfigurationOption option)
        {
            var rate = default(int);
            if (int.TryParse(option.Id, out rate))
            {
                return rate;
            }
            return OutputRate.PCM_44100;
        }

        public static IEnumerable<SelectionConfigurationOption> GetDepthOptions()
        {
            yield return new SelectionConfigurationOption(DEPTH_16_OPTION, "16bit");
            yield return new SelectionConfigurationOption(DEPTH_32_OPTION, "32bit floating point");
        }

        public static bool GetFloat(SelectionConfigurationOption option)
        {
            switch (option.Id)
            {
                default:
                case DEPTH_16_OPTION:
                    return false;
                case DEPTH_32_OPTION:
                    return true;
            }
        }
    }
}
