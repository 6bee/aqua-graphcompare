﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.GraphCompare
{
    using System;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class DisplayStringAttribute : Attribute
    {
        public DisplayStringAttribute(string displayString)
        {
            DisplayString = displayString;
        }

        public string DisplayString { get; }
    }
}
