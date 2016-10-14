using System;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Reflection;
using System.Collections.Generic;
namespace compilers_1_template_engine {
	public class Compiler {
		string s;
		string errors;
		string nm;
		string result;

		public bool HasErrors {
			get {
				return errors != null;
			}
		}
		public string Errors {
			get {
				return errors;
			}
		}
		public string Result {
			get {
				return result;
			}
		}

		public Compiler(string contents, string target_namespace) {
			s = contents; nm = target_namespace;
		}

		public void Compile() {
			CompilerParameters parameters = new CompilerParameters();
			parameters.GenerateInMemory = true;
			parameters.ReferencedAssemblies.Add("System.Core.dll");
			parameters.ReferencedAssemblies.Add("System.dll");
			CompilerResults results = new CSharpCodeProvider().CompileAssemblyFromSource(parameters, s);
			if (results.Errors.HasErrors) {
				List<string> errs = new List<string>();
				foreach (var error in results.Errors) {
					errs.Add(error.ToString());
				}
				errors = string.Join("; ", errs);
				return;
			}
			Assembly assembly = results.CompiledAssembly;
			Type program = assembly.GetType(string.Format("{0}.PreprocessedT4Template_1", nm));
			ConstructorInfo constructor = program.GetConstructor(new Type[] { });
			var transformer_inst = constructor.Invoke(null);
			var transformer_type = transformer_inst.GetType();
			MethodInfo transform_text_method = transformer_type.GetMethod("TransformText");
			try {
				result = (string)transform_text_method.Invoke(transformer_inst, null);
			}
			catch (Exception e) {
				errors = e.Message;
				return;
			}
		}
	}
}