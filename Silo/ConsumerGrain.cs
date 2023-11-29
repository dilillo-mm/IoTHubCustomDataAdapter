using Common;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Orleans.Streams;
using Silo;

namespace Grains;
public interface IConsumerGrain : IGrainWithStringKey
{
}

[ImplicitStreamSubscription(Constants.StreamNamespace)]
public class ConsumerGrain: Grain, IConsumerGrain
{
    private readonly ILogger<IConsumerGrain> _logger;
    private readonly IPersistentState<ConsumerGrainState> _state;

    public ConsumerGrain(ILogger<IConsumerGrain> logger,
            [PersistentState(stateName: "consumer", storageName: "consumers")] IPersistentState<ConsumerGrainState> state)
    {
        _logger = logger;
        _state = state;
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        // Create a GUID based on our GUID as a grain
        var deviceId = this.GetPrimaryKeyString();

        // Get one of the providers which we defined in config
        var streamProvider = this.GetStreamProvider(Constants.StreamProvider);

        // Get the reference to a stream
        var streamId = StreamId.Create(Constants.StreamNamespace, deviceId);
        var stream = streamProvider.GetStream<object>(streamId);

        // Set our OnNext method to the lambda which simply prints the data.
        // This doesn't make new subscriptions, because we are using implicit 
        // subscriptions via [ImplicitStreamSubscription].
        await stream.SubscribeAsync<object>(
            async (data, token) =>
            {
                _state.State = new()
                {
                    CurrentStreamSequenceToken = token,
                    CurrentData = data.ToString()
                };

                await _state.WriteStateAsync();

                _logger.LogInformation("Device {deviceId} sent {data}", deviceId, data);
            },
            (exception) =>
            {
                _logger.LogError(exception, "Error in stream subscription");

                return Task.CompletedTask;
            },  
            () =>
            {
                _logger.LogInformation("Stream completed");

               return Task.CompletedTask;
            },
            _state.State?.CurrentStreamSequenceToken);
    }
}

// ImplicitStreamSubscription attribute here is to subscribe implicitely to all stream within
// a given namespace: whenever some data is pushed to the streams of namespace Constants.StreamNamespace,
// a grain of type ConsumerGrain with the same guid of the stream will receive the message.
// Even if no activations of the grain currently exist, the runtime will automatically
// create a new one and send the message to it.
//[ImplicitStreamSubscription(Constants.StreamNamespace)]
//public class ConsumerGrain : Grain, IConsumerGrain, IStreamSubscriptionObserver
//{
//    private readonly ILogger<IConsumerGrain> _logger;
//    private readonly LoggerObserver _observer;

//    /// <summary>
//    /// Class that will log streaming events
//    /// </summary>
//    private class LoggerObserver : IAsyncObserver<int>
//    {
//        private readonly ILogger<IConsumerGrain> _logger;

//        public LoggerObserver(ILogger<IConsumerGrain> logger)
//        {
//            _logger = logger;
//        }

//        public Task OnCompletedAsync()
//        {
//            _logger.LogInformation("OnCompletedAsync");
//            return Task.CompletedTask;
//        }

//        public Task OnErrorAsync(Exception ex)
//        {
//            _logger.LogInformation("OnErrorAsync: {Exception}", ex);
//            return Task.CompletedTask;
//        }

//        public Task OnNextAsync(int item, StreamSequenceToken? token = null)
//        {
//            _logger.LogInformation("OnNextAsync: item: {Item}, token = {Token}", item, token);
//            return Task.CompletedTask;
//        }
//    }

//    public ConsumerGrain(ILogger<IConsumerGrain> logger)
//    {
//        _logger = logger;
//        _observer = new LoggerObserver(_logger);
//    }

//    // Called when a subscription is added
//    public async Task OnSubscribed(IStreamSubscriptionHandleFactory handleFactory)
//    {
//        // Plug our LoggerObserver to the stream
//        var handle = handleFactory.Create<int>();
//        await handle.ResumeAsync(_observer);
//    }

//    public override Task OnActivateAsync(CancellationToken cancellationToken)
//    {
//        _logger.LogInformation("OnActivateAsync");
//        return Task.CompletedTask;
//    }
//}