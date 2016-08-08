using Reactive.Streams;

namespace Reactor.Core.Tests.TCK
{
    class FlattenTest : FluxPublisherVerification<int>
    {
        public override IPublisher<int> CreatePublisher(long elements)
        {
            var s1 = Flux.From(Enumerate(elements/2));
            var s2 = Flux.From(Enumerate((elements + 1)/2));

            return Flux.From(s1, s2).FlatMap(x => x);
        }
    }
}
