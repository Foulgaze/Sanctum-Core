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

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkAttribute"/> class with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the network attribute.</param>
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

        /// <summary>
        /// Sets the value of the attribute.
        /// </summary>
        /// <param name="value">The value to set.</param>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkAttribute{T}"/> class with the specified identifier and value.
        /// </summary>
        /// <param name="id">The unique identifier of the network attribute.</param>
        /// <param name="value">The initial value of the network attribute.</param>
        public NetworkAttribute(string id, T value) : base(id)
        {
            this.Value = value;
        }
    }
}
