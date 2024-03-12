using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using System;

namespace MapMaker
{
    public class SettingsManager
    {
        public void SaveSettingsToFile(string filename, Dictionary<string, ProfileSettings> profiles)
        {
            using (StreamWriter writer = new StreamWriter(filename))
            {
                foreach (var kvp in profiles)
                {
                    string profileName = kvp.Key;
                    ProfileSettings settings = kvp.Value;

                    writer.WriteLine("ProfileName=" + profileName);

                    foreach (var prop in typeof(ProfileSettings).GetProperties())
                    {
                        writer.WriteLine($"{prop.Name}={prop.GetValue(settings)}");
                    }

                    writer.WriteLine();
                }
            }
        }

        public Dictionary<string, ProfileSettings> LoadSettingsFromFile(string filename)
        {
            var profiles = new Dictionary<string, ProfileSettings>();

            if (File.Exists(filename))
            {
                ProfileSettings currentProfile = null;

                foreach (var line in File.ReadLines(filename))
                {
                    var parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        if (parts[0] == "ProfileName")
                        {
                            currentProfile = new ProfileSettings();
                            profiles[parts[1]] = currentProfile;
                        }

                        if (currentProfile != null)
                        {
                            var property = typeof(ProfileSettings).GetProperty(parts[0]);
                            if (property != null)
                            {
                                var convertedValue = Convert.ChangeType(parts[1], property.PropertyType);
                                property.SetValue(currentProfile, convertedValue);
                            }
                        }
                    }
                }
            }
            else
            {
                // Create new profile and use defaultSettings as default values
                var defaultSettings = ProfileSettings.CreateDefaultSettings();
                // Create default settings for each profile and save to the file
                profiles.Add("Map1", defaultSettings);
                profiles.Add("Map2", defaultSettings);
                profiles.Add("Map3", defaultSettings);
                profiles.Add("Map4", defaultSettings);
                profiles.Add("Map5", defaultSettings);
                SaveSettingsToFile(filename, profiles);
            }

            return profiles;
        }


    }


    public class ProfileSettings
    {
        //  route 
        public int RoomCount { get; set; }        
        public bool RoomRandomApproach { get; set; }
        public bool Upramps { get; set; }
        public bool Downramps { get; set; }
        public double RoomSize { get; set; }

        // blocks art
        public string Material1 { get; set; }
        public string Material2 { get; set; }
        public string Material3 { get; set; }
        public string Material4 { get; set; }
        public bool Material2Checkbox { get; set; }
        public bool Material3Checkbox { get; set; }
        public bool Material4Checkbox { get; set; }
        public bool BevelsCheckbox { get; set; }

        // rooms art
        public bool RoomOutlinesCheckbox { get; set; }
        public string RoomOutlinesMaterial { get; set; }
        public string RoomOutlinesColor1 { get; set; }
        public string RoomOutlinesColor2 { get; set; }
        public string RoomOutlinesLightsColor { get; set; }

        public bool RoomLightsCheckbox { get; set; }
        public string RoomLightsColor { get; set; }
        public bool RouteArrowsCheckbox { get; set; }
        public string RouteArrowsColor { get; set; }
        public bool StartEndBillboardsCheckbox { get; set; }
        public string StartEndBillboardsColor { get; set; }

        public bool RampBillboardsCheckbox { get; set; }
        public string RampBillboardsColor { get; set; }
        public bool RampOutlinesCheckbox { get; set; }
        public string RampOutlinesMaterial { get; set; }
        public string RampOutlinesColor1 { get; set; }
        public string RampOutlinesColor2 { get; set; }
        public string RampOutlinesLightsColor { get; set; }
        public bool RampPropCheckbox { get; set; }
        public string RampPropMaterial { get; set; }
        public string RampPropColor1 { get; set; }
        public string RampPropColor2 { get; set; }
        public string RampPropColor3 { get; set; }        
        
        // globals
        public bool SkyboxCheckbox { get; set; }
        public string SkyboxName { get; set; }
        public string AccentColor { get; set; }
        public bool BFDecalCheckbox { get; set; }
        public string BFDecalPath { get; set; }
        public string BFDecalColor { get; set; }
        public bool ShadowAmbientCheckbox { get; set; }
        public string ShadowAmbientColor { get; set; }
        public bool FogCheckbox { get; set; }
        public string FogColor { get; set; }
        public bool AmbientCheckbox { get; set; }
        public string AmbientColor { get; set; }
        public bool SunCheckbox { get; set; }
        public string SunColor { get; set; }
        public bool RlCheckbox { get; set; }
        public bool DjCheckbox { get; set; }
        public bool HasteCheckbox { get; set; }
        public bool MjCheckbox { get; set; }
        public int GravityTextbox { get; set; }

        public static ProfileSettings CreateDefaultSettings()
        {
            return new ProfileSettings
            {
                // Route
                RoomCount           = 10,
                RoomRandomApproach  = false,
                Upramps             = true,
                Downramps           = true,
                RoomSize            = 1,

                // Blocks Art
                Material1           = "sport_concrete01_darkgray",
                Material2Checkbox   = true,
                Material2           = "sport_concrete01_dirty",
                Material3Checkbox   = true,
                Material3           = "temple_stone_plain02",
                Material4Checkbox   = true,
                Material4           = "planks_vert_wall",
                BevelsCheckbox      = true,

                // Rooms Art
                RoomOutlinesCheckbox        = true,
                RoomOutlinesMaterial        = "colorable",
                RoomOutlinesColor1          = "FFE700",
                RoomOutlinesColor2          = "000000",
                RoomOutlinesLightsColor     = "FFAF00",

                RoomLightsCheckbox          = true,
                RoomLightsColor             = "A4A4A4",

                RouteArrowsCheckbox         = true,
                RouteArrowsColor            = "FAFF00",

                StartEndBillboardsCheckbox  = true,
                StartEndBillboardsColor     = "151515",

                RampOutlinesCheckbox        = true,
                RampOutlinesMaterial        = "colorable",
                RampOutlinesColor1          = "000000",
                RampOutlinesColor2          = "000000",
                RampOutlinesLightsColor     = "5E4B15",

                RampPropCheckbox            = true,
                RampPropMaterial            = "colorable",
                RampPropColor1              = "090909",
                RampPropColor2              = "000000",
                RampPropColor3              = "000000",

                RampBillboardsCheckbox      = false,
                RampBillboardsColor         = "FFFFFF", 

                // Globals
                SkyboxCheckbox        = true,
                SkyboxName            = "deepspace",
                AccentColor           = "FFB400",

                BFDecalCheckbox       = true,
                BFDecalPath           = "textures/customization/st/cybersym_routing",
                BFDecalColor          = "FFFFFF",

                ShadowAmbientCheckbox = true,
                ShadowAmbientColor    = "FFFFFF",
                FogCheckbox           = true,
                FogColor              = "261D00",
                AmbientCheckbox       = true,
                AmbientColor          = "FFFFFF",
                SunCheckbox           = true,
                SunColor              = "FFFFFF",


                // Gameplay & Utility
                RlCheckbox      = false,
                DjCheckbox      = false,
                HasteCheckbox   = false,
                MjCheckbox      = true,
                GravityTextbox  = 800
            };
        }
    }
}
