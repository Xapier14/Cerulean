﻿namespace Cerulean.CLI.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ElementTypeAttribute : Attribute
{
    public ElementTypeAttribute(string elementType)
    {
        ElementType = elementType;
    }

    public string ElementType { get; init; }
}