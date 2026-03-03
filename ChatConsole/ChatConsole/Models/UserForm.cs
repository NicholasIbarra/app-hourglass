using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatConsole.Models;

public record UserForm
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public DateTime? DateOfBirth { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? Notes { get; init; }
}
