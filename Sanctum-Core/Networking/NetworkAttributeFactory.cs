using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sanctum_Core
{
    public static class NetworkAttributeFactory
    {
        public static event PropertyChangedEventHandler attributeValueChanged = delegate { };
        public static Dictionary<string, NetworkAttribute> networkAttributes = new();


        private static void AttributeChangedEventHandler(object sender, PropertyChangedEventArgs e)
        {
            attributeValueChanged(sender, e);
        }

        public static void HandleNetworkedAttribute(object sender, PropertyChangedEventArgs e)
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
            NetworkAttribute attribute = networkAttributes[ID];
            if (attribute == null)
            {
                // Log Error: Attribute with given ID not found
                return;
            }

            try
            {
                object deserializedValue = JsonConvert.DeserializeObject(serializedNewValue, attribute.ValueType);
                attribute.SetValue(deserializedValue);
            }
            catch
            {
                // Bad NetworkAttribute, skip
            }

            // Network this. 
        }


        public static NetworkAttribute<T> AddNetworkAttribute<T>(string id, T value)
        {
            if(networkAttributes.ContainsKey(id))
            {
                throw new Exception($"{id} already present in dictionary");
            }
            NetworkAttribute<T> newAttribute = new(id, value);
            networkAttributes.Add(id, newAttribute);
            newAttribute.valueChange += AttributeChangedEventHandler;
            return newAttribute;
        }
    }
}
