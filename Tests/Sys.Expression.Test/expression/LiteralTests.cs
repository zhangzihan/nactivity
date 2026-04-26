using Spring.Core.TypeResolution;
using Spring.Expressions;
using Spring.Expressions.Processors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Xunit;
using ExpressionEvaluator = Spring.Expressions.ExpressionEvaluator;

namespace Sys.Workflow.Client.Tests.Expression
{
    /// <summary>
    /// 字面量解析测试：null、布尔、整数、实数、十六进制、字符串
    /// </summary>
    public class LiteralTests
    {
        [Fact]
        public void Null字面量()
        {
            var result = ExpressionEvaluator.GetValue(null, "null");
            Assert.Null(result);
        }

        [Fact]
        public void Boolean字面量_true()
        {
            var result = ExpressionEvaluator.GetValue(null, "true");
            Assert.True((bool)result);
        }

        [Fact]
        public void Boolean字面量_false()
        {
            var result = ExpressionEvaluator.GetValue(null, "false");
            Assert.False((bool)result);
        }

        [Fact]
        public void 整数字面量()
        {
            var result = ExpressionEvaluator.GetValue(null, "42");
            Assert.Equal(42, result);
        }

        [Fact]
        public void 负整数字面量()
        {
            var result = ExpressionEvaluator.GetValue(null, "-10");
            Assert.Equal(-10, result);
        }

        [Fact]
        public void 长整数字面量()
        {
            var result = ExpressionEvaluator.GetValue(null, "9999999999");
            Assert.Equal(9999999999L, Convert.ToInt64(result));
        }

        [Fact]
        public void 实数字面量_小数()
        {
            var result = ExpressionEvaluator.GetValue(null, "3.14");
            Assert.Equal(3.14, (double)result, 4);
        }

        [Fact]
        public void 实数字面量_Float后缀()
        {
            var result = ExpressionEvaluator.GetValue(null, "2.5F");
            Assert.Equal(2.5f, result);
        }

        [Fact]
        public void 实数字面量_Decimal后缀()
        {
            var result = ExpressionEvaluator.GetValue(null, "1.23M");
            Assert.Equal(1.23m, result);
        }

        [Fact]
        public void 实数字面量_Double后缀()
        {
            var result = ExpressionEvaluator.GetValue(null, "2.718D");
            Assert.Equal(2.718d, result);
        }

        [Fact]
        public void 实数字面量_无整数部分()
        {
            var result = ExpressionEvaluator.GetValue(null, ".5");
            Assert.Equal(0.5, (double)result, 2);
        }

        [Fact]
        public void 实数字面量_科学计数法()
        {
            var result = ExpressionEvaluator.GetValue(null, "1e3");
            Assert.Equal(1000.0, (double)result, 0);
        }

        [Fact]
        public void 十六进制字面量()
        {
            var result = ExpressionEvaluator.GetValue(null, "0xFF");
            Assert.Equal(255, result);
        }

        [Fact]
        public void 十六进制字面量_大写()
        {
            var result = ExpressionEvaluator.GetValue(null, "0xABCD");
            Assert.Equal(0xABCD, result);
        }

        [Fact]
        public void 字符串字面量()
        {
            var result = ExpressionEvaluator.GetValue(null, "'hello'");
            Assert.Equal("hello", result);
        }

        [Fact]
        public void 字符串字面量_空字符串()
        {
            var result = ExpressionEvaluator.GetValue(null, "''");
            Assert.Equal("", result);
        }

        [Fact]
        public void 字符串字面量_转义单引号()
        {
            var result = ExpressionEvaluator.GetValue(null, "'it''s ok'");
            Assert.Equal("it's ok", result);
        }

        [Fact]
        public void 字符串字面量_中文()
        {
            var result = ExpressionEvaluator.GetValue(null, "'你好世界'");
            Assert.Equal("你好世界", result);
        }

        [Fact]
        public void 整数运算结果类型()
        {
            var result = ExpressionEvaluator.GetValue(null, "10 + 20");
            Assert.IsType<int>(result);
            Assert.Equal(30, result);
        }
    }
}
