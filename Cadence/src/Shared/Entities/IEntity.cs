using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace Shared.Entities
{
    public interface IEntity;

    public interface IEntity<out TId> : IEntity
    {
        TId Id { get; }
    }
}
