﻿using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sanctum_Core
{
    public class NetworkAttributeFactory
    {
        public event PropertyChangedEventHandler attributeValueChanged = delegate { };
        public Dictionary<string, NetworkAttribute> networkAttributes = new();


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
            string id = splitInstruction[0];
            string serializedNewValue = splitInstruction[1];
            NetworkAttribute attribute = this.networkAttributes[id];
            if (attribute == null)
            {
                // Log Error: Attribute with given ID not found
                return;
            }

            try
            {
                object deserializedValue = JsonConvert.DeserializeObject(serializedNewValue, attribute.ValueType);
                if( !attribute.outsideSettable)
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


        public NetworkAttribute<T> AddNetworkAttribute<T>(string id, T value, bool outsideSettable = true)
        {
            if(this.networkAttributes.ContainsKey(id))
            {
                throw new Exception($"{id} already present in dictionary");
            }
            NetworkAttribute<T> newAttribute = new(id, value);
            this.networkAttributes.Add(id, newAttribute);
            newAttribute.valueChange += this.AttributeChangedEventHandler;
            newAttribute.outsideSettable = outsideSettable;
            return newAttribute;
        }
    }
}
