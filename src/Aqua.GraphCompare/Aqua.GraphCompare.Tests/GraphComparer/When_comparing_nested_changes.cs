// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare.Tests.GraphComparer
{
    using Aqua.GraphCompare;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;
    using Xunit.Should;

    public class When_comparing_nested_changes
    {
        class L0
        {
            public L1 L1Property { get; set; }
        }

        class L1
        {
            public string NameProperty { get; set; }

            public L2 L2Property { get; set; }
        }

        class L2
        {
            public int ValueProperty { get; set; }

            public V[] Collection1Property { get; set; }

            public int[] Collection2Property { get; set; }

            public IEnumerable<N> Collection3Property { get; set; }
        }

        class V
        {
            public string XProperty { get; set; }
        }

        enum N
        {
            One,
            Two,
            Three
        }

        ComparisonResult result;

        public When_comparing_nested_changes()
        {
            var item1 = new L0
            {
                L1Property = new L1
                {
                    NameProperty = "test name",
                    L2Property = new L2
                    {
                        ValueProperty = 10,
                        Collection1Property = new[] 
                        {
                            new V { XProperty = "x1" },
                            new V { XProperty = "x2" },
                        },
                        Collection2Property = new[] 
                        {
                            1,
                            2,
                        },
                        Collection3Property = new[] 
                        {
                            N.One,
                            N.Two,
                        },
                    }
                }
            };

            var item2 = new L0
            {
                L1Property = new L1
                {
                    NameProperty = "new test name",
                    L2Property = new L2
                    {
                        ValueProperty = 20,
                        Collection1Property = new[] 
                        {
                            new V { XProperty = "x1" },
                            new V { XProperty = "x3" },
                        },
                        Collection2Property = new[] 
                        {
                            1,
                            3,
                        },
                        Collection3Property = new[] 
                        {
                            N.One,
                            N.Three,
                        },
                    }
                }
            };

            result = new GraphComparer().Compare(item1, item2);
        }

        [Fact]
        public void Should_report_differences()
        {
            result.IsMatch.ShouldBeFalse();
            result.Deltas.Count().ShouldBe(8);
        }

        [Fact]
        public void Name_property_delta_should_have_expected_values()
        {
            var nameProperty = typeof(L1).GetProperty("NameProperty");

            var delta = result.GetDelta(nameProperty);

            delta.ChangeType.ShouldBe(ChangeType.Update);
            delta.OldValue.ShouldBe("test name");
            delta.NewValue.ShouldBe("new test name");
            delta.DisplayValuesShouldBeNull();
            delta.PropertiesShouldBe(nameProperty);
        }

        [Fact]
        public void Name_property_breadcrumb_should_have_expected_values()
        {
            var l1Property = typeof(L0).GetProperty("L1Property");
            var nameProperty = typeof(L1).GetProperty("NameProperty");

            var breadcrumb = result.GetDelta(nameProperty).Breadcrumb;

            breadcrumb.DisplayString.ShouldBeNull();

            breadcrumb.PropertiesShouldBe(nameProperty);

            breadcrumb.ItemTypesShouldBe<L1>();

            breadcrumb.PathShouldBe(typeof(L0).FullName, "L1Property");
            breadcrumb.PropertiesShouldBe(nameProperty);

            breadcrumb.Parent.PathShouldBe(typeof(L0).FullName);
            breadcrumb.Parent.PropertiesShouldBe(l1Property);

            breadcrumb.Parent.Parent.Path.ShouldBeNull();

            breadcrumb.Parent.Parent.Parent.ShouldBeNull();
        }

        [Fact]
        public void Value_property_delta_should_have_expected_values()
        {
            var valueProperty = typeof(L2).GetProperty("ValueProperty");

            var delta = result.GetDelta(valueProperty);

            delta.ChangeType.ShouldBe(ChangeType.Update);
            delta.OldValue.ShouldBe(10);
            delta.NewValue.ShouldBe(20);
            delta.DisplayValuesShouldBeNull();
            delta.PropertiesShouldBe(valueProperty);
        }

        [Fact]
        public void Value_property_breadcrumb_should_have_expected_values()
        {
            var l1Property = typeof(L0).GetProperty("L1Property");
            var l2Property = typeof(L1).GetProperty("L2Property");
            var valueProperty = typeof(L2).GetProperty("ValueProperty");

            var breadcrumb = result.GetDelta(valueProperty).Breadcrumb;

            breadcrumb.DisplayString.ShouldBeNull();

            breadcrumb.PropertiesShouldBe(valueProperty);

            breadcrumb.ItemTypesShouldBe<L2>();

            breadcrumb.PathShouldBe(typeof(L0).FullName, "L1Property", "L2Property");
            breadcrumb.PropertiesShouldBe(valueProperty);

            breadcrumb.Parent.PathShouldBe(typeof(L0).FullName, "L1Property");
            breadcrumb.Parent.PropertiesShouldBe(l2Property);

            breadcrumb.Parent.Parent.PathShouldBe(typeof(L0).FullName);
            breadcrumb.Parent.Parent.PropertiesShouldBe(l1Property);

            breadcrumb.Parent.Parent.Parent.Path.ShouldBeNull();

            breadcrumb.Parent.Parent.Parent.Parent.ShouldBeNull();
        }

        [Fact]
        public void X2_property_delta_should_have_expected_values()
        {
            var xProperty = typeof(V).GetProperty("XProperty");

            var delta = result.GetDeltas(xProperty).Single(x => x.ChangeType == ChangeType.Delete);

            delta.ChangeType.ShouldBe(ChangeType.Delete);
            delta.OldValue.ShouldBe("x2");
            delta.NewValue.ShouldBeNull();
            delta.DisplayValuesShouldBeNull();
            delta.PropertyFrom.ShouldBe(xProperty);
            delta.PropertyTo.ShouldBeNull();
        }

        [Fact]
        public void X2_property_breadcrumb_should_have_expected_values()
        {
            var l1Property = typeof(L0).GetProperty("L1Property");
            var l2Property = typeof(L1).GetProperty("L2Property");
            var collectionProperty = typeof(L2).GetProperty("Collection1Property");
            var xProperty = typeof(V).GetProperty("XProperty");

            var breadcrumb = result.GetDeltas(xProperty).Single(x => x.ChangeType == ChangeType.Delete).Breadcrumb;

            breadcrumb.PropertyFrom.ShouldBe(xProperty);
            breadcrumb.PropertyTo.ShouldBeNull();

            breadcrumb.ItemFrom.TypesShouldBe<V>();
            breadcrumb.ItemTo.ShouldBeNull();

            X_property_breadcrumb_asserts(breadcrumb, hasItemTo: false);
        }

        [Fact]
        public void X3_property_delta_should_have_expected_values()
        {
            var xProperty = typeof(V).GetProperty("XProperty");

            var delta = result.GetDeltas(xProperty).Single(x => x.ChangeType == ChangeType.Insert);

            delta.ChangeType.ShouldBe(ChangeType.Insert);
            delta.OldValue.ShouldBeNull();
            delta.NewValue.ShouldBe("x3");
            delta.DisplayValuesShouldBeNull();
            delta.PropertyFrom.ShouldBeNull();
            delta.PropertyTo.ShouldBe(xProperty);
        }

        [Fact]
        public void X3_property_breadcrumb_should_have_expected_values()
        {
            var l1Property = typeof(L0).GetProperty("L1Property");
            var l2Property = typeof(L1).GetProperty("L2Property");
            var collectionProperty = typeof(L2).GetProperty("Collection1Property");
            var xProperty = typeof(V).GetProperty("XProperty");

            var breadcrumb = result.GetDeltas(xProperty).Single(x => x.ChangeType == ChangeType.Insert).Breadcrumb;

            breadcrumb.PropertyFrom.ShouldBeNull();
            breadcrumb.PropertyTo.ShouldBe(xProperty);

            breadcrumb.ItemFrom.ShouldBeNull();
            breadcrumb.ItemTo.TypesShouldBe<V>();

            X_property_breadcrumb_asserts(breadcrumb, hasItemFrom: false);
        }

        private static void X_property_breadcrumb_asserts(Breadcrumb breadcrumb, bool hasItemFrom = true, bool hasItemTo = true)
        {
            var collectionProperty = typeof(L2).GetProperty("Collection1Property");

            breadcrumb.DisplayString.ShouldBeNull();

            breadcrumb.PathShouldBe(typeof(L0).FullName, "L1Property", "L2Property", "Collection1Property", typeof(V).FullName);

            breadcrumb.DisplayString.ShouldBeNull();

            L2_member_breadcrumb_asserts<V>(breadcrumb.Parent, collectionProperty, hasItemFrom, hasItemTo, hasDynamicObject: true);
        }

        [Fact]
        public void Collection2_2_property_delta_should_have_expected_values()
        {
            var collectionProperty = typeof(L2).GetProperty("Collection2Property");

            var delta = result.Deltas.Single(x => x.Breadcrumb.Parent != null && x.Breadcrumb.Parent.PropertyFrom == collectionProperty && x.ChangeType == ChangeType.Delete);

            delta.ChangeType.ShouldBe(ChangeType.Delete);
            delta.OldValue.ShouldBe(2);
            delta.NewValue.ShouldBeNull();
            delta.DisplayValuesShouldBeNull();
            delta.PropertiesShouldBeNull();
        }

        [Fact]
        public void Collection2_2_property_breadcrumb_should_have_expected_values()
        {
            var collectionProperty = typeof(L2).GetProperty("Collection2Property");

            var breadcrumb = result.Deltas.Single(x => x.Breadcrumb.Parent != null && x.Breadcrumb.Parent.PropertyFrom == collectionProperty && x.ChangeType == ChangeType.Delete).Breadcrumb;

            breadcrumb.DisplayString.ShouldBeNull();

            L2_member_breadcrumb_asserts<int[]>(breadcrumb, collectionProperty);
        }

        [Fact]
        public void Collection2_3_property_delta_should_have_expected_values()
        {
            var collectionProperty = typeof(L2).GetProperty("Collection2Property");

            var delta = result.Deltas.Single(x => x.Breadcrumb.Parent != null && x.Breadcrumb.Parent.PropertyFrom == collectionProperty && x.ChangeType == ChangeType.Insert);

            delta.ChangeType.ShouldBe(ChangeType.Insert);
            delta.OldValue.ShouldBeNull();
            delta.NewValue.ShouldBe(3);
            delta.DisplayValuesShouldBeNull();
            delta.PropertiesShouldBeNull();
        }

        [Fact]
        public void Collection2_3_property_breadcrumb_should_have_expected_values()
        {
            var collectionProperty = typeof(L2).GetProperty("Collection2Property");

            var breadcrumb = result.Deltas.Single(x => x.Breadcrumb.Parent != null && x.Breadcrumb.Parent.PropertyFrom == collectionProperty && x.ChangeType == ChangeType.Insert).Breadcrumb;

            breadcrumb.DisplayString.ShouldBeNull();

            L2_member_breadcrumb_asserts<int[]>(breadcrumb, collectionProperty);
        }

        [Fact]
        public void Collection3_2_property_delta_should_have_expected_values()
        {
            var collectionProperty = typeof(L2).GetProperty("Collection3Property");

            var delta = result.Deltas.Single(x => x.Breadcrumb.Parent != null && x.Breadcrumb.Parent.PropertyFrom == collectionProperty && x.ChangeType == ChangeType.Delete);

            delta.ChangeType.ShouldBe(ChangeType.Delete);
            delta.OldValue.ShouldBe(N.Two.ToString());
            delta.NewValue.ShouldBeNull();
            delta.DisplayValuesShouldBeNull();
            delta.PropertiesShouldBeNull();
        }

        [Fact]
        public void Collection3_2_property_breadcrumb_should_have_expected_values()
        {
            var collectionProperty = typeof(L2).GetProperty("Collection3Property");

            var breadcrumb = result.Deltas.Single(x => x.Breadcrumb.Parent != null && x.Breadcrumb.Parent.PropertyFrom == collectionProperty && x.ChangeType == ChangeType.Delete).Breadcrumb;

            breadcrumb.DisplayString.ShouldBeNull();

            L2_member_breadcrumb_asserts<N[]>(breadcrumb, collectionProperty);
        }

        [Fact]
        public void Collection3_3_property_delta_should_have_expected_values()
        {
            var collectionProperty = typeof(L2).GetProperty("Collection3Property");

            var delta = result.Deltas.Single(x => x.Breadcrumb.Parent != null && x.Breadcrumb.Parent.PropertyFrom == collectionProperty && x.ChangeType == ChangeType.Insert);

            delta.ChangeType.ShouldBe(ChangeType.Insert);
            delta.OldValue.ShouldBeNull();
            delta.NewValue.ShouldBe(N.Three.ToString());
            delta.DisplayValuesShouldBeNull();
            delta.PropertiesShouldBeNull();
        }

        [Fact]
        public void Collection3_3_property_breadcrumb_should_have_expected_values()
        {
            var collectionProperty = typeof(L2).GetProperty("Collection3Property");

            var breadcrumb = result.Deltas.Single(x => x.Breadcrumb.Parent != null && x.Breadcrumb.Parent.PropertyFrom == collectionProperty && x.ChangeType == ChangeType.Insert).Breadcrumb;

            breadcrumb.DisplayString.ShouldBeNull();

            L2_member_breadcrumb_asserts<N[]>(breadcrumb, collectionProperty);
        }

        private static void L2_member_breadcrumb_asserts<T>(Breadcrumb breadcrumb, System.Reflection.PropertyInfo property, bool hasItemFrom = true, bool hasItemTo = true, bool hasDynamicObject = false)
        {
            var l1Property = typeof(L0).GetProperty("L1Property");
            var l2Property = typeof(L1).GetProperty("L2Property");

            breadcrumb.PropertiesShouldBeNull();

            if (hasItemFrom)
            {
                breadcrumb.ItemFrom.Instance.ShouldBeInstanceOf<T>();
                breadcrumb.ItemFrom.InstanceType.ShouldBe(typeof(T));

                if (hasDynamicObject)
                {
                    breadcrumb.ItemFrom.DynamicObject.Type.Type.ShouldBe(typeof(T));
                }
                else
                {
                    breadcrumb.ItemFrom.DynamicObject.ShouldBeNull();
                }
            }
            else
            {
                breadcrumb.ItemFrom.ShouldBeNull();
            }

            if (hasItemTo)
            {
                breadcrumb.ItemTo.Instance.ShouldBeInstanceOf<T>();
                breadcrumb.ItemTo.InstanceType.ShouldBe(typeof(T));

                if (hasDynamicObject)
                {
                    breadcrumb.ItemTo.DynamicObject.Type.Type.ShouldBe(typeof(T));
                }
                else
                {
                    breadcrumb.ItemTo.DynamicObject.ShouldBeNull();
                }
            }
            else
            {
                breadcrumb.ItemTo.ShouldBeNull();
            }

            breadcrumb.PathShouldBe(typeof(L0).FullName, "L1Property", "L2Property", property.Name);

            breadcrumb.Parent.PathShouldBe(typeof(L0).FullName, "L1Property", "L2Property");
            breadcrumb.Parent.PropertiesShouldBe(property);

            breadcrumb.Parent.Parent.PathShouldBe(typeof(L0).FullName, "L1Property");
            breadcrumb.Parent.Parent.PropertiesShouldBe(l2Property);

            breadcrumb.Parent.Parent.Parent.PathShouldBe(typeof(L0).FullName);
            breadcrumb.Parent.Parent.Parent.PropertiesShouldBe(l1Property);

            breadcrumb.Parent.Parent.Parent.Parent.Path.ShouldBeNull();

            breadcrumb.Parent.Parent.Parent.Parent.Parent.ShouldBeNull();
        }
    }
}
