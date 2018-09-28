namespace Report_Formatters
{
    using Abstractions;
    using ClosedXML.Excel;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class ExcelFormatter : IExcelFormatter
    {
        /// <summary>
        ///     Класс для форматирования Excel-отчета согласно требованиям.
        /// </summary>
        /// <param name="stream">Поток, содержащий выгруженный Excel-отчет.</param>
        /// <param name="reportHeader">Заголовок отчета.</param>
        /// <param name="parameters">Параметры отчёта.</param>
        /// <returns></returns>
        public byte[] GetFormattedExcelByteArray(Stream stream, string reportHeader, JObject parameters)
        {
            var workbook = new XLWorkbook(stream);
            var dataSheet = workbook.Worksheet(1);
            dataSheet.Name = "Отчёт";
            //PutHeaderToSheet(ref dataSheet, reportHeader);
            //MakeUpRows(ref dataSheet);

            //var paramsSheet = workbook.AddWorksheet("Параметры");
            //WriteParametersToSheet(paramsSheet, parameters);

            byte[] result;
            using (var resultStream = new MemoryStream())
            {
                workbook.SaveAs(resultStream);
                result = resultStream.ToArray();
            }
            return result;
        }
    }
}
