using Spring.Expressions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using ExpressionEvaluator = Spring.Expressions.ExpressionEvaluator;

namespace Sys.Workflow.Client.Tests.Expression
{
    /// <summary>
    /// 运算符测试：比较运算、逻辑运算、一元运算、位运算、默认值
    /// </summary>
    public class OperatorTests
    {
        // ========== 比较运算符 ==========

        [Theory]
        [InlineData(3, 3)]
        [InlineData("abc", "abc")]
        public void OpEqual_相等(object left, object right)
        {
            var data = new { l = left, r = right };
            Assert.True((bool)ExpressionEvaluator.GetValue(data, "l == r"));
        }

        [Theory]
        [InlineData(3, 4)]
        [InlineData("a", "b")]
        public void OpEqual_不等(object left, object right)
        {
            var data = new { l = left, r = right };
            Assert.False((bool)ExpressionEvaluator.GetValue(data, "l == r"));
        }

        [Fact]
        public void OpNotEqual_不等()
        {
            Assert.True((bool)ExpressionEvaluator.GetValue(new { a = 1, b = 2 }, "a != b"));
        }

        [Fact]
        public void OpNotEqual_相等()
        {
            Assert.False((bool)ExpressionEvaluator.GetValue(new { a = 1, b = 1 }, "a != b"));
        }

        [Fact]
        public void OpGreater()
        {
            Assert.True((bool)ExpressionEvaluator.GetValue(new { a = 5, b = 3 }, "a > b"));
            Assert.False((bool)ExpressionEvaluator.GetValue(new { a = 3, b = 5 }, "a > b"));
            Assert.False((bool)ExpressionEvaluator.GetValue(new { a = 3, b = 3 }, "a > b"));
        }

        [Fact]
        public void OpGreaterOrEqual()
        {
            Assert.True((bool)ExpressionEvaluator.GetValue(new { a = 5, b = 3 }, "a >= b"));
            Assert.True((bool)ExpressionEvaluator.GetValue(new { a = 3, b = 3 }, "a >= b"));
            Assert.False((bool)ExpressionEvaluator.GetValue(new { a = 2, b = 3 }, "a >= b"));
        }

        [Fact]
        public void OpLessOrEqual()
        {
            Assert.True((bool)ExpressionEvaluator.GetValue(new { a = 2, b = 3 }, "a <= b"));
            Assert.True((bool)ExpressionEvaluator.GetValue(new { a = 3, b = 3 }, "a <= b"));
            Assert.False((bool)ExpressionEvaluator.GetValue(new { a = 5, b = 3 }, "a <= b"));
        }

        // ========== 逻辑运算符 ==========

        [Fact]
        public void OpAND_逻辑与()
        {
            Assert.True((bool)ExpressionEvaluator.GetValue(new { a = true, b = true }, "a and b"));
            Assert.False((bool)ExpressionEvaluator.GetValue(new { a = true, b = false }, "a and b"));
            Assert.False((bool)ExpressionEvaluator.GetValue(new { a = false, b = true }, "a and b"));
        }

        [Fact]
        public void OpOR_逻辑或()
        {
            Assert.True((bool)ExpressionEvaluator.GetValue(new { a = false, b = true }, "a or b"));
            Assert.False((bool)ExpressionEvaluator.GetValue(new { a = false, b = false }, "a or b"));
        }

        [Fact]
        public void OpXOR_逻辑异或()
        {
            Assert.True((bool)ExpressionEvaluator.GetValue(new { a = true, b = false }, "a xor b"));
            Assert.False((bool)ExpressionEvaluator.GetValue(new { a = true, b = true }, "a xor b"));
        }

        [Fact]
        public void OpNOT_逻辑非()
        {
            Assert.False((bool)ExpressionEvaluator.GetValue(new { a = true }, "!a"));
            Assert.True((bool)ExpressionEvaluator.GetValue(new { a = false }, "!a"));
        }

        // ========== 一元运算符 ==========

        [Fact]
        public void OpUnaryPlus()
        {
            var result = ExpressionEvaluator.GetValue(new { a = 5 }, "+a");
            Assert.Equal(5, result);
        }

        [Fact]
        public void OpUnaryMinus()
        {
            var result = ExpressionEvaluator.GetValue(new { a = 5 }, "-a");
            Assert.Equal(-5, result);
        }

        // ========== 幂运算 ==========

        [Fact]
        public void OpPower_2的10次方()
        {
            var result = ExpressionEvaluator.GetValue(null, "2 ^ 10");
            Assert.Equal(1024.0, Convert.ToDouble(result), 0);
        }

        // ========== 复合运算 ==========

        [Fact]
        public void 混合算术运算_优先级()
        {
            var result = ExpressionEvaluator.GetValue(null, "2 + 3 * 4");
            Assert.Equal(14, result);
        }

        [Fact]
        public void 混合算术运算_括号()
        {
            var result = ExpressionEvaluator.GetValue(null, "(2 + 3) * 4");
            Assert.Equal(20, result);
        }

        [Fact]
        public void 混合逻辑与比较()
        {
            var data = new { x = 5 };
            Assert.True((bool)ExpressionEvaluator.GetValue(data, "x > 3 and x < 10"));
        }

        [Fact]
        public void 嵌套括号()
        {
            var result = ExpressionEvaluator.GetValue(null, "((1 + 2) * (3 + 4))");
            Assert.Equal(21, result);
        }

        // ========== 默认值运算符 ==========

        [Fact]
        public void Default运算符_左非null()
        {
            var data = new { a = "hello", b = "world" };
            var result = ExpressionEvaluator.GetValue(data, "a ?? b");
            Assert.Equal("hello", result);
        }

        [Fact]
        public void Default运算符_左为null()
        {
            var data = new { a = (string)null, b = "world" };
            var result = ExpressionEvaluator.GetValue(data, "a ?? b");
            Assert.Equal("world", result);
        }

        // ========== 比较运算符 is ==========

        [Fact]
        public void Is运算符_类型判断()
        {
            var data = new { a = "hello" };
            var result = ExpressionEvaluator.GetValue(data, "a is string");
            Assert.True((bool)result);
        }

        [Fact]
        public void Is运算符_类型不匹配()
        {
            var data = new { a = 42 };
            var result = ExpressionEvaluator.GetValue(data, "a is string");
            Assert.False((bool)result);
        }

        // ========== 比较运算符 in ==========

        [Fact]
        public void In运算符()
        {
            var data = new { val = 3, items = new[] { 1, 2, 3, 4, 5 } };
            var result = ExpressionEvaluator.GetValue(data, "val in items");
            Assert.True((bool)result);
        }

        [Fact]
        public void In运算符_不在集合中()
        {
            var data = new { val = 99, items = new[] { 1, 2, 3, 4, 5 } };
            var result = ExpressionEvaluator.GetValue(data, "val in items");
            Assert.False((bool)result);
        }

        // ========== like / matches ==========

        [Fact]
        public void Like运算符_前缀匹配()
        {
            var data = new { name = "hello world" };
            var result = ExpressionEvaluator.GetValue(data, "name like 'hel*'");
            Assert.True((bool)result);
        }

        [Fact]
        public void Like运算符_后缀匹配()
        {
            var data = new { name = "hello world" };
            var result = ExpressionEvaluator.GetValue(data, "name like '*world'");
            Assert.True((bool)result);
        }

        [Fact]
        public void Matches运算符()
        {
            var data = new { email = "test@example.com" };
            var result = ExpressionEvaluator.GetValue(data, "email matches '.+@.+\\..+'");
            Assert.True((bool)result);
        }

        [Fact]
        public void Matches运算符_不匹配()
        {
            var data = new { email = "invalid-email" };
            var result = ExpressionEvaluator.GetValue(data, "email matches '.+@.+\\..+'");
            Assert.False((bool)result);
        }
    }
}
