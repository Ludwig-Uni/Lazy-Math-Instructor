namespace LazyMathInstructor
{
    /// <summary>
    /// Represents a variable a, b, ..., z of the term
    /// </summary>
    public enum Variable : byte
    {
        A, B, C, D, E, F, G, H, I, J, K, L, M,
        N, O, P, Q, R, S, T, U, V, W, X, Y, Z
    }

    /// <summary>
    /// Represents a product of 0 or more variables, each with an exponent.
    /// For example one <see cref="VariableCombination"/> would be "a^3 b^4 c"
    /// </summary>
    public class VariableCombination
    {
        /// <summary>
        /// Maps all variables appearing in this variable combination to their respective exponents.
        /// Variables with exponent 0 do not appear in the dictionary.
        /// </summary>
        public Dictionary<Variable, int> Exponents { get; }

        /// <summary>
        /// Construct a <see cref="VariableCombination"/> with no variables.
        /// </summary>
        public VariableCombination()
        {
            Exponents = new Dictionary<Variable, int>();
        }

        /// <summary>
        /// Construct a <see cref="VariableCombination"/> with one variable <paramref name="v"/>
        /// that has an exponent of 1.
        /// </summary>
        public VariableCombination(Variable v)
        {
            Exponents = new Dictionary<Variable, int>()
            {
                { v, 1 }
            };
        }

        /// <summary>
        /// Construct a <see cref="VariableCombination"/> that copies the exponents from the
        /// existing dictionary <paramref name="exponents"/> (of another <see cref="VariableCombination"/>).
        /// </summary>
        private VariableCombination(Dictionary<Variable, int> exponents)
        {
            Exponents = new Dictionary<Variable, int>(exponents);
        }

        /// <summary>
        /// Operator for multiplication of two variable combinations.
        /// <paramref name="first"/> * <paramref name="second"/> is calculated by
        /// adding the exponents of all variables in the combination (i.e. ab * b^2 = a b^3).
        /// </summary>
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

        /// <summary>
        /// Overloaded method to get hash code based solely on the content of the <see cref="Exponents"/>,
        /// since two <see cref="VariableCombination"/>s with matching <see cref="Exponents"/> are considered equal.
        /// </summary>
        public override int GetHashCode()
        {
            unchecked // FIXME TODO improve this
            {
                return Exponents.Sum(x => x.Key.GetHashCode() * x.Value.GetHashCode());
            }
        }

        /// <summary>
        /// Overloaded equality check based solely on the content of the <see cref="Exponents"/>.
        /// If all entries in the <see cref="Exponents"/> dictionary match, 
        /// the <see cref="VariableCombination"/>s are considered to be equal.
        /// This is needed to use and compare them, e.g. as a key in Dictionaries.
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is VariableCombination other
                && Exponents.Count == other.Exponents.Count
                && !Exponents.Except(other.Exponents).Any();
        }
    }

    /// <summary>
    /// Represents a (normalized) term which can be compared to another term to check for equivalence.
    /// </summary>
    public class Term
    {
        /// <summary>
        /// Maps all <see cref="VariableCombination"/>s in this term to their coefficients
        /// (that is, the constant factor). Two terms are equivalent iff their <see cref="Coefficients"/> match.
        /// </summary>
        private Dictionary<VariableCombination, int> Coefficients { get; }

        /// <summary>
        /// Parse the character <paramref name="c"/> between 'a' and 'z' to its
        /// representation as a <see cref="Variable"/>.
        /// </summary>
        private static Variable ParseVariable(char c)
        {
            return Enum.Parse<Variable>(c.ToString().ToUpper());
        }

        /// <summary>
        /// For the string <paramref name="s"/> ending with a ')' character, finds
        /// the index in the string where the *corresponding* '(' character is.
        /// </summary>
        /// <exception cref="ArgumentException">If <paramref name="s"/> doesn't contain a corresponding '('</exception>
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
            throw new ArgumentException("Malformed term: No matching closing bracket found", nameof(s));
        }

        /// <summary>
        /// Constructs an empty term.
        /// </summary>
        private Term()
        {
            Coefficients = new Dictionary<VariableCombination, int>();
        }

        /// <summary>
        /// Constructs a term representing an integer constant.
        /// </summary>
        private Term(int constant)
        {
            Coefficients = new Dictionary<VariableCombination, int>()
            {
                { new VariableCombination(), constant }
            };
        }

        /// <summary>
        /// Constructs a term representing a single variable (with exponent 1).
        /// </summary>
        private Term(Variable variable)
        {
            Coefficients = new Dictionary<VariableCombination, int>()
            {
                { new VariableCombination(variable), 1 }
            };
        }

        /// <summary>
        /// Parses the string <paramref name="s"/> to a normalized term that can be compared to other terms.
        /// Parsing is done recursively for sub-terms in brackets, and terms are added/subtracted/multiplied
        /// left-to-right. This is done by parsing the last term and recursively parsing all terms before that.
        /// </summary>
        /// <exception cref="ArgumentException">If two sub-terms are not connected by a valid binary operator</exception>
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
                _ => throw new ArgumentException("Malformed term: expected operator after sub-term", nameof(s)),
            };
        }

        /// <summary>
        /// Operator for addition of two terms. The coefficients of matching variable combinations are added.
        /// </summary>
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

        /// <summary>
        /// Operator for subtraction of two terms. The coefficients of matching variable combinations are subtracted.
        /// </summary>
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

        /// <summary>
        /// Operator for multiplication of two terms. All variable combinations and corresponding coefficients are
        /// multiplied pairwise (i.e. (4a + 3b) * 2c = 8ac + 6bc)
        /// </summary>
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

        /// <summary>
        /// Overloaded method to get hash code based solely on the content of the <see cref="Coefficients"/>,
        /// since two <see cref="Term"/>s with matching <see cref="Coefficients"/> are considered equal.
        /// </summary>
        public override int GetHashCode()
        {
            unchecked // FIXME TODO improve this
            {
                return Coefficients.Sum(x => x.Key.GetHashCode() * x.Value.GetHashCode());
            }
        }

        /// <summary>
        /// Overloaded equality check based solely on the content of the <see cref="Coefficients"/>.
        /// If all entries in the <see cref="Coefficients"/> dictionary match, 
        /// the <see cref="Term"/>s are considered to be equal.
        /// This is needed to use and compare them (Term a is equivalent to Term b iff a.Equals(b)).
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is Term other
                && Coefficients.Count == other.Coefficients.Count
                && !Coefficients.Except(other.Coefficients).Any();
        }
    }

    internal class Program
    {
        /// <summary>
        /// Reads the input from STDIN, parses the terms and returns them in a list of tuples
        /// of two terms each, that are to be checked for equivalence.
        /// </summary>
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

        /// <summary>
        /// Entry point of the program. Reads input from STDIN and prints YES or NO for each pair of terms
        /// entered, depending on whether they are equivalent, according to the specification.
        /// </summary>
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