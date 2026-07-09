// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Cpx.Tests;

public sealed class OpcCpxDataFilterTests
{
    [Test]
    public async Task Apply_EqualityMatch_ReturnsOriginalValue()
    {
        var value = CreateValue(status: "Good", count: 3);

        var result = OpcCpxDataFilter.Apply(value, CreateType(), "Status == Good");

        await Assert.That(result.Error).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(result.Value).IsEqualTo(value);
    }

    [Test]
    public async Task Apply_QuotedStringEqualityMatch_ReturnsOriginalValue()
    {
        var value = CreateValue(status: "Needs Attention", count: 3);

        var result = OpcCpxDataFilter.Apply(value, CreateType(), "Status = 'Needs Attention'");

        await Assert.That(result.Error).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(result.Value).IsEqualTo(value);
    }

    [Test]
    public async Task Apply_NumericEqualityMatch_ReturnsOriginalValue()
    {
        var value = CreateValue(status: "Good", count: 3);

        var result = OpcCpxDataFilter.Apply(value, CreateType(), "Count == 3");

        await Assert.That(result.Error).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(result.Value).IsEqualTo(value);
    }

    [Test]
    public async Task Apply_EqualityMiss_ReturnsFilterNoData()
    {
        var value = CreateValue(status: "Bad", count: 3);

        var result = OpcCpxDataFilter.Apply(value, CreateType(), "Status == Good");

        await Assert.That(result.Error).IsEqualTo(OpcComplexDataResult.OPCCPX_S_FILTER_NO_DATA);
        await Assert.That(result.Value!.Fields.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Apply_FieldNotFound_ReturnsFilterError()
    {
        var value = CreateValue(status: "Good", count: 3);

        var result = OpcCpxDataFilter.Apply(value, CreateType(), "Missing == Good");

        await Assert.That(result.Error).IsEqualTo(OpcComplexDataResult.OPCCPX_E_FILTER_ERROR);
    }

    [Test]
    public async Task Apply_InvalidExpression_ReturnsFilterInvalid()
    {
        var value = CreateValue(status: "Good", count: 3);

        var result = OpcCpxDataFilter.Apply(value, CreateType(), "Status Good");

        await Assert.That(result.Error).IsEqualTo(OpcComplexDataResult.OPCCPX_E_FILTER_INVALID);
    }

    [Test]
    public async Task ValidateNewFilter_DuplicateName_ReturnsFilterDuplicate()
    {
        var result = OpcCpxDataFilter.ValidateNewFilter("Filter01", "Status == Good", new[] { "Filter01" });

        await Assert.That(result).IsEqualTo(OpcComplexDataResult.OPCCPX_E_FILTER_DUPLICATE);
    }

    private static TypeDescription CreateType() => new(
        "StatusType",
        "StatusType",
        TypeKind.StructReference,
        true,
        new[]
        {
            new TypeField("Status", TypeKind.String),
            new TypeField("Count", TypeKind.Int32),
        });

    private static ComplexValue CreateValue(string status, int count) => new()
    {
        Type = new StructType { Name = "StatusType" },
        Fields = new Dictionary<string, object?>
        {
            ["Status"] = status,
            ["Count"] = count,
        },
    };
}
