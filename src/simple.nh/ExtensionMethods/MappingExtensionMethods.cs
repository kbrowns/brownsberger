using System;
using System.Text.RegularExpressions;
using Simple.NH.Mapping;

namespace Simple.NH.ExtensionMethods
{
    public static class MappingExtensionMethods
    {
        /// <summary>
        /// This method will turn camel casing string into a lower case delimited by an underscore.  e.g.:
        ///     FooBarBaz -> foo_bar_baz
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToDbSchemaName(this string value)
        {
            var delimited = value.ToDelimitedBy('_');

            return delimited.IsNullOrEmpty() ? delimited : delimited.ToLower();
        }

        /// <summary>
        /// This method will strip off any known suffixes of a domain entity's name, and run it through the ToDbSchemaName function
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string ToDbSchemaName(this Type type)
        {
            if (type == null)
                return null;

            var name = type.Name;

            var mapping = type.GetAttribute<ClassMappingAttribute>(false);

            if (mapping != null)
            {
                // does the type have a direct mapping of the name?
                name = mapping.TableName ?? name;
            }
            else
            {
                if (type.BaseType != null)
                {
                    // is this part of an inheritance chain?
                    var root = type.BaseType.GetAttribute<InheritanceRootAttribute>(false);

                    if (root != null && root.TableNameFormatExpression != null)
                        name = string.Format(root.TableNameFormatExpression, name);
                }
            }

            if (name.EndsWith("Entity"))
                name = name.Substring(0, name.Length - "Entity".Length);

            if (name.EndsWith("Entity"))
                name = name.Substring(0, name.Length - "Entity".Length);

            return name.ToDbSchemaName();
        }

        /// <summary>
        /// Will get the FK name part for this type and look for any ForeignKeyAliasAttribute occurences.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetForeignKeyNamePart(this Type type)
        {
            var alias = type.GetAttribute<ForeignKeyAliasAttribute>();

            if (alias == null)
                return type.ToDbSchemaName();

            return alias.Alias;
        }

        /// <summary>
        /// This method will turn camel casing string into a lower case delimited by the specified value.  e.g.:
        ///     FooBarBaz -> foo_bar_baz
        /// </summary>
        /// <param name="value"></param>
        /// <param name="delimiter">Whatever</param>
        /// <returns></returns>
        public static string ToDelimitedBy(this string value, char delimiter)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            string result = Regex.Replace(value, "([A-Z])", string.Concat(delimiter, "$1"), RegexOptions.Compiled);

            if (!string.IsNullOrEmpty(result) && result[0] == delimiter)
                result = result.Substring(1);

            return result;
        }
    }

}
