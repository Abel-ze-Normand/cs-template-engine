using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.CodeDom.Compiler;
using System.CodeDom;

namespace compilers_1_template_engine {
	/// <summary>
	/// Processing tokens and writing Preprocessed T4 template 
	/// </summary>
	public class Preprocessor {
		List<Token> list;
		public string template_language;
		public bool? template_hostspecific;
		public bool? template_debug;
		public bool? template_linePragmas;
		public string output_encoding;
		public string assembly;
		public string namespace_name;
		public List<string> imports = new List<string>();
		public TextWriter err_log = Console.Error;
		int pos = 0;
		List<Token> internal_tokens;
		Regex re = new Regex(@"(?<word>\w[\w\d_]*)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

		/// <summary>
		/// Initializes a new instance of the <see cref="T:compilers_1_template_engine.Preprocessor"/> class.
		/// </summary>
		/// <param name="input_list">List of tokens</param>
		public Preprocessor(List<Token> input_list, string namespace_name) {
			list = input_list; this.namespace_name = namespace_name;
		}

		/// <summary>
		/// Preprocess given tokens and write them in result string
		/// </summary>
		public string Preprocess() {
			PreprocessDirectives(list);
			return GenerateCode(list.Where(x => x.type != TokenType.DirectiveBlock));
		}

		void PreprocessDirectives(IEnumerable<Token> l) {
			List<Token> directives = l.Where(x => x.type == TokenType.DirectiveBlock).ToList();
			foreach (var d in directives) {
				ChooseDirective(d.str);
			}
		}

		void ChooseDirective(string directive) {
			var match = re.Match(directive);
			if (!match.Success) {
				throw new Exception("Directive options parse error");
			}
			var first_word = match.Groups["word"].Value;
			var directive_opts = directive.Replace(first_word, "").Trim();
			switch (first_word) {
				case "template":
					DirectiveProcessor.ProcessTemplateDirective(directive_opts, this);
					break;
				case "output":
					DirectiveProcessor.ProcessOutputDirective(directive_opts, this);
					break;
				case "assembly":
					DirectiveProcessor.ProcessAssemblyDirective(directive_opts, this);
					break;
				case "import":
					DirectiveProcessor.ProcessImportDirective(directive_opts, this);
					break;
				default: throw new Exception("Unknown directive");
			}
		}

		string ParseClassFeature() {
			string res = "";
			res += CurrentToken + LineTerminator + Indent(CurrentIndent);
			Next();
			//IndentNextLevel();
			while (!CurrentToken.IsClassFeatureEnd) {
				if (CurrentToken.Skippable) {
					res += string.Format("this.Write({0});{1}",
										 ToLiteral(CurrentToken.str), LineTerminator + Indent(CurrentIndent));
					Next();
					continue;
				}
				switch (CurrentToken.type) {
					case TokenType.StandardCB:
						res += CurrentToken + LineTerminator + Indent(CurrentIndent);
						break;
					case TokenType.ClassFeatureCB:
						throw new Exception("Can't declare function inside function");
					case TokenType.ExpressionCB:
						res +=
							string.Format("this.Write(this.ToStringHelper.ToStringWithCulture({0}));{1}",
										  CurrentToken, LineTerminator + Indent(CurrentIndent));
						break;
					default:
						throw new Exception("Unknown block type");
				}
				Next();
			}
			res.TrimEnd();
			res += LineTerminator + Indent(UnindentLevel())
				+ "}";
			return res;
		}

		string GenerateCode(IEnumerable<Token> l) {
			pos = 0;
			internal_tokens = l.ToList();
			string res = "";
			string delayed_write = "";
			string transform_text = "";

			// start indent from 2 level
			IndentNextLevel();
			IndentNextLevel();
			IndentNextLevel();

			// iterate all tokens, prepare list of output
			while (IsAvailableTokens) {
				if (CurrentToken.Skippable) {
					transform_text += string.Format("this.Write({0});{1}",
													ToLiteral(CurrentToken.str), LineTerminator + Indent(CurrentIndent));
					Next();
					continue;
				}
				switch (CurrentToken.type) {
					case TokenType.StandardCB:
						transform_text += CurrentToken + LineTerminator + Indent(CurrentIndent);
						break;
					case TokenType.ClassFeatureCB:
						delayed_write += ParseClassFeature();
						break;
					case TokenType.ExpressionCB:
						transform_text +=
							string.Format("this.Write(this.ToStringHelper.ToStringWithCulture({0}));{1}",
										  CurrentToken, LineTerminator + Indent(CurrentIndent));
						break;
					default:
						throw new Exception("Unknown block type");
				}
				Next();
			}
			transform_text.TrimEnd();
			delayed_write.TrimEnd();

			CurrentIndent = 0;
			res = string.Format(HardcodedResources.HeadPart, GenerateUsings(imports), namespace_name)
						+ " {"
						+ LineTerminator + Indent(IndentNextLevel())
						+ "public partial class PreprocessedT4Template_1 : PreprocessedT4Template_1Base {"
						+ LineTerminator + Indent(IndentNextLevel())
						+ "public virtual string TransformText() {"
						+ LineTerminator + Indent(IndentNextLevel())
						+ "this.GenerationEnvironment = null;"
						+ transform_text
						+ LineTerminator + Indent(CurrentIndent)
						+ "return this.GenerationEnvironment.ToString();"
						+ LineTerminator + Indent(UnindentLevel())
						+ "}" // close TransformText
						+ LineTerminator
						+ LineTerminator + Indent(CurrentIndent)
						+ delayed_write
						+ LineTerminator + Indent(CurrentIndent)
						+ "public virtual void Initialize() {}"
						+ LineTerminator + Indent(UnindentLevel())
						+ "}" // close PreprocessedT4Template_1 class
						+ LineTerminator
						+ LineTerminator + Indent(CurrentIndent)
						+ HardcodedResources.TailPart;
			return res;
		}

		bool IsAvailableTokens {
			get {
				return pos < internal_tokens.Count();
			}
		}

		void Next() {
			pos++;
		}

		Token CurrentToken {
			get {
				return internal_tokens[pos];
			}
		}

		int CurrentIndent { get; set; }

		string LineTerminator {
			get {
				if (Environment.OSVersion.Platform == PlatformID.Win32Windows) {
					return "\r\n";
				}
				return "\n";
			}
		}

		string Indent(int amount) {
			string res = "";
			for (int i = 0; i < amount; i++) {
				res += "    ";
			}
			return res;
		}

		int IndentNextLevel() {
			CurrentIndent++;
			return CurrentIndent;
		}

		int UnindentLevel() {
			CurrentIndent--;
			return CurrentIndent;
		}

		string GenerateUsings(IEnumerable<string> usings_list) {
			return string.Join(LineTerminator, usings_list.Select(x => "using " + x.Trim() + ";"));
		}

		string ToLiteral(string input) {
			using (var writer = new StringWriter()) {
				using (var provider = CodeDomProvider.CreateProvider("CSharp")) {
					provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, null);
					return writer.ToString();
				}
			}
		}
	}
}
