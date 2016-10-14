namespace compilers_1_template_engine {
	public enum TokenType {
		/// <summary>
		/// Representing ordinary chars, without embedded code
		/// </summary>
		Text,
		/// <summary>
		/// String enclosed in '#@' tags
		/// </summary>
		DirectiveBlock,
		/// <summary>
		/// String enclosed in '#' tags - control block can contain statements
		/// </summary>
		StandardCB,
		/// <summary>
		/// String enclosed in '#=' tags - control block can contain expressions
		/// </summary>
		ExpressionCB,
		/// <summary>
		/// String enclosed in '#+' tags - control block can contain methods, fields and properties
		/// </summary>
		ClassFeatureCB,
	}
}
