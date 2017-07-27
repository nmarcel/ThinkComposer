// -------------------------------------------------------------------------------------------
// Instrumind ThinkComposer
//
// Copyright (C) 2011-2015 Néstor Marcel Sánchez Ahumada.
// http://thinkcomposer.codeplex.com
//
// This file is part of ThinkComposer, which is free software licensed under the GNU General Public License.
// It is provided without any warranty. You should find a copy of the license in the root directory of this software product.
// -------------------------------------------------------------------------------------------
//
// Project: Instrumind ThinkComposer v1.0
// File   : General.cs
// Object : Instrumind.Common.General (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.06.23 Néstor Sánchez A.  Creation
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Documents;

using Microsoft.Win32;

using Instrumind.Common.EntityBase;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /// <summary>
    /// Provides common features such as encryption, serialization, strings handling extensions and global constants.
    /// </summary>
    public static partial class General
    {
        /* IMPORTANT: FOR FILE-ACCESS USING LONG PATHS SEE...
         * http://blogs.msdn.com/bclteam/archive/2007/02/13/long-paths-in-net-part-1-of-3-kim-hamilton.aspx
         * http://bcl.codeplex.com/wikipage?title=Long%20Path&referringTitle=Home
         */

        /// <summary>
        /// Static Constructor.
        /// </summary>
        static General()
        {
            // Registers non-serializable types for Store-Box storage.
            StoreBox.RegisterStorableType<MModelPropertyDefinitor>(ConvertByteArrayToMModelPropertyDefinitor, ConvertMModelPropertyDefinitorToByteArray);

            // If using the NetOffice library: Initialize Api COMObject Support (for MS Office interop)
            // LateBindingApi.Core.Factory.Initialize();
        }

        /// <summary>
        /// General string symbol representing an unspecified, undefined or unknown value.
        /// </summary>
        public const string UNSPECIFIED = "?";

        /// <summary>
        /// Code for ownership of local (exclusive, non-shared) access.
        /// </summary>
        public const string OWNERSHIP_LOCAL = "L";

        /// <summary>
        /// Code for ownership of global (shared collectively) access.
        /// </summary>
        public const string OWNERSHIP_GLOBAL = "G";

        /// <summary>
        /// Set of digits used for representing numbers from base 2 to base 64.
        /// </summary>
        public const string DIGITS_BASE2TOBASE64 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz$&";

        /// <summary>
        /// Set of digits used for representing numbers from base 2 to base 64 without vocals.
        /// </summary>
        public const string DIGITS_BASE2TOBASE64_NOVOCALS = "0123456789BCDFGHJKLMNPQRSTVWXYZbcdfghjklmnpqrstvwxyz$&!@+-*.,:=%";

        /// <summary>
        /// Absent date representation.
        /// </summary>
        public static readonly DateTime EMPTY_DATE = new DateTime(1, 1, 1);

        /// <summary>
        /// Logic value for True, taken by default when represented as text.
        /// </summary>
        public const string LOGIC_VALUE_TRUE = "Y";

        /// <summary>
        /// Logic value for False, taken by default when represented as text.
        /// </summary>
        public const string LOGIC_VALUE_FALSE = "N";

        /// <summary>
        /// Null reference indicator.
        /// </summary>
        public const string STR_NULLREF = "<NOTHING>";

        /// <summary>
        /// Simple separation string.
        /// </summary>
        public const string STR_SEPARATOR = "~";

        /// <summary>
        /// Default text delimiter for data export/import.
        /// </summary>
        public const string STR_DATA_DELIMITER = ";";

        /// <summary>
        /// String for signaling a modification applied.
        /// Used commonly as prefix or suffix for unsaved document names.
        /// </summary>
        public const string STR_SIGNAL_MODIFICATION = "*";

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Language Code: English.
        /// </summary>
        public const string LANG_ENGLISH = "EN";

        /// <summary>
        /// Language Code: Spanish.
        /// </summary>
        public const string LANG_SPANISH = "ES";

        /// <summary>
        /// Translation code separation string.
        /// </summary>
        public const string STR_TRLCOD_SEPARATOR = "/";

        /// <summary>
        /// Definitions of numeric limits.
        /// </summary>
        private static readonly Dictionary<Type, Tuple<decimal, decimal, byte, byte>> NumericTypesLimits_ = new Dictionary<Type, Tuple<decimal, decimal, byte, byte>>
            { { typeof(byte), Tuple.Create(Convert.ToDecimal(byte.MinValue), Convert.ToDecimal(byte.MaxValue), Convert.ToDecimal(byte.MaxValue).GetIntegerDigits(), (byte)0) },
            { typeof(sbyte), Tuple.Create(Convert.ToDecimal(sbyte.MinValue), Convert.ToDecimal(sbyte.MaxValue), Convert.ToDecimal(sbyte.MaxValue).GetIntegerDigits(), (byte)0) },
            { typeof(short), Tuple.Create(Convert.ToDecimal(short.MinValue), Convert.ToDecimal(short.MaxValue), Convert.ToDecimal(short.MaxValue).GetIntegerDigits(), (byte)0) },
            { typeof(ushort), Tuple.Create(Convert.ToDecimal(ushort.MinValue), Convert.ToDecimal(ushort.MaxValue), Convert.ToDecimal(ushort.MaxValue).GetIntegerDigits(), (byte)0) },
            { typeof(int), Tuple.Create(Convert.ToDecimal(int.MinValue), Convert.ToDecimal(int.MaxValue), Convert.ToDecimal(int.MaxValue).GetIntegerDigits(), (byte)0) },
            { typeof(uint), Tuple.Create(Convert.ToDecimal(uint.MinValue), Convert.ToDecimal(uint.MaxValue), Convert.ToDecimal(uint.MaxValue).GetIntegerDigits(), (byte)0) },
            { typeof(long), Tuple.Create(Convert.ToDecimal(long.MinValue), Convert.ToDecimal(long.MaxValue), Convert.ToDecimal(long.MaxValue).GetIntegerDigits(), (byte)0) },
            { typeof(ulong), Tuple.Create(Convert.ToDecimal(ulong.MinValue), Convert.ToDecimal(ulong.MaxValue), Convert.ToDecimal(ulong.MaxValue).GetIntegerDigits(), (byte)0) },
            { typeof(float), Tuple.Create(Convert.ToDecimal(int.MinValue), Convert.ToDecimal(int.MaxValue), Convert.ToDecimal(int.MaxValue).GetIntegerDigits(), (byte)4) },     // Consider as big as int, losing precision
            { typeof(double), Tuple.Create(Convert.ToDecimal(long.MinValue), Convert.ToDecimal(long.MaxValue), Convert.ToDecimal(long.MaxValue).GetIntegerDigits(), (byte)8) },    // Consider as big as long, losing precision
            { typeof(decimal), Tuple.Create(decimal.MinValue, decimal.MaxValue, decimal.MaxValue.GetIntegerDigits(), (byte)16) } };

        /// <summary>
        /// Collection of limits (min and max values, plus number of integer and decimal digits) for standard numeric types.
        /// </summary>
        public static readonly ReadOnlyDictionary<Type, Tuple<decimal, decimal, byte, byte>> NumericTypesLimits =
                                new ReadOnlyDictionary<Type, Tuple<decimal, decimal, byte, byte>>(General.NumericTypesLimits_);

        /// <summary>
        /// Collection of available languages for user-interface usage.
        /// </summary>
        public static readonly Dictionary<string, string> AvailableLanguages =
            new Dictionary<string, string> { { LANG_ENGLISH, "English" },
                                             { LANG_SPANISH, "Español" } };

        /// <summary>
        /// Gets or sets the current user-interface language.
        /// </summary>
        public static string CurrentLanguage
        {
            get { return CurrentLanguage_; }
            set
            {
                value = value.ToStringAlways().Trim().ToUpper();

                if (!AvailableLanguages.ContainsKey(value))
                {
                    Console.WriteLine("Cannot change to language with the unregistered code '{0}'.", value);
                    return;
                }

                CurrentLanguage_ = value;
            }
        }
        private static string CurrentLanguage_ = LANG_ENGLISH;

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// For this supplied text, defines a set of translations to be used.
        /// The returned text is that matching the Current Language, or the original if none is found.
        /// Sample:
        /// string Message = "Good day".Translation("es/Buen día", "fr/Bon jour", "de/Guten tag");
        /// </summary>
        /// <param name="Original">Original text which is translatable.</param>
        /// <param name="Translations">Defined translations with: Language code, separator and the translation.</param>
        /// <returns>Text matching the Current Language or the original.</returns>
        public static string Translation(this string Original, params string[] Translations)
        {
            if (Translations != null)
                foreach (var Text in Translations)
                {
                    var PosSep = Text.IndexOf(STR_TRLCOD_SEPARATOR);
                    if (PosSep <= 0 || PosSep == Text.Length - 1)
                        continue;

                    var LangCode = Text.GetLeft(PosSep - 1).Trim().ToUpper();
                    if (LangCode == CurrentLanguage)
                        return Text.Substring(PosSep + 1);
                }

            return Original;
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the number of integer digits for this value.
        /// </summary>
        public static byte GetIntegerDigits(this decimal Value)
        {
            decimal TruncatedValue = Math.Truncate(Value);
            var Text = TruncatedValue.ToString(CultureInfo.InvariantCulture);
            return (byte)Text.Length;
        }

        /// <summary>
        /// Gets the number of decimal digits for this value.
        /// </summary>
        public static byte GetDecimalDigits(this decimal Value)
        {
            var TruncatedValue = Math.Truncate(Value);

            if (TruncatedValue == Value)
                return 0;

            var Text = (Value - TruncatedValue).ToString(CultureInfo.InvariantCulture);
            var Decs = (Text.Length - (Text.IndexOf(".") + 1));
            return (byte)Decs;
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Dictionary with the maximum number of days of each month (indexed from 1=january)
        /// </summary>
        public static readonly Dictionary<int, int> DaysOfMonths = new Dictionary<int, int>
                                                                     { {1, 31}, {2, 29}, {3, 31},
                                                                       {4, 30}, {5, 31}, {6, 30},
                                                                       {7, 31}, {8, 31}, {9, 30},
                                                                       {10, 31}, {11, 30}, {12, 31} };

        /// <summary>
        /// Dictonary with the week days (indexed from 0=sunday)
        /// </summary>
        public static readonly Dictionary<int, string> WeekDays = new Dictionary<int, string>
                                                                   { {0, "sunday"}, {1, "monday"}, {2, "tuesday"}, {3, "wednesday"},
                                                                     {4, "thursday"}, {5, "friday"}, {6, "saturday"} };

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Fast way of evaluating true for a supplied bool? / Nullable{bool}
        /// </summary>
        public static bool IsTrue(this Nullable<bool> Condition)
        {
            if (Condition == null || !Condition.HasValue)
                return false;

            return Condition.Value;
        }

        /// <summary>
        /// Fast way of evaluating true for a supplied object which might -or not- be a bool.
        /// </summary>
        public static bool IsTrue(this object PossibleCondition)
        {
            return (PossibleCondition is bool && (bool)PossibleCondition);
        }

        /// <summary>
        /// Detects if an object is a nullable.
        /// </summary>
        public static bool IsNullable(this object Instance)
        {
            if (Instance == null)
                return false;

            var Result = Instance.GetType().Name == typeof(Nullable<>).Name;
            var Res = Instance.GetType() == typeof(Nullable<>);
            return (Result || Res);

            /*
            if (Instance == null)
                return true; // obvious

            var type = typeof(T);
            if (!type.IsValueType)
                return true; // ref-type

            if (Nullable.GetUnderlyingType(type) != null)
                return true; // Nullable<T>

            return false; // value-type */
        }
        
        /// <summary>
        /// Indicates whether the string is absent (null or empty),
        /// plus optionally requiring to trim spaces prior to evaluation.
        /// </summary>
        public static bool IsAbsent(this string Data, bool TrimSpaces = false)
        {
            return (Data == null || (TrimSpaces ? Data.Trim() : Data) == String.Empty);
        }

        /// <summary>
        /// Indicates whether the string is absent (null or empty),
        /// plus optionally requiring a minimum length.
        /// </summary>
        public static bool IsAbsent(this string Data, int MinimumLength)
        {
            return (String.IsNullOrEmpty(Data) || Data.Length < MinimumLength);
        }

        /// <summary>
        /// Returns the supplied default value if the target reference is null, else returns the target.
        /// (It can be used instead of the ?? operator to speed-up code typing while writing long expressions by avoiding extra parenthesis use)
        /// </summary>
        public static TDefault NullDefault<TDefault>(this TDefault Data, TDefault DefaultValue)
            where TDefault : class
        {
            return (Data ?? DefaultValue);
        }

        /// <summary>
        /// Returns the supplied default value if the target nullable value is null, else returns the target value.
        /// </summary>
        public static TDefault NullDefaultTo<TDefault>(this Nullable<TDefault> Data, TDefault DefaultValue)
                where TDefault : struct
        {
            return (Data == null || !Data.HasValue ? DefaultValue : Data.Value);
        }

        /// <summary>
        /// Returns a default value if this double value is not a number (NaN).
        /// If the default value is not supplied, then zero is used.
        /// </summary>
        public static double NaNDefault(this double Value, double DefaultValue = 0.0)
        {
            return (double.IsNaN(Value) ? DefaultValue : Value);
        }

        /// <summary>
        /// Returns this nullable Instant as the specified Default when null/not-present.
        /// </summary>
        public static DateTime AsExistent(this DateTime? Instant, DateTime Default = default(DateTime))
        {
            if (Instant == null || !Instant.HasValue)
                return Default;

            return Instant.Value;
        }

        /// <summary>
        /// For a supplied Source class, returns the value obtained with the provided Extractor.
        /// If the Source class is null, then returns the specified Defaullt-Value.
        /// Intended to avoid writing of 'var x = (y==null ? null : y.z)';
        /// </summary>
        public static TValue Get<TClass,TValue>(this TClass Source, Func<TClass, TValue> Extractor, TValue DefaultValue = default(TValue))
                where TClass : class
        {
            if (Source == null)
                return DefaultValue;

            var Value = Extractor(Source);
            return Value;
        }

        /// <summary>
        /// If this Original value is equal to the Sample one, then the Substitute is returned, else the Original.
        /// </summary>
        public static TValue SubstituteFor<TValue>(this TValue Original, TValue Sample, TValue Substitute)
        {
            if (Original.IsEqual(Sample))
                return Substitute;

            return Original;
        }

        /// <summary>
        /// For an Original value to be returned, if the specified Condition is accomplished then the Substitute value is returned.
        /// </summary>
        public static TValue SubstituteIf<TValue>(this TValue Original, bool Condition, TValue Substitute)
        {
            if (Condition)
                return Substitute;

            return Original;
        }

        /// <summary>
        /// For an Original value to be returned, if the specified Condition-Evaluator result is accomplished then the Substitute value is returned.
        /// </summary>
        public static TValue SubstituteIf<TValue>(this TValue Original, Func<TValue, bool> ConditionEvaluator, TValue Substitute)
        {
            if (ConditionEvaluator != null && ConditionEvaluator(Original))
                return Substitute;

            return Original;
        }

        /// <summary>
        /// Executes, for this supplied Target object, the specified Operation action.
        /// Byte default, the operation is not executed if the Target is null.
        /// Returns the Target object.
        /// </summary>
        public static TTarget Exec<TTarget>(this TTarget Target, Action<TTarget> Operation, bool ExecuteIfNull = false) where TTarget : class
        {
            if (Target != null || ExecuteIfNull)
                Operation(Target);

            return Target;
        }

        /// <summary>
        /// Executes the specified operation, returning a result container, indicating possible failure and error-message on console.
        /// </summary>
        public static OperationResult<TResult> Execute<TResult>(Func<TResult> Operation, string ErrorMessage = null,
                                                                bool ShowInConsole = true, bool AppendDetectedProblem = false)
        {
            OperationResult<TResult> Result = null;

            try
            {
                Result = OperationResult.Success(Operation());
            }
            catch (Exception Problem)
            {
                ErrorMessage = (ErrorMessage.NullDefault("Operation failed.") +
                                (AppendDetectedProblem ? " Problem: " + Problem.Message : "")).Trim();

                if (ShowInConsole)
                    Console.WriteLine(ErrorMessage);

                Result = OperationResult.Failure<TResult>(ErrorMessage);
            }

            return Result;
        }

        /// <summary>
        /// Executes the specified operation, indicating possible failure and error-message on console, returning indication of success.
        /// </summary>
        public static bool Execute(Action Operation, string ErrorMessage = null, bool ShowInConsole = true, bool AppendDetectedProblem = false)
        {
            try
            {
                Operation();
            }
            catch (Exception Problem)
            {
                if (AppendDetectedProblem)
                    ErrorMessage = (ErrorMessage.AbsentDefault() + " Problem: " + Problem.Message).Trim();

                if (ShowInConsole && !ErrorMessage.IsAbsent())
                    Console.WriteLine(ErrorMessage);

                return false;
            }

            return true;
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Indicates whether the supplied Value is a Not-A-Number one.
        /// </summary>
        public static bool IsNan(this double Value)
        {
            return double.IsNaN(Value);

            /*T if (double.IsNaN(Value))
                return true;    // Put breakpoint here to detect the corrected behaviour

            return false; */
        }

        /// <summary>
        /// Indicates whether this supplied Value has decimal part.
        /// </summary>
        public static bool HasDecimals(this double Value)
        {
            var IsDifferent = (Math.Truncate(Value) != Value);
            return IsDifferent;
        }

        /// <summary>
        /// Enforces that this specified double value is equal or less than the supplied maximum limit.
        /// </summary>
        public static double EnforceMaximum(this double Value, double Max)
        {
            return (Value <= Max || Value.IsNan() ? Value : Max);
        }

        /// <summary>
        /// Enforces that this specified double value is equal or greater than the supplied minimum limit.
        /// </summary>
        public static double EnforceMinimum(this double Value, double Min)
        {
            return (Value >= Min || Value.IsNan() ? Value : Min);
        }

        /// <summary>
        /// Enforces that this specified int value is equal or less than the supplied maximum limit.
        /// </summary>
        public static int EnforceMaximum(this int Value, int Max)
        {
            return (Value <= Max ? Value : Max);
        }

        /// <summary>
        /// Enforces that this specified int value is equal or greater than the supplied minimum limit.
        /// </summary>
        public static int EnforceMinimum(this int Value, int Min)
        {
            return (Value >= Min ? Value : Min);
        }

        /// <summary>
        /// Enforces that this specified double value is between the supplied minimum and maximum limits.
        /// </summary>
        public static double EnforceRange(this double Value, double Min, double Max)
        {
            return Math.Max(Min, Math.Min(Max, Value));
        }

        /// <summary>
        /// Enforces that this specified int value is between the supplied minimum and maximum limits.
        /// </summary>
        public static int EnforceRange(this int Value, int Min, int Max)
        {
            return Math.Max(Min, Math.Min(Max, Value));
        }

        /// <summary>
        /// Indicates whether this supplied int value is the range between the two specified minimum and maximum limits (inclusive).
        /// </summary>
        public static bool IsInRange(this int Value, int Min, int Max)
        {
            return (Min <= Value && Value <= Max);
        }

        /// <summary>
        /// Indicates whether this supplied double value is the range between the two specified minimum and maximum limits (inclusive).
        /// </summary>
        public static bool IsInRange(this double Value, double Min, double Max)
        {
            return (Min <= Value && Value <= Max);
        }

        /// <summary>
        /// Indicates whether this supplied int value is close to the target by the specified absolute-range (inclusive).
        /// </summary>
        public static bool IsCloseTo(this int Value, int Target, int AbsoluteRange)
        {
            return Value.IsInRange(Target - AbsoluteRange, Target + AbsoluteRange);
        }

        /// <summary>
        /// Indicates whether this supplied double value is close to the target by the specified absolute-range (inclusive).
        /// </summary>
        public static bool IsCloseTo(this double Value, double Target, double AbsoluteRange)
        {
            return Value.IsInRange(Target - AbsoluteRange, Target + AbsoluteRange);
        }

        /// <summary>
        /// Returns, for this supplied date-time Source Format info, the order of date parts indicated as only three characters of the set YMD (all uppercase).
        /// Example: Format (Short-Date .NET pattern) "MM-dd-yyyy" is "MDY".
        /// </summary>
        public static string DatePartsDisposition(this DateTimeFormatInfo SourceFormat)
        {
            StringBuilder Result = new StringBuilder(3);

            string Source = SourceFormat.ShortDatePattern.ToUpper();
            string Comparer = "YMD";
            int Index = 0;
            while (Index < Source.Length && Comparer.Length > 0)
            {
                var Character = Source[Index];

                if (Comparer.IndexOf(Character) >= 0)
                {
                    Result.Append(Character);
                    Comparer = Comparer.Replace(Character.ToString(), "");
                }

                Index++;
            }

            return Result.ToString();
        }

        /// <summary>
        /// Returns the supplied DateTime in "YYYYMMDD" format.
        /// </summary>
        public static string AsCommonDate(this DateTime Moment)
        {
            return Moment.ToString("yyyyMMdd");
        }

        /// <summary>
        /// Returns the supplied DateTime in "YYYYMMDD HHMMSS" format, optionally including milliseconds.
        /// </summary>
        public static string AsCommonDateTime(this DateTime Moment, bool IncludeMilliseconds=false, bool IncludeSeparators=false)
        {
            string Format = (IncludeSeparators ? "yyyy/MM/dd HH:mm:ss" : "yyyyMMdd HHmmss");

            if (IncludeMilliseconds)
                Format = Format + (IncludeSeparators ? "." : "") + "fff";

            return Moment.ToString(Format);
        }

        /// <summary>
        /// Returns the supplied DateTime in "HHMMSS" format.
        /// </summary>
        public static string AsCommonTime(this DateTime Moment)
        {
            return Moment.ToString("HHmmss");
        }

        /// <summary>
        /// Converts a string in stream.
        /// </summary>
        public static Stream StringToStream(this string Text)
        {
            return (new MemoryStream(System.Text.Encoding.UTF8.GetBytes(Text)));
        }

        /// <summary>
        /// Converts a stream in string.
        /// </summary>
        public static string StreamToString(this Stream Torrent)
        {
            if (Torrent == null)
                return null;

            if (Torrent is MemoryStream)
                return BytesHandling.BytesToString(((MemoryStream)Torrent).ToArray());

            if (Torrent.CanSeek)
                Torrent.Seek(0, SeekOrigin.Begin);

            var Reader = new StreamReader(Torrent);
            var Result = Reader.ReadToEnd();
            Reader.Close();

            return Result;
        }

        /// <summary>
        /// Saves a string into a file.
        /// </summary>
        public static void StringToFile(string FileName, string Text)
        {
            using (var Writer = new StreamWriter(new FileStream(FileName,
                                                                FileMode.Create,
                                                                FileAccess.Write)))
            {
                Writer.Write(Text);
                Writer.Flush();
                Writer.Close();
            }
        }

        /// <summary>
        /// Reads a whole file as string.
        /// </summary>
        public static string FileToString(string FileName)
        {
            string Result = null;
            using (var Reader = new StreamReader(new FileStream(FileName,
                                                                FileMode.Open,
                                                                FileAccess.Read,
                                                                FileShare.ReadWrite)))
            {
                Result = Reader.ReadToEnd();
                Reader.Close();
            }

            return Result;
        }

        /// <summary>
        /// Reads a whole file as strings list.
        /// </summary>
        public static List<string> FileToStrings(string FileName)
        {
            return ToStrings(FileToString(FileName));
        }

        /// <summary>
        /// Reads a delimited text file and returns its content as a list of records plus the header, each composed of string fields.
        /// </summary>
        public static Tuple<List<List<string>>, List<string>> LoadFileDelimitedIntoStrings (string FilePath, string Delimiter = STR_DATA_DELIMITER,
                                                                                            bool HasQuotedValues = false, bool HasHeader = true,
                                                                                            bool TrimSpaces = true)
        {
            using(var Torrent = new FileStream(FilePath, FileMode.Open))
                return LoadStreamDelimitedIntoStrings(Torrent, Delimiter, HasQuotedValues, HasHeader, TrimSpaces);
        }

        /// <summary>
        /// Reads a Source stream of delimited text and returns its content as a list of records plus the header, each composed of string fields.
        /// </summary>
        public static Tuple<List<List<string>>, List<string>> LoadStreamDelimitedIntoStrings(Stream Source, string Delimiter = STR_DATA_DELIMITER,
                                                                                             bool HasQuotedValues = false, bool HasHeader = true,
                                                                                             bool TrimSpaces = true)
        {
            var ResultRecords = new List<List<string>>();
            var ResultHeader = new List<string>();

            var Parser = new Microsoft.VisualBasic.FileIO.TextFieldParser(Source);
            Parser.SetDelimiters(Delimiter);
            Parser.HasFieldsEnclosedInQuotes = HasQuotedValues;
            Parser.TrimWhiteSpace = TrimSpaces;

            var AtFirstLine = true;
            while (!Parser.EndOfData)
            {
                var Fields = Parser.ReadFields();
                if (Fields != null && Fields.Length > 0)
                    if (HasHeader && AtFirstLine)
                        ResultHeader = Fields.ToList();
                    else
                        ResultRecords.Add(Fields.ToList());

                AtFirstLine = false;
            }

            return Tuple.Create(ResultRecords, ResultHeader);
        }

        /// <summary>
        /// Saves a set of data records into a file for export, considering: Header, delimiter, append, quoted-values and space trimming.
        /// </summary>
        public static void SaveFileDelimited(string FilePath, IEnumerable<IEnumerable<string>> Records, IEnumerable<string> Header = null,
                                             string Delimiter = STR_DATA_DELIMITER, bool Append = false,
                                             bool WithQuotedValues = false, bool TrimSpaces = true, bool ReplaceDelimiterInData = false)
        {
            using (var Writer = TextWriter.Synchronized(new StreamWriter(FilePath, Append)))
            {
                bool FirstColumn = true;

                if (Header != null)
                {
                    foreach (var Label in Header)
                    {
                        Writer.Write((FirstColumn ? "" : Delimiter.NullDefault("")) +
                                        GetValueForExport(Label, WithQuotedValues, true, Delimiter, ReplaceDelimiterInData));
                        FirstColumn = false;
                    }

                    Writer.WriteLine();
                }

                foreach (var Record in Records)
                {
                    FirstColumn = true;

                    foreach (var Value in Record)
                    {
                        // NOTE: The record field value must come pre-quoted only when necessary (strings)
                        Writer.Write((FirstColumn ? "" : Delimiter.NullDefault("")) +
                                        GetValueForExport(Value, false, TrimSpaces, Delimiter, ReplaceDelimiterInData));
                        FirstColumn = false;
                    }

                    Writer.WriteLine();
                }

                Writer.Flush();
                Writer.Close();
            }
        }

        // -------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the best delimiter under the current-culture (Windows regional) configuration.
        /// </summary>
        public static string GetCurrentTextListDelimiter()
        {
            var Delimiter = (CultureInfo.CurrentCulture.TextInfo.ListSeparator == CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator
                         ? (General.STR_DATA_DELIMITER == CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator
                            ? General.STR_SEPARATOR
                            : General.STR_DATA_DELIMITER)
                         : CultureInfo.CurrentCulture.TextInfo.ListSeparator);

            return Delimiter;
        }

        // -------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the specified value string ready to be exported, considering: quotes, spaces trimming and delimiter.
        /// </summary>
        public static string GetValueForExport(string Value, bool AppendQuotes = false, bool TrimSpaces = true,
                                               string Delimiter = null, bool ReplaceDelimiterInData = false)
        {
            if (TrimSpaces)
                Value = Value.Trim();

            if (!Delimiter.IsAbsent())
            {
                var Replacement = (ReplaceDelimiterInData ? " ".Replicate(8) : Delimiter + Delimiter); // Either 8 spaces as for tab-delimited, or "" as for comma-delimited.
                Value = Value.Replace(Delimiter, Replacement);
            }

            if (AppendQuotes)
                Value =  "\"" + Value + "\"";

            return Value;
        }

        /// <summary>
        /// Saves a whole stream in a file.
        /// </summary>
        public static void StreamToFile(string FileName, Stream Torrent)
        {
            FileStream TargetFile = new FileStream(FileName, FileMode.Create, FileAccess.Write);
            BinaryWriter Writer = new BinaryWriter(TargetFile);
            int ReadByte = -1;

            if (Torrent.CanSeek)
                Torrent.Seek(0, SeekOrigin.Begin);

            if (Torrent is MemoryStream)
                Writer.Write(((MemoryStream)Torrent).ToArray());
            else
                while ((ReadByte = Torrent.ReadByte()) >= 0)
                    Writer.Write((byte)ReadByte);

            Writer.Flush();
            Writer.Close();
        }

        /// <summary>
        /// Reads a whole file in a stream.
        /// </summary>
        public static Stream FileToStream(string FileName)
        {
            Stream Torrent = new FileStream(FileName, FileMode.Open, FileAccess.Read);
            Torrent.Seek(0, SeekOrigin.Begin);
            return Torrent;
        }

        /// <summary>
        /// Saves a byte array in a file.
        /// </summary>
        public static void BytesToFile(string FileName, byte[] Bytes)
        {
            FileStream TargetFile = new FileStream(FileName, FileMode.Create, FileAccess.Write);
            BinaryWriter Writer = new BinaryWriter(TargetFile);

            Writer.Write(Bytes);

            Writer.Flush();
            Writer.Close();
        }

        /// <summary>
        /// Reads a whole file in a byte array.
        /// </summary>
        public static byte[] FileToBytes(string FileName)
        {
            byte[] Result = null;

            try
            {
                int ReadBytesCount = 0;
                int TargetPosition = 0;
                var FileSize = (int)(new FileInfo(FileName)).Length;
                int BlockSize = Math.Min(FileSize, BytesHandling.BLOCK_SIZE_BIG);
                Result = new byte[FileSize];

                using (var Torrent = new FileStream(FileName, FileMode.Open, FileAccess.Read))
                {
                    using (var Reader = new BinaryReader(Torrent))
                    {

                        while ((ReadBytesCount = Reader.Read(Result, TargetPosition, BlockSize)) > 0)
                        {
                            TargetPosition += ReadBytesCount;

                            if (TargetPosition + BlockSize > FileSize)
                                BlockSize = FileSize - TargetPosition;
                        }
                    }
                }
            }
            catch (Exception Problem)
            {
                throw new UsageAnomaly("File cannot be read.", Problem, FileName);
            }

            return Result;

            /* OLD VERSION...
            try
            {
                while ((ReadBytesCount = Reader.Read((Block = new byte[BytesHandling.BLOCK_SIZE_BIG]), 0, BytesHandling.BLOCK_SIZE_BIG))
                       == BytesHandling.BLOCK_SIZE_BIG)
                    Blocks.Add(Block);

                if (ReadBytesCount > 0)
                {
                    byte[] FinalBlock = new byte[ReadBytesCount];
                    BytesHandling.CopyByteArray(FinalBlock, Block);
                    Blocks.Add(FinalBlock);
                }
            }
            catch (Exception Anomaly)
            {
                throw new UsageAnomaly("File cannot be read.", Anomaly, FileName);
            }
            finally
            {
                if (Reader != null)
                    Reader.Close();

                if (Torrent != null)
                    Torrent.Close();
            }

            return BytesHandling.FusionateByteArrays(Blocks.GetArray()); */
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Copies the source-directory into the target-directory, optionally indicating overwriting (default=true).
        /// </summary>
        public static void CopyDirectory(string SourceDir, string TargetDir, bool Overwrite = true)
        {
            Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(SourceDir, TargetDir, Overwrite);
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Deletes the content of the supplied Directory, if exists.
        /// Returns indication of directory found and deleted.
        /// </summary>
        public static bool DeleteDirectoryAndContents(string DirectoryPath)
        {
            if (!Directory.Exists(DirectoryPath))
                return false;

            var Dir = new DirectoryInfo(DirectoryPath);
            Dir.Delete(true);

            return true;
        }
        
        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Converts an integer supplied in base 10 to the same in the specified base.
        /// The supported base range is 2 to 64.
        /// </summary>
        public static string NumberBase10InBaseN(int Number, int Base)
        {
            if (Base < 2 || Base > 64)
                throw new UsageAnomaly("Only conversions to bases between 2 and 64 are supported.", Base);

            string Result = "";
            int Remainder = 0;

            do
            {
                Remainder = Number % Base;
                Number = (Number - Remainder) / Base;
                Result = DIGITS_BASE2TOBASE64[Remainder] + Result;
            }
            while (Number > 0);

            return Result;
        }

        /// <summary>
        /// Converts an integer supplied in base N to the same in base 10.
        /// The supported base range is 2 to 64.
        /// </summary>
        public static int NumberBaseNInBase10(string Number, int Base)
        {
            if (Base < 2 || Base > 64)
                throw new UsageAnomaly("Only conversions from bases between 2 and 64 are supported.", Base);

            int Result = 0;
            char SearchedChar;
            int Digit = 0;
            int Index = 0;

            do
            {
                SearchedChar = Number[Index];
                Digit = DIGITS_BASE2TOBASE64.IndexOf(SearchedChar);

                if (Digit < 0)
                    throw new UsageAnomaly("Character not recognized.", SearchedChar);

                Result += Digit * (int)Math.Pow(Base, (Number.Length - Index) - 1);
                Index++;
            }
            while (Index < Number.Length);

            return Result;
        }

        /// <summary>
        /// Returns the supplied number expressed as alphabetical sequence of combined letters in base 0.
        /// For example: 0=A, 1=B, 25=Z, 26=BA, 27=BB, 28=BC, ...
        /// Note: This differs from the column naming system of spreadsheets like Excel or 1-2-3.
        /// </summary>
        public static string NumberAsLetters(int Number)
        {
            string Result = "";
            int Remainder = 0;
            int Index = 0;

            do
            {
                Remainder = Number % 26;
                Number = (Number - Remainder) / 26;
                Result = (char)((byte)(65 + Remainder)) + Result;
                Index++;
            }
            while (Number > 0);

            return Result;
        }

        /// <summary>
        /// Returns whether the supplied object is numeric.
        /// </summary>
        public static bool IsNumericData(object Data)
        {
            return (Data is Decimal || Data is System.Numerics.BigInteger ||
                    (Data.GetType().IsPrimitive &&
                     !((Data is Boolean) || (Data is Char))));
        }

        /// <summary>
        /// Returns random file name based on the supplied prefix, extension and fixed-length of the generated random number.
        /// </summary>
        public static string GenerateRandomFileName(string Prefix, string Extension, int NumLength = 10)
        {
            Extension = Extension.NullDefault("");
            NumLength = NumLength.EnforceRange(1, 10);
            var Result = Prefix + (new Random()).Next().ToString("0".Replicate(10)).GetRight(NumLength)
                                   + (Extension.StartsWith(".") ? Extension : (Extension.IsAbsent() ? "" : "." + Extension));
            return Result;
        }

        /// <summary>
        /// Tries to extract, from a string with common date (format "YYYYMMDD"), its parts
        /// of year, month and day. Returns true if the capture is successful.
        /// </summary>
        public static bool TryCaptureOfCommonDate(string CommonDate, out int Year, out int Month, out int Day)
        {
            Year = 0; Month = 0; Day = 0;

            if (!(Int32.TryParse(CommonDate.Substring(0, 4), out Year) &&
                  Int32.TryParse(CommonDate.Substring(4, 2), out Month) &&
                  Int32.TryParse(CommonDate.Substring(6, 2), out Day)) ||
                  (Year < 1 || Year > 9999) || (Month < 1 || Month > 12) || (Day < 1 || Day > 31))
                return false;

            try
            {
                // This tests the date parts coherence (e.g.: Cannot accept a 20010229)
                new DateTime(Year, Month, Day);
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Converts common date (format "YYYYMMDD") to julian date (format "YYYYDDD").
        /// </summary>
        public static string CommonDateToJulian(string CommonDate)
        {
            int Year = -1, Month = -1, Day = -1;

            if (CommonDate == "00000000")
                CommonDate = "19000101";

            if (!TryCaptureOfCommonDate(CommonDate, out Year, out Month, out Day))
                throw new UsageAnomaly("Cannot interpret argument as Common Date.", CommonDate);

            DateTime NewDate = new DateTime(Year, Month, Day);
            TimeSpan Lapse = NewDate.Subtract(new DateTime(Year - 1, 12, 31));
            return ((Year * 1000) + Lapse.Days).ToString();
        }

        /// <summary>
        /// Converts a julian date (format "YYYYMMDD") to common date (format "YYYYMMDD").
        /// </summary>
        public static string JulianDateToCommon(string JulianDate)
        {
            int Year = -1, Day = -1;
            if (JulianDate == "0000000")
                JulianDate = "1900001";

            if (!(Int32.TryParse(JulianDate.Substring(0, 4), out Year) &&
                   Int32.TryParse(JulianDate.Substring(4), out Day)) ||
                 (Year < 1 || Year > 9999) || (Day < 1 || Day > 366))
                throw new UsageAnomaly("Cannot interpret argument as Julian Date.", JulianDate);

            DateTime NewDate = (new DateTime(Year - 1, 12, 31)).AddDays(Day);
            return DateToInteger(NewDate).ToString();
        }

        /// <summary>
        /// Converts an integer (format "YYYYMMDD") to a Datetime.
        /// </summary>
        public static DateTime IntegerToDate(int Integer)
        {
            int Year = -1, Month = -1, Day = -1;
            string Text = Integer.ToString();

            if (!TryCaptureOfCommonDate(Text, out Year, out Month, out Day))
                throw new UsageAnomaly("Cannot interpret argument as Common Date.", Text);

            return (new DateTime(Year, Month, Day));
        }

        /// <summary>
        /// Converts DateTime date to integer (format "YYYYMMDD").
        /// </summary>
        public static int DateToInteger(this DateTime TargetDate)
        {
            return ((TargetDate.Year * 10000) + (TargetDate.Month * 100) + TargetDate.Day);
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Tries to execute a supplied function and don't crash if it fails, else returns a default value.
        /// That default value can be supplied, else it returns the default value of the return type.
        /// </summary>
        public static TResult Try<TResult>(Func<TResult> Function, TResult DefaultValue = default(TResult))
        {
            try
            {
                return Function();
            }
            catch
            {
                return DefaultValue;
            }
        }

        /// <summary>
        /// Tries to execute a supplied Operation/Action until a maximum number of intents is reached.
        /// Returns null if it succeeded, else returns the last catched exception.
        /// </summary>
        public static Exception TryMultipleTimes(Action<int> Operation, int MaxIntents)
        {
            Exception LastFailure = null;

            for (int Intent = 1; Intent <= MaxIntents; Intent++)
                try
                {
                    Operation(Intent);
                    return null;
                }
                catch(Exception Failure)
                {
                    LastFailure = Failure;
                }

            return LastFailure;
        }

        //------------------------------------------------------------------------------------------
        // NOTE: This converter is just for qualified-name storage (therefore full descendant-type compliance is not needed).
        public static MModelPropertyDefinitor ConvertByteArrayToMModelPropertyDefinitor(byte[] Value)
        {
            if (Value == null || Value.Length == 0)
                return null;

            var Result = (MModelClassDefinitor.DeclaredMemberDefinitors.GetValueOrDefault(Value.BytesToString()) as MModelPropertyDefinitor);
            return Result;
        }

        // NOTE: This converter is just for qualified-name storage (therefore full descendant-type compliance is not needed).
        public static byte[] ConvertMModelPropertyDefinitorToByteArray(MModelPropertyDefinitor Value)
        {
            if (Value == null)
                return null;

            var Result = Value.QualifiedTechName.StringToBytes();
            return Result;
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the last typed URL.
        /// </summary>
        public static string GetLastTypedURL()
        {
            // PENDING: For other common web browsers...
            // Obtain the default web browser and then get the last url typed
            // (from another registry-key or config file).

            var WebURL = "";

            try
            {
                WebURL = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Internet Explorer\TypedURLs", "url1", "").ToStringAlways();
            }
            catch (Exception Problem)
            {
                Console.WriteLine("Cannot read Windows Registry. Problem: " + Problem.Message);
            }

            return WebURL;
        }

        /// <summary>
        /// Gets the last typed URLs.
        /// </summary>
        public static IList<string> GetLastTypedURLs()
        {
            var Result = new List<string>();

            try
            {
                var RegKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Internet Explorer\TypedURLs");
                string WebUrl = null;
                int Count = 1;
                while (true)
                {
                    var ValueName = "url" + Count.ToString();
                    WebUrl = RegKey.GetValue(ValueName) as string;
                    if (WebUrl == null)
                        break;

                    Result.Add(WebUrl);
                    Count++;
                }
            }
            catch (Exception Problem)
            {
                Console.WriteLine("Cannot read Windows Registry. Problem: " + Problem.Message);
            }

            return Result;
        }

        //------------------------------------------------------------------------------------------
    }
}
