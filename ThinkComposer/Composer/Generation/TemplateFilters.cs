using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DotLiquid;

using Instrumind.Common;

using Instrumind.ThinkComposer.Model.GraphModel;

namespace Instrumind.ThinkComposer.Composer.Generation
{
    public static class TemplateFilters
    {
        public const string TEXT_LIST_SEPARATOR = General.STR_DATA_DELIMITER;

        public const string PREFIX_XAML_RICHTEXT = "<Section xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"";

        public static int InjectionDepth(object input)
        {
            return DotLiquid.Tags.Inject.RecursionLevel;
        }

        public static bool Any(object input)
        {
            if (input is string)
                return ((string)input).Length > 0;

            if (input is IEnumerable)
                return ((IEnumerable)input).Cast<object>().Any();

            return false;
        }

        public static string ToBase64(object input)
        {
            var Proxy = input as DropProxy;
            if (Proxy != null)
                input = Proxy.GetObject();

            var Result = Instrumind.Common.General.ToBase64(input, true) ?? "";

            return Result;
        }

        public static string AsChar(object input)
        {
            var Code = input.ToStringAlways();

            var NumChar = 0;
            if (Int32.TryParse(Code, out NumChar) && NumChar >= 9)
            {
                string Result = null;

                try
                {
                    Result = ((char)NumChar).ToString();
                }
                catch
                {
                    return Code;
                }

                return Result;
            }

            switch (Code.ToUpper())
            {
                case "NEWLINE": return Environment.NewLine;
                case "TAB": return "\t";
            }

            return Code;
        }

        public static string ToPlainText(object input)
        {
            var Proxy = input as DropProxy;
            if (Proxy != null)
                input = Proxy.GetObject();

            string Result = null;

            if (input is string)
                Result = (string)input ?? "";
            else
                Result = ToBase64(input);

            return Result;
        }

        public static string ToUnformattedText(string input)
        {
            if (input != null && input.StartsWith(PREFIX_XAML_RICHTEXT))
                input = Instrumind.Common.Visualization.Display.XamlRichTextToPlainText(input);

            return (input ?? "");
        }

        /// <summary>
        /// Gets, from a Source collection of Ideas, those based on Idea-Definitions having the specified Tech-Names (separated by ';')
        /// </summary>
        public static IEnumerable<Idea> GetIdeasDefinedAs(IEnumerable<Idea> Source, string IdeaDefTechNames)
        {
            var IdeaDefTechNamesList = IdeaDefTechNames.Segment(TEXT_LIST_SEPARATOR, true);

            // Made with different iterations to preserve order by TechNames
            foreach (var TechName in IdeaDefTechNamesList)
                foreach (var Item in Source.Where(item => item.IdeaDefinitor.TechName == TechName))
                    yield return Item;
        }

        /// <summary>
        /// Gets, from a Source collection of Role-Based Links, those having Variants with Tech-Name like those provided (separated by ';')
        /// </summary>
        public static IEnumerable<RoleBasedLink> GetLinksByVariant(IEnumerable<RoleBasedLink> Source, string LinkVariantTechNames)
        {
            var LinkVariantTechNamesList = LinkVariantTechNames.Segment(TEXT_LIST_SEPARATOR, true);

            foreach (var TechName in LinkVariantTechNamesList)
                foreach (var Link in Source.Where(link => link.RoleVariant.TechName == TechName))
                    yield return Link;
        }

        /// <summary>
        /// Gets, from a Source collection of Elements (IIdentifiableElement), those having the specified Tech-Names (separated by ';')
        /// </summary>
        public static IEnumerable<IIdentifiableElement> GetElements(IEnumerable<IIdentifiableElement> Source, string TechNames)
        {
            var TechNamesList = TechNames.Segment(TEXT_LIST_SEPARATOR, true);

            // Ordered by TechName
            // POSTPONED: Support '*token*' filtering
            foreach (var TechName in TechNamesList)
            {
                var Item = Source.FirstOrDefault(item => item.TechName == TechName);
                if (Item != null)
                    yield return Item;
            }
        }

        /// <summary>
        /// Gets, from the Source object, the specified Property by Tech-Name
        /// </summary>
        public static object Get(object Source, string PropertyTechName)
        {
            var Result = DotLiquid.DropProxy.ExtractValue(Source, PropertyTechName);
            return Result;
        }

        /// <summary>
        /// From a Source collection of items having a property, which is also another collection, gets the union of all the items of these collections.
        /// </summary>
        public static System.Collections.IEnumerable SelectMany(System.Collections.IEnumerable Source, string CollectionTechName)
        {
            var Collections = Source.Cast<object>()
                                .Select(item => Get(item, CollectionTechName) as System.Collections.IEnumerable);

            foreach (var Collection in Collections)
                foreach(var Item in Collection)
                    yield return Item;
        }
    }
}
