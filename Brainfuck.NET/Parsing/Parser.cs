using System;
using System.Collections.Generic;

namespace Brainfuck.NET.Parsing
{
	public class Parser
	{
		private readonly string _code;

		private static readonly Dictionary<char, int> CumulativeOperationIncrements = new()
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

			_code = code;
		}

		public IEnumerable<Operation> GetOperations()
		{
			var cumulativeValue = 0;

			for (var i = 0; i < _code.Length; i++)
			{
				var codeChar = _code[i];
				var isLast = i == _code.Length - 1;

				var operationType = GetOperationType(codeChar);

				if (operationType is OperationType.Nop)
				{
					continue;
				}

				var isCumulative = operationType is OperationType.Increment or OperationType.MoveHead;
				if (!isCumulative)
				{
					yield return new Operation(operationType);
				}
				else
				{
					cumulativeValue += CumulativeOperationIncrements[codeChar];
				}

				if (!isLast && GetOperationType(_code[i + 1]) == operationType)
				{
					continue;
				}

				// cumulative operation with result of 0 is useless
				if (cumulativeValue == 0)
				{
					continue;
				}

				yield return new Operation(operationType, cumulativeValue);
				cumulativeValue = 0;
			}
		}

		private static OperationType GetOperationType(char codeChar)
		{
			switch (codeChar)
			{
				case '+':
				case '-':
					return OperationType.Increment;
				case '>':
				case '<':
					return OperationType.MoveHead;
				case '[':
					return OperationType.StartLoop;
				case ']':
					return OperationType.FinishLoop;
				case '.':
					return OperationType.Output;
				case ',':
					return OperationType.Input;
				default:
					return OperationType.Nop;
			}
		}
	}
}