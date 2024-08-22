using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApiDotNet.Application.Payloads.Responses
{
    public class ResponseObject<T>
    {
        public int Status { get; set; } // 400, 404, 403, 401, 500, 200, 201,...
        public string Message { get; set; }
        public T? Data { get; set; }
        public ResponseObject() { }
        public ResponseObject(int status, string message, T? data)
        {
            Status = status;
            Message = message;
            Data = data;
        }
    }
}
