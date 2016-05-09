// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare.Tests.GraphComparer
{
    using Aqua.GraphCompare;
    using Shouldly;
    using System.Linq;
    using Xunit;

    public class When_comparing_anonymous_types
    {
        ComparisonResult result;

        public When_comparing_anonymous_types()
        {
            var item1 = new
            {
                Name = "test name",
                Value = 1.23456789m,
                Collection1 = new[]
                {
                    new { X = "x1" },
                    new { X = "x2" },
                    new { X = "x3" },
                },
                Collection2 = new[]
                {
                    new { Y = "y1" },
                    new { Y = "y2" },
                },
                Collection3 = new[]
                {
                    "z1",
                    "z2"
                },
                Collection4 = new[]
                {
                    "z1",
                    "z2"
                },
                Collection5 = (int[])null,
                Collection6 = new[]
                {
                    1,
                    2
                },
            };

            var item2 = new
            {
                Name = "test new name",
                Value = "abc",
                Collection1 = new[]
                {
                    new { X = "x1" },
                    new { X = "x2" },
                    new { X = "x3" },
                },
                Collection2 = new[]
                {
                    new { Y = "y1" },
                    new { Y = "y3" },
                },
                Collection3 = new[]
                {
                    "z1",
                    "z3"
                },
                Collection4 = (string[])null,
                Collection5 = new[]
                {
                    1,
                    2
                },
                Collection6 = new[]
                {
                    1,
                    3
                },
            };

            result = new GraphComparer().Compare(item1, item2);
        }

        [Fact]
        public void Should_report_difference()
        {
            result.IsMatch.ShouldBeFalse();
        }

        [Fact]
        public void Should_have_expected_number_of_deltas()
        {
            result.Deltas.Count().ShouldBe(13);
        }

        [Fact]
        public void Should_have_delta_for_added_value_property_of_type_string()
        {
            var d = result.Deltas.Single(x => x.PropertyTo != null && x.PropertyTo.Name == "Value");
            d.PropertyFrom.ShouldBeNull();
            d.PropertyTo.PropertyType.ShouldBe(typeof(string));
            d.OldValue.ShouldBeNull();
            d.NewValue.ShouldBe("abc");
        }

        [Fact]
        public void Should_have_delta_for_removed_value_property_of_type_decimal()
        {
            var d = result.Deltas.Single(x => x.PropertyFrom != null && x.PropertyFrom.Name == "Value");
            d.PropertyTo.ShouldBeNull();
            d.PropertyFrom.PropertyType.ShouldBe(typeof(decimal));
            d.OldValue.ShouldBe(1.23456789m);
            d.NewValue.ShouldBeNull();
        }

        [Fact]
        public void Should_have_delta_for_chnage_name_property()
        {
            var d = result.Deltas.Single(x => x.PropertyFrom != null && x.PropertyFrom.Name == "Name");
            d.PropertyTo.Name.ShouldBe("Name");
            d.OldValue.ShouldBe("test name");
            d.NewValue.ShouldBe("test new name");
        }
    }
}
