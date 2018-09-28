namespace Abstractions
{
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     Интерфейс для работы с сервисом отчётов.
    /// </summary>
    public interface IReportManager
    {
        /// <summary>
        ///     Возвращает URI для формирования отчёта.
        /// </summary>
        /// <param name="reportPath">Путь к отчёту.</param>
        /// <param name="parameters">Параметры отчёта.</param>
        string GetReportUri(string reportPath, IDictionary<string, string[]> parameters);

        /// <summary>
        ///     Получает готовый отчет c сервера отчетов с заданными параметрами.
        /// </summary>
        /// <param name="reportPath">Путь к отчёту.</param>
        /// <param name="parameters">Параметры отчёта.</param>
        Task<string> GetReportHtml(string reportPath, JObject parameters, CancellationToken ct);

        /// <summary>
        ///     Получает параметры отчёта.
        /// </summary>
        /// <param name="reportPath">Путь к отчёту.</param>
        string GetReportParameters(string reportPath);

        /// <summary>
        ///     Получает количество страниц в отчёте.
        /// </summary>
        /// <param name="reportPath">Путь к отчёту.</param>
        /// <param name="parameters">Параметры отчёта.</param>
        Task<int> GetReportPageCount(string reportPath, JObject parameters, CancellationToken ct);

        /// <summary>
        ///     Экспортирует отчёт.
        /// </summary>
        /// <param name="reportPath">Путь к отчёту.</param>
        /// <param name="parameters">Параметры отчёта.</param>
        Task<HttpResponseMessage> ExportReport(string reportPath, JObject parameters, CancellationToken ct);
    }
}
