﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core
{
    public class GenericDictionary
    {
        private readonly Dictionary<string, object> _dict = new();

        public void Add<T>(string key, T value)
        {
            this._dict.Add(key, value);
        }

        public T GetValue<T>(string key) where T : class
        {
            return this._dict[key] as T;
        }
    }

    public class NetworkAttributeFactory
    {
        public event PropertyChangedEventHandler attributeValueChanged = delegate { };
        public GenericDictionary networkAttributes = new();

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
            NetworkAttribute<object> attribute = this.networkAttributes.GetValue<NetworkAttribute<object>>(ID);
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
            string newID = $"{uuid}-{this._id++}";
            NetworkAttribute<T> newAttribute = new(newID, value);
            this.networkAttributes.Add<T>(newID, value);
            newAttribute.networkValueChange += this.AttributeChangedEventHandler;
            return newAttribute;
        }

    }
}
