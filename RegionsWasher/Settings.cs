using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Settings;

namespace RegionsWasher
{
    internal enum BehaviorMode
    {
        DoNothing,
        ExpandAll,
        CollapseAll,
        PreventCollapse
    }

    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("2C449B98-B5DB-42D4-BAAC-B4A3363EBC54")]
    internal class Settings : UIElementDialogPage, INotifyPropertyChanged
    {
        private const string CollectionPath = "RegionsWasher";
        private Color? background;
        private BehaviorMode behaviorMode;
        private Color? foreground;
        private bool isBold;
        private bool isItalic;
        private bool isLoaded;
        private double? size;

        public Settings()
        {
            BehaviorMode = BehaviorMode.DoNothing;
            Foreground = Colors.LightGray;
            Background = null;
            Size = 8;
            IsItalic = true;

            BehaviorModes = new[]
            {
                new {Mode = BehaviorMode.DoNothing, Name = "Do Nothing"},
                new {Mode = BehaviorMode.CollapseAll, Name = "Collapse All"},
                new {Mode = BehaviorMode.ExpandAll, Name = "Expand All"},
                new {Mode = BehaviorMode.PreventCollapse, Name = "Prevent Collapse"}
            };

            var defaultBrush = new ImageBrush
            {
                ImageSource = new DrawingImage {Drawing = new GeometryDrawing {Brush = Brushes.Gray, Geometry = Geometry.Parse("M 0,0 L 50,0 50,50 0,50 Z M 50,50 L 100,50 100,100 50,100 Z")}},
                Stretch = Stretch.Fill
            };

            var insertSpaceBeforeCapital = new Regex(@"(.*?\w)([A-Z])");
            DefinedColors = new dynamic[] {new {Name = "Default", Color = (Color?) null, Brush = defaultBrush}}
                .Concat(typeof(Colors).GetProperties(BindingFlags.Public | BindingFlags.Static)
                    .Select(p => new {Name = insertSpaceBeforeCapital.Replace(p.Name, "$1 $2"), Color = p.GetValue(null), Brush = new SolidColorBrush((Color) p.GetValue(null))})
                    .ToArray());
        }

        protected override UIElement Child => new SettingsPageControl(this);

        [SettingsProperty]
        public BehaviorMode BehaviorMode
        {
            get => behaviorMode;
            set
            {
                behaviorMode = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<dynamic> BehaviorModes { get; }

        [SettingsProperty]
        public Color? Foreground
        {
            get => foreground;
            set
            {
                foreground = value;
                OnPropertyChanged();
            }
        }

        [SettingsProperty]
        public Color? Background
        {
            get => background;
            set
            {
                background = value;
                OnPropertyChanged();
            }
        }

        [SettingsProperty]
        public double? Size
        {
            get => size;
            set
            {
                if (value < 1.0 || value > 99.0)
                {
                    throw new ArgumentException("the size must be in range: 1..99 or empty");
                }

                size = value;
                OnPropertyChanged();
            }
        }

        [SettingsProperty]
        public bool IsItalic
        {
            get => isItalic;
            set
            {
                isItalic = value;
                OnPropertyChanged();
            }
        }

        [SettingsProperty]
        public bool IsBold
        {
            get => isBold;
            set
            {
                isBold = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<dynamic> DefinedColors { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected override void OnActivate(CancelEventArgs args)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            base.OnActivate(args);

            LoadFromStore();
        }

        protected override void OnClosed(EventArgs args)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            base.OnClosed(args);

            SaveToStore();
        }

        public void LoadFromStore()
        {
            if (isLoaded)
            {
                return;
            }

            ThreadHelper.ThrowIfNotOnUIThread();

            if (!GetSettingsStore(out var settingsStore) || !settingsStore.CollectionExists(CollectionPath))
            {
                return;
            }

            var properties = typeof(Settings).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.GetCustomAttribute<SettingsPropertyAttribute>() != null);
            foreach (var property in properties)
            {
                try
                {
                    var storeValue = settingsStore.GetString(CollectionPath, property.Name);
                    if (string.IsNullOrWhiteSpace(storeValue))
                    {
                        property.SetValue(this, null);
                        continue;
                    }

                    object propertyValue = null;
                    if (property.PropertyType == typeof(double?))
                    {
                        propertyValue = XmlConvert.ToDouble(storeValue);
                    }
                    else if (property.PropertyType == typeof(bool))
                    {
                        propertyValue = XmlConvert.ToBoolean(storeValue);
                    }
                    else if (property.PropertyType == typeof(Color?))
                    {
                        propertyValue = ColorConverter.ConvertFromString(storeValue);
                    }
                    else if (property.PropertyType == typeof(BehaviorMode))
                    {
                        propertyValue = (BehaviorMode) Enum.Parse(typeof(BehaviorMode), storeValue);
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException($"unknown type: '{property.PropertyType}'");
                    }

                    property.SetValue(this, propertyValue);
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception);
                }
            }

            isLoaded = true;
        }

        public void SaveToStore()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!GetSettingsStore(out var settingsStore))
            {
                return;
            }

            var properties = typeof(Settings).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.GetCustomAttribute<SettingsPropertyAttribute>() != null);
            foreach (var property in properties)
            {
                var propertyValue = property.GetValue(this);
                if (propertyValue == null)
                {
                    settingsStore.SetString(CollectionPath, property.Name, string.Empty);
                    continue;
                }

                string storeValue = null;
                if (property.PropertyType == typeof(double?))
                {
                    storeValue = XmlConvert.ToString((double) propertyValue);
                }
                else if (property.PropertyType == typeof(bool))
                {
                    storeValue = XmlConvert.ToString((bool) propertyValue);
                }
                else if (property.PropertyType == typeof(Color?))
                {
                    storeValue = ((Color) propertyValue).ToString();
                }
                else if (property.PropertyType == typeof(BehaviorMode))
                {
                    storeValue = ((BehaviorMode) propertyValue).ToString();
                }
                else
                {
                    throw new ArgumentOutOfRangeException($"unknown type: '{property.PropertyType}'");
                }

                settingsStore.SetString(CollectionPath, property.Name, storeValue);
            }

            TinyMessageBroker.Instance.Publish(this);
        }

        private bool GetSettingsStore(out WritableSettingsStore settingsStore)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            settingsStore = null;

            var vsSettingsManager = ServiceProvider.GlobalProvider.GetService(typeof(SVsSettingsManager)) as IVsSettingsManager;
            if (vsSettingsManager == null)
            {
                return false;
            }

            var serviceManager = new ShellSettingsManager(vsSettingsManager);
            settingsStore = serviceManager?.GetWritableSettingsStore(SettingsScope.UserSettings);

            if (!settingsStore.CollectionExists(CollectionPath))
            {
                settingsStore.CreateCollection(CollectionPath);
            }

            return true;
        }

        public override void LoadSettingsFromStorage()
        {
        }

        public override void SaveSettingsToStorage()
        {
        }
    }
}
