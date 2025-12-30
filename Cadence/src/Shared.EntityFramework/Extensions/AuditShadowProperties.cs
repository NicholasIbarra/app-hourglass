using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.EntityFramework.Extensions;

public static class AuditShadowProperties
{
    public const string CreatedAt = "CreatedAt";
    public const string CreatedBy = "CreatedBy";

    public const string UpdatedAt = "UpdatedAt";
    public const string UpdatedBy = "UpdatedBy";

    public const string IsDeleted = "IsDeleted";
    public const string DeletedAt = "DeletedAt";
    public const string DeletedBy = "DeletedBy";
}

