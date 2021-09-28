namespace Brainfuck.NET.Parsing
{
	public record Operation
	{
		public Operation(OperationType type)
		{
			Type = type;
		}

		public Operation(OperationType type, int cumulativeValue)
			: this(type)
		{
			CumulativeValue = cumulativeValue;
		}

		public OperationType Type { get; }

		public int CumulativeValue { get; }
	}
}