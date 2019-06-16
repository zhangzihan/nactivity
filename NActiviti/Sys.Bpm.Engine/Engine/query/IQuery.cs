using System.Collections.Generic;

/* Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace Sys.Workflow.Engine.Query
{

    /// <summary>
    /// Describes basic methods for querying.
    /// 
    /// 
    /// </summary>
    public interface IQuery<T, U>
    {

        /// <summary>
        /// Order the results ascending on the given property as defined in this class (needs to come after a call to one of the orderByXxxx methods).
        /// </summary>
        T Asc();

        /// <summary>
        /// Order the results descending on the given property as defined in this class (needs to come after a call to one of the orderByXxxx methods).
        /// </summary>
        T Desc();

        /// <summary>
        /// Order the results by the given <seealso cref="IQueryProperty"/> (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>) </summary>
        /// <param name="property"> the {@code QueryProperty} to be used to order the results </param>
        /// <returns> itself </returns>
        T SetOrderBy(IQueryProperty property);

        /// <summary>
        /// Executes the query and returns the number of results </summary>
        long Count();

        /// <summary>
        /// Executes the query and returns the resulting entity or null if no entity matches the query criteria.
        /// </summary>
        /// <exception cref="ActivitiException">
        ///           when the query results in more than one entities. </exception>
        U SingleResult();

        /// <summary>
        /// Executes the query and get a list of entities as the result. </summary>
        IList<U> List();

        /// <summary>
        /// Executes the query and get a list of entities as the result. </summary>
        IList<U> ListPage(int firstResult, int maxResults);
    }

}