using System.ComponentModel;
using Sanctum_Core;
namespace Sanctum_Core_Testing
{ 
        public class NetworkAttributeFactoryTests
        {
            private NetworkAttributeFactory networkAttributeFactory;

            [SetUp]
            public void Setup()
            {
                this.networkAttributeFactory = new NetworkAttributeFactory();
            }

            [Test]
            public void AddNetworkAttribute_ShouldAddAttribute()
            {
                NetworkAttribute<int> attribute = this.networkAttributeFactory.AddNetworkAttribute("test-attribute", 5);

                Assert.That(this.networkAttributeFactory.networkAttributes.ContainsKey("test-attribute"), Is.True);
                Assert.That(attribute.Value, Is.EqualTo(5));
            }

            [Test]
            public void AddNetworkAttribute_ShouldThrowException_WhenDuplicateId()
            {
                _ = this.networkAttributeFactory.AddNetworkAttribute("test-attribute", 5);

                _ = Assert.Throws<Exception>(() => this.networkAttributeFactory.AddNetworkAttribute("test-attribute", 10));
            }

            [Test]
            public void HandleNetworkedAttribute_ShouldSetAttributeValue()
            {
                NetworkAttribute<int> attribute = this.networkAttributeFactory.AddNetworkAttribute("test-attribute", 5);
                string instruction = "test-attribute|10";

                this.networkAttributeFactory.HandleNetworkedAttribute(instruction);

                Assert.That(attribute.Value, Is.EqualTo(10));
            }

            [Test]
            public void HandleNetworkedAttribute_ShouldNotSetAttributeValue_WhenOutsideSettableIsFalse()
            {
                NetworkAttribute<int> attribute = this.networkAttributeFactory.AddNetworkAttribute("test-attribute", 5, false);
                string instruction = "test-attribute|10";

                this.networkAttributeFactory.HandleNetworkedAttribute(instruction);

                Assert.That(attribute.Value, Is.EqualTo(5));
            }

            [Test]
            public void HandleNetworkedAttribute_ShouldLogError_WhenAttributeNotFound()
            {
                string instruction = "invalid-attribute|10";

                Assert.DoesNotThrow(() => this.networkAttributeFactory.HandleNetworkedAttribute(instruction));
            }

            [Test]
            public void HandleNetworkedAttribute_ShouldLogError_WhenInstructionFormatIsInvalid()
            {
                NetworkAttribute<int> attribute = this.networkAttributeFactory.AddNetworkAttribute("test-attribute", 5);
                string instruction = "invalid-format";

                Assert.DoesNotThrow(() => this.networkAttributeFactory.HandleNetworkedAttribute(instruction));
            }


            [Test]
            public void NetworkAttribute_SetValue_ShouldTriggerValueChangeEvent()
            { 
                bool eventTriggered = false;
                NetworkAttribute<int> attribute = new("test-attribute", 5);
                attribute.valueChanged += (attribute) => eventTriggered = true;

                attribute.SetValue(10);

                Assert.That(eventTriggered, Is.True);
                Assert.That(attribute.Value, Is.EqualTo(10));
            }
        }
  }

