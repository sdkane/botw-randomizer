using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Forms;

namespace BOTWSplitsRandomizer {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();

            List<string> movementProbabilities = new List<string> { "Very Low", "Low", "Medium", "High", "Very High" };
            cBtbProbability.ItemsSource = movementProbabilities;
            cBtbProbability.SelectedItem = "Medium";
            cWindbombProbability.ItemsSource = movementProbabilities;
            cWindbombProbability.SelectedItem = "Medium";
            cStasisLaunchProbability.ItemsSource = movementProbabilities;
            cStasisLaunchProbability.SelectedItem = "Medium";
            cHorsebackProbability.ItemsSource = movementProbabilities;
            cHorsebackProbability.SelectedItem = "Medium";
            cShieldSurfProbability.ItemsSource = movementProbabilities;
            cShieldSurfProbability.SelectedItem = "Medium";
            cWalkProbability.ItemsSource = movementProbabilities;
            cWalkProbability.SelectedItem = "Medium";
            cMoonJumpProbability.ItemsSource = movementProbabilities;
            cMoonJumpProbability.SelectedItem = "Medium";
        }

        private void CreateSplits(object sender, RoutedEventArgs e) {
            FileProperties fileProps = GetFileProperties();
            if (fileProps == null) { return; }

            int safetyEscape = 0;
            List<string> movementTypes = new List<string> { };
            List<string> locations = new List<string> { };
            List<string> finalList = new List<string> { };

            Random rnd = new Random();
            int locIndex;
            int movIndex;
            string text;
            string innerText;

            if (cBtb.IsChecked == true) { AddMovementWithProbability(ref movementTypes, "BTB", cBtbProbability.SelectedIndex); };
            if (cWindbomb.IsChecked == true) { AddMovementWithProbability(ref movementTypes, "Windbomb", cWindbombProbability.SelectedIndex); };
            if (cStasisLaunch.IsChecked == true) { AddMovementWithProbability(ref movementTypes, "Stasis Launch", cStasisLaunchProbability.SelectedIndex); };
            if (cHorseback.IsChecked == true) { AddMovementWithProbability(ref movementTypes, "Horseback", cHorsebackProbability.SelectedIndex); };
            if (cShieldSurf.IsChecked == true) { AddMovementWithProbability(ref movementTypes, "Shield Surf", cShieldSurfProbability.SelectedIndex); };
            if (cWalk.IsChecked == true) { AddMovementWithProbability(ref movementTypes, "Walk", cWalkProbability.SelectedIndex); };
            if (cMoonJump.IsChecked == true) { AddMovementWithProbability(ref movementTypes, "Moon Jump", cMoonJumpProbability.SelectedIndex); };

            if (cShrines.IsChecked == true) { locations.AddRange(Shrines); };
            if (cTowers.IsChecked == true) { locations.AddRange(Towers); };
            if (cBeasts.IsChecked == true) { locations.AddRange(Beasts); };
            if (cStables.IsChecked == true) { locations.AddRange(Stables); };
            if (cFairies.IsChecked == true) { locations.AddRange(Fairies); };

            int shrineCounter = 1;

            if (cRuneShrines.IsChecked == true) {
                List<string> tempLocations = RuneShrines;
                for (int i = 0; i <= 3; i++) {
                    locIndex = rnd.Next(1, tempLocations.Count) - 1;
                    finalList.Add(tempLocations[locIndex] + " [" + shrineCounter + "]");
                    tempLocations.RemoveAt(locIndex);
                    shrineCounter += 1;
                }
            }
            else {
                for (int i = 0; i <= 3; i++) {
                    finalList.Add(RuneShrines[i] + " [" + shrineCounter + "]");
                    shrineCounter += 1;
                }
            }

            while (locations.Count > 0 && safetyEscape < 1000) {
                locIndex = rnd.Next(0, locations.Count);
                movIndex = rnd.Next(0, movementTypes.Count);
                if (Shrines.Contains(locations[locIndex]) || RuneShrines.Contains(locations[locIndex])) {
                    innerText = " [" + shrineCounter + "] ";
                    shrineCounter += 1;
                }
                else {
                    innerText = " ";
                }
                text = locations[locIndex] + innerText;
                if (movementTypes.Count > 0) { text += movementTypes[movIndex]; }
                finalList.Add(text);
                locations.RemoveAt(locIndex);
                safetyEscape += 1;
            }

            finalList.AddRange(Ganon);

            using (TextWriter tw = new StreamWriter(fileProps.Path + "\\" + fileProps.Name)) {
                foreach (String s in finalList)
                    tw.WriteLine(s);
            }

            System.Diagnostics.Process.Start(fileProps.Path + "\\" + fileProps.Name);
        }

        private static void AddMovementWithProbability(ref List<string> movementTypes, string movement, int probability) {
            for (int i = 1; i <= (probability + 1); i++) {
                movementTypes.Add(movement);
            }
        }

        public class FileProperties {
            public string Name = "";
            public string Path = "";
        }

        private static FileProperties GetFileProperties() {
            try {
                FileProperties fileprops = new FileProperties();
                while (true) {
                    SaveFileDialog savedialog = new SaveFileDialog() { CheckFileExists = false, DefaultExt = ".txt", Filter = "Text Files | *.txt", Title = "BOTW Randomized Splits" };
                    savedialog.FileName = "botw_randomized_splits";

                    System.Windows.Forms.DialogResult response = savedialog.ShowDialog();
                    if (response.Equals(System.Windows.Forms.DialogResult.OK)) {
                        fileprops.Name = Path.GetFileName(savedialog.FileName);
                        fileprops.Path = Path.GetDirectoryName(savedialog.FileName) + "\\";
                    }
                    else { return null; }
                    if (CheckFileLock(savedialog.FileName) == true) { System.Windows.Forms.MessageBox.Show("Selected file open for editing" + System.Environment.NewLine + "Close file or select new filename", "BastianCAD Extract", MessageBoxButtons.OK); }
                    else { return fileprops; }
                }
            }
            catch (System.Exception ex) { System.Diagnostics.Debug.Print(ex.Message); }
            return null;
        }

        /// <summary>returns true if the file is locked for editing by another process</summary>        
        private static bool CheckFileLock(string filename) {
            try {
                FileStream stream = null;
                FileInfo filedata = new FileInfo(filename);
                if (filedata.Exists == true) {
                    try { stream = filedata.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None); }
                    catch (IOException) { return true; }
                    finally { if (stream != null) { stream.Close(); } }
                }
                return false;
            }
            catch { return true; }
        }

        public static List<string> Ganon = new List<string> {
            "Ganon"
        };

        public static List<string> Beasts = new List<string> {
            "Vah Medoh",
            "Vah Naboris",
            "Vah Rudania",
            "Vah Ruta",
        };

        public static List<string> Stables = new List<string> {
            "Dueling Peaks Stable",
            "Lakeside Stable",
            "Highland Stable",
            "Gerudo Canyon Stable",
            "Wetland Stable",
            "South Akkala Stable",
            "East Akkala Stable",
            "Foothill Stable",
            "Woodland Stable",
            "Outskirt Stable",
            "Riverside Stable",
            "Serenne Stable",
            "Tabantha Stable",
            "Rito Stable",
            "Snowfield Stable",
        };

        public static List<string> Fairies = new List<string> {
            "Great Fairy Cotera",
            "Great Fairy Kaysa",
            "Great Fairy Mija",
            "Great Fairy Tera",
        };

        public static List<string> Towers = new List<string> {
            "Dueling Peaks Tower",
            "Hateno Tower",
            "Lanayru Tower",
            "Lake Tower",
            "Faron Tower",
            "Central Tower",
            "Wasteland Tower",
            "Gerudo Tower",
            "Ridgeland Tower",
            "Woodland Tower",
            "Eldin Tower",
            "Akkala Tower",
            "Tabantha Tower",
            "Hebra Tower",
        };

        public static List<string> RuneShrines = new List<string> {
            "Owa Daim",
            "Keh Namut",
            "Oman Au",
            "Ja Baij",
        };

        public static List<string> Shrines = new List<string> {
            "Bosh Kala",
            "Toto Sah",
            "Shee Vaneer",
            "Ree Dahee",
            "Shee Venath",
            "Ha Dahamar",
            "Ta'loh Naeg",
            "Hila Rao",
            "Lakna Rokee",
            "Chaas Qeta",
            "Myahm Agana",
            "Tahno O'ah",
            "Jitan Sa'mi",
            "Dow Na'eh",
            "Kam Urog",
            "Mezza Lo",
            "Daka Tuss",
            "Kaya Wan",
            "Soh Kofi",
            "Sheh Rata",
            "Rucco Maag",
            "Shai Yota",
            "Dagah Keek",
            "Ne'ez Yohma",
            "Kah Mael",
            "Rona Kachta",
            "Monya Toma",
            "Kuhn Sidajj",
            "Daag Chokah",
            "Keo Ruug",
            "Maag Halan",
            "Ketoh Wawai",
            "Mirro Shaz",
            "Dah Kaso",
            "Rota Ooh",
            "Wahgo Katta",
            "Kaam Ya'tak",
            "Katah Chuki",
            "Noya Neha",
            "Saas Ko'sah",
            "Namika Ozz",
            "Ishto Soh",
            "Shoqa Tatone",
            "Ka'o Makagh",
            "Pumaag Nitae",
            "Ya Naga",
            "Shae Katha",
            "Shai Utoh",
            "Qukah Nata",
            "Shoda Sah",
            "Tawa Jinn",
            "Yah Rin",
            "Kah Yah",
            "Muwo Jeem",
            "Korgu Chideh",
            "Mijah Rokee",
            "Shae Loya",
            "Sheem Dagoze",
            "Mogg Latan",
            "Zalta Wa",
            "Maag No'rah",
            "Toh Yahsa",
            "Sha Warvo",
            "Voo Lota",
            "Akh Va'quot",
            "Bareeda Naag",
            "Tena Ko'sah",
            "Kah Okeo",
            "Hia Miu",
            "To Quomo",
            "Mozo Shenno",
            "Shada Naw",
            "Rok Uwog",
            "Sha Gehma",
            "Qaza Tokki",
            "Goma Asaagh",
            "Maka Rah",
            "Dunba Taag",
            "Lanno Kooh",
            "Gee Ha'rah",
            "Rin Oyaa",
            "Hawa Koth",
            "Kema Zoos",
            "Tho Kayu",
            "Raqa Zunzo",
            "Misae Suma",
            "Dila Maag",
            "Korsh O'hu",
            "Kay Noh",
            "Dako Tah",
            "Suma Sahma",
            "Jee Noh",
            "Daqo Chisay",
            "Keeha Yoog",
            "Kuh Takkar",
            "Kema Kosassa",
            "Sasa Kai",
            "Joloo Nah",
            "Sho Dantu",
            "Shora Hah",
            "Daqa Koh",
            "Qua Raym",
            "Tah Muhl",
            "Mo'a Keet",
            "Sah Dahaj",
            "Gorae Torr",
            "Kayra Mah",
            "Shae Mo'sah",
            "Zuna Kai",
            "Ze Kasho",
            "Ke'nai Shakah",
            "Ritaag Zumo",
            "Tutsuwa Nima",
            "Tu Ka'loh",
            "Dah Hesho",
            "Katosa Aug"
        };
    }
}

