using System.Linq;
using Brainfuck.NET.Parsing;
using NUnit.Framework;

namespace Brainfuck.NET.Tests
{
	[TestFixture]
	public class ParserSpec
	{
		[TestCaseSource(nameof(_parserCases))]
		public void Parser_ShouldGiveOptimalOperationList(string code, Operation[] expectedOperations)
		{
			var parser = new Parser(code);
			var operations = parser.GetOperations().ToArray();

			Assert.That(operations, Is.EqualTo(expectedOperations));
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
			new object[] { "+++", new[] { new Operation(OperationType.Increment, 3) } },
			new object[] { "+++.", new[] { new Operation(OperationType.Increment, 3), new Operation(OperationType.Output) } },
			new object[]
			{
				",+++.", new[]
				{
					new Operation(OperationType.Input),
					new Operation(OperationType.Increment, 3),
					new Operation(OperationType.Output)
				}
			},
			new object[]
			{
				",+-.", new[]
				{
					new Operation(OperationType.Input),
					new Operation(OperationType.Output)
				}
			},
			new object[]
			{
				",+>>.", new[]
				{
					new Operation(OperationType.Input),
					new Operation(OperationType.Increment, 1),
					new Operation(OperationType.MoveHead, 2),
					new Operation(OperationType.Output)
				}
			}
		};
	}
}