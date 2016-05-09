// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare.Tests.GraphComparer
{
    using Aqua.Dynamic;
    using Aqua.GraphCompare;
    using Shouldly;
    using System.Linq;
    using System.Reflection;
    using Xunit;

    public class When_comparing_dynamic_objects
    {
        class A
        {
            public int Int32Value { get; set; }

            public string StringValue { get; set; }
        }

        ComparisonResult result;

        public When_comparing_dynamic_objects()
        {
            var item1 = new DynamicObject(typeof(A))
            {
                { "Int32Value", 1 },
                { "StringValue", "S1" },
            };

            var item2 = new DynamicObject(typeof(A))
            {
                { "Int32Value", 2 },
                { "StringValue", "S2" },
            };

            result = new GraphComparer().Compare(item1, item2);
        }

        [Fact]
        public void Should_report_difference()
        {
            result.IsMatch.ShouldBeFalse();
        }

        [Fact]
        public void Should_have_two_deltas()
        {
            result.Deltas.Count().ShouldBe(2);
        }

        [Fact]
        public void Type_infos_should_be_set()
        {
            result.FromType.ShouldBe(typeof(A));
            result.ToType.ShouldBe(typeof(A));
        }

        [Fact]
        public void Result_should_have_delta_for_int_property()
        {
            var d = result.Deltas.Single(x => x.PropertyTo.Name == "Int32Value");

            d.ChangeType.ShouldBe(ChangeType.Update);
            d.OldValue.ShouldBe(1);
            d.NewValue.ShouldBe(2);
            d.OldDisplayValue.ShouldBeNull();
            d.NewDisplayValue.ShouldBeNull();
            d.PropertyFrom.Name.ShouldBe("Int32Value");
            d.PropertyTo.Name.ShouldBe("Int32Value");
            d.PropertyFrom.PropertyType.ShouldBe(typeof(int));
            d.PropertyTo.PropertyType.ShouldBe(typeof(int));
            d.PropertyFrom.DeclaringType.ShouldBe(typeof(A));
            d.PropertyTo.DeclaringType.ShouldBe(typeof(A));
        }

        [Fact]
        public void Result_should_have_delta_for_string_property()
        {
            var d = result.Deltas.Single(x => x.PropertyTo.Name == "StringValue");

            d.ChangeType.ShouldBe(ChangeType.Update);
            d.OldValue.ShouldBe("S1");
            d.NewValue.ShouldBe("S2");
            d.OldDisplayValue.ShouldBeNull();
            d.NewDisplayValue.ShouldBeNull();
            d.PropertyFrom.Name.ShouldBe("StringValue");
            d.PropertyTo.Name.ShouldBe("StringValue");
            d.PropertyFrom.PropertyType.ShouldBe(typeof(string));
            d.PropertyTo.PropertyType.ShouldBe(typeof(string));
            d.PropertyFrom.DeclaringType.ShouldBe(typeof(A));
            d.PropertyTo.DeclaringType.ShouldBe(typeof(A));
        }
    }
}
