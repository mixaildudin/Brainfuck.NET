# Brainfuck.NET

## What is it?
A [Brainfuck](https://ru.wikipedia.org/wiki/Brainfuck) compiler for .NET Framework.
Generates an .exe file which is ready to execute.

Emits optimal code by collapsing consecutive increments/decrements and data pointer movements:
e.g. `++-++` is just `+=4` instead of five separate instructions.

## Hot to use?

### CLI
`bfc.exe [--file source.b] [--code code] --out outputExe [--tape tapeLength] [--help]`

Here
* `--file` - optional, source file with your brainfuck code
* `--code` - optional, pass your source code in a command line argument (but no spaces or line breaks)
* `--out` - required, output .exe file path
* `--tape` - optional, `int`, size of the tape, default is 30 000 cells
* `--help` - do nothing, just show help

You must specify either `--file` or `--code`.

### Code
To be done...

## Why not .NET Core or .NET 5?
I mainly started this project for educational purposes: to understand how MSIL works.
I needed an .exe file generation, but in .NET Core / .NET 5 the `AssemblyBuilder` class
does not have a convenient `Save` method as it does in good old .NET Framework.

The project is written under 4.8. The compiled executables were tested under 4.8,
but might as well work under older versions.

Maybe I will port this project to .NET 5 or 6 making use of [`Mono.Cecil`](https://github.com/jbevain/cecil)
or [`ILPack`](https://github.com/Lokad/ILPack) in the future.
