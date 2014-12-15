public static class StringExtensionMethods
{
    public static string FormatWith(this string expression, params object[] args)
    {
        if (expression.IsNullOrEmpty() || args == null || args.Length < 1)
            return expression;

        return string.Format(expression, args);
    }

    public static bool IsNullOrEmpty(this string s)
    {
        return string.IsNullOrEmpty(s);
    }
}