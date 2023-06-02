﻿namespace LazyMathInstructor
{
    /// <summary>
    /// Represents a product of 0 or more variables, each with an exponent.
    /// For example one <see cref="VariableCombination"/> would be "a^3 b^4 c"
    /// </summary>
    public class VariableCombination : IComparable
    {
        /// <summary>
        /// Maps all variables appearing in this variable combination to their respective exponents.
        /// Variables with exponent 0 do not appear in the dictionary.
        /// </summary>
        public SortedDictionary<Variable, int> Exponents { get; }

        /// <summary>
        /// Construct a <see cref="VariableCombination"/> with no variables.
        /// </summary>
        public VariableCombination()
        {
            Exponents = new SortedDictionary<Variable, int>();
        }

        /// <summary>
        /// Construct a <see cref="VariableCombination"/> with one variable <paramref name="v"/>
        /// that has an exponent of 1.
        /// </summary>
        public VariableCombination(Variable v)
        {
            Exponents = new SortedDictionary<Variable, int>()
            {
                { v, 1 }
            };
        }

        /// <summary>
        /// Construct a <see cref="VariableCombination"/> that copies the exponents from the
        /// existing dictionary <paramref name="exponents"/> (of another <see cref="VariableCombination"/>).
        /// </summary>
        private VariableCombination(SortedDictionary<Variable, int> exponents)
        {
            Exponents = new SortedDictionary<Variable, int>(exponents);
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
                }
                else
                {
                    result.Exponents.Add(x.Key, x.Value);
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
            var hash = new HashCode();
            foreach (var exponent in Exponents)
            {
                hash.Add((exponent.Key, exponent.Value));
            }
            return hash.ToHashCode();
        }

        /// <summary>
        /// Overloaded equality check based solely on the content of the <see cref="Exponents"/>.
        /// If all entries in the <see cref="Exponents"/> dictionary match, 
        /// the <see cref="VariableCombination"/>s are considered to be equal.
        /// This is needed to use and compare them, e.g. as a key in Dictionaries.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is not VariableCombination other) return false;
            if (this.Exponents.Count != other.Exponents.Count) return false;

            var thisEnum = this.Exponents.GetEnumerator();
            var otherEnum = other.Exponents.GetEnumerator();

            while (thisEnum.MoveNext())
            {
                otherEnum.MoveNext();

                if (thisEnum.Current.Key != otherEnum.Current.Key || thisEnum.Current.Value != otherEnum.Current.Value)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Pretty-Printing for debugging purposes / verbose output.
        /// </summary>
        public override string ToString()
        {
            string result = "";
            foreach (var varTerm in Exponents.OrderBy(x => x.Key))
            {
                var exponent = varTerm.Value switch
                {
                    1 => "",
                    < 10 => $"^{varTerm.Value}",
                    _ => $"^({varTerm.Value})",
                };

                result += varTerm.Key.ToString().ToLower() + exponent;
            }
            return result;
        }

        /// <summary>
        /// Compares two <see cref="VariableCombination"/>s to establish an ordering. 
        /// Earlier letters and higher exponents are ordered before later letters and smaller exponents:
        /// a^3 < a^2 < ab < b^2 < c
        /// </summary>
        public int CompareTo(object obj)
        {
            if (obj is not VariableCombination other)
                throw new ArgumentException("Can't compare variable combination to object of different type!");

            for (Variable v = Variable.A; v <= Variable.Z; v++)
            {
                int thisExponent = Exponents.GetValueOrDefault(v);
                int otherExponent = other.Exponents.GetValueOrDefault(v);
                if (thisExponent != otherExponent)
                    return otherExponent - thisExponent; // Greater exponent precedes smaller exponent in ordering
            }

            return 0;
        }
    }
}