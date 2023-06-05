using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Captain.Library.Common
{
    public class ModuleItem : INotifyPropertyChanged
    {
        public string Title { get; set; }
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                _isExpanded = value;
                NotifyPropertyChanged("IsExpanded");
            }
        }

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (_isChecked != value)
                {
                    Set_Children_Check(value);
                    Set_Parent_Check(value);
                }
            }
        }
        public ImageSource IconPath
        {
            get { return _bitmapImg; }
            set { _bitmapImg = value; NotifyPropertyChanged("IconPath"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private ModuleItem _parentNode;
        private ObservableCollection<ModuleItem> _children = new ObservableCollection<ModuleItem>();
        private ImageSource _bitmapImg;
        private bool _isExpanded;
        private bool _isChecked;

        public ModuleItem ParentNode
        {
            get { return _parentNode; }
            private set { _parentNode = value; }
        }
        public ObservableCollection<ModuleItem> Children
        {
            get { return _children; }
            set { _children = value; }
        }

        //public NewMetadata Data;
        public ModuleItem(ModuleItem parentNode)
        {
            ParentNode = parentNode;
        }

        public bool IsLeaf
        {
            get
            {
                return Children.Count == 0;
            }
        }
        #region Private Properties And Methods
        private void Set_Children_Check(bool value)
        {
            this._isChecked = value;
            //if (Layer is IFeatureLayer)
            //{
            //    (Layer as IFeatureLayer).Selectable = value;
            //}
            NotifyPropertyChanged("IsChecked");
            foreach (ModuleItem c in Children)
                c.Set_Children_Check(value);
        }

        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        private void Set_Parent_Check(bool value)
        {
            if (_parentNode != null)
            {
                List<ModuleItem> _lst_else = _parentNode.Children.Where(t => t.IsChecked == true).ToList();
                int checkCount = _lst_else.Count();
                if (checkCount == _parentNode.Children.Count && value == true)
                {
                    _parentNode.IsChecked = value;
                    _parentNode.Set_Parent_Check(value);
                }
                else if (checkCount < _parentNode.Children.Count && value == false && checkCount > 0)
                {
                    _parentNode.IsChecked = value;
                    _parentNode.Set_Parent_Check(value);
                    _lst_else.ForEach((t) => { t.IsChecked = true; });
                }
            }
        }
        #endregion
    }
}
