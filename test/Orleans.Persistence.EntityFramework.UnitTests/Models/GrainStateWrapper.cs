namespace Orleans.Persistence.EntityFramework.UnitTests.Models
{
    public class GrainStateWrapper<T>
    {
        public T Value { get; set; }
        
    }
}