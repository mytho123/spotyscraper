using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SpotyScraper.Model.Utils
{
    public class PropertyChangeNotifier : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void Set<TProperty>(ref TProperty _backingField, TProperty value, [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(_backingField, value))
                return;

            _backingField = value;
            this.RaisePropertyChanged(propertyName);
        }
    }
}