using System;
using System.Collections.Generic;
using System.Web;

#nullable enable

namespace H.Recognizers.Utilities
{
    internal static class HttpExtensions
    {
        public static Uri WithQuery(this Uri uri, Dictionary<string, string?> dictionary)
        {
            uri = uri ?? throw new ArgumentNullException(nameof(uri));
            dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));

            var builder = new UriBuilder(uri);
            var parameters = HttpUtility.ParseQueryString(uri.Query);

            foreach (var pair in dictionary)
            {
                if (pair.Value == null)
                {
                    continue;
                }

                parameters[pair.Key] = pair.Value;
            }

            builder.Query = parameters.ToString();

            return builder.Uri;
        }
    }
}