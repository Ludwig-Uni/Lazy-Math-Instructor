# Lazy Math Instructor

## Introduction
This is my code for the proseminar [Selected Fun Problems of the ACM Programming Contest](https://db.cs.uni-tuebingen.de/teaching/ss23/acm-programming-contest-proseminar/)
at the University of Tübingen, Summer Term 2023.

The problem worked on is *Lazy Math Instructor*: The exercise is to check arbitrary terms for equivalence.
Terms may contain brackets, addition, subtraction and multiplication of integer constants and variables `a` to `z`.

They are evaluated from left to right with equal operator precedence for all operations.


## Contents

This repository contains:
- The [problem description](./Lazy-Math-Instructor.pdf)
- My [presentation slides](./Slides.pdf)
- My [source code](./LazyMathInstructor) (split in several `.cs` files)
- Several input examples: [Binomial formulas](./binomial_formulas.txt), [Examples from the problem description](./problem_examples.txt) and a [long term](./long_term.txt)


## Usage of the program

The program is written in C# .NET 7.0 and should run on any supported platform.

Program reads from STDIN:
- 1 line with the number `n` of equivalences to test
- 2`n` lines with a term each.

Program writes to STDOUT:
- `n` lines, where the `k`th line is
  - `YES` if the `k`-th pair of terms are equivalent
  - `NO` if they aren't.

Verbose mode can be enabled by passing `-v` as a command line argument, in which case the program will print each addition, 
subtraction and multiplication operation it performs, as well as each term after it was parsed, normalized and ordered deterministically.