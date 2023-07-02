# Lazy Math Instructor

## Introduction
This is my code for the proseminar [Selected Fun Problems of the ACM Programming Contest](https://db.cs.uni-tuebingen.de/teaching/ss23/acm-programming-contest-proseminar/) at the University of Tübingen, Summer Term 2023.

The problem worked on is the [Lazy Math Instructor](./Lazy-Math-Instructor.pdf): The exercise is to check arbitrary terms for equivalence.
Terms may contain brackets, addition, subtraction and multiplication of integer constants and variables `a` to `z`.

They are evaluated from left to right with no operator precedence.

## Usage of the program

The program is written in C# .NET 7.0 and should run on any supported platform.

Program reads from STDIN:
- 1 line with the number `n` of equivalences to test
- 2`n` lines with a term each.

Program writes to STDOUT:
- `n` lines, where the `k`th line is
  - `YES` if the `k`-th pair of terms are equivalent
  - `NO` if they aren't.

Verbose mode can be enabled by passing `-v` as a command line argument, in which case the program will print each term after it was parsed, normalized and ordered deterministically.