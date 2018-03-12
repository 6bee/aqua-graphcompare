// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare.Tests.GraphComparer
{
    using Aqua.GraphCompare;
    using Shouldly;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class When_comparing_collections
    {
        private readonly ComparisonResult result;

        public When_comparing_collections()
        {
            var collection1 = new[]
            {
                "1",
                "2",
                "3",
                "4",
            };

            var collection2 = new List<object>
            {
                "1",
                "2",
                "4",
                "5",
                99,
            };

            result = new GraphComparer().Compare(new { Collection = collection1 }, new { Collection = collection2 });
        }

        [Fact]
        public void Should_report_difference()
        {
            result.IsMatch.ShouldBeFalse();
        }

        [Fact]
        public void Should_have_three_difference()
        {
            result.Deltas.Count().ShouldBe(3);
        }

        [Fact]
        public void First_deltal_should_be_removed_tree()
        {
            var delta = result.Deltas.ElementAt(0);

            delta.ChangeType.ShouldBe(ChangeType.Delete);
            delta.OldValue.ShouldBe("3");
            delta.NewValue.ShouldBeNull();
        }

        [Fact]
        public void Second_deltal_should_be_added_five()
        {
            var delta = result.Deltas.ElementAt(1);

            delta.ChangeType.ShouldBe(ChangeType.Insert);
            delta.OldValue.ShouldBeNull();
            delta.NewValue.ShouldBe("5");
        }

        [Fact]
        public void Third_deltal_should_be_added_number()
        {
            var delta = result.Deltas.ElementAt(2);

            delta.ChangeType.ShouldBe(ChangeType.Insert);
            delta.OldValue.ShouldBeNull();
            delta.NewValue.ShouldBe(99);
        }
    }
}
