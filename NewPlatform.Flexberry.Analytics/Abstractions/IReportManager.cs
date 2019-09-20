namespace NewPlatform.Flexberry.Analytics.Abstractions
{
    using Newtonsoft.Json.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     Интерфейс для работы с сервисом отчётов.
    /// </summary>
    public interface IReportManager
    {
        /// <summary>
        ///     Получает готовый отчет c сервера отчетов с заданными параметрами.
        /// </summary>
        /// <param name="reportPath">Путь к отчёту.</param>
        /// <param name="parameters">Параметры отчёта.</param>
        /// <param name="ct">Маркер отмены.</param>
        Task<string> GetReportHtml(string reportPath, JObject parameters, CancellationToken ct);

        /// <summary>
        ///     Получает количество страниц в отчёте.
        /// </summary>
        /// <param name="reportPath">Путь к отчёту.</param>
        /// <param name="parameters">Параметры отчёта.</param>
        /// <param name="ct">Маркер отмены.</param>
        Task<int> GetReportPageCount(string reportPath, JObject parameters, CancellationToken ct);

        /// <summary>
        ///     Экспортирует отчёт.
        /// </summary>
        /// <param name="reportPath">Путь к отчёту.</param>
        /// <param name="parameters">Параметры отчёта.</param>
        /// <param name="ct">Маркер отмены.</param>
        Task<HttpResponseMessage> ExportReport(string reportPath, JObject parameters, CancellationToken ct);
    }
}
