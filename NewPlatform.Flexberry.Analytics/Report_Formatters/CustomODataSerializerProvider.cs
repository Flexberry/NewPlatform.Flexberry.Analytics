namespace Report_Formatters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.OData;
    using System.Web.OData.Formatter.Serialization;
    using Microsoft.OData.Edm;

    /// <summary>
    /// An CustomODataSerializerProvider is a factory for creating <see cref="T:System.Web.OData.Formatter.Serialization.ODataSerializer"/>s.
    ///
    /// </summary>
    public class CustomODataSerializerProvider : DefaultODataSerializerProvider
    {
        private readonly CustomODataFeedSerializer _feedSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomODataSerializerProvider"/> class.
        /// </summary>
        public CustomODataSerializerProvider()
            : base()
        {
            _feedSerializer = new CustomODataFeedSerializer(this);
        }

        /// <summary>
        /// Gets an <see cref="T:System.Web.OData.Formatter.Serialization.ODataEdmTypeSerializer"/> for the given edmType.
        ///
        /// </summary>
        /// <param name="edmType">The <see cref="T:Microsoft.OData.Edm.IEdmTypeReference"/>.</param>
        /// <returns>
        /// The <see cref="T:System.Web.OData.Formatter.Serialization.ODataSerializer"/>.
        /// </returns>
        public override ODataEdmTypeSerializer GetEdmTypeSerializer(IEdmTypeReference edmType)
        {
            ODataEdmTypeSerializer serializer = base.GetEdmTypeSerializer(edmType);
            if (serializer is ODataFeedSerializer)
            {
                serializer = _feedSerializer;
            }

            return serializer;
        }

        /// <summary>
        /// Gets an <see cref="T:System.Web.OData.Formatter.Serialization.ODataSerializer"/> for the given <paramref name="model"/> and <paramref name="type"/>.
        ///
        /// </summary>
        /// <param name="model">The EDM model associated with the request.</param><param name="type">The <see cref="T:System.Type"/> for which the serializer is being requested.</param><param name="request">The request for which the response is being serialized.</param>
        /// <returns>
        /// The <see cref="T:System.Web.OData.Formatter.Serialization.ODataSerializer"/> for the given type.
        /// </returns>
        public override ODataSerializer GetODataPayloadSerializer(IEdmModel model, Type type, HttpRequestMessage request)
        {
            if (type == typeof(EnumerableQuery<IEdmEntityObject>))
            {
                return _feedSerializer;
            }

            return base.GetODataPayloadSerializer(model, type, request);
        }
    }
}
namespace Report_Formatters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.OData;
    using System.Web.OData.Formatter.Serialization;
    using Microsoft.OData.Edm;
    using Microsoft.OData.Core;
    using Microsoft.OData.Edm.Library;
    using System.Collections;
    using System.Diagnostics.Contracts;


    /// <summary>
    /// OData serializer for serializing a collection of <see cref="IEdmEntityType" />
    /// </summary>
    internal class CustomODataFeedSerializer : ODataFeedSerializer
    {
        /// <summary>
        /// Name for count property in Request.
        /// </summary>
        public const string Count = "CustomODataFeedSerializer_Count";

        /// <returns>
        /// The number of items in the feed.
        /// </returns>
        /// <summary>
        /// Initializes a new instance of <see cref="ODataFeedSerializer"/>.
        /// </summary>
        /// <param name="serializerProvider">The <see cref="ODataSerializerProvider"/> to use to write nested entries.</param>
        public CustomODataFeedSerializer(CustomODataSerializerProvider serializerProvider)
            : base(serializerProvider)
        {
        }

        /// <summary>
        /// Create the <see cref="ODataFeed"/> to be written for the given feed instance.
        /// </summary>
        /// <param name="feedInstance">The instance representing the feed being written.</param>
        /// <param name="feedType">The EDM type of the feed being written.</param>
        /// <param name="writeContext">The serializer context.</param>
        /// <returns>The created <see cref="ODataFeed"/> object.</returns>
        public override ODataFeed CreateODataFeed(IEnumerable feedInstance, IEdmCollectionTypeReference feedType, ODataSerializerContext writeContext)
        {
            var feed = base.CreateODataFeed(feedInstance, feedType, writeContext);

            if (writeContext.Request.Properties.ContainsKey(Count))
            {
                feed.Count = (int)writeContext.Request.Properties[Count];
            }

            return feed;
        }

        /// <summary>
        /// Writes the given object specified by the parameter graph as a whole using the given messageWriter and writeContext.
        /// </summary>
        /// <param name="graph">The object to be written</param>
        /// <param name="type">The type of the object to be written.</param>
        /// <param name="messageWriter">The <see cref="ODataMessageWriter"/> to be used for writing.</param>
        /// <param name="writeContext">The <see cref="ODataSerializerContext"/>.</param>
        public override void WriteObject(object graph, Type type, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
        {
            if (graph is EnumerableQuery<IEdmEntityObject>)
            {
                var list = ((EnumerableQuery<IEdmEntityObject>)graph).AsIList();
                var entityCollectionType = new EdmCollectionTypeReference((EdmCollectionType)((EdmEntitySet)writeContext.NavigationSource).Type);
                graph = new EdmEntityObjectCollection(entityCollectionType, list);
            }

            base.WriteObject(graph, type, messageWriter, writeContext);
        }
    }
    internal static class CollectionExtensions
    {
        /// <summary>
        /// Return the enumerable as a IList of T, copying if required. Avoid mutating the return value.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="enumerable">enumerable</param>
        /// <returns>IList</returns>
        public static IList<T> AsIList<T>(this IEnumerable<T> enumerable)
        {
            Contract.Assert(enumerable != null);

            IList<T> list = enumerable as IList<T>;
            if (list != null)
            {
                return list;
            }

            return new List<T>(enumerable);
        }
    }
}

