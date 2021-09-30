using System.Linq;
using Brainfuck.NET.Parsing;
using NUnit.Framework;

namespace Brainfuck.NET.Tests
{
	[TestFixture]
	public class ParserSpec
	{
		[TestCaseSource(nameof(_parserCases))]
		public void Parser_ShouldGiveOptimalInstructionList(string code, Instruction[] expectedInstructions)
		{
			var parser = new Parser(code);
			var instructions = parser.GetInstructions().ToArray();

			Assert.That(instructions, Is.EqualTo(expectedInstructions));
		}

		[TestCase("+++", null, null)]
		[TestCase("[+++]", null, null)]
		[TestCase("[+++]]", 6, null)]
		[TestCase("[[+++]", null, new[] { 1 })]
		[TestCase("[+[+[+]", null, new[] { 1, 3 })]
		public void Parser_ShouldCorrectlyAnalyzeBracketsNesting(string code, int? unexpectedClosing,
			int[] unclosedOpening)
		{
			if (unexpectedClosing == null && unclosedOpening == null)
			{
				Assert.DoesNotThrow(Parse);
				return;
			}

			var ex = Assert.Throws<BrainfuckParsingException>(Parse);

			if (unexpectedClosing.HasValue)
			{
				Assert.That(ex.Message, Does.Contain($"Position {unexpectedClosing}: Unexpected ']'"));
				return;
			}

			if (unclosedOpening != null)
			{
				Assert.That(ex.Message, Does.Contain(string.Join(", ", unclosedOpening) + " are unclosed"));
			}

			// ReSharper disable once ObjectCreationAsStatement
			void Parse() => new Parser(code);
		}

		private static object[] _parserCases =
		{
			new object[] { "+++", new[] { new Instruction(InstructionType.Increment, 3) } },
			new object[] { "+++.", new[] { new Instruction(InstructionType.Increment, 3), new Instruction(InstructionType.Output) } },
			new object[]
			{
				",+++.", new[]
				{
					new Instruction(InstructionType.Input),
					new Instruction(InstructionType.Increment, 3),
					new Instruction(InstructionType.Output)
				}
			},
			new object[]
			{
				",+-.", new[]
				{
					new Instruction(InstructionType.Input),
					new Instruction(InstructionType.Output)
				}
			},
			new object[]
			{
				",+>>.", new[]
				{
					new Instruction(InstructionType.Input),
					new Instruction(InstructionType.Increment, 1),
					new Instruction(InstructionType.MoveHead, 2),
					new Instruction(InstructionType.Output)
				}
			}
		};
	}
}