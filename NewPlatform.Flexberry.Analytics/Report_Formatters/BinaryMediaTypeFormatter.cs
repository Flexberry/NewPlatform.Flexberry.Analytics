using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Report_Formatters
{
    //public class BinaryMediaTypeFormatter : BufferedMediaTypeFormatter
    //{

    //    private static Type _supportedType = typeof(byte[]);
    //    private const int BufferSize = 8192; // 8K 

    //    public BinaryMediaTypeFormatter()
    //    {
    //        SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/octet-stream"));
    //        //MediaTypeMappings.Add();
    //    }

    //    public override bool CanReadType(Type type)
    //    {
    //        return type == _supportedType;
    //    }

    //    public override bool CanWriteType(Type type)
    //    {
    //        return type == _supportedType;
    //    }

    //    public  Task<object> ReadFromStreamAsync(Type type, Stream stream, HttpContentHeaders contentHeaders, IFormatterLogger formatterLogger)
    //    {
    //        var taskSource = new TaskCompletionSource<object>();
    //        try
    //        {
    //            var ms = new MemoryStream();
    //            stream.CopyTo(ms, BufferSize);
    //            taskSource.SetResult(ms.ToArray());
    //        }
    //        catch (Exception e)
    //        {
    //            taskSource.SetException(e);
    //        }
    //        return taskSource.Task;
    //    }

    //    public Task WriteToStreamAsync(Type type, object value, Stream stream, HttpContentHeaders contentHeaders, TransportContext transportContext)
    //    {
    //        var taskSource = new TaskCompletionSource<object>();
    //        try
    //        {
    //            if (value == null)
    //                value = new byte[0];
    //            var ms = new MemoryStream((byte[])value);
    //            ms.CopyTo(stream);
    //            taskSource.SetResult(null);
    //        }
    //        catch (Exception e)
    //        {
    //            taskSource.SetException(e);
    //        }
    //        return taskSource.Task;
    //    }
    //}

    public class BinaryMediaTypeFormatter : MediaTypeFormatter
    {

        private static Type _supportedType = typeof(byte[]);
        private bool _isAsync = false;

        public BinaryMediaTypeFormatter() : this(false)
        {
        }

        public BinaryMediaTypeFormatter(bool isAsync)
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/octet-stream"));
            MediaTypeMappings.Add();
            IsAsync = isAsync;
        }

        public bool IsAsync
        {
            get { return _isAsync; }
            set { _isAsync = value; }
        }


        public override bool CanReadType(Type type)
        {
            return type == _supportedType;
        }

        public override bool CanWriteType(Type type)
        {
            return type == _supportedType;
        }

        public Task<object> ReadFromStreamAsync(Type type, Stream stream,HttpContentHeaders contentHeaders, IFormatterLogger formatterLogger)
        {
            Task<object> readTask = GetReadTask(stream);
            if (_isAsync)
            {
                readTask.Start();
            }
            else
            {
                readTask.RunSynchronously();
            }
            return readTask;

        }

        private Task<object> GetReadTask(Stream stream)
        {
            return new Task<object>(() =>
            {
                var ms = new MemoryStream();
                stream.CopyTo(ms);
                return ms.ToArray();
            });
        }

        private Task GetWriteTask(Stream stream, byte[] data)
        {
            return new Task(() =>
            {
                var ms = new MemoryStream(data);
                ms.CopyTo(stream);
            });
        }


        public Task WriteToStreamAsync(Type type, object value, Stream stream,
            HttpContentHeaders contentHeaders, TransportContext transportContext)
        {

            if (value == null)
                value = new byte[0];
            Task writeTask = GetWriteTask(stream, (byte[])value);
            if (_isAsync)
            {
                writeTask.Start();
            }
            else
            {
                writeTask.RunSynchronously();
            }
            return writeTask;
        }
    }
}
