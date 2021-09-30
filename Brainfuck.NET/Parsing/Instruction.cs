namespace Brainfuck.NET.Parsing
{
	public record Instruction
	{
		public Instruction(InstructionType type)
		{
			Type = type;
		}

		public Instruction(InstructionType type, int cumulativeValue)
			: this(type)
		{
			CumulativeValue = cumulativeValue;
		}

		public InstructionType Type { get; }

		public int CumulativeValue { get; }
	}
}