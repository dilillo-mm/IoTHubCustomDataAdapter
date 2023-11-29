using Microsoft.Extensions.Configuration;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DeviceSimulator
{
    public class SimulateDevicesCommandSettings : CommandSettings
    {
        [CommandArgument(0, "[DeviceIdPrefix]")]
        public string DeviceIdPrefix { get; set; } = "sim1device";

        [CommandArgument(1, "[DeviceCount]")]
        public int DeviceCount { get; set; } = 10;

        [CommandArgument(2, "[PayloadFile]")]
        public string PayloadFile { get; set; } = "payload-default.json";
    }

    public class SimulateDevicesCommand : AsyncCommand<SimulateDevicesCommandSettings>
    {
        private readonly IoTHubManager _iotHubManager;

        public SimulateDevicesCommand(IConfiguration configuration)
        {
            var iotHubConnectionString = configuration.GetConnectionString("TargetIoTHub") ?? throw new ArgumentNullException("TargetIoTHub");

            _iotHubManager = new IoTHubManager(iotHubConnectionString);
        }

        public override async Task<int> ExecuteAsync(CommandContext context, SimulateDevicesCommandSettings settings)
        {
            try
            {
                await _iotHubManager.Open();

                var simulatedDeviceManager = new SimulatedDeviceManager(_iotHubManager);

                var payload = await File.ReadAllTextAsync(settings.PayloadFile);

                await simulatedDeviceManager.StartDevices(settings.DeviceIdPrefix, settings.DeviceCount, payload);

                AnsiConsole.WriteLine("devices provisioned and transmitting.  press enter to stop ...");
                AnsiConsole.WriteLine();

                _ = Console.ReadLine();

                await simulatedDeviceManager.StopDevices();

                await _iotHubManager.Close();
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex);
            }

            return 0;
        }
    }
}
