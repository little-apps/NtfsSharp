using System.ComponentModel;
using System.Runtime.CompilerServices;
using DiskUsage.Annotations;

namespace DiskUsage
{
    public class Usage : INotifyPropertyChanged
    {
        private string _key = "";
        private long _value = 0;

        public string Key
        {
            get { return _key; }
            set
            {
                _key = value;
                OnPropertyChanged(nameof(Key));
            }
        }

        public long Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged(nameof(Value));
            }
        }

        public Usage(string name)
        {
            Key = name;
            Value = 1;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
