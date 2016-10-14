using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
namespace compilers_1_template_engine {
	public class DirectiveProcessor {
		static Regex option_regex = new Regex(@"(?<option_name>[\w\d_]+)=""(?<option_value>[^""]*)""",
									   RegexOptions.CultureInvariant |
									   RegexOptions.ExplicitCapture |
									   RegexOptions.Compiled);

		static readonly TextWriter err_log = Console.Error;

		static Dictionary<string, string> GetDirectiveOpts(string body) {
			Dictionary<string, string> result = new Dictionary<string, string>();
			while (body.Length > 0) {
				var match = option_regex.Match(body);
				if (!match.Success) {
					throw new Exception("Error in directive options parse");
				}
				if (match.Index != 0) {
					throw new Exception("Error in directive options parse");
				}
				var opt_name = match.Groups["option_name"].Value;
				var opt_val = match.Groups["option_value"].Value;
				result.Add(opt_name, opt_val);
				body = body.Replace(string.Format(@"{0}=""{1}""", opt_name, opt_val), "").Trim();
			}
			return result;
		}

		static public void ProcessTemplateDirective(string body, Preprocessor pp) {
			var opts = GetDirectiveOpts(body);
			if (opts.ContainsKey("language")) {
				ProcessTemplateLanguageOption(opts["language"], pp);
				opts.Remove("language");
			}
			if (opts.ContainsKey("hostspecific")) {
				ProcessTemplateHostSpecificOption(opts["hostspecific"], pp);
				opts.Remove("hostspecific");
			}
			if (opts.ContainsKey("debug")) {
				ProcessTemplateDebugOption(opts["debug"], pp);
				opts.Remove("debug");
			}
			if (opts.ContainsKey("linePragmas")) {
				ProcessTemplateLinePragmasOption(opts["linePragmas"], pp);
				opts.Remove("linePragmas");
			}
			if (opts.Count > 0) {
				ProcessOtherOptions(opts.Keys, "template");
			}
		}

		static void ProcessTemplateLanguageOption(string val, Preprocessor pp) {
			if (val != "C#") {
				ErrBlame("Unsupported 'language' option value. Fallback to C#");
				val = "C#";
			}
			if (pp.template_language != null) {
				ErrBlame("Language option already set");
			}
			pp.template_language = val;
		}

		static void ProcessTemplateHostSpecificOption(string val, Preprocessor pp) {
			if (val != false.ToString()) {
				ErrBlame("Unsupported 'hostspecific' option value. Fallback to false");
				val = "false";
			}
			if (pp.template_hostspecific != null) {
				ErrBlame("Hostspecific option already set");
			}
			pp.template_hostspecific = bool.Parse(val);
		}

		static void ProcessTemplateDebugOption(string val, Preprocessor pp) {
			if (val != false.ToString()) {
				ErrBlame("Unsupported 'debug' option value. Fallback to false");
				val = "false";
			}
			if (pp.template_hostspecific != null) {
				ErrBlame("Debug option already set");
			}
			pp.template_debug = bool.Parse(val);
		}

		static void ProcessTemplateLinePragmasOption(string val, Preprocessor pp) {
			if (val != false.ToString()) {
				ErrBlame("Unsupported 'linePragmas' option value. Fallback to false");
				val = "false";
			}
			if (pp.template_hostspecific != null) {
				ErrBlame("LinePragmas option already set");
			}
			pp.template_linePragmas = bool.Parse(val);
		}

		static void ProcessOtherOptions(IEnumerable<string> opts, string dir_name) {
			ErrBlame(string.Format("{1}: unhandled attributes: ({0})", string.Join(", ", opts), dir_name));
		}

		static public void ProcessOutputDirective(string body, Preprocessor pp) {
			var opts = GetDirectiveOpts(body);
			if (opts.ContainsKey("encoding")) {
				ProcessOutputEncodingOption(opts["encoding"], pp);
				opts.Remove("encoding");
			}
			if (opts.Count > 0) {
				ProcessOtherOptions(opts.Keys, "output");
			}
		}

		static void ProcessOutputEncodingOption(string val, Preprocessor pp) {
			ErrBlame("Encoding option unsupported in 'output' directive. Fallback to default null");
			val = null;
			pp.output_encoding = val;
		}

		static public void ProcessAssemblyDirective(string body, Preprocessor pp) {
			var opts = GetDirectiveOpts(body);
			if (opts.ContainsKey("name")) {
				ProcessAssemblyNameOption(opts["name"], pp);
				opts.Remove("name");
			}
			if (opts.Count > 0) {
				ProcessOtherOptions(opts.Keys, "assembly");
			}
		}

		static void ProcessAssemblyNameOption(string val, Preprocessor pp) {
			if (val != "assembly strong name") {
				ErrBlame("Unsupported 'name' option value. Fallback to assembly strong name");
				val = "assembly strong name";
			}
			pp.assembly = val;
		}

		static public void ProcessImportDirective(string body, Preprocessor pp) {
			var opts = GetDirectiveOpts(body);
			if (opts.ContainsKey("namespace")) {
				ProcessImportNamespaceOption(opts["namespace"], pp);
				opts.Remove("namespace");
			}
			if (opts.Count > 0) {
				ProcessOtherOptions(opts.Keys, "import");
			}
		}

		static void ProcessImportNamespaceOption(string val, Preprocessor pp) {
			pp.imports.Add(val);
		}

		static public void ErrBlame(string body) {
			err_log.WriteLine("|| WARNING || : {0}", body);
		}
	}
}
