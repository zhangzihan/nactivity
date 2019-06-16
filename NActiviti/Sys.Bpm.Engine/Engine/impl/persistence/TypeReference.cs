using System;

namespace Sys.Workflow.engine.impl.persistence
{
    public class TypeReference<T>
    {
        private Type rawType;

        protected TypeReference()
        {
            rawType = typeof(T);
        }

        public Type RawType => rawType;

        Type GetSuperclassTypeParameter()
        {
            //            Type clazz = typeof(T);
            //            Type genericSuperclass = clazz.GetGenericTypeDefinition();//.getGenericSuperclass();
            //            if (genericSuperclass is T) {
            //                // try to climb up the hierarchy until meet something useful
            //                if (TypeReference.class != genericSuperclass) {
            //                return getSuperclassTypeParameter(clazz.getSuperclass());
            //    }

            //              throw new TypeException("'" + getClass() + "' extends TypeReference but misses the type parameter. "
            //                + "Remove the extension or add a type parameter to it.");
            //}

            //Type rawType = ((ParameterizedType)genericSuperclass).getActualTypeArguments()[0];
            //            // TODO remove this when Reflector is fixed to return Types
            //            if (rawType instanceof ParameterizedType) {
            //              rawType = ((ParameterizedType) rawType).getRawType();
            //            }

            return rawType;
        }

        public override string ToString()
        {
            return rawType.ToString();
        }
    }
}
