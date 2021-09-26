using System;

namespace Brainfuck.NET
{
	internal class Program
	{
		public static int Main(string[] args)
		{
			if (args.Length < 2)
			{
				Console.WriteLine("USAGE: Brainfuck.NET.exe code outputExeName");
				return 1;
			}

			var compiler = new Compiler();
			compiler.Compile(args[0], args[1]);

			return 0;
		}
	}
}