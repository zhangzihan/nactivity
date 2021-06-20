#region License

/* 
 * Copyright ?2002-2011 the original author or authors. 
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); 
 * you may not use this file except in compliance with the License. 
 * You may obtain a copy of the License at 
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software 
 * distributed under the License is distributed on an "AS IS" BASIS, 
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
 * See the License for the specific language governing permissions and 
 * limitations under the License. 
 */

#endregion 

#region Imports

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using Spring.Util;

#endregion

namespace Spring.Core.TypeResolution
{
    /// <summary> 
    /// Provides access to a central registry of aliased <see cref="System.Type"/>s.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Simplifies configuration by allowing aliases to be used instead of
    /// fully qualified type names.
    /// </p>
    /// <p>
    /// Comes 'pre-loaded' with a number of convenience alias' for the more
    /// common types; an example would be the '<c>int</c>' (or '<c>Integer</c>'
    /// for Visual Basic.NET developers) alias for the <see cref="System.Int32"/>
    /// type.
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    /// <seealso cref="Spring.Context.Support.TypeAliasesSectionHandler"/>
    public sealed class TypeRegistry
    {
        #region Constants

        /// <summary>
        /// Name of the .Net config section that contains Spring.Net type aliases.
        /// </summary>
        private const string TypeAliasesSectionName = "spring/typeAliases";

        /// <summary>
        /// The alias around the 'int' type.
        /// </summary>
        public const string Int32Alias = "int";

        /// <summary>
        /// The alias around the 'Integer' type (Visual Basic.NET style).
        /// </summary>
        public const string Int32AliasVB = "Integer";

        /// <summary>
        /// The alias around the 'int[]' array type.
        /// </summary>
        public const string Int32ArrayAlias = "int[]";

        /// <summary>
        /// The alias around the 'Integer()' array type (Visual Basic.NET style).
        /// </summary>
        public const string Int32ArrayAliasVB = "Integer()";

        /// <summary>
        /// The alias around the 'decimal' type.
        /// </summary>
        public const string DecimalAlias = "decimal";

        /// <summary>
        /// The alias around the 'Decimal' type (Visual Basic.NET style).
        /// </summary>
        public const string DecimalAliasVB = "Decimal";

        /// <summary>
        /// The alias around the 'decimal[]' array type.
        /// </summary>
        public const string DecimalArrayAlias = "decimal[]";

        /// <summary>
        /// The alias around the 'Decimal()' array type (Visual Basic.NET style).
        /// </summary>
        public const string DecimalArrayAliasVB = "Decimal()";

        /// <summary>
        /// The alias around the 'char' type.
        /// </summary>
        public const string CharAlias = "char";

        /// <summary>
        /// The alias around the 'Char' type (Visual Basic.NET style).
        /// </summary>
        public const string CharAliasVB = "Char";

        /// <summary>
        /// The alias around the 'char[]' array type.
        /// </summary>
        public const string CharArrayAlias = "char[]";

        /// <summary>
        /// The alias around the 'Char()' array type (Visual Basic.NET style).
        /// </summary>
        public const string CharArrayAliasVB = "Char()";

        /// <summary>
        /// The alias around the 'long' type.
        /// </summary>
        public const string Int64Alias = "long";

        /// <summary>
        /// The alias around the 'Long' type (Visual Basic.NET style).
        /// </summary>
        public const string Int64AliasVB = "Long";

        /// <summary>
        /// The alias around the 'long[]' array type.
        /// </summary>
        public const string Int64ArrayAlias = "long[]";

        /// <summary>
        /// The alias around the 'Long()' array type (Visual Basic.NET style).
        /// </summary>
        public const string Int64ArrayAliasVB = "Long()";

        /// <summary>
        /// The alias around the 'short' type.
        /// </summary>
        public const string Int16Alias = "short";

        /// <summary>
        /// The alias around the 'Short' type (Visual Basic.NET style).
        /// </summary>
        public const string Int16AliasVB = "Short";

        /// <summary>
        /// The alias around the 'short[]' array type.
        /// </summary>
        public const string Int16ArrayAlias = "short[]";

        /// <summary>
        /// The alias around the 'Short()' array type (Visual Basic.NET style).
        /// </summary>
        public const string Int16ArrayAliasVB = "Short()";

        /// <summary>
        /// The alias around the 'unsigned int' type.
        /// </summary>
        public const string UInt32Alias = "uint";

        /// <summary>
        /// The alias around the 'unsigned long' type.
        /// </summary>
        public const string UInt64Alias = "ulong";

        /// <summary>
        /// The alias around the 'ulong[]' array type.
        /// </summary>
        public const string UInt64ArrayAlias = "ulong[]";

        /// <summary>
        /// The alias around the 'uint[]' array type.
        /// </summary>
        public const string UInt32ArrayAlias = "uint[]";

        /// <summary>
        /// The alias around the 'unsigned short' type.
        /// </summary>
        public const string UInt16Alias = "ushort";

        /// <summary>
        /// The alias around the 'ushort[]' array type.
        /// </summary>
        public const string UInt16ArrayAlias = "ushort[]";

        /// <summary>
        /// The alias around the 'double' type.
        /// </summary>
        public const string DoubleAlias = "double";

        /// <summary>
        /// The alias around the 'Double' type (Visual Basic.NET style).
        /// </summary>
        public const string DoubleAliasVB = "Double";

        /// <summary>
        /// The alias around the 'double[]' array type.
        /// </summary>
        public const string DoubleArrayAlias = "double[]";

        /// <summary>
        /// The alias around the 'Double()' array type (Visual Basic.NET style).
        /// </summary>
        public const string DoubleArrayAliasVB = "Double()";

        /// <summary>
        /// The alias around the 'float' type.
        /// </summary>
        public const string FloatAlias = "float";

        /// <summary>
        /// The alias around the 'Single' type (Visual Basic.NET style).
        /// </summary>
        public const string SingleAlias = "Single";

        /// <summary>
        /// The alias around the 'float[]' array type.
        /// </summary>
        public const string FloatArrayAlias = "float[]";

        /// <summary>
        /// The alias around the 'Single()' array type (Visual Basic.NET style).
        /// </summary>
        public const string SingleArrayAliasVB = "Single()";

        /// <summary>
        /// The alias around the 'DateTime' type.
        /// </summary>
        public const string DateTimeAlias = "DateTime";

        /// <summary>
        /// The alias around the 'DateTime' type (C# style).
        /// </summary>
        public const string DateAlias = "date";

        /// <summary>
        /// The alias around the 'DateTime' type (Visual Basic.NET style).
        /// </summary>
        public const string DateAliasVB = "Date";

        /// <summary>
        /// The alias around the 'DateTime[]' array type.
        /// </summary>
        public const string DateTimeArrayAlias = "DateTime[]";

        /// <summary>
        /// The alias around the 'DateTime[]' array type.
        /// </summary>
        public const string DateTimeArrayAliasCSharp = "date[]";

        /// <summary>
        /// The alias around the 'DateTime()' array type (Visual Basic.NET style).
        /// </summary>
        public const string DateTimeArrayAliasVB = "DateTime()";

        /// <summary>
        /// The alias around the 'bool' type.
        /// </summary>
        public const string BoolAlias = "bool";

        /// <summary>
        /// The alias around the 'Boolean' type (Visual Basic.NET style).
        /// </summary>
        public const string BoolAliasVB = "Boolean";

        /// <summary>
        /// The alias around the 'bool[]' array type.
        /// </summary>
        public const string BoolArrayAlias = "bool[]";

        /// <summary>
        /// The alias around the 'Boolean()' array type (Visual Basic.NET style).
        /// </summary>
        public const string BoolArrayAliasVB = "Boolean()";

        /// <summary>
        /// The alias around the 'string' type.
        /// </summary>
        public const string StringAlias = "string";

        /// <summary>
        /// The alias around the 'string' type (Visual Basic.NET style).
        /// </summary>
        public const string StringAliasVB = "String";

        /// <summary>
        /// The alias around the 'string[]' array type.
        /// </summary>
        public const string StringArrayAlias = "string[]";

        /// <summary>
        /// The alias around the 'string[]' array type (Visual Basic.NET style).
        /// </summary>
        public const string StringArrayAliasVB = "String()";

        /// <summary>
        /// The alias around the 'object' type.
        /// </summary>
        public const string ObjectAlias = "object";

        /// <summary>
        /// The alias around the 'object' type (Visual Basic.NET style).
        /// </summary>
        public const string ObjectAliasVB = "Object";

        /// <summary>
        /// The alias around the 'object[]' array type.
        /// </summary>
        public const string ObjectArrayAlias = "object[]";

        /// <summary>
        /// The alias around the 'object[]' array type (Visual Basic.NET style).
        /// </summary>
        public const string ObjectArrayAliasVB = "Object()";

        /// <summary>
        /// The alias around the 'int?' type.
        /// </summary>
        public const string NullableInt32Alias = "int?";

        /// <summary>
        /// The alias around the 'int?[]' array type.
        /// </summary>
        public const string NullableInt32ArrayAlias = "int?[]";

        /// <summary>
        /// The alias around the 'decimal?' type.
        /// </summary>
        public const string NullableDecimalAlias = "decimal?";

        /// <summary>
        /// The alias around the 'decimal?[]' array type.
        /// </summary>
        public const string NullableDecimalArrayAlias = "decimal?[]";

        /// <summary>
        /// The alias around the 'char?' type.
        /// </summary>
        public const string NullableCharAlias = "char?";

        /// <summary>
        /// The alias around the 'char?[]' array type.
        /// </summary>
        public const string NullableCharArrayAlias = "char?[]";

        /// <summary>
        /// The alias around the 'long?' type.
        /// </summary>
        public const string NullableInt64Alias = "long?";

        /// <summary>
        /// The alias around the 'long?[]' array type.
        /// </summary>
        public const string NullableInt64ArrayAlias = "long?[]";

        /// <summary>
        /// The alias around the 'short?' type.
        /// </summary>
        public const string NullableInt16Alias = "short?";

        /// <summary>
        /// The alias around the 'short?[]' array type.
        /// </summary>
        public const string NullableInt16ArrayAlias = "short?[]";

        /// <summary>
        /// The alias around the 'unsigned int?' type.
        /// </summary>
        public const string NullableUInt32Alias = "uint?";

        /// <summary>
        /// The alias around the 'unsigned long?' type.
        /// </summary>
        public const string NullableUInt64Alias = "ulong?";

        /// <summary>
        /// The alias around the 'ulong?[]' array type.
        /// </summary>
        public const string NullableUInt64ArrayAlias = "ulong?[]";

        /// <summary>
        /// The alias around the 'uint?[]' array type.
        /// </summary>
        public const string NullableUInt32ArrayAlias = "uint?[]";

        /// <summary>
        /// The alias around the 'unsigned short?' type.
        /// </summary>
        public const string NullableUInt16Alias = "ushort?";

        /// <summary>
        /// The alias around the 'ushort?[]' array type.
        /// </summary>
        public const string NullableUInt16ArrayAlias = "ushort?[]";

        /// <summary>
        /// The alias around the 'double?' type.
        /// </summary>
        public const string NullableDoubleAlias = "double?";

        /// <summary>
        /// The alias around the 'double?[]' array type.
        /// </summary>
        public const string NullableDoubleArrayAlias = "double?[]";

        /// <summary>
        /// The alias around the 'float?' type.
        /// </summary>
        public const string NullableFloatAlias = "float?";

        /// <summary>
        /// The alias around the 'float?[]' array type.
        /// </summary>
        public const string NullableFloatArrayAlias = "float?[]";

        /// <summary>
        /// The alias around the 'bool?' type.
        /// </summary>
        public const string NullableBoolAlias = "bool?";

        /// <summary>
        /// The alias around the 'bool?[]' array type.
        /// </summary>
        public const string NullableBoolArrayAlias = "bool?[]";

        public const string NumberUtils = "NumberUtils";

        #endregion

        #region Fields

        private static readonly ConcurrentDictionary<string, Type> types = new();

        #endregion

        #region Constructor (s) / Destructor

        /// <summary>
        /// Registers standard and user-configured type aliases.
        /// </summary>
        static TypeRegistry()
        {
            types.TryAdd("Int32", typeof(Int32));
            types.TryAdd(Int32Alias, typeof(Int32));
            types.TryAdd(Int32AliasVB, typeof(Int32));
            types.TryAdd(Int32ArrayAlias, typeof(Int32[]));
            types.TryAdd(Int32ArrayAliasVB, typeof(Int32[]));

            types.TryAdd("UInt32", typeof(UInt32));
            types.TryAdd(UInt32Alias, typeof(UInt32));
            types.TryAdd(UInt32ArrayAlias, typeof(UInt32[]));

            types.TryAdd("Int16", typeof(Int16));
            types.TryAdd(Int16Alias, typeof(Int16));
            types.TryAdd(Int16AliasVB, typeof(Int16));
            types.TryAdd(Int16ArrayAlias, typeof(Int16[]));
            types.TryAdd(Int16ArrayAliasVB, typeof(Int16[]));

            types.TryAdd("UInt16", typeof(UInt16));
            types.TryAdd(UInt16Alias, typeof(UInt16));
            types.TryAdd(UInt16ArrayAlias, typeof(UInt16[]));

            types.TryAdd("Int64", typeof(Int64));
            types.TryAdd(Int64Alias, typeof(Int64));
            types.TryAdd(Int64AliasVB, typeof(Int64));
            types.TryAdd(Int64ArrayAlias, typeof(Int64[]));
            types.TryAdd(Int64ArrayAliasVB, typeof(Int64[]));

            types.TryAdd("UInt64", typeof(UInt64));
            types.TryAdd(UInt64Alias, typeof(UInt64));
            types.TryAdd(UInt64ArrayAlias, typeof(UInt64[]));

            types.TryAdd(DoubleAlias, typeof(double));
            types.TryAdd(DoubleAliasVB, typeof(double));
            types.TryAdd(DoubleArrayAlias, typeof(double[]));
            types.TryAdd(DoubleArrayAliasVB, typeof(double[]));

            types.TryAdd(FloatAlias, typeof(float));
            types.TryAdd(SingleAlias, typeof(float));
            types.TryAdd(FloatArrayAlias, typeof(float[]));
            types.TryAdd(SingleArrayAliasVB, typeof(float[]));

            types.TryAdd(DateTimeAlias, typeof(DateTime));
            types.TryAdd(DateAlias, typeof(DateTime));
            types.TryAdd(DateAliasVB, typeof(DateTime));
            types.TryAdd(DateTimeArrayAlias, typeof(DateTime[]));
            types.TryAdd(DateTimeArrayAliasCSharp, typeof(DateTime[]));
            types.TryAdd(DateTimeArrayAliasVB, typeof(DateTime[]));

            types.TryAdd(BoolAlias, typeof(bool));
            types.TryAdd(BoolAliasVB, typeof(bool));
            types.TryAdd(BoolArrayAlias, typeof(bool[]));
            types.TryAdd(BoolArrayAliasVB, typeof(bool[]));

            types.TryAdd(DecimalAlias, typeof(decimal));
            types.TryAdd(DecimalAliasVB, typeof(decimal));
            types.TryAdd(DecimalArrayAlias, typeof(decimal[]));
            types.TryAdd(DecimalArrayAliasVB, typeof(decimal[]));

            types.TryAdd(CharAlias, typeof(char));
            types.TryAdd(CharAliasVB, typeof(char));
            types.TryAdd(CharArrayAlias, typeof(char[]));
            types.TryAdd(CharArrayAliasVB, typeof(char[]));

            types.TryAdd(StringAlias, typeof(string));
            types.TryAdd(StringAliasVB, typeof(string));
            types.TryAdd(StringArrayAlias, typeof(string[]));
            types.TryAdd(StringArrayAliasVB, typeof(string[]));

            types.TryAdd(ObjectAlias, typeof(object));
            types.TryAdd(ObjectAliasVB, typeof(object));
            types.TryAdd(ObjectArrayAlias, typeof(object[]));
            types.TryAdd(ObjectArrayAliasVB, typeof(object[]));

            types.TryAdd(NullableInt32Alias, typeof(int?));
            types.TryAdd(NullableInt32ArrayAlias, typeof(int?[]));

            types.TryAdd(NullableDecimalAlias, typeof(decimal?));
            types.TryAdd(NullableDecimalArrayAlias, typeof(decimal?[]));

            types.TryAdd(NullableCharAlias, typeof(char?));
            types.TryAdd(NullableCharArrayAlias, typeof(char?[]));

            types.TryAdd(NullableInt64Alias, typeof(long?));
            types.TryAdd(NullableInt64ArrayAlias, typeof(long?[]));

            types.TryAdd(NullableInt16Alias, typeof(short?));
            types.TryAdd(NullableInt16ArrayAlias, typeof(short?[]));

            types.TryAdd(NullableUInt32Alias, typeof(uint?));
            types.TryAdd(NullableUInt32ArrayAlias, typeof(uint?[]));

            types.TryAdd(NullableUInt64Alias, typeof(ulong?));
            types.TryAdd(NullableUInt64ArrayAlias, typeof(ulong?[]));

            types.TryAdd(NullableUInt16Alias, typeof(ushort?));
            types.TryAdd(NullableUInt16ArrayAlias, typeof(ushort?[]));

            types.TryAdd(NullableDoubleAlias, typeof(double?));
            types.TryAdd(NullableDoubleArrayAlias, typeof(double?[]));

            types.TryAdd(NullableFloatAlias, typeof(float?));
            types.TryAdd(NullableFloatArrayAlias, typeof(float?[]));

            types.TryAdd(NullableBoolAlias, typeof(bool?));
            types.TryAdd(NullableBoolArrayAlias, typeof(bool?[]));

            types.TryAdd(NumberUtils, typeof(NumberUtils));

            // register user-configured type aliases
            ConfigurationUtils.GetSection(TypeAliasesSectionName);
        }

        #endregion

        /// <summary> 
        /// Registers an alias for the specified <see cref="System.Type"/>. 
        /// </summary>
        /// <remarks>
        /// <p>
        /// This overload does eager resolution of the <see cref="System.Type"/>
        /// referred to by the <paramref name="typeName"/> parameter. It will throw a
        /// <see cref="System.TypeLoadException"/> if the <see cref="System.Type"/> referred
        /// to by the <paramref name="typeName"/> parameter cannot be resolved.
        /// </p>
        /// </remarks>
        /// <param name="alias">
        /// A string that will be used as an alias for the specified
        /// <see cref="System.Type"/>.
        /// </param>
        /// <param name="typeName">
        /// The (possibly partially assembly qualified) name of the
        /// <see cref="System.Type"/> to register the alias for.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// If either of the supplied parameters is <see langword="null"/> or
        /// contains only whitespace character(s).
        /// </exception>
        /// <exception cref="System.TypeLoadException">
        /// If the <see cref="System.Type"/> referred to by the supplied
        /// <paramref name="typeName"/> cannot be loaded.
        /// </exception>
        public static void RegisterType(string alias, string typeName)
        {
            AssertUtils.ArgumentHasText(alias, "alias");
            AssertUtils.ArgumentHasText(typeName, "typeName");

            Type type = TypeResolutionUtils.ResolveType(typeName);

            if (type.IsGenericTypeDefinition)
                alias += ("`" + type.GetGenericArguments().Length);

            RegisterType(alias, type);
        }

        /// <summary> 
        /// Registers short type name as an alias for 
        /// the supplied <see cref="System.Type"/>. 
        /// </summary> 
        /// <param name="type">
        /// The <see cref="System.Type"/> to register.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="type"/> is <see langword="null"/>.
        /// </exception>
        public static void RegisterType(Type type)
        {
            AssertUtils.ArgumentNotNull(type, "type");
            types.TryAdd(type.Name, type);
        }

        /// <summary> 
        /// Registers an alias for the supplied <see cref="System.Type"/>. 
        /// </summary> 
        /// <param name="alias">
        /// The alias for the supplied <see cref="System.Type"/>.
        /// </param>
        /// <param name="type">
        /// The <see cref="System.Type"/> to register the supplied <paramref name="alias"/> under.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="type"/> is <see langword="null"/>; or if
        /// the supplied <paramref name="alias"/> is <see langword="null"/> or
        /// contains only whitespace character(s).
        /// </exception>
        public static void RegisterType(string alias, Type type)
        {
            AssertUtils.ArgumentHasText(alias, "alias");
            AssertUtils.ArgumentNotNull(type, "type");
            types.TryAdd(alias, type);
        }

        /// <summary> 
        /// Resolves the supplied <paramref name="alias"/> to a <see cref="System.Type"/>. 
        /// </summary> 
        /// <param name="alias">
        /// The alias to resolve.
        /// </param>
        /// <returns>
        /// The <see cref="System.Type"/> the supplied <paramref name="alias"/> was
        /// associated with, or <see lang="null"/> if no <see cref="System.Type"/> 
        /// was previously registered for the supplied <paramref name="alias"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="alias"/> is <see langword="null"/> or
        /// contains only whitespace character(s).
        /// </exception>
        public static Type ResolveType(string alias)
        {
            AssertUtils.ArgumentHasText(alias, "alias");
            types.TryGetValue(alias, out Type type);
            return type;
        }

        /// <summary>
        /// Returns a flag specifying whether <b>TypeRegistry</b> contains
        /// specified alias or not.
        /// </summary>
        /// <param name="alias">
        /// Alias to check.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified type alias is registered, 
        /// <c>false</c> otherwise.
        /// </returns>
        public static bool ContainsAlias(string alias)
        {
            return types.ContainsKey(alias);
        }
    }
}