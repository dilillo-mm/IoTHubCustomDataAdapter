using Spectre.Console;

namespace DeviceSimulator
{
    public class SimulatedDeviceManager
    {
        readonly IoTHubManager _iotHubManager;
        readonly List<SimulatedDevice> _devices = new();

        public SimulatedDeviceManager(IoTHubManager ioTHubHelper) 
        {
            _iotHubManager = ioTHubHelper;
        }

        public async Task StartDevices(string deviceIdPrefix, int deviceCount, string payload)
        {
            await AnsiConsole.Progress()
                .StartAsync(async ctx =>
                {
                    // Define tasks
                    var task1 = ctx.AddTask($"provisioning {deviceCount} devices");
                    var task2 = ctx.AddTask("starting devices");

                    var incrementValue = 100d / deviceCount;
                    
                    await _iotHubManager.Open();

                    var deviceIds = Array.CreateInstance(typeof(string), deviceCount);

                    for (var i = 0; i < deviceCount; i++)
                    {
                        var newDeviceId = deviceIdPrefix + i;

                        var iotHubDeviceManager = await _iotHubManager.AddDevice(newDeviceId);

                        var newDevice = new SimulatedDevice(iotHubDeviceManager, payload);

                        _devices.Add(newDevice);

                        task1.Increment(incrementValue);
                    }

                    var options = new ParallelOptions()
                    {
                        MaxDegreeOfParallelism = 10
                    };

                    await Parallel.ForEachAsync(_devices, options, async (i, ct) =>
                    {
                        await i.Start();

                        task2.Increment(incrementValue);
                    });
                });
        }

        public async Task StopDevices()
        {
            await AnsiConsole.Progress()
                .StartAsync(async ctx =>
                {
                    // Define tasks
                    var task1 = ctx.AddTask("stopping devices");

                    double incrementValue = 100d / _devices.Count;

                    var options = new ParallelOptions()
                    {
                        MaxDegreeOfParallelism = 10
                    };

                    await Parallel.ForEachAsync(_devices, options, async (i, ct) =>
                    {
                        await i.Stop();

                        task1.Increment(incrementValue);
                    });

                    _devices.Clear();
                });
        }
    }
}
