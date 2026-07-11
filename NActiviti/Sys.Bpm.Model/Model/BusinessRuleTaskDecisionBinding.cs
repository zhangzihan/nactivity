using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Sys.Workflow.Bpmn.Models
{
    /// <summary>
    /// 定义 BusinessRuleTask 的结构化决策绑定配置。
    /// </summary>
    public sealed class BusinessRuleTaskDecisionBinding
    {
        /// <summary>
        /// 获取或设置决策主键。
        /// </summary>
        public string DecisionKey { get; set; } = string.Empty;

        /// <summary>
        /// 获取或设置决策名称。
        /// </summary>
        public string? DecisionName { get; set; }

        /// <summary>
        /// 获取或设置决策类型。
        /// </summary>
        public string? DecisionType { get; set; }

        /// <summary>
        /// 获取或设置决策绑定模式。
        /// </summary>
        public string? BindingMode { get; set; }

        /// <summary>
        /// 获取或设置决策执行模式。
        /// </summary>
        public string? ExecutionMode { get; set; }

        /// <summary>
        /// 获取或设置是否启用解释追踪。
        /// </summary>
        public bool? TraceEnabled { get; set; }

        /// <summary>
        /// 获取或设置失败回退策略。
        /// </summary>
        public string? FallbackPolicy { get; set; }

        /// <summary>
        /// 获取或设置执行超时时间（毫秒）。
        /// </summary>
        public int? TimeoutMs { get; set; }

        /// <summary>
        /// 获取或设置版本标签。
        /// </summary>
        public string? VersionTag { get; set; }

        /// <summary>
        /// 获取决策输入映射集合。
        /// </summary>
        public IList<BusinessRuleTaskDecisionInputMapping> InputMappings { get; } = new List<BusinessRuleTaskDecisionInputMapping>();

        /// <summary>
        /// 获取决策输出映射集合。
        /// </summary>
        public IList<BusinessRuleTaskDecisionOutputMapping> OutputMappings { get; } = new List<BusinessRuleTaskDecisionOutputMapping>();

        /// <summary>
        /// 获取失败策略扩展选项集合。
        /// </summary>
        public IList<BusinessRuleTaskDecisionOption> Options { get; } = new List<BusinessRuleTaskDecisionOption>();
    }

    /// <summary>
    /// 定义 BusinessRuleTask 到决策输入的字段映射。
    /// </summary>
    public sealed class BusinessRuleTaskDecisionInputMapping
    {
        /// <summary>
        /// 获取或设置决策输入名。
        /// </summary>
        public string InputName { get; set; } = string.Empty;

        /// <summary>
        /// 获取或设置输入来源类型。
        /// </summary>
        public string SourceType { get; set; } = string.Empty;

        /// <summary>
        /// 获取或设置输入来源值。
        /// </summary>
        public string SourceValue { get; set; } = string.Empty;
    }

    /// <summary>
    /// 定义决策输出到流程变量的字段映射。
    /// </summary>
    public sealed class BusinessRuleTaskDecisionOutputMapping
    {
        /// <summary>
        /// 获取或设置决策输出名。
        /// </summary>
        public string OutputName { get; set; } = string.Empty;

        /// <summary>
        /// 获取或设置输出目标类型。
        /// </summary>
        public string TargetType { get; set; } = string.Empty;

        /// <summary>
        /// 获取或设置输出目标值。
        /// </summary>
        public string TargetValue { get; set; } = string.Empty;
    }

    /// <summary>
    /// 定义 BusinessRuleTask 失败策略与扩展语义使用的决策选项。
    /// </summary>
    public sealed class BusinessRuleTaskDecisionOption
    {
        /// <summary>
        /// 获取或设置选项名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 获取或设置选项值。
        /// </summary>
        public string Value { get; set; } = string.Empty;
    }

    /// <summary>
    /// 提供 BusinessRuleTask 结构化决策绑定解析扩展。
    /// </summary>
    public static class BusinessRuleTaskDecisionBindingExtensions
    {
        private const string DecisionBindingElementName = "decisionBinding";
        private const string InputMappingsElementName = "inputMappings";
        private const string InputMappingElementName = "inputMapping";
        private const string OutputMappingsElementName = "outputMappings";
        private const string OutputMappingElementName = "outputMapping";
        private const string DecisionOptionElementName = "decisionOption";
        private const string OptionElementName = "option";
        private const string DecisionKeyAttributeName = "decisionKey";
        private const string DecisionNameAttributeName = "decisionName";
        private const string DecisionTypeAttributeName = "decisionType";
        private const string BindingModeAttributeName = "bindingMode";
        private const string ExecutionModeAttributeName = "executionMode";
        private const string TraceEnabledAttributeName = "traceEnabled";
        private const string FallbackPolicyAttributeName = "fallbackPolicy";
        private const string TimeoutMsAttributeName = "timeoutMs";
        private const string VersionTagAttributeName = "versionTag";
        private const string InputNameAttributeName = "inputName";
        private const string SourceTypeAttributeName = "sourceType";
        private const string SourceValueAttributeName = "sourceValue";
        private const string OutputNameAttributeName = "outputName";
        private const string TargetTypeAttributeName = "targetType";
        private const string TargetValueAttributeName = "targetValue";
        private const string OptionNameAttributeName = "name";
        private const string OptionValueAttributeName = "value";

        /// <summary>
        /// 从扩展元素中解析结构化决策绑定配置。
        /// </summary>
        /// <param name="element">当前 BPMN 元素。</param>
        /// <returns>结构化决策绑定；若不存在则返回空。</returns>
        public static BusinessRuleTaskDecisionBinding? GetDecisionBinding(this BaseElement? element)
        {
            if (element?.ExtensionElements == null ||
                !element.ExtensionElements.TryGetValue(DecisionBindingElementName, out var bindingElements) ||
                bindingElements.Count == 0)
            {
                return null;
            }

            var bindingElement = bindingElements[0];
            var decisionKey = GetAttributeValue(bindingElement, DecisionKeyAttributeName);
            if (string.IsNullOrWhiteSpace(decisionKey))
            {
                return null;
            }

            var binding = new BusinessRuleTaskDecisionBinding
            {
                DecisionKey = decisionKey,
                DecisionName = GetAttributeValue(bindingElement, DecisionNameAttributeName),
                DecisionType = GetAttributeValue(bindingElement, DecisionTypeAttributeName),
                BindingMode = GetAttributeValue(bindingElement, BindingModeAttributeName),
                ExecutionMode = GetAttributeValue(bindingElement, ExecutionModeAttributeName),
                TraceEnabled = GetNullableBooleanAttributeValue(bindingElement, TraceEnabledAttributeName),
                FallbackPolicy = GetAttributeValue(bindingElement, FallbackPolicyAttributeName),
                TimeoutMs = GetNullableInt32AttributeValue(bindingElement, TimeoutMsAttributeName),
                VersionTag = GetAttributeValue(bindingElement, VersionTagAttributeName)
            };

            PopulateInputMappings(binding, bindingElement);
            PopulateOutputMappings(binding, bindingElement);
            PopulateOptions(binding, bindingElement);
            return binding;
        }

        /// <summary>
        /// 填充输入映射集合。
        /// </summary>
        /// <param name="binding">目标绑定对象。</param>
        /// <param name="bindingElement">扩展根节点。</param>
        private static void PopulateInputMappings(BusinessRuleTaskDecisionBinding binding, ExtensionElement bindingElement)
        {
            foreach (var mappingsElement in GetChildElements(bindingElement, InputMappingsElementName))
            {
                foreach (var inputMappingElement in GetChildElements(mappingsElement, InputMappingElementName))
                {
                    var inputName = GetAttributeValue(inputMappingElement, InputNameAttributeName);
                    var sourceType = GetAttributeValue(inputMappingElement, SourceTypeAttributeName);
                    var sourceValue = GetAttributeValue(inputMappingElement, SourceValueAttributeName);

                    if (string.IsNullOrWhiteSpace(inputName) ||
                        string.IsNullOrWhiteSpace(sourceType) ||
                        string.IsNullOrWhiteSpace(sourceValue))
                    {
                        continue;
                    }

                    binding.InputMappings.Add(new BusinessRuleTaskDecisionInputMapping
                    {
                        InputName = inputName,
                        SourceType = sourceType,
                        SourceValue = sourceValue
                    });
                }
            }
        }

        /// <summary>
        /// 填充输出映射集合。
        /// </summary>
        /// <param name="binding">目标绑定对象。</param>
        /// <param name="bindingElement">扩展根节点。</param>
        private static void PopulateOutputMappings(BusinessRuleTaskDecisionBinding binding, ExtensionElement bindingElement)
        {
            foreach (var mappingsElement in GetChildElements(bindingElement, OutputMappingsElementName))
            {
                foreach (var outputMappingElement in GetChildElements(mappingsElement, OutputMappingElementName))
                {
                    var outputName = GetAttributeValue(outputMappingElement, OutputNameAttributeName);
                    var targetType = GetAttributeValue(outputMappingElement, TargetTypeAttributeName);
                    var targetValue = GetAttributeValue(outputMappingElement, TargetValueAttributeName);

                    if (string.IsNullOrWhiteSpace(outputName) ||
                        string.IsNullOrWhiteSpace(targetType))
                    {
                        continue;
                    }

                    binding.OutputMappings.Add(new BusinessRuleTaskDecisionOutputMapping
                    {
                        OutputName = outputName,
                        TargetType = targetType,
                        TargetValue = targetValue ?? string.Empty
                    });
                }
            }
        }

        /// <summary>
        /// 填充失败策略扩展选项集合。
        /// </summary>
        /// <param name="binding">目标绑定对象。</param>
        /// <param name="bindingElement">扩展根节点。</param>
        private static void PopulateOptions(BusinessRuleTaskDecisionBinding binding, ExtensionElement bindingElement)
        {
            foreach (var optionElement in GetDecisionOptionElements(bindingElement))
            {
                var optionName = GetAttributeValue(optionElement, OptionNameAttributeName);
                if (string.IsNullOrWhiteSpace(optionName))
                {
                    continue;
                }

                binding.Options.Add(new BusinessRuleTaskDecisionOption
                {
                    Name = optionName,
                    Value = GetAttributeValue(optionElement, OptionValueAttributeName) ?? string.Empty
                });
            }
        }

        /// <summary>
        /// 读取扩展元素属性值。
        /// </summary>
        /// <param name="element">扩展元素。</param>
        /// <param name="attributeName">属性名。</param>
        /// <returns>属性值。</returns>
        private static string? GetAttributeValue(ExtensionElement element, string attributeName)
        {
            if (!element.Attributes.TryGetValue(attributeName, out var attributes) || attributes.Count == 0)
            {
                return null;
            }

            return attributes.FirstOrDefault()?.Value;
        }

        /// <summary>
        /// 读取可空布尔属性值。
        /// </summary>
        /// <param name="element">扩展元素。</param>
        /// <param name="attributeName">属性名。</param>
        /// <returns>布尔值；若未配置或格式非法则返回空。</returns>
        private static bool? GetNullableBooleanAttributeValue(ExtensionElement element, string attributeName)
        {
            var rawValue = GetAttributeValue(element, attributeName);
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return null;
            }

            if (bool.TryParse(rawValue, out var value))
            {
                return value;
            }

            return null;
        }

        /// <summary>
        /// 读取可空整型属性值。
        /// </summary>
        /// <param name="element">扩展元素。</param>
        /// <param name="attributeName">属性名。</param>
        /// <returns>整数值；若未配置或格式非法则返回空。</returns>
        private static int? GetNullableInt32AttributeValue(ExtensionElement element, string attributeName)
        {
            var rawValue = GetAttributeValue(element, attributeName);
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return null;
            }

            if (int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
            {
                return value;
            }

            return null;
        }

        /// <summary>
        /// 读取指定名称的子扩展元素集合。
        /// </summary>
        /// <param name="element">父扩展元素。</param>
        /// <param name="childName">子元素名。</param>
        /// <returns>子元素集合。</returns>
        private static IEnumerable<ExtensionElement> GetChildElements(ExtensionElement element, string childName)
        {
            if (!element.ChildElements.TryGetValue(childName, out var childElements) || childElements.Count == 0)
            {
                return Array.Empty<ExtensionElement>();
            }

            return childElements;
        }

        /// <summary>
        /// 获取决策选项子元素集合。
        /// </summary>
        /// <param name="element">父扩展元素。</param>
        /// <returns>选项子元素集合。</returns>
        private static IEnumerable<ExtensionElement> GetDecisionOptionElements(ExtensionElement element)
        {
            return GetChildElements(element, OptionElementName)
                .Concat(GetChildElements(element, DecisionOptionElementName));
        }
    }
}
