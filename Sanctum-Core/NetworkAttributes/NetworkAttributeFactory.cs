using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sanctum_Core
{
    public class NetworkAttributeFactory
    {
        public event PropertyChangedEventHandler attributeValueChanged = delegate { };
        public Dictionary<string, NetworkAttribute> networkAttributes = new();


        private void AttributeChangedEventHandler(object? sender, PropertyChangedEventArgs e)
        {
            attributeValueChanged(sender, e);
        }

        /// <summary>
        /// Processes a networked attribute change by deserializing the new value and updating the attribute.
        /// </summary>
        /// <param name="sender">The instruction containing the attribute ID and the serialized new value.</param>
        /// <param name="e">The event data containing information about the property change.</param>
        public void HandleNetworkedAttribute(object? sender, PropertyChangedEventArgs e)
        {
            if(sender == null)
            {
                return;
            }
            string instruction = (string)sender;
            string[] splitInstruction = instruction.Split("|");
            if (splitInstruction.Length != 2)
            {
                // Log Error;
                return;
            }
            string id = splitInstruction[0];
            string serializedNewValue = splitInstruction[1];
            NetworkAttribute? attribute = this.networkAttributes.GetValueOrDefault(id);
            if (attribute == null)
            {
                // Log Error: Attribute with given ID not found
                return;
            }

            try
            {
                object? deserializedValue = JsonConvert.DeserializeObject(serializedNewValue, attribute.ValueType);
                if( !attribute.outsideSettable || deserializedValue is null)
                {
                    return;
                }
                attribute.SetValue(deserializedValue);
            }
            catch
            {
                // Bad NetworkAttribute, skip
            }

            // Network this. 
        }

        public void ClearNetworkAttributes()
        {
            foreach (NetworkAttribute attribute in this.networkAttributes.Values)
            {
                attribute.ClearListeners();
            }
            this.networkAttributes.Clear();
        }

        /// <summary>
        /// Adds a new network attribute with the specified ID and value, and optionally configures its settings.
        /// </summary>
        /// <typeparam name="T">The type of the value held by the attribute.</typeparam>
        /// <param name="id">The unique identifier for the network attribute.</param>
        /// <param name="value">The initial value of the network attribute.</param>
        /// <param name="outsideSettable">Indicates whether the attribute can be set from outside (default is true).</param>
        /// <param name="networkChange">Indicates whether changes to the attribute should be networked (default is true).</param>
        /// <returns>The created <see cref="NetworkAttribute{T}"/> instance.</returns>
        /// <exception cref="Exception">Thrown if the attribute ID is already present in the dictionary.</exception>
        public NetworkAttribute<T> AddNetworkAttribute<T>(string id, T value, bool outsideSettable = true, bool networkChange = true)
        {
            if(this.networkAttributes.ContainsKey(id))
            {
                throw new Exception($"{id} already present in dictionary");
            }
            NetworkAttribute<T> newAttribute = new(id, value);
            this.networkAttributes.Add(id, newAttribute);
            if(networkChange)
            {
                newAttribute.valueChange += this.AttributeChangedEventHandler;
            }
            newAttribute.outsideSettable = outsideSettable;
            return newAttribute;
        }
    }
}
