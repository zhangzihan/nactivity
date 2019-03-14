using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace org.activiti.api.runtime.shared.query
{

    /// <summary>
    /// 
    /// </summary>
    public class PageableModelBinderProvider : IModelBinderProvider
    {
        /// <summary>
        /// 
        /// </summary>
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (context.Metadata.ModelType == (typeof(Pageable)))
            {
                return new PageableModelBinder(context.Metadata.ModelType);
            }
            return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class PageableModelBinder : IModelBinder
    {
        /// <summary>
        /// 
        /// </summary>
        public PageableModelBinder(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }
            var context = bindingContext.ActionContext.HttpContext;
            ValueProviderResult result = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            try
            {
                if (typeof(Pageable).IsAssignableFrom(bindingContext.ModelType))
                {
                    var bid = bindingContext.BindingSource?.Id.ToLower();
                    if (string.IsNullOrWhiteSpace(bid))
                    {
                        bid = context.Request.ContentType == "application/x-www-form-urlencoded" || context.Request.ContentType == "multipart/form-data" ? "form" : "body";
                    }
                    Pageable obj = new Pageable();
                    switch (bid)
                    {
                        case "body":
                            var body = context.Request.Body;
                            if (body != null)
                            {
                                var ms = new MemoryStream();
                                var reader = new StreamReader(ms);
                                body.CopyTo(ms);
                                obj = JsonConvert.DeserializeObject<Pageable>(reader.ReadToEnd());
                                ms.Seek(0, SeekOrigin.Begin);
                                context.Request.Body = ms;
                            }
                            break;
                        case "form":
                        case "query":
                            IEnumerable<KeyValuePair<string, StringValues>> query = bid == "form" ?
                                context.Request.Form as IEnumerable<KeyValuePair<string, StringValues>> :
                                context.Request.Query as IEnumerable<KeyValuePair<string, StringValues>>;

                            if (int.TryParse(query.FirstOrDefault(x => x.Key.ToLower() == "pageno").Value, out int pageNo))
                            {
                                obj.PageNo = pageNo;
                            }
                            if (int.TryParse(query.FirstOrDefault(x => x.Key.ToLower() == "pagesize").Value, out int pageSize))
                            {
                                obj.PageSize = pageSize;
                            }
                            else
                            {
                                obj.PageSize = Int32.MaxValue;
                            }

                            string sort = query.FirstOrDefault(x => x.Key.ToLower() == "sort").Value;
                            obj.Sort = new Sort();
                            if (string.IsNullOrWhiteSpace(sort) == false)
                            {
                                obj.Sort.Orders = JsonConvert.DeserializeObject<Sort.Order[]>(sort);
                            }
                            break;
                    }

                    bindingContext.Result = (ModelBindingResult.Success(obj));
                    return Task.CompletedTask;
                }
                return Task.CompletedTask;
            }
            catch (Exception exception)
            {
                if (!(exception is FormatException) && (exception.InnerException != null))
                {
                    exception = ExceptionDispatchInfo.Capture(exception.InnerException).SourceException;
                }
                bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, exception, bindingContext.ModelMetadata);
                return Task.CompletedTask;
            }
        }
    }
}
