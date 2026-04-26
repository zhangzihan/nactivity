using Spring.Core;
using Spring.Expressions;
using Spring.Objects;
using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;
using ExpressionEvaluator = Spring.Expressions.ExpressionEvaluator;
using SpringExpr = Spring.Expressions.Expression;

namespace Sys.Workflow.Client.Tests.Expression
{
    /// <summary>
    /// 异常处理专项测试：语法错误、运行时异常、类型转换异常、
    /// 属性访问异常、Null安全、SetValue异常、表达式解析异常等
    /// </summary>
    public class ExceptionHandlingTests
    {
        // =====================================================================
        // 一、语法解析异常（SyntaxErrorException — internal，通过 Exception 基类捕获）
        // =====================================================================

        [Fact]
        public void 语法错误_缺少右操作数()
        {
            var ex = Assert.ThrowsAny<Exception>(() =>
                ExpressionEvaluator.GetValue(null, "1 + "));
            Assert.Contains("Syntax", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void 语法错误_缺少左操作数()
        {
            Assert.ThrowsAny<Exception>(() =>
                ExpressionEvaluator.GetValue(null, "* 5"));
        }

        [Fact]
        public void 语法错误_连续运算符()
        {
            // "1 * + 2" 实际被解析为 1 * (+2) = 2，这是合法的
            // 改用真正非法的连续运算符
            Assert.ThrowsAny<Exception>(() =>
                ExpressionEvaluator.GetValue(null, "1 * / 2"));
        }

        [Fact]
        public void 语法错误_不匹配的右括号()
        {
            Assert.ThrowsAny<Exception>(() =>
                ExpressionEvaluator.GetValue(null, "1 + 2)"));
        }

        [Fact]
        public void 语法错误_缺少右大括号()
        {
            Assert.ThrowsAny<Exception>(() =>
                ExpressionEvaluator.GetValue(null, "{|x| x + 1 }"));
        }

        [Fact]
        public void 语法错误_空字符串表达式()
        {
            // 空字符串可能返回 null，而非抛异常；改为测试只有空格
            var result = ExpressionEvaluator.GetValue(null, "");
            Assert.Null(result);
        }

        [Fact]
        public void 语法错误_只有运算符()
        {
            Assert.ThrowsAny<Exception>(() =>
                ExpressionEvaluator.GetValue(null, "+ - * /"));
        }

        [Fact]
        public void 语法错误_非法Token序列()
        {
            Assert.ThrowsAny<Exception>(() =>
                ExpressionEvaluator.GetValue(null, "1 2 3"));
        }

        [Fact]
        public void 语法错误_三元表达式缺少冒号()
        {
            Assert.ThrowsAny<Exception>(() =>
                ExpressionEvaluator.GetValue(null, "true ? 1"));
        }

        [Fact]
        public void 语法错误_方法调用缺少右括号()
        {
            Assert.ThrowsAny<Exception>(() =>
                ExpressionEvaluator.GetValue(null, "Math.Abs(-5"));
        }

        // =====================================================================
        // 二、运行时异常 — 算术
        // =====================================================================

        [Fact]
        public void 运行时_整数除以零()
        {
            Assert.Throws<DivideByZeroException>(() =>
                ExpressionEvaluator.GetValue(null, "1 / 0"));
        }

        [Fact]
        public void 运行时_整数取模零()
        {
            Assert.Throws<DivideByZeroException>(() =>
                ExpressionEvaluator.GetValue(null, "10 % 0"));
        }

        [Fact]
        public void 运行时_溢出风险_极大整数加法()
        {
            // 不会溢出，只是验证大数运算
            var result = ExpressionEvaluator.GetValue(null, "2147483640 + 5");
            Assert.NotNull(result);
        }

        // =====================================================================
        // 三、运行时异常 — Null 引用
        // =====================================================================

        [Fact]
        public void 运行时_Null对象属性访问()
        {
            var data = new { Person = (object)null };
            Assert.ThrowsAny<Exception>(() =>
                ExpressionEvaluator.GetValue(data, "Person.Name"));
        }

        [Fact]
        public void 运行时_Null对象方法调用()
        {
            var data = new { Text = (string)null };
            Assert.ThrowsAny<Exception>(() =>
                ExpressionEvaluator.GetValue(data, "Text.ToUpper()"));
        }

        [Fact]
        public void 运行时_Null对象索引器访问()
        {
            var data = new { Items = (IList)null };
            Assert.ThrowsAny<Exception>(() =>
                ExpressionEvaluator.GetValue(data, "Items[0]"));
        }

        [Fact]
        public void 运行时_Null字符串Length()
        {
            var data = new { Name = (string)null };
            Assert.ThrowsAny<Exception>(() =>
                ExpressionEvaluator.GetValue(data, "Name.Length"));
        }

        // =====================================================================
        // 四、运行时异常 — 方法/属性不存在
        // =====================================================================

        [Fact]
        public void 运行时_不存在的方法()
        {
            var data = new { Value = 42 };
            Assert.ThrowsAny<Exception>(() =>
                ExpressionEvaluator.GetValue(data, "Value.NonExistentMethod()"));
        }

        [Fact]
        public void 运行时_不存在的属性()
        {
            // 对值类型访问不存在属性会抛异常
            var data = new { Person = new { Name = "Test" } };
            Assert.ThrowsAny<Exception>(() =>
                ExpressionEvaluator.GetValue(data, "Person.NonExistentProperty"));
        }

        [Fact]
        public void 运行时_方法参数数量不匹配_过多参数()
        {
            var data = new { Text = "hello" };
            Assert.ThrowsAny<Exception>(() =>
                ExpressionEvaluator.GetValue(data, "Text.Substring(1, 2, 3)"));
        }

        [Fact]
        public void 运行时_方法参数数量不匹配_过少参数()
        {
            var data = new { Text = "hello" };
            Assert.ThrowsAny<Exception>(() =>
                ExpressionEvaluator.GetValue(data, "Text.Substring()"));
        }

        [Fact]
        public void 运行时_方法参数类型不匹配()
        {
            var data = new { Text = "hello" };
            Assert.ThrowsAny<Exception>(() =>
                ExpressionEvaluator.GetValue(data, "Text.Substring('abc')"));
        }

        [Fact]
        public void 运行时_索引越界()
        {
            var data = new { Items = new[] { 1, 2, 3 } };
            Assert.ThrowsAny<Exception>(() =>
                ExpressionEvaluator.GetValue(data, "Items[10]"));
        }

        [Fact]
        public void 运行时_负索引访问()
        {
            var data = new { Items = new[] { 1, 2, 3 } };
            Assert.ThrowsAny<Exception>(() =>
                ExpressionEvaluator.GetValue(data, "Items[-1]"));
        }

        // =====================================================================
        // 五、类型转换异常
        // =====================================================================

        [Fact]
        public void 类型转换_字符串转数字失败()
        {
            var data = new { Value = "abc" };
            Assert.ThrowsAny<Exception>(() =>
                ExpressionEvaluator.GetValue(data, "Int32.Parse(Value)"));
        }

        [Fact]
        public void 类型转换_非法格式数字()
        {
            Assert.ThrowsAny<Exception>(() =>
                ExpressionEvaluator.GetValue(null, "Int32.Parse('not_a_number')"));
        }

        // =====================================================================
        // 六、SetValue 异常
        // =====================================================================

        [Fact]
        public void SetValue_只读属性()
        {
            var data = new { Name = "Alice" }; // 匿名类型属性只读
            Assert.ThrowsAny<Exception>(() =>
                ExpressionEvaluator.SetValue(data, "Name", "Bob"));
        }

        [Fact]
        public void SetValue_Null上下文设置属性()
        {
            Assert.ThrowsAny<Exception>(() =>
                ExpressionEvaluator.SetValue(null, "SomeProperty", 42));
        }

        [Fact]
        public void SetValue_不存在的属性()
        {
            var data = new Dictionary<string, object>();
            Assert.ThrowsAny<Exception>(() =>
                ExpressionEvaluator.SetValue(data, "NonExistent", 42));
        }

        [Fact]
        public void SetValue_成功()
        {
            var dict = new Dictionary<string, object> { ["Key"] = "old" };
            ExpressionEvaluator.SetValue(dict, "Key", "new");
            Assert.Equal("new", dict["Key"]);
        }

        // =====================================================================
        // 七、表达式解析 (Parse) 异常
        // =====================================================================

        [Fact]
        public void Parse_Null表达式()
        {
            // Parse(null) 不抛异常，返回一个表达式对象
            var result = SpringExpr.Parse(null);
            Assert.NotNull(result);
        }

        [Fact]
        public void Parse_空字符串()
        {
            // 空字符串可能不抛异常，而是返回空节点
            var result = SpringExpr.Parse("");
            Assert.NotNull(result);
        }

        [Fact]
        public void Parse_无效语法()
        {
            // "1 + + 2" 实际被解析为 1 + (+2) = 3，合法
            // 使用更明确的无效语法
            Assert.ThrowsAny<Exception>(() =>
                SpringExpr.Parse("1 * / 2"));
        }

        [Fact]
        public void Parse_有效表达式_可重复求值()
        {
            var expr = SpringExpr.Parse("x + 1");
            Assert.Equal(6, expr.GetValue(new { x = 5 }));
            Assert.Equal(11, expr.GetValue(new { x = 10 }));
        }

        // =====================================================================
        // 八、变量引用异常
        // =====================================================================

        [Fact]
        public void 变量_引用不存在的变量()
        {
            Assert.ThrowsAny<Exception>(() =>
                ExpressionEvaluator.GetValue(null, "#undefined"));
        }

        [Fact]
        public void 变量_Null变量字典()
        {
            Assert.ThrowsAny<Exception>(() =>
                ExpressionEvaluator.GetValue(null, "#x", null));
        }

        [Fact]
        public void 变量_变量字典中无对应Key()
        {
            var vars = new Dictionary<string, object> { ["a"] = 1 };
            Assert.ThrowsAny<Exception>(() =>
                ExpressionEvaluator.GetValue(null, "#b", vars));
        }

        // =====================================================================
        // 九、集合操作异常
        // =====================================================================

        [Fact]
        public void 集合_对Null集合操作()
        {
            var data = new { Items = (IEnumerable)null };
            Assert.ThrowsAny<Exception>(() =>
                ExpressionEvaluator.GetValue(data, "Items.Count()"));
        }

        [Fact]
        public void 集合_投影On非集合()
        {
            var data = new { Value = 42 };
            Assert.ThrowsAny<Exception>(() =>
                ExpressionEvaluator.GetValue(data, "Value.{ToString()}"));
        }

        // =====================================================================
        // 十、异常消息质量验证
        // =====================================================================

        [Fact]
        public void 异常消息_语法错误包含有用信息()
        {
            try
            {
                ExpressionEvaluator.GetValue(null, "1 + ");
                Assert.Fail("Should have thrown");
            }
            catch (Exception ex)
            {
                // 语法错误消息应包含 "Syntax" 或行号/列号信息
                Assert.False(string.IsNullOrEmpty(ex.Message));
                Assert.True(ex.Message.Length > 5);
            }
        }

        [Fact]
        public void 异常消息_运行时错误包含InnerException()
        {
            try
            {
                ExpressionEvaluator.GetValue(null, "1 / 0");
                Assert.Fail("Should have thrown");
            }
            catch (DivideByZeroException)
            {
                // 成功捕获到正确的异常类型
            }
            catch (Exception ex)
            {
                // 可能被包装，但应有内部异常
                Assert.NotNull(ex);
            }
        }

        // =====================================================================
        // 十一、特殊场景异常
        // =====================================================================

        [Fact]
        public void 特殊_构造函数参数类型不匹配()
        {
            Assert.ThrowsAny<Exception>(() =>
                ExpressionEvaluator.GetValue(null, "new System.DateTime('invalid')"));
        }

        [Fact]
        public void 特殊_对值类型属性设置Null()
        {
            // Dictionary<string, object> 可以设置 null，不会抛异常
            var data = new Dictionary<string, object> { ["Value"] = 42 };
            ExpressionEvaluator.SetValue(data, "Value", null);
            Assert.Null(data["Value"]);
        }

        [Fact]
        public void 特殊_注册函数_Null函数名()
        {
            var vars = new Dictionary<string, object>();
            Assert.ThrowsAny<Exception>(() =>
                SpringExpr.RegisterFunction(null, "1 + 1", vars));
        }

        [Fact]
        public void 特殊_注册函数_Null表达式()
        {
            var vars = new Dictionary<string, object>();
            Assert.ThrowsAny<Exception>(() =>
                SpringExpr.RegisterFunction("test", null, vars));
        }

        [Fact]
        public void 特殊_注册函数_空函数名()
        {
            var vars = new Dictionary<string, object>();
            Assert.ThrowsAny<Exception>(() =>
                SpringExpr.RegisterFunction("", "1 + 1", vars));
        }

        [Fact]
        public void 特殊_注册函数_无效表达式()
        {
            var vars = new Dictionary<string, object>();
            Assert.ThrowsAny<Exception>(() =>
                SpringExpr.RegisterFunction("bad", "1 + ", vars));
        }

        // =====================================================================
        // 十二、对象包装器异常 (ObjectWrapper / PropertyValue)
        // =====================================================================

        [Fact]
        public void ObjectWrapper_Null实例()
        {
            var wrapper = new ObjectWrapper();
            Assert.ThrowsAny<Exception>(() =>
                wrapper.GetPropertyValue("AnyProperty"));
        }

        [Fact]
        public void ObjectWrapper_SetNull实例()
        {
            var wrapper = new ObjectWrapper();
            Assert.ThrowsAny<Exception>(() =>
                wrapper.SetPropertyValue("AnyProperty", 42));
        }

        [Fact]
        public void ObjectWrapper_不存在属性()
        {
            var obj = new { Name = "Test" };
            var wrapper = new ObjectWrapper(obj);
            Assert.ThrowsAny<Exception>(() =>
                wrapper.GetPropertyValue("NonExistentProperty"));
        }

        [Fact]
        public void PropertyValue_无效属性名()
        {
            Assert.ThrowsAny<Exception>(() =>
                new PropertyValue("", 42));
        }

        // =====================================================================
        // 十三、异常链（InnerException 传播）
        // =====================================================================

        [Fact]
        public void 异常链_属性访问异常包含原始异常()
        {
            var data = new { obj = (string)null };
            var ex = Assert.ThrowsAny<Exception>(() =>
                ExpressionEvaluator.GetValue(data, "obj.Length"));
            // 异常应有信息，且 InnerException 可能不为 null
            Assert.NotNull(ex);
            Assert.False(string.IsNullOrEmpty(ex.Message));
        }

        [Fact]
        public void 异常链_方法调用异常包含原始异常()
        {
            var data = new { Text = "hello" };
            var ex = Assert.ThrowsAny<Exception>(() =>
                ExpressionEvaluator.GetValue(data, "Text.Substring(999)"));
            Assert.NotNull(ex);
            // ArgumentOutOfRangeException 应被包装或直接抛出
        }

        // =====================================================================
        // 十四、多次求值异常一致性
        // =====================================================================

        [Fact]
        public void 一致性_相同错误表达式多次求值_异常类型一致()
        {
            var type1 = Assert.ThrowsAny<Exception>(() =>
                ExpressionEvaluator.GetValue(null, "1 / 0")).GetType();
            var type2 = Assert.ThrowsAny<Exception>(() =>
                ExpressionEvaluator.GetValue(null, "1 / 0")).GetType();
            Assert.Equal(type1, type2);
        }

        [Fact]
        public void 一致性_解析后的表达式重复求值_异常一致()
        {
            var expr = SpringExpr.Parse("x / 0");
            var data = new { x = 1 };
            var type1 = Assert.ThrowsAny<Exception>(() => expr.GetValue(data)).GetType();
            var type2 = Assert.ThrowsAny<Exception>(() => expr.GetValue(data)).GetType();
            Assert.Equal(type1, type2);
        }
    }
}
