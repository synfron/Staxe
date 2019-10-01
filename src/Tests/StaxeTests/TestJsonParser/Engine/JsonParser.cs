
using Synfron.Staxe.Matcher;
using Synfron.Staxe.Matcher.Data;
using Synfron.Staxe.Shared;
using Synfron.Staxe.Shared.Exceptions;
using System;
using System.Collections.Generic;

namespace StaxeTests.TestJsonParser.Engine
{
	public class JsonParser
	{
		private readonly ILanguageMatchEngine _languageMatchEngine;

		public JsonParser(ILanguageMatchEngine languageMatchEngine)
		{
			_languageMatchEngine = languageMatchEngine;
		}

		public IJsonItem Parse(string json)
		{
			(IMatchData matchData, bool success, int _, int? failureIndex, string _) = _languageMatchEngine.Match(json);
			if (!success)
			{
				int line = EngineUtils.GetLineNumber(json, failureIndex.Value);
				throw new LanaguageSyntaxException($"Syntax error at line {line}: {EngineUtils.GetLine(json, line, 100).Trim()}...", failureIndex.Value);
			}
			JsonItem item = GetJsonItem(((FragmentMatchData)matchData).Parts[0]);
			item.Load(false);
			return item;
		}

		public static JsonItem GetJsonItem(IMatchData matchData)
		{
			switch (matchData.Name)
			{
				case "Object":
					return new JsonObject((FragmentMatchData)matchData);
				case "Array":
					return new JsonArray((FragmentMatchData)matchData);
				case "Boolean":
					return new JsonValue((StringMatchData)matchData, JsonDataType.Boolean);
				case "Number":
					return new JsonValue((StringMatchData)matchData, JsonDataType.Number);
				case "Null":
					return new JsonValue((StringMatchData)matchData, JsonDataType.Null);
				case "String":
					return new JsonValue((StringMatchData)matchData, JsonDataType.String);
				default:
					return null;
			}
		}

		private static string GetStringMatchDataText(StringMatchData matchData)
		{
			return matchData.ToString();
		}

		private static string GetStringText(string value)
		{
			return value.Substring(1, value.Length - 2);
		}

		public abstract class JsonItem : IJsonItem
		{
			protected JsonItem(JsonDataType dataType)
			{
				DataType = dataType;
			}

			public JsonDataType DataType { get; }

			public abstract void Load(bool eager = false);
		}

		private class JsonArray : JsonItem, IJsonArray
		{
			private readonly FragmentMatchData _matchData;
			private bool _loaded = false;
			private IJsonItem[] _items = null;

			public JsonArray(FragmentMatchData matchData) : base(JsonDataType.Array)
			{
				_matchData = matchData;
			}

			public IJsonItem this[int index]
			{
				get
				{
					Load();
					return _items[index];
				}
			}

			public int Count
			{
				get
				{
					Load();
					return _items.Length;
				}
			}

			public override void Load(bool eager = false)
			{
				if (!_loaded)
				{
					List<IMatchData> parts = _matchData.Parts;
					int size = parts.Count;
					IJsonItem[] items = new IJsonItem[size];
					for (int i = 0; i < size; i++)
					{
						JsonItem item = GetJsonItem(parts[i]);
						if (eager)
						{
							item.Load(eager);
						}
						items[i] = item;
					}
					_items = items;
					_loaded = true;
				}
			}
		}

		private class JsonObject : JsonItem, IJsonObject
		{
			private readonly FragmentMatchData _matchData;
			private bool _loaded = false;
			private Dictionary<string, IJsonItem> _items = null;

			public JsonObject(FragmentMatchData matchData) : base(JsonDataType.Object)
			{
				_matchData = matchData;
			}

			public IJsonItem this[string key]
			{
				get
				{
					Load();
					return _items[key];
				}
			}

			public int Count
			{
				get
				{
					Load();
					return _items.Count;
				}
			}

			public ICollection<string> Keys
			{
				get
				{
					Load();
					return _items.Keys;
				}
			}

			public bool ContainsKey(string key)
			{
				return _items.ContainsKey(key);
			}

			public override void Load(bool eager = false)
			{
				if (!_loaded)
				{
					List<IMatchData> parts = _matchData.Parts;
					int size = parts.Count;
					Dictionary<string, IJsonItem> items = new Dictionary<string, IJsonItem>(size);
					for (int i = 0; i < size; i++)
					{
						FragmentMatchData keyValue = (FragmentMatchData)parts[i];
						JsonItem item = GetJsonItem(keyValue.Parts[1]);
						if (eager)
						{
							item.Load(eager);
						}
						items[GetStringText(GetStringMatchDataText((StringMatchData)keyValue.Parts[0]))] = item;
					}
					_items = items;
					_loaded = true;
				}
			}
		}

		private class JsonValue : JsonItem, IJsonValue
		{
			private readonly StringMatchData _matchData;
			private bool _loaded = false;
			private object _value = null;

			public JsonValue(StringMatchData matchData, JsonDataType dataType) : base(dataType)
			{
				_matchData = matchData;
			}

			public object Value
			{
				get
				{
					Load();
					return _value;
				}
			}

			public override void Load(bool eager = false)
			{
				if (!_loaded)
				{
					switch (DataType)
					{
						case JsonDataType.Boolean:
							_value = bool.Parse(GetStringMatchDataText(_matchData));
							break;
						case JsonDataType.Number:
							_value = double.Parse(GetStringMatchDataText(_matchData));
							break;
						case JsonDataType.String:
							_value = GetStringText(GetStringMatchDataText(_matchData));
							break;
						case JsonDataType.Null:
							_value = null;
							break;
						default:
							throw new InvalidCastException($"{DataType} is not supported.");
					}
					_loaded = true;
				}
			}
		}
	}
}
