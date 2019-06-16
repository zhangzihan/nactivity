namespace Sys.Workflow.Hateoas
{
    /// <summary>
    /// 
    /// </summary>
    public interface IResourceAssembler<T, D> where D : ResourceSupport
    {

        /**
         * Converts the given entity into an {@link ResourceSupport}.
         * 
         * @param entity
         * @return
         */
        D ToResource(T entity);
    }
}
