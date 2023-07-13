namespace LazyMathInstructor
{
    /// <summary>
    /// Represents a polynomial which can be compared to another polynomial to check for equivalence.
    /// </summary>
    public class Polynomial
    {
        /// <summary>
        /// Maps all <see cref="VariableProduct"/>s in this polynomial to their coefficients
        /// (that is, the constant factor). Two polynomials are equivalent iff their <see cref="Coefficients"/> match.
        /// </summary>
        private SortedDictionary<VariableProduct, int> Coefficients { get; }

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
        /// Constructs a polynomial representing an integer constant.
        /// </summary>
        private Polynomial(int constant = 0)
        {
            Coefficients = new SortedDictionary<VariableProduct, int>();
            if (constant != 0)
                Coefficients.Add(new VariableProduct(), constant);
        }

        /// <summary>
        /// Constructs a polynomial representing a single variable (with exponent 1).
        /// </summary>
        private Polynomial(Variable variable)
        {
            Coefficients = new SortedDictionary<VariableProduct, int>()
            {
                { new VariableProduct(variable), 1 }
            };
        }

        /// <summary>
        /// Helper method: Output string <paramref name="s"/> to STDOUT with console color <paramref name="c"/>, 
        /// restoring the original console color afterwards.
        /// </summary>
        private static void PrintColored(string s, ConsoleColor c)
        {
            ConsoleColor prevColor = Console.ForegroundColor;
            Console.ForegroundColor = c;
            Console.Write(s);
            Console.ForegroundColor = prevColor;
        }

        /// <summary>
        /// Helper method: Output information about a polynomial calculation to STDOUT.
        /// <paramref name="first"/> <paramref name="symbol"/> <paramref name="second"/> = <paramref name="result"/>,
        /// where <paramref name="symbol"/> is the symbol (+, -, *) of the performed <paramref name="operation"/>.
        /// </summary>
        private static void PrintOperation(Polynomial first, Polynomial second, Polynomial result, string operation, char symbol)
        {
            Console.Write(operation);
            PrintColored("(", ConsoleColor.Red);
            Console.Write(first.ToString());
            PrintColored($") {symbol} (", ConsoleColor.Red);
            Console.Write(second.ToString());
            PrintColored(") = ", ConsoleColor.Red);
            PrintColored(result.ToString(), ConsoleColor.Green);
            Console.WriteLine();
        }

        /// <summary>
        /// Parses the term <paramref name="s"/> to a normalized polynomial that can be compared to other polynomials.
        /// Parsing is done recursively for sub-terms in brackets, and terms are added/subtracted/multiplied
        /// left-to-right. This is done by parsing the last term and recursively parsing all terms before that.
        /// </summary>
        /// <exception cref="ArgumentException">If two sub-terms are not connected by a valid binary operator</exception>
        public static Polynomial Parse(string s)
        {
            Polynomial lastTerm;
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

            if (integerStartsAt < s.Length) // The term ends with an integer constant, parse that
            {
                int integerConstant = int.Parse(s[integerStartsAt..]);
                lastTerm = new Polynomial(integerConstant);
                restIndex = integerStartsAt;
            }
            else if (s[^1] == ')') // The term ends with a sub-term in parentheses, parse that
            {
                int openingBracketIndex = FindMatchingOpeningBracket(s);
                lastTerm = Polynomial.Parse(s[(openingBracketIndex + 1)..^1]);
                restIndex = openingBracketIndex;
            }
            else // The term ends in a variable 'a' to 'z', parse that
            {
                Variable variable = ParseVariable(s[^1]);
                lastTerm = new Polynomial(variable);
                restIndex = s.Length - 1;
            }

            if (restIndex <= 0) // We finished parsing the entire term, there's nothing more to the left to parse
                return lastTerm;


            Polynomial leftTerm = Polynomial.Parse(s[..(restIndex - 1)]);
            char operation = s[restIndex - 1];

            // Recursively parse the rest of the term to the left, *then* apply the correct operation between both subterms
            return operation switch
            {
                '+' => leftTerm + lastTerm,
                '-' => leftTerm - lastTerm,
                '*' => leftTerm * lastTerm,
                _ => throw new ArgumentException("Malformed term: expected operator after sub-term", nameof(s)),
            };
        }

        /// <summary>
        /// Operator for addition of two polynomials. The coefficients of matching variable products are added.
        /// </summary>
        public static Polynomial operator +(Polynomial first, Polynomial second)
        {
            var result = new Polynomial();
            foreach (VariableProduct varCombo in first.Coefficients.Keys.Union(second.Coefficients.Keys))
            {
                int coefficient = first.Coefficients.GetValueOrDefault(varCombo)
                    + second.Coefficients.GetValueOrDefault(varCombo);

                if (coefficient != 0)
                    result.Coefficients.Add(varCombo, coefficient);
            }

            if (Program.Verbose)
                PrintOperation(first, second, result, "Add:  ", '+');

            return result;
        }

        /// <summary>
        /// Operator for subtraction of two polynomials. The coefficients of matching variable products are subtracted.
        /// </summary>
        public static Polynomial operator -(Polynomial first, Polynomial second)
        {
            var result = new Polynomial();
            foreach (VariableProduct varCombo in first.Coefficients.Keys.Union(second.Coefficients.Keys))
            {
                int coefficient = first.Coefficients.GetValueOrDefault(varCombo)
                    - second.Coefficients.GetValueOrDefault(varCombo);

                if (coefficient != 0)
                    result.Coefficients.Add(varCombo, coefficient);
            }

            if (Program.Verbose)
                PrintOperation(first, second, result, "Sub:  ", '-');

            return result;
        }

        /// <summary>
        /// Operator for multiplication of two polynomials. All variable products and corresponding coefficients are
        /// multiplied pairwise (i.e. (4a + 3b) * 2c = 8ac + 6bc)
        /// </summary>
        public static Polynomial operator *(Polynomial first, Polynomial second)
        {
            var result = new Polynomial();
            foreach (VariableProduct varCombo1 in first.Coefficients.Keys)
            {
                foreach (VariableProduct varCombo2 in second.Coefficients.Keys)
                {
                    VariableProduct productVar = varCombo1 * varCombo2;

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

            if (Program.Verbose)
                PrintOperation(first, second, result, "Mult: ", '*');

            return result;
        }

        /// <summary>
        /// Overloaded method to get hash code based solely on the content of the <see cref="Coefficients"/>,
        /// since two <see cref="Polynomial"/>s with matching <see cref="Coefficients"/> are considered equal.
        /// </summary>
        public override int GetHashCode()
        {
            var hash = new HashCode();
            foreach (var coefficient in Coefficients)
            {
                hash.Add((coefficient.Key, coefficient.Value));
            }
            return hash.ToHashCode();
        }

        /// <summary>
        /// Overloaded equality check based solely on the content of the <see cref="Coefficients"/>.
        /// If all entries in the <see cref="Coefficients"/> dictionary match, 
        /// the <see cref="Polynomial"/>s are considered to be equal.
        /// This is needed to use and compare them (Polynomial a is equivalent to Polynomial b iff a.Equals(b)).
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is not Polynomial other) return false;
            if (this.Coefficients.Count != other.Coefficients.Count) return false;

            var thisEnum = this.Coefficients.GetEnumerator();
            var otherEnum = other.Coefficients.GetEnumerator();

            while (thisEnum.MoveNext())
            {
                otherEnum.MoveNext();

                if (!thisEnum.Current.Key.Equals(otherEnum.Current.Key) || thisEnum.Current.Value != otherEnum.Current.Value)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Pretty-Printing for debugging purposes / verbose output.
        /// </summary>
        public override string ToString()
        {
            return string.Join(" + ", Coefficients.Select(x => (x.Value == 1 && x.Key.Exponents.Any() ? "" : x.Value)
                                                                   + x.Key.ToString()));
        }
    }
}