using System;

namespace SampleApi.Models
{
    public class BusinessThing
    {
        public int Id { get; set; } = 0;
        public string Name { get; set; } = "";
        public Guid OwnerUserId { get; set; } = Guid.Empty;

        // It's not possible to reference `ApplicationUser` 
        // as foreign key since it resides in different DbContext
    }
}
