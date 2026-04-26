using Spring.Core.TypeResolution;
using Spring.Expressions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using ExpressionEvaluator = Spring.Expressions.ExpressionEvaluator;
using SpringExpr = Spring.Expressions.Expression;

namespace Sys.Workflow.Client.Tests.Expression
{
    /// <summary>
    /// 高级特性测试：类型访问、构造函数、索引器、投影、选择、赋值、lambda、变量
    /// </summary>
    public class AdvancedFeatureTests
    {
        class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public List<string> Tags { get; set; }
        }

        public AdvancedFeatureTests()
        {
            TypeRegistry.RegisterType(typeof(Math));
            TypeRegistry.RegisterType(typeof(string));
        }

        // ========== 属性/字段访问 ==========

        [Fact]
        public void 属性访问()
        {
            var data = new { Name = "Alice" };
            Assert.Equal("Alice", ExpressionEvaluator.GetValue(data, "Name"));
        }

        [Fact]
        public void 嵌套属性访问()
        {
            var data = new { User = new { Address = new { City = "Beijing" } } };
            Assert.Equal("Beijing", ExpressionEvaluator.GetValue(data, "User.Address.City"));
        }

        [Fact]
        public void 方法调用()
        {
            var data = new { Text = "hello world" };
            var result = (string)ExpressionEvaluator.GetValue(data, "Text.ToUpper()");
            Assert.Equal("HELLO WORLD", result);
        }

        [Fact]
        public void 方法调用_带参数()
        {
            var data = new { Text = "hello world" };
            var result = (string)ExpressionEvaluator.GetValue(data, "Text.Substring(0, 5)");
            Assert.Equal("hello", result);
        }

        // ========== 类型访问（T() 语法需 TypeRegistry，此处测试已注册的短名） ==========

        [Fact]
        public void 类型访问_Math_Abs()
        {
            // 使用 Math 类 - 已在全局 TypeRegistry 中注册
            var data = new { a = -5 };
            var result = ExpressionEvaluator.GetValue(data, "Math.Abs(a)");
            Assert.Equal(5, Convert.ToInt32(result));
        }

        // ========== 构造函数 ==========

        [Fact]
        public void 构造函数_数组创建()
        {
            var result = ExpressionEvaluator.GetValue(null, "new int[3]");
            Assert.NotNull(result);
            Assert.True(result is Array);
            Assert.Equal(3, ((Array)result).Length);
        }

        // ========== 数组构造 ==========

        [Fact]
        public void 数组构造_一维()
        {
            var result = ExpressionEvaluator.GetValue(null, "new int[5]");
            Assert.NotNull(result);
            Assert.True(result is int[]);
            Assert.Equal(5, ((int[])result).Length);
        }

        [Fact]
        public void 列表初始化器()
        {
            var result = ExpressionEvaluator.GetValue(null, "{1, 2, 3}");
            Assert.NotNull(result);
            var list = result as IList;
            Assert.NotNull(list);
            Assert.Equal(3, list.Count);
        }

        // ========== Map初始化器 ==========

        [Fact]
        public void Map初始化器()
        {
            var result = ExpressionEvaluator.GetValue(null, "#{'a': 1, 'b': 2}");
            Assert.NotNull(result);
            Assert.True(result is IDictionary);
            var dict = (IDictionary)result;
            Assert.Equal(2, dict.Count);
            Assert.Equal(1, dict["a"]);
            Assert.Equal(2, dict["b"]);
        }

        // ========== 索引器 ==========

        [Fact]
        public void 索引器_数组()
        {
            var data = new { arr = new[] { 10, 20, 30 } };
            Assert.Equal(20, ExpressionEvaluator.GetValue(data, "arr[1]"));
        }

        [Fact]
        public void 索引器_字典()
        {
            var dict = new Dictionary<string, object> { ["key1"] = "value1", ["key2"] = 42 };
            Assert.Equal("value1", ExpressionEvaluator.GetValue(dict, "['key1']"));
        }

        [Fact]
        public void 索引器_List()
        {
            var data = new { list = new List<int> { 100, 200, 300 } };
            Assert.Equal(300, ExpressionEvaluator.GetValue(data, "list[2]"));
        }

        // ========== 三元表达式 ==========

        [Fact]
        public void 三元表达式_真()
        {
            var data = new { flag = true };
            Assert.Equal("yes", ExpressionEvaluator.GetValue(data, "flag ? 'yes' : 'no'"));
        }

        [Fact]
        public void 三元表达式_假()
        {
            var data = new { flag = false };
            Assert.Equal("no", ExpressionEvaluator.GetValue(data, "flag ? 'yes' : 'no'"));
        }

        [Fact]
        public void 三元表达式_嵌套()
        {
            var data = new { x = 5 };
            var result = ExpressionEvaluator.GetValue(data, "x > 10 ? 'big' : x > 3 ? 'medium' : 'small'");
            Assert.Equal("medium", result);
        }

        // ========== 赋值表达式 ==========

        [Fact]
        public void 赋值表达式_属性()
        {
            var person = new Person { Name = "old" };
            ExpressionEvaluator.SetValue(person, "Name", "new");
            Assert.Equal("new", person.Name);
        }

        // ========== 投影 ==========

        [Fact]
        public void 投影_获取所有Name()
        {
            var data = new
            {
                people = new[]
                {
                    new { Name = "Alice" },
                    new { Name = "Bob" },
                    new { Name = "Charlie" }
                }
            };
            var result = ExpressionEvaluator.GetValue(data, "people.!{Name}") as IList;
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal("Alice", result[0]);
            Assert.Equal("Bob", result[1]);
            Assert.Equal("Charlie", result[2]);
        }

        // ========== 选择 ==========

        [Fact]
        public void 选择_过滤()
        {
            var data = new
            {
                people = new[]
                {
                    new { Name = "Alice", Age = 25 },
                    new { Name = "Bob", Age = 17 },
                    new { Name = "Charlie", Age = 30 }
                }
            };
            var result = ExpressionEvaluator.GetValue(data, "people.?{Age > 18}") as IList;
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void 选择_首个()
        {
            var data = new
            {
                people = new[]
                {
                    new { Name = "Alice", Age = 25 },
                    new { Name = "Bob", Age = 17 },
                    new { Name = "Charlie", Age = 30 }
                }
            };
            var result = ExpressionEvaluator.GetValue(data, "people.^{Age > 18}");
            Assert.NotNull(result);
        }

        [Fact]
        public void 选择_末个()
        {
            var data = new
            {
                people = new[]
                {
                    new { Name = "Alice", Age = 25 },
                    new { Name = "Bob", Age = 17 },
                    new { Name = "Charlie", Age = 30 }
                }
            };
            var result = ExpressionEvaluator.GetValue(data, "people.${Age > 18}");
            Assert.NotNull(result);
        }

        // ========== 变量 ==========

        [Fact]
        public void 变量引用()
        {
            var variables = new Dictionary<string, object> { { "myVar", 42 } };
            var result = ExpressionEvaluator.GetValue(null, "#myVar", variables);
            Assert.Equal(42, result);
        }

        [Fact]
        public void 变量赋值和引用()
        {
            var variables = new Dictionary<string, object>();
            ExpressionEvaluator.GetValue(null, "#myVar = 100", variables);
            Assert.Equal(100, variables["myVar"]);
        }

        // ========== Lambda ==========

        [Fact]
        public void Lambda_单参数()
        {
            var data = new { items = new[] { 1, 2, 3, 4, 5 } };
            var result = ExpressionEvaluator.GetValue(data, "CollectionUtil.Count(items, '{|x| x > 3 }')");
            Assert.Equal(2, result);
        }

        // ========== 注册函数 ==========

        [Fact]
        public void 注册函数()
        {
            var variables = new Dictionary<string, object>();
            SpringExpr.RegisterFunction("addOne", "{|x| x + 1 }", variables);
            Assert.NotNull(variables["addOne"]);
        }

        // ========== 字符串拼接 ==========

        [Fact]
        public void 字符串拼接()
        {
            var data = new { a = "hello", b = "world" };
            var result = ExpressionEvaluator.GetValue(data, "a + ' ' + b");
            Assert.Equal("hello world", result);
        }

        [Fact]
        public void 字符串与数字拼接()
        {
            var data = new { n = 42 };
            var result = ExpressionEvaluator.GetValue(data, "'answer: ' + n");
            Assert.Equal("answer: 42", result);
        }

        // ========== null 比较 ==========

        [Fact]
        public void Null比较()
        {
            var data = new { a = (string)null };
            Assert.True((bool)ExpressionEvaluator.GetValue(data, "a == null"));
            Assert.False((bool)ExpressionEvaluator.GetValue(data, "a != null"));
        }

        // ========== 设置属性值 ==========

        [Fact]
        public void 设置属性值()
        {
            var person = new Person { Name = "Alice", Age = 20 };
            ExpressionEvaluator.SetValue(person, "Age", 25);
            Assert.Equal(25, person.Age);
        }

        // ========== 集合投影_运算 ==========

        [Fact]
        public void 投影_运算表达式()
        {
            var data = new { items = new[] { 1, 2, 3 } };
            var result = ExpressionEvaluator.GetValue(data, "items.!{#this * 2}") as IList;
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
        }

        // ========== 集合选择_条件 ==========

        [Fact]
        public void 选择_条件过滤()
        {
            var data = new { items = new[] { 1, 2, 3, 4, 5 } };
            var result = ExpressionEvaluator.GetValue(data, "items.?{#this > 3}") as IList;
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }
    }
}
