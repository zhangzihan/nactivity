/*
 * Copyright 2018 Alfresco, Inc. and/or its affiliates.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *       http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Sys.Workflow.engine.impl.identity;
using Sys.Workflow;

namespace Sys.Workflow.cloud.services.core
{
    /// <summary>
    /// Wrap Authentication.java so as to be able to mock static methods. May later want to move this to engine level but not necessary now.
    /// </summary>
    public class AuthenticationWrapper //: BaseAuthenticationWrapper
    {
        /// <summary>
        /// 
        /// </summary>
        public virtual IUserInfo AuthenticatedUser
        {
            set
            {
                Authentication.AuthenticatedUser = value;
            }
            get
            {
                return Authentication.AuthenticatedUser;
            }
        }

    }

}