using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using System.Text;
using Message = Microsoft.Azure.Devices.Client.Message;

namespace DeviceSimulator
{
    public class IoTHubDeviceManager
    {
        readonly DeviceClient _deviceClient;

        public IoTHubDeviceManager(string iotHubHostname, Device device)
        {
            var auth = new DeviceAuthenticationWithRegistrySymmetricKey(device.Id, device.Authentication.SymmetricKey.PrimaryKey);

            _deviceClient = DeviceClient.Create(iotHubHostname, auth);

            DeviceId = device.Id;
        }
        
        public string DeviceId { get; }

        public Task Open() => _deviceClient.OpenAsync();

        public Task Close() => _deviceClient.CloseAsync();

        public async Task SendMessage(string messageBody)
        {
            var message = new Message(Encoding.UTF8.GetBytes(messageBody));

            await _deviceClient.SendEventAsync(message);
        }
    }
}
