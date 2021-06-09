using System;
using System.Collections.Generic;

namespace Sys.Workflow.Cloud.Services.Core
{
    public class SecurityPoliciesApplicationService : BaseSecurityPoliciesApplicationService
    {
        private SecurityPoliciesService securityPoliciesService;
        
        private SecurityPoliciesProcessDefinitionRestrictionApplier processDefinitionRestrictionApplier;
        
        private SecurityPoliciesProcessInstanceRestrictionApplier processInstanceRestrictionApplier;
        
        private RuntimeBundleProperties runtimeBundleProperties;

        public virtual ProcessDefinitionQuery restrictProcessDefQuery(ProcessDefinitionQuery query, SecurityPolicy securityPolicy)
        {

            return restrictQuery(query, processDefinitionRestrictionApplier, securityPolicy);
        }

        private ISet<string> definitionKeysAllowedForRBPolicy(SecurityPolicy securityPolicy)
        {
            IDictionary<string, ISet<string>> restrictions = definitionKeysAllowedForPolicy(securityPolicy);
            ISet<string> keys = new HashSet<string>();

            foreach (string appName in restrictions.Keys)
            {
                //only take policies for this app
                //or if we don't know our own appName (just being defensive) then include everything
                //ignore hyphens and case due to values getting set via env vars
                if ((runtimeBundleProperties is null || string.ReferenceEquals(runtimeBundleProperties.ServiceName, null)) || (!string.ReferenceEquals(appName, null) && appName.Replace("-", "").Equals(runtimeBundleProperties.ServiceName.Replace("-", ""), StringComparison.CurrentCultureIgnoreCase)) || (!string.ReferenceEquals(runtimeBundleProperties.ServiceFullName, null) && !string.ReferenceEquals(appName, null) && appName.Replace("-", "").Equals(runtimeBundleProperties.ServiceFullName.Replace("-", ""), StringComparison.CurrentCultureIgnoreCase)))
                {
                    keys.addAll(restrictions[appName]);
                }
            }
            return keys;
        }


        public virtual ProcessInstanceQuery restrictProcessInstQuery(ProcessInstanceQuery query, SecurityPolicy securityPolicy)
        {
            return restrictQuery(query, processInstanceRestrictionApplier, securityPolicy);
        }

        private T restrictQuery<T>(T query, SecurityPoliciesRestrictionApplier<T> restrictionApplier, SecurityPolicy securityPolicy)
        {
            if (noSecurityPoliciesOrNoUser())
            {
                return query;
            }

            ISet<string> keys = definitionKeysAllowedForRBPolicy(securityPolicy);

            if (keys is object && keys.Count > 0)
            {

                if (keys.Contains(securityPoliciesService.Wildcard))
                {
                    return query;
                }

                return restrictionApplier.restrictToKeys(query, keys);
            }

            //policies are in place but if we've got here then none for this user
            if (keys is object && securityPoliciesService.policiesDefined())
            {
                restrictionApplier.denyAll(query);
            }

            return query;
        }

        public virtual bool canWrite(string processDefId)
        {
            return hasPermission(processDefId, SecurityPolicy.WRITE, runtimeBundleProperties.ServiceName) || hasPermission(processDefId, SecurityPolicy.WRITE, runtimeBundleProperties.ServiceFullName);
        }

        public virtual bool canRead(string processDefId)
        {
            return hasPermission(processDefId, SecurityPolicy.READ, runtimeBundleProperties.ServiceName) || hasPermission(processDefId, SecurityPolicy.WRITE, runtimeBundleProperties.ServiceFullName);
        }


    }

}