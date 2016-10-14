namespace compilers_1_template_engine {
	public class Token {
		public string str;
		public TokenType type;

		public Token(string s, TokenType t) {
			str = s; type = t;
		}

		public bool Skippable {
			get {
				return type == TokenType.Text;
			}
		}

		public bool IsClassFeatureEnd {
			get {
				return type == TokenType.ClassFeatureCB && str == "}";
			}
		}

		public override string ToString() {
			return str;
		}
	}
}
