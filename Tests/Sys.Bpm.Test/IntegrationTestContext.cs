using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.api.model.converter;
using org.activiti.cloud.services.core.pageable;
using org.activiti.cloud.services.rest.api;
using org.activiti.engine;
using Sys.Bpm.rest.controllers;
using Sys.Bpm.Services.Rest;
using Sys.Net.Http;
using Sys.Workflow;
using Sys.Workflow.Engine.Bpmn.Rules;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace Sys.Bpmn.Test
{
    /// <summary>
    /// 集成测试上下文
    /// </summary>
    public class IntegrationTestContext
    {
        public IConfiguration Configuration { get; set; }

        public HttpContext HttpContext { get; private set; }

        public TestServer TestServer { get; private set; }

        public IntegrationTestContext()
        {

        }

        /// <summary>
        /// 解析服务
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public object Resolve(Type serviceType)
        {
            return TestServer.Host.Services.GetService(serviceType);
        }

        /// <summary>
        /// 读取bpmn文件
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string ReadBpmn(string name)
        {
            string root = AppDomain.CurrentDomain.BaseDirectory;

            string xml = File.ReadAllText(Path.Combine(root, "resources\\samples", name));

            return xml;
        }

        /// <summary>
        /// 读取bpmn文件
        /// </summary>
        /// <param name="name"></param>
        /// <param name="processKeyName"></param>
        /// <returns></returns>
        public string ReadBpmn(string name, string processKeyName)
        {
            string root = AppDomain.CurrentDomain.BaseDirectory;

            string xml = File.ReadAllText(Path.Combine(root, "resources\\samples", name));

            XDocument doc = XDocument.Load(new MemoryStream(Encoding.UTF8.GetBytes(xml)), LoadOptions.PreserveWhitespace);

            XElement elem = doc.Descendants(XName.Get("process", "http://www.omg.org/spec/BPMN/20100524/MODEL")).First();

            elem.Attribute("id").Value = processKeyName;

            return doc.ToString();
        }

        /// <summary>
        /// 解析服务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Resolve<T>()
        {
            return TestServer.Host.Services.GetService<T>();
        }

        public string TenantId => "13ddb8dd-99e3-477c-8316-7928e938ce80";

        public string AuthUserId => "验证用户";

        internal static IntegrationTestContext unitTestContext;

        private static object syncRoot = new object();

        private readonly object syncClient = new object();

        private readonly object syncDep = new object();

        /// <summary>
        /// 创建测试环境上下文
        /// </summary>
        /// <returns></returns>
        public static IntegrationTestContext CreateDefaultUnitTestContext()
        {
            lock (syncRoot)
            {
                if (unitTestContext == null)
                {
                    AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
                    AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                    unitTestContext = new IntegrationTestContext();

                    unitTestContext.HttpContext = new DefaultHttpContext();

                    string root = AppDomain.CurrentDomain.BaseDirectory;

                    unitTestContext.Configuration = new ConfigurationBuilder()
                        .SetBasePath(root)
                        .AddJsonFile("appsettings.json")
                        .AddJsonFile("resources\\activiti.cfg.json")
                        .Build();

                    IWebHostBuilder webHost = new WebHostBuilder()
                        .UseConfiguration(unitTestContext.Configuration)
                        .UseStartup<UnitTestStartup>();

                    unitTestContext.TestServer = new TestServer(webHost);
                }

                return unitTestContext;
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
        }

        private static void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
        }

        public Deployment HttpDeployProcess(string bpmnName, string bpmnFile, bool enableDuplicateFiltering = true)
        {
            ProcessDefinitionDeployer pdd = new ProcessDefinitionDeployer();

            pdd.BpmnXML = ReadBpmn(bpmnFile);
            pdd.Name = bpmnName;
            pdd.DisableBpmnValidation = false;
            pdd.EnableDuplicateFiltering = enableDuplicateFiltering;
            pdd.TenantId = TenantId;

            HttpResponseMessage response = PostAsync<HttpResponseMessage>($"{WorkflowConstants.PROC_DEP_ROUTER_V1}", pdd).Result;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Deployment deployment = JsonConvert.DeserializeObject<Deployment>(response.Content.ReadAsStringAsync().Result);

            return deployment;
        }

        public Deployment HttpDeployProcess(string bpmnFile = "简单顺序流.bpmn", bool enableDuplicateFiltering = true)
        {
            return HttpDeployProcess(Path.GetFileNameWithoutExtension(bpmnFile), bpmnFile, enableDuplicateFiltering);
        }

        public IHttpClientProxy CreateHttpClientProxy(IUserInfo user = null)
        {
            HttpClient httpClient = TestServer.CreateClient();
            httpClient.BaseAddress = new Uri("http://localhost");

            string accessToken = WebUtility.UrlEncode(JsonConvert.SerializeObject(user ?? new UserInfo
            {
                Id = "新用户1",
                Name = "新用户1",
                TenantId = ""
            }));

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            DefaultHttpContext httpContext = new DefaultHttpContext();
            
            return new DefaultHttpClientProxy(httpClient, 
                this.Resolve<IAccessTokenProvider>(), 
                httpContext);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <returns></returns>
        public Task<T> GetAsync<T>(string url, IUserInfo user = null)
        {
            IHttpClientProxy httpClient = CreateHttpClientProxy(user);

            return httpClient.GetAsync<T>(url);
        }

        public Task<T> PostAsync<T>(string url, object data, IUserInfo user = null)
        {
            IHttpClientProxy httpClient = CreateHttpClientProxy(user);

            return httpClient.PostAsync<T>(url, data);
        }

        /// <summary>
        /// lock锁保证在并发测试是只有一个测试实例传入，并且保证http调用按顺序执行.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public void Invoke(HttpInvoke[] posts)
        {
            lock (syncClient)
            {
                foreach (HttpInvoke post in posts)
                {
                    IHttpClientProxy httpClient = CreateHttpClientProxy(post.User);

                    HttpResponseMessage response = null;

                    if (post.Method == HttpMethod.Post)
                    {
                        response = httpClient.PostAsync<HttpResponseMessage>(post.Url, post.Data).Result;
                    }
                    else if (post.Method == HttpMethod.Get)
                    {
                        response = httpClient.GetAsync<HttpResponseMessage>(post.Url).Result;
                    }

                    post.Response.Invoke(response);
                }
            }
        }
    }

    public class UnitTestStartup
    {
        public IConfiguration Configuration { get; }

        public IServiceProvider ServiceProvider { get; private set; }

        public UnitTestStartup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        { 
            services.AddSingleton<IConfiguration>(Configuration)
                .AddHttpContextAccessor()
                .AddHttpClient()
                .AddLogging()
                .AddProcessEngine(Configuration);

            var sd = services.FirstOrDefault(x => x.ServiceType == typeof(IUserServiceProxy));
            if (sd != null)
            {
                services.Remove(sd);
            }

            services.AddSingleton<IUserServiceProxy, DefaultUserServiceProxy>();

            IMvcBuilder mvcBuilder = services.AddMvc()
                .AddProcessEngineRestServices(Configuration)
                .SetCompatibilityVersion(CompatibilityVersion.Latest);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            ServiceProvider = app.ApplicationServices;

            app.UseProcessEngine();

            app.UseCors(cors =>
                cors.AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin());

            app.UseWorkflow();

            app.UseMvc();
        }
    }

    public class HttpInvoke
    {
        public string Url { get; set; }

        public IUserInfo User { get; set; }

        public object Data { get; set; }

        public Action<HttpResponseMessage> Response { get; set; }

        public HttpMethod Method { get; set; } = HttpMethod.Post;
    }
}
