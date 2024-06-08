using Newtonsoft.Json;
using System.ComponentModel;

namespace Sanctum_Core
{
    public class NetworkAttribute<T>
    {
        public event PropertyChangedEventHandler networkValueChange = delegate { };
        public event PropertyChangedEventHandler valueChange = delegate { };

        private T value;
        public bool changeHasBeenNetworked;
        public T Value
        {
            get => this.value;
            set
            {
                this.changeHasBeenNetworked = true;
                this.networkValueChange(this.Id, new PropertyChangedEventArgs(JsonConvert.SerializeObject(value)));
            }
        }
        public string Id { get; }
        public NetworkAttribute(string id, T value)
        {
            this.Id = id;
            this.value = value;
            this.changeHasBeenNetworked = false;
        }
        public void NonNetworkedSet(T value)
        {
            this.value = value;
            this.changeHasBeenNetworked = false;
            this.valueChange(value, new PropertyChangedEventArgs("NonNetworkChange"));
        }
    }
}
