using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace WebApplication1.Utility {
	public static class Extensions {

		public static T FromJson<T>(this string pJson, bool pCamelCase = false) {

			return (T)Newtonsoft.Json.JsonConvert.DeserializeObject<T>(pJson, pCamelCase ? new Newtonsoft.Json.JsonSerializerSettings { ContractResolver = JavaScriptCamelCaseResolver.Instance } : null);

		}

		/// <summary>
		/// Newtonsoft extension method
		/// </summary>
		public static string ToJson(this object pObject, bool pCamelCase = false) {

			return Newtonsoft.Json.JsonConvert.SerializeObject(pObject, pCamelCase ? new Newtonsoft.Json.JsonSerializerSettings { ContractResolver = JavaScriptCamelCaseResolver.Instance } : null);

		}

		/// <summary>
		/// Newtonsoft extension method
		/// </summary>
		public static string ToJson(this object pObject, Newtonsoft.Json.JsonSerializerSettings pSettings) {

			return Newtonsoft.Json.JsonConvert.SerializeObject(pObject, pSettings);

		}

		/// <summary>
		/// Newtonsoft extension method
		/// </summary>
		public static string ToJson(this object pObject, Newtonsoft.Json.Formatting pFormatting) {

			return Newtonsoft.Json.JsonConvert.SerializeObject(pObject, pFormatting);

		}

		/// <summary>
		/// Newtonsoft extension method
		/// </summary>
		public static string ToJson(this object pObject, Newtonsoft.Json.Formatting pFormatting, Newtonsoft.Json.JsonSerializerSettings pSettings) {

			return Newtonsoft.Json.JsonConvert.SerializeObject(pObject, pFormatting, pSettings);

		}

	}
	public class JavaScriptCamelCaseResolver : DefaultContractResolver {
		public static readonly JavaScriptCamelCaseResolver Instance = new JavaScriptCamelCaseResolver();

		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization) {

			var property = base.CreateProperty(member, memberSerialization);

			property.PropertyName = $"{property.PropertyName.Substring(0, 1).ToLower()}{ property.PropertyName.Substring(1)}";
			return property;

		}

	}

}