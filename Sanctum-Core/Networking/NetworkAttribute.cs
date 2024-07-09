using Newtonsoft.Json;
using System.ComponentModel;
using System.Net.NetworkInformation;

namespace Sanctum_Core
{
    public abstract class NetworkAttribute
    {
        public string Id { get; }
        protected readonly bool networkChange;
        public abstract Type ValueType { get; }

        public bool outsideSettable { get; set; }

        protected NetworkAttribute(string id, bool networkChange)
        {
            this.Id = id;
            this.networkChange = networkChange;
        }
        public abstract void SetValue(object value);
    }

    public class NetworkAttribute<T> : NetworkAttribute
    {
        public event PropertyChangedEventHandler valueChange = delegate { };

        public T Value { get; protected set; }

        public override void SetValue(object value)
        { 
            this.Value = (T)value;
            if(this.networkChange)
            {
                this.valueChange(this.Id, new PropertyChangedEventArgs(JsonConvert.SerializeObject(value)));
            }
        }

        public override Type ValueType => typeof(T);

        public NetworkAttribute(string id, T value, bool networkChange = true) : base(id, networkChange)
        {
            this.Value = value;
        }
    }
}
