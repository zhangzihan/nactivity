using System.Collections.Generic;
using System.Linq;

namespace Sys.Rules.DmnEngine.Feel.Parsing
{
    /// <summary>
    /// Registry of known names (variables, functions) for scope-aware FEEL parsing.
    /// FEEL allows multi-word names (e.g. "string length", "date and time") and this
    /// scope is consulted during token merging to resolve ambiguity.
    /// </summary>
    public class FeelScope
    {
        /// <summary>
        /// Known multi-word names stored as arrays of individual words.
        /// Single-word names don't need special handling during lexing.
        /// </summary>
        private readonly List<string[]> multiWordNames = new();

        /// <summary>
        /// All known names (single and multi-word) for quick lookup.
        /// </summary>
        private readonly HashSet<string> allNames = new();

        /// <summary>
        /// Built-in multi-word function names from the FEEL specification.
        /// </summary>
        private static readonly string[] BuiltInMultiWordNames =
        {
            // String functions
            "string length",
            "upper case",
            "lower case",
            "substring before",
            "substring after",
            "starts with",
            "ends with",
            "string join",

            // List functions
            "list contains",
            "insert before",
            "distinct values",
            "index of",
            "list replace",

            // Numeric functions
            "round up",
            "round down",
            "round half up",
            "round half down",

            // Date/time functions
            "date and time",
            "years and months duration",
            "day of year",
            "day of week",
            "month of year",
            "week of year",

            // Context functions
            "get value",
            "get entries",
            "context put",
            "context merge",

            // Range functions
            "met by",
            "overlaps before",
            "overlaps after",
            "started by",
            "finished by",
        };

        public FeelScope()
        {
            foreach (var name in BuiltInMultiWordNames)
            {
                AddName(name);
            }
        }

        /// <summary>
        /// Add a name to the scope. If it contains spaces, it's registered as a multi-word name.
        /// </summary>
        public void AddName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            allNames.Add(name);
            var parts = name.Split(' ');
            if (parts.Length > 1)
            {
                multiWordNames.Add(parts);
            }
        }

        /// <summary>
        /// Check if a name is known in this scope.
        /// </summary>
        public bool ContainsName(string name) => allNames.Contains(name);

        /// <summary>
        /// Given a starting word, find all multi-word names that begin with that word.
        /// Returns candidate name parts arrays for longest-match resolution.
        /// </summary>
        public IReadOnlyList<string[]> GetMultiWordCandidates(string firstWord)
        {
            return multiWordNames.Where(parts => parts[0] == firstWord).ToList();
        }
    }
}

