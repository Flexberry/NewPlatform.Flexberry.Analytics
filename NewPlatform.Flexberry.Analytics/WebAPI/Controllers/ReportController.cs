﻿using ICSSoft.STORMNET;
using NewPlatform.Flexberry.Analytics.Abstractions;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace NewPlatform.Flexberry.Analytics.WebAPI
{

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
            try
            {
                string reportPath = GetParameterValue(parameters, reportPathParamName);
                parameters.Remove(reportPathParamName);
                var result = await _reportManager.GetReportHtml(reportPath, parameters, ct);
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
        /// <returns></returns>
        [HttpPost]
        [Route("export")]
        public async Task<IHttpActionResult> ExportReport([FromBody]JObject parameters, CancellationToken ct)
        {
            try
            {
                string reportPath = GetParameterValue(parameters, reportPathParamName);
                parameters.Remove(reportPathParamName);
                var result = await _reportManager.ExportReport(reportPath, parameters, ct);
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
        [HttpPost]
        [Route("getPageCount")]
        public async Task<IHttpActionResult> GetReportPageCount([FromBody]JObject parameters, CancellationToken ct)
        {
            try
            {
                string reportPath = GetParameterValue(parameters, reportPathParamName);
                parameters.Remove(reportPathParamName);
                var result = await _reportManager.GetReportPageCount(reportPath, parameters, ct);
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
