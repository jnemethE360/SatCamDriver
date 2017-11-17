using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edge360.IPSC.Driver.Axis
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class AxisEncodingAttribute : Attribute
    {
        /// <summary>
        /// Creates a new attibute class, with the name of the encoding type, and SDK parameters needed for displaying the feed.
        /// </summary>
        /// <param name="scheme"></param>
        /// <param name="mediaType"></param>
        /// <param name="suffix"></param>
        public AxisEncodingAttribute(AxisEncodingScheme scheme, string mediaType, string suffix)
        {
            MediaType = mediaType;
            Scheme = scheme;
            Suffix = suffix;
        }

        /// <summary>
        /// Gets the media type.
        /// </summary>
        public string MediaType { get; private set; }

        /// <summary>
        /// Gets the url scheme.
        /// </summary>
        public AxisEncodingScheme Scheme { get; private set; }

        /// <summary>
        /// Gets the suffix url part including the leading forward slash.
        /// </summary>
        public string Suffix { get; private set; }
    }
}
