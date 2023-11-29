using Orleans.Streams;

namespace Silo
{
    [GenerateSerializer]
    public record class ConsumerGrainState
    {
        [Id(0)]
        public StreamSequenceToken? CurrentStreamSequenceToken { get; set; }

        [Id(1)]
        public string? CurrentData { get; set; }
    }
}
