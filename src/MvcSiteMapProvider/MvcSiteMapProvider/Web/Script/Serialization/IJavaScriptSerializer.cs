using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Script.Serialization;

namespace MvcSiteMapProvider.Web.Script.Serialization;

/// <summary>
/// Contract for <see cref="JavaScriptSerializer"/> wrapper class.
/// </summary>
public interface IJavaScriptSerializer
{
    object ConvertToType(object obj, Type targetType);
    object Deserialize(string input, Type targetType);
    T Deserialize<T>(string input);
    object DeserializeObject(string input);
    void RegisterConverters(IEnumerable<JavaScriptConverter> converters);
    string Serialize(object obj);
    void Serialize(object obj, StringBuilder output);
}