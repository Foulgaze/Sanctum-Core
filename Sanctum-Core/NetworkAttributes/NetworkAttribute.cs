using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Sanctum_Core
{
    public abstract class NetworkAttribute
    {
        public string Id { get; }
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
        public abstract void NonNetworkedSet(object value);


        public abstract string SerializedValue { get; }
    }

    public class NetworkAttribute<T> : NetworkAttribute
    {
        public event Action<NetworkAttribute> valueChanged = delegate { };
        public event Action<NetworkAttribute> nonNetworkChange = delegate { };

        private T value;
        private string serializedValue;
        private bool isSerializedValueDirty = true;

        public T Value
        {
            get => this.value;
            protected set
            {
                if (EqualityComparer<T>.Default.Equals(this.value, value))
                {
                    return;   
                }
                this.value = value;
                this.isSerializedValueDirty = true;
                valueChanged(this);
            }
        }

        /// <summary>
        /// Sets the value of the attribute.
        /// </summary>
        /// <param name="value">The value to set.</param>
        public override void SetValue(object value)
        {
            this.Value = (T)value;
        }

        public override void NonNetworkedSet(object value)
        {
            this.value = (T)value;
            this.isSerializedValueDirty = true;
            nonNetworkChange(this);
        }

        /// <summary>
        /// Clears all listeners on the networkattribute
        /// </summary>
        public override void ClearListeners()
        {
            foreach (Delegate d in valueChanged.GetInvocationList())
            {
                valueChanged -= (Action<NetworkAttribute>)d;
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
            this.value = value;
            this.isSerializedValueDirty = true;
        }

        /// <summary>
        /// Gets the serialized value of the attribute.
        /// </summary>
        public override string SerializedValue
        {
            get
            {
                if (this.isSerializedValueDirty)
                {
                    this.serializedValue = JsonConvert.SerializeObject(this.value);
                    this.isSerializedValueDirty = false;
                }
                return this.serializedValue;
            }
        }
    }
}