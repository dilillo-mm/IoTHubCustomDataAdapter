using Spectre.Console;

namespace DeviceSimulator
{
    public partial class SimulatedDevice
    {
        readonly IoTHubDeviceManager _iotHubDeviceManager;

        readonly System.Timers.Timer _timer = new(5000)
        {
            AutoReset = true
        };

        public SimulatedDevice(IoTHubDeviceManager iotHubDeviceHelper, string payload)
        {
            _iotHubDeviceManager = iotHubDeviceHelper;

            _timer.Elapsed += (object? sender, System.Timers.ElapsedEventArgs e) =>
            {
                _ = Task.Factory.StartNew(async () =>
                {
                    await _iotHubDeviceManager.SendMessage(payload);

                    AnsiConsole.WriteLine($"{DeviceId} sent message");
                });
            };
        }

        public string DeviceId => _iotHubDeviceManager.DeviceId;

        public async Task Start()
        {
            await _iotHubDeviceManager.Open();

            _timer.Start();
        }

        public async Task Stop()
        {
            _timer.Stop();

            await Task.Delay(5000);

            await _iotHubDeviceManager.Close();
        }
    }
}
