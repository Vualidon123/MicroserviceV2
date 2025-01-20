using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

public class Emp
{
    [BsonId]
    public int ID { get; set; }
    public string PhoneNumber { get; set; }
    public string EmailAddress { get; set; }
    public string Name { get; set; }
    public string Password { get; set; }

    // Navigation property for the many-to-many relationship with Role
    public ICollection<EmpRole> EmpRoles { get; set; }
}

public class EmpRole
{
    // Composite key
    public int EmpId { get; set; }
    public int RoleId { get; set; }

    // Navigation properties
    public virtual Emp Emp { get; set; }
    public virtual Role Role { get; set; }
}

public class Func
{
    [BsonId]
    public int ID { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }

    // Navigation property for the many-to-many relationship with Role
    public virtual ICollection<RoleFunc> RoleFuncs { get; set; }
}

public class Role
{
    [BsonId]
    public int ID { get; set; }
    public string RoleName { get; set; }

    // Add a collection for EmpRoles
    public ICollection<EmpRole> EmpRoles { get; set; }
    public ICollection<RoleFunc> RoleFuncs { get; set; }
}

public class RoleFunc
{
    // Composite key
    public int RoleId { get; set; }
    public int FuncId { get; set; }

    // Navigation properties
    public virtual Role Role { get; set; }
    public virtual Func Func { get; set; }
}
