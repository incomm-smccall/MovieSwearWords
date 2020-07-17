using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieSwearWords
{
    public class SearchWord : INotifyPropertyChanged
    {
        private int _id;
        public int ID
        {
            get => _id;
            set
            {
                if (_id == value) return;
                _id = value;
                this.NotifyPropertyChanged("ID");
            }
        }

        private string _displayword;
        public string Displayword
        {
            get => _displayword;
            set
            {
                if (_displayword == value) return;
                _displayword = value;
                this.NotifyPropertyChanged("Displayword");
            }
        }

        private string _keyword;
        public string Keyword
        {
            get => _keyword;
            set
            {
                if (_keyword == value) return;
                _keyword = value;
                this.NotifyPropertyChanged("Keyword");
            }
        }

        private int _wordCount;
        public int WordCount
        {
            get => _wordCount;
            set
            {
                if (_wordCount == value) return;
                _wordCount = value;
                this.NotifyPropertyChanged("WordCount");
            }
        }

        private bool _isWordActive;
        public bool IsWordActive
        {
            get => _isWordActive;
            set
            {
                if (_isWordActive == value) return;
                _isWordActive = value;
                this.NotifyPropertyChanged("IsWordActive");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}
