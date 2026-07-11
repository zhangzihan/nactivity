using System.Collections.Generic;
using Antlr4.Runtime;

namespace Sys.Rules.DmnEngine.Feel.Parsing
{
    /// <summary>
    /// Post-lexer token source that merges adjacent NAME tokens into multi-word names
    /// using longest-match semantics. Consults a <see cref="FeelScope"/> for known names.
    /// Also considers FEEL keyword tokens (and, or, of, not, etc.) as potential
    /// components of multi-word names like "date and time", "index of", etc.
    /// </summary>
    internal class FeelNameResolver : ITokenSource
    {
        private readonly ITokenSource source;
        private readonly FeelScope scope;
        private readonly Queue<IToken> buffer = new();
        private readonly CommonTokenStream fullStream;
        private int position;

        /// <summary>
        /// Token types that can appear as words within multi-word names.
        /// FEEL keywords like "and", "of", "not" can be part of function names.
        /// </summary>
        private static readonly HashSet<int> NameLikeTokens = new()
        {
            FeelLexer.NAME,
            FeelLexer.AND,  // "date and time", "years and months duration"
            FeelLexer.OF,   // "index of", "day of year", "month of year", "week of year", "instance of"
            FeelLexer.NOT,  // "not" can be a function name
            FeelLexer.IN,
        };

        public FeelNameResolver(CommonTokenStream fullStream, FeelScope scope)
        {
            this.fullStream = fullStream;
            this.source = fullStream.TokenSource;
            this.scope = scope;
            this.position = 0;
            // Fill the stream so we can random-access tokens
            fullStream.Fill();
        }

        public IToken NextToken()
        {
            if (buffer.Count > 0) return buffer.Dequeue();

            if (position >= fullStream.Size)
                return fullStream.Get(fullStream.Size - 1); // EOF

            var token = fullStream.Get(position);

            if (NameLikeTokens.Contains(token.Type))
            {
                var merged = TryMergeMultiWordName(position);
                if (merged != null)
                {
                    return merged;
                }
            }

            position++;
            return token;
        }

        /// <summary>
        /// Attempt to merge consecutive NAME tokens (separated only by whitespace)
        /// into a single NAME token using longest-match against known scope names.
        /// </summary>
        private IToken TryMergeMultiWordName(int startPos)
        {
            var firstToken = fullStream.Get(startPos);
            var firstWord = firstToken.Text;
            var candidates = scope.GetMultiWordCandidates(firstWord);

            if (candidates.Count == 0)
                return null; // No multi-word names start with this word

            // Collect subsequent NAME tokens (skipping hidden WS tokens)
            var nameTokens = new List<IToken> { firstToken };
            var words = new List<string> { firstWord };
            var scanPos = startPos + 1;

            // Look ahead for more NAME tokens separated only by whitespace
            while (scanPos < fullStream.Size)
            {
                var nextToken = fullStream.Get(scanPos);

                // Skip whitespace on hidden channel
                if (nextToken.Channel != Lexer.DefaultTokenChannel)
                {
                    scanPos++;
                    continue;
                }

                // Must be a name-like token to continue merging
                if (!NameLikeTokens.Contains(nextToken.Type))
                    break;

                words.Add(nextToken.Text);
                nameTokens.Add(nextToken);
                scanPos++;
            }

            if (words.Count <= 1)
                return null; // Only one word, no merging possible

            // Find longest match among candidates
            string bestMatch = null;
            var bestMatchWordCount = 0;

            foreach (var candidateParts in candidates)
            {
                if (candidateParts.Length > words.Count) continue;

                var matches = true;
                for (var i = 0; i < candidateParts.Length; i++)
                {
                    if (candidateParts[i] != words[i])
                    {
                        matches = false;
                        break;
                    }
                }

                if (matches && candidateParts.Length > bestMatchWordCount)
                {
                    bestMatch = string.Join(" ", candidateParts);
                    bestMatchWordCount = candidateParts.Length;
                }
            }

            if (bestMatch == null)
                return null; // No multi-word name matched

            // Create merged token spanning from first to last matched NAME token
            var lastMatchedToken = nameTokens[bestMatchWordCount - 1];
            var mergedToken = new CommonToken(firstToken)
            {
                Type = FeelLexer.NAME,
                Text = bestMatch,
                StopIndex = lastMatchedToken.StopIndex
            };

            // Advance position past all consumed tokens (including hidden WS between them)
            // Find the position after the last matched NAME token
            var consumed = 0;
            var pos = startPos;
            while (pos < fullStream.Size && consumed < bestMatchWordCount)
            {
                var t = fullStream.Get(pos);
                if (t.Channel == Lexer.DefaultTokenChannel)
                    consumed++;
                pos++;
            }
            position = pos;

            return mergedToken;
        }

        public int Line => source.Line;
        public int Column => source.Column;
        public ICharStream InputStream => source.InputStream;
        public string SourceName => source.SourceName;
        public ITokenFactory TokenFactory
        {
            get => source.TokenFactory;
            set => source.TokenFactory = value;
        }
    }
}

