using System.IO;
using System.IO.Compression;
using System.Reflection;
//copy from DotSpatial.Projections
namespace Agro.LibCore
{
    internal static class DeflateStreamReader
    {
        /// <summary>
        /// Decodedes embedded resource and returns it as Stream
        /// </summary>
        /// <param name="resourceName">Resource name</param>
        /// <returns>Stream</returns>
        public static Stream DecodeEmbeddedResource(string resourceName)
        {
            using var s = Assembly.GetCallingAssembly().GetManifestResourceStream(resourceName);
            var msUncompressed = new MemoryStream();
            using (var ds = new DeflateStream(s, CompressionMode.Decompress, true))
            {
                var buffer = new byte[4096];
                int numRead;
                while ((numRead = ds.Read(buffer, 0, buffer.Length)) != 0)
                {
                    msUncompressed.Write(buffer, 0, numRead);
                }
            }
            msUncompressed.Seek(0, SeekOrigin.Begin);
            return msUncompressed;
        }
    }
}