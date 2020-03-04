using System.Collections;
using System.Collections.Generic;
using FerramAerospaceResearch.Reflection;
using FerramAerospaceResearch.Threading;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnassignedField.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable CollectionNeverUpdated.Global

namespace FerramAerospaceResearch.Settings
{
    [ConfigNode("BodyAtmosphericData")]
    public struct BodySettings
    {
        [ConfigValue("index")]
        public int Index;

        [ConfigValue("name")]
        public string Name;

        [ConfigValue("viscosityAtReferenceTemp")]
        public double ReferenceViscosity;

        [ConfigValue("referenceTemp")]
        public double ReferenceTemperature;
    }

    [ConfigNode("FARAeroData", true)]
    public class FARAeroData : Singleton<FARAeroData>, FerramAerospaceResearch.Interfaces.IConfigNode
    {
        [ConfigValue("massPerWingAreaSupported")]
        public static double MassPerWingAreaSupported = 0.04f;

        [ConfigValue("massStressPower")]
        public static double MassStressPower = 0.85f;

        [ConfigValue("ctrlSurfTimeConstant")]
        public static double ControlSurfaceTimeConstant = 0.25f;

        [ConfigValue("ctrlSurfTimeConstantFlap")]
        public static double ControlSurfaceTimeConstantFlap = 10;

        [ConfigValue("ctrlSurfTimeConstantSpoiler")]
        public static double ControlSurfaceTimeConstantSpoiler = 0.75f;

        [ConfigValue]
        private static readonly List<BodySettings> AtmosphericData = new List<BodySettings>();

        [ConfigValueIgnore]
        public static readonly Dictionary<int, BodySettings> AtmosphericConfiguration =
            new Dictionary<int, BodySettings>();

        private MainThread.CoroutineTask OnLoad;

        /// <inheritdoc />
        public void BeforeLoaded()
        {
            AtmosphericConfiguration.Clear();
            AtmosphericData.Clear();
        }

        /// <inheritdoc />
        public void AfterLoaded()
        {
            OnLoad?.Cancel();
            OnLoad = MainThread.StartCoroutine(LoadGlobals);
        }

        private static IEnumerator LoadGlobals()
        {
            yield return new WaitWhile(() => HighLogic.LoadedScene == GameScenes.LOADING || FlightGlobals.fetch == null);

            foreach (BodySettings settings in AtmosphericData)
            {
                int index = settings.Index;
                if (!string.IsNullOrEmpty(settings.Name))
                {
                    foreach (CelestialBody body in FlightGlobals.Bodies)
                        if (body.bodyName == settings.Name)
                        {
                            index = body.flightGlobalsIndex;
                            break;
                        }
                }

                AtmosphericConfiguration.Add(index, settings);
            }

            //For any bodies that lack a configuration, use Earth-like properties
            foreach (CelestialBody body in FlightGlobals.Bodies)
            {
                if (AtmosphericConfiguration.ContainsKey(body.flightGlobalsIndex))
                    continue;

                AtmosphericConfiguration.Add(body.flightGlobalsIndex,
                                             new BodySettings
                                             {
                                                 Index = body.flightGlobalsIndex,
                                                 Name = body.name,
                                                 ReferenceTemperature = 288,
                                                 ReferenceViscosity = 1.7894e-5f
                                             });
            }
        }

        /// <inheritdoc />
        public void BeforeSaved()
        {
            if (AtmosphericConfiguration.Count == 0)
                return;
            AtmosphericData.Clear();
            foreach (KeyValuePair<int, BodySettings> pair in AtmosphericConfiguration)
            {
                int i;
                for (i = 0; i < AtmosphericData.Count; i++)
                    if (pair.Key == AtmosphericData[i].Index || pair.Value.Name == AtmosphericData[i].Name)
                        break;

                if (i < AtmosphericData.Count)
                    AtmosphericData[i] = pair.Value;
                else
                    AtmosphericData.Add(pair.Value);
            }
        }

        /// <inheritdoc />
        public void AfterSaved()
        {
        }
    }
}
