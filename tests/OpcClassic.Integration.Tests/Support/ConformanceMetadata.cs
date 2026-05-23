//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System.Reflection;

namespace OpcClassic.Integration.Tests.Support;

internal static class ConformanceMetadata
{
    public static bool HasCategory(Type testType, string methodName, string category)
    {
        var method = testType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
        return method?.GetCustomAttributesData().Any(attribute =>
            attribute.AttributeType.FullName == "TUnit.Core.CategoryAttribute"
            && attribute.ConstructorArguments.Any(argument =>
                argument.Value is string value && string.Equals(value, category, StringComparison.Ordinal))) == true;
    }

    public static Type ReadType<T>() => typeof(T);

    public static string ReadString(string value) => value;

    public static int ReadInt32(int value) => value;
}
