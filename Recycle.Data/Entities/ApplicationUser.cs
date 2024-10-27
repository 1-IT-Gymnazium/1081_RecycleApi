using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recycle.Data.Entities;

public class ApplicationUser
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public bool IsAdmin { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int DateOfBirth { get; set; }
    public string Email { get; set; }
    public int PhoneNumber { get; set; }
    public DateTime DateOfRegistration { get; set; }
}
