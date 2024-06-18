using Newtonsoft.Json;
using System.ComponentModel;

namespace Sanctum_Core
{
    public abstract class NetworkAttribute
    {
        public string Id { get; }
        public abstract Type ValueType { get; }

        protected NetworkAttribute(string id)
        {
            this.Id = id;
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
            this.valueChange(this.Id, new PropertyChangedEventArgs(JsonConvert.SerializeObject(value)));
        }

        public override Type ValueType => typeof(T);

        public NetworkAttribute(string id, T value) : base(id)
        {
            this.Value = value;
        }
    }
}
