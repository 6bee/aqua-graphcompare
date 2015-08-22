// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare.Tests.GraphComparer
{
    using Aqua.GraphCompare;
    using System.Linq;
    using Xunit;
    using Xunit.Should;

    public class When_comparing_anonymous_types
    {
        ComparisonResult result;

        public When_comparing_anonymous_types()
        {
            var item1 = new
            {
                Name = "test nane",
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
            };

            var item2 = new
            {
                Name = "test nane",
                Value = 0.23456789m,
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
                }
            };

            result = new GraphComparer().Compare(item1, item2);
        }

        [Fact]
        public void Should_report_difference()
        {
            result.IsMatch.ShouldBeFalse();
        }

        [Fact]
        public void Should_have_three_difference()
        {
            result.Deltas.Count().ShouldBe(9);
        }
    }
}
