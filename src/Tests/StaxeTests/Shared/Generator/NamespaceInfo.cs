using System.Collections.Generic;

namespace StaxeTests.Shared.Generator
{
	public class NamespaceInfo
	{
		public string Name
		{
			get;
			set;
		}

		public Dictionary<string, GroupInfo> Groups
		{
			get;
			set;
		} = new Dictionary<string, GroupInfo>();
	}
}
