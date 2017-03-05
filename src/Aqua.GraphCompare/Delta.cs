// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare
{
    using System.Reflection;

    public class Delta
    {
        public Delta(ChangeType changeType, Breadcrumb breadcrumb, object oldValue, object newValue, string oldDisplayValue, string newDisplayValue)
        {
            ChangeType = changeType;
            Breadcrumb = breadcrumb;
            OldValue = oldValue;
            NewValue = newValue;
            OldDisplayValue = oldDisplayValue;
            NewDisplayValue = newDisplayValue;
        }

        public ChangeType ChangeType { get; }

        public Breadcrumb Breadcrumb { get; }

        public object OldValue { get; }

        public object NewValue { get; }

        public string OldDisplayValue { get; }

        public string NewDisplayValue { get; }

        public PropertyInfo PropertyFrom => Breadcrumb.PropertyFrom;

        public PropertyInfo PropertyTo => Breadcrumb.PropertyTo;

        public override string ToString()
        {
            return string.Format("[{0}] {1}: {2} -> {3}",
                ChangeType.ToString().ToUpper(),
                Breadcrumb,
                OldDisplayValue ?? OldValue ?? "[NULL]",
                NewDisplayValue ?? NewValue ?? "[NULL]");
        }
    }
}
