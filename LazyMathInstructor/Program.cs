namespace LazyMathInstructor
{
    public class Program
    {
        /// <summary>
        /// Indicates if the program was started with the --verbose flag
        /// and should output (intermediate) parsing results.
        /// </summary>
        public static bool Verbose { get; private set; }

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
                if (Program.Verbose) Console.WriteLine();
                Term second = Term.Parse(Console.ReadLine()!.Replace(" ", "").Replace("\t", ""));
                if (Program.Verbose) Console.WriteLine();
                terms.Add((first, second));
            }

            return terms;
        }

        /// <summary>
        /// Print help message when the program is called with -h or --help command line argument.
        /// </summary>
        static void PrintHelpMessage()
        {
            Console.WriteLine("LazyMathInstructor by Ludwig Kolesch\n" +
                "Source and explanation: https://github.com/Ludwig-Uni/Lazy-Math-Instructor\n" +
                "Usage: Enter the number of equivalences to test and all the terms via STDIN.\n" +
                "Terminate each input with a newline.\n" +
                "\n" +
                "Options:\n" +
                " -v, --verbose: Output parsed terms step-by-step\n" +
                " -h, --help:    Display this message");
        }

        /// <summary>
        /// Entry point of the program. Reads input from STDIN and prints YES or NO for each pair of terms
        /// entered, depending on whether they are equivalent, according to the specification.
        /// </summary>
        static void Main(string[] args)
        {
            if (args.Contains("-h") || args.Contains("--help"))
            {
                PrintHelpMessage();
                return;
            }

            Console.WriteLine("LazyMathInstructor by Ludwig Kolesch\nReading input from STDIN now.\n");

            Verbose = args.Contains("-v") || args.Contains("--verbose");

            var terms = GetInputTerms();
            foreach (var (first, second) in terms)
            {
                Console.WriteLine(first.Equals(second) ? "YES" : "NO");
                if (Verbose)
                {
                    Console.WriteLine("First:  " + first.ToString());
                    Console.WriteLine("Second: " + second.ToString());
                }
            }
        }
    }
}