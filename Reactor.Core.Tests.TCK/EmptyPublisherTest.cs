using Reactive.Streams;

namespace Reactor.Core.Tests.TCK
{
    class EmptyPublisherTest : FluxPublisherVerification<int?>
    {
        public override IPublisher<int?> CreatePublisher(long elements) => Flux.Empty<int?>();

        public override long MaxElementsFromPublisher { get; } = 0;
    }
}
