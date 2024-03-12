using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WindowsFormsApp1;
using Newtonsoft.Json;
using System.Diagnostics;
using Nest;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace MapMaker
{
    public partial class MapMakerForm : Form
    {
        
        private bool isSaveButtonPressed    = false;
        private bool isResetButtonPressed   = false;
        public int selectedProfileIndex     = 1;
        public MapMakerForm()
        {
            InitializeComponent();
        }

        private void MapMakerForm_Load(object sender, EventArgs e)
        {
            this.FormBorderStyle         = FormBorderStyle.FixedSingle;
            RoomCount.KeyPress           += RoomCountText_KeyPress;
            GravityTextbox.KeyPress      += GravityTextbox_KeyPress;
            this.ProfileName.TextChanged += ProfileName_TextChanged;
            LoadProfile(1);
            ColorsToUpper();
            calcEstTime();
            LoadToolTips();
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            if (isSaveButtonPressed)   { SavingCancelled(); }
            if (isResetButtonPressed)  { ResetCancelled(); }
            ColorsToUpper();
            string message;
            var watch                       = Stopwatch.StartNew();
            ProfileSettings currentSettings = GetCurrentFormSettings();
            MapHandler mh                   = new MapHandler();
            string fileName                 = "MapMakerTemplate.rbe";
            string filePath                 = Path.Combine(Application.StartupPath, fileName);

            if (!File.Exists(filePath))
            {
                byte[] fileBytes = Properties.Resources.MapMakerTemplate;
                File.WriteAllBytes(filePath, fileBytes);
            }
            MapObject m = mh.ParseMap(fileName);
            int[] matids;
            
            string targetDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Diabotical\Maps\";
            if (!Directory.Exists(targetDir))
            {
                targetDir = Environment.CurrentDirectory + @"\";
            }
            

            if (!string.IsNullOrEmpty(currentSettings.Material1))
                m.AddMaterial(Material1.Text);
            else
                m.AddMaterial("sport_concrete01_darkgray");
            

            if (!string.IsNullOrEmpty(currentSettings.Material2) && currentSettings.Material2Checkbox)
                m.AddMaterial(Material2.Text);
            else
                m.AddMaterial("sport_concrete01_dirty");
            
            if (!string.IsNullOrEmpty(currentSettings.Material3) && currentSettings.Material3Checkbox)
                m.AddMaterial(Material3.Text);
            else
                m.AddMaterial("temple_stone_plain02");
            
            if (!string.IsNullOrEmpty(currentSettings.Material4) && currentSettings.Material4Checkbox)
                m.AddMaterial(currentSettings.Material4);
            else
                m.AddMaterial("planks_vert_wall");
            
            
            if (currentSettings.Material2Checkbox)
            {
                if (currentSettings.Material3Checkbox)
                {
                    if (currentSettings.Material4Checkbox)
                    {
                        matids = new int[] { 1, 2, 3, 4 };
                    }
                    else
                    {
                        matids = new int[] { 1, 2, 3, 1 };
                    }
                }
                else
                {
                    if (currentSettings.Material4Checkbox)
                    {
                        matids = new int[] { 1, 2, 1, 4 };
                    }
                    else
                    {
                        matids = new int[] { 1, 2, 1, 1 };
                    }
                }
            }
            else
            {
                if (currentSettings.Material3Checkbox)
                {
                    if (currentSettings.Material4Checkbox)
                    {
                        matids = new int[] { 1, 1, 3, 4 };
                    }
                    else
                    {
                        matids = new int[] { 1, 1, 3, 1 };
                    }
                }
                else
                {
                    if (currentSettings.Material4Checkbox)
                    {
                        matids = new int[] { 1, 1, 1, 4 };
                    }
                    else
                    {
                        matids = new int[] { 1, 1, 1, 1 };
                    }
                }
            }           
            

            int roomCount = currentSettings.RoomCount;
            if (roomCount > 0)
            {
                // Route                
                bool roomRandomApproach = currentSettings.RoomRandomApproach;                
                bool upramps            = currentSettings.Upramps;
                bool downramps          = currentSettings.Downramps;
                float roomSizeMultiply  = (float)Convert.ToDouble(currentSettings.RoomSize);

                // blocks art
                bool bevels                 = currentSettings.BevelsCheckbox;

                // rooms art
                bool roomOutlinesFlag       = currentSettings.RoomOutlinesCheckbox;
                string roomOutlinesMaterial = currentSettings.RoomOutlinesMaterial;
                string roomOutlinesColor1   = currentSettings.RoomOutlinesColor1;
                string roomOutlinesColor2   = currentSettings.RoomOutlinesColor2;
                string roomOutlinesColor3   = currentSettings.RoomOutlinesLightsColor;

                bool roomLightsFlag             = currentSettings.RoomLightsCheckbox;
                string roomLightsColor          = currentSettings.RoomLightsColor;
                bool routeArrowsFlag            = currentSettings.RouteArrowsCheckbox;
                string routeArrowsColor         = currentSettings.RouteArrowsColor;
                bool startEndBillboardsFlag     = currentSettings.StartEndBillboardsCheckbox;
                string startEndBillboardsColor  = currentSettings.StartEndBillboardsColor;

                bool rampBillboardsFlag     = currentSettings.RampBillboardsCheckbox;
                string rampBillboardsColor  = currentSettings.RampBillboardsColor;
                bool rampOutlinesFlag       = currentSettings.RampOutlinesCheckbox;
                string rampOutlinesMaterial = currentSettings.RampOutlinesMaterial;
                string rampOutlinesColor1   = currentSettings.RampOutlinesColor1;
                string rampOutlinesColor2   = currentSettings.RampOutlinesColor2;
                string rampOutlinesColor3   = currentSettings.RampOutlinesLightsColor;
                bool rampPropFlag           = currentSettings.RampPropCheckbox;
                string rampPropMaterial     = currentSettings.RampPropMaterial;
                string rampPropColor1       = currentSettings.RampPropColor1;
                string rampPropColor2       = currentSettings.RampPropColor2;
                string rampPropColor3       = currentSettings.RampPropColor3;

                // globals
                bool skyboxFlag     = currentSettings.SkyboxCheckbox;
                string skyboxName   = currentSettings.SkyboxName;
                string accent1      = currentSettings.AccentColor;

                bool bfDecal        = currentSettings.BFDecalCheckbox;
                string bfDecalPath  = currentSettings.BFDecalPath;
                string bfDecalColor = currentSettings.BFDecalColor;

                bool shadowAmbient          = currentSettings.ShadowAmbientCheckbox;
                string shadowAmbientColor   = currentSettings.ShadowAmbientColor;
                bool fog                    = currentSettings.FogCheckbox;
                string fogColor             = currentSettings.FogColor;
                bool ambient                = currentSettings.AmbientCheckbox;
                string ambientColor         = currentSettings.AmbientColor;
                bool sun                    = currentSettings.SunCheckbox;
                string sunColor             = currentSettings.SunColor;
                
                // gameplay
                bool rl         = currentSettings.RlCheckbox;
                bool dj         = currentSettings.DjCheckbox;
                bool haste      = currentSettings.HasteCheckbox;
                bool mj         = currentSettings.MjCheckbox;
                string gravity  = $"{currentSettings.GravityTextbox}";
                
                m.GenerateMaze(roomCount, roomRandomApproach, upramps, downramps, roomSizeMultiply, 
                    matids, bevels, 
                    roomOutlinesFlag, roomOutlinesMaterial, roomOutlinesColor1, roomOutlinesColor2, roomOutlinesColor3,
                    rampOutlinesFlag, rampOutlinesMaterial, rampOutlinesColor1, rampOutlinesColor2, rampOutlinesColor3,
                    rampPropFlag, rampPropMaterial, rampPropColor1, rampPropColor2, rampPropColor3,
                    rampBillboardsFlag, rampBillboardsColor,
                    startEndBillboardsFlag, startEndBillboardsColor,
                    routeArrowsFlag, routeArrowsColor,
                    roomLightsFlag, roomLightsColor,
                    skyboxFlag, skyboxName, accent1, 
                    bfDecal, bfDecalPath, bfDecalColor,
                    shadowAmbient, shadowAmbientColor,
                    fog, fogColor,
                    ambient, ambientColor,
                    sun, sunColor,
                    rl,  dj, haste, mj, gravity);

                mh.WriteMap(m, Path.Combine(targetDir, $"ttmm_{GetNewMapID(ProfileName.Text)}.rbe"));
                var elapsedMs = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds);   
                
                message = DateTime.Now.ToString("HH:mm") + " - Map generated successfully!";
                richTextBox1.SelectionColor = Color.Green;
                richTextBox1.AppendText(message + Environment.NewLine);
                richTextBox1.ScrollToCaret();
                richTextBox1.SelectionColor = richTextBox1.ForeColor;

                message = DateTime.Now.ToString("HH:mm") + " - [Blocks created: " + m.blocks_list.Count.ToString("N0") + "]";
                richTextBox1.AppendText(message + Environment.NewLine);
                richTextBox1.ScrollToCaret();

                message = DateTime.Now.ToString("HH:mm") + " - [Entities created: " + m.entities.Count.ToString("N0") + "]";
                richTextBox1.AppendText(message + Environment.NewLine);
                richTextBox1.ScrollToCaret();

                message = DateTime.Now.ToString("HH:mm") + " - [Creation took: " + string.Format("{0}.{1:D3}", (int)elapsedMs.TotalSeconds, elapsedMs.Milliseconds) + "s]";
                richTextBox1.AppendText(message + Environment.NewLine);
                richTextBox1.ScrollToCaret();
            } else
            {
                string errorMessage = DateTime.Now.ToString("HH:mm") + " - Room count must be more than 0.";

                richTextBox1.SelectionColor = Color.Red;
                richTextBox1.AppendText(errorMessage + Environment.NewLine);
                richTextBox1.ScrollToCaret();
                richTextBox1.SelectionColor = richTextBox1.ForeColor;
            }         

            
        }

        
        private void RoomCountTextbox_TextChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
            calcEstTime();
        }

        private void calcEstTime ()
        {
            bool RL, haste;
            double min, max;
            if (!int.TryParse(RoomCount.Text, out int roomCount))
            {                
                return;
            }

            if (!double.TryParse(RoomSizeBox.Text, out double roomSizeMultiply))
            {
                return;
            }
            if (rlCheckbox.Checked)
            {
                 RL = true;
            } else
            {
                 RL = false;
            }
            if (hasteCheckbox.Checked)
            {
                haste = true;
            }
            else
            {
                haste = false;
            }
            if (RL)
            {
                min = roomCount / 3.7 * roomSizeMultiply;
                max = roomCount / 2.6 * roomSizeMultiply;
            } else
            {
                min = roomCount / 2.3 * roomSizeMultiply;
                max = roomCount / 1.6 * roomSizeMultiply;
            }
            if (haste)
            {
                min /= 1.2;
                max /= 1.2;
            }
            label21.Text = min.ToString("0.0") + " - " + max.ToString("0.0") + "s";
        }

        private void SaveProfile(int profileIndex)
        {
            SettingsManager settingsManager = new SettingsManager();
            Dictionary<string, ProfileSettings> profiles = settingsManager.LoadSettingsFromFile("settings.txt");
            KeyValuePair<string, ProfileSettings> selectedProfile = profiles.ElementAt(profileIndex - 1);
            ProfileSettings settingsToSave = selectedProfile.Value;

            profiles.Remove(selectedProfile.Key);
            ColorsToUpper();
            foreach (var prop in typeof(ProfileSettings).GetProperties())
            {
                var control = Controls.Find(prop.Name, true).FirstOrDefault();
                if (control != null)
                {
                    if (control is System.Windows.Forms.TextBox)
                    {
                        var value = (control as System.Windows.Forms.TextBox).Text;
                        if (prop.PropertyType == typeof(int))
                        {
                            prop.SetValue(settingsToSave, int.Parse(value));
                        }
                        else if (prop.PropertyType == typeof(double))
                        {
                            prop.SetValue(settingsToSave, double.Parse(value));
                        }
                        else
                        {
                            prop.SetValue(settingsToSave, value);
                        }
                    }
                    else if (control is CheckBox)
                    {
                        prop.SetValue(settingsToSave, (control as CheckBox).Checked);
                    }
                    else if (control is NumericUpDown)
                    {
                        prop.SetValue(settingsToSave, Convert.ToDouble((control as NumericUpDown).Value));
                    }
                }
            }
            if (randButton1.Checked)
            {
                settingsToSave.RoomRandomApproach = true;
            }
            else
            {
                settingsToSave.RoomRandomApproach = false;
            }
            settingsToSave.RoomSize = (double)RoomSize.Value / 100;

            profiles[ProfileName.Text] = settingsToSave;

            settingsManager.SaveSettingsToFile("settings.txt", profiles);
            isSaveButtonPressed = false;
            string message = DateTime.Now.ToString("HH:mm") + $" - Profile {profileIndex} saved.";
            richTextBox1.AppendText(message + Environment.NewLine);
            richTextBox1.ScrollToCaret();
        }




        private void LoadProfile(int profileIndex)
        {
            SettingsManager settingsManager = new SettingsManager();
            Dictionary<string, ProfileSettings> profiles = settingsManager.LoadSettingsFromFile("settings.txt");
            KeyValuePair<string, ProfileSettings> selectedProfile = profiles.ElementAt(profileIndex - 1);
            ProfileSettings settingsToLoad = selectedProfile.Value;

            foreach (var prop in typeof(ProfileSettings).GetProperties())
            {
                var control = this.Controls.Find(prop.Name, true).FirstOrDefault();
                if (control != null)
                {
                    
                    if (control is System.Windows.Forms.TextBox)
                    {
                        (control as System.Windows.Forms.TextBox).Text = prop.GetValue(settingsToLoad)?.ToString();
                    }
                    else if (control is CheckBox)
                    {
                        (control as CheckBox).Checked = (bool)prop.GetValue(settingsToLoad);
                    }
                    else if (control is NumericUpDown)
                    {
                        (control as NumericUpDown).Value = Convert.ToDecimal(prop.GetValue(settingsToLoad));
                    }
                    else if (control is System.Windows.Forms.TrackBar)
                    {
                        (control as System.Windows.Forms.TrackBar).Value = (int)((double)prop.GetValue(settingsToLoad) * 100);
                    }
                }
            }
            if (settingsToLoad.RoomRandomApproach)
            {
                randButton1.Checked = true;
            }
            else
            {
                randButton0.Checked = true;
            }
            RoomSizeBox.Text = settingsToLoad.RoomSize.ToString("0.00");
            ProfileName.Text = selectedProfile.Key;
            label6.Text      = $"Profile #{profileIndex}";
            ColorsToUpper();
            string message   = DateTime.Now.ToString("HH:mm") + $" - Profile {profileIndex} loaded.";
            richTextBox1.AppendText(message + Environment.NewLine);
            richTextBox1.ScrollToCaret();
            calcEstTime();
        }
                
        private void RoomCountText_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }

            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }

        private void roomCountLabel_Click(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void roomSizeMultiplyVar_ValueChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
            RoomSizeBox.Text = (RoomSize.Value / 100.00).ToString("0.00");
            calcEstTime();
        }

        private void roomSizeMultiplyBox_TextChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }

            if (double.TryParse(RoomSizeBox.Text, out double parsedValue))
            {
                parsedValue *= 100;
                if (parsedValue < 100)
                {
                    parsedValue = 100;
                    RoomSizeBox.Text = "1.00";
                    string errorMessage = DateTime.Now.ToString("HH:mm") + " - Room size multiplier cannot be lower than 1.00.";

                    richTextBox1.SelectionColor = Color.Red;
                    richTextBox1.AppendText(errorMessage + Environment.NewLine);
                    richTextBox1.ScrollToCaret();
                    richTextBox1.SelectionColor = richTextBox1.ForeColor;
                }
                else if (parsedValue > 250)
                {
                    parsedValue = 250;
                    RoomSizeBox.Text = "2.5";
                    string errorMessage = DateTime.Now.ToString("HH:mm") + " - Room size multiplier cannot be higher than 2.50.";

                    richTextBox1.SelectionColor = Color.Red;
                    richTextBox1.AppendText(errorMessage + Environment.NewLine);
                    richTextBox1.ScrollToCaret();
                    richTextBox1.SelectionColor = richTextBox1.ForeColor;
                }

                RoomSize.Value = (int)parsedValue;
            }
            else
            {
                string errorMessage = DateTime.Now.ToString("HH:mm") + " - Invalid value in the roomSizeMultiplyBox field.";

                richTextBox1.SelectionColor = Color.Red;
                richTextBox1.AppendText(errorMessage + Environment.NewLine);
                richTextBox1.ScrollToCaret();
                richTextBox1.SelectionColor = richTextBox1.ForeColor;
            }
            calcEstTime();
        }

        private void roomSizeMultiplyBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // only int, dot or backspace 
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true; // cancele input if not one of the above
            }

            // checking that dot in float appears only once
            if (e.KeyChar == '.' && (sender as System.Windows.Forms.TextBox).Text.Contains('.'))
            {
                e.Handled = true; // cancel input if more than 1 dot in float
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            if (isResetButtonPressed) { ResetCancelled(); }
            selectedProfileIndex = 1;
            if (isSaveButtonPressed)
            {
                SaveProfile(1);
            }
            else
            {
                LoadProfile(1);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (isResetButtonPressed) { ResetCancelled(); }
            selectedProfileIndex = 2;
            if (isSaveButtonPressed)
            {
                SaveProfile(2);
            }
            else
            {
                LoadProfile(2);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (isResetButtonPressed) { ResetCancelled(); }
            selectedProfileIndex = 3;
            if (isSaveButtonPressed)
            {
                SaveProfile(3);
            }
            else
            {
                LoadProfile(3);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (isResetButtonPressed) { ResetCancelled(); }
            selectedProfileIndex = 4;
            if (isSaveButtonPressed)
            {
                SaveProfile(4);
            }
            else
            {
                LoadProfile(4);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (isResetButtonPressed) { ResetCancelled(); }
            selectedProfileIndex = 5;
            if (isSaveButtonPressed)
            {
                SaveProfile(5);
            }
            else
            {
                LoadProfile(5);
            }
        }        

        private void button6_Click(object sender, EventArgs e)
        {
            if (isResetButtonPressed) { ResetCancelled(); }
            isSaveButtonPressed = true;
            string message = DateTime.Now.ToString("HH:mm") + " - Choose profile to save current settings";
            richTextBox1.AppendText(message + Environment.NewLine);
            richTextBox1.ScrollToCaret();
        }

        private void SavingCancelled()
        {
            isSaveButtonPressed = false;
            string message = DateTime.Now.ToString("HH:mm") + " - Saving cancelled.";
            richTextBox1.AppendText(message + Environment.NewLine);
            richTextBox1.ScrollToCaret();

        }

        private void ResetCancelled()
        {
            isResetButtonPressed = false;
            string message = DateTime.Now.ToString("HH:mm") + " - Reset cancelled.";
            richTextBox1.AppendText(message + Environment.NewLine);
            richTextBox1.ScrollToCaret();
        }

        private void randButton0_CheckedChanged(object sender, EventArgs e)
        {
            if (isResetButtonPressed) { ResetCancelled(); }
            if (isSaveButtonPressed) { SavingCancelled(); }
        }

        private void randButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void color1Var_TextChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void color2Var_TextChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void color3Var_TextChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void color4Var_TextChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void color2Checkbox_CheckedChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void color3Checkbox_CheckedChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void color4Checkbox_CheckedChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void celShadingVar_CheckedChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void rlVar_CheckedChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
            calcEstTime();
        }

        private void djVar_CheckedChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void hasteVar_CheckedChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
            calcEstTime();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }

            if (!isResetButtonPressed)
            {
                isResetButtonPressed = true;
                string message = DateTime.Now.ToString("HH:mm") + $" - To reset Profile #{selectedProfileIndex} to default values press RESET again. Or any other button to cancel.";
                richTextBox1.SelectionColor = Color.Red;
                richTextBox1.AppendText(message + Environment.NewLine);
                richTextBox1.ScrollToCaret();
                richTextBox1.SelectionColor = richTextBox1.ForeColor;
            } 
            else 
            {
                SettingsManager settingsManager = new SettingsManager();
                //File.Delete("settings.txt");
                settingsManager.LoadSettingsFromFile("settings.txt");  
                isResetButtonPressed = false;

                
                Dictionary<string, ProfileSettings> profiles = settingsManager.LoadSettingsFromFile("settings.txt");
                KeyValuePair<string, ProfileSettings> selectedProfile = profiles.ElementAt(selectedProfileIndex - 1);
                profiles.Remove(selectedProfile.Key);

                var defaultSettings = ProfileSettings.CreateDefaultSettings();
                // Create default settings for each profile and save to the file
                profiles.Add($"Map{selectedProfileIndex}", defaultSettings);

                settingsManager.SaveSettingsToFile("settings.txt", profiles);

                string message = DateTime.Now.ToString("HH:mm") + $" - Current Profile #{selectedProfileIndex} have been set to default values.";
                richTextBox1.AppendText(message + Environment.NewLine);
                richTextBox1.ScrollToCaret();

                LoadProfile(selectedProfileIndex);
            }
        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {
            RoomSizeBox.Text = "0.75";
            RoomSize.Value = 75;
        }

        private void label7_Click(object sender, EventArgs e)
        {
            RoomSizeBox.Text = "1.00";
            RoomSize.Value = 100;
        }

        private void label8_Click(object sender, EventArgs e)
        {
            RoomSizeBox.Text = "1.50";
            RoomSize.Value = 150;
        }

        private void label9_Click(object sender, EventArgs e)
        {
            RoomSizeBox.Text = "2.00";
            RoomSize.Value = 200;
        }

        private void label10_Click(object sender, EventArgs e)
        {
            RoomSizeBox.Text = "2.50";
            RoomSize.Value = 250;
        }

        private void label35_Click(object sender, EventArgs e)
        {

        }

        private void textBox23_TextChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void RampOutlinesColor2Textbox_TextChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
            RampOutlinesColor2.Text = RampOutlinesColor2.Text.ToUpper(); 
        }

        private void GravityTextbox_TextChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void GravityTextbox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }

            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }

        private void SkyboxNameTextbox_TextChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void SkyboxColor1Textbox_TextChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void BFDecalPathTextbox_TextChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void ShadowAmbientTextbox_TextChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void FogTextbox_TextChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void AmbientTextbox_TextChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void SunTextbox_TextChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void RoomOutlinesTextbox_TextChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void RoomOutlinesColor1Textbox_TextChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void RoomOutlinesColor2Textbox_TextChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void RoomOutlinesLightsTextbox_TextChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void PointLightsTextbox_TextChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void RouteArrowsTextbox_TextChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void StartEndBillboardsTextbox_TextChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void RampBillboardsTextbox_TextChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void RampOutlinesTextbox_TextChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void RampOutlinesLightsTextbox_TextChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void RampPropTextbox_TextChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void RampPropColor2Textbox_TextChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void RampPropColor3Textbox_TextChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void RampPropColor1Textbox_TextChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void RampOutlinesColor1Textbox_TextChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void UprampsCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void DownrampsCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void RoomOutlinesCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void PointLightsCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void RouteArrowsCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void StartEndBillboardsCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void RampBillboardsCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void RampOutlinesCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void RampPropCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void SkyboxCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void BFDecalCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void ShadowAmbientCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void FogCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void AmbientCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void SunCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (isSaveButtonPressed) { SavingCancelled(); }
            if (isResetButtonPressed) { ResetCancelled(); }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            ColorsToUpper();
            ProfileSettings settingsToCopy = GetCurrentFormSettings();
            Clipboard.SetText(JsonConvert.SerializeObject(settingsToCopy));
            string message = DateTime.Now.ToString("HH:mm") + " - Current settings copied to clipboard.";
            richTextBox1.SelectionColor = Color.Green;
            richTextBox1.AppendText(message + Environment.NewLine);
            richTextBox1.ScrollToCaret();
            richTextBox1.SelectionColor = richTextBox1.ForeColor;
        }

        public ProfileSettings GetCurrentFormSettings()
        {
            return new ProfileSettings
            {
                // Route
                RoomCount           = int.Parse(RoomCount.Text),
                RoomRandomApproach  = randButton1.Checked ? true : false,
                Upramps             = Upramps.Checked,
                Downramps           = Downramps.Checked,
                RoomSize            = Convert.ToDouble(RoomSizeBox.Text),

                // Blocks Art
                Material1           = Material1.Text,
                Material2Checkbox   = Material2Checkbox.Checked,
                Material2           = Material2.Text,
                Material3Checkbox   = Material3Checkbox.Checked,
                Material3           = Material3.Text,
                Material4Checkbox   = Material4Checkbox.Checked,
                Material4           = Material4.Text,
                BevelsCheckbox      = BevelsCheckbox.Checked,

                // Rooms Art
                RoomOutlinesCheckbox    = RoomOutlinesCheckbox.Checked,
                RoomOutlinesMaterial    = RoomOutlinesMaterial.Text,
                RoomOutlinesColor1      = RoomOutlinesColor1.Text,
                RoomOutlinesColor2      = RoomOutlinesColor2.Text,
                RoomOutlinesLightsColor = RoomOutlinesLightsColor.Text,

                RoomLightsCheckbox  = RoomLightsCheckbox.Checked,
                RoomLightsColor     = RoomLightsColor.Text,

                RouteArrowsCheckbox = RouteArrowsCheckbox.Checked,
                RouteArrowsColor    = RouteArrowsColor.Text,

                StartEndBillboardsCheckbox  = StartEndBillboardsCheckbox.Checked,
                StartEndBillboardsColor     = StartEndBillboardsColor.Text,

                RampOutlinesCheckbox    = RampOutlinesCheckbox.Checked,
                RampOutlinesMaterial    = RampOutlinesMaterial.Text,
                RampOutlinesColor1      = RampOutlinesColor1.Text,
                RampOutlinesColor2      = RampOutlinesColor2.Text,
                RampOutlinesLightsColor = RampOutlinesLightsColor.Text,

                RampPropCheckbox    = RampPropCheckbox.Checked,
                RampPropMaterial    = RampPropMaterial.Text,
                RampPropColor1      = RampPropColor1.Text,
                RampPropColor2      = RampPropColor2.Text,
                RampPropColor3      = RampPropColor3.Text,

                RampBillboardsCheckbox  = RampBillboardsCheckbox.Checked,
                RampBillboardsColor     = RampBillboardsColor.Text,

                // Globals
                SkyboxCheckbox  = SkyboxCheckbox.Checked,
                SkyboxName      = SkyboxName.Text,
                AccentColor     = AccentColor.Text,

                BFDecalCheckbox = BFDecalCheckbox.Checked,
                BFDecalPath     = BFDecalPath.Text,
                BFDecalColor    = BFDecalColor.Text,

                ShadowAmbientCheckbox   = ShadowAmbientCheckbox.Checked,
                ShadowAmbientColor      = ShadowAmbientColor.Text,
                FogCheckbox             = FogCheckbox.Checked,
                FogColor                = FogColor.Text,
                AmbientCheckbox         = AmbientCheckbox.Checked,
                AmbientColor            = AmbientColor.Text,
                SunCheckbox             = SunCheckbox.Checked,
                SunColor                = SunColor.Text,

                // Gameplay
                RlCheckbox      = rlCheckbox.Checked,
                DjCheckbox      = djCheckbox.Checked,
                HasteCheckbox   = hasteCheckbox.Checked,
                MjCheckbox      = mjCheckbox.Checked,
                GravityTextbox  = int.Parse(GravityTextbox.Text)
            };
        }

        private void button9_Click(object sender, EventArgs e)
        {
            // Get the settings string from the clipboard
            string settingsString = Clipboard.GetText();

            try
            {


                // Convert the string back into a ProfileSettings object
                ProfileSettings importedSettings = JsonConvert.DeserializeObject<ProfileSettings>(settingsString);

                // Apply the imported settings to the form variables

                // Route
                RoomCount.Text = importedSettings.RoomCount.ToString();
                randButton1.Checked = importedSettings.RoomRandomApproach;
                Upramps.Checked = importedSettings.Upramps;
                Downramps.Checked = importedSettings.Downramps;
                RoomSizeBox.Text = importedSettings.RoomSize.ToString();

                // Blocks art
                Material1.Text = importedSettings.Material1;
                Material2Checkbox.Checked = importedSettings.Material2Checkbox;
                Material2.Text = importedSettings.Material2;
                Material3Checkbox.Checked = importedSettings.Material3Checkbox;
                Material3.Text = importedSettings.Material3;
                Material4Checkbox.Checked = importedSettings.Material4Checkbox;
                Material4.Text = importedSettings.Material4;
                BevelsCheckbox.Checked = importedSettings.BevelsCheckbox;

                // Rooms art
                RoomOutlinesCheckbox.Checked = importedSettings.RoomOutlinesCheckbox;
                RoomOutlinesMaterial.Text = importedSettings.RoomOutlinesMaterial;
                RoomOutlinesColor1.Text = importedSettings.RoomOutlinesColor1;
                RoomOutlinesColor2.Text = importedSettings.RoomOutlinesColor2;
                RoomOutlinesLightsColor.Text = importedSettings.RoomOutlinesLightsColor;

                RoomLightsCheckbox.Checked = importedSettings.RoomLightsCheckbox;
                RoomLightsColor.Text = importedSettings.RoomLightsColor;

                RouteArrowsCheckbox.Checked = importedSettings.RouteArrowsCheckbox;
                RouteArrowsColor.Text = importedSettings.RouteArrowsColor;

                StartEndBillboardsCheckbox.Checked = importedSettings.StartEndBillboardsCheckbox;
                StartEndBillboardsColor.Text = importedSettings.StartEndBillboardsColor;

                RampOutlinesCheckbox.Checked = importedSettings.RampOutlinesCheckbox;
                RampOutlinesMaterial.Text = importedSettings.RampOutlinesMaterial;
                RampOutlinesColor1.Text = importedSettings.RampOutlinesColor1;
                RampOutlinesColor2.Text = importedSettings.RampOutlinesColor2;
                RampOutlinesLightsColor.Text = importedSettings.RampOutlinesLightsColor;

                RampPropCheckbox.Checked = importedSettings.RampPropCheckbox;
                RampPropMaterial.Text = importedSettings.RampPropMaterial;
                RampPropColor1.Text = importedSettings.RampPropColor1;
                RampPropColor2.Text = importedSettings.RampPropColor2;
                RampPropColor3.Text = importedSettings.RampPropColor3;

                RampBillboardsCheckbox.Checked = importedSettings.RampBillboardsCheckbox;
                RampBillboardsColor.Text = importedSettings.RampBillboardsColor;

                // Globals
                SkyboxCheckbox.Checked = importedSettings.SkyboxCheckbox;
                SkyboxName.Text = importedSettings.SkyboxName;
                AccentColor.Text = importedSettings.AccentColor;

                BFDecalCheckbox.Checked = importedSettings.BFDecalCheckbox;
                BFDecalPath.Text = importedSettings.BFDecalPath;
                BFDecalColor.Text = importedSettings.BFDecalColor;

                ShadowAmbientCheckbox.Checked = importedSettings.ShadowAmbientCheckbox;
                ShadowAmbientColor.Text = importedSettings.ShadowAmbientColor;
                FogCheckbox.Checked = importedSettings.FogCheckbox;
                FogColor.Text = importedSettings.FogColor;
                AmbientCheckbox.Checked = importedSettings.AmbientCheckbox;
                AmbientColor.Text = importedSettings.AmbientColor;
                SunCheckbox.Checked = importedSettings.SunCheckbox;
                SunColor.Text = importedSettings.SunColor;

                // Gameplay
                rlCheckbox.Checked = importedSettings.RlCheckbox;
                djCheckbox.Checked = importedSettings.DjCheckbox;
                hasteCheckbox.Checked = importedSettings.HasteCheckbox;
                mjCheckbox.Checked = importedSettings.MjCheckbox;
                GravityTextbox.Text = importedSettings.GravityTextbox.ToString();
                ColorsToUpper();
                string message = DateTime.Now.ToString("HH:mm") + " - Import completed.";
                richTextBox1.SelectionColor = Color.Green;
                richTextBox1.AppendText(message + Environment.NewLine);
                richTextBox1.ScrollToCaret();
                richTextBox1.SelectionColor = richTextBox1.ForeColor;
            } catch (JsonReaderException)
            {
                MessageBox.Show("ERROR: The input profile settings is not valid JSON.");
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(label17.Text);
            string message = DateTime.Now.ToString("HH:mm") + " - Console launch command has been copied to clipboard.";
            richTextBox1.SelectionColor = Color.Green;
            richTextBox1.AppendText(message + Environment.NewLine);
            richTextBox1.ScrollToCaret();
            richTextBox1.SelectionColor = richTextBox1.ForeColor;
        }

        private void label17_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(label17.Text);
            string message = DateTime.Now.ToString("HH:mm") + " - Console launch command has been copied to clipboard.";
            richTextBox1.SelectionColor = Color.Green;
            richTextBox1.AppendText(message + Environment.NewLine);
            richTextBox1.ScrollToCaret();
            richTextBox1.SelectionColor = richTextBox1.ForeColor;
        }
        private void ProfileName_TextChanged(object sender, EventArgs e)
        {
            string profileName = this.ProfileName.Text;
            profileName        = GetNewMapID(profileName);
            string newText     = $"/edit ttmm_{profileName} race";
            this.label17.Text = newText;
        }

        public string GetNewMapID(string profileName)
        {
            profileName = profileName.ToLower();
            profileName = profileName.Replace(" ", "_");
            profileName = System.Text.RegularExpressions.Regex.Replace(profileName, @"[^a-z0-9_]", "");
            return profileName;
        }

        public void ColorsToUpper()
        {
            RoomOutlinesColor1.Text      = RoomOutlinesColor1.Text.ToUpper();
            RoomOutlinesColor2.Text      = RoomOutlinesColor2.Text.ToUpper();
            RoomOutlinesLightsColor.Text = RoomOutlinesLightsColor.Text.ToUpper();

            RoomLightsColor.Text         = RoomLightsColor.Text.ToUpper();
            RouteArrowsColor.Text        = RouteArrowsColor.Text.ToUpper();
            StartEndBillboardsColor.Text = StartEndBillboardsColor.Text.ToUpper();

            RampBillboardsColor.Text     = RampBillboardsColor.Text.ToUpper();
            RampOutlinesColor1.Text      = RampOutlinesColor1.Text.ToUpper();
            RampOutlinesColor2.Text      = RampOutlinesColor2.Text.ToUpper();
            RampOutlinesLightsColor.Text = RampOutlinesLightsColor.Text.ToUpper();

            RampPropColor1.Text = RampPropColor1.Text.ToUpper();
            RampPropColor2.Text = RampPropColor2.Text.ToUpper();
            RampPropColor3.Text = RampPropColor3.Text.ToUpper();

            AccentColor.Text  = AccentColor.Text.ToUpper();
            BFDecalColor.Text = BFDecalColor.Text.ToUpper();

            ShadowAmbientColor.Text = ShadowAmbientColor.Text.ToUpper();
            FogColor.Text           = FogColor.Text.ToUpper();
            AmbientColor.Text       = AmbientColor.Text.ToUpper();
            SunColor.Text           = SunColor.Text.ToUpper();
        }

        private void pictureBox10_Click(object sender, EventArgs e)
        {
            string url = "https://github.com/ScheduleTracker/DiaboticalTracker/blob/master/packs/textures_customization.dbp.files";            
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }

        private void pictureBox9_Click(object sender, EventArgs e)
        {
            string url = "https://liquipedia.net/arenafps/Diabotical/Map_Editing/Global#Skybox";
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }

        public void LoadToolTips()
        {
            System.Windows.Forms.ToolTip toolTip1 = new System.Windows.Forms.ToolTip();

            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;

            toolTip1.SetToolTip(this.RoomCountLabel, "How many rooms to generate ( >=1 )");
            toolTip1.SetToolTip(this.RoomCount, "How many rooms to generate ( >=1 )");

            toolTip1.SetToolTip(this.GenerationApproachLabel, "Pattern of generation: 1) Normal - linear type of map 2) Random - more chaotic results, might be 'non-walkable', but better for RL maps");
            toolTip1.SetToolTip(this.randButton0, "Pattern of generation: 1) Normal - linear type of map 2) Random - more chaotic results, might be 'non-walkable'");
            toolTip1.SetToolTip(this.randButton1, "Pattern of generation: 1) Normal - linear type of map 2) Random - more chaotic and interesting results, but might be 'non-walkable'");

            toolTip1.SetToolTip(this.Material1Label, "To find Material name select face of the block and open Editpad -> Properties -> Material");
            toolTip1.SetToolTip(this.Material2Checkbox, "To find Material name select face of the block and open Editpad -> Properties -> Material");
            toolTip1.SetToolTip(this.Material3Checkbox, "To find Material name select face of the block and open Editpad -> Properties -> Material");
            toolTip1.SetToolTip(this.Material4Checkbox, "To find Material name select face of the block and open Editpad -> Properties -> Material");
            toolTip1.SetToolTip(this.Material1, "To find Material name select face of the block and open Editpad -> Properties -> Material");
            toolTip1.SetToolTip(this.Material2, "To find Material name select face of the block and open Editpad -> Properties -> Material");
            toolTip1.SetToolTip(this.Material3, "To find Material name select face of the block and open Editpad -> Properties -> Material");
            toolTip1.SetToolTip(this.Material4, "To find Material name select face of the block and open Editpad -> Properties -> Material");
            toolTip1.SetToolTip(this.BevelsCheckbox, "You can see example of this in 'tt_cel_shaded' map. Downside(upside?) - map will be darker as decals paints every block and prop by default too. Try it.");

            toolTip1.SetToolTip(this.RoomOutlinesCheckbox, "Generates props to outline floor of the room");
            toolTip1.SetToolTip(this.RoomOutlinesMaterial, "You can see all props and their materials in 'tt_props20' and 'prop_materials' maps. '/forsel set %1' (in console) to see material of the prop.");
            toolTip1.SetToolTip(this.label15, "You can see all props and their materials in 'tt_props20' and 'prop_materials' maps. '/forsel set %1' (in console) to see material of the prop.");
            toolTip1.SetToolTip(this.RoomOutlinesColor1Label, "RRGGBB");
            toolTip1.SetToolTip(this.RoomOutlinesColor1, "RRGGBB");
            toolTip1.SetToolTip(this.RoomOutlinesColor2Label, "RRGGBB");
            toolTip1.SetToolTip(this.RoomOutlinesColor2, "RRGGBB");
            toolTip1.SetToolTip(this.RoomOutlinesLightsColor, "RRGGBB");
            toolTip1.SetToolTip(this.RoomOutlinesColor3Label, "RRGGBB");

            toolTip1.SetToolTip(this.RoomLightsCheckbox, "Generates Point Light (auto-size) at middle of every room (RRGGBB)");
            toolTip1.SetToolTip(this.RoomLightsColor, "Generates Point Light (auto-size) at middle of every room (RRGGBB)");
            toolTip1.SetToolTip(this.RouteArrowsCheckbox, "Generates 3 billboard Arrows in the middle of the room before each next left/right rooms (AARRGGBB or RRGGBB)");
            toolTip1.SetToolTip(this.RouteArrowsColor, "Generates 3 billboard Arrows in the middle of the room before each next left/right rooms (AARRGGBB or RRGGBB)");
            toolTip1.SetToolTip(this.StartEndBillboardsCheckbox, "Generates billboards that visualizes Start/End triggers zones (AARRGGBB or RRGGBB)");
            toolTip1.SetToolTip(this.StartEndBillboardsColor, "Generates billboard that visualizes Start/End trigger zones (AARRGGBB or RRGGBB)");

            toolTip1.SetToolTip(this.RampBillboardsCheckbox, "If you'll set ARGB value - ramp will get 'transparent' effect. ('tt_neon_strafe', 'tt_ethereal_grounds') Don't use this with 'Ramp prop: ON'. Ramp outlines recommended to ON. (AARRGGBB or RRGGBB)");
            toolTip1.SetToolTip(this.RampBillboardsColor, "If you'll set ARGB value - ramp will get 'transparent' effect. ('tt_neon_strafe', 'tt_ethereal_grounds') Don't use this with 'Ramp prop: ON'. Ramp outlines recommended to ON. (AARRGGBB or RRGGBB)");

            toolTip1.SetToolTip(this.RampOutlinesCheckbox, "Generates props to outline ramps");
            toolTip1.SetToolTip(this.RampOutlinesMaterial, "You can see all props and their materials in 'tt_props20' and 'prop_materials' maps. '/forsel set %1' (in console) to see material of the prop.");
            toolTip1.SetToolTip(this.RampOutlinesColor1Label, "RRGGBB");
            toolTip1.SetToolTip(this.RampOutlinesColor1, "RRGGBB");
            toolTip1.SetToolTip(this.RampOutlinesColor2Label, "RRGGBB");
            toolTip1.SetToolTip(this.RampOutlinesColor2, "RRGGBB");
            toolTip1.SetToolTip(this.RampOutlinesLightsColor, "RRGGBB");
            toolTip1.SetToolTip(this.RampOutlinesColor3Label, "RRGGBB");

            toolTip1.SetToolTip(this.RampPropCheckbox, "Sets material of Ramps (by default it's invisible w/o RampProp or RampBillboards ON). Don't use this with RampBillboards ON");
            toolTip1.SetToolTip(this.RampPropMaterial, "You can see all props and their materials in 'tt_props20' and 'prop_materials' maps. '/forsel set %1' (in console) to see material of the prop.");
            toolTip1.SetToolTip(this.RampPropColor1Label, "RRGGBB");
            toolTip1.SetToolTip(this.RampPropColor1, "RRGGBB");
            toolTip1.SetToolTip(this.RampPropColor2Label, "RRGGBB");
            toolTip1.SetToolTip(this.RampPropColor2, "RRGGBB");
            toolTip1.SetToolTip(this.RampPropColor3Label, "RRGGBB");
            toolTip1.SetToolTip(this.RampPropColor3Label, "RRGGBB");

            toolTip1.SetToolTip(this.SkyboxCheckbox, "Check link for all available skyboxes");
            toolTip1.SetToolTip(this.SkyboxName, "Check link for all available skyboxes");
            toolTip1.SetToolTip(this.label29, "Sets accent1 global color. Can be used for Block material ('colorable_surface:1'). Color of SmartSpawn color indicator");
            toolTip1.SetToolTip(this.AccentColor, "Sets accent1 global color. Can be used for Block material ('colorable_surface:1'). Color of SmartSpawn color indicator");
            toolTip1.SetToolTip(this.pictureBox9, "https://liquipedia.net/arenafps/Diabotical/Map_Editing/Global#Skybox");

            toolTip1.SetToolTip(this.BFDecalCheckbox, "Like BFG, but it's decal. Look under the map. Check link for (almost) all available decals in-game.");
            toolTip1.SetToolTip(this.BFDecalPath, "Like BFG, but it's decal. Look under the map. Check link for (almost) all available decals in-game.");
            toolTip1.SetToolTip(this.BFDecalColor, "AARRGGBB or RRGGBB");
            toolTip1.SetToolTip(this.pictureBox10, "https://github.com/ScheduleTracker/DiaboticalTracker/blob/master/packs/textures_customization.dbp.files");

            toolTip1.SetToolTip(this.ShadowAmbientCheckbox, "RRGGBB");
            toolTip1.SetToolTip(this.ShadowAmbientColor, "RRGGBB");
            toolTip1.SetToolTip(this.FogCheckbox, "Also lightens up the skybox from the bottom (RRGGBB)");
            toolTip1.SetToolTip(this.FogColor, "Also lightens up the skybox from the bottom (RRGGBB)");
            toolTip1.SetToolTip(this.AmbientCheckbox, "RRGGBB");
            toolTip1.SetToolTip(this.AmbientColor, "RRGGBB");
            toolTip1.SetToolTip(this.SunCheckbox, "RRGGBB");
            toolTip1.SetToolTip(this.SunColor, "RRGGBB");

            toolTip1.SetToolTip(this.rlCheckbox, "Gives RL at Start trigger");
            toolTip1.SetToolTip(this.djCheckbox, "Gives Double jumps on Player connect");
            toolTip1.SetToolTip(this.hasteCheckbox, "Gives Player changed air_accel + haste powerup at Start trigger");
            toolTip1.SetToolTip(this.mjCheckbox, "Vintage and VQ3 bros can do sters jumping like CPM");

            toolTip1.SetToolTip(this.pictureBox7, "Tap name to change it. Automatically converts into MapID with same name");
            toolTip1.SetToolTip(this.label6, "Tap name to change it. Automatically converts into MapID with same name");

            toolTip1.SetToolTip(this.pictureBox8, "https://www.youtube.com/watch?v=dQw4w9WgXcQ");
            toolTip1.SetToolTip(this.label20, "https://www.youtube.com/watch?v=dQw4w9WgXcQ");
            toolTip1.SetToolTip(this.label21, "https://www.youtube.com/watch?v=dQw4w9WgXcQ");

            toolTip1.SetToolTip(this.pictureBox1, "Paste this command into in-game console to launch the map");
            toolTip1.SetToolTip(this.label16, "Paste this command into in-game console to launch the map");
            toolTip1.SetToolTip(this.label17, "Paste this command into in-game console to launch the map");

            SettingsManager settingsManager = new SettingsManager();
            Dictionary<string, ProfileSettings> profiles = settingsManager.LoadSettingsFromFile("settings.txt");
            List<string> profileNames = new List<string>(profiles.Keys);
            toolTip1.SetToolTip(this.button1, profileNames[0]);
            toolTip1.SetToolTip(this.button2, profileNames[1]);
            toolTip1.SetToolTip(this.button3, profileNames[2]);
            toolTip1.SetToolTip(this.button4, profileNames[3]);
            toolTip1.SetToolTip(this.button5, profileNames[4]);

            toolTip1.SetToolTip(this.button9, "Import from clipboard JSON with map settings into MapMaker");
            toolTip1.SetToolTip(this.button8, "Export to clipboard JSON with map settings");
            toolTip1.SetToolTip(this.button7, "Reset current Profile to default values");
            toolTip1.SetToolTip(this.button6, "Save current map settings to Profile 1-5");
            toolTip1.SetToolTip(this.btnGenerate, "Friendly reminder: App won't respond until generation ends");

            toolTip1.SetToolTip(this.label24, "Made by Dhcold");

        }
    }
    
}

