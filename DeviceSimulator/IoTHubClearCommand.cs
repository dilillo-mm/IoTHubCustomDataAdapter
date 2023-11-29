using Microsoft.Extensions.Configuration;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DeviceSimulator
{
    public class IoTHubClearCommand : AsyncCommand
    {
        private readonly IoTHubManager _iotHubManager;

        public IoTHubClearCommand(IConfiguration configuration)
        {
            var iotHubConnectionString = configuration.GetConnectionString("TargetIoTHub") ?? throw new ArgumentNullException("TargetIoTHub");

            _iotHubManager = new IoTHubManager(iotHubConnectionString);
        }

        public override async Task<int> ExecuteAsync(CommandContext context)
        {
            try
            {
                await _iotHubManager.Open();

                await _iotHubManager.ClearDevices();

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
