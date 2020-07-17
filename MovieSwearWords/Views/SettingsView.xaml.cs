using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
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
using System.Xml.Linq;

namespace MovieSwearWords.Views
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        private ObservableCollection<SearchWord> _searchList = new ObservableCollection<SearchWord>();
        private readonly string _settingsPath;

        public SettingsView(string settingsPath)
        {
            InitializeComponent();
            _settingsPath = settingsPath;
            BuildSearchList();
            WordSettings.ItemsSource = _searchList;
        }

        private void BuildSearchList()
        {
            _searchList.Clear();
            if (File.Exists(_settingsPath))
            {
                XDocument doc = XDocument.Load(_settingsPath);
                foreach (var node in doc.Descendants("Swearword"))
                {
                    _searchList.Add(new SearchWord()
                    {
                        ID = int.Parse(node.Element("ID").Value),
                        Displayword = node.Element("Displayword").Value,
                        Keyword = node.Element("Searchword").Value,
                        IsWordActive = (node.Element("IsActive").Value.ToLower() == "true") ? true : false
                    }); ;
                }
            }
        }

        private void BtnSave_Click (object sender, RoutedEventArgs e)
        {
            if (File.Exists(_settingsPath))
            {
                XDocument doc = XDocument.Load(_settingsPath);
                if (lblSelectId.Content != null)
                {
                    XElement searchWord = (from xSearch in doc.Descendants("Swearword")
                                           where xSearch.Element("ID").Value == lblSelectId.Content.ToString()
                                           select xSearch).FirstOrDefault();

                    searchWord.Element("Displayword").Value = TxtEditDisplay.Text;
                    searchWord.Element("Searchword").Value = TxtEditSearch.Text;
                    searchWord.Element("IsActive").Value = ChkIsActive.IsChecked.ToString();
                }
                else
                {
                    int count = doc.Descendants("Swearword").Count() + 1;

                    XElement newNode = new XElement("Swearword",
                        new XElement ("ID", count.ToString()),
                        new XElement ("Displayword", TxtEditDisplay.Text),
                        new XElement ("Searchword", TxtEditSearch.Text),
                        new XElement ("WordCount", string.Empty),
                        new XElement ("IsActive", ChkIsActive.IsChecked.ToString())
                    );
                    doc.Element("Swearwords").Add(newNode);
                }

                doc.Save(_settingsPath);

                TxtEditDisplay.Text = string.Empty;
                TxtEditSearch.Text = string.Empty;
                ChkIsActive.IsChecked = false;
                lblSelectId.Content = null;

                BuildSearchList();
                WordSettings.ItemsSource = _searchList;
            }
        }

        private void BtnEditWord_Click (object sender, RoutedEventArgs e)
        {
            if (WordSettings.SelectedValue != null)
            {
                SearchWord selectedWord = (SearchWord)WordSettings.SelectedItem;
                TxtEditDisplay.Text = selectedWord.Displayword;
                TxtEditSearch.Text = selectedWord.Keyword;
                ChkIsActive.IsChecked = selectedWord.IsWordActive;
                lblSelectId.Content = selectedWord.ID;
            }
        }
    }
}
