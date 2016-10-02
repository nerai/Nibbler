using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace Nibbler.Core.Simple.Definition
{
	[DataContract]
	public class JsonTmDefinition
	{
		[DataMember]
		public string OriginalDefinition;

		[DataMember]
		public string[] NonfinalStates;

		[DataMember]
		public string AcceptingState;

		[DataMember]
		public string RefusingState;

		[DataMember]
		public string InitialState;

		[DataMember]
		public string[] Sigma;

		[DataMember]
		public string[] Gamma;

		[DataContract]
		public class Transition
		{
			[DataMember]
			public string From;

			[DataMember]
			public string Read;

			[DataMember]
			public string To;

			[DataMember]
			public string Write;

			[DataMember]
			public string Dir;
		}

		[DataMember]
		public Transition[] Delta;

		[DataMember]
		public string Info_Name;

		[DataMember]
		public string Info_ExpectedResult;

		[DataMember]
		public string[] Info_Comment;

		[DataMember]
		public string Info_Url;

		[DataMember]
		public int? SuggestedMacroSize;

		public string Persist ()
		{
			return JsonConvert.SerializeObject (this, Formatting.Indented);
		}

		public static JsonTmDefinition Restore (string s)
		{
			return JsonConvert.DeserializeObject<JsonTmDefinition> (s);
		}
	}
}
