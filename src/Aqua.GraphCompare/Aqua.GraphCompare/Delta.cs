// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare
{
    using System.Reflection;

    public sealed class Delta
    {
        internal Delta(ChangeType changeType, Breadcrumb breadcrumb, object oldValue, object newValue, string oldDisplayValue, string newDisplayValue)
        {
            ChangeType = changeType;
            Breadcrumb = breadcrumb;
            OldValue = oldValue;
            NewValue = newValue;
            OldDisplayValue = oldDisplayValue;
            NewDisplayValue = newDisplayValue;
        }

        public ChangeType ChangeType { get; private set; }

        public Breadcrumb Breadcrumb { get; private set; }

        public object OldValue { get; private set; }

        public object NewValue { get; private set; }

        public string OldDisplayValue { get; private set; }

        public string NewDisplayValue { get; private set; }

        public PropertyInfo PropertyFrom
        {
            get { return Breadcrumb.PropertyFrom; }
        }

        public PropertyInfo PropertyTo
        {
            get { return Breadcrumb.PropertyTo; }
        }

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