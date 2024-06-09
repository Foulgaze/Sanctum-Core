using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sanctum_Core
{
    public class NetworkAttributeFactory
    {
        public event PropertyChangedEventHandler attributeValueChanged = delegate { };
        public Dictionary<string, NetworkAttribute> networkAttributes = new();

        private int _id = 0;

        private void AttributeChangedEventHandler(object sender, PropertyChangedEventArgs e)
        {
            attributeValueChanged(sender, e);
        }

        public void HandleNetworkedAttribute(object sender, PropertyChangedEventArgs e)
        {
            string instruction = (string)sender;
            string[] splitInstruction = instruction.Split("|");
            if (splitInstruction.Length != 2)
            {
                // Log Error;
                return;
            }
            string ID = splitInstruction[0];
            string serializedNewValue = splitInstruction[1];
            NetworkAttribute attribute = this.networkAttributes[ID];
            if (attribute == null)
            {
                // Log Error: Attribute with given ID not found
                return;
            }

            object deserializedValue = JsonConvert.DeserializeObject(serializedNewValue, attribute.ValueType);

            attribute.NonNetworkedSet(deserializedValue);
        }


        public NetworkAttribute<T> AddNetworkAttribute<T>(string uuid, T value)
        {
            string newID = $"{uuid}-{this._id++}";
            NetworkAttribute<T> newAttribute = new(newID, value);
            this.networkAttributes.Add(newID, newAttribute);
            newAttribute.networkValueChange += this.AttributeChangedEventHandler;
            return newAttribute;
        }
    }
}
