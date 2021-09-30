using System;
using System.IO;

namespace Brainfuck.NET
{
	internal class Program
	{
		public static int Main(string[] args)
		{
			var parsedArgs = ParseArgs(args);
			if (parsedArgs == null)
			{
				Console.WriteLine("Incorrect input.");
				Console.WriteLine();
				ShowUsage();
				return 1;
			}

			if (parsedArgs.ShowHelp)
			{
				ShowUsage();
				return 0;
			}

			var error = ValidateArgs(parsedArgs);
			if (!string.IsNullOrEmpty(error))
			{
				Console.WriteLine(error);
				ShowHelpSuggestion();
				return 2;
			}

			var code = GetCode(parsedArgs.File, parsedArgs.Code);

			var compiler = new Compiler();
			compiler.Compile(code, new CompilerOptions
			{
				TapeLength = parsedArgs.TapeLength,
				OutputAssemblyPath = parsedArgs.Output
			});

			return 0;
		}

		private static string GetCode(string filePath, string code)
		{
			return !string.IsNullOrEmpty(code) ? code : File.ReadAllText(filePath);
		}

		private static string ValidateArgs(Args args)
		{
			if (!string.IsNullOrEmpty(args.File) && !string.IsNullOrEmpty(args.Code))
			{
				return "Specify either --file or --code key, but not both.";
			}

			if (string.IsNullOrEmpty(args.File) && string.IsNullOrEmpty(args.Code))
			{
				return "Specify either --file or --code key";
			}

			if (string.IsNullOrEmpty(args.Output))
			{
				return "Specify output exe file path";
			}

			return null;
		}

		private static Args ParseArgs(string[] args)
		{
			var result = new Args();

			for (var i = 0; i < args.Length; i++)
			{
				var isLast = i == args.Length - 1;

				var arg = args[i];

				if (arg == "--help")
				{
					return new Args { ShowHelp = true };
				}

				if (arg == "--file")
				{
					if (isLast)
					{
						return null;
					}

					result.File = args[++i];
				}

				if (arg == "--code")
				{
					if (isLast)
					{
						return null;
					}

					result.Code = args[++i];
				}

				if (arg == "--out")
				{
					if (isLast)
					{
						return null;
					}

					result.Output = args[++i];
				}

				if (arg == "--tape")
				{
					if (isLast)
					{
						return null;
					}

					result.TapeLength = int.Parse(args[++i]);
				}
			}

			return result;
		}

		private static void ShowUsage()
		{
			Console.WriteLine("USAGE: bfc.exe [--file source.b] [--code code] --out outputExe [--tape tapeLength] [--help]");
			Console.WriteLine();
			Console.WriteLine("--file - optional, source file with your brainfuck code");
			Console.WriteLine("--code - optional, source code to compile (no spaces or line breaks)");
			Console.WriteLine("--out  - output .exe file path");
			Console.WriteLine($"--tape - optional, size of the tape, default is {Compiler.DefaultTapeLength} cells");
			Console.WriteLine("--help - do nothing, just show help");
		}

		private static void ShowHelpSuggestion()
		{
			Console.WriteLine("Use --help key to get some help.");
		}

		private class Args
		{
			public string File { get; set; }

			public string Code { get; set; }

			public string Output { get; set; }

			public int TapeLength { get; set; } = Compiler.DefaultTapeLength;

			public bool ShowHelp { get; set; }
		}
	}
}