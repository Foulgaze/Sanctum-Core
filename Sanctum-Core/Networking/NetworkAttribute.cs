using Newtonsoft.Json;
using System.ComponentModel;

namespace Sanctum_Core
{
    public abstract class NetworkAttribute
    {
        public string Id { get; }
        public bool IsReadOnly { get; set; } = false;
        public abstract Type ValueType { get; }

        protected NetworkAttribute(string id)
        {
            this.Id = id;
        }

        public abstract void NonNetworkedSet(object value);
    }

    public class NetworkAttribute<T> : NetworkAttribute
    {
        public event PropertyChangedEventHandler networkValueChange = delegate { };
        public event PropertyChangedEventHandler valueChange = delegate { };

        private T value;
        public bool changeHasBeenNetworked;

        public T Value
        {
            get => this.value;
            set
            {
                if(this.IsReadOnly)
                {
                    return;
                }
                this.value = value;
                this.changeHasBeenNetworked = true;
                this.networkValueChange(this.Id, new PropertyChangedEventArgs(JsonConvert.SerializeObject(value)));
            }
        }

        public override Type ValueType => typeof(T);

        public NetworkAttribute(string id, T value) : base(id)
        {
            this.value = value;
            this.changeHasBeenNetworked = false;
        }

        public override void NonNetworkedSet(object value)
        {
            this.value = (T)value;
            this.changeHasBeenNetworked = false;
            this.valueChange(value, new PropertyChangedEventArgs("NonNetworkChange"));
        }
    }
}
