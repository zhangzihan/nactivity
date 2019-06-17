using BpmnTest.Test;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sys.Workflow.Cloud.Services.Api.Commands;
using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Cloud.Services.Rest.Api;
using Sys.Workflow.Engine.Impl.Bpmn.Webservice;
using Sys.Workflow.Rest.Client;
using Sys.Workflow.Services.Rest;
using Sys.Net.Http;
using Sys.Workflow;
using Sys.Workflow.Engine.Bpmn.Rules;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace Sys.Workflown.Test
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

        public string[] Guid2IntString(Guid value)
        {
            byte[] b = value.ToByteArray();
            int bint = Math.Abs(BitConverter.ToInt32(b, 0));
            var bint1 = Math.Abs(BitConverter.ToInt32(b, 4));
            var bint2 = Math.Abs(BitConverter.ToInt32(b, 8));
            var bint3 = Math.Abs(BitConverter.ToInt32(b, 12));
            return new[] { bint.ToString(), bint1.ToString(), bint2.ToString(), bint3.ToString() };
        }

        /// <summary>
        /// 读取bpmn文件
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string ReadBpmn(string name)
        {
            string root = AppDomain.CurrentDomain.BaseDirectory;

            string xml = File.ReadAllText(Path.Combine(new string[] { root, "resources", "samples", name }));

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

            string xml = File.ReadAllText(Path.Combine(new string[] { root, "resources", "samples", name }));

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

        public string TenantId => "cb79f3dd-e84e-49b0-95c2-0bdafc80f09d";

        public string AuthUserId => "8a010000-5d88-0015-e013-08d6bd87c815";

        internal static IntegrationTestContext testContext;

        private static object syncRoot = new object();

        private readonly object syncClient = new object();

        /// <summary>
        /// 创建测试环境上下文
        /// </summary>
        /// <returns></returns>
        public static IntegrationTestContext CreateDefaultUnitTestContext()
        {
            lock (syncRoot)
            {
                if (testContext == null)
                {
                    testContext = new IntegrationTestContext
                    {
                        HttpContext = new DefaultHttpContext()
                    };

                    string root = AppDomain.CurrentDomain.BaseDirectory;

                    testContext.Configuration = new ConfigurationBuilder()
                        .SetBasePath(root)
                        .AddJsonFile("appsettings.json")
                        .AddJsonFile("resources\\activiti.cfg.json")
                        .Build();

                    IWebHostBuilder webHost = new WebHostBuilder()
                        .UseConfiguration(testContext.Configuration)
                        .ConfigureLogging((host, logging) =>
                        {
                            logging.ClearProviders();
                            logging.AddConfiguration(host.Configuration.GetSection("Logging"));
                            //logging.AddConsole();
                            //logging.AddDebug();
                            //logging.AddEventSourceLogger();
                            logging.AddFile("Logs/nactiviti-{Date}.json", LogLevel.Error, isJson: true);
                        })
                        .UseStartup<IntegrationTestStartup>();

                    testContext.TestServer = new TestServer(webHost);

                    AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
                    AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                }

                return testContext;
            }
        }

        private static ConcurrentDictionary<string, ProcessDefinition> procDefines = new ConcurrentDictionary<string, ProcessDefinition>();

        public ProcessDefinition GetOrAddProcessDefinition(string bpmnFile)
        {
            return procDefines.GetOrAdd(bpmnFile, (key) =>
            {
                return AsyncHelper.RunSync(() => DeploySampleBpmn(key));
            });
        }

        private async Task<ProcessDefinition> DeploySampleBpmn(string bpmnFile)
        {
            string root = AppDomain.CurrentDomain.BaseDirectory;
            string file = Path.Combine(new string[] { root, "resources", "samples", bpmnFile });

            IProcessDefinitionController client = CreateWorkflowHttpProxy().GetProcessDefinitionClient();

            //foreach (var file in files)
            //{
            Deployment dep = Deploy(file, Path.GetFileNameWithoutExtension(file));

            var defines = await client.LatestProcessDefinitions(new ProcessDefinitionQuery
            {
                DeploymentId = dep.Id,
                TenantId = TenantId
            }).ConfigureAwait(false);

            return defines.List.First();

            //procDefines.Add(Path.GetFileName(file), defines.List.First());
            //}
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                //ILogger logger = unitTestContext.Resolve<ILoggerFactory>().CreateLogger<IntegrationTestContext>();

                //logger.LogError($"{(sender == null ? "" : sender.GetType().FullName)}{ex.Message}");
            }
        }

        private static void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            if (e.Exception != null)
            {
                //ILogger logger = unitTestContext.Resolve<ILoggerFactory>().CreateLogger<IntegrationTestContext>();

                //logger.LogError($"{(sender == null ? "" : sender.GetType().FullName)}{e.Exception.Message}{e.Exception.StackTrace}");
            }
        }

        /// <summary>
        /// 部署流程
        /// </summary>
        /// <param name="bpmnFile"></param>
        /// <param name="bpmnName"></param>
        /// <returns></returns>
        public Deployment Deploy(string bpmnFile, string bpmnName = null)
        {
            IProcessDefinitionDeployerController deployerClient = CreateWorkflowHttpProxy().GetDefinitionDeployerClient();

            string depBpmnName = string.IsNullOrWhiteSpace(bpmnName) ? Path.GetFileNameWithoutExtension(bpmnFile) : bpmnName;

            string xml = string.IsNullOrWhiteSpace(bpmnName) ? ReadBpmn(bpmnFile) :
                ReadBpmn(bpmnFile, bpmnName);

            ProcessDefinitionDeployer deployer = new ProcessDefinitionDeployer
            {
                Name = depBpmnName,
                BpmnXML = xml,
                TenantId = TenantId,
                EnableDuplicateFiltering = false
            };

            Deployment deployment = AsyncHelper.RunSync<Deployment>(() => deployerClient.Deploy(deployer));

            return deployment;
        }

        /// <summary>
        /// 部署流程
        /// </summary>
        /// <param name="bpmnFile"></param>
        /// <param name="bpmnName"></param>
        /// <returns></returns>
        public async Task<Deployment> DeployAsync(string bpmnFile, string bpmnName = null)
        {
            IProcessDefinitionDeployerController deployerClient = CreateWorkflowHttpProxy().GetDefinitionDeployerClient();

            string depBpmnName = string.IsNullOrWhiteSpace(bpmnName) ? Path.GetFileNameWithoutExtension(bpmnFile) : bpmnName;

            string xml = string.IsNullOrWhiteSpace(bpmnName) ? ReadBpmn(bpmnFile) :
                ReadBpmn(bpmnFile, bpmnName);

            ProcessDefinitionDeployer deployer = new ProcessDefinitionDeployer
            {
                Name = depBpmnName,
                BpmnXML = xml,
                TenantId = TenantId,
                EnableDuplicateFiltering = false
            };

            Deployment deployment = await deployerClient.Deploy(deployer).ConfigureAwait(false);

            var list = await deployerClient.Latest(new DeploymentQuery
            {
                Ids = new string[] { deployment.Id },
                TenantId = TenantId
            }).ConfigureAwait(false);

            Assert.NotNull(list);
            Assert.True(list.List.Count() == 1);


            return deployment;
        }

        public ProcessInstance[] Start(string bpmnFile, string[] users, IDictionary<string, object> variables = null)
        {
            IProcessInstanceController client = CreateWorkflowHttpProxy().GetProcessInstanceClient();

            string bpmnName = string.Join("", Guid2IntString(Guid.NewGuid()));
            Deployment dep = Deploy(bpmnFile, bpmnName);

            StartProcessInstanceCmd[] cmds = new StartProcessInstanceCmd[users.Length];
            for (int idx = 0; idx < users.Length; idx++)
            {
                var user = users[idx];

                var vars = new Dictionary<string, object>(variables ?? new Dictionary<string, object>());
                vars.Add("name", new string[] { user });
                StartProcessInstanceCmd cmd = new StartProcessInstanceCmd()
                {
                    ProcessName = bpmnName,
                    Variables = vars,
                    TenantId = TenantId
                };

                cmds[idx] = cmd;
            }

            ProcessInstance[] instances = AsyncHelper.RunSync<ProcessInstance[]>(() =>
            {
                return client.Start(cmds);
            });

            return instances;
        }

        public Task<ProcessInstance[]> StartUseFile(ProcessDefinition process, IDictionary<string, object> variables = null)
        {
            IProcessInstanceController client = CreateWorkflowHttpProxy().GetProcessInstanceClient();

            var vars = new Dictionary<string, object>(variables ?? new Dictionary<string, object>());

            StartProcessInstanceCmd cmd = new StartProcessInstanceCmd()
            {
                ProcessDefinitionId = process.Id,
                Variables = vars,
                TenantId = TenantId
            };

            StartProcessInstanceCmd[] cmds = new StartProcessInstanceCmd[] { cmd };

            return client.Start(cmds);
        }

        public Task<ProcessInstance[]> StartUseFile(string bpmnFile, string[] users, IDictionary<string, object> variables = null, string businessKey = null)
        {
            IProcessInstanceController client = CreateWorkflowHttpProxy().GetProcessInstanceClient();

            string processDefinitionId = GetOrAddProcessDefinition(bpmnFile).Id;

            var vars = new Dictionary<string, object>(variables ?? new Dictionary<string, object>());
            if (!(users is null))
            {
                vars.TryAdd("name", users);
            }

            StartProcessInstanceCmd cmd = new StartProcessInstanceCmd()
            {
                ProcessDefinitionId = processDefinitionId,
                Variables = vars,
                BusinessKey = businessKey,
                TenantId = TenantId
            };

            StartProcessInstanceCmd[] cmds = new StartProcessInstanceCmd[] { cmd };

            return client.Start(cmds);
        }

        public async Task<ProcessInstance[]> StartAsync(string bpmnFile, string[] users, IDictionary<string, object> variables = null, string bpmnName = null)
        {
            IProcessInstanceController client = CreateWorkflowHttpProxy().GetProcessInstanceClient();

            Deployment dep = await DeployAsync(bpmnFile, bpmnName).ConfigureAwait(false);

            StartProcessInstanceCmd[] cmds = new StartProcessInstanceCmd[users.Length];
            for (int idx = 0; idx < users.Length; idx++)
            {
                var user = users[idx];

                var vars = new Dictionary<string, object>(variables ?? new Dictionary<string, object>())
                {
                    { "name", new string[] { user } }
                };
                StartProcessInstanceCmd cmd = new StartProcessInstanceCmd()
                {
                    ProcessName = dep.Name,
                    Variables = vars,
                    TenantId = TenantId
                };

                cmds[idx] = cmd;
            }

            ProcessInstance[] instances = await client.Start(cmds).ConfigureAwait(false);
            return instances;
        }

        public IHttpClientProxy CreateHttpClientProxy(IUserInfo user = null)
        {
            HttpClient httpClient = Resolve<IHttpClientFactory>().CreateClient(); //TestServer.CreateClient();
            httpClient.BaseAddress = new Uri(Configuration.GetValue<string>("BaseUrl"));

            string accessToken = WebUtility.UrlEncode(JsonConvert.SerializeObject(user ?? new UserInfo
            {
                Id = "8a010000-5d88-0015-e013-08d6bd87c815",
                FullName = "新用户1",
                TenantId = TenantId
            }));

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var session = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new
            {
                UserId = "8a010000-5d88-0015-e013-08d6bd87c815",
                FullName = "新用户1",
                TopOrgId = TenantId,
                TenantId = TenantId
            }));

            _httpClient.DefaultRequestHeaders.Add("Evos-Authentication", WebEncoders.Base64UrlEncode(session));

            DefaultHttpContext httpContext = new DefaultHttpContext();

            return new DefaultHttpClientProxy(httpClient,
                this.Resolve<IAccessTokenProvider>(),
                new HttpContextAccessor()
                {
                    HttpContext = httpContext
                });
        }

        private HttpClient _httpClient;

        private readonly object syncHttp = new object();

        public WorkflowHttpClientProxyProvider CreateWorkflowHttpProxy()
        {
            _httpClient = Resolve<IHttpClientFactory>().CreateClient();//TestServer.CreateClient(); 
            _httpClient.BaseAddress = new Uri(Configuration.GetValue<string>("BaseUrl"));

            string accessToken = WebUtility.UrlEncode(JsonConvert.SerializeObject(new
            {
                Id = "8a010000-5d88-0015-e013-08d6bd87c815",
                FullName = "新用户1",
                TenantId
            }));
            if (_httpClient.DefaultRequestHeaders.Any(x => x.Key == "Authorization") == false)
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
            }

            var session = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new
            {
                UserId = "8a010000-5d88-0015-e013-08d6bd87c815",
                FullName = "新用户1",
                TopOrgId = TenantId,
                TenantId
            }));

            _httpClient.DefaultRequestHeaders.Add("Evos-Authentication", WebEncoders.Base64UrlEncode(session));

            IHttpClientProxy clientProxy = Resolve<IHttpClientProxy>();

            clientProxy.HttpClient = _httpClient;

            return new WorkflowHttpClientProxyProvider(clientProxy);
        }
    }

    public class IntegrationTestStartup
    {
        public IConfiguration Configuration { get; }

        public IServiceProvider ServiceProvider { get; private set; }

        public IntegrationTestStartup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IConfiguration>(Configuration)
                .AddHttpContextAccessor()
                .AddLogging(builder =>
                {
                })
                .AddHttpClient("workflow")
                .ConfigureHttpMessageHandlerBuilder(builder =>
                {
                    HttpClientHandler handler = new HttpClientHandler();
                    handler.MaxRequestContentBufferSize = int.MaxValue;
                    handler.ClientCertificateOptions = ClientCertificateOption.Automatic;
                    handler.UseDefaultCredentials = true;
                    handler.UseProxy = false;
                    handler.ServerCertificateCustomValidationCallback += (s, arg1, arg2, arg3) => true;
                    builder.PrimaryHandler = handler;
                    builder.Build();
                });

            services.AddProcessEngine(Configuration);

            services.AddSingleton<IUserServiceProxy, DefaultUserServiceProxy>();

            var sd = services.FirstOrDefault(x => x.ServiceType == typeof(IServiceWebApiHttpProxy));
            if (sd != null)
            {
                services.Remove(sd);
            }
            services.AddTransient<IServiceWebApiHttpProxy>(sp =>
            {
                IHttpClientProxy proxy = IntegrationTestContext.testContext.CreateHttpClientProxy(new UserInfo
                {
                    Id = "8a010000-5d88-0015-e013-08d6bd87c815",
                    FullName = "用户1",
                    TenantId = IntegrationTestContext.testContext.TenantId
                });
                return new ServiceWebApiHttpProxy(proxy, sp.GetService<ILoggerFactory>());
            });

            IMvcBuilder mvcBuilder = services.AddMvc()
                .AddProcessEngineRestServices(Configuration)
                .SetCompatibilityVersion(CompatibilityVersion.Latest);

            mvcBuilder.AddApplicationPart(GetType().Assembly);

            ServiceProvider = services.BuildServiceProvider();

            return ServiceProvider;
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime lifetime)
        {
            app.UseProcessEngine(lifetime);

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

        public object Data { get; set; }

        public Action<HttpResponseMessage> Response { get; set; }

        public HttpMethod Method { get; set; } = HttpMethod.Post;
    }
}
