namespace Synfron.Staxe.Matcher.Interop.Model
{
	public class PatternMatcherDefinition
	{
		public string Name { get; set; }

		public string Pattern { get; set; }

		public bool IsNoise { get; set; }

		public bool Mergable { get; set; }

		public string Fragment { get; set; }

		public bool IsAuxiliary { get; set; }
	}
}
