using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace Brainfuck.NET
{
	public class Compiler
	{
		private ILGenerator _il;

		private LocalBuilder _tapeVar;

		private LocalBuilder _headVar;

		private readonly Stack<Loop> _loops = new();

		private readonly MethodInfo _consoleWrite = typeof(Console).GetMethod(nameof(Console.Write),
			new[] { typeof(char) }) ?? throw new InvalidOperationException();

		private readonly MethodInfo _consoleRead = typeof(Console).GetMethod(nameof(Console.Read),
			Array.Empty<Type>()) ?? throw new InvalidOperationException();

		public void Compile(string code, string assemblyName)
		{
			const string typeName = "Program";

			var name = new AssemblyName(assemblyName);
			var builder = Thread.GetDomain().DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndSave);
			var module = builder.DefineDynamicModule(assemblyName, assemblyName);
			var typeBuilder = module.DefineType(typeName, TypeAttributes.Public | TypeAttributes.Class);
			var mainMethodBuilder = typeBuilder.DefineMethod("Main",
				MethodAttributes.Public | MethodAttributes.Static,
				typeof(void), null);

			_il = mainMethodBuilder.GetILGenerator();

			_tapeVar = _il.DeclareLocal(typeof(byte[]));
			_headVar = _il.DeclareLocal(typeof(int));

			_il.Emit(OpCodes.Ldc_I4, 30_000);
			_il.Emit(OpCodes.Newarr, typeof(byte));
			_il.Emit(OpCodes.Stloc, _tapeVar);

			foreach (var codeChar in code)
			{
				switch (codeChar)
				{
					case '+':
						EmitCurrentCellIncrement(1);
						break;
					case '-':
						EmitCurrentCellIncrement(-1);
						break;
					case '>':
						EmitMoveHead(1);
						break;
					case '<':
						EmitMoveHead(-1);
						break;
					case '[':
						EmitLoopStart();
						break;
					case ']':
						EmitLoopFinish();
						break;
					case '.':
						EmitOutput();
						break;
					case ',':
						EmitInput();
						break;
				}
			}

			_il.Emit(OpCodes.Ret);

			typeBuilder.CreateType();

			builder.SetEntryPoint(mainMethodBuilder);
			builder.Save(assemblyName);
		}

		private void EmitCurrentCellIncrement(int incrementBy)
		{
			// tape[head] += incrementBy;
			_il.Emit(OpCodes.Ldloc, _tapeVar);
			_il.Emit(OpCodes.Ldloc, _headVar);
			_il.Emit(OpCodes.Ldelema, typeof(byte));
			_il.Emit(OpCodes.Dup);
			_il.Emit(OpCodes.Ldind_U1);
			_il.Emit(OpCodes.Ldc_I4, incrementBy);
			_il.Emit(OpCodes.Add);
			_il.Emit(OpCodes.Conv_U1);
			_il.Emit(OpCodes.Stind_I1);
		}

		private void EmitMoveHead(int incrementBy)
		{
			// head += incrementBy;
			_il.Emit(OpCodes.Ldloc, _headVar);
			_il.Emit(OpCodes.Ldc_I4, incrementBy);
			_il.Emit(OpCodes.Add);
			_il.Emit(OpCodes.Stloc, _headVar);
		}

		private void EmitOutput()
		{
			// Console.WriteLine(tape[head]);
			_il.Emit(OpCodes.Ldloc, _tapeVar);
			_il.Emit(OpCodes.Ldloc, _headVar);
			_il.Emit(OpCodes.Ldelem_U1);

			_il.Emit(OpCodes.Call, _consoleWrite);
		}

		private void EmitInput()
		{
			// tape[head] = (byte)Console.Read();
			_il.Emit(OpCodes.Ldloc, _tapeVar);
			_il.Emit(OpCodes.Ldloc, _headVar);
			_il.Emit(OpCodes.Call, _consoleRead);
			_il.Emit(OpCodes.Conv_U1);
			_il.Emit(OpCodes.Stelem_I1);
		}

		private void EmitLoopStart()
		{
			var loopBody = _il.DefineLabel();
			var loopCondition = _il.DefineLabel();

			_loops.Push(new Loop(loopCondition, loopBody));

			_il.Emit(OpCodes.Br, loopCondition);

			_il.MarkLabel(loopBody);
			_il.Emit(OpCodes.Nop); // just to mark it as loop body

			// loop body goes next...
		}

		private void EmitLoopFinish()
		{
			var loop = _loops.Pop();

			_il.MarkLabel(loop.Condition);

			_il.Emit(OpCodes.Ldloc, _tapeVar);
			_il.Emit(OpCodes.Ldloc, _headVar);
			_il.Emit(OpCodes.Ldelem_U1);
			_il.Emit(OpCodes.Ldc_I4_0);
			_il.Emit(OpCodes.Cgt_Un);
			_il.Emit(OpCodes.Brtrue, loop.Body);
		}

		private class Loop
		{
			public Label Condition { get; }

			public Label Body { get; }

			public Loop(Label condition, Label body)
			{
				Condition = condition;
				Body = body;
			}
		}
	}
}