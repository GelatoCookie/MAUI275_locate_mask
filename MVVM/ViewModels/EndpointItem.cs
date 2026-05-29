using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
namespace MauiRfidSample.MVVM.ViewModels
{
    using System;
    using System.ComponentModel;

    public class EndpointItem : INotifyPropertyChanged, IEquatable<EndpointItem>
    {
        private bool _isChecked;
        public string EPName { get; set; }

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    OnPropertyChanged(nameof(IsChecked));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Implement the IEquatable interface
        public bool Equals(EndpointItem other)
        {
            if (other == null) return false;
            return EPName == other.EPName; // Assuming Name is unique for each EndpointItem
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            return Equals((EndpointItem)obj);
        }

        public override int GetHashCode()
        {
            return EPName?.GetHashCode() ?? 0;
        }
    }

}
