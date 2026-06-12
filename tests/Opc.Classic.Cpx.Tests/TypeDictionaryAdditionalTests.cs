//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Cpx.Tests;

public sealed class TypeDictionaryAdditionalTests
{
    [Test]
    public async Task Constructor_CopiesTypesAndNormalizesNullName()
    {
        var type = new TypeDescription("Level", "LevelType", TypeKind.Double, isComplex: false);
        TypeDescription[] source = [type];

        var dictionary = new TypeDictionary(null!, source, defaultBigEndian: false, defaultStringEncoding: "UTF-8", defaultCharWidth: 1, defaultFloatFormat: "IEEE-754");
        source[0] = new TypeDescription("Other", "OtherType", TypeKind.Int32, isComplex: false);

        await Assert.That(dictionary.Name).IsEqualTo(string.Empty);
        await Assert.That(dictionary.DefaultBigEndian).IsFalse();
        await Assert.That(dictionary.DefaultStringEncoding).IsEqualTo("UTF-8");
        await Assert.That(dictionary.DefaultCharWidth).IsEqualTo(1);
        await Assert.That(dictionary.DefaultFloatFormat).IsEqualTo("IEEE-754");
        await Assert.That(dictionary.Types.Count).IsEqualTo(1);
        await Assert.That(dictionary.TryGet("Level")).IsEqualTo(type);
        await Assert.That(dictionary.TryGetByTypeId("LevelType")).IsEqualTo(type);
        await Assert.That(dictionary.Contains("Other")).IsFalse();
    }

    [Test]
    public async Task Lookup_NullNames_ThrowArgumentNullException()
    {
        var dictionary = TypeDictionary.FromTypes(new TypeDescription("Level", "LevelType", TypeKind.Double, isComplex: false));

        await Assert.That(() => dictionary.TryGet(null!))
            .Throws<ArgumentNullException>();
        await Assert.That(() => dictionary.TryGetByTypeId(null!))
            .Throws<ArgumentNullException>();
        await Assert.That(() => dictionary.Contains(null!))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Constructor_InvalidDefaults_ThrowSpecificExceptions()
    {
        var type = new TypeDescription("Level", "LevelType", TypeKind.Double, isComplex: false);

        await Assert.That(() => new TypeDictionary("Bad", null!))
            .Throws<ArgumentNullException>();
        await Assert.That(() => new TypeDictionary("Bad", new[] { type }, defaultStringEncoding: " "))
            .Throws<ArgumentException>();
        await Assert.That(() => new TypeDictionary("Bad", new[] { type }, defaultCharWidth: 0))
            .Throws<ArgumentOutOfRangeException>();
        await Assert.That(() => new TypeDictionary("Bad", new[] { type }, defaultFloatFormat: " "))
            .Throws<ArgumentException>();
    }

    [Test]
    public async Task Equality_IncludesMetadataAndTypeOrder()
    {
        var first = new TypeDescription("A", "AType", TypeKind.Int16, isComplex: false);
        var second = new TypeDescription("B", "BType", TypeKind.Int32, isComplex: false);
        var left = new TypeDictionary("Plant", new[] { first, second }, defaultBigEndian: true);
        var same = new TypeDictionary("Plant", new[] { first, second }, defaultBigEndian: true);
        var differentOrder = new TypeDictionary("Plant", new[] { second, first }, defaultBigEndian: true);
        var differentMetadata = new TypeDictionary("Plant", new[] { first, second }, defaultBigEndian: false);

        await Assert.That(left).IsEqualTo(same);
        await Assert.That(left.GetHashCode()).IsEqualTo(same.GetHashCode());
        await Assert.That(left).IsNotEqualTo(differentOrder);
        await Assert.That(left).IsNotEqualTo(differentMetadata);
    }
}
