// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare.Tests.GraphComparer
{
    using Aqua.GraphCompare;
    using Shouldly;
    using System.Linq;
    using Xunit;

    public class When_comparing_objects_with_different_property_type
    {
        private readonly ComparisonResult result;

        public When_comparing_objects_with_different_property_type()
        {
            var a = new { Value = 123 };

            var b = new { Value = 123.0 };

            result = new GraphComparer().Compare(a, b);
        }

        [Fact]
        public void Should_report_difference()
        {
            result.IsMatch.ShouldBeFalse();
        }

        [Fact]
        public void Should_have_two_difference()
        {
            result.Deltas.Count().ShouldBe(2);
        }

        [Fact]
        public void Result_should_have_delta_for_removed_int_property()
        {
            var delta = result.Deltas.Single(x => x.ChangeType == ChangeType.Delete);

            delta.OldValue.ShouldBe(123);
            delta.NewValue.ShouldBeNull();
            delta.PropertyFrom.PropertyType.ShouldBe(typeof(int));
            delta.PropertyTo.ShouldBeNull();
        }

        [Fact]
        public void Result_should_have_delta_for_added_double_property()
        {
            var delta = result.Deltas.Single(x => x.ChangeType == ChangeType.Insert);

            delta.OldValue.ShouldBeNull();
            delta.NewValue.ShouldBe(123.0);
            delta.PropertyFrom.ShouldBeNull();
            delta.PropertyTo.PropertyType.ShouldBe(typeof(double));
        }
    }
}
