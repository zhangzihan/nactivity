using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using org.activiti.bpmn.converter;
using org.activiti.engine;
using org.activiti.engine.impl.identity;
using org.activiti.engine.impl.util;
using org.activiti.engine.repository;
using org.activiti.engine.runtime;
using org.activiti.engine.task;
using Spring.Core;
using Spring.Core.TypeResolution;
using Sys;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Activiti
{

    class Program
    {
        static string file = null;

        static void Main(string[] args)
        {
            //if (args.Length == 1)
            //{
            //    System.Console.WriteLine("输入流程名:");

            //    file = System.Console.ReadLine();
            //}
            //else
            //{
            //    file = args[1];
            //}

            //while(File.Exists(file))
            //{
            //    System.Console.WriteLine($"{file} 流程文件不存在.退出程序按N或重新输入文件.");

            //    if (System.Console.ReadKey().Key == ConsoleKey.N)
            //    {

            //    }
            //}

            //testXmlElement();

            //testXml();

            //testModel();

            testEngine(args);
        }

        static void testXmlElement()
        {
            //自行修改为本地路径
            string file = @"Sys.Bpm.Engine\Resources\db\mapping\entity\Task.xml";

            XmlReader reader = XmlReader.Create(file);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(reader);
            XmlNamespaceManager xmlNsM = new XmlNamespaceManager(xmlDoc.NameTable);
            xmlNsM.AddNamespace("ns", "http://SmartSql.net/schemas/SmartSqlMap.xsd");
            var node = xmlDoc.SelectSingleNode("//ns:Statement[@Id='commonSelectTaskByQueryCriteriaSql']", xmlNsM);
            var elem = XElement.Parse(node.OuterXml, LoadOptions.SetLineInfo);
        }

        static IHost host;

        static void testEngine(string[] args)
        {
            TypeRegistry.RegisterType(typeof(CollectionUtil));

            //test();

            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;

            host = new HostBuilder()
                .ConfigureHostConfiguration(cfg =>
                {
                    cfg.AddEnvironmentVariables();
                    cfg.AddJsonFile("appsettings.json", optional: true);
                    cfg.AddCommandLine(args);
                })
                .ConfigureLogging((hostctx, cfg) =>
                {
                    cfg.Services.AddLogging();
                    DebugLoggerFactoryExtensions.AddDebug(cfg)
                    .AddConsole(opts =>
                    {
                    });

                    //cfg.Services.AddSingleton<ILoggerFactory>(lf);
                })
                .ConfigureAppConfiguration((hostctx, cfg) =>
                {
                    cfg.AddJsonFile($"appsettings.{hostctx.HostingEnvironment.EnvironmentName}.json");
                    cfg.AddJsonFile("activiti.cfg.json");
                    cfg.AddCommandLine(args);
                })
                .ConfigureServices((hostctx, services) =>
                {
                    services.AddSingleton<LogManager>();

                    services.AddLogging();

                    services.AddProcessEngine();
                })
                .UseConsoleLifetime()
                .Build();

            LogManager.Instance = host.Services.GetService<LogManager>();

            //Container = new Container();
            //Container.With((rules) =>
            //{
            //    return rules.With(FactoryMethod.ConstructorWithResolvableArguments);
            //}).With((rules) =>
            //{
            //    return rules.With(propertiesAndFields: (request) =>
            //    {
            //        return (request.ImplementationType ?? request.ServiceType)
            //        .GetTypeInfo()
            //        .DeclaredProperties.Where(x => x.IsInjectable(true))
            //        .Select(PropertyOrFieldServiceInfo.Of);
            //    });
            //});
            //Container.RegisterDelegate<IConfiguration>(sp => host.Services.GetService<IConfiguration>(), Reuse.Singleton);

            var engine = host.Services.GetService<IProcessEngine>();
            Authentication.AuthenticatedUserId = Guid.Empty.ToString();

            deploy(engine);

            start(engine);

            complet(engine);

            //do
            //{
            //    System.Console.WriteLine("输入任务数:");

            //    string tasks = System.Console.ReadLine();

            //    System.Console.WriteLine($"准备运行任务:{tasks}");

            //    var sw = new Stopwatch();
            //    sw.Start();
            //    try
            //    {
            //        //testSimpleProcess();
            //        testVacationRequest(int.Parse(tasks));
            //    }
            //    catch (Exception ex)
            //    {

            //    }
            //    sw.Stop();
            //    System.Console.WriteLine($"执行时间{sw.ElapsedMilliseconds}");
            //    System.Console.WriteLine("退出运行按N键,继续?");

            //    var keyInfo = System.Console.ReadKey();
            //    if (keyInfo.Key == ConsoleKey.N)
            //    {
            //        break;
            //    }
            //} while (true);

            //host.Run();
        }

        static void testXml()
        {
            //自行修改为本地路径
            string file = @"Sys.Bpm.Engine\Resources\db\mapping\entity\Task.xml";

            var doc = XDocument.Load(file, LoadOptions.SetLineInfo);
            var nametable = doc.Root.CreateReader().NameTable;
            XmlNamespaceManager nsm = new XmlNamespaceManager(nametable);
            nsm.AddNamespace("ns", "http://SmartSql.net/schemas/SmartSqlMap.xsd");
            var elem = doc.Root.XPathSelectElement("//ns:Statement[@Id='commonSelectTaskByQueryCriteriaSql']", nsm);

            elem.VisitNodes((o, depth) =>
            {
                Console.WriteLine("{0}{1}{2}{3}",
                    o is IXmlLineInfo li ? $"({li.LineNumber},{li.LinePosition})" : "(NaN, NaN)",
                    "".PadRight(depth * 4),
                    (XmlElementVisitor.GetName(o) + ": ").PadRight(14),
                    XmlElementVisitor.GetLeafValue(o));
            });

            //var node = elem.FirstNode;

            //Action<XElement> visitor = new Action<XElement>((x) =>
            //{
            //    foreach (var node in x.Descendants())
            //    {
            //        visitor(node);
            //    }
            //});
        }

        /// <summary>
        /// 测试模型解析是否正确
        /// </summary>
        static void testModel()
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(), "diagram.bpmn");//VacationRequest.bpmn20.xml";

            BpmnXMLConverter bpm = new BpmnXMLConverter();

            var m = bpm.convertToBpmnModel(file);
        }

        private static void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {

        }

        /// <summary>
        /// 测试简单流程
        /// </summary>
        static void testSimpleProcess()
        {

            var engine = host.Services.GetService<IProcessEngine>();

            try
            {
                IRepositoryService rs = engine.RepositoryService;

                IDeploymentBuilder depb = rs.createDeployment();
                var dep = depb.addClasspathResource(Path.Combine(Directory.GetCurrentDirectory(), "Test.bpmn20.xml"))
                        //.addBpmnModel("test", model)
                        .deploy();

                IRuntimeService runtime = engine.RuntimeService;
                IProcessInstance pi = runtime.startProcessInstanceByKey("Process_1");
            }
            catch
            {

            }
        }

        /// <summary>
        /// 测试稍微复杂点的流程
        /// </summary>
        /// <param name="ts"></param>
        static void testVacationRequest(int ts)
        {
            var engine = host.Services.GetService<IProcessEngine>();
            Authentication.AuthenticatedUserId = Guid.Empty.ToString();

            //deploy(engine);

            int failed = 0;

            List<System.Threading.Tasks.Task> tss = new List<System.Threading.Tasks.Task>(ts);

            for (var idx = 0; idx < ts; idx++)
            {
                tss.Add(System.Threading.Tasks.Task.Factory.StartNew((i) =>
                {
                    try
                    {
                        start(engine, i);

                        ITaskService taskService = engine.TaskService;
                        //IList<ITask> tasks = taskService.createTaskQuery().orderByDueDateNullsLast().asc().listPage(1, 50);
                        IList<ITask> tasks = taskService.createTaskQuery().taskAssignee($"emp{i}").list();
                        //.taskAssignee("Kermit").list();
                        if (tasks.Count == 0)
                        {
                            System.Console.WriteLine($"任务没有开始{i}");
                            return;
                        }
                        System.Console.WriteLine($"任务开始{i}");
                        //foreach (ITaskEntity task in tasks)
                        //{
                        //    Console.WriteLine("Task available: " + task.Name);
                        //}

                        //ITaskInfo t = tasks[0];
                        //Dictionary<string, object> taskV = new Dictionary<string, object>();
                        //taskV.Add("resendRequest", false);
                        //taskV.Add("vacationApproved", false);
                        //taskV.Add("managerMotivation", "We have a tight deadline!");
                        //taskService.complete(t.Id);//, taskV);

                        //System.Console.WriteLine($"Completed {t.Name} {i}");

                        //tasks = taskService.createTaskQuery().taskAssignee($"emp{i}").list();
                        //foreach (ITaskEntity task in tasks)
                        //{
                        //    Console.WriteLine("Task available: " + task.Name);
                        //}
                    }
                    catch
                    {
                        failed = failed + 1;
                    }
                }, idx));
            }

            System.Threading.Tasks.Task.WaitAll(tss.ToArray());

            if (failed > 0)
            {
                System.Console.Write($"Failed {failed}");
            }
        }

        static void start(IProcessEngine engine, object index)
        {
            Dictionary<string, object> variables = new Dictionary<String, Object>();
            variables.Add($"taskIndex", index);
            variables.Add("confirm", true);

            IRuntimeService runtime = engine.RuntimeService;
            IProcessInstance pi = runtime.startProcessInstanceByKey("bpmn-js", variables);

            //var cnt = runtime.createProcessInstanceQuery().count();
            //Console.WriteLine(cnt);
            //Dictionary<string, object> variables = new Dictionary<String, Object>();
            //variables.Add("employeeName", "Kermit");
            //variables.Add("numberOfDays", 4);
            //variables.Add("vacationMotivation", "I'm really tired!");

            //IRuntimeService runtime = engine.RuntimeService;
            //IProcessInstance pi = runtime.startProcessInstanceByKey("vacationRequest", variables);

            //var cnt = runtime.createProcessInstanceQuery().count();
            //Console.WriteLine(cnt);
        }

        static void start(IProcessEngine processEngine)
        {
            Dictionary<String, Object> variables = new Dictionary<String, Object>();
            variables.Add("employeeName", "Kermit");
            variables.Add("numberOfDays",4);
            variables.Add("vacationMotivation", "I'm really tired!");

            IRuntimeService runtime = processEngine.RuntimeService;
            IProcessInstance pi = runtime.startProcessInstanceByKey("vacationRequest", variables);

            long cnt = runtime.createProcessInstanceQuery().count();
            Console.WriteLine(cnt);
        }

        static void complet(IProcessEngine processEngine)
        {
            ITaskService taskService = processEngine.TaskService;
            IList<ITask> tasks = taskService.createTaskQuery()
                    .taskCandidateGroup("management")
                    .list();

            foreach (ITask task in tasks)
            {
                Console.WriteLine("Task available: " + task.Name);
            }

            ITask t = tasks[0];

            Dictionary<String, Object> taskV = new Dictionary<String, Object>();
            taskV.Add("vacationApproved", false);
            taskV.Add("managerMotivation", "We have a tight deadline!");
            taskService.complete(t.Id, taskV);

            tasks = taskService.createTaskQuery().list();
            foreach (ITask task in tasks)
            {
                Console.WriteLine("Task available: " + task.Name);
            }
        }

        static void deploy(IProcessEngine engine)
        {
            IRepositoryService rs = engine.RepositoryService;

            IDeploymentBuilder depb = rs.createDeployment();
            var dep = depb.addClasspathResource(Path.Combine(Directory.GetCurrentDirectory(), "VacationRequest.bpmn20.bpmn"))
                    .deploy();
        }

        static void test()
        {
            var file = @"E:\project\test.xml";

            XmlReader reader = XmlReader.Create(file);

            while (reader.Read())
            {
                try
                {
                    Console.WriteLine($"{Enum.GetName(typeof(XmlNodeType), reader.NodeType)}-{reader.LocalName}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }

    class Startup : IHostedService
    {
        private readonly ILoggerFactory loggerFactory;

        public Startup(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
        }

        public IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();

            return services;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    public static class XmlElementVisitor
    {
        public delegate T XObjectProjectionFunc<T>(object o, int depth);


        public delegate T XElementProjectionFunc<T>(XElement o, int depth);

        public static string GetName(object o)
        {
            return
                o is XElement elem ? elem.Name.LocalName :
                    o is XAttribute attr ? attr.Name.LocalName :
                        o is XNode node ? node.NodeType.ToString() : o?.ToString();
        }

        public static string GetLeafValue(object o)
        {
            XElement ell = o as XElement;
            if (ell != null)
                if (!ell.Elements().Any())
                    return (string)ell;

            XAttribute att = o as XAttribute;
            if (att != null)
                return (string)att;

            if (o is XNode node && !node.CreateNavigator().HasChildren)
            {
                return node.CreateNavigator().Value;
            }

            return "";
        }

        public static IEnumerable<T> Visit<T>(this XElement source, XObjectProjectionFunc<T> func)
        {
            foreach (var v in Visit(source, func, 0))
                yield return v;
        }



        public static IEnumerable<T> Visit<T>(XElement source, XObjectProjectionFunc<T> func, int depth)
        {
            yield return func(source, depth);
            foreach (XAttribute att in source.Attributes())
                yield return func(att, depth + 1);
            foreach (XElement child in source.Elements())
                foreach (T s in Visit(child, func, depth + 1))
                    yield return s;
        }



        public delegate void XObjectVisitor(object o, int depth);



        public static void Visit(this XElement source, XObjectVisitor func)
        {
            Visit(source, func, 0);
        }



        public static void Visit(XElement source, XObjectVisitor func, int depth)
        {
            func(source, depth);
            foreach (XAttribute att in source.Attributes())
                func(att, depth + 1);
            foreach (XElement child in source.Elements())
                Visit(child, func, depth + 1);
        }



        public static IEnumerable<T> VisitElements<T>(this XElement source, XElementProjectionFunc<T> func)
        {
            foreach (var v in VisitElements(source, func, 0))
                yield return v;
        }



        public static IEnumerable<T> VisitElements<T>(XElement source, XElementProjectionFunc<T> func, int depth)
        {
            yield return func(source, depth);
            foreach (XElement child in source.Elements())
                foreach (T s in VisitElements(child, func, depth + 1))
                    yield return s;
        }



        public delegate void XElementVisitor(XElement o, int depth);



        public static void VisitElements(this XElement source, XElementVisitor func)
        {
            VisitElements(source, func, 0);
        }



        public static void VisitElements(XElement source, XElementVisitor func, int depth)
        {
            func(source, depth);
            foreach (XElement child in source.Elements())
                VisitElements(child, func, depth + 1);
        }



        public delegate void XNodeVisitor(XNode o, int depth);

        public static void VisitNodes(this XContainer source, XNodeVisitor func)
        {
            VisitNodes(source, func, 0);
        }

        public static void VisitNodes(XContainer source, XNodeVisitor func, int depth)
        {
            func(source, depth);
            foreach (XNode child in source.Nodes())
            {
                if (child is XElement)
                {
                    VisitNodes(child as XContainer, func, depth + 1);
                }
                else
                {
                    func(child, depth + 1);
                }
            }
        }

    }
}
