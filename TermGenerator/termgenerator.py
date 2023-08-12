from random import choice, randint
import sys


def print_args_error():
    print('\nUsage: termgenerator.py N LENGTH\nwhere\n' +
          '  N:      Number of terms to generate (must be even)\n' +
          '  LENGTH: Approximate length of each generated term\n' +
          'The output of this script can be used as input for the LazyMathInstructor program.')


def read_config():
    # Number of terms to be generated
    N = 20

    # Approximate length of the term to be generated
    LENGTH = 250

    if len(sys.argv) == 3:
        try:
            N = int(sys.argv[1])
            LENGTH = int(sys.argv[2])
        except ValueError:
            print_args_error()
            sys.exit(1)

        if (N <= 0) or (LENGTH <= 0) or (N % 2 == 1):
            print_args_error()
            sys.exit(1)

    elif len(sys.argv) != 1:
        print_args_error()
        sys.exit(1)

    return N, LENGTH



ATOMS = [chr(x) for x in range(ord('a'), ord('z') + 1)] * 4 + [str(x) for x in range(101)]
def random_atom():
    return choice(ATOMS)


def random_term(length):
    term = random_atom()

    while len(term) < length:
        choice = randint(0, 101)
        if choice < 25:
            term = f'({term})'
        elif choice < 45:
            term = f'{term}+{random_term(randint(1, (length - len(term))))}'
        elif choice < 75:
            term = f'{term}-{random_term(randint(1, (length - len(term))))}'
        else:
            term = f'{term}*{random_term(randint(1, (length - len(term))))}'

    return term


def main():
    N, LENGTH = read_config()

    print(N // 2)
    for i in range(N):
        print(random_term(LENGTH))



if __name__ == '__main__':
    main()
