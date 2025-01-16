using System.Text.RegularExpressions;

namespace Alpha.Helpers
{
    public static class DirectiveReplacerExtension
    {
        public static IEnumerable<string> ExpandPlaceholders(string input, Dictionary<int, List<string>> replacements)
        {
            MatchCollection matches = Regex.Matches(input, @"\$(\d+)");
            List<int> placeholderKeys = [];
            foreach (Match match in matches)
            {
                int key = int.Parse(match.Groups[1].Value);
                if (!placeholderKeys.Contains(key))
                {
                    placeholderKeys.Add(key);
                }
            }

            // Start with an empty combination
            IEnumerable<IEnumerable<(int Key, string Value)>> combos =
                new List<IEnumerable<(int, string)>> { Enumerable.Empty<(int, string)>() };

            // Build combinations
            foreach (int key in placeholderKeys)
            {
                combos = combos.SelectMany(c =>
                    replacements[key].Select(val => c.Append((key, val))));
            }

            // Replace placeholders in each combination
            foreach (IEnumerable<(int Key, string Value)> combo in combos)
            {
                string result = input;
                foreach ((int k, string v) in combo)
                {
                    result = Regex.Replace(result, @"\$" + k + @"\b", v);
                }
                yield return result;
            }
        }

        public static bool HasPlaceholders(string input, int id)
        {
            return Regex.IsMatch(input, @"\$" + id + @"\b");
        }
    }
}
