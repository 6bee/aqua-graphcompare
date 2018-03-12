// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare.Tests.GraphComparer
{
    using Aqua.GraphCompare;
    using Shouldly;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Xunit;

    public class When_comparing_nested_changes_with_display_string_attributes
    {
        [DisplayString("ROOT")]
        private class L0
        {
            public N Version { get; set; }

            [DisplayString("Minor Version")]
            public N? MinorVersion { get; set; }

            [DisplayString("Level-1-Property")]
            public L1 L1Property { get; set; }
        }

        [DisplayString("Level-1")]
        private class L1
        {
            public string NameProperty { get; set; }

            public L2 L2Property { get; set; }
        }

        [DisplayString("Level-2")]
        private class L2
        {
            public int ValueProperty { get; set; }

            [DisplayString("")]
            public V[] Collection1Property { get; set; }

            [DisplayString("INT-ARRAY")]
            public int[] Collection2Property { get; set; }

            [DisplayString(null)]
            public IEnumerable<N> Collection3Property { get; set; }
        }

        [DisplayString("THE-V")]
        private class V
        {
            public string XProperty { get; set; }
        }

        [DisplayString("N-DISPLAY")]
        private enum N
        {
            [DisplayString("N-1")]
            One,

            [DisplayString("N-2")]
            Two,

            [DisplayString("N-3")]
            Three,
        }

        private readonly ComparisonResult result;

        public When_comparing_nested_changes_with_display_string_attributes()
        {
            var item1 = new L0
            {
                Version = N.One,
                MinorVersion = null,
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
                    },
                },
            };

            var item2 = new L0
            {
                Version = N.Two,
                MinorVersion = N.One,
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
                    },
                },
            };

            result = new GraphComparer().Compare(item1, item2);
        }

        [Fact]
        public void Should_report_differences()
        {
            result.IsMatch.ShouldBeFalse();
            result.Deltas.Count().ShouldBe(10);
        }

        [Fact]
        public void Version_property_delta_should_have_expected_values()
        {
            var versionProperty = typeof(L0).GetProperty("Version");

            var delta = result.GetDelta(versionProperty);

            delta.ChangeType.ShouldBe(ChangeType.Update);
            delta.OldValue.ShouldBe(N.One);
            delta.NewValue.ShouldBe(N.Two);
            delta.OldDisplayValue.ShouldBe("N-1");
            delta.NewDisplayValue.ShouldBe("N-2");
            delta.PropertiesShouldBe(versionProperty);
        }

        [Fact]
        public void Version_property_breadcrumb_should_have_expected_values()
        {
            var versionProperty = typeof(L0).GetProperty("Version");

            var breadcrumb = result.GetDelta(versionProperty).Breadcrumb;

            breadcrumb.DisplayString.ShouldBe("N-DISPLAY");

            breadcrumb.PropertiesShouldBe(versionProperty);

            breadcrumb.ItemTypesShouldBe<L0>();

            breadcrumb.PathShouldBe("ROOT");
            breadcrumb.PropertiesShouldBe(versionProperty);

            Assert_root_breadcrumb_values(breadcrumb.Parent);
        }

        [Fact]
        public void MinorVersion_property_delta_should_have_expected_values()
        {
            var versionProperty = typeof(L0).GetProperty("MinorVersion");

            var delta = result.GetDelta(versionProperty);

            delta.ChangeType.ShouldBe(ChangeType.Update);
            delta.OldValue.ShouldBeNull();
            delta.NewValue.ShouldBe(N.One);
            delta.OldDisplayValue.ShouldBeNull();
            delta.NewDisplayValue.ShouldBe("N-1");
            delta.PropertiesShouldBe(versionProperty);
        }

        [Fact]
        public void MinorVersion_property_breadcrumb_should_have_expected_values()
        {
            var versionProperty = typeof(L0).GetProperty("MinorVersion");

            var breadcrumb = result.GetDelta(versionProperty).Breadcrumb;

            breadcrumb.DisplayString.ShouldBe("Minor Version");

            breadcrumb.PropertiesShouldBe(versionProperty);

            breadcrumb.ItemTypesShouldBe<L0>();

            breadcrumb.PathShouldBe("ROOT");
            breadcrumb.PropertiesShouldBe(versionProperty);

            Assert_root_breadcrumb_values(breadcrumb.Parent);
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
            var nameProperty = typeof(L1).GetProperty("NameProperty");

            var breadcrumb = result.GetDelta(nameProperty).Breadcrumb;

            breadcrumb.DisplayString.ShouldBe(nameProperty.Name);

            breadcrumb.PropertiesShouldBe(nameProperty);

            breadcrumb.ItemTypesShouldBe<L1>();

            breadcrumb.PathShouldBe("ROOT", "Level-1-Property");
            breadcrumb.PropertiesShouldBe(nameProperty);

            Assert_level1_breadcrumb_values(breadcrumb.Parent);
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
            var valueProperty = typeof(L2).GetProperty("ValueProperty");

            var breadcrumb = result.GetDelta(valueProperty).Breadcrumb;

            breadcrumb.DisplayString.ShouldBe(valueProperty.Name);

            breadcrumb.PropertiesShouldBe(valueProperty);

            breadcrumb.ItemTypesShouldBe<L2>();

            breadcrumb.PathShouldBe("ROOT", "Level-1-Property", "Level-2");
            breadcrumb.PropertiesShouldBe(valueProperty);

            Assert_level2_breadcrumb_values(breadcrumb.Parent);
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
            Assert_X_property_breadcrumb_values(ChangeType.Delete, hasItemTo: false);
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
            Assert_X_property_breadcrumb_values(ChangeType.Insert, hasItemFrom: false);
        }

        private void Assert_X_property_breadcrumb_values(ChangeType changeType, bool hasItemFrom = true, bool hasItemTo = true)
        {
            var collectionProperty = typeof(L2).GetProperty("Collection1Property");

            var xProperty = typeof(V).GetProperty("XProperty");

            var breadcrumb = result.GetDeltas(xProperty).Single(x => x.ChangeType == changeType).Breadcrumb;

            breadcrumb.DisplayString.ShouldBe("XProperty");

            breadcrumb.PathShouldBe("ROOT", "Level-1-Property", "Level-2", "THE-V");

            breadcrumb.Parent.PropertiesShouldBeNull();

            if (hasItemFrom)
            {
                breadcrumb.ItemFrom.TypesShouldBe<V>();
                breadcrumb.PropertyFrom.ShouldBe(xProperty);
                breadcrumb.Parent.ItemFrom.Instance.ShouldBeOfType<V>();
                breadcrumb.Parent.ItemFrom.InstanceType.ShouldBe(typeof(V));
                breadcrumb.Parent.ItemFrom.DynamicObject.Type.Type.ShouldBe(typeof(V));
            }
            else
            {
                breadcrumb.ItemFrom.ShouldBeNull();
                breadcrumb.PropertyFrom.ShouldBeNull();
                breadcrumb.Parent.ItemFrom.ShouldBeNull();
            }

            if (hasItemTo)
            {
                breadcrumb.ItemTo.TypesShouldBe<V>();
                breadcrumb.PropertyTo.ShouldBe(xProperty);
                breadcrumb.Parent.ItemTo.Instance.ShouldBeOfType<V>();
                breadcrumb.Parent.ItemTo.InstanceType.ShouldBe(typeof(V));
                breadcrumb.Parent.ItemTo.DynamicObject.Type.Type.ShouldBe(typeof(V));
            }
            else
            {
                breadcrumb.ItemTo.ShouldBeNull();
                breadcrumb.Parent.ItemTo.ShouldBeNull();
                breadcrumb.PropertyTo.ShouldBeNull();
            }

            breadcrumb.Parent.PathShouldBe("ROOT", "Level-1-Property", "Level-2");

            breadcrumb.Parent.Parent.PathShouldBe("ROOT", "Level-1-Property", "Level-2");
            breadcrumb.Parent.Parent.PropertiesShouldBe(collectionProperty);

            Assert_level2_breadcrumb_values(breadcrumb.Parent.Parent.Parent);
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

            breadcrumb.PropertiesShouldBeNull();

            breadcrumb.ItemFrom.Instance.ShouldBeOfType<int[]>();
            breadcrumb.ItemFrom.InstanceType.ShouldBe(typeof(int[]));
            breadcrumb.ItemFrom.DynamicObject.ShouldBeNull();

            breadcrumb.ItemTo.Instance.ShouldBeOfType<int[]>();
            breadcrumb.ItemTo.InstanceType.ShouldBe(typeof(int[]));
            breadcrumb.ItemTo.DynamicObject.ShouldBeNull();

            breadcrumb.PathShouldBe("ROOT", "Level-1-Property", "Level-2", "INT-ARRAY");

            breadcrumb.Parent.PathShouldBe("ROOT", "Level-1-Property", "Level-2");
            breadcrumb.Parent.PropertiesShouldBe(collectionProperty);

            Assert_level2_breadcrumb_values(breadcrumb.Parent.Parent);
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

            breadcrumb.PropertiesShouldBeNull();

            breadcrumb.ItemFrom.Instance.ShouldBeOfType<int[]>();
            breadcrumb.ItemFrom.InstanceType.ShouldBe(typeof(int[]));
            breadcrumb.ItemFrom.DynamicObject.ShouldBeNull();

            breadcrumb.ItemTo.Instance.ShouldBeOfType<int[]>();
            breadcrumb.ItemTo.InstanceType.ShouldBe(typeof(int[]));
            breadcrumb.ItemTo.DynamicObject.ShouldBeNull();

            breadcrumb.PathShouldBe("ROOT", "Level-1-Property", "Level-2", "INT-ARRAY");

            breadcrumb.Parent.PathShouldBe("ROOT", "Level-1-Property", "Level-2");
            breadcrumb.Parent.PropertiesShouldBe(collectionProperty);

            Assert_level2_breadcrumb_values(breadcrumb.Parent.Parent);
        }

        [Fact]
        public void Collection3_2_property_delta_should_have_expected_values()
        {
            var collectionProperty = typeof(L2).GetProperty("Collection3Property");

            var delta = result.Deltas.Single(x => x.Breadcrumb.Parent != null && x.Breadcrumb.Parent.PropertyFrom == collectionProperty && x.ChangeType == ChangeType.Delete);

            delta.ChangeType.ShouldBe(ChangeType.Delete);
            delta.OldValue.ShouldBe(N.Two);
            delta.NewValue.ShouldBeNull();
            delta.OldDisplayValue.ShouldBe("N-2");
            delta.NewDisplayValue.ShouldBeNull();
            delta.PropertiesShouldBeNull();
        }

        [Fact]
        public void Collection3_2_property_breadcrumb_should_have_expected_values()
        {
            var collectionProperty = typeof(L2).GetProperty("Collection3Property");

            var breadcrumb = result.Deltas.Single(x => x.Breadcrumb.Parent != null && x.Breadcrumb.Parent.PropertyFrom == collectionProperty && x.ChangeType == ChangeType.Delete).Breadcrumb;

            breadcrumb.DisplayString.ShouldBeNull();

            breadcrumb.PropertiesShouldBeNull();

            breadcrumb.ItemFrom.Instance.ShouldBeOfType<N[]>();
            breadcrumb.ItemFrom.InstanceType.ShouldBe(typeof(N[]));
            breadcrumb.ItemFrom.DynamicObject.ShouldBeNull();

            breadcrumb.ItemTo.Instance.ShouldBeOfType<N[]>();
            breadcrumb.ItemTo.InstanceType.ShouldBe(typeof(N[]));
            breadcrumb.ItemTo.DynamicObject.ShouldBeNull();

            breadcrumb.PathShouldBe("ROOT", "Level-1-Property", "Level-2");

            breadcrumb.Parent.PathShouldBe("ROOT", "Level-1-Property", "Level-2");
            breadcrumb.Parent.PropertiesShouldBe(collectionProperty);

            Assert_level2_breadcrumb_values(breadcrumb.Parent.Parent);
        }

        [Fact]
        public void Collection3_3_property_delta_should_have_expected_values()
        {
            var collectionProperty = typeof(L2).GetProperty("Collection3Property");

            var delta = result.Deltas.Single(x => x.Breadcrumb.Parent != null && x.Breadcrumb.Parent.PropertyFrom == collectionProperty && x.ChangeType == ChangeType.Insert);

            delta.ChangeType.ShouldBe(ChangeType.Insert);
            delta.OldValue.ShouldBeNull();
            delta.NewValue.ShouldBe(N.Three);
            delta.OldDisplayValue.ShouldBeNull();
            delta.NewDisplayValue.ShouldBe("N-3");
            delta.PropertiesShouldBeNull();
        }

        [Fact]
        public void Collection3_3_property_breadcrumb_should_have_expected_values()
        {
            var collectionProperty = typeof(L2).GetProperty("Collection3Property");

            var breadcrumb = result.Deltas.Single(x => x.Breadcrumb.Parent != null && x.Breadcrumb.Parent.PropertyFrom == collectionProperty && x.ChangeType == ChangeType.Insert).Breadcrumb;

            breadcrumb.DisplayString.ShouldBeNull();

            breadcrumb.PropertiesShouldBeNull();

            breadcrumb.ItemFrom.Instance.ShouldBeOfType<N[]>();
            breadcrumb.ItemFrom.InstanceType.ShouldBe(typeof(N[]));
            breadcrumb.ItemFrom.DynamicObject.ShouldBeNull();

            breadcrumb.ItemTo.Instance.ShouldBeOfType<N[]>();
            breadcrumb.ItemTo.InstanceType.ShouldBe(typeof(N[]));
            breadcrumb.ItemTo.DynamicObject.ShouldBeNull();

            breadcrumb.PathShouldBe("ROOT", "Level-1-Property", "Level-2");

            breadcrumb.Parent.PathShouldBe("ROOT", "Level-1-Property", "Level-2");
            breadcrumb.Parent.PropertiesShouldBe(collectionProperty);

            Assert_level2_breadcrumb_values(breadcrumb.Parent.Parent);
        }

        private static void Assert_level2_breadcrumb_values(Breadcrumb breadcrumb)
        {
            var l2Property = typeof(L1).GetProperty("L2Property");

            breadcrumb.PathShouldBe("ROOT", "Level-1-Property");
            breadcrumb.DisplayString.ShouldBe("Level-2");
            breadcrumb.PropertiesShouldBe(l2Property);
            breadcrumb.ItemTypesShouldBe<L1>();

            Assert_level1_breadcrumb_values(breadcrumb.Parent);
        }

        private static void Assert_level1_breadcrumb_values(Breadcrumb breadcrumb)
        {
            var l1Property = typeof(L0).GetProperty("L1Property");

            breadcrumb.PathShouldBe("ROOT");
            breadcrumb.DisplayString.ShouldBe("Level-1-Property");
            breadcrumb.PropertiesShouldBe(l1Property);
            breadcrumb.ItemTypesShouldBe<L0>();

            Assert_root_breadcrumb_values(breadcrumb.Parent);
        }

        private static void Assert_root_breadcrumb_values(Breadcrumb breadcrumb)
        {
            breadcrumb.ShouldNotBeNull();
            breadcrumb.DisplayString.ShouldBe("ROOT");
            breadcrumb.ItemTypesShouldBe<L0>();
            breadcrumb.Parent.ShouldBeNull();
            breadcrumb.Path.ShouldBeNull();
            breadcrumb.PropertiesShouldBeNull();
        }
    }
}
