using System;
using Reactive.Streams;

namespace Reactor.Core.Tests.TCK
{
    class GroupByTest : FluxPublisherVerification<int>
    {
        public override IPublisher<int> CreatePublisher(long elements)
        {
            if (elements == 0)
                return Flux.Empty<int>();

            var all = Flux.From(Enumerate(elements)).GroupBy(_ => "all");
            var groupedFlux = all.BlockFirst(TimeSpan.FromSeconds(3));
            return groupedFlux.Map(x => x);
        }
    }
}
