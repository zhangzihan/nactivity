using Moq;
using Sys.Workflow.Engine.Impl.Util;
using Spring.Core.TypeResolution;
using Spring.Expressions;
using Sys.Workflow.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Sys.Workflow.Services.Api.Commands;
using Newtonsoft.Json.Linq;
using Sys.Workflow.Engine.Api;
using Newtonsoft.Json;
using System.Reflection;
using Spring.Expressions.Processors;
using System.Dynamic;
using System.Runtime.CompilerServices;
using Microsoft.CSharp.RuntimeBinder;
using MSExpr = AdaptiveExpressions;
using Sys.Expressions;
using ExpressionEvaluator = Spring.Expressions.ExpressionEvaluator;
using System.Linq.Expressions;

namespace Sys.Workflow.Client.Tests.Expression
{
    public class ExpressionTest
    {
        private ExpressionTypeRegistry typeRegistry = null;

        public ExpressionTest()
        {
            typeRegistry = new ExpressionTypeRegistry();

            //MSExpr.Expression.Functions.Add("CollectionUtil.SingletonMap", (args) =>
            //{
            //    return CollectionUtil.SingletonMap(args[0]?.ToString(), args[1].value);
            //});

            //LambdaExpression

            //TypeRegistry.RegisterType(typeof(CollectionUtil));
            //TypeRegistry.RegisterType(typeof(ConfigUtil));
            //TypeRegistry.RegisterType(typeof(DateTimeHelper));
            //TypeRegistry.RegisterType(typeof(UrlUtil));
            //TypeRegistry.RegisterType(typeof(Math));
            //TypeRegistry.RegisterType(typeof(String));
            //TypeRegistry.RegisterType(typeof(MathHelper));
            //TypeRegistry.RegisterType(typeof(NumberUtils));
        }

        class ObjectData
        {
            public int ReviewDays { get; set; }
        }

        [Fact]
        public void WorkflowVariable_FromObject_测试()
        {
            var data = new ObjectData { ReviewDays = 10 };

            WorkflowVariable wv = WorkflowVariable.FromObject(data);

            var obj = ExpressionEvaluator.GetValue(wv, "DateTimeHelper.AddDays(ReviewDays)");
        }

        [Fact]
        public void Sum_基础类型()
        {
            int[] data = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            decimal sum = data.Sum();

            var obj = ExpressionEvaluator.GetValue(new { data }, "CollectionUtil.Sum(data, null)");

            Assert.True((decimal)obj == sum);
        }

        [Fact]
        public void Sum_对象字段()
        {
            dynamic[] data = new dynamic[] {
                new { Qty = 1 },
                new { Qty = 2 },
                new { Qty = 3 },
                new { Qty = 4 },
                new { Qty = 5 },
                new { Qty =  6 },
                new { Qty = 7 },
                new { Qty = 8 },
                new { Qty = 9 },
                new { Qty = 10 } };
            decimal sum = data.Sum(x => x.Qty);

            var obj = ExpressionEvaluator.GetValue(new { data }, "CollectionUtil.Sum(data, 'Qty')");

            Assert.True((decimal)obj == sum);
        }

        class Data
        {
            public int Qty { get; set; }
            public decimal? Price { get; set; }
        }

        [Fact]
        public void Sum_对象字段表达式()
        {
            Data[] data = new Data[] {
                new Data { Qty =1, Price = 0.26441427611951451M },
                new Data { Qty =2, Price = 0.67757684536165408M },
                new Data { Qty =3, Price = 0.46424437522154505M },
                new Data { Qty =4, Price = 0.21508280803220478M },
                new Data { Qty =5, Price = 0.5194062308964349M },
                new Data { Qty =6, Price = 0.79699524389439969M },
                new Data { Qty =7, Price = 0.7432121749702898M },
                new Data { Qty =8, Price = 0.86787303763808354M },
                new Data { Qty =9, Price = null },
                new Data { Qty =10, Price = 0.1405017795695466M } };
            decimal sum = data.Sum(x =>
            {
                return x.Price.GetValueOrDefault() * x.Qty;
            });

            var obj = ExpressionEvaluator.GetValue(new { data }, "CollectionUtil.Sum(data, 'isnull(Price,0M) * Qty')");

            Assert.True((decimal)obj == sum);
        }

        [Fact]
        public void Avg_基础类型()
        {
            int[] 同意 = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            decimal avg = 同意.Select(x => decimal.Parse(x.ToString())).Average();

            var obj = ExpressionEvaluator.GetValue(new { 同意 }, "CollectionUtil.Avg(同意, null)");

            Assert.True((decimal)obj == avg);
        }

        [Fact]
        public void Avg_对象字段表达式()
        {
            Data[] data = new Data[] {
                new Data { Qty =1, Price = 0.26441427611951451M },
                new Data { Qty =2, Price = 0.67757684536165408M },
                new Data { Qty =3, Price = 0.46424437522154505M },
                new Data { Qty =4, Price = 0.21508280803220478M },
                new Data { Qty =5, Price = 0.5194062308964349M },
                new Data { Qty =6, Price = 0.79699524389439969M },
                new Data { Qty =7, Price = 0.7432121749702898M },
                new Data { Qty =8, Price = 0.86787303763808354M },
                new Data { Qty =9, Price = null },
                new Data { Qty =10, Price = 0.1405017795695466M } };
            decimal avg = data.Average(x =>
            {
                return x.Price.GetValueOrDefault() * x.Qty;
            });

            var obj = ExpressionEvaluator.GetValue(new { data }, "CollectionUtil.Avg(data, 'isnull(Price,0M) * Qty')");

            Assert.True((decimal)obj == avg);
        }

        [Fact]
        public void Max_基础类型()
        {
            int[] data = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            int max = data.Max();

            var obj = ExpressionEvaluator.GetValue(data, "max()");

            Assert.True(obj.ToString() == max.ToString());
        }

        [Fact]
        public void Max_对象字段表达式()
        {
            Data[] data = new Data[] {
                new Data { Qty =1, Price = 0.26441427611951451M },
                new Data { Qty =2, Price = 0.67757684536165408M },
                new Data { Qty =3, Price = 0.46424437522154505M },
                new Data { Qty =4, Price = 0.21508280803220478M },
                new Data { Qty =5, Price = 0.5194062308964349M },
                new Data { Qty =6, Price = 0.79699524389439969M },
                new Data { Qty =7, Price = 0.7432121749702898M },
                new Data { Qty =8, Price = 0.86787303763808354M },
                new Data { Qty =9, Price = null },
                new Data { Qty =10, Price = 0.1405017795695466M } };
            decimal max = data.Max(x =>
            {
                return x.Price.GetValueOrDefault() * x.Qty;
            });

            var obj = ExpressionEvaluator.GetValue(new { data }, "CollectionUtil.Max(data, 'isnull(Price,0M) * Qty')");

            Assert.True(obj.ToString() == max.ToString());
        }

        [Fact]
        public void Count()
        {
            int[] data = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            int count = data.Where(x => x > 2).Count();

            var obj = ExpressionEvaluator.GetValue(new { data }, "CollectionUtil.Count(data, '{|x| x>2 }')");

            Assert.True((int)obj == count);
        }

        [Fact]
        public void Count_Nullable()
        {
            int?[] data = new int?[] { null, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            int count = data.Where(x => x.GetValueOrDefault() > 2).Count();

            var obj = ExpressionEvaluator.GetValue(data, "CollectionUtil.Count(nonNull(), '{|x| x>2}')");

            Assert.True((int)obj == count);
        }

        [Fact]
        public void Count_对象字段()
        {
            Data[] data = new Data[] {
                new Data { Price = null },
                new Data { Price = 2 },
                new Data { Price = 3 },
                new Data { Price = 4 },
                new Data { Price = 5 },
                new Data { Price = 6 },
                new Data { Price = 7 },
                new Data { Price = 8 },
                new Data { Price = 9 },
                new Data { Price = 10 }
                };
            int count = data.Where(x => x.Price.GetValueOrDefault() > 2).Count();

            var obj = ExpressionEvaluator.GetValue(data, "CollectionUtil.Count(nonNull(), '{|Price| Price>2 }')");

            Assert.True((int)obj == count);
        }

        [Fact]
        public void Count_条件计数()
        {
            int[] data = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            int count = data.Count();

            var obj = ExpressionEvaluator.GetValue(new { data }, "CollectionUtil.Count(data, null)");

            Assert.True((int)obj == count);
        }

        [Fact]
        public void Take()
        {
            int[] data = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            IEnumerable<int> takes = data.Skip(2).Take(2);

            var obj = ExpressionEvaluator.GetValue(new { data }, "CollectionUtil.Take(data, 2, 2)") as IEnumerable<object>;

            Assert.True(takes.Sum() == obj.Select(x => int.Parse(x.ToString())).Sum());
        }

        [Fact]
        public void Min_基础类型()
        {
            int[] data = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            int min = data.Min();

            var obj = ExpressionEvaluator.GetValue(new { data }, "CollectionUtil.Min(data, null)");

            Assert.True(obj.ToString() == min.ToString());
        }

        [Fact]
        public void 对象路径表达式()
        {
            object context = new
            {
                Query = new
                {
                    Id = ""
                }
            };

            var obj = ExpressionEvaluator.GetValue(context, "query.id is object");
        }

        [Fact]
        public void Math_Abs()
        {
            try
            {
                var data = new
                {
                    a = 1,
                    b = 2
                };

                var abs = ExpressionEvaluator.GetValue(data, "Math.Abs(a - b)");
            }
            catch (Exception ex)
            {

            }
        }

        [Fact]
        public void Min_对象字段表达式()
        {
            Data[] data = new Data[] {
                new Data { Qty =1, Price = 0.26441427611951451M },
                new Data { Qty =2, Price = 0.67757684536165408M },
                new Data { Qty =3, Price = 0.46424437522154505M },
                new Data { Qty =4, Price = 0.21508280803220478M },
                new Data { Qty =5, Price = 0.5194062308964349M },
                new Data { Qty =6, Price = 0.79699524389439969M },
                new Data { Qty =7, Price = 0.7432121749702898M },
                new Data { Qty =8, Price = 0.86787303763808354M },
                new Data { Qty =9, Price = null },
                new Data { Qty =10, Price = 0.1405017795695466M } };
            decimal min = data.Min(x =>
            {
                return x.Price.GetValueOrDefault() * x.Qty;
            });

            var obj = ExpressionEvaluator.GetValue(new { data }, "CollectionUtil.Min(data, 'isnull(Price,0M) * Qty')");

            Assert.True(obj.ToString() == min.ToString());
        }

        [Theory]
        [InlineData(1, 2)]
        [InlineData("1", 2)]
        [InlineData(1, "2")]
        public void OpAdd(object left, object right)
        {
            var data = new
            {
                l = left,
                r = right
            };
            var val = ExpressionEvaluator.GetValue(data, "l + r");
            Assert.True(val.ToString() == "3");
        }

        [Theory]
        [InlineData(1, 2.2)]
        [InlineData("1.2", 2)]
        [InlineData(1, "2.2")]
        public void OpAdd_Numeric(object left, object right)
        {
            var data = new
            {
                l = left,
                r = right
            };
            var val = ExpressionEvaluator.GetValue(data, "l + r");
            decimal.TryParse(val.ToString(), out decimal v);
            Assert.True(v == 3.2M);
        }

        [Theory]
        [InlineData(1, 2)]
        [InlineData("1", 2)]
        [InlineData(1, "2")]
        public void OpSUBTRACT(object left, object right)
        {
            var data = new
            {
                l = left,
                r = right
            };
            var val = ExpressionEvaluator.GetValue(data, "l - r");
            Assert.True(val.ToString() == "-1");
        }

        [Theory]
        [InlineData(1.1, 2)]
        [InlineData("1.1", 2)]
        [InlineData(1.1, "2")]
        public void OpSUBTRACT_Numeric(object left, object right)
        {
            var data = new
            {
                l = left,
                r = right
            };
            var val = ExpressionEvaluator.GetValue(data, "l - r");
            decimal.TryParse(val.ToString(), out decimal v);
            Assert.True(v == -0.9M);
        }

        [Theory]
        [InlineData(1, 2)]
        [InlineData("1", 2)]
        [InlineData(1, "2")]
        public void OpMULTIPLY(object left, object right)
        {
            var data = new
            {
                l = left,
                r = right
            };
            var val = ExpressionEvaluator.GetValue(data, "l * r");
            Assert.True(val.ToString() == "2");
        }

        [Theory]
        [InlineData(1.1, 2)]
        [InlineData("1.1", 2)]
        [InlineData(1.1, "2")]
        public void OpMULTIPLY_Numeric(object left, object right)
        {
            var data = new
            {
                l = left,
                r = right
            };
            var val = ExpressionEvaluator.GetValue(data, "l * r");
            decimal.TryParse(val.ToString(), out decimal v);
            Assert.True(v == 2.2M);
        }

        [Theory]
        [InlineData(1, 2)]
        [InlineData("1", 2)]
        [InlineData(1, "2")]
        public void OpDIVIDE(object left, object right)
        {
            var data = new
            {
                l = left,
                r = right
            };
            var val = ExpressionEvaluator.GetValue(data, "l / r");
            Assert.True(val.ToString() == (1D / 2D).ToString());
        }

        [Theory]
        [InlineData(1, 2)]
        [InlineData("1", 2)]
        [InlineData(1, "2")]
        public void OpMODULUS(object left, object right)
        {
            var data = new
            {
                l = left,
                r = right
            };
            var val = ExpressionEvaluator.GetValue(data, "l % r");
            Assert.Equal(1, int.Parse(val.ToString()));
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData("1", 1)]
        [InlineData(1, "1")]
        public void OpEqual(object left, object right)
        {
            var data = new
            {
                l = left,
                r = right
            };
            var val = ExpressionEvaluator.GetValue(data, "l == r");
            Assert.True(bool.Parse(val.ToString()));
        }

        [Theory]
        [InlineData(1, 2)]
        [InlineData("1", 2)]
        [InlineData(1, "2")]
        public void OpNotEqual(object left, object right)
        {
            var data = new
            {
                l = left,
                r = right
            };
            var val = ExpressionEvaluator.GetValue(data, "l != r");
            Assert.True(bool.Parse(val.ToString()));
        }

        [Theory]
        [InlineData(true)]
        [InlineData("true")]
        public void OpNOT(object operand)
        {
            var data = new
            {
                operand,
            };
            var val = ExpressionEvaluator.GetValue(data, "!operand");
            Assert.False(bool.Parse(val.ToString()));
        }

        [Theory]
        [InlineData(1.1, 2)]
        [InlineData("1.1", 2)]
        [InlineData(1.1, "2")]
        public void OpDIVIDE_Numeric(object left, object right)
        {
            var data = new
            {
                l = left,
                r = right
            };
            var val = ExpressionEvaluator.GetValue(data, "l / r");
            decimal.TryParse(val.ToString(), out decimal v);
            Assert.True(v == 1.1M / 2);
        }

        [Theory]
        [InlineData(1.1, 2)]
        [InlineData("1.1", 2)]
        [InlineData(1.1, "2")]
        public void OpPower_Numeric(object left, object right)
        {
            var data = new
            {
                l = left,
                r = right
            };
            var val = ExpressionEvaluator.GetValue(data, "l ^ r");
            Assert.Equal(Math.Pow(1.1D, 2D).ToString(), double.Parse(val.ToString()).ToString());
        }

        [Fact]
        public void TestCondition()
        {
            var expr = "nrOfCompletedInstances/nrOfActiveInstances>0.5 or (test==0)";
            var val = ExpressionEvaluator.GetValue(new
            {
                nrOfCompletedInstances = 3,
                nrOfActiveInstances = 4,
                test = 1
            }, expr);
        }

        [Fact]
        public void OpStrAdd()
        {
            string expr = "'str ' + obj + ' str'";
            var val = ExpressionEvaluator.GetValue(new { obj = 3 }, expr);
        }

        [Fact]
        public void OpAdd_DateTime()
        {
            DateTime now = DateTime.Now;
            var data = new
            {
                l = now,
                r = 1
            };
            var val = ExpressionEvaluator.GetValue(data, "l + r");
            Assert.Equal(now.AddDays(1).ToString(), val.ToString());
        }

        [Fact]
        public void OpSUBSTRACT_DateTime()
        {
            DateTime now = DateTime.Now;
            var data = new
            {
                l = now,
                r = 1
            };
            var val = ExpressionEvaluator.GetValue(data, "l - r");
            Assert.Equal(now.AddDays(-1).ToString(), val.ToString());
        }

        [Theory]
        [InlineData(1.1, 2)]
        [InlineData("1.1", 2)]
        [InlineData(1.1, "2")]
        public void OpLess(object left, object right)
        {
            var data = new
            {
                l = left,
                r = right
            };
            var obj = ExpressionEvaluator.GetValue(data, $"l<r");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Boolean表达式(bool constBoolean)
        {
            var data = new
            {
                nrOfActiveInstances = 1,
            };
            var obj = ExpressionEvaluator.GetValue(data, $"nrOfActiveInstances == 0 or({constBoolean.ToString().ToLower()})");

            Assert.True(constBoolean ? obj.ToString() == "True" : obj.ToString() == "False");
        }

        class IIFMethod : IMethodCallProcessor
        {
            public object Process(object context, object[] args)
            {
                bool b = bool.Parse(args[0]?.ToString());
                return b ? args[1] : args[2];
            }
        }

        class Test : DynamicObject
        {
            // The inner dictionary.
            public Dictionary<string, object> dictionary
                = new Dictionary<string, object>();

            // Getting a property.
            public override bool TryGetMember(
                GetMemberBinder binder, out object result)
            {
                return dictionary.TryGetValue(binder.Name, out result);
            }

            // Setting a property.
            public override bool TrySetMember(
                SetMemberBinder binder, object value)
            {
                dictionary[binder.Name] = value;
                return true;
            }

            public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
            {
                return base.TryInvoke(binder, args, out result);
            }

            // Calling a method.
            public override bool TryInvokeMember(
                InvokeMemberBinder binder, object[] args, out object result)
            {
                if (dictionary.TryGetValue(binder.Name, out result))
                {
                    if (result is MethodInfo method)
                    {
                        if (method.ReturnParameter.Name != "Void")
                        {
                            result = method.Invoke(this, args);
                            return true;
                        }
                        else
                        {
                            result = null;
                            method.Invoke(this, args);
                            return true;
                        }
                    }

                }

                return base.TryInvokeMember(binder, args, out result);

                //return base.TryInvokeMember(binder, args, out result);
                //Type dictType = typeof(Dictionary<string, object>);
                //try
                //{
                //    result = dictType.InvokeMember(
                //                 binder.Name,
                //                 BindingFlags.InvokeMethod,
                //                 null, dictionary, args);
                //    return true;
                //}
                //catch
                //{
                //    result = null;
                //    return false;
                //}
                //result = null;
                //return true;
            }

            // This methods prints out dictionary elements.
            public void Print()
            {
                foreach (var pair in dictionary)
                    Console.WriteLine(pair.Key + " " + pair.Value);
                if (dictionary.Count == 0)
                    Console.WriteLine("No elements in the dictionary.");
            }

            //public object IIF(bool b, object x, object y)
            //{
            //    return b ? x : y;
            //}
        }

        static class Test1
        {
            public static object IIF(bool b, object x, object y)
            {
                return b ? x : y;
            }
        }

        [Fact]
        public void 对象函数表达式()
        {

            var act = new Action(() => { });
            dynamic context = new Test();
            context.Name = "test";
            Assert.Equal("test", context.Name);
            //context.IIF = typeof(Test1).GetMethod("IIF");

            //context.IIF = new Func<bool, object, object, object>((b, arg1, arg2) =>
            //{
            //    return b ? arg1 : arg2;
            //});

            //var methods = context.GetType().GetMethods();
            //int? i = context.IIF(true, 1, 0);
            //Assert.Equal(1, i);

            //var binder = Microsoft.CSharp.RuntimeBinder.Binder.GetMember(
            //    CSharpBinderFlags.None,
            //    "IIF",
            //    context.GetType(),
            //    new[]
            //    {
            //        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
            //    });
            //var callsite = CallSite<Func<CallSite, object, object>>.Create(binder);
            //var res = callsite.Target(callsite, context);
            //i = res.Method.Invoke(res.Target, new object[] { true, 1, 0 });
            //Assert.Equal(1, i);
            //dynamic context = new
            //{
            //    IIF = new Func<bool, object, object, object>((b, arg1, arg2) =>
            //    {
            //        return b ? arg1 : arg2;
            //    })
            //};
            //i = context.IIF(true, 1, 0);
            //Assert.Equal(1, i);

            var str = ExpressionEvaluator.GetValue(context, "IIF(true, '1', '0')");

            Assert.Equal("1", str.ToString());
        }

        [Fact]
        public void 问号表达式()
        {
            var data = new
            {
                同意 = true
            };

            object str = ExpressionEvaluator.GetValue(data, $"同意?'true':'false'");

            Assert.Equal("true", str.ToString());
        }

        [Fact]
        public void TestMethodResolutionWithLargeNumberOfParametersDoesNotThrow()
        {
            int expectedResult = 150;
            int result = 0;

            Foo foo = new Foo();
            string expression = $"MethodWithParamArray({string.Join(", ", Enumerable.Range(0, expectedResult))})";

            var ex = Record.Exception(() =>
            {
                result = (int)ExpressionEvaluator.GetValue(foo, expression);
            });
            Assert.Null(ex);

            Assert.Equal(expectedResult, result);
        }

    }

    internal sealed class Bar
    {
        private int[] numbers = new int[] { 1, 2, 3 };

        public int this[int index]
        {
            get { return numbers[index]; }
        }
    }

    internal class Foo
    {
        private FooType type;
        private Nullable<DateTime> nullableDate;
        private Nullable<Int32> nullableInt;

        public Foo() : this(FooType.One)
        {
        }

        public Foo(FooType type)
        {
            this.type = type;
        }

        public Foo(params string[] values)
        {
        }

        public Foo(bool flag, params string[] values)
        {
        }

        public Foo(int flag, Bar[] bars)
        {
        }

        public Foo(int flag, ICollection bars)
        {
            throw new InvalidOperationException("should have selected ctor(int, Bar[])");
        }

        public string this[Bar[] bars]
        {
            get { return "ExactMatch"; }
        }

        public string this[ICollection bars]
        {
            get { return "AssignableMatch"; }
        }

        public object this[int foo, string key]
        {
            get { return key + "_" + foo; }
        }

        public FooType Type
        {
            get { return type; }
        }

        public DateTime? NullableDate
        {
            get { return nullableDate; }
            set { nullableDate = value; }
        }

        public int? NullableInt
        {
            get { return nullableInt; }
            set { nullableInt = value; }
        }

        public string MethodWithSimilarArguments(int flags, Bar[] bars)
        {
            return "ExactMatch";
        }

        public string MethodWithSimilarArguments(int flags, ICollection bar)
        {
            return "AssignableMatch";
        }

        public string MethodWithArrayArgument(string[] values)
        {
            return string.Join("|", values);
        }

        public string MethodWithParamArray(params string[] values)
        {
            return string.Join("|", values);
        }

        public string MethodWithParamArray(bool uppercase, params string[] values)
        {
            string ret = string.Join("|", values);
            return (uppercase ? ret.ToUpper() : ret);
        }

        public int MethodWithParamArray(params int[] values)
        {
            return values.Length;
        }
    }

    internal enum FooType
    {
        One,
        Two,
        Three
    }
}
