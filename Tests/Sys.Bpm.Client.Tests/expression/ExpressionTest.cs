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
using Sys.Expressions;
using Newtonsoft.Json.Linq;
using Sys.Workflow.Engine.Api;

namespace Sys.Workflow.Client.Tests.Expression
{
    public class ExpressionTest
    {
        private ExpressionTypeRegistry typeRegistry = null;

        public ExpressionTest()
        {
            typeRegistry = new ExpressionTypeRegistry();
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

            var obj = ExpressionEvaluator.GetValue(context, "query.id != null");
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
    }
}
