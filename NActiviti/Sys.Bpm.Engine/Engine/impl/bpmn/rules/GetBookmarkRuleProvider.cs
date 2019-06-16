///////////////////////////////////////////////////////////
//  GetBookmarkRuleProvider.cs
//  Implementation of the Class GetBookmarkRuleProvider
//  Generated by Enterprise Architect
//  Created on:      30-1月-2019 8:32:00
//  Original author: 张楠
///////////////////////////////////////////////////////////

using Sys.Workflow.engine.exceptions;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Sys.Workflow.Engine.Bpmn.Rules
{
    /// <inheritdoc />
    public class GetBookmarkRuleProvider : IGetBookmarkRuleProvider
    {
        private static readonly ConcurrentDictionary<string, Type> types = new ConcurrentDictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        static GetBookmarkRuleProvider()
        {
            Type[] inner = typeof(GetBookmarkRuleProvider)
                .Assembly.GetTypes()
                .Where(x => typeof(IGetBookmarkRule).IsAssignableFrom(x))
                .ToArray();

            foreach (Type type in inner)
            {
                GetBookmarkDescriptorAttribute desc = type.GetCustomAttributes(false)
                    .FirstOrDefault(x => typeof(GetBookmarkDescriptorAttribute) == x.GetType()) as GetBookmarkDescriptorAttribute;

                string name = type.Name;
                if (desc != null)
                {
                    name = desc.Name;
                }

                types.TryAdd(name, type);
            }
        }

        /// <inheritdoc />
        public GetBookmarkRuleProvider()
        {

        }

        /// 
        /// <param name="ruleName">会签角色规则类名称</param>
        public IGetBookmarkRule CreateBookmarkRule(string ruleName)
        {
            if (types.TryGetValue(ruleName, out Type ruleType) == false)
            {
                throw new NotExistsRuleTypeException(ruleName);
            }

            return Activator.CreateInstance(ruleType) as IGetBookmarkRule;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ruleName"></param>
        /// <param name="ruleType"></param>
        public void Register(string ruleName, Type ruleType)
        {
            types.AddOrUpdate(ruleName, ruleType, (rn, oldType) => ruleType);
        }
    }
}