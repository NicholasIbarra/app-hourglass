using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Entities
{
    public abstract class BaseEntity<T>
    {
        public T Id { get; set; }
    }

    public abstract class BaseEntity : BaseEntity<Guid>
    {
    }
}
