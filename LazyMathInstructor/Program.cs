namespace LazyMathInstructor
{
    public enum Variable : byte
    {
        A, B, C, D, E, F, G, H, I, J, K, L, M,
        N, O, P, Q, R, S, T, U, V, W, X, Y, Z
    }

    public class VariableCombination
    {
        public Dictionary<Variable, int> Exponents { get; }

        public VariableCombination()
        {
            Exponents = new Dictionary<Variable, int>();
        }

        public VariableCombination(Variable v)
        {
            Exponents = new Dictionary<Variable, int>()
            {
                { v, 1 }
            };
        }

        private VariableCombination(Dictionary<Variable, int> exponents)
        {
            Exponents = new Dictionary<Variable, int>(exponents);
        }

        public static VariableCombination operator *(VariableCombination first, VariableCombination second)
        {
            var result = new VariableCombination(first.Exponents);
            foreach (var x in second.Exponents)
            {
                if (result.Exponents.ContainsKey(x.Key))
                {
                    result.Exponents[x.Key] += x.Value;
                    if (result.Exponents[x.Key] == 0) result.Exponents.Remove(x.Key);
                }
                else
                {
                    if (x.Value != 0) result.Exponents.Add(x.Key, x.Value);
                }
            }
            return result;
        }

        public override int GetHashCode()
        {
            unchecked // FIXME TODO improve this
            {
                return Exponents.Sum(x => x.Key.GetHashCode() * x.Value.GetHashCode());
            }
        }

        public override bool Equals(object? obj)
        {
            return obj is VariableCombination other
                && Exponents.Count == other.Exponents.Count
                && !Exponents.Except(other.Exponents).Any();
        }
    }

    public class Term
    {
        private Dictionary<VariableCombination, int> Coefficients { get; }

        private static Variable ParseVariable(char c)
        {
            return Enum.Parse<Variable>(c.ToString().ToUpper());
        }

        private static int FindMatchingOpeningBracket(string s)
        {
            // s[^1] == ')', we need to find the matching '('.
            int bracketsClosed = 1;
            for (int i = s.Length - 2; i >= 0; i--)
            {
                switch (s[i])
                {
                    case '(':
                        bracketsClosed--;
                        if (bracketsClosed == 0) return i;
                        break;

                    case ')':
                        bracketsClosed++;
                        break;
                }
            }

            // This will not happen if the input is well-formatted.
            throw new Exception("Malformed term: No matching closing bracket found");
        }

        private Term()
        {
            Coefficients = new Dictionary<VariableCombination, int>();
        }

        private Term(int constant)
        {
            Coefficients = new Dictionary<VariableCombination, int>()
            {
                { new VariableCombination(), constant }
            };
        }

        private Term(Variable variable)
        {
            Coefficients = new Dictionary<VariableCombination, int>()
            {
                { new VariableCombination(variable), 1 }
            };
        }

        public static Term Parse(string s)
        {
            Term lastTerm;
            int restIndex;

            // If the string ends in an integer, first index of that integer, otherwise s.Length
            // (might be 0 if the entire string is an integer constant)
            int integerStartsAt = 0;
            for (int j = s.Length - 1; j >= 0; j--)
            {
                if (!(s[j] >= '0' && s[j] <= '9'))
                {
                    integerStartsAt = j + 1;
                    break;
                }
            }

            if (integerStartsAt < s.Length)
            {
                lastTerm = new Term(int.Parse(s[integerStartsAt..]));
                restIndex = integerStartsAt;
            }
            else if (s[^1] == ')')
            {
                int openingBracketIndex = FindMatchingOpeningBracket(s);
                lastTerm = Term.Parse(s[(openingBracketIndex + 1)..^1]);
                restIndex = openingBracketIndex;
            }
            else // We have a variable 'a' to 'z'
            {
                Variable variable = ParseVariable(s[^1]);
                lastTerm = new Term(variable);
                restIndex = s.Length - 1;
            }

            if (restIndex <= 0) return lastTerm;

            return s[restIndex - 1] switch
            {
                '+' => Term.Parse(s[..(restIndex - 1)]) + lastTerm,
                '-' => Term.Parse(s[..(restIndex - 1)]) - lastTerm,
                '*' => Term.Parse(s[..(restIndex - 1)]) * lastTerm,
                _ => throw new Exception("Malformed term: expected operator after sub-term"),
            };
        }

        public static Term operator +(Term first, Term second)
        {
            var result = new Term();
            foreach (VariableCombination varCombo in first.Coefficients.Keys.Union(second.Coefficients.Keys))
            {
                int coefficient = first.Coefficients.GetValueOrDefault(varCombo)
                    + second.Coefficients.GetValueOrDefault(varCombo);

                if (coefficient != 0)
                    result.Coefficients.Add(varCombo, coefficient);
            }

            return result;
        }

        public static Term operator -(Term first, Term second)
        {
            var result = new Term();
            foreach (VariableCombination varCombo in first.Coefficients.Keys.Union(second.Coefficients.Keys))
            {
                int coefficient = first.Coefficients.GetValueOrDefault(varCombo)
                    - second.Coefficients.GetValueOrDefault(varCombo);

                if (coefficient != 0)
                    result.Coefficients.Add(varCombo, coefficient);
            }

            return result;
        }

        public static Term operator *(Term first, Term second)
        {
            var result = new Term();
            foreach (VariableCombination varCombo1 in first.Coefficients.Keys)
            {
                foreach (VariableCombination varCombo2 in second.Coefficients.Keys)
                {
                    VariableCombination productVar = varCombo1 * varCombo2;

                    if (result.Coefficients.ContainsKey(productVar))
                    {
                        result.Coefficients[productVar] += first.Coefficients[varCombo1] * second.Coefficients[varCombo2];
                        if (result.Coefficients[productVar] == 0)
                            result.Coefficients.Remove(productVar);
                    }
                    else
                    {
                        if (first.Coefficients[varCombo1] * second.Coefficients[varCombo2] != 0)
                            result.Coefficients.Add(productVar, first.Coefficients[varCombo1] * second.Coefficients[varCombo2]);
                    }
                }
            }
            return result;
        }

        public override int GetHashCode()
        {
            unchecked // FIXME TODO improve this
            {
                return Coefficients.Sum(x => x.Key.GetHashCode() * x.Value.GetHashCode());
            }
        }

        public override bool Equals(object? obj)
        {
            return obj is Term other
                && Coefficients.Count == other.Coefficients.Count
                && !Coefficients.Except(other.Coefficients).Any();
        }
    }

    internal class Program
    {
        static List<(Term First, Term Second)> GetInputTerms()
        {
            var terms = new List<(Term First, Term Second)>();

            int n = int.Parse(Console.ReadLine()!);
            for (int i = 0; i < n; i++)
            {
                Term first = Term.Parse(Console.ReadLine()!.Replace(" ", "").Replace("\t", ""));
                Term second = Term.Parse(Console.ReadLine()!.Replace(" ", "").Replace("\t", ""));
                terms.Add((first, second));
            }

            return terms;
        }

        static void Main()
        {
            var terms = GetInputTerms();
            Console.WriteLine(string.Join("\n",
                                          terms.Select(x => x.First.Equals(x.Second)
                                                                ? "YES"
                                                                : "NO")));
        }
    }
}