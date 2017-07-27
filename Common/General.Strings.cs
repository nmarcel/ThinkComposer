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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Windows.Documents;

using Microsoft.Win32;

using Instrumind.Common.EntityBase;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /// <summary>
    /// Provides common features such as encryption, serialization, strings handling extensions and global constants. Strings part.
    /// </summary>
    public static partial class General
    {
        /// <summary>
        /// Character used instead of Space in identifiers.
        /// </summary>
        public const char IDENTIF_SPACE_REPLACEMENT = '_';

        /// <summary>
        /// Character used instead of Space in URL identifiers.
        /// </summary>
        public const char URLIDENTIF_SPACE_REPLACEMENT = '-';

        /// <summary>
        /// Standard identation text.
        /// </summary>
        public const string INDENT_TEXT = "    ";

        /// <summary>
        /// Set of regular letter and digits.
        /// </summary>
        public const string STR_LETTER_AND_DIGITS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        /// <summary>
        /// Characters valid for file names.
        /// </summary>
        public const string STR_VALID_FILENAME_CHARS   = STR_LETTER_AND_DIGITS + "_-+!$&()=.,;[]{}·^%";

        /// <summary>
        /// Characters valid for C# identifiers.
        /// </summary>
        public const string STR_VALID_CSHARPIDENTIF_CHARS = STR_LETTER_AND_DIGITS + "_";

        /// <summary>
        /// Characters valid for URLs.
        /// </summary>
        public const string STR_VALID_URLIDENTIF_CHARS = STR_LETTER_AND_DIGITS + "_-+!$&()=.,;[]~#@'";  // Removed: /?:*

        /// <summary>
        /// Characters valid as identifier symbols (also valid for file names).
        /// </summary>
        // IMPORTANT: Reconsider if user-defined identifier names will be used in a future Scripting Language.
        public const string STR_VALID_IDENTIFIER_CHARS = STR_VALID_FILENAME_CHARS; // IF-CHANGED: Then check all current file-name related usage (see ResourceLink and Attachment).

        /// <summary>
        /// Set of valid characters in a Regularized text.
        /// </summary>
        public const string STR_VALID_CHARACTERS = STR_LETTER_AND_DIGITS + "ÑñÇç .:,;+-*/=?!$%&()[]{}_|ºª'@";

        /// <summary>
        /// Returns this supplied string in its plural form, if possible.
        /// </summary>
        public static string Pluralize(this string Original)
        {
            if (Original.IsAbsent())
                return Original;

            var LastIndex = Original.Length - 1;
            var Capitalized = Original.ToUpper();

            string Suffix = "S";

            if (Capitalized[LastIndex].IsOneOf('O', 'S', 'Z', 'X'))
                Suffix = "ES";

            if (!Original[LastIndex].IsUpperCase())
                Suffix = Suffix.ToLower();

            return Original + Suffix;
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns this text as encoded HTML.
        /// </summary>
        public static string ToHtmlEncoded(this string Source)
        {
            var Chars = HttpUtility.HtmlEncode(Source).ToCharArray();
            var Text = new StringBuilder((int)((double)Source.Length * 1.1));
            var Previous = 0;

            foreach (char c in Chars)
            {
                var Value = Convert.ToInt32(c);

                if (Value > 127)
                    Text.AppendFormat("&#{0};", Value);
                else
                    switch(Value)
                    {
                        case 13:
                        case 10:
                            if (Previous == 13) // Force emit of just one line-break.
                                Text.Append("<br/>");
                            break;
                        case 9:
                            Text.Append("&nbsp;&nbsp;&nbsp;&nbsp;");
                            break;
                        case 32:
                            if (Text.Length > 0 && Text[Text.Length - 1] == ' ')
                                Text.Append("&nbsp;");
                            else
                                Text.Append(' ');
                            break;
                        default:
                            Text.Append(c);
                            break;
                    }

                Previous = Value;
            }

            var Result = Text.ToString();
            return Result;
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns an inverse-sortable version of this supplied Text,
        /// optionally reaching to a Max-Lenght size with fillers, plus lower and upper character limits.
        /// </summary>
        public static string AsInverseSortable(this string Text, int MaxLenght = 0, char CharLowerLimit = Char.MinValue, char CharUpperLimit = Char.MaxValue)
        {
            MaxLenght = (MaxLenght == 0 ? Text.Length : MaxLenght);

            var Builder = new StringBuilder(MaxLenght);
            for (int Index = 0; Index < MaxLenght; Index++)
            {
                var Character = (char)(Index < Text.Length ? Text[Index] : 0);
                Builder.Append((char)((CharUpperLimit - Character) + CharLowerLimit));
            }

            var Result = Builder.ToString();
            //T var Len = Result.Length;
            //T Console.WriteLine("Text({0})='{1}'", Len, Result);
            return Result;
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns this supplied string reversed.
        /// </summary>
        public static string Reversed(this string Source)
        {
            var Result = new string(Source.Reverse().ToArray());
            return Result;
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns this supplied string intercalated each interval characterse with the specified intercalation text (an space, by default).
        /// </summary>
        public static string Intercalate(this string Text, int Interval, string Intercalation)
        {
            if (Intercalation.IsAbsent() || Interval < 1 || Text.Length <= Interval)
                return Text;

            var EstimatedSize = Text.Length + (Intercalation.Length * (int)Math.Ceiling((double)Text.Length / Interval));
            var Builder = new StringBuilder(EstimatedSize);

            int Pos = 0;
            while (Pos <= Text.Length)
            {
                Builder.Append(Text.GetMid(Pos, Interval) + Intercalation);
                Pos += Interval;
            }

            var Result = Builder.ToString();
            return Result;
        }

        /// <summary>
        /// Returns this supplied string intercalated with the specified intercalation text (an space, by default).
        /// </summary>
        public static string Intercalate(this string Text, string Intercalation = " ")
        {
            if (Intercalation.IsAbsent())
                return Text;

            var Builder = new StringBuilder((Text.Length - 1) * Intercalation.Length);
            Builder.Append(Text[0]);

            foreach (var Char in Text.Skip(1))
                Builder.Append(Intercalation + Char);

            var Result = Builder.ToString();
            return Result;
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Indicates whether this supplied character is upper-case.
        /// </summary>
        public static bool IsUpperCase(this char Character)
        {
            return (Character >= 65 && Character <= 90);
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Indicates whether this supplied character is a vowel.
        /// </summary>
        public static bool IsVowel(this char Character)
        {
            return (Character.IsOneOf('A', 'E', 'I', 'O', 'U', 'a', 'e', 'i', 'o', 'u'));
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the supplied default value if the target object is empty or null, else returns the target object to string.
        /// </summary>
        public static string ToStringOrDefault(this object Data, string Default = "")
        {
            return (Data == null ? Default : Data.ToString());
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the supplied default value always as string (at least the empty string, never as null).
        /// </summary>
        public static string ToStringAlways(this object Data, string Default = "")
        {
            return Data.ToStringOrDefault(Default) ?? Default;
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the supplied default value if the target string is empty or null, else returns the target string.
        /// </summary>
        public static string AbsentDefault(this string Data, string Default = "")
        {
            return (Data.IsAbsent() ? Default : Data);
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the string representation of the supplied object, even when empty (null).
        /// </summary>
        public static string ConvertToString(object objeto)
        {
            string Texto;
            return (objeto == null || (Texto = objeto.ToString()) == null ? STR_NULLREF : Texto);
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the count of occurrences of a char in a string.
        /// </summary>
        public static int CountOccurrences(this string TargetText, char SearchedText)
        {
            return CountOccurrences(TargetText, SearchedText.ToString());
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the count of occurrences of a string in another.
        /// </summary>
        public static int CountOccurrences(this string TargetText, string SearchedText)
        {
            int Count = 0, Position = 0;
            int SearchedLength = SearchedText.Length;

            if (SearchedLength > 0 && TargetText.Length > 0)
                while ((Position = TargetText.IndexOf(SearchedText, Position)) >= 0)
                {
                    Count++;
                    Position = Position + SearchedLength;
                };

            return Count;
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the text between the specified Enclosing-Prefix and Enclosing-Suffix.
        /// Optionally, if no Enclosing-Prefix or Enclosing-Suffix are found, it can return the remainder, else nothing (the default).
        /// </summary>
        public static string CutBetween(this string SourceText, string EnclosingPrefix, string EnclosingSuffix = null,
                                        bool ReturnRemainderWhenNoEnclosing = false)
        {
            var PosIni = (EnclosingPrefix == null ? -1 : SourceText.IndexOf(EnclosingPrefix));
            if (PosIni < 0)
                if (!ReturnRemainderWhenNoEnclosing)
                    return "";

            var Offset = (EnclosingPrefix == null ? 1 : EnclosingPrefix.Length);

            var PosEnd = (EnclosingSuffix == null ? SourceText.Length : SourceText.IndexOf(EnclosingSuffix, PosIni + Offset));
            if (PosEnd < 0)
                if (!ReturnRemainderWhenNoEnclosing)
                    return "";
                else
                    PosEnd = SourceText.Length;

            var Result = SourceText.Substring(PosIni + Offset, (PosEnd - PosIni) - Offset);
            return Result;
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns a string minus another from the end, if found.
        /// </summary>
        public static string CutFromEnd(this string SourceText, string CuttingText)
        {
            var Reversed = SourceText.Reversed();
            var Pos = (Reversed.Length == 0 ? -1 : Reversed.IndexOf(CuttingText.Reversed(), 0));

            if (Pos < 0)
                return SourceText;

            SourceText = Reversed.Substring(Pos + CuttingText.Length).Reversed();
            return SourceText;
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns a string builded with the replication of another.
        /// </summary>
        public static string Replicate(this string Text, int Times)
        {
            if (Times < 1)
                return "";

            StringBuilder Result = new StringBuilder(Text.Length * Times);

            for (int i = 0; i < Times; i++)
                Result.Append(Text);

            return Result.ToString();
        }

        /// <summary>
        /// Removes, from this supplied Text, all the specified strings.
        /// </summary>
        public static string Remove(this string Text, params string[] Strings)
        {
            if (Text == null)
                return null;

            var Result = Text;
            int Index = -1;
            bool ApplyRemoval = true;

            while (ApplyRemoval)
            {
                ApplyRemoval = false;

                foreach (var Replacement in Strings)
                {
                    Index = Result.IndexOf(Replacement);
                    if (Index >= 0)
                    {
                        Result = Result.Remove(Index, Replacement.Length);
                        ApplyRemoval = true;  // Re-evaluate (remotion of string could have generated removable strings again)
                    }
                }
            }

            return Result;
        }

        /// <summary>
        /// Removes, from this supplied Text, all the specified characters.
        /// </summary>
        public static string Remove(this string Text, params char[] Characters)
        {
            if (Text == null)
                return null;

            var Result = new StringBuilder(Text.Length);

            foreach (var Character in Text)
                if (!Character.IsIn(Characters))
                    Result.Append(Character);

            return Result.ToString();
        }

        /// <summary>
        /// Removes, from this supplied Text, all the characters matching the supplied Removal Condition.
        /// </summary>
        public static string RemoveWhere(this string Text, Func<char, bool> RemovalCondition)
        {
            if (Text == null)
                return null;

            var Result = new StringBuilder(Text.Length);

            foreach (var Character in Text)
                if (!RemovalCondition(Character))
                    Result.Append(Character);

            return Result.ToString();
        }

        /// <summary>
        /// Returns the passed Text without new-line characters (new-line or carriage-return).
        /// </summary>
        public static string RemoveNewLines(this string Text, string Replacement = " ")
        {
             var Result = (Text == null ? null : Text.Replace("\n", Replacement).Replace("\r", Replacement));
             return Result;
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Given a string, returns the specified number of characters from its left side.
        /// </summary>
        public static string GetLeft(this string Text, int Count)
        {
            if (Count < 0)
                return "";

            var Result = Text.Substring(0, (Count > Text.Length ? Text.Length : Count));
            return Result;
        }

        /// <summary>
        /// Given a string, returns the specified number of characters from the specified position not traspassing length limit.
        /// </summary>
        public static string GetMid(this string Text, int Position, int Count = 0)
        {
            if (Count < 1 || (Position + Count > Text.Length))
                Count = Text.Length - Position;

            var Result = (Count < 0 ? "" : Text.Substring(Position, Count));
            return Result;
        }

        /// <summary>
        /// Given a string, returns the specified number of characters from its right side.
        /// </summary>
        public static string GetRight(this string Text, int Count)
        {
            if (Count < 1)
                return "";

            if (Count > Text.Length)
                return Text;

            var Result = Text.Substring((Text.Length - Count), Count);
            return Result;
        }

        /// <summary>
        /// Gets this supplied text truncated at the specified count, appending an ellipsis ("...") when larger.
        /// </summary>
        public static string GetTruncatedWithEllipsis(this string Text, int Count)
        {
            if (Text == null)
                return null;

            if (Text.Length <= TXT_ELLIPSIS.Length)
                return Text;

            Count = Count.EnforceMinimum(TXT_ELLIPSIS.Length + 1);

            if (Text.Length > Count)
                Text = Text.GetLeft(Count - 1) + TXT_ELLIPSIS;

            return Text;
        }

        private const string TXT_ELLIPSIS = "...";

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns this supplied Text, with the text at Position of size Length, replaced by the New-Text string.
        /// </summary>
        public static string ReplaceAt(this string Text, int Position, int Length, string NewText)
        {
            var ContinuationPos = Position + Length;
            var Result = Text.GetLeft(Position) + NewText +
                         (ContinuationPos >= Text.Length ? "" : Text.Substring(ContinuationPos));

            return Result;
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Fills a string with another in the center of the first.
        /// </summary>
        public static string FillCentered(this string Text, int Length, string Filler)
        {
            int FillCount;

            if ((Text.Length % 2) == 0)
                FillCount = (int)((Length / 2) -
                                  ((decimal)Text.Length / 2));
            else
                FillCount = (int)(((decimal)Length / 2) -
                                  ((decimal)Text.Length / 2));

            var Result = GetLeft(GetRight(Replicate(Filler, FillCount), FillCount) + Text +
                                 Replicate(Filler, Length - Text.Length - FillCount),
                                 Length);
            return Result;
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns array of string segments divided by a separator and,
        /// optionally, omitting empty segments.
        /// PENDING: Optimize with IndexOfAny for searching with the first character.
        /// </summary>
        public static string[] Segment(this string Text, string Separator, bool OmitEmptySegments = false)
        {
            if (Text.Length == 0 || Separator.Length == 0) return (new string[0]);

            int SeparatorLength = Separator.Length;
            int InitialPosition = 0,
                FinalPosition = 0;
            SimpleList<string> List = new SimpleList<string>();
            string Fragment = "";

            while ((FinalPosition = Text.IndexOf(Separator, InitialPosition)) >= 0)
            {
                Fragment = Text.Substring(InitialPosition, (FinalPosition - InitialPosition));
                if (!OmitEmptySegments || Fragment.Length > 0)
                    List.Add(Fragment);
                InitialPosition = FinalPosition + SeparatorLength;
            }

            Fragment = Text.Substring(InitialPosition);
            if (!OmitEmptySegments || Fragment.Length > 0)
                List.Add(Fragment);

            return List.GetArray();
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns an array of rows of text, each with a maximum length, respecting whole words if possible.
        /// </summary>
        public static IEnumerable<string> SplitRespectingWords(this string Text, int MaxLength)
        {
            if (Text.Length <= MaxLength)
            {
                yield return Text.GetLeft(MaxLength).Trim();
                yield break;
            }

            var BlankChars = new char[] { ' ', '\n', '\t' };
            var PosIni = MaxLength.EnforceMaximum(Text.Length) - 1;
            var Pos = PosIni;
            while (Pos >= 0)
            {
                if (Text[Pos].IsIn(BlankChars))
                    break;
                Pos--;
            }

            if (Pos < 0) // Si no encontró donde cortar... devolver el segmento completo no más
                Pos = PosIni;

            yield return Text.GetLeft(Pos + 1).Trim();

            var Extras = Text.Substring(Pos + 1).SplitRespectingWords(MaxLength);
            foreach (var Extra in Extras)
                if (!Extra.IsAbsent())
                    yield return Extra;
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Validates whether the supplied string can be used as an standard Instrumind identifier.
        /// </summary>
        public static bool IsValidIdentifier(this string Text)
        {
            var Result = IsValidTextForCharsSet(Text, STR_VALID_IDENTIFIER_CHARS);
            return Result;
        }

        /// <summary>
        /// Validates whether the supplied string is a valid C# identifier.
        /// </summary>
        public static bool IsValidCSharpIdentifier(this string Text)
        {
            if (Text.IsAbsent())
                return false;

            var Result = (IsValidTextForCharsSet(Text, STR_VALID_CSHARPIDENTIF_CHARS)
                          && (Text[0] == '_' || Char.IsLetter(Text[0])));
            return Result;
        }

        /// <summary>
        /// Validates whether the supplied string is a valid URL identifier.
        /// </summary>
        public static bool IsValidUrlIdentifier(this string Text)
        {
            var Result = IsValidTextForCharsSet(Text, STR_VALID_URLIDENTIF_CHARS);
            return Result;
        }

        /// <summary>
        /// Validates whether the supplied string is valid respect the specified valid-chars-set.
        /// </summary>
        public static bool IsValidTextForCharsSet(string Text, string ValidCharsSet)
        {
            General.ContractRequiresNotNull(Text);

            if (Text == String.Empty)
                return false;

            foreach (char Character in Text)
                if (!ValidCharsSet.Contains(Character))
                    return false;

            return true;
        }

        /// <summary>
        /// Validates whether the supplied string can be used as an standard Instrumind non empty text (no control characters allowed).
        /// </summary>
        public static bool IsValidText(this string Text)
        {
            var Result = (Text != String.Empty && !Text.Any(value => Char.IsControl(value)));
            return Result;
        }

        /// <summary>
        /// Returns this supplied Text with only the characters accepted by the specified Valid-Char-Selector
        /// (by default, accepts only Letters or Digits).
        /// </summary>
        public static string GetOnlyValidChars(this string Text, Func<char, bool> ValidCharSelector = null)
        {
            var result = new String(Text.Where(chr => ValidCharSelector.NullDefault(Char.IsLetterOrDigit)(chr)).ToArray());
            return result;
        }

        /// <summary>
        /// Returns a string from this supplied one, with any possible identifier invalid character replaced by the specified one.
        /// </summary>
        public static string ReplaceIdentifierInvalidCharsBy(this string Text, char Replacement)
        {
            var Result = ReplaceInvalidCharsBy(Text, Replacement, STR_VALID_IDENTIFIER_CHARS);
            return Result;
        }

        /// <summary>
        /// Returns a string from this supplied one, with any possible C# identifier invalid character replaced by the specified one.
        /// </summary>
        public static string ReplaceCSharpIdentifierInvalidCharsBy(this string Text, char Replacement)
        {
            var Result = ReplaceInvalidCharsBy(Text, Replacement, STR_VALID_CSHARPIDENTIF_CHARS);
            return Result;
        }

        /// <summary>
        /// Returns a string from this supplied one, with any possible URL invalid character replaced by the specified one.
        /// Optionally, a set of additional invalid characters can be specified.
        /// </summary>
        public static string ReplaceUrlIdentifierInvalidCharsBy(this string Text, char Replacement, string AdditionalInvalidChars = null)
        {
            General.ContractRequiresNotNull(Text);

            if (AdditionalInvalidChars == null)
                AdditionalInvalidChars = "";

            var Result = ReplaceInvalidCharsBy(Text, Replacement, STR_VALID_URLIDENTIF_CHARS + AdditionalInvalidChars);
            return Result;
        }

        /// <summary>
        /// Returns a string from the supplied one, with any possible character not in the valid-chars-set replaced by the specified one.
        /// </summary>
        public static string ReplaceInvalidCharsBy(string Text, char Replacement, string ValidCharsSet)
        {
            General.ContractRequiresNotNull(Text);

            StringBuilder Replacer = null;

            for (int Index = 0; Index < Text.Length; Index++)
                if (!ValidCharsSet.Contains(Text[Index]))
                {
                    if (Replacer == null)
                        Replacer = new StringBuilder(Text);

                    Replacer[Index] = Replacement;
                }

            var Result = (Replacer == null ? Text : Replacer.ToString());
            return Result;
        }

        /// <summary>
        /// Set of characters to be replaced by others in a Regularized text.
        /// </summary>
        // PENDING: Solve problem... how to change multichar replacements, like the german 'ß' by 'SS'.
        public static readonly Dictionary<char, char> CharacterReplacements =
            new Dictionary<char, char>()
            {
                {'Á', 'A'},                {'À', 'A'},                {'Ä', 'A'},                {'Â', 'A'},
                {'á', 'a'},                {'à', 'a'},                {'ä', 'a'},                {'â', 'a'},
                {'É', 'E'},                {'È', 'E'},                {'Ë', 'E'},                {'Ê', 'E'},
                {'é', 'e'},                {'è', 'e'},                {'ë', 'e'},                {'ê', 'e'},
                {'Í', 'I'},                {'Ì', 'I'},                {'Ï', 'I'},                {'Î', 'I'},
                {'í', 'i'},                {'ì', 'i'},                {'ï', 'i'},                {'î', 'i'},
                {'Ó', 'O'},                {'Ò', 'O'},                {'Ö', 'O'},                {'Ô', 'O'},
                {'ó', 'o'},                {'ò', 'o'},                {'ö', 'o'},                {'ô', 'o'},
                {'Ú', 'U'},                {'Ù', 'U'},                {'Ü', 'U'},                {'Û', 'U'},
                {'ú', 'u'},                {'ù', 'U'},                {'ü', 'u'},                {'û', 'u'}
                /* Accepted
                {'Ñ', 'N'},                {'Ç', 'C'},
                {'ñ', 'n'},                {'ç', 'c'} */
            };

        /// <summary>
        /// Returns a modified string with non-regular characters replaced by valid ones, or the same string if no replacement is needed.
        /// Optionally, an invalid-character-replacement can be specified. Use the null-character ('\0') to remove non-valid chars.
        /// </summary>
        public static string Regularize(this string OriginalText, char InvalidCharReplacement = '?', string ValidCharacters = STR_VALID_CHARACTERS)
        {
            /* For Testing...

                string Text = "";
                Text = "Almíbar y dulce néctar. Ándate y vuelve con Él.";
                Console.WriteLine("Original=[{0}], Regularized=[{1}]", Text, Text.Regularize());
                Text = "Bernardo O'Higgins y Hernán Büchi.";
                Console.WriteLine("Original=[{0}], Regularized=[{1}]", Text, Text.Regularize());
                Text = "Bernardo O'Higgins y Hernán Büchi.";
                Console.WriteLine("Original=[{0}], Regularized=[{1}]", Text, Text.Regularize('\0'));
                Text = "El Nº1 de las hûestes de O'Higgins conocía al almirante O'Brien.";
                Console.WriteLine("Original=[{0}], Regularized=[{1}]", Text, Text.Regularize());
                Text = "El Nº1 de los Hûsares de O'Higgins conocía al almirante O'Brien.";
                Console.WriteLine("Original=[{0}], Regularized=[{1}]", Text, Text.Regularize('\0')); */

            if (String.IsNullOrEmpty(OriginalText))
                return OriginalText;

            StringBuilder NewText = null;
            int NewTextIndex = 0;

            for (int Index = 0; Index < OriginalText.Length; Index++)
            {
                var OriginalChar = OriginalText[Index];

                if (!ValidCharacters.Contains(OriginalChar))
                {
                    if (NewText == null)
                        NewText = new StringBuilder(OriginalText);

                    var Replacer = (CharacterReplacements.ContainsKey(OriginalChar)
                                    ? CharacterReplacements[OriginalChar]
                                    : InvalidCharReplacement);

                    if (Replacer == '\0')
                    {
                        NewText.Remove(NewTextIndex, 1);
                        NewTextIndex--;
                    }
                    else
                        NewText[NewTextIndex] = Replacer;
                }

                NewTextIndex++;
            }

            var Result = (NewText == null ? OriginalText : NewText.ToString());

            return Result;
        }

        /// <summary>
        /// Returns the supplied text as file filter extension (form "*.XYZ") or null if cannot convert it.
        /// </summary>
        public static string GetFilterExtension(string FilterExtension)
        {
            FilterExtension = FilterExtension.Trim().TextToIdentifier();

            if (FilterExtension.GetLeft(2) != "*.")
                FilterExtension = "*." + FilterExtension;

            if (!FilterExtension.Substring(2).IsValidIdentifier())
                FilterExtension = null;

            return FilterExtension;
        }

        /// <summary>
        /// Returns a resource (file, folder, url, etc.) name, as Identifier text and without its dominants/parents, from the supplied Path or Uri.
        /// </summary>
        public static string GetSimplifiedResourceName(this string Source)
        {
            if (Source.Length <= 1)
                return Source.TextToIdentifier();

            Source = Source.Replace('\\', '/');
            if (Source.EndsWith("/"))
                Source = Source.GetLeft(Source.Length - 1);

            var LastSlashPos = Source.LastIndexOf('/');
            if (LastSlashPos > 0)
                Source = Source.Substring(LastSlashPos + 1);

            return Source.TextToIdentifier().Trim();
        }

        /// <summary>
        /// Converts the supplied Text to its Identifier normalized equivalent.
        /// Optionally, an indication of reject absent text and a replacement character (for spaces and other non-conformant characters) can be specified.
        /// </summary>
        public static string TextToIdentifier(this string Text, bool RejectEmptyText = false, char Replacement = IDENTIF_SPACE_REPLACEMENT)
        {
            if (Text.IsAbsent() && !RejectEmptyText)
                return Text;

            // Note: The rejection of empty text is made by IsValidIdentifier().
            var Result = Text.Trim().Replace(' ', Replacement).ReplaceIdentifierInvalidCharsBy(Replacement);

            /*T if (!IsValidIdentifier(Result))
                throw new UsageAnomaly("Cannot convert Text to Identifier.", Text); */

            return Result;
        }

        /// <summary>
        /// Converts the supplied Text to its C# Identifier normalized equivalent.
        /// Optionally, an indication of reject absent text can be specified.
        /// </summary>
        public static string TextToCSharpIdentifier(this string Text, bool RejectEmptyText = false)
        {
            if (Text.IsAbsent() && !RejectEmptyText)
                return Text;

            var Result = Text.Trim().Replace(' ', '_').ReplaceCSharpIdentifierInvalidCharsBy('_');
            if (!Char.IsLetter(Result[0]) && Result[0] != '_')
                Result = "_" + Result;

            /*T if (!IsValidCSharpIdentifier(Result))
                throw new UsageAnomaly("Cannot convert Text to C# Identifier.", Text); */

            return Result;
        }

        /// <summary>
        /// Converts the supplied Text to its URL Identifier normalized equivalent.
        /// Optionally, a replacement character (for spaces and other non-conformant characters)
        /// and a set of additional invalid characters can be specified.
        /// </summary>
        public static string TextToUrlIdentifier(this string Text, char Replacement = URLIDENTIF_SPACE_REPLACEMENT,
                                                 string AdditionalInvalidChars = null)
        {
            // Note: The rejection of empty text is made by IsValidIdentifier().
            var Result = Text.Trim().Replace(' ', Replacement).ReplaceUrlIdentifierInvalidCharsBy(Replacement, AdditionalInvalidChars);

            /*T if (!IsValidUrlIdentifier(Result))
                throw new UsageAnomaly("Cannot convert Text to URL-Identifier.", Text); */

            return Result;
        }

        /// <summary>
        /// Converts the supplied Identifier to its Text normalized equivalent using the specified replacement.
        /// Optionally, an space representator can be specified. The default is underscore ("_").
        /// </summary>
        public static string IdentifierToText(this string Identifier, char SpaceRepresentator = IDENTIF_SPACE_REPLACEMENT)
        {
            if (Identifier == null)
                return null;

            var Result = (SpaceRepresentator == ((char)0) ? Identifier : Identifier.Replace(SpaceRepresentator, ' '));

            if (!IsValidText(Result))
                throw new UsageAnomaly("Cannot convert Identifier to Text.", Identifier);

            return Result;
        }

        /// <summary>
        /// Returns concatenation of texts generated by the supplied function (ToStringAlways() if null) over each element of this collection.
        /// Optionally, it can be included a text separator between each segment except at the end (also optional).
        /// </summary>
        public static string GetConcatenation<TItem>(this IEnumerable<TItem> SourceCollection,
                                                     Func<TItem, string> Concatenator = null,
                                                     string Separator = null,
                                                     bool IncludeSeparatorAtTheEnd = false)
        {
            Separator = Separator ?? "";
            var Text = new StringBuilder();

            foreach (TItem Item in SourceCollection)
                Text.Append((Concatenator != null ? Concatenator(Item) : Item.ToStringAlways()) + Separator);

            if (!IncludeSeparatorAtTheEnd && !Separator.IsAbsent() && Text.Length >= Separator.Length)
                Text.Length -= Separator.Length;

            return Text.ToString();
        }

        /// <summary>
        /// Returns concatenation of texts generated by the supplied function (ToStringAlways() if null), which received item and index, over each element of this collection.
        /// Optionally, it can be included a text separator between each segment except at the end (also optional).
        /// </summary>
        public static string GetConcatenationIndexed<TItem>(this IEnumerable<TItem> SourceCollection,
                                                            Func<TItem, int, string> Concatenator = null,
                                                            string Separator = null,
                                                            bool IncludeSeparatorAtTheEnd = false)
        {
            Separator = Separator ?? "";
            var Text = new StringBuilder();

            var Index = 0;
            foreach (TItem Item in SourceCollection)
            {
                Text.Append((Concatenator != null ? Concatenator(Item, Index) : Item.ToStringAlways()) + Separator);
                Index++;
            }

            if (!IncludeSeparatorAtTheEnd && !Separator.IsAbsent() && Text.Length >= Separator.Length)
                Text.Length -= Separator.Length;

            return Text.ToString();
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns this supplied string without duplicate characters.
        /// </summary>
        static string RemoveDuplicateChars(this string Text)
        {
            // --- Removes duplicate chars using char arrays. ---
            int TextLength = Text.Length;

            // Store encountered letters in this array.
            char[] table = new char[TextLength];
            int tableLength = 0;

            // Store the result in this array.
            char[] result = new char[TextLength];
            int resultLength = 0;

            // Loop through all characters
            foreach (char value in Text)
            {
                // Scan the table to see if the letter is in it.
                bool exists = false;
                for (int i = 0; i < tableLength; i++)
                {
                    if (value == table[i])
                    {
                        exists = true;
                        break;
                    }
                }
                // If the letter is new, add to the table and the result.
                if (!exists)
                {
                    table[tableLength] = value;
                    tableLength++;

                    result[resultLength] = value;
                    resultLength++;
                }
            }

            // Return the string at this range.
            return new string(result, 0, resultLength);
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns indication of detected common token separation pattern in set of text records.
        /// </summary>
        // IMPROVEMENT: Support quoted text, as in CSV files.
        public static bool TextRecordsAreProbablySeparated(IEnumerable<string> TextRecords, string Separator,
                                                           int MinRecordsToCompare = 3)
        {
            MinRecordsToCompare = MinRecordsToCompare.EnforceMinimum(2);
            int RecordsCount = 0;
            int SeparatorsCount = 0, PreviousSeparatorsCount = 0;

            foreach (var Record in TextRecords)
            {
                RecordsCount++;
                if (RecordsCount > MinRecordsToCompare)
                    break;

                SeparatorsCount = Record.CountOccurrences(Separator);

                if (RecordsCount > 1)
                    if (SeparatorsCount != PreviousSeparatorsCount)
                        return false;

                PreviousSeparatorsCount = SeparatorsCount;
            }

            return (RecordsCount >= MinRecordsToCompare && SeparatorsCount > 0);
        }

        // -------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the supplied Version-Number as separated numeric values for:
        /// Version, Major-Revision and Minor-Revision. Null if returned when format is invalid.
        /// </summary>
        public static Tuple<byte, byte, int> GetVersionNumberParts(string VersionNumber, bool AnnounceFailure = true)
        {
            try
            {
                var Parts = VersionNumber.Split('.');
                var Result = Tuple.Create(Byte.Parse(Parts[0]), Byte.Parse(Parts[1]), Int32.Parse(Parts[2]));
                return Result;
            }
            catch (Exception Problem)
            {
                if (AnnounceFailure)
                    Console.WriteLine("Version number [{0}]. cannot be interpreted. Problem: {1}",
                                      VersionNumber.ToStringAlways(), Problem.Message);

                return null;
            }
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns this Source object in its Base-64 representation, optionally inserting line-breaks every 76 chars.
        /// </summary>
        public static string ToBase64(object Source, bool InsertLineBreaks = false)
        {
            var Bytes = Source as byte[];

            if (Bytes == null)
            {
                var Text = Source as string;
                if (Text != null)
                    Bytes = Text.StringToBytesUnicode();
                else
                {
                    var Image = Source as System.Windows.Media.ImageSource;
                    if (Image != null)
                        Bytes = Instrumind.Common.Visualization.Display.ToBytes(Image);
                    else
                        Bytes = BytesHandling.SerializeToBytes(Source);
                }
            }

            if (Bytes == null)
                return null;

            var Result = Convert.ToBase64String(Bytes, (InsertLineBreaks
                                                        ? Base64FormattingOptions.InsertLineBreaks
                                                        : Base64FormattingOptions.None));
            return Result;
        }


        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns indication that this supplied text is regularly separated by the specified separator.
        /// </summary>
        public static bool IsRegularlySeparatedBy(this IEnumerable<string> Text, char Separator, int MinSeparators = 2, int MinLines = 2, int MaxLines = 255)
        {
            var PrevSepartors = -1;
            var NoMoreLinesExpected = false;
            var Count = 0;

            foreach (var Line in Text)
            {
                if (Line == "")
                {
                    NoMoreLinesExpected = true;
                    continue;
                }

                if (NoMoreLinesExpected)
                    return false;

                var Separators = Line.CountOccurrences(Separator);
                if (Separators < MinSeparators)
                    return false;

                if (PrevSepartors >= 0 && PrevSepartors != Separators)
                    return false;

                PrevSepartors = Separators;
                Count++;
            }

            if (Count < MinLines)
                return false;

            return true;
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns a multi-line string as strings list.
        /// </summary>
        public static List<string> ToStrings(this string Text, int Limit = 0, bool IncludeRemainderTextIntoLastLine = false)
        {
            var Result = new List<string>();
            string TextSegment = null;

            using (var Reader = new StringReader(Text))
                while (true)
                {
                    if ((Limit > 0 && Result.Count >= (Limit - 1)) && IncludeRemainderTextIntoLastLine)
                    {
                        TextSegment = Reader.ReadToEnd();
                        Result.Add(TextSegment);
                        break;
                    }

                    TextSegment = Reader.ReadLine();
                    if (TextSegment == null)
                        break;

                    Result.Add(TextSegment);
                }

            return Result;
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// For a given object, returns the value associated to a matching key from a list of candidates inside a candidates-declaration text.
        /// If none matches, then a default value can be specified for return.
        /// The candidates-declaration is defined as "Key1=Value1;Key2=Value2;KeyN=ValueN", where the "=" assigner and ";" separator can be optionally specified.
        /// </summary>
        public static string SelectCorrespondence(this object Evaluated, string CandidatesDeclaration, string Default = null,
                                                  string CandidateAssigner = "=", string CandidatesSeparator = ";")
        {
            if (CandidatesDeclaration.IsAbsent())
                return Default;
            var Candidates = CandidatesDeclaration.Segment(CandidatesSeparator, true);
            foreach (var Candidate in Candidates)
            {
                var Parts = Candidate.Segment(CandidateAssigner);
                if (Parts.Length < 2)
                    continue;
                if (Evaluated.ToStringAlways() == Parts[0])
                    return Parts[1];
            }
            return Default;
        }

        // -------------------------------------------------------------------------------------------
    }
}