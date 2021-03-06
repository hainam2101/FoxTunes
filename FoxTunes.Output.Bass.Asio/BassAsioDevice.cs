﻿using FoxTunes.Interfaces;
using ManagedBass.Asio;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    public static class BassAsioDevice
    {
        public const int PRIMARY_CHANNEL = 0;

        public const int SECONDARY_CHANNEL = 1;

        static BassAsioDevice()
        {
            Devices = new Dictionary<int, BassAsioDeviceInfo>();
        }

        private static IDictionary<int, BassAsioDeviceInfo> Devices { get; set; }

        public static int Device { get; private set; }

        public static bool IsInitialized { get; private set; }

        public static void Init()
        {
            Init(Device);
        }

        public static void Init(int device)
        {
            if (device == BassAsioStreamOutputConfiguration.ASIO_NO_DEVICE)
            {
                throw new InvalidOperationException("A valid device must be provided.");
            }
            LogManager.Logger.Write(typeof(BassAsioDevice), LogLevel.Debug, "Initializing BASS ASIO.");
            BassAsioUtils.OK(BassAsio.Init(device, AsioInitFlags.Thread));
            BassAsioUtils.OK(BassAsioHandler.Init());
            IsInitialized = true;
            var info = default(AsioChannelInfo);
            BassAsioUtils.OK(BassAsio.ChannelGetInfo(false, PRIMARY_CHANNEL, out info));
            Device = device;
            Devices[device] = new BassAsioDeviceInfo(
                info.Name,
                Convert.ToInt32(BassAsio.Rate),
                BassAsio.Info.Inputs,
                BassAsio.Info.Outputs,
                GetSupportedRates(),
                info.Format
            );
            LogManager.Logger.Write(typeof(BassAsioDevice), LogLevel.Debug, "Detected ASIO device: {0} => Name => {1}, Inputs => {2}, Outputs = {3}, Rate = {4}, Format = {5}", Device, Info.Name, Info.Inputs, Info.Outputs, Info.Rate, Enum.GetName(typeof(AsioSampleFormat), Info.Format));
            LogManager.Logger.Write(typeof(BassAsioDevice), LogLevel.Debug, "Detected ASIO device: {0} => Rates => {1}", Device, string.Join(", ", Info.SupportedRates));
        }

        private static IEnumerable<int> GetSupportedRates()
        {
            //TODO: I'm not sure if we should be setting DSD mode before querying DSD rates.
            return Enumerable.Concat(
                OutputRate.PCM,
                OutputRate.DSD
            ).Where(rate => BassAsio.CheckRate(rate)).ToArray();
        }

        public static void Free()
        {
            if (!IsInitialized)
            {
                return;
            }
            LogManager.Logger.Write(typeof(BassAsioDevice), LogLevel.Debug, "Releasing BASS ASIO.");
            BassAsio.Free();
            BassAsioHandler.Free();
            IsInitialized = false;
        }

        public static BassAsioDeviceInfo Info
        {
            get
            {
                if (!Devices.ContainsKey(Device))
                {
                    return null;
                }
                return Devices[Device];
            }
        }

        public class BassAsioDeviceInfo
        {
            public BassAsioDeviceInfo(string name, int rate, int inputs, int outputs, IEnumerable<int> supportedRates, AsioSampleFormat format)
            {
                this.Name = name;
                this.Rate = rate;
                this.Inputs = inputs;
                this.Outputs = outputs;
                this.SupportedRates = supportedRates;
                this.Format = format;
            }

            public string Name { get; private set; }

            public int Rate { get; private set; }

            public int Inputs { get; private set; }

            public int Outputs { get; private set; }

            public IEnumerable<int> SupportedRates { get; private set; }

            public AsioSampleFormat Format { get; private set; }

            public bool ControlPanel()
            {
                return BassAsio.ControlPanel();
            }
        }
    }
}
