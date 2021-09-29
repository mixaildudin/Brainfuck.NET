using System;

namespace Brainfuck.NET.Parsing
{
	public class BrainfuckParsingException : Exception
	{
		public BrainfuckParsingException(int position, string message)
			: base($"Position {position}: {message}")
		{
		}

		public BrainfuckParsingException(string message)
			: base(message)
		{
		}
	}
}