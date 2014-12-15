using System;

public static class ObjectExtensionMethods
{
    public static T CheckArg<T>(this T arg, string name) where T : class
    {
        if (arg == null)
            throw new ArgumentNullException(name, "Argument named {0} was null when it was expected to not be".FormatWith(name));

        return arg;
    }

    public static string CheckStringArg(this string arg, string name)
    {
        if(arg.IsNullOrEmpty())
            throw new ArgumentNullException(name, "String argument named {0} was null or empty when it was expected to not be".FormatWith(name));

        return arg;
    }
}