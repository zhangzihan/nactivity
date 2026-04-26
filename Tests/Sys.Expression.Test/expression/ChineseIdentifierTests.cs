using Spring.Expressions;
using System;
using System.Collections.Generic;
using Xunit;
using ExpressionEvaluator = Spring.Expressions.ExpressionEvaluator;

namespace Sys.Workflow.Client.Tests.Expression
{
    /// <summary>
    /// 中文变量命名测试：验证 ID 词法规则对中文标识符的支持
    /// 覆盖：基本汉字、CJK 扩展 A、CJK 兼容汉字、中文属性、中文变量、中文函数参数
    /// </summary>
    public class ChineseIdentifierTests
    {
        // ========== 基本汉字（CJK 统一汉字 \u4E00-\u9FFF） ==========

        [Fact]
        public void 基本汉字_单字属性()
        {
            var data = new { 数 = 42 };
            var result = ExpressionEvaluator.GetValue(data, "数");
            Assert.Equal(42, result);
        }

        [Fact]
        public void 基本汉字_多字属性()
        {
            var data = new { 总金额 = 999.5 };
            var result = ExpressionEvaluator.GetValue(data, "总金额");
            Assert.Equal(999.5, Convert.ToDouble(result), 3);
        }

        [Fact]
        public void 基本汉字_运算()
        {
            var data = new { 单价 = 10, 数量 = 5 };
            var result = ExpressionEvaluator.GetValue(data, "单价 * 数量");
            Assert.Equal(50, Convert.ToInt32(result));
        }

        [Fact]
        public void 基本汉字_比较()
        {
            var data = new { 年龄 = 25 };
            var result = ExpressionEvaluator.GetValue(data, "年龄 > 18");
            Assert.True((bool)result);
        }

        [Fact]
        public void 基本汉字_三元表达式()
        {
            var data = new { 分数 = 85 };
            var result = ExpressionEvaluator.GetValue(data, "分数 >= 60 ? '及格' : '不及格'");
            Assert.Equal("及格", result);
        }

        [Fact]
        public void 基本汉字_嵌套属性()
        {
            var data = new { 订单 = new { 编号 = "ORD001" } };
            var result = ExpressionEvaluator.GetValue(data, "订单.编号");
            Assert.Equal("ORD001", result);
        }

        // ========== 混合命名（中文+英文+数字+下划线） ==========

        [Fact]
        public void 混合命名_中文前缀()
        {
            var data = new { 变量1 = 100 };
            var result = ExpressionEvaluator.GetValue(data, "变量1");
            Assert.Equal(100, result);
        }

        [Fact]
        public void 混合命名_下划线中文()
        {
            var data = new { _计数器 = 7 };
            var result = ExpressionEvaluator.GetValue(data, "_计数器");
            Assert.Equal(7, result);
        }

        [Fact]
        public void 混合命名_英文中文()
        {
            var data = new { total金额 = 200 };
            var result = ExpressionEvaluator.GetValue(data, "total金额");
            Assert.Equal(200, result);
        }

        // ========== CJK 扩展 A 区汉字（\u3400-\u4DBF） ==========

        [Fact]
        public void 扩展A区汉字_属性()
        {
            // \u3447 是 CJK Ext-A 中的汉字 "㑇"
            var data = new Dictionary<string, object> { ["\u3447值"] = 123 };
            var result = ExpressionEvaluator.GetValue(data, "\u3447值");
            Assert.Equal(123, Convert.ToInt32(result));
        }

        // ========== CJK 兼容汉字（\uF900-\uFAFF） ==========

        [Fact]
        public void 兼容汉字_属性()
        {
            // \uFA00 是 CJK 兼容汉字
            var data = new Dictionary<string, object> { ["\uFA00量"] = 456 };
            var result = ExpressionEvaluator.GetValue(data, "\uFA00量");
            Assert.Equal(456, Convert.ToInt32(result));
        }

        // ========== CJK 基本区边缘字符 ==========

        [Fact]
        public void 基本区首字_四E00()
        {
            // \u4E00 是 "一"（CJK 基本区第一个字）
            var data = new Dictionary<string, object> { ["\u4E00"] = 1 };
            var result = ExpressionEvaluator.GetValue(data, "\u4E00");
            Assert.Equal(1, result);
        }

        [Fact]
        public void 基本区尾字_九FFF()
        {
            // \u9FFF 是 CJK 基本区最后一个字（之前旧范围 \u9FA5 不包含此字）
            var data = new Dictionary<string, object> { ["\u9FFF值"] = 789 };
            var result = ExpressionEvaluator.GetValue(data, "\u9FFF值");
            Assert.Equal(789, Convert.ToInt32(result));
        }

        [Fact]
        public void 基本区边缘_九FA6()
        {
            // \u9FA6 在旧范围 (\u9FA5) 之外但在新范围 (\u9FFF) 之内
            var data = new Dictionary<string, object> { ["\u9FA6数据"] = 321 };
            var result = ExpressionEvaluator.GetValue(data, "\u9FA6数据");
            Assert.Equal(321, Convert.ToInt32(result));
        }

        // ========== 中文变量和表达式上下文 ==========

        [Fact]
        public void 中文变量_字典上下文()
        {
            var vars = new Dictionary<string, object>
            {
                ["姓名"] = "张三",
                ["年龄"] = 30
            };
            var result = ExpressionEvaluator.GetValue(null, "#姓名 + ' 年龄:' + #年龄.ToString()", vars);
            Assert.Contains("张三", result?.ToString());
        }

        [Fact]
        public void 中文变量_求值()
        {
            var vars = new Dictionary<string, object>
            {
                ["宽度"] = 10,
                ["高度"] = 20
            };
            var result = ExpressionEvaluator.GetValue(null, "#宽度 * #高度", vars);
            Assert.Equal(200, Convert.ToInt32(result));
        }

        // ========== 中文属性赋值 ==========

        [Fact]
        public void 中文属性_赋值()
        {
            var data = new Dictionary<string, object> { ["状态"] = "初始" };
            ExpressionEvaluator.SetValue(data, "状态", "完成");
            Assert.Equal("完成", data["状态"]);
        }

        // ========== 中文属性集合操作 ==========

        [Fact]
        public void 中文属性_数组Length()
        {
            var data = new Dictionary<string, object>
            {
                ["数据"] = new[] { 1, 2, 3 }
            };
            var result = ExpressionEvaluator.GetValue(data, "数据.Length");
            Assert.Equal(3, Convert.ToInt32(result));
        }

        // ========== 中文方法名 ==========

        [Fact]
        public void 中文属性_调用方法()
        {
            var data = new { 名称 = "Hello" };
            var result = ExpressionEvaluator.GetValue(data, "名称.ToUpper()");
            Assert.Equal("HELLO", result);
        }

        // ========== 字符串中的中文（非标识符） ==========

        [Fact]
        public void 字符串字面量_中文()
        {
            var result = ExpressionEvaluator.GetValue(null, "'你好世界'");
            Assert.Equal("你好世界", result);
        }

        [Fact]
        public void 字符串字面量_中文混合()
        {
            var result = ExpressionEvaluator.GetValue(null, "'价格: 100元'");
            Assert.Equal("价格: 100元", result);
        }

        // ========== 中文标识符在复杂表达式中的使用 ==========

        [Fact]
        public void 复杂表达式_中文()
        {
            var data = new
            {
                基本工资 = 5000,
                绩效系数 = 1.2,
                补贴 = 500
            };
            var result = ExpressionEvaluator.GetValue(data, "基本工资 * 绩效系数 + 补贴");
            Assert.Equal(6500, Convert.ToInt32(result));
        }

        [Fact]
        public void 逻辑表达式_中文()
        {
            var data = new { 是否启用 = true, 优先级 = 3 };
            var result = ExpressionEvaluator.GetValue(data, "是否启用 and 优先级 > 2");
            Assert.True((bool)result);
        }
    }
}
