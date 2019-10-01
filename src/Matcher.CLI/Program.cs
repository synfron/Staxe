using Newtonsoft.Json;
using Synfron.Staxe.Matcher.Input;
using Synfron.Staxe.Matcher.Interop;
using Synfron.Staxe.Matcher.Interop.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Synfron.Staxe.Matcher.CLI
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			Dictionary<string, List<string>> parsedArgs = GetCommandLineParameters(string.Join(" ", args));

			if (parsedArgs.Count == 0)
			{
				PrintHelpText();
				return;
			}

			string definitionFile = GetDefinitionFile(parsedArgs);
			string outputFolder = GetOutputFolder(parsedArgs);
			OutputType outputType = GetOutputType(parsedArgs);

			string definitionFileText;
			if (!string.IsNullOrEmpty(definitionFile))
			{
				definitionFileText = File.ReadAllText(definitionFile);
			}
			else
			{
				definitionFileText = Console.In.ReadToEnd();
				if (string.IsNullOrWhiteSpace(definitionFileText))
				{
					throw new ArgumentException("A language defintion file is required.");
				}
			}

			try
			{
				LanguageMatcherDefinition languageMatcherModel = JsonConvert.DeserializeObject<LanguageMatcherDefinition>(definitionFileText);
				LanguageMatcher languageMatcher = DefinitionConverter.Convert(languageMatcherModel);

				MatcherClassGenerator generator = new MatcherClassGenerator(languageMatcher);

				if (outputType == OutputType.Assembly)
				{
					generator.OutputAssembly(outputFolder);
				}
				else if (outputFolder == "stdout")
				{
					Console.WriteLine(generator.GetClass());
				}
				else
				{
					generator.OutputClass(outputFolder);
				}
			}
			catch (Exception)
			{
				throw new ArgumentException("A matcher could not be generated using the given definition file.");
			}
		}

		private static void PrintHelpText()
		{
			string text = @"
Usage:
  ./matchercli [<json definition file path>[ [<output directory>[ <output type>]]] [options]
	options:
	  --def		The json definition file path. Default: Read from stdin
	  --out		The directory to output generated file. Not applicable for 'stdout' output type. Default: Current directory
	  --type	Output type (i.e. class, assembly, stdout). Default: class
";
			Console.WriteLine(text);
		}

		private static string GetDefinitionFile(Dictionary<string, List<string>> parsedArgs)
		{
			return parsedArgs.GetValueOrDefault("def")?.FirstOrDefault() ?? parsedArgs.GetValueOrDefault("")?.ElementAtOrDefault(0);
		}

		private static string GetOutputFolder(Dictionary<string, List<string>> parsedArgs)
		{
			return parsedArgs.GetValueOrDefault("out")?.FirstOrDefault() ?? parsedArgs.GetValueOrDefault("")?.ElementAtOrDefault(1) ?? Directory.GetCurrentDirectory();
		}

		private static OutputType GetOutputType(Dictionary<string, List<string>> parsedArgs)
		{
			string type = parsedArgs.GetValueOrDefault("type")?.FirstOrDefault() ?? parsedArgs.GetValueOrDefault("")?.ElementAtOrDefault(2) ?? OutputType.Class.ToString();
			return Enum.Parse<OutputType>(type, true);
		}

		private static Dictionary<string, List<string>> GetCommandLineParameters(string command)
		{
			Dictionary<string, List<string>> parameterPairs = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
			if (!string.IsNullOrWhiteSpace(command))
			{
				string[] parameters = Regex.Match(command.Trim(), "^(?:(?:(?<param>[^\\s\"]+)|(?:\"(?<param>[^\"]+)\"))\\s*)*$").Groups["param"].Captures.Cast<Capture>().Select(capture => capture.Value).ToArray();
				string lastParameter = "";
				bool hasParam = false;


				for (int paramNum = 0; paramNum < parameters.Length; paramNum++)
				{
					if (parameters[paramNum].StartsWith("--"))
					{
						lastParameter = parameters[paramNum].Substring(2);
						hasParam = true;
						parameterPairs.Add(lastParameter, new List<string>());
					}
					else if (hasParam)
					{
						hasParam = false;
						parameterPairs[lastParameter].Add(parameters[paramNum]);
					}
					else
					{
						if (!parameterPairs.TryGetValue("", out List<string> values))
						{
							values = new List<string>();
							parameterPairs[""] = values;
						}
						values.Add(parameters[paramNum]);
					}
				}
			}
			return parameterPairs;
		}
	}
}
