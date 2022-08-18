using Spring.Expressions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
namespace Sys.Workflow.Engine.Impl.Util
{

    /// <summary>
    /// helper/convenience methods for working with collections.
    /// 
    /// 
    /// </summary>
    public static class CollectionUtil
    {
        //public static void removeAll(this ICollection items, ICollection removeItems)
        //{
        //    foreach (var il in removeItems)
        //    {
        //        var item = items.FirstOrDefault(x => x.Id == il.Id);
        //        if (item is object)
        //        {
        //            executionEntity.IdentityLinks.Remove(item);
        //        }
        //    }
        //}

        /// <summary>
        /// Helper method that creates a singleton map.
        /// 
        /// Alternative for Collections.singletonMap(), since that method returns a generic typed map <K,T> depending on the input type, but we often need a <String, Object> map.
        /// </summary>
        public static IDictionary<string, object> SingletonMap(string key, object value)
        {
            IDictionary<string, object> map = new Dictionary<string, object>
            {
                [key] = value
            };
            return map;
        }

        /// <summary>
        /// Helper method to easily create a map.
        /// 
        /// Takes as input a varargs containing the key1, value1, key2, value2, etc. Note: although an Object, we will cast the key to String internally.
        /// </summary>
        public static IDictionary<string, object> Map(params object[] objects)
        {

            if (objects.Length % 2 != 0)
            {
                throw new ActivitiIllegalArgumentException("The input should always be even since we expect a list of key-value pairs!");
            }

            IDictionary<string, object> map = new Dictionary<string, object>();
            for (int i = 0; i < objects.Length; i += 2)
            {
                map[(string)objects[i]] = objects[i + 1];
            }

            return map;
        }

        public static bool IsEmpty(object @param)
        {
            if (param is not IEnumerable collection)
            {
                return true;
            }

            IEnumerator enumerator = collection.GetEnumerator();
            bool isEmpty = !enumerator.MoveNext();
            if (isEmpty)
            {
                enumerator.Reset();
            }

            return isEmpty;
        }

        public static bool IsNotEmpty(object @param)
        {
            return !IsEmpty(@param);
        }

        public static object ElementAt(IEnumerable list, int index)
        {
            if (index < 0)
            {
                return null;
            }

            var objects = list.Cast<object>().ToList();
            if (index >= objects.Count)
            {
                return null;
            }

            return objects.ElementAt(index);
        }

        public static object First(this IEnumerable list)
        {
            if (list is null)
            {
                return null;
            }

            return list.Cast<object>().FirstOrDefault();
        }

        public static decimal Sum(this IEnumerable list, string expression)
        {
            if (list is null)
            {
                return 0;
            }

            IEnumerable<object> objs = list.Cast<object>();

            return objs.Select(x =>
            {
                if (string.IsNullOrWhiteSpace(expression))
                {
                    decimal.TryParse(x?.ToString(), out var num);
                    return num;
                }
                else
                {
                    var value = ExpressionEvaluator.GetValue(x, expression);

                    if (decimal.TryParse(value?.ToString(), out decimal num))
                    {
                        return num;
                    }

                    return 0;
                }
            }).Sum();
        }

        public static int Count(IEnumerable list, string predicate)
        {
            if (list is null)
            {
                return 0;
            }

            return list.Cast<object>().Where(x =>
            {
                if (string.IsNullOrWhiteSpace(predicate))
                {
                    return true;
                }

                try
                {
                    bool result = Convert.ToBoolean(ExpressionEvaluator.GetValue(x, predicate));
                    return result;
                }
                catch (Exception)
                {
                    return false;
                }
            }).Count();
        }

        public static decimal Avg(this IEnumerable list, string expression)
        {
            if (list is null)
            {
                return 0;
            }

            IEnumerable<object> objs = list.Cast<object>();

            return objs.Select(x =>
            {
                if (string.IsNullOrWhiteSpace(expression))
                {
                    decimal.TryParse(x?.ToString(), out var num);
                    return num;
                }
                else
                {
                    var value = ExpressionEvaluator.GetValue(x, expression);

                    if (decimal.TryParse(value?.ToString(), out decimal num))
                    {
                        return num;
                    }

                    return 0;
                }
            }).Average();
        }

        public static object Max(this IEnumerable list, string expression)
        {
            if (list is null)
            {
                return null;
            }

            return list.Cast<object>().Select(x =>
            {
                if (string.IsNullOrWhiteSpace(expression))
                {
                    return x;
                }

                var value = ExpressionEvaluator.GetValue(x, expression);

                return value;
            }).Max();
        }

        public static object Min(this IEnumerable list, string expression)
        {
            if (list is null)
            {
                return null;
            }

            return list.Cast<object>().Select(x =>
            {
                if (string.IsNullOrWhiteSpace(expression))
                {
                    return x;
                }

                var value = ExpressionEvaluator.GetValue(x, expression);

                return value;
            }).Min();
        }

        public static bool IsZero(IEnumerable<object> list, string property)
        {
            if (list is null)
            {
                return true;
            }

            return Sum(list, property) == 0;
        }

        public static decimal Sub(IEnumerable<object> list, string property)
        {
            if (list is null)
            {
                return 0;
            }

            IEnumerable<object> objs = list.Cast<object>();

            decimal result = 0;
            foreach (var x in objs)
            {
                if (string.IsNullOrWhiteSpace(property))
                {
                    decimal.TryParse(x?.ToString(), out var num);
                    result -= num;
                }
                else
                {
                    var value = ExpressionEvaluator.GetValue(x, property);

                    if (decimal.TryParse(value?.ToString(), out decimal num))
                    {
                        result -= num;
                    }
                }
            };

            return result;
        }

        public static object Last(this IEnumerable list)
        {
            if (list is null)
            {
                return null;
            }

            return list.Cast<object>().LastOrDefault();
        }

        public static IEnumerable Take(this IEnumerable list, int skip, int take)
        {
            if (list is null)
            {
                return null;
            }

            return list.Cast<object>().Skip(skip).Take(take);
        }

        public static IEnumerable Sort(this IEnumerable list, string field)
        {
            if (list is null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(field))
            {
                return list.Cast<object>().OrderBy(x => x);
            }
            else
            {
                return list.Cast<object>().OrderBy(x => ExpressionEvaluator.GetValue(x, field));
            }
        }
    }
}