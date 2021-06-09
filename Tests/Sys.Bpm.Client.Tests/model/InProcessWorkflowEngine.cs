//using Microsoft.AspNetCore.Builder.Internal;
//using Microsoft.AspNetCore.Hosting.Internal;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.WebUtilities;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using Newtonsoft.Json;
//using Sys.Workflow.Engine;
//using Sys.Workflow.Engine.Impl.Bpmn.Webservice;
//using Sys.Net.Http;
//using Sys.Workflow;
//using Sys.Workflow.Engine.Bpmn.Rules;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Net.Http.Headers;
//using System.Text;

//namespace Sys.Workflow
//{
//    public class InProcessWorkflowEngine
//    {
//        public IConfiguration Configuration { get; private set; }

//        public IServiceProvider ServiceProvider { get; private set; }

//        public string TenantId { get; set; } = "cb79f3dd-e84e-49b0-95c2-0bdafc80f09d";

//        private HttpClient _httpClient;

//        public class TestUser : IUserInfo
//        {
//            public string Id { get; set; }
//            public string LoginUserId { get; set; }
//            public string FullName { get; set; }
//            public string Email { get; set; }
//            public string Phone { get; set; }
//            public string TenantId { get; set; }
//        }

//        public void Create()
//        {
//            string root = AppDomain.CurrentDomain.BaseDirectory;

//            Configuration = new ConfigurationBuilder()
//                .SetBasePath(root)
//                .AddJsonFile("appsettings.json")
//                .AddJsonFile("resources\\activiti.cfg.json")
//                .Build();

//            IServiceCollection services = new ServiceCollection();

//            services.AddSingleton<IConfiguration>(Configuration)
//                .AddHttpContextAccessor()
//                .AddLogging(builder =>
//                {
//                })
//                .AddHttpClient("workflow")
//                .ConfigureHttpMessageHandlerBuilder(builder =>
//                {
//                    HttpClientHandler handler = new HttpClientHandler
//                    {
//                        MaxRequestContentBufferSize = int.MaxValue,
//                        ClientCertificateOptions = ClientCertificateOption.Automatic,
//                        UseDefaultCredentials = true,
//                        UseProxy = false
//                    };
//                    handler.ServerCertificateCustomValidationCallback += (s, arg1, arg2, arg3) => true;
//                    builder.PrimaryHandler = handler;
//                    builder.Build();
//                });

//            services.AddProcessEngine(Configuration);

//            services.AddSingleton<IUserServiceProxy, DefaultUserServiceProxy>();

//            var sd = services.FirstOrDefault(x => x.ServiceType == typeof(IServiceWebApiHttpProxy));
//            if (sd is object)
//            {
//                services.Remove(sd);
//            }
//            services.AddTransient<IServiceWebApiHttpProxy>(sp =>
//            {
//                IHttpClientProxy proxy = CreateHttpClientProxy(new UserInfo
//                {
//                    Id = "8a010000-5d88-0015-e013-08d6bd87c815",
//                    FullName = "用户1",
//                    TenantId = TenantId
//                });
//                return new ServiceWebApiHttpProxy(proxy, sp.GetService<ILoggerFactory>());
//            });
//            services.AddLogging();

//            ServiceProvider = services.BuildServiceProvider();

//            var app = new ApplicationBuilder(ServiceProvider);
//            var lifetime = new ApplicationLifetime(ServiceProvider.GetService<ILoggerFactory>().CreateLogger<ApplicationLifetime>());

//            IProcessEngine engine = ServiceProvider.UseProcessEngine(lifetime, "InProcess");
//        }

//        private IHttpClientProxy CreateHttpClientProxy(IUserInfo user = null)
//        {
//            HttpClient httpClient = ServiceProvider.GetService<IHttpClientFactory>().CreateClient(); //TestServer.CreateClient();
//            httpClient.BaseAddress = new Uri(Configuration.GetValue<string>("BaseUrl"));

//            string accessToken = WebUtility.UrlEncode(JsonConvert.SerializeObject(user ?? new UserInfo
//            {
//                Id = "8a010000-5d88-0015-e013-08d6bd87c815",
//                FullName = "新用户1",
//                TenantId = TenantId
//            }));

//            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

//            var session = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new
//            {
//                UserId = "8a010000-5d88-0015-e013-08d6bd87c815",
//                FullName = "新用户1",
//                TopOrgId = TenantId,
//                TenantId = TenantId
//            }));

//            _httpClient.DefaultRequestHeaders.Add("Evos-Authentication", WebEncoders.Base64UrlEncode(session));

//            DefaultHttpContext httpContext = new DefaultHttpContext();

//            return new DefaultHttpClientProxy(httpClient,
//                this.Resolve<IAccessTokenProvider>(),
//                new HttpContextAccessor()
//                {
//                    HttpContext = httpContext
//                },
//                Resolve<ILoggerFactory>());
//        }

//        public T Resolve<T>()
//        {
//            return ServiceProvider.GetService<T>();
//        }
//    }
//}
