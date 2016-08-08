using Reactive.Streams;

namespace Reactor.Core.Tests.TCK
{
    class ConcatTest : FluxPublisherVerification<int>
    {
        public override IPublisher<int> CreatePublisher(long elements)
            => Flux.From(Enumerate(elements/2)).ConcatWith(Flux.From(Enumerate((elements + 1)/2)));
    }
}
