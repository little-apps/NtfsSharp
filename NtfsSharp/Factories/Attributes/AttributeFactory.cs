using NtfsSharp.Factories.Attributes;
using NtfsSharp.FileRecords;

namespace NtfsSharp.Factories
{
    public class AttributeFactory
    {
        /// <summary>
        /// Builds an attribute
        /// </summary>
        /// <param name="data">Data containg attribute</param>
        /// <param name="fileRecord">File record containing attribute</param>
        /// <returns>Attribute</returns>
        public FileRecords.Attributes.Attribute Build(byte[] data, FileRecord fileRecord)
        {
            var headerFactory = new HeaderFactory();
            var header = headerFactory.Build(data, fileRecord);

            var bodyFactory = new BodyFactory();
            var body = bodyFactory.Build(header);

            return new FileRecords.Attributes.Attribute(header, body);
        }
    }
}
