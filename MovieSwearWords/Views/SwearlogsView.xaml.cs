using System;
using System.Collections.Generic;
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
    /// Interaction logic for SwearlogsView.xaml
    /// </summary>
    public partial class SwearlogsView : UserControl
    {
        public SwearlogsView()
        {
            InitializeComponent();
            BuildFileLogList();
        }

        private void BtnViewLog_Click(object sender, RoutedEventArgs e)
        {
            if (WordLogs.SelectedItem != null)
            {
                string selectFilename = WordLogs.SelectedItem.ToString();
                
            }
        }

        private void BuildFileLogList()
        {
            List<string> loglist = new List<string>();
            XDocument doc = XDocument.Load("SwearwordLogs.xml");
            foreach (var node in doc.Descendants("FileLog"))
            {
                loglist.Add(node.Element("FileName").Value);
            }

            WordLogs.ItemsSource = loglist;
        }
    }
}
