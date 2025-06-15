using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Yandex.Music.Api.Models.Common;

namespace Yandex.Music.Api.Common.Providers
{
    /// <summary>
    /// Стандартный провайдер запросов
    /// </summary>
    public class DefaultRequestProvider : CommonRequestProvider
    {
        #region Вспомогательные функции

        private Exception ProcessException(Exception ex)
        {
            if (!(ex is WebException webException)) // Replaced 'is not' with '!(is)'
                return ex;

            if (webException.Response is null)
                return ex;

            Stream s = webException.Response.GetResponseStream();
            if (s is null)
                return ex;

            string result = ""; // Fixed variable declaration
            using (StreamReader sr = new StreamReader(s))
            {
                result = sr.ReadToEnd();
            }

            YErrorResponse exception = JsonConvert.DeserializeObject<YErrorResponse>(result);

            return exception ?? ex;
        }

        #endregion Вспомогательные функции

        #region Основные функции

        public DefaultRequestProvider(AuthStorage authStorage) : base(authStorage)
        {
        }

        #endregion Основные функции

        #region IRequestProvider

        public override Task<HttpResponseMessage> GetWebResponseAsync(HttpRequestMessage message)
        {
            try
            {
                HttpMessageHandler handler = new HttpClientHandler()
                {
                    Proxy = storage.Context.WebProxy,
                    AutomaticDecompression = DecompressionMethods.GZip,
                    UseCookies = true,
                    CookieContainer = storage.Context.Cookies,

                };


                HttpClient client = new HttpClient(handler);

                return client.SendAsync(message);
            }
            catch (Exception ex)
            {
                throw ProcessException(ex);
            }
        }

        #endregion IRequestProvider
    }
}