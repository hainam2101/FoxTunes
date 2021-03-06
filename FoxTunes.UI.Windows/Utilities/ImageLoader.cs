﻿using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class ImageLoader : StandardComponent, IConfigurableComponent
    {
        public ImageResizer ImageResizer { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public bool HighQualityResizer { get; private set; }

        public CappedDictionary<string, Lazy<ImageSource>> Store { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.ImageResizer = ComponentRegistry.Instance.GetComponent<ImageResizer>();
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<BooleanConfigurationElement>(
                ImageLoaderConfiguration.SECTION,
                ImageLoaderConfiguration.HIGH_QUALITY_RESIZER
            ).ConnectValue(value => this.HighQualityResizer = value);
            this.Configuration.GetElement<IntegerConfigurationElement>(
                ImageLoaderConfiguration.SECTION,
                ImageLoaderConfiguration.CACHE_SIZE
            ).ConnectValue(value => this.Store = new CappedDictionary<string, Lazy<ImageSource>>(value));
            base.InitializeComponent(core);
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return ImageLoaderConfiguration.GetConfigurationSections();
        }

        public ImageSource Load(string id, string fileName, int width, int height, bool cache)
        {
            if (cache)
            {
                var imageSource = default(Lazy<ImageSource>);
                if (Store.TryGetValue(id, out imageSource))
                {
                    return imageSource.Value;
                }
                Store.Add(id, new Lazy<ImageSource>(() => this.LoadCore(id, fileName, width, height)));
                //Second iteration will always hit cache.
                return this.Load(id, fileName, width, height, cache);
            }
            return this.LoadCore(id, fileName, width, height);
        }

        private ImageSource LoadCore(string id, string fileName, int width, int height)
        {
            try
            {
                var decode = false;
                if (width != 0 && height != 0 && this.HighQualityResizer)
                {
                    fileName = ImageResizer.Resize(id, fileName, width, height);
                }
                else
                {
                    decode = true;
                }
                var source = new BitmapImage();
                source.BeginInit();
                source.CacheOption = BitmapCacheOption.OnLoad;
                source.UriSource = new Uri(fileName);
                if (decode)
                {
                    if (width != 0)
                    {
                        source.DecodePixelWidth = width;
                    }
                    else if (height != 0)
                    {
                        source.DecodePixelHeight = height;
                    }
                }
                source.EndInit();
                source.Freeze();
                return source;
            }
            catch (Exception e)
            {
                Logger.Write(typeof(ImageLoader), LogLevel.Warn, "Failed to load image: {0}", e.Message);
                return null;
            }
        }

        public ImageSource Load(string id, Func<Stream> factory, bool cache)
        {
            if (cache)
            {
                var imageSource = default(Lazy<ImageSource>);
                if (Store.TryGetValue(id, out imageSource))
                {
                    return imageSource.Value;
                }
                Store.Add(id, new Lazy<ImageSource>(() => this.LoadCore(id, factory)));
                //Second iteration will always hit cache.
                return this.Load(id, factory, cache);
            }
            return this.LoadCore(id, factory);
        }

        public ImageSource LoadCore(string id, Func<Stream> factory)
        {
            try
            {
                var source = new BitmapImage();
                source.BeginInit();
                source.CacheOption = BitmapCacheOption.OnLoad;
                using (var stream = factory())
                {
                    source.StreamSource = stream;
                    source.EndInit();
                    source.Freeze();
                }
                return source;
            }
            catch (Exception e)
            {
                Logger.Write(typeof(ImageLoader), LogLevel.Warn, "Failed to load image: {0}", e.Message);
                return null;
            }
        }
    }
}
