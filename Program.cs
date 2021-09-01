using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Options;

namespace cat_compare {
	class Program {
		static int Verbosity = 0;
		static string? Directory;
		static bool Skip = false;

		static void Die (string message)
		{
			Console.Error.WriteLine (message);
			Environment.Exit (-1);
		}

		static void Message (string message, int verbosity = 1)
		{
			if (Verbosity >= verbosity) {
				Console.WriteLine (message);
			}
		}

		static string []? ReadFileIfExists (string file, string friendlyName)
		{
			if (File.Exists (file)) {
				return File.ReadAllLines (file);
			}
			return null;
		}

		static string [] ReadFile (string file, string friendlyName)
		{
			if (!File.Exists (file)) {
				Die ($"Can not find {friendlyName} at: {file}");
			}
			return File.ReadAllLines (file);
		}

		static HashSet<string> ProcessXtroFile (string [] ? contents) 
		{
			var set = new HashSet<string>();
			if (contents != null) {
				set.UnionWith(contents.Where(l => l.StartsWith("!")));
			}
			return set;
		}

		static bool CheckLine (string line, HashSet<string> list, ref int counter)
		{
			if (list.Contains(line)) {
				counter += 1;
				return true;
			}
			return false;
		}

		static int Process (string name)
		{
			string [] todoLines = ReadFile ($"{Directory!}/MacCatalyst-{name}.todo", "todo");
		   
			var iOSTodo = ProcessXtroFile (ReadFileIfExists($"{Directory!}/iOS-{name}.todo", "iOS todo"));
			var iOSIgnore = ProcessXtroFile (ReadFileIfExists($"{Directory!}/iOS-{name}.ignore", "iOS ignore"));
			var macTodo = ProcessXtroFile (ReadFileIfExists($"{Directory!}/macOS-{name}.todo", "macOS todo"));

			int iOSTodoSkipCounter = 0;
			int iOSIgnoreSkipCounter = 0;
			int macTodoSkipCounter = 0;
			int keptCounter = 0;

			foreach (var line in todoLines) {
				if (line.StartsWith("!")) {
					if (CheckLine (line, iOSTodo, ref iOSTodoSkipCounter) ||
						CheckLine (line, iOSIgnore, ref iOSIgnoreSkipCounter) ||
						CheckLine (line, macTodo, ref macTodoSkipCounter)) {
						Message($"\tSkipping '{line}'", 2);
						continue;
					}
				}
				keptCounter +=1;
				if (!Skip) {
					Console.WriteLine(line);
				}
			}
			if (!Skip && Verbosity > 0) {
				Console.WriteLine();
			}
			Message($"Orignal Lines: {todoLines.Length}");
			Message($"Kept Lines: {keptCounter}");
			Message($"Removed Lines: {todoLines.Length - keptCounter}");
			Message($"Skipped due to iOS todo: {iOSTodoSkipCounter}");
			Message($"Skipped due to iOS ignore: {iOSIgnoreSkipCounter}");
			Message($"Skipped due to macOS todo: {macTodoSkipCounter}");
			return keptCounter == 0 ? 0 : 1;
		}

		static int Main (string [] args)
		{
			var options = new OptionSet {
				{ "v", "increase debug message verbosity", v => { if (v != null) ++Verbosity; } },
				{ "d=", "directory with todo files", d => Directory = d},
				{ "s", "skip ouput of non-ignored lines", s => Skip = true}
			};
			List<string> extra = options.Parse (args);
			if (extra.Count != 1 || Directory == null) {
				Console.Error.WriteLine ("cat_compare [API]");
				Console.Error.WriteLine ("e.g.: cat_compare AVFoundation");
				Environment.Exit(-1);
			}
			return Process (extra [0]);
		}
	}
}
