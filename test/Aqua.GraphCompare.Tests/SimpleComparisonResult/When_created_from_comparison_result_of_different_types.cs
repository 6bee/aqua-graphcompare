// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare.Tests.SimpleComparisonResult
{
    using Aqua.Dynamic;
    using Aqua.GraphCompare;
    using Shouldly;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Xunit;

    public class When_created_from_comparison_result_of_different_types
    {
        private class A
        {
            public string StringValue { get; set; }

            public int Int32Value { get; set; }
        }

        private class B
        {
            public string StringValue { get; set; }

            public double Int64Value { get; set; }
        }

        private static PropertyInfo StringValuePropertyA = typeof(A).GetProperty(nameof(A.StringValue));
        private static PropertyInfo StringValuePropertyB = typeof(B).GetProperty(nameof(B.StringValue));
        private static PropertyInfo Int32ValueProperty = typeof(A).GetProperty(nameof(A.Int32Value));
        private static PropertyInfo Int64ValueProperty = typeof(B).GetProperty(nameof(B.Int64Value));

        private readonly SimpleComparisonResult result;

        public When_created_from_comparison_result_of_different_types()
        {
            var a = new A { Int32Value = 32, StringValue = "S1" };
            var b = new B { Int64Value = 64, StringValue = "S2" };

            var breadcrumbUpdate = new Breadcrumb(null, StringValuePropertyA, StringValuePropertyB, new DynamicObject(a), a, new DynamicObject(b), b, () => "ValueS");
            var breadcrumbInsert = new Breadcrumb(null, null, Int64ValueProperty, new DynamicObject(a), a, new DynamicObject(b), b, () => "Value64");
            var breadcrumbDelete = new Breadcrumb(null, Int32ValueProperty, null, new DynamicObject(a), a, new DynamicObject(b), b, () => "Value32");

            var deltas = new[]
            {
                new Delta(ChangeType.Update, breadcrumbUpdate, "S1", "S2", "vS1", "vS2"),
                new Delta(ChangeType.Delete, breadcrumbDelete, 32, null, "v32", "NULL"),
                new Delta(ChangeType.Insert, breadcrumbInsert, null, 64, "NULL", "v64"),
            };

            result = new ComparisonResult(a.GetType(), b.GetType(), deltas).AsSimpleResult();
        }

        [Fact]
        public void Should_report_difference()
        {
            result.IsMatch.ShouldBeFalse();
        }

        [Fact]
        public void Result_should_contain_three_deltas()
        {
            result.Deltas.Count().ShouldBe(3);
        }

        [Fact]
        public void Result_should_delta_for_changed_string_property()
        {
            var d = result.Deltas.Single(x => x.Property == StringValuePropertyB);
            d.ChangeType.ShouldBe(ChangeType.Update);
            d.OldValue.ShouldBe("S1");
            d.NewValue.ShouldBe("S2");
            d.OldDisplayValue.ShouldBe("vS1");
            d.NewDisplayValue.ShouldBe("vS2");
            d.Breadcrumb.ItemFrom.Instance.ShouldBeOfType<A>();
            d.Breadcrumb.ItemTo.Instance.ShouldBeOfType<B>();
            d.Breadcrumb.Parent.ShouldBeNull();
            d.Breadcrumb.Property.ShouldBe(StringValuePropertyB);
        }

        [Fact]
        public void Result_should_delta_for_removed_int_property()
        {
            var d = result.Deltas.Single(x => x.Property == Int32ValueProperty);
            d.ChangeType.ShouldBe(ChangeType.Delete);
            d.OldValue.ShouldBe(32);
            d.NewValue.ShouldBeNull();
            d.OldDisplayValue.ShouldBe("v32");
            d.NewDisplayValue.ShouldBe("NULL");
            d.Breadcrumb.ItemFrom.Instance.ShouldBeOfType<A>();
            d.Breadcrumb.ItemTo.Instance.ShouldBeOfType<B>();
            d.Breadcrumb.Parent.ShouldBeNull();
            d.Breadcrumb.Property.ShouldBe(Int32ValueProperty);
        }

        [Fact]
        public void Result_should_delta_for_added_long_property()
        {
            var d = result.Deltas.Single(x => x.Property == Int64ValueProperty);
            d.ChangeType.ShouldBe(ChangeType.Insert);
            d.OldValue.ShouldBeNull();
            d.NewValue.ShouldBe(64);
            d.OldDisplayValue.ShouldBe("NULL");
            d.NewDisplayValue.ShouldBe("v64");
            d.Breadcrumb.ItemFrom.Instance.ShouldBeOfType<A>();
            d.Breadcrumb.ItemTo.Instance.ShouldBeOfType<B>();
            d.Breadcrumb.Parent.ShouldBeNull();
            d.Breadcrumb.Property.ShouldBe(Int64ValueProperty);
        }
    }
}
