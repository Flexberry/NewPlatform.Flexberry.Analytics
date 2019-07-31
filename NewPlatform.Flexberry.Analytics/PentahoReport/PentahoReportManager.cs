namespace NewPlatform.Flexberry.Analytics.ReportManager
{
    using NewPlatform.Flexberry.Analytics.Abstractions;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    public class PentahoReportManager : IReportManager
    {
        private readonly HttpClient PentahoHttpClient;

        public PentahoReportManager(string reportServiceEndpoint, string login, string password, int timeout = 0)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(reportServiceEndpoint))
            {
                throw new NullReferenceException();
            }

            var handler = new HttpClientHandler
            {
                Credentials = new NetworkCredential(login, password)
            };

            PentahoHttpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(reportServiceEndpoint),
                Timeout = timeout > 0 ? new TimeSpan(0, 0, timeout) : Timeout.InfiniteTimeSpan
            };
        }

        /// <inheritdoc />
        public async Task<string> GetReportHtml(string reportPath, JObject parameters, CancellationToken ct)
        {
            if (ct.IsCancellationRequested == true)
            {
                ct.ThrowIfCancellationRequested();
            }

            const string pageNumberKey = "accepted-page";
            if (parameters[pageNumberKey] == null || !int.TryParse(parameters[pageNumberKey].ToString(), out _))
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

            return $"<style>{css}</style>{reportHtml}";
        }

        /// <inheritdoc />
        public string GetReportParameters(string reportPath)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage> ExportReport(string reportPath, JObject parameters, CancellationToken ct)
        {
            if (ct.IsCancellationRequested == true)
            {
                ct.ThrowIfCancellationRequested();
            }

            string contentType = parameters.GetValue("output-target")?.ToString();

            string requestUri = $"/pentaho/api/repos/{reportPath}/generatedContent?" + GetRequestBody(parameters);

            HttpResponseMessage response = await PentahoHttpClient.GetAsync(requestUri, ct);
            response.EnsureSuccessStatusCode();

            byte[] resultData;
            switch (contentType)
            {
                case ("table/csv;page-mode=stream"):
                    byte[] bytes = await response.Content.ReadAsByteArrayAsync();
                    resultData = Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding("windows-1251"), bytes);
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

        /// <inheritdoc />
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

            return string.Join("&", keyValuePair.ToArray());
        }
    }
}
