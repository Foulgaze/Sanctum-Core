using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core.Networking
{


    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Newtonsoft.Json;
    public class GenericDictionary
    {
        private Dictionary<string, object> _dict = new Dictionary<string, object>();

        public void Add<T>(string key, T value)
        {
            _dict.Add(key, value);
        }

        public T GetValue<T>(string key) where T : class
        {
            return _dict[key] as T;
        }
    }

    public class NetworkAttributeFactory
    {
        public event PropertyChangedEventHandler attributeValueChanged = delegate { };
        public GenericDictionary networkAttributes = new GenericDictionary();

        private int _id = 0;

        private void AttributeChangedEventHandler(object sender, PropertyChangedEventArgs e)
        {
            attributeValueChanged(sender, e);
        }

        private void HandleNetworkedAttribute(object sender, PropertyChangedEventArgs e)
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
            NetworkAttribute<object> attribute = networkAttributes.GetValue<NetworkAttribute<object>>(ID);
            if (attribute == null)
            {
                // Log Error: Attribute with given ID not found
                return;
            }

            object deserializedValue = JsonConvert.DeserializeObject(serializedNewValue, attribute.Value.GetType());

            attribute.NonNetworkedSet(deserializedValue);


        }
        public NetworkAttribute<T> AddNetworkAttribute<T>(string uuid, T value)
        {
            string newID = $"{uuid}-{_id++}";
            NetworkAttribute<T> newAttribute = new NetworkAttribute<T>(newID, value);
            networkAttributes.Add<T>(newID, value);
            newAttribute.networkValueChange += AttributeChangedEventHandler;
            return newAttribute;
        }

    }
}
