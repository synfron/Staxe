using Synfron.Staxe.Matcher.Input;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Synfron.Staxe.Matcher
{
	public class MatcherEngineGenerator
	{
		private readonly Dictionary<string, string> _existingMethods = new Dictionary<string, string>();
		private readonly Dictionary<string, string> _methodHashes = new Dictionary<string, string>();
		private readonly StringBuilder _codeBuilder = new StringBuilder();

		public static string GenerateEngine(LanguageMatcher languageMatcher)
		{
			MatcherEngineGenerator generator = new MatcherEngineGenerator(languageMatcher);
			return generator.Generate();
		}

		private MatcherEngineGenerator(LanguageMatcher languageMatcher)
		{
			LanguageMatcher = languageMatcher;
			IndexingMode = languageMatcher.IndexingMode;
		}

		public LanguageMatcher LanguageMatcher { get; }

		public IndexingMode IndexingMode
		{
			get;
			private set;
		}

		private string Generate()
		{
			return LanguageMatchEngine.Generate(this).Replace("<<Generated Methods>>", _codeBuilder.ToString());
		}

		public bool TryGetMethod(string methodName, ref string method)
		{
			if (_existingMethods.TryGetValue(methodName, out string storedMethod))
			{
				method = storedMethod;
				return true;
			}
			return false;
		}

		public string Add(string method, string methodName, string code)
		{
			string hashedCode = Hash(Regex.Replace(code, @"\s+", "").Replace(methodName, "<<any>>"));
			if (_methodHashes.TryGetValue(hashedCode, out string storedMethod))
			{
				_existingMethods[methodName] = storedMethod;
				method = storedMethod;
			}
			else
			{
				_methodHashes.Add(hashedCode, method);
				_codeBuilder.AppendLine(code);
			}
			return method;
		}

		public void Add(string methodName, string method)
		{
			_existingMethods.Add(methodName, method);
		}

		private string Hash(string plainText)
		{
			using (SHA256 sha256 = SHA256.Create())
			{
				return BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes(plainText)));
			}
		}
	}
}
