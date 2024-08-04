using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        protected NetworkAttribute(string id)
        {
            this.Id = id;
        }
        public abstract void SetValue(object value);
        public abstract void ClearListeners();
    }

    public class NetworkAttribute<T> : NetworkAttribute
    {
        public event PropertyChangedEventHandler valueChange = delegate { };


        public T Value { get; protected set; }


        public override void SetValue(object value)
        { 
            if(EqualityComparer<T>.Default.Equals((T)value, this.Value))
            {
                return;
            }
            this.Value = (T)value;
            
            this.valueChange(this.Id, new PropertyChangedEventArgs(JsonConvert.SerializeObject(value)));
        }

        public override void ClearListeners()
        {
            foreach (Delegate d in valueChange.GetInvocationList())
            {
                valueChange -= (PropertyChangedEventHandler)d;
            }
        }

        public override Type ValueType => typeof(T);

        public NetworkAttribute(string id, T value) : base(id)
        {
            this.Value = value;
        }
    }
}
