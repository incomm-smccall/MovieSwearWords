using MovieSwearWords.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Navigation;
using System.Xml.Linq;

namespace MovieSwearWords
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private ObservableCollection<SearchWord> _searchList = new ObservableCollection<SearchWord>();
        private readonly string swearwordDir = @"C:\ProgramData\MovieSwearwords";
        private readonly string movieLogFile = "SwearwordLogs.xml";
        private readonly string settingsFile = "Settings.xml";
        private string swearwordPath = string.Empty;
        private string settingsPath = string.Empty;
        
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        public MainWindow()
        {
            InitializeComponent();
            ContentWindow.Content = new UserControl();
            ControlVisibility(Visibility.Hidden, Visibility.Hidden);
            swearwordPath = Path.Combine(swearwordDir, movieLogFile);
            settingsPath = Path.Combine(swearwordDir, settingsFile);
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void BtnSearchSubtitle_Click (object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string filepath = openFileDialog.FileName;
                TxtSubtitle.Text = filepath;
                string ext = filepath.Substring(filepath.Length - 3, 3);
                if (ext == "zip")
                    Unzip(filepath);
                if (ext == "srt")
                    GetWorkCount(filepath);
            }
        }

        private void BtnSettings_Click (object sender, RoutedEventArgs e)
        {
            ControlVisibility(Visibility.Visible, Visibility.Hidden);
            ContentWindow.Content = new SettingsView(settingsPath);
            BtnClose.Visibility = Visibility.Visible;
        }

        private void BtnClose_Click (object sender, RoutedEventArgs e)
        {
            ContentWindow.Content = new UserControl();
            ControlVisibility(Visibility.Hidden, Visibility.Hidden);
            BtnClose.Visibility = Visibility.Hidden;
        }

        private void BtnLogs_Click (object sender, RoutedEventArgs e)
        {
            //ContentWindow.Content = new SwearlogsView();
            ObservableCollection<string> wordLogList = new ObservableCollection<string>();
            if (File.Exists(Path.Combine(swearwordDir, movieLogFile)))
            {
                string logfilePath = Path.Combine(swearwordDir, movieLogFile);
                XDocument doc = XDocument.Load(logfilePath);
                foreach (var node in doc.Descendants("FileLog"))
                {
                    wordLogList.Add(node.Element("FileName").Value);
                }
                WordLogs.ItemsSource = wordLogList;
            }
            ControlVisibility(Visibility.Hidden, Visibility.Visible);
            BtnClose.Visibility = Visibility.Visible;
        }

        private void BtnViewLog_Click(object sender, RoutedEventArgs e)
        {
            if (WordLogs.SelectedItem != null)
            {
                string logname = WordLogs.SelectedItem.ToString();
                if (File.Exists(Path.Combine(swearwordDir, movieLogFile)))
                {
                    string logfilePath = Path.Combine(swearwordDir, movieLogFile);
                    XDocument doc = XDocument.Load(logfilePath);
                    XElement log = (from xlog in doc.Descendants("FileLog")
                                    where xlog.Element("FileName").Value == logname
                                    select xlog).FirstOrDefault();

                    TxtSubtitle.Text = log.Element("FileName").Value;
                    _searchList.Clear();
                    foreach (var item in log.Descendants("Swearword"))
                    {
                        _searchList.Add(new SearchWord()
                        {
                            Displayword = item.Element("Displayword").Value,
                            WordCount = int.Parse(item.Element("WordCount").Value)
                        });
                    }
                    SearchListControl.ItemsSource = _searchList;
                }
            }
        }

        private void Unzip(string path)
        {
            string dirpath = Path.GetDirectoryName(path);
            string foldname = Path.GetFileNameWithoutExtension(path);
            string fullpath = Path.Combine(dirpath, foldname);
            ZipFile.ExtractToDirectory(path, fullpath);

            string filepath = Directory.EnumerateFiles(fullpath, "*.srt", SearchOption.TopDirectoryOnly).FirstOrDefault();
            GetWorkCount(filepath);
        }

        private void GetWorkCount(string filepath)
        {
            _searchList.Clear();
            BuildSearchWordList();
            foreach (var line in File.ReadAllLines(filepath))
            {
                foreach (SearchWord word in _searchList)
                {
                    word.WordCount += Regex.Matches(line, word.Keyword, RegexOptions.IgnoreCase).Count;
                }
            }
            SearchListControl.ItemsSource = _searchList;
            AddFileToLogs(Path.GetFileName(filepath));
        }

        private void BuildSearchWordList()
        {
            if (File.Exists(settingsPath))
            {
                //List<SearchWord> searchList = new List<SearchWord>();
                XDocument doc = XDocument.Load(settingsPath);
                foreach (var node in doc.Descendants("Swearword"))
                {
                    if (node.Element("IsActive").Value.ToLower() == "true")
                    {
                        _searchList.Add(new SearchWord()
                        {
                            ID = int.Parse(node.Element("ID").Value),
                            Displayword = $"{node.Element("Displayword").Value} word count",
                            Keyword = node.Element("Searchword").Value,
                            IsWordActive = (node.Element("IsActive").Value == "true") ? true : false
                        });
                    }
                    //_searchList.Add(word);
                }
            }
        }

        private void AddFileToLogs(string filename)
        {
            if (!File.Exists(swearwordPath))
            {
                File.Create(swearwordPath).Close();
                XDocument xdoc = new XDocument(
                    new XElement("FileLogs")
                );
                xdoc.Save(swearwordPath);
            }

            XDocument doc = XDocument.Load(swearwordPath);
            if (!(from xlog in doc.Descendants("FileLog")
                 where xlog.Element("FileName").Value == filename
                 select xlog).Any())
            {
                int count = doc.Descendants("FileLog").Count() + 1;

                XElement newNode = new XElement("FileLog",
                    new XElement("FileName", filename),
                    new XElement("FileLogID", count.ToString()),
                    new XElement("ReadDate", DateTime.Now.ToString("MM/dd/yyyy hh:mm tt")),
                    new XElement("Swearwords",
                        _searchList.SelectMany(x => new[] { new XElement("Swearword", new[] { new XElement("Displayword", x.Displayword), new XElement("WordCount", x.WordCount) }) })
                    )
                );
                doc.Element("FileLogs").Add(newNode);
                doc.Save(swearwordPath);
            }
        }

        private void ControlVisibility(Visibility content, Visibility settings)
        {
            ContentWindow.Visibility = content;
            WordHistory.Visibility = settings;
            //WordLogs.Visibility = settings;
        }

        //private List<SearchWord> XXXBuildSearchWordList()
        //{
        //    return new List<SearchWord>
        //    {
        //        new SearchWord { Displayword = "F* word count:", Keyword = "fuck", WordCount = 0 },
        //        new SearchWord { Displayword = "Shit word count:", Keyword = "shit", WordCount = 0 },
        //        new SearchWord { Displayword = "Damn word count:", Keyword = "damn", WordCount = 0 },
        //        new SearchWord { Displayword = "Hell word count:", Keyword = "hell", WordCount = 0 },
        //        new SearchWord { Displayword = "Piss word count:", Keyword = "piss", WordCount = 0 },
        //        new SearchWord { Displayword = "Bitch word count:", Keyword = "bitch", WordCount = 0 },
        //        new SearchWord { Displayword = "Cock word count:", Keyword = "cock", WordCount = 0 },
        //        new SearchWord { Displayword = "Pussy word count:", Keyword = "pussy", WordCount = 0 },
        //        new SearchWord { Displayword = "Cunt word count:", Keyword = "cunt", WordCount = 0 }
        //    };
        //}
    }

    //public class SearchWord
    //{
    //    public string Displayword { get; set; }
    //    public string Keyword { get; set; }
    //    public int WordCount { get; set; }
    //}
}
