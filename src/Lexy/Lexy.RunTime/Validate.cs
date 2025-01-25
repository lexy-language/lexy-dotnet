using System;
using System.Collections.Generic;

namespace Lexy.RunTime;

public static class Validate
{
    public static bool String(string name, object value, bool optional, List<string> validationErrors)
    {
        if (IsMissing(name, value, optional, "string", validationErrors)) return false;
        if (value is not string)
        {
            validationErrors.Add($"'{name}' should have a 'string' value. Invalid type: '{value.GetType().Name}'");
            return false;
        }
        return true;
    }

    public static void Number(string name, object value, bool optional, List<string> validationErrors)
    {
        if (IsMissing(name, value, optional, "string", validationErrors)) return;
        if (value is not int && value is not decimal && value is not float && value is not double)
        {
            validationErrors.Add($"'{name}' should have a 'number' value. Invalid type: '{value.GetType().Name}'");
        }
    }

    public static void Boolean(string name, object value, bool optional, List<string> validationErrors)
    {
        if (IsMissing(name, value, optional, "string", validationErrors)) return;
        if (value is not bool)
        {
            validationErrors.Add($"'{name}' should have a 'boolean' value. Invalid type: '{value.GetType().Name}'");
        }
    }

    public static void Date(string name, object value, bool optional, List<string> validationErrors)
    {
        if (IsMissing(name, value, optional, "string", validationErrors)) return;
        if (value is not DateTime)
        {
            validationErrors.Add($"'{name}' should have a 'date' value. Invalid type: '{value.GetType().Name}'");
        }
    }

    public static bool IsMissing(string name, object value, bool optional, string type, List<string> validationErrors) {
        if (value != null) return false;
        if (!optional)
        {
            validationErrors.Add($"'{name}' should have a '{type}' value. Value missing.");
        }
        return true;
    }

}