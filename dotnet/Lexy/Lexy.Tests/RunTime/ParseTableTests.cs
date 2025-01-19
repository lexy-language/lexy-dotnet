using System;
using System.Collections.Generic;
using Lexy.RunTime;
using NUnit.Framework;

namespace Lexy.Tests.RunTime;

public class ValidationTests : ScopedServicesTestFixture
{
    [Test]
    public void OptionalVariablesCanBeNull()
    {
        var errors = new List<string>();
        Validate.String("param", null, true, errors);
        CheckError(null, errors);
    }

    [Test]
    [TestCaseSource(nameof(StringValidationResultCases))]
    public void StringValidationResult(object value, string error)
    {
        var errors = new List<string>();
        Validate.String("param", value, false, errors);
        CheckError(error, errors);
    }

    private static object[] StringValidationResultCases =
    {
        new object[] { "", null },
        new object[] { "aaa", null },
        new object[] { "abc", null },
        new object[] { 5, "'param' should have a 'string' value. Invalid type: " },
        new object[] { false, "'param' should have a 'string' value. Invalid type: " },
        new object[] { new DateTime(), "'param' should have a 'string' value. Invalid type: " },
    };

    [Test]
    [TestCaseSource(nameof(NumberValidationResultCases))]
    public void NumberValidationResult(object value, string error)
    {
        var errors = new List<string>();
        Validate.Number("param", value, false, errors);
        CheckError(error, errors);
    }

    private static object[] NumberValidationResultCases =
    {
        new object[] { 5, null },
        new object[] { 55.66, null },
        new object[] { -486.87, null },
        new object[] { "5", "'param' should have a 'number' value. Invalid type: " },
        new object[] { false, "'param' should have a 'number' value. Invalid type: " },
        new object[] { new DateTime(), "'param' should have a 'number' value. Invalid type: " },
    };

    [Test]
    [TestCaseSource(nameof(BooleanValidationResultCases))]
    public void BooleanValidationResult(object value, string error)
    {
        var errors = new List<string>();
        Validate.Boolean("param", value, false, errors);
        CheckError(error, errors);
    }

    private static object[] BooleanValidationResultCases =
    {
        new object[] { true, null },
        new object[] { false, null },
        new object[] { "true", "'param' should have a 'boolean' value. Invalid type: " },
        new object[] { "false", "'param' should have a 'boolean' value. Invalid type: " },
        new object[] { DateTime.Now, "'param' should have a 'boolean' value. Invalid type: " },
        new object[] { 123, "'param' should have a 'boolean' value. Invalid type: " },
    };

    [Test]
    [TestCaseSource(nameof(DateValidationResultCases))]
    public void DateValidationResult(object value, string error)
    {
        var errors = new List<string>();
        Validate.Date("param", value, false, errors);
        CheckError(error, errors);
    }

    private static object[] DateValidationResultCases =
    {
        new object[] { new DateTime(), null },
        new object[] { new DateTime(2024, 12, 31, 12, 12, 13), null },
        new object[] { "2024/12/31 12:12:13", "'param' should have a 'date' value. Invalid type: " },
        new object[] { false, "'param' should have a 'date' value. Invalid type: " },
        new object[] { 123, "'param' should have a 'date' value. Invalid type: " },
    };

    private void CheckError(string error, List<string> errors)
    {
        if (error == null)
        {
            if (errors.Count != 0)
            {
                throw new InvalidOperationException("No error expected: " + errors[0]);
            }
        }
        else
        {
            if (errors.Count != 1 && !errors[0].StartsWith(error))
            {
                throw new InvalidOperationException("Invalid error: " + errors[0]);
            }
        }
    }
}