namespace Report_WebApi.Controllers
{
    using Abstractions;
    using ICSSoft.STORMNET;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Cors;


    //[OverrideAuthorization]

    [RoutePrefix("api/Report")]
    public class ReportController : ApiController
    {
        const string reportPathParamName = "reportPath";

        private readonly IReportManager _reportManager;

        public ReportController(IReportManager reportManager)
        {
            _reportManager = reportManager;
        }

        /// <summary>
        ///     Получает готовый отчет из сервера отчетов с заданными параметрами.
        /// </summary>
        /// <param name="parameters">Параметры отчёта.</param>
        [HttpPost]
        [Route("getReport")]
        public async Task<IHttpActionResult> GetReportHtml([FromBody]JObject parameters, CancellationToken ct)
        {
            string reportPath = parameters[reportPathParamName].ToString();

            if (string.IsNullOrEmpty(reportPath))
            {
                return BadRequest($"Параметр {reportPathParamName} не должен быть пуст.");
            }
            parameters.Remove(reportPathParamName);

            try
            {
                var result = await _reportManager.GetReportHtml(reportPath, parameters, ct);
                return Ok(result);
            }
            catch (TaskCanceledException tce)
            {
                LogService.LogInfo("Запрос генерации отчета был отменен", tce);
                return null;
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
        /// <returns></returns>
        [HttpPost]
        [Route("export")]
        public async Task<IHttpActionResult> ExportReport([FromBody]JObject parameters, CancellationToken ct)
        {
            try
            {
                string reportPath = parameters[reportPathParamName].ToString();

                if (string.IsNullOrEmpty(reportPath))
                {
                    return BadRequest($"Параметр {reportPathParamName} не должен быть пуст.");
                }

                parameters.Remove(reportPathParamName);

                var result = await _reportManager.ExportReport(reportPath, parameters, ct);

                return ResponseMessage(result);
            }
            catch (TaskCanceledException tce)
            {
                LogService.LogInfo("Запрос экспорта отчета был отменен", tce);
                return null;
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
        [HttpPost]
        [Route("getPageCount")]
        public async Task<IHttpActionResult> GetReportPageCount([FromBody]JObject parameters, CancellationToken ct)
        {
            string reportPath = parameters[reportPathParamName].ToString();
            if (string.IsNullOrEmpty(reportPath))
            {
                return BadRequest($"Параметр {reportPathParamName} не должен быть пуст.");
            }

            parameters.Remove(reportPathParamName);
            try
            {
                var result = await _reportManager.GetReportPageCount(reportPath, parameters, ct);
                return Ok(result);
            }
            catch (TaskCanceledException tce)
            {
                LogService.LogInfo("Запрос получения количества страниц был отменен", tce);
                return null;
            }
            catch (Exception e)
            {
                LogService.LogError("Исключение при получении количества страниц отчёта.", e);
                return InternalServerError(e);
            }
        }
    }
}