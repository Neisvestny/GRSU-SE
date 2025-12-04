using System.Text.RegularExpressions;

namespace GRSU_SE
{
    public static class Parser
    {
        public static List<object> ParseSentenceTokens(string sentenceText)
        {
            var tokens = new List<object>();
            var matches = Regex.Matches(sentenceText, @"\w+|[^\w\s]");

            foreach (Match match in matches)
            {
                if (Regex.IsMatch(match.Value, @"\w+"))
                    tokens.Add(new Word(match.Value));
                else
                    tokens.Add(new Punctuation(match.Value));
            }

            return tokens;
        }

        public static Text ParseText(string text)
        {
            var parsedText = new Text();
            var sentences = Regex.Split(text, @"(?<=[.!?])\s+");

            foreach (var sentenceText in sentences)
            {
                if (!string.IsNullOrWhiteSpace(sentenceText))
                {
                    parsedText.Sentences.Add(new Sentence(sentenceText.Trim()));
                }
            }

            return parsedText;
        }
    }
}