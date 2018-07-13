/*--------------------------------------------------------------------------
* DynamicJson
* ver 1.2.0.0 (May. 21th, 2010)
*
* created and maintained by neuecc <ils@neue.cc>
* licensed under Microsoft Public License(Ms-PL)
* http://neue.cc/
* http://dynamicjson.codeplex.com/
*--------------------------------------------------------------------------*/

// may 2014 edited by jargoman // have to move this
// edited this to work on mono
// added depth to isDefined, it's now possible to do... isDefined("one.two.three");

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using RippleLibSharp;


using RippleLibSharp.Result;

namespace Codeplex.Data
{
    public class DynamicJson : DynamicObject, IDisposable
    {
        private enum JsonType
        {
            @string, number, boolean, @object, array, @null
        }

        // public static methods

        /// <summary>from JsonSring to DynamicJson</summary>
        public static dynamic Parse(string json)
        {

            return Parse(json, Encoding.Unicode);
        }

		public static bool CanParse (string json)
		{
			DynamicJson d = DynamicJson.Parse(json);
			if (d == null) {
				return false;
			}

			return true;
		}

        /// <summary>from JsonSring to DynamicJson</summary>
        public static dynamic Parse(string json, Encoding encoding)
        {
			
            using (var reader = JsonReaderWriterFactory.CreateJsonReader(encoding.GetBytes(json), XmlDictionaryReaderQuotas.Max))
            {


		var v = XElement.Load (reader);

                return ToValue(v);
            }
        }

        /// <summary>from JsonSringStream to DynamicJson</summary>
        public static dynamic Parse(Stream stream)
        {
            using (var reader = JsonReaderWriterFactory.CreateJsonReader(stream, XmlDictionaryReaderQuotas.Max))
            {
                return ToValue(XElement.Load(reader));
            }
        }

        /// <summary>from JsonSringStream to DynamicJson</summary>
        public static dynamic Parse(Stream stream, Encoding encoding)
        {
            using (var reader = JsonReaderWriterFactory.CreateJsonReader(stream, encoding, XmlDictionaryReaderQuotas.Max, _ => { }))
            {
                return ToValue(XElement.Load(reader));
            }
        }

        /// <summary>create JsonSring from primitive or IEnumerable or Object({public property name:property value})</summary>
        public static string Serialize(object obj)
        {
            return CreateJsonString(new XStreamingElement("root", CreateTypeAttr(GetJsonType(obj)), CreateJsonNode(obj)));
        }

        // private static methods

        private static dynamic ToValue(XElement element)
        {
			
            var type = (JsonType)Enum.Parse(typeof(JsonType), element.Attribute("type").Value);
            switch (type)
            {
                case JsonType.boolean:
                    return (bool)element;
                case JsonType.number:
				
		    return (double)element;
                case JsonType.@string:
                    return (string)element;
                case JsonType.@object:
                case JsonType.array:
                    return new DynamicJson(element, type);
                case JsonType.@null:
                default:
                    return null;
            }
        }

	



        private static JsonType GetJsonType(object obj)
        {
            if (obj == null) return JsonType.@null;

            switch (Type.GetTypeCode(obj.GetType()))
            {
                case TypeCode.Boolean:
                    return JsonType.boolean;
                case TypeCode.String:
                case TypeCode.Char:
                case TypeCode.DateTime:
                    return JsonType.@string;
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                case TypeCode.SByte:
                case TypeCode.Byte:
                    return JsonType.number;
                case TypeCode.Object:
                    return (obj is IEnumerable) ? JsonType.array : JsonType.@object;
                case TypeCode.DBNull:
                case TypeCode.Empty:
                default:
                    return JsonType.@null;
            }
        }

        private static XAttribute CreateTypeAttr(JsonType type)
        {
            return new XAttribute("type", type.ToString());
        }

        private static object CreateJsonNode(object obj)
        {
			
            var type = GetJsonType(obj);
			//Console.Out.Write (type.ToString());
            switch (type)
            {
                case JsonType.@string:
                case JsonType.number:
                    return obj;
                case JsonType.boolean:
                    return obj.ToString().ToLower();
                case JsonType.@object:
                    return CreateXObject(obj);
                case JsonType.array:
                    return CreateXArray(obj as IEnumerable);
                case JsonType.@null:
                default:
                    return null;
            }
        }

        private static IEnumerable<XStreamingElement> CreateXArray<T>(T obj) where T : IEnumerable
        {
            return obj.Cast<object>()
                .Select(o => new XStreamingElement("item", CreateTypeAttr(GetJsonType(o)), CreateJsonNode(o)));
        }

        private static IEnumerable<XStreamingElement> CreateXObject(object obj)
        {
            return obj.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
#pragma warning disable IDE0037 // Use inferred member name
				.Select (pi => new { Name = pi.Name, Value = pi.GetValue(obj, null) })
#pragma warning restore IDE0037 // Use inferred member name
				.Select(a => new XStreamingElement(a.Name, CreateTypeAttr(GetJsonType(a.Value)), CreateJsonNode(a.Value)));
        }

		        private static string CreateJsonString(XStreamingElement element)
		        {
		            using (var ms = new MemoryStream())
		            using (var writer = JsonReaderWriterFactory.CreateJsonWriter(ms, Encoding.Unicode))
		            {
		                element.WriteTo(writer);
						//Console.Out.WriteLine ();
		              	writer.Flush();   // why was flush commented out?

				#if DEBUG
				if ( RippleLibSharp.Util.DebugRippleLibSharp.DynamicJson) {
					byte [] ba = ms.ToArray ();
					Console.Out.WriteLine ("byte bufferlength is " + ba.Length.ToString());
				}
				#endif
		                return Encoding.Unicode.GetString(ms.ToArray());
		            }
		        }

        // dynamic structure represents JavaScript Object/Array

        readonly XElement xml;
        readonly JsonType jsonType;

        /// <summary>create blank JSObject</summary>
        public DynamicJson()
        {
            xml = new XElement("root", CreateTypeAttr(JsonType.@object));
            jsonType = JsonType.@object;
        }

		/*
		~DynamicJson() {
			
			//var doc = xml.Document;

			//doc.RemoveNodes();
			//doc.Remove();

			//doc.Root.RemoveAll();
			//doc.Root.RemoveNodes();
			//doc.Root.Remove();


			//xml.RemoveAll();
			//xml.Remove();
		}
		*/

        private DynamicJson(XElement element, JsonType type)
        {
            Debug.Assert(type == JsonType.array || type == JsonType.@object);

            xml = element;
            jsonType = type;
        }

        public bool IsObject { get { return jsonType == JsonType.@object; } }

        public bool IsArray { get { return jsonType == JsonType.array; } }

        /// <summary>has property or not</summary>
        public bool IsDefined(string name)
        {
			if (!IsObject) {
				return false;
			}
			String[] tokens = name.Split('.');

			XElement x = xml;
			for (int i = 0; i < tokens.Length; i++) {
				x = x.Element(tokens[i]);
				if (x == null) {
					return false;
				}


				//IhildaWallet.Logging.write(tokens[i]);
			}

            return true;
        }
	
        /// <summary>has property or not</summary>
        public bool IsDefined(int index)
        {
            return IsArray && (xml.Elements().ElementAtOrDefault(index) != null);
        }

        /// <summary>delete property</summary>
        public bool Delete(string name)
        {
            var elem = xml.Element(name);
            if (elem != null)
            {
                elem.Remove();
                return true;
            }
            else return false;
        }

        /// <summary>delete property</summary>
        public bool Delete(int index)
        {
            var elem = xml.Elements().ElementAtOrDefault(index);
            if (elem != null)
            {
                elem.Remove();
                return true;
            }
            else return false;
        }

        /// <summary>mapping to Array or Class by Public PropertyName</summary>
        public T Deserialize<T>()
        {
            return (T)Deserialize(typeof(T));
        }

        private object Deserialize(Type type)
        {
            return (IsArray) ? DeserializeArray(type) : DeserializeObject(type);
        }


		private dynamic DeserializeValue(XElement xelement, Type targetType){
#if DEBUG

			string method_sig = nameof (DeserializeValue);

			if (RippleLibSharp.Util.DebugRippleLibSharp.DynamicJson) {
				RippleLibSharp.Util.Logging.WriteLog ("\n\nDeserializeValue : ");
				RippleLibSharp.Util.Logging.WriteLog ("targetType=" + targetType.ToString());
				//RippleLibSharp.Util.Logging.writeLog ("xelement=" + xelement.ToString());
			}
			#endif



			//JsonType xelementType = (JsonType)Enum.Parse(typeof(JsonType), xelement.Attribute("type").Value);

			// var worked. I switched to dynamic because that's what the compiler uses anyway. (var lets the compilet pick the object)
			dynamic value = ToValue(xelement);		

			if (value == null) {
				#if DEBUG
				if (RippleLibSharp.Util.DebugRippleLibSharp.DynamicJson) {
					RippleLibSharp.Util.Logging.WriteLog ("value == null");
				}
				#endif
				return null;
			}

			Type valuesActualType = value.GetType ();


			if (targetType == typeof(Decimal)) {
				#if DEBUG
				if (RippleLibSharp.Util.DebugRippleLibSharp.DynamicJson) {
					RippleLibSharp.Util.Logging.WriteLog ("targetType == typeof(Decimal)");
				}
				#endif


				if (valuesActualType == typeof(double)) {
					#if DEBUG
					if (RippleLibSharp.Util.DebugRippleLibSharp.DynamicJson) {
						RippleLibSharp.Util.Logging.WriteLog ("valuesActualType == typeof(double)");
					}
					#endif
					return Convert.ToDecimal (value);
				}
			}
			//





			if (value is DynamicJson val) {

#if DEBUG
				if (RippleLibSharp.Util.DebugRippleLibSharp.DynamicJson) {
					RippleLibSharp.Util.Logging.WriteLog ("val=" + val.ToString ());
					RippleLibSharp.Util.Logging.WriteLog ("targetTypr =" + targetType.ToString ());
				}
#endif

				Type respType = new Json_Response ().GetType ();

				if (targetType.Equals (respType)) {
#if DEBUG
					if (RippleLibSharp.Util.DebugRippleLibSharp.DynamicJson) {
						RippleLibSharp.Util.Logging.WriteLog ("elementType.Equals(ot)");
					}
#endif
					return new Json_Response (val.ToString ());

				}

				value = val.Deserialize (targetType);




				if (value is IRawJsonInterface jsonObj) {
					
					jsonObj.RawJSON = val.ToString ();
				}

			}


			if (value is string) {
				string s = value as string;

				#if DEBUG
				if (RippleLibSharp.Util.DebugRippleLibSharp.DynamicJson) {
					RippleLibSharp.Util.Logging.WriteLog ("value as string = " + s);
				}
				#endif

				/*
				if (targetType == typeof(string)) {
					return s;
				}
				*/


			}

			bool b = typeof(IConvertible).IsAssignableFrom (targetType);

			if (b) {
				#if DEBUG
				if (RippleLibSharp.Util.DebugRippleLibSharp.DynamicJson) {
					RippleLibSharp.Util.Logging.WriteLog ("elementType is convertable");
				}
				#endif
				return Convert.ChangeType (value, targetType);
			}

			try {

				if (value is IConvertible iconvertible) {
#if DEBUG
					if (RippleLibSharp.Util.DebugRippleLibSharp.DynamicJson) {
						RippleLibSharp.Util.Logging.WriteLog ("iconvertible != null");
					}
#endif

					return Convert.ChangeType (iconvertible, targetType);
				}
			}

#pragma warning disable 0168
			catch (Exception e) {
#pragma warning restore 0168

#if DEBUG
				if (RippleLibSharp.Util.DebugRippleLibSharp.DynamicJson) {
					RippleLibSharp.Util.Logging.ReportException (method_sig, e);
				}
#endif
			}



			//RippleLibSharp.Util.Debug.DynamicJson
				
		/*
		if () {
			
		} else {*/

			/*
			Type currencyType = new RippleLibSharp.Transactions.RippleCurrency ().GetType ();
			if (targetType.Equals(currencyType)) {
				RippleLibSharp.Util.Logging.writeLog ("elementType.Equals(ct)");
				if (valuesActualType == typeof(string)) {
					return new RippleLibSharp.Transactions.RippleCurrency (value);

				} else {
					object[] args = new object[] { value };
					value = Activator.CreateInstance (targetType, args);
					return value;
				}
				//return value;
			}
			*/

			#if DEBUG
			if (RippleLibSharp.Util.DebugRippleLibSharp.DynamicJson) {
				RippleLibSharp.Util.Logging.WriteLog ("elementType Is NOT Iconvertable\n");
			}
			#endif
				Type ot = new Object ().GetType ();

			if (targetType.Equals(ot)) {
				#if DEBUG
				if (RippleLibSharp.Util.DebugRippleLibSharp.DynamicJson) {
					RippleLibSharp.Util.Logging.WriteLog ("elementType.Equals(ot)");
				}
				#endif
				return value;
			}

			#if DEBUG
			if (RippleLibSharp.Util.DebugRippleLibSharp.DynamicJson) {
				RippleLibSharp.Util.Logging.WriteLog ("valuesActualType=" + valuesActualType.ToString());
			}
			#endif

			if (targetType.IsGenericType
				&& targetType.GetGenericTypeDefinition() == typeof(Nullable<>)
				&& targetType.GetGenericArguments().Any(t => t.IsValueType && t.IsPrimitive))
			{
				// it's a nullable primitive
				Type[] innerPrimative = targetType.GetGenericArguments();

				return Convert.ChangeType (value, innerPrimative[0]);

			}


			/* good intentions but we already called ChangeType above since primatives ARE Iconvertable
			if (targetType.IsPrimitive  ) {
				RippleLibSharp.Util.Logging.writeLog ("elementType.IsPrimitive\n");

				//if (xelementType.i) {

				//}
				return Convert.ChangeType (value, targetType);
			}
			*/

			/* this causes a false positive and raises an exception trying to convert string to string[] */

			try {
			if (!targetType.Equals (valuesActualType)) {
					#if DEBUG
					if (RippleLibSharp.Util.DebugRippleLibSharp.DynamicJson) {
						RippleLibSharp.Util.Logging.WriteLog ("!elementType.Equals( value.GetType())");
					}
					#endif
				object[] args2 = { value };
				value = Activator.CreateInstance (targetType, args2);	
			}
			}

			#pragma warning disable 0168
			catch (Exception e ) {
			#pragma warning restore 0168

				#if DEBUG
				if (RippleLibSharp.Util.DebugRippleLibSharp.DynamicJson) {
					RippleLibSharp.Util.Logging.WriteLog(e.Message);
				}

				#endif
			}
				

			return value;
		/*}*/
		
        }



        private object DeserializeObject(	Type targetType)
		{	
			#if DEBUG
			if (RippleLibSharp.Util.DebugRippleLibSharp.DynamicJson) {
				RippleLibSharp.Util.Logging.WriteLog ("\n\nDeserializeObject()\n");
				RippleLibSharp.Util.Logging.WriteLog ("targetType=" + targetType.ToString());
			}
			#endif
            	var result = Activator.CreateInstance(targetType);
            	var dict = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite)
                .ToDictionary(pi => pi.Name, pi => pi);
            foreach (var item in xml.Elements()){
#if DEBUG
				if (RippleLibSharp.Util.DebugRippleLibSharp.DynamicJson) {
					RippleLibSharp.Util.Logging.WriteLog ("local=" + item.Name.LocalName);
				}
#endif
				if (!dict.TryGetValue(item.Name.LocalName, out PropertyInfo propertyInfo)) continue;
				#if DEBUG
				if (RippleLibSharp.Util.DebugRippleLibSharp.DynamicJson) {
					RippleLibSharp.Util.Logging.WriteLog ("prop=" + propertyInfo.ToString());
				}
				#endif
                var value = DeserializeValue(item, propertyInfo.PropertyType);

                propertyInfo.SetValue(result, value, null);
            }
            return result;
        }

        private object DeserializeArray(Type targetType)
        {
            if (targetType.IsArray) // Foo[]
            {
                var elemType = targetType.GetElementType();
                dynamic array = Array.CreateInstance(elemType, xml.Elements().Count());
                var index = 0;
                foreach (var item in xml.Elements())
                {
                    array[index++] = DeserializeValue(item, elemType);
                }
                return array;
            }
            else // List<Foo>
            {
		Type[] ta = targetType.GetGenericArguments ();
				Type elemType = null;
				if (ta.Length > 0) {
					elemType = ta[0];
				}
                dynamic list = Activator.CreateInstance(targetType);

				//int i = 0;
                foreach (var item in xml.Elements())
                {
                    list.Add(DeserializeValue(item, elemType));
                }
                return list;
            }
        }

        // Delete
        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            result = (IsArray)
                ? Delete((int)args[0])
                : Delete((string)args[0]);
            return true;
        }

        // IsDefined, if has args then TryGetMember
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (args.Length > 0)
            {
                result = null;
                return false;
            }

            result = IsDefined(binder.Name);
            return true;
        }

        // Deserialize or foreach(IEnumerable)
        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            if (binder.Type == typeof(IEnumerable) || binder.Type == typeof(object[]))
            {
                var ie = (IsArray)
                    ? xml.Elements().Select(ToValue)
                    : xml.Elements().Select(x => (dynamic)new KeyValuePair<string, object>(x.Name.LocalName, ToValue(x)));
                result = (binder.Type == typeof(object[])) ? ie.ToArray() : ie;
            }
            else
            {
                result = Deserialize(binder.Type);
            }
            return true;
        }

        private bool TryGet(XElement element, out object result)
        {
            if (element == null)
            {
                result = null;
                return false;
            }

            result = ToValue(element);
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            return (IsArray)
                ? TryGet(xml.Elements().ElementAtOrDefault((int)indexes[0]), out result)
                : TryGet(xml.Element((string)indexes[0]), out result);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return (IsArray)
                ? TryGet(xml.Elements().ElementAtOrDefault(int.Parse(binder.Name)), out result)
                : TryGet(xml.Element(binder.Name), out result);
        }

        private bool TrySet(string name, object value)
        {
            var type = GetJsonType(value);
            var element = xml.Element(name);
            if (element == null)
            {
                xml.Add(new XElement(name, CreateTypeAttr(type), CreateJsonNode(value)));
            }
            else
            {
                element.Attribute("type").Value = type.ToString();
                element.ReplaceNodes(CreateJsonNode(value));
            }

            return true;
        }

        private bool TrySet(int index, object value)
        {
            var type = GetJsonType(value);
            var e = xml.Elements().ElementAtOrDefault(index);
            if (e == null)
            {
                xml.Add(new XElement("item", CreateTypeAttr(type), CreateJsonNode(value)));
            }
            else
            {
                e.Attribute("type").Value = type.ToString();
                e.ReplaceNodes(CreateJsonNode(value));
            }

            return true;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            return (IsArray)
                ? TrySet((int)indexes[0], value)
                : TrySet((string)indexes[0], value);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            return (IsArray)
                ? TrySet(int.Parse(binder.Name), value)
                : TrySet(binder.Name, value);
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return (IsArray)
                ? xml.Elements().Select((x, i) => i.ToString())
                : xml.Elements().Select(x => x.Name.LocalName);
        }

        /// <summary>Serialize to JsonString</summary>
        public override string ToString()
        {
            // <foo type="null"></foo> is can't serialize. replace to <foo type="null" />
            foreach (var elem in xml.Descendants().Where(x => x.Attribute("type").Value == "null"))
            {
                elem.RemoveNodes();
            }
            return CreateJsonString(new XStreamingElement("root", CreateTypeAttr(jsonType), xml.Elements()));
        }

		#region IDisposable implementation
		public void Dispose ()
		{

			//this.xml.Document.RemoveNodes();
			this.xml.RemoveAll();




		}
		#endregion

    }
}