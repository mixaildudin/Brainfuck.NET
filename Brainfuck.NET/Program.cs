using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace Brainfuck.NET
{
	internal class Program
	{
		private static ILGenerator _il;

		private static LocalBuilder _tapeVar;

		private static LocalBuilder _headVar;

		private static readonly MethodInfo ConsoleWrite = typeof(Console).GetMethod(nameof(Console.Write),
			new[] { typeof(char) }) ?? throw new InvalidOperationException();

		private static readonly MethodInfo ConsoleRead = typeof(Console).GetMethod(nameof(Console.Read),
			Array.Empty<Type>()) ?? throw new InvalidOperationException();

		public static void Main(string[] args)
		{
			const string typeName = "Program";
			const string assemblyName = "test.exe";

			var name = new AssemblyName(assemblyName);
			var builder = Thread.GetDomain().DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndSave);
			var module = builder.DefineDynamicModule(assemblyName, assemblyName);
			var typeBuilder = module.DefineType(typeName, TypeAttributes.Public | TypeAttributes.Class);
			var mainMethodBuilder = typeBuilder.DefineMethod("PointMain", MethodAttributes.Public | MethodAttributes.Static,
				typeof(void), null);

			_il = mainMethodBuilder.GetILGenerator();

			_tapeVar = _il.DeclareLocal(typeof(byte[]));
			_headVar = _il.DeclareLocal(typeof(int));

			_il.Emit(OpCodes.Ldc_I4, 30_000);
			_il.Emit(OpCodes.Newarr, typeof(byte[]));
			_il.Emit(OpCodes.Stloc, _tapeVar);

			var code = args[0];
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

		private static void EmitCurrentCellIncrement(int incrementBy)
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

		private static void EmitMoveHead(int incrementBy)
		{
			// head += incrementBy;
			_il.Emit(OpCodes.Ldloc, _headVar);
			_il.Emit(OpCodes.Ldc_I4, incrementBy);
			_il.Emit(OpCodes.Add);
			_il.Emit(OpCodes.Stloc, _headVar);
		}

		private static void EmitOutput()
		{
			// Console.WriteLine(tape[head]);
			_il.Emit(OpCodes.Ldloc, _tapeVar);
			_il.Emit(OpCodes.Ldloc, _headVar);
			_il.Emit(OpCodes.Ldelem_U1);

			_il.Emit(OpCodes.Call, ConsoleWrite);
		}

		private static void EmitInput()
		{
			// tape[head] = (byte)Console.Read();
			_il.Emit(OpCodes.Ldloc, _tapeVar);
			_il.Emit(OpCodes.Ldloc, _headVar);
			_il.Emit(OpCodes.Call, ConsoleRead);
			_il.Emit(OpCodes.Conv_U1);
			_il.Emit(OpCodes.Stelem_I);
		}
	}
}