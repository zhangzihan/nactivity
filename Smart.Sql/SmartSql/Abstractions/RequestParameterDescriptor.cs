using System;
using System.Collections.Generic;
using System.Text;

namespace SmartSql.Abstractions
{
    public class RequestParameterDescriptor
    {
        private RequestParameterDescriptor()
        {

        }

        public Type ParameterType { get; set; }

        public object Value { get; set; }

        public static RequestParameterDescriptor Create(object value)
        {
            var req = new RequestParameterDescriptor();
            if (value != null)
            {
                req.ParameterType = value.GetType();
            }
            else
            {
                req.ParameterType = Type.Missing.GetType();
            }

            return req;
        }
    }
}
