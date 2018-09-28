namespace Abstractions
{
    using Newtonsoft.Json.Linq;
    using System.IO;

    public interface IExcelFormatter
    {
        byte[] GetFormattedExcelByteArray(Stream stream, string reportHeader, JObject parameters);
    }
}
