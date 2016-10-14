using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
namespace compilers_1_template_engine {
	/// <summary>
	/// Parses given text for directives, template tags and break text in tokens
	/// </summary>
	public class Parser {
		int pos;
		string s;
		Regex re;

		#region REGEXES
		string directive_pattern = "<#@(?<directive_body>.+?)#>\n?";
		string class_feature_cb_pattern = @"<#\+(?<class_feat_cb_body>.+?)#>\n?";
		string expr_cb_pattern = "<#=(?<expr_cb_body>.+?)#>\n?";
		string stand_cb_pattern = "<#(?<stand_cb_body>.+?)#>\n?";
		string unparsed_symbols = @"(?<unparsed>[^\<]+)\n?";
		#endregion REGEXES

		/// <summary>
		/// Initializes a new instance of the <see cref="T:compilers_1_template_engine.Parser"/> class.
		/// </summary>
		/// <param name="text">Input text</param>
		public Parser(string text) {
			s = text;
			re = new Regex(string.Format("{0}|{1}|{2}|{3}|{4}",
										 directive_pattern,
										 class_feature_cb_pattern,
										 expr_cb_pattern,
										 stand_cb_pattern,
										 unparsed_symbols),
						   RegexOptions.Compiled |
						   RegexOptions.CultureInvariant |
						   RegexOptions.ExplicitCapture);
		}

		/// <summary>
		/// Break given text on tokens
		/// </summary>
		public List<Token> Parse() {
			var res = new List<Token>();
			while (pos < s.Length) {
				var substr = s.Substring(pos);
				var m = re.Match(substr);
				if (!m.Success) {
					throw new Exception("Parsing error - unknown token type");
				}
				if (m.Index != 0) {
					throw new Exception("Parsing error - matched substring not at position 0");
				}
				pos += m.Length;
				if (m.Groups["directive_body"].Success) {
					res.Add(new Token(m.Groups["directive_body"].Value.Trim(), TokenType.DirectiveBlock));
				}
				else if (m.Groups["class_feat_cb_body"].Success) {
					res.Add(new Token(m.Groups["class_feat_cb_body"].Value.Trim(), TokenType.ClassFeatureCB));
				}
				else if (m.Groups["expr_cb_body"].Success) {
					res.Add(new Token(m.Groups["expr_cb_body"].Value.Trim(), TokenType.ExpressionCB));
				}
				else if (m.Groups["stand_cb_body"].Success) {
					res.Add(new Token(m.Groups["stand_cb_body"].Value.Trim(), TokenType.StandardCB));
				}
				else if (m.Groups["unparsed"].Success) {
					res.Add(new Token(m.Groups["unparsed"].Value, TokenType.Text));
				}
				else {
					throw new Exception("PARSING FATAL ERROR");
				}
			}
			return res;
		}
	}
}
