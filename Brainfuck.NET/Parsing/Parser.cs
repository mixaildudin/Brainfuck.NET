using System;
using System.Collections.Generic;
using System.Linq;

namespace Brainfuck.NET.Parsing
{
	public class Parser
	{
		private readonly string _code;

		private static readonly Dictionary<char, int> CumulativeInstructionsIncrements = new()
		{
			{ '+', 1 },
			{ '-', -1 },
			{ '>', 1 },
			{ '<', -1 },
		};

		public Parser(string code)
		{
			if (string.IsNullOrEmpty(code))
			{
				throw new ArgumentOutOfRangeException(nameof(code), $"Specify {code}");
			}

			ValidateBrackets(code);

			_code = code;
		}

		private static void ValidateBrackets(string code)
		{
			var openingBracketsPositions = new Stack<int>();

			for (var i = 0; i < code.Length; i++)
			{
				var codeChar = code[i];
				switch (codeChar)
				{
					case '[':
						openingBracketsPositions.Push(i);
						break;
					case ']':
						if (!openingBracketsPositions.Any())
						{
							throw new BrainfuckParsingException(i + 1, "Unexpected ']'");
						}

						openingBracketsPositions.Pop();
						break;
				}
			}

			if (!openingBracketsPositions.Any())
			{
				return;
			}

			// oops, found unclosed '['s,
			// reverse the stack so that indices could be read left to right
			var mismatchingBracketsPositions = openingBracketsPositions.Reverse().Select(p => p + 1);
			throw new BrainfuckParsingException(
				$"Opening square brackets on positions {string.Join(", ", mismatchingBracketsPositions)} are unclosed");
		}

		public IEnumerable<Instruction> GetInstructions()
		{
			var cumulativeValue = 0;

			for (var i = 0; i < _code.Length; i++)
			{
				var codeChar = _code[i];
				var isLast = i == _code.Length - 1;

				var instructionType = GetInstructionType(codeChar);

				if (instructionType is InstructionType.Nop)
				{
					continue;
				}

				var isCumulative = CumulativeInstructionsIncrements.TryGetValue(codeChar, out var valueIncrement);

				if (isCumulative)
				{
					cumulativeValue += valueIncrement;
				}
				else
				{
					yield return new Instruction(instructionType);
					continue;
				}

				if (!isLast && GetInstructionType(_code[i + 1]) == instructionType)
				{
					continue;
				}

				// current cumulative instruction is finished, but the 0 result is useless
				if (cumulativeValue == 0)
				{
					continue;
				}

				yield return new Instruction(instructionType, cumulativeValue);
				cumulativeValue = 0;
			}
		}

		private static InstructionType GetInstructionType(char codeChar)
		{
			switch (codeChar)
			{
				case '+':
				case '-':
					return InstructionType.Increment;
				case '>':
				case '<':
					return InstructionType.MoveHead;
				case '[':
					return InstructionType.StartLoop;
				case ']':
					return InstructionType.FinishLoop;
				case '.':
					return InstructionType.Output;
				case ',':
					return InstructionType.Input;
				default:
					return InstructionType.Nop;
			}
		}
	}
}