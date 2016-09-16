/*
 * Title: Template engine (T4 prototype)
 * Author: Nail Gibaev
 * Email: abel.ze.normand@gmail.com
 */
using System;
using NDesk.Options;
using System.Collections;
using System.Collections.Generic;

namespace compilers_1_template_engine {
	class MainClass {
		/// <summary>
		/// The entry point of the program, where the program control starts and ends.
		/// </summary>
		/// <param name="args">The command-line arguments.</param>
		public static void Main(string[] args) {
			string output_file = null;
			bool keep_preprocessed_file = false;
			string target_namespace = "default_ns";

			var opts = new OptionSet() {
				{"out=", "Output file path", x => output_file = x},
				{"d", "Keep preprocessed file", (bool x) => keep_preprocessed_file = x},
				{"ns=", "Target namespace", x => target_namespace = x}
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

			var preprocessed_text =
		}
	}
}
