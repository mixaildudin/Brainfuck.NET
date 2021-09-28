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