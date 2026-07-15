// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Globalization;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Cpx.Tests;

public sealed class OpcCpxDataFilterTests
{
    [Test]
    public async Task ReferenceFilter_ImplementsPublicContractAndExposesBounds()
    {
        IOpcCpxDataFilter filter = new OpcCpxReferenceDataFilter();
        var value = CreateValue();

        var result = filter.Apply(value, CreateType(), "Status == Good");

        await Assert.That(result.Error).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(result.Value).IsSameReferenceAs(value);
        await Assert.That(RuntimeValue(OpcCpxReferenceDataFilter.MaxExpressionLength)).IsEqualTo(4_096);
        await Assert.That(RuntimeValue(OpcCpxReferenceDataFilter.MaxNestingDepth)).IsEqualTo(32);
        await Assert.That(RuntimeValue(OpcCpxReferenceDataFilter.MaxComparisons)).IsEqualTo(128);
        await Assert.That(RuntimeValue(OpcCpxReferenceDataFilter.MaxPathSegments)).IsEqualTo(32);
        await Assert.That(RuntimeValue(OpcCpxReferenceDataFilter.MaxLiteralLength)).IsEqualTo(1_024);
    }

    [Test]
    [Arguments("Status = Good", true)]
    [Arguments("Status == Good", true)]
    [Arguments("Status != Bad", true)]
    [Arguments("Status <> Bad", true)]
    [Arguments("Status = Bad", false)]
    [Arguments("Status != Good", false)]
    public async Task Apply_EqualityAndInequalityOperators_ReturnExpectedMatch(
        string expression,
        bool expectedMatch)
    {
        var value = CreateValue();

        var result = OpcCpxDataFilter.Apply(value, CreateType(), expression);

        await AssertMatch(result, value, expectedMatch);
    }

    [Test]
    [Arguments("Count < 4", true)]
    [Arguments("Count <= 3", true)]
    [Arguments("Count > 2", true)]
    [Arguments("Count >= 3", true)]
    [Arguments("Count < 3", false)]
    [Arguments("Count <= 2", false)]
    [Arguments("Count > 3", false)]
    [Arguments("Count >= 4", false)]
    public async Task Apply_OrderingOperators_ReturnExpectedMatch(
        string expression,
        bool expectedMatch)
    {
        var value = CreateValue();

        var result = OpcCpxDataFilter.Apply(value, CreateType(), expression);

        await AssertMatch(result, value, expectedMatch);
    }

    [Test]
    public async Task Apply_QuotedStringsEscapesBooleansNullDatesAndGuids_AreTyped()
    {
        var id = Guid.Parse("9fc5185d-b686-4d48-93f2-344113db0feb");
        var timestamp = new DateTime(2026, 7, 14, 18, 30, 0, DateTimeKind.Utc);
        var value = CreateValue(
            status: "Operator's panel",
            enabled: true,
            optional: null,
            timestamp: timestamp,
            id: id);

        string[] matches =
        [
            "Status = 'Operator''s panel'",
            "Enabled = TRUE",
            "Optional = null",
            "Timestamp >= '2026-07-14T18:30:00Z'",
            "Id = 9fc5185d-b686-4d48-93f2-344113db0feb",
        ];

        foreach (var expression in matches)
        {
            var result = OpcCpxDataFilter.Apply(value, CreateType(), expression);
            await Assert.That(result.Error).IsEqualTo(OpcResultId.Ok.Code);
            await Assert.That(result.Value).IsSameReferenceAs(value);
        }

        var quotedNull = OpcCpxDataFilter.Apply(value, CreateType(), "Optional = 'null'");
        await Assert.That(quotedNull.Error).IsEqualTo(OpcComplexDataResult.OPCCPX_S_FILTER_NO_DATA);
    }

    [Test]
    public async Task Apply_BooleanComposition_UsesAndBeforeOrAndHonorsParentheses()
    {
        var value = CreateValue(status: "Bad", count: 3, enabled: true);

        var precedence = OpcCpxDataFilter.Apply(
            value,
            CreateType(),
            "Status = Good OR Count = 3 AND Enabled = true");
        var parenthesizedMiss = OpcCpxDataFilter.Apply(
            value,
            CreateType(),
            "(Status = Good OR Count = 3) AND Enabled = false");
        var parenthesizedMatch = OpcCpxDataFilter.Apply(
            value,
            CreateType(),
            "(Status = Good OR Count = 3) AND (Enabled = true)");
        var symbolic = OpcCpxDataFilter.Apply(
            value,
            CreateType(),
            "Status = Bad && (Count = 2 || Enabled = true)");

        await Assert.That(precedence.Error).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(parenthesizedMiss.Error).IsEqualTo(OpcComplexDataResult.OPCCPX_S_FILTER_NO_DATA);
        await Assert.That(parenthesizedMatch.Error).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(symbolic.Error).IsEqualTo(OpcResultId.Ok.Code);
    }

    [Test]
    public async Task Apply_NestedAndBracketedFieldPaths_ResolveWithoutReflection()
    {
        var details = CreateComplexValue(
            "DetailsType",
            new Dictionary<string, object?>
            {
                ["State"] = "Running",
                ["Status Code"] = 7,
                ["A]B"] = "escaped",
            });
        var value = CreateValue(details: details);

        string[] expressions =
        [
            "Details.State = Running",
            "[Details].[Status Code] = 7",
            "[Details].[A]]B] = escaped",
            "[Status] = Good",
        ];

        foreach (var expression in expressions)
        {
            var result = OpcCpxDataFilter.Apply(value, CreateType(), expression);
            await Assert.That(result.Error).IsEqualTo(OpcResultId.Ok.Code);
        }
    }

    [Test]
    public async Task Apply_FalseAndAndTrueOr_ShortCircuitRuntimeErrors()
    {
        var value = CreateValue();

        var falseAnd = OpcCpxDataFilter.Apply(value, CreateType(), "Status = Bad AND Missing = 1");
        var trueOr = OpcCpxDataFilter.Apply(value, CreateType(), "Status = Good OR Missing = 1");
        var trueAnd = OpcCpxDataFilter.Apply(value, CreateType(), "Status = Good AND Missing = 1");
        var falseOr = OpcCpxDataFilter.Apply(value, CreateType(), "Status = Bad OR Missing = 1");

        await Assert.That(falseAnd.Error).IsEqualTo(OpcComplexDataResult.OPCCPX_S_FILTER_NO_DATA);
        await Assert.That(trueOr.Error).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(trueAnd.Error).IsEqualTo(OpcComplexDataResult.OPCCPX_E_FILTER_ERROR);
        await Assert.That(falseOr.Error).IsEqualTo(OpcComplexDataResult.OPCCPX_E_FILTER_ERROR);
    }

    [Test]
    [Arguments("Missing = 1")]
    [Arguments("status = Good")]
    [Arguments("Count = not-a-number")]
    [Arguments("Enabled < true")]
    [Arguments("Id > 9fc5185d-b686-4d48-93f2-344113db0feb")]
    [Arguments("Payload = bytes")]
    [Arguments("Details.State = Running")]
    public async Task Apply_WellFormedButUnevaluableComparison_ReturnsFilterError(string expression)
    {
        var value = CreateValue(details: "not-complex");

        var result = OpcCpxDataFilter.Apply(value, CreateType(), expression);

        await Assert.That(result.Error).IsEqualTo(OpcComplexDataResult.OPCCPX_E_FILTER_ERROR);
        await Assert.That(result.Value).IsNull();
    }

    [Test]
    [Arguments("")]
    [Arguments(" ")]
    [Arguments("Status Good")]
    [Arguments("= Good")]
    [Arguments("Status =")]
    [Arguments("(Status = Good")]
    [Arguments("Status = Good)")]
    [Arguments("()")]
    [Arguments("Status = 'Good")]
    [Arguments("Status = Good AND")]
    [Arguments("AND Status = Good")]
    [Arguments("Status === Good")]
    [Arguments("Status = Good Count = 3")]
    [Arguments("[Status = Good")]
    [Arguments("Status..Code = Good")]
    [Arguments("Status = Good & Count = 3")]
    [Arguments("Status = Good OR OR Count = 3")]
    [Arguments("Status = Good, Count = 3")]
    public async Task Apply_MalformedSyntax_ReturnsFilterInvalid(string expression)
    {
        var result = OpcCpxDataFilter.Apply(CreateValue(), CreateType(), expression);

        await Assert.That(result.Error).IsEqualTo(OpcComplexDataResult.OPCCPX_E_FILTER_INVALID);
        await Assert.That(result.Value).IsNull();
    }

    [Test]
    [Arguments("Status LIKE 'Good'")]
    [Arguments("Status IN ('Good', 'Bad')")]
    [Arguments("NOT Status = Good")]
    [Arguments("Status IS NULL")]
    [Arguments("lower(Status) = good")]
    [Arguments("Count + 1 = 4")]
    [Arguments("Status ~= Good")]
    [Arguments("Status =~ /Good/")]
    [Arguments("vendor:match(Status, Good)")]
    public async Task Apply_UnsupportedOrVendorSyntax_ReturnsFilterInvalid(string expression)
    {
        var result = OpcCpxDataFilter.Apply(CreateValue(), CreateType(), expression);

        await Assert.That(result.Error).IsEqualTo(OpcComplexDataResult.OPCCPX_E_FILTER_INVALID);
    }

    [Test]
    public async Task Apply_FalseResult_ReturnsEmptyValueWithSuccessfulNoDataHResult()
    {
        var value = CreateValue();

        var result = OpcCpxDataFilter.Apply(value, CreateType(), "Status = Bad");

        await Assert.That(result.Error).IsEqualTo(OpcComplexDataResult.OPCCPX_S_FILTER_NO_DATA);
        await Assert.That(result.Value).IsNotNull();
        await Assert.That(result.Value!.Type.Name).IsEqualTo("StatusType");
        await Assert.That(result.Value.Fields).IsEmpty();
    }

    [Test]
    public async Task ValidateNewFilter_ValidatesSyntaxAndChecksDuplicateFirst()
    {
        var valid = OpcCpxDataFilter.ValidateNewFilter(
            "Filter01",
            "(Status = Good OR Count >= 3) AND Enabled = true",
            Array.Empty<string>());
        var invalid = OpcCpxDataFilter.ValidateNewFilter(
            "Filter01",
            "Status LIKE Good",
            Array.Empty<string>());
        var duplicate = OpcCpxDataFilter.ValidateNewFilter(
            "Filter01",
            "not valid",
            ["Filter01"]);

        await Assert.That(valid).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(invalid).IsEqualTo(OpcComplexDataResult.OPCCPX_E_FILTER_INVALID);
        await Assert.That(duplicate).IsEqualTo(OpcComplexDataResult.OPCCPX_E_FILTER_DUPLICATE);
    }

    [Test]
    public async Task PublicContracts_KeepExistingArgumentValidation()
    {
        IOpcCpxDataFilter filter = new OpcCpxReferenceDataFilter();
        var value = CreateValue();
        var type = CreateType();

        await Assert.That(() => filter.Apply(null!, type, "Status = Good")).Throws<ArgumentNullException>();
        await Assert.That(() => filter.Apply(value, null!, "Status = Good")).Throws<ArgumentNullException>();
        await Assert.That(() => filter.ValidateNewFilter("", "Status = Good", [])).Throws<ArgumentException>();
        await Assert.That(() => filter.ValidateNewFilter("Name", "Status = Good", null!))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Apply_ExpressionLengthLimit_AcceptsBoundaryAndRejectsOnePast()
    {
        var value = CreateValue();
        var valid = "Status = Good".PadRight(OpcCpxReferenceDataFilter.MaxExpressionLength);
        var invalid = valid + " ";

        var boundary = OpcCpxDataFilter.Apply(value, CreateType(), valid);
        var overLimit = OpcCpxDataFilter.Apply(value, CreateType(), invalid);

        await Assert.That(boundary.Error).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(overLimit.Error).IsEqualTo(OpcComplexDataResult.OPCCPX_E_FILTER_INVALID);
    }

    [Test]
    public async Task Apply_LiteralLengthLimit_AcceptsBoundaryAndRejectsOnePast()
    {
        var boundaryText = new string('x', OpcCpxReferenceDataFilter.MaxLiteralLength);
        var value = CreateValue(status: boundaryText);

        var boundary = OpcCpxDataFilter.Apply(value, CreateType(), $"Status = '{boundaryText}'");
        var overLimit = OpcCpxDataFilter.Apply(value, CreateType(), $"Status = '{boundaryText}x'");

        await Assert.That(boundary.Error).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(overLimit.Error).IsEqualTo(OpcComplexDataResult.OPCCPX_E_FILTER_INVALID);
    }

    [Test]
    public async Task Apply_ParenthesisDepthLimit_AcceptsBoundaryAndRejectsOnePast()
    {
        var value = CreateValue();
        var boundary = Parenthesize("Status = Good", OpcCpxReferenceDataFilter.MaxNestingDepth);
        var overLimit = Parenthesize("Status = Good", OpcCpxReferenceDataFilter.MaxNestingDepth + 1);

        var boundaryResult = OpcCpxDataFilter.Apply(value, CreateType(), boundary);
        var overLimitResult = OpcCpxDataFilter.Apply(value, CreateType(), overLimit);

        await Assert.That(boundaryResult.Error).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(overLimitResult.Error).IsEqualTo(OpcComplexDataResult.OPCCPX_E_FILTER_INVALID);
    }

    [Test]
    public async Task Apply_ComparisonCountLimit_AcceptsBoundaryAndRejectsOnePast()
    {
        var value = CreateValue();
        var boundary = string.Join(
            " AND ",
            Enumerable.Repeat("Enabled = true", OpcCpxReferenceDataFilter.MaxComparisons));
        var overLimit = boundary + " AND Enabled = true";

        var boundaryResult = OpcCpxDataFilter.Apply(value, CreateType(), boundary);
        var overLimitResult = OpcCpxDataFilter.Apply(value, CreateType(), overLimit);

        await Assert.That(boundaryResult.Error).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(overLimitResult.Error).IsEqualTo(OpcComplexDataResult.OPCCPX_E_FILTER_INVALID);
    }

    [Test]
    public async Task Apply_PathSegmentLimit_AcceptsBoundaryAndRejectsOnePast()
    {
        var value = CreateNestedPathValue(OpcCpxReferenceDataFilter.MaxPathSegments, 7);
        var type = new TypeDescription(
            "Root",
            "Root",
            TypeKind.StructReference,
            true,
            [new TypeField("P0", TypeKind.StructReference)]);
        var boundaryPath = string.Join(
            ".",
            Enumerable.Range(0, OpcCpxReferenceDataFilter.MaxPathSegments).Select(static i => $"P{i}"));
        var overLimitPath = boundaryPath + ".P32";

        var boundary = OpcCpxDataFilter.Apply(value, type, $"{boundaryPath} = 7");
        var overLimit = OpcCpxDataFilter.Apply(value, type, $"{overLimitPath} = 7");

        await Assert.That(boundary.Error).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(overLimit.Error).IsEqualTo(OpcComplexDataResult.OPCCPX_E_FILTER_INVALID);
    }

    [Test]
    public async Task Apply_IntegralRepresentativeValues_ObeyEqualityAndOrderingProperties()
    {
        for (var count = -32; count <= 32; count++)
        {
            var value = CreateValue(count: count);
            var equality = OpcCpxDataFilter.Apply(value, CreateType(), $"Count = {count}");
            var lower = OpcCpxDataFilter.Apply(value, CreateType(), $"Count > {count - 1}");
            var upper = OpcCpxDataFilter.Apply(value, CreateType(), $"Count < {count + 1}");
            var different = OpcCpxDataFilter.Apply(value, CreateType(), $"Count != {count}");

            await Assert.That(equality.Error).IsEqualTo(OpcResultId.Ok.Code);
            await Assert.That(lower.Error).IsEqualTo(OpcResultId.Ok.Code);
            await Assert.That(upper.Error).IsEqualTo(OpcResultId.Ok.Code);
            await Assert.That(different.Error).IsEqualTo(OpcComplexDataResult.OPCCPX_S_FILTER_NO_DATA);
        }
    }

    [Test]
    [NotInParallel]
    public async Task Apply_NumericParsing_IsInvariantToCurrentCulture()
    {
        var previousCulture = CultureInfo.CurrentCulture;
        var previousUiCulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("de-DE");

            var invariant = OpcCpxDataFilter.Apply(CreateValue(count: 1234), CreateType(), "Count = 1234");
            var cultureFormatted = OpcCpxDataFilter.Apply(CreateValue(count: 1234), CreateType(), "Count = 1.234");

            await Assert.That(invariant.Error).IsEqualTo(OpcResultId.Ok.Code);
            await Assert.That(cultureFormatted.Error).IsEqualTo(OpcComplexDataResult.OPCCPX_E_FILTER_ERROR);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousUiCulture;
        }
    }

    private static async Task AssertMatch(
        OpcCpxFilterResult result,
        ComplexValue original,
        bool expectedMatch)
    {
        if (expectedMatch)
        {
            await Assert.That(result.Error).IsEqualTo(OpcResultId.Ok.Code);
            await Assert.That(result.Value).IsSameReferenceAs(original);
        }
        else
        {
            await Assert.That(result.Error).IsEqualTo(OpcComplexDataResult.OPCCPX_S_FILTER_NO_DATA);
            await Assert.That(result.Value!.Fields).IsEmpty();
        }
    }

    private static string Parenthesize(string expression, int depth) =>
        new string('(', depth) + expression + new string(')', depth);

    private static T RuntimeValue<T>(T value) => value;

    private static ComplexValue CreateNestedPathValue(int segmentCount, object? leaf)
    {
        object? current = leaf;
        for (var index = segmentCount - 1; index >= 0; index--)
        {
            current = CreateComplexValue(
                $"Level{index}",
                new Dictionary<string, object?> { [$"P{index}"] = current });
        }

        return (ComplexValue)current!;
    }

    private static TypeDescription CreateType() => new(
        "StatusType",
        "StatusType",
        TypeKind.StructReference,
        true,
        [
            new TypeField("Status", TypeKind.String),
            new TypeField("Count", TypeKind.Int32),
            new TypeField("Enabled", TypeKind.Boolean),
            new TypeField("Optional", TypeKind.String),
            new TypeField("Timestamp", TypeKind.FileTime),
            new TypeField("Id", TypeKind.Guid),
            new TypeField("Details", TypeKind.StructReference, "DetailsType"),
            new TypeField("Payload", TypeKind.Blob),
        ]);

    private static ComplexValue CreateValue(
        string status = "Good",
        int count = 3,
        bool enabled = true,
        string? optional = "present",
        DateTime? timestamp = null,
        Guid? id = null,
        object? details = null) =>
        CreateComplexValue(
            "StatusType",
            new Dictionary<string, object?>
            {
                ["Status"] = status,
                ["Count"] = count,
                ["Enabled"] = enabled,
                ["Optional"] = optional,
                ["Timestamp"] = timestamp ?? new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                ["Id"] = id ?? Guid.Parse("9fc5185d-b686-4d48-93f2-344113db0feb"),
                ["Details"] = details,
                ["Payload"] = new byte[] { 1, 2, 3 },
            });

    private static ComplexValue CreateComplexValue(
        string typeName,
        IReadOnlyDictionary<string, object?> fields) => new()
        {
            Type = new StructType { Name = typeName },
            Fields = fields,
        };
}
