namespace ReportManager
{
    using Abstractions;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    public class PentahoReportManager : IReportManager
    {
        private readonly HttpClient PentahoHttpClient;

        private string ReportServiceEndpoint => ConfigurationManager.AppSettings["ReportServiceEndpoint"];

        public PentahoReportManager()
        {
            string login = ConfigurationManager.AppSettings["PentahoReportLogin"];
            string password = ConfigurationManager.AppSettings["PentahoReportPassword"];

            var handler = new HttpClientHandler
            {
                Credentials = new NetworkCredential(login, password)
            };

            PentahoHttpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(ReportServiceEndpoint),
                Timeout = Timeout.InfiniteTimeSpan
            };
        }

        public PentahoReportManager(string reportServiceEndpoint, string login, string password)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                return;
            }

            var handler = new HttpClientHandler
            {
                Credentials = new NetworkCredential(login, password)
            };

            PentahoHttpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(reportServiceEndpoint),
                Timeout = Timeout.InfiniteTimeSpan
            };
        }

        /// <summary>
        ///     Возвращает URI для формирования отчёта.
        /// </summary>
        /// <param name="reportPath">Путь к отчёту.</param>
        /// <param name="parameters">Параметры отчёта.</param>
        public string GetReportUri(string reportPath, IDictionary<string, string[]> parameters)
        {
            string requestUri = $"/pentaho/api/repos/{reportPath}/generatedContent?";
            requestUri += string.Join("&", parameters.Select(p => $"{p.Key}={p.Value}"));
            return ReportServiceEndpoint + requestUri;
        }

        /// <summary>
        ///     Получает готовый отчет c сервера отчетов с заданными параметрами.
        /// </summary>
        /// <param name="reportPath">Путь к отчёту.</param>
        /// <param name="parameters">Параметры отчёта.</param>
        public async Task<string> GetReportHtml(string reportPath, JObject parameters, CancellationToken ct)
        {
            if (ct.IsCancellationRequested == true)
            {
                ct.ThrowIfCancellationRequested();
            }

            const string pageNumberKey = "accepted-page";
            if (parameters[pageNumberKey] == null || !int.TryParse(parameters[pageNumberKey].ToString(), out int result))
            {
                parameters.Add(pageNumberKey, 0);
            }

            string requestUri = $"/pentaho/api/repos/{reportPath}/generatedContent?" + GetRequestBody(parameters);

            var response = await PentahoHttpClient.GetAsync(requestUri, ct);
            response.EnsureSuccessStatusCode();
            string reportHtml = await response.Content.ReadAsStringAsync();
            string css = await GetReportStyleSheet(reportHtml, ct);

            // Очищаем отчет от относительных ссылок.
            reportHtml = Regex.Replace(reportHtml, "(<link.*>)", "");

            return $"{reportHtml}<style>{css}</style>";
        }

        /// <summary>
        ///     Получает параметры отчёта.
        /// </summary>
        /// <param name="reportPath">Путь к отчёту.</param>
        public string GetReportParameters(string reportPath)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Экспортирует отчёт.
        /// </summary>
        /// <param name="reportPath">Путь к отчёту.</param>
        /// <param name="parameters">Параметры отчёта.</param>
        public async Task<HttpResponseMessage> ExportReport(string reportPath, JObject parameters, CancellationToken ct)
        {
            if (ct.IsCancellationRequested == true)
            {
                ct.ThrowIfCancellationRequested();
            }

            string contentType = parameters["output-target"].ToString();

            string requestUri = $"/pentaho/api/repos/{reportPath}/generatedContent?" + GetRequestBody(parameters);

            HttpResponseMessage response = await PentahoHttpClient.GetAsync(requestUri, ct);
            response.EnsureSuccessStatusCode();

            byte[] resultData;
            switch (contentType)
            {
                case ("table/csv;page-mode=stream"):
                    byte[] bytes = await response.Content.ReadAsByteArrayAsync();
                    resultData =Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding("windows-1251"), bytes);
                    break;

                default:
                    bytes = await response.Content.ReadAsByteArrayAsync();
                    resultData = bytes;
                    break;
            }

            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                 Content = new ByteArrayContent(resultData),
            };

            return result;
        }

        /// <summary>
        ///     Получает количество страниц в отчёте.
        /// </summary>
        /// <param name="reportPath">Путь к отчёту.</param>
        /// <param name="parameters">Параметры отчёта.</param>
        public async Task<int> GetReportPageCount(string reportPath, JObject parameters, CancellationToken ct)
        {
            if (ct.IsCancellationRequested == true)
            {
                ct.ThrowIfCancellationRequested();
            }

            parameters.Add("changedParameters", "null");

            string requestUri = $"/pentaho/api/repos/{reportPath}/parameter";
            string requestBody = GetRequestBody(parameters);

            var content = new StringContent(requestBody, Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await PentahoHttpClient.PostAsync(requestUri, content, ct);
            string reportMetadata = await response.Content.ReadAsStringAsync();

            var regex = new Regex("page-count=\"([0-9]+)\"");
            var match = regex.Match(reportMetadata);

            if (match.Groups.Count > 1)
            {
                return int.Parse(match.Groups[1].Value);
            }

            return -1;
        }

        /// <summary>
        ///     Получает таблицу стилей для отчёта, извлекая ссылку на нее из разметки отчёта.
        /// </summary>
        /// <param name="reportHtml">Разметка отчёта.</param>
        private async Task<string> GetReportStyleSheet(string reportHtml, CancellationToken ct)
        {
            if (ct.IsCancellationRequested == true)
            {
                ct.ThrowIfCancellationRequested();
            }

            string cssRequestUri = "";
            HttpResponseMessage responseMsg = null;

            var regex = new Regex("<link\\w|\\shref=\"(?=([^\"]*))");
            var match = regex.Match(reportHtml);

            if (match.Groups.Count > 1)
            {
                cssRequestUri = match.Groups[1].Value;
                responseMsg = await PentahoHttpClient.GetAsync(cssRequestUri, ct);
            }

            return responseMsg == null ?
                string.Empty :
                await responseMsg.Content.ReadAsStringAsync();
        }

        /// <summary>
        ///     Возвращает тело GET-запроса для отправки на сервер Pentaho.
        /// </summary>
        /// <param name="requestUri"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private string GetRequestBody(JObject parameters)
        {
            List<string> keyValuePair = new List<string>();

            foreach (var parameter in parameters)
            {
                JToken paramName = parameter.Value.HasValues ? parameter.Value["paramName"] : parameter.Key;
                JToken paramValue = parameter.Value.HasValues ? parameter.Value["value"] : parameter.Value;

                if (paramValue is JArray)
                {
                    foreach (var arrayItem in paramValue)
                    {
                        JToken arrayItemValue = arrayItem["value"] ?? arrayItem;
                        keyValuePair.Add($"{parameter.Key}={arrayItemValue}");
                    }
                }
                else
                {
                    keyValuePair.Add($"{paramName}={paramValue}");
                }
            }

            return string.Join("&",keyValuePair.ToArray());
        }
    }
}
