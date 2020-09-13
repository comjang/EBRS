using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EBRS
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        string directory = @"C:\Users\JANG\source\repos\EBRS\EBRS\bin\Debug";
        string n3Header = "ebrs";

        string eyeCmd = @"C:\Program Files\eye\bin\eye.cmd";
        string queryAllName = "query-all.n3";

        string inputFileName = "input.n3";
        string outputName = "output.txt";
        string ruleName = "rule.n3";
        string queryPreset = ""; // "{?a ?b ?c} => {?a ?b ?c}.";
        string homeLink = "http://dbpedia.org/page/Semantic_Web";

        public MainWindow()
        {
            InitializeComponent();

            directory = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            loadInputFiles();
            loadRuleFiles();
            textBox_ask.Text = queryPreset;

            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }

        private void loadInputFiles()
        {
            DirectoryInfo d;
            FileInfo[] Files;
            string filetext;
            string[] checkedList;

            d = new DirectoryInfo(directory + @"\input\");
            Files = d.GetFiles("*.n3");
            filetext = System.IO.File.ReadAllText(directory + @"\setting\input.setting");
            checkedList = Regex.Split(filetext, "\r\n");

            stackPanel_inputFiles.Children.Clear();
            foreach (FileInfo file in Files)
            {
                string n3Content = System.IO.File.ReadAllText(directory + @"\input\" + file.Name);
                addItemToList(checkedList, n3Content, file.Name, stackPanel_inputFiles, @"http://dbpedia.org/page/");
            }
        }

        private void loadRuleFiles()
        {
            DirectoryInfo d;
            FileInfo[] Files;
            string filetext;
            string[] checkedList;

            d = new DirectoryInfo(directory + @"\rule\ontology\");
            Files = d.GetFiles("*.n3");
            filetext = System.IO.File.ReadAllText(directory + @"\setting\ontology.setting");
            checkedList = Regex.Split(filetext, "\r\n");

            stackPanel_ontologyFiles.Children.Clear();
            foreach (FileInfo file in Files)
            {
                string n3Content = System.IO.File.ReadAllText(directory + @"\rule\ontology\" + file.Name);
                addItemToList(checkedList, n3Content, file.Name, stackPanel_ontologyFiles);
            }

            d = new DirectoryInfo(directory + @"\rule\theory\");
            Files = d.GetFiles("*.n3");
            filetext = System.IO.File.ReadAllText(directory + @"\setting\theory.setting");
            checkedList = Regex.Split(filetext, "\r\n");

            stackPanel_theoryFiles.Children.Clear();
            foreach (FileInfo file in Files)
            {
                string n3Content = System.IO.File.ReadAllText(directory + @"\rule\theory\" + file.Name);
                addItemToList(checkedList, n3Content, file.Name, stackPanel_theoryFiles);
            }
        }

        private void addItemToList(string[] checkedList, string n3Content, string name, StackPanel stackPanel, string link = "")
        {
            WrapPanel wrapPanel = new WrapPanel();
            wrapPanel.VerticalAlignment = VerticalAlignment.Center;

            CheckBox checkBox = new CheckBox();
            checkBox.Margin = new Thickness(0, 8, 0, 0);
            checkBox.Tag = name;
            if (checkedList.Contains(name))
                checkBox.IsChecked = true;

            Label label = new Label();
            label.Content = name;
            label.Tag = n3Content;
            if (link == "")
                label.MouseUp += (ss, ee) =>
                {
                    string content = label.Tag.ToString();
                    textBlock_rules.Text = content;
                };
            else
                label.MouseUp += (ss, ee) =>
                {
                    try
                    {
                        string[] lines = name.Split('.');
                        string hyperlink = link + lines[0];
                        SubWindow subWindow = new SubWindow();
                        subWindow.Show();
                        subWindow.webBrowser_.Navigate(hyperlink);
                        subWindow.Closed += SubWindow_Closed;
                    }
                    catch (Exception ex) { }
                };

            wrapPanel.Children.Add(checkBox);
            wrapPanel.Children.Add(label);

            stackPanel.Children.Add(wrapPanel);
        }

        private void SubWindow_Closed(object sender, EventArgs e)
        {
            loadInputFiles();
        }

        private string savefileList(StackPanel stackPanel, string filename)
        {
            string text = "";
            foreach (object obj in stackPanel.Children)
            {
                WrapPanel wp = (WrapPanel)obj;
                CheckBox cb = (CheckBox)wp.Children[0];
                if (cb.IsChecked == false) continue;
                text += cb.Tag.ToString();
                text += "\r\n";
            }
            System.IO.File.WriteAllText(directory + @"\setting\" + filename + ".setting", text, Encoding.UTF8);
            return text;
        }

        private void btn_saveInput_Click(object sender, RoutedEventArgs e)
        {
            savefileList(stackPanel_inputFiles, "input");
        }

        private void btn_saveRules_Click(object sender, RoutedEventArgs e)
        {
            string text = "";
            text += savefileList(stackPanel_ontologyFiles, "ontology") + "\r\n";
            text += savefileList(stackPanel_theoryFiles, "theory");
            textBlock_rules.Text = text;
        }

        bool processDone = true;
        private void run()
        {
            if (!processDone) return;
            processDone = false;

            // save query
            System.IO.File.WriteAllText(directory + @"\" + queryAllName, textBox_ask.Text, Encoding.UTF8);

            string list = "";
            StackPanel stackPanel = stackPanel_inputFiles;
            foreach (object obj in stackPanel.Children)
            {
                WrapPanel wp = (WrapPanel)obj;
                CheckBox cb = (CheckBox)wp.Children[0];
                if (cb.IsChecked == false) continue;
                list += "\"input/" + cb.Tag.ToString() + "\" ";
            }
            stackPanel = stackPanel_ontologyFiles;
            foreach (object obj in stackPanel.Children)
            {
                WrapPanel wp = (WrapPanel)obj;
                CheckBox cb = (CheckBox)wp.Children[0];
                if (cb.IsChecked == false) continue;
                list += "\"rule/ontology/" + cb.Tag.ToString() + "\" ";
            }
            stackPanel = stackPanel_theoryFiles;
            foreach (object obj in stackPanel.Children)
            {
                WrapPanel wp = (WrapPanel)obj;
                CheckBox cb = (CheckBox)wp.Children[0];
                if (cb.IsChecked == false) continue;
                list += "\"rule/theory/" + cb.Tag.ToString() + "\" ";
            }

            try
            {
                Process process = new Process();
                process.StartInfo.FileName = eyeCmd;
                process.StartInfo.WorkingDirectory = directory; // @"C:\Users\JANG\Desktop\test";
                // process.StartInfo.Arguments = list + " --query " + queryAllName + " --nope --n3p > " + outputName;
                process.StartInfo.Arguments = list + " --query " + queryAllName + " --nope > " + outputName;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardError = true;
                process.Start();

                string fullError = errorFullText = "";
                while (!process.StandardError.EndOfStream)
                {
                    string line = process.StandardError.ReadLine();
                    fullError += line + "\r\n";
                }
                errorFullText = fullError;
                if (errorFullText.Contains("** ERROR **"))
                    textBox_result.Text = errorFullText = fullError;
            }
            catch (Exception exc) { }
            finally
            {
                processDone = true;
            }
        }

        bool isWatcherOn = false;
        private void CreateFileWatcher(string path)
        {
            if (isWatcherOn) return;
            isWatcherOn = true;

            // Create a new FileSystemWatcher and set its properties.
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = path;
            /* Watch for changes in LastAccess and LastWrite times, and 
               the renaming of files or directories. */
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
               | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            // Only watch text files.
            watcher.Filter = outputName;

            // Add event handlers.
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.Deleted += new FileSystemEventHandler(OnChanged);

            // Begin watching.
            watcher.EnableRaisingEvents = true;
        }

        // Define the event handlers.
        static string[] outputLines = { };
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            try
            {
                // Specify what is done when a file is changed, created, or deleted.
                Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);

                outputFullText = File.ReadAllText(e.FullPath, Encoding.UTF8);
                hasToDoUpdate = true;
            }
            catch (Exception exc)
            {
            }
            finally
            {
                processDone = true;
            }
        }

        private void btn_reason_Click(object sender, RoutedEventArgs e)
        {
            CreateFileWatcher(directory);
            run();
        }

        string outputFullText = "";
        string errorFullText = "";
        bool hasToDoUpdate = false;
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (hasToDoUpdate) {
                    if (errorFullText.Contains("** ERROR **"))
                        return;
                    else
                        textBox_result.Text = outputFullText;
                    hasToDoUpdate = false;
                }
            }
            catch (Exception exc) { }
        }

        private void btn_openBrowser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string hyperlink = homeLink;
                SubWindow subWindow = new SubWindow();
                subWindow.Show();
                subWindow.webBrowser_.Navigate(hyperlink);
                subWindow.Closed += SubWindow_Closed;
            }
            catch (Exception ex) { }
        }
    }
}
