/*
 * Title: Template engine (T4 prototype)
 * Author: Nail Gibaev
 * Email: abel.ze.normand@gmail.com
 */
using System;
using NDesk.Options;
using System.Collections.Generic;
using System.IO;

namespace compilers_1_template_engine {
	class MainClass {
		/// <summary>
		/// The entry point of the program, where the program control starts and ends.
		/// </summary>
		/// <param name="args">The command-line arguments.</param>
		public static void Main(string[] args) {
			string output_file = "out.cs";
			bool keep_preprocessed_file = false;
			string target_namespace = "default_ns";
			string input_filename = null;

			var opts = new OptionSet() {
				{ "out:", "Output file path", x => output_file = x },
				{ "d", "Keep preprocessed file", x => keep_preprocessed_file = (x != null) },
				{ "ns:", "Target namespace", x => target_namespace = x },
				{ "f=", "Input file", x => input_filename = x }
			};

			List<string> extra = null;
			try {
				extra = opts.Parse(args);
			}
			catch (OptionException e) {
				var err_io = Console.Error;
				err_io.WriteLine("Error in parsing arguments: {0}", e.Message);
				return;
			}

			//input_filename = "input_file.tt";

			// ####### PARSE TOKENS ########
			var tokens = new Parser(File.ReadAllText(input_filename)).Parse();
			// ##### END PARSE TOKENS ######

			// ###### PREPROCESS FILE ######
			var preprocessed_text = new Preprocessor(tokens, target_namespace).Preprocess();
			if (keep_preprocessed_file) {
				File.WriteAllText(output_file, preprocessed_text);
				Console.WriteLine("Code generation done. Exit.");
				return;
			}
			// ## END OF PREPROCESS FILE ###

			// # COMPILE PREPROCESSED FILE #
			var compiler = new Compiler(preprocessed_text, target_namespace);
			compiler.Compile();
			if (compiler.HasErrors) {
				Console.WriteLine("Errors occured during compilation: {0}", compiler.Errors);
				return;
			}
			// #### END OF COMPILATION #####

			// ########## TEH END ##########
			File.WriteAllText(output_file, compiler.Result);
			Console.WriteLine("Compilation done");
			// ########## TEH END ##########
		}
	}
}
