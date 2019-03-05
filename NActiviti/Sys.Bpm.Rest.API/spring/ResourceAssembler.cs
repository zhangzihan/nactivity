namespace org.springframework.hateoas
{
    public interface IResourceAssembler<T, D> where D : ResourceSupport
    {

        /**
         * Converts the given entity into an {@link ResourceSupport}.
         * 
         * @param entity
         * @return
         */
        D toResource(T entity);
    }
}
