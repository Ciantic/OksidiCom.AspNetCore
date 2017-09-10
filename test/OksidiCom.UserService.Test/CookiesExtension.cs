using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace OksidiCom.UserService.Test
{
    public static class CookiesExtension
    {

        public static IDictionary<string, string> GetCookies(this HttpResponseMessage response)
        {
            var result = new Dictionary<string, string>();
            IEnumerable<string> values;
            if (response.Headers.TryGetValues("Set-Cookie", out values))
            {
                SetCookieHeaderValue.ParseList(values.ToList()).ToList().ForEach(cookie =>
                {
                    result.Add(cookie.Name.Value, cookie.Value.Value);
                });
            }
            return result;
        }

        public static HttpRequestMessage SetCookies(this HttpRequestMessage request, IDictionary<string, string> cookies)
        {
            cookies.ToList().ForEach(kv =>
            {
                request.Headers.Add("Cookie", new CookieHeaderValue(kv.Key, kv.Value).ToString());
            });

            return request;
        }
    }
}
