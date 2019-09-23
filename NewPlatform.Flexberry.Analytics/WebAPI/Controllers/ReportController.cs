namespace NewPlatform.Flexberry.Analytics.WebAPI
{
    using ICSSoft.STORMNET;
    using NewPlatform.Flexberry.Analytics.Abstractions;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;

    public class ReportController : ApiController
    {
        private const string ReportPathParamName = "reportPath";

        private readonly IReportManager ReportManager;

        public ReportController(IReportManager reportManager)
        {
            ReportManager = reportManager;
        }

        /// <summary>
        ///     Получает готовый отчет из сервера отчетов с заданными параметрами.
        /// </summary>
        /// <param name="parameters">Параметры отчёта.</param>
        /// <param name="ct">Маркер отмены.</param>
        [HttpPost]
        [ActionName("getReport")]
        public async Task<IHttpActionResult> GetReportHtml([FromBody]JObject parameters, CancellationToken ct)
        {
            try
            {
                string reportPath = GetParameterValue(parameters, ReportPathParamName);
                parameters.Remove(ReportPathParamName);
                string result = await ReportManager.GetReportHtml(reportPath, parameters, ct);
                return Ok(result);
            }
            catch (TaskCanceledException tce)
            {
                LogService.LogInfo("Запрос генерации отчета был отменен", tce);
                return null;
            }
            catch (ArgumentNullException ane)
            {
                LogService.LogError("Ошибка получения параметра", ane);
                return BadRequest(ane.Message);
            }
            catch (Exception e)
            {
                LogService.LogError("Исключение при построении отчёта.", e);
                return InternalServerError(e);
            }
        }

        /// <summary>
        ///     Экспортирует отчёт.
        /// </summary>
        /// <param name="parameters">Параметры отчёта.</param>
        /// <param name="ct">Маркер отмены.</param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("export")]
        public async Task<IHttpActionResult> ExportReport([FromBody]JObject parameters, CancellationToken ct)
        {
            try
            {
                string reportPath = GetParameterValue(parameters, ReportPathParamName);
                parameters.Remove(ReportPathParamName);
                HttpResponseMessage result = await ReportManager.ExportReport(reportPath, parameters, ct);
                return ResponseMessage(result);
            }
            catch (TaskCanceledException tce)
            {
                LogService.LogInfo("Запрос экспорта отчета был отменен", tce);
                return null;
            }
            catch (ArgumentNullException ane)
            {
                LogService.LogError("Ошибка получения параметра", ane);
                return BadRequest(ane.Message);
            }
            catch (Exception ex)
            {
                LogService.LogError("Исключение при экспорте отчёта.", ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        ///     Получает количество страниц в отчёте.
        /// </summary>
        /// <param name="parameters">Параметры отчёта.</param>
        /// <param name="ct">Маркер отмены.</param>
        [HttpPost]
        [ActionName("getPageCount")]
        public async Task<IHttpActionResult> GetReportPageCount([FromBody]JObject parameters, CancellationToken ct)
        {
            try
            {
                string reportPath = GetParameterValue(parameters, ReportPathParamName);
                parameters.Remove(ReportPathParamName);
                int result = await ReportManager.GetReportPageCount(reportPath, parameters, ct);
                return Ok(result);
            }
            catch (TaskCanceledException tce)
            {
                LogService.LogInfo("Запрос получения количества страниц был отменен", tce);
                return null;
            }
            catch (ArgumentNullException ane)
            {
                LogService.LogError("Ошибка получения параметра", ane);
                return BadRequest(ane.Message);
            }
            catch (Exception e)
            {
                LogService.LogError("Исключение при получении количества страниц отчёта.", e);
                return InternalServerError(e);
            }
        }

        /// <summary>
        /// Получает значение параметра.
        /// </summary>
        /// <param name="parameters">Список параметров.</param>
        /// <param name="parameterName">Имя параметра.</param>
        /// <returns>Возвращает не пустое значение параметра.</returns>
        private string GetParameterValue(JObject parameters, string parameterName)
        {
            if (string.IsNullOrEmpty(parameterName))
            {
                throw new ArgumentNullException();
            }

            string result = parameters.GetValue(parameterName)?.ToString();
            if (string.IsNullOrEmpty(result))
            {
                throw new ArgumentNullException(parameterName, $"Параметр не должен быть пустым.");
            }

            return result;
        }
    }
}
