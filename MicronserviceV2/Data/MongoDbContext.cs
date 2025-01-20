using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

namespace JWT_Authen.Data
{
    public class ApplicationDbContext
    {
        private readonly IMongoDatabase _database;

        public ApplicationDbContext(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<Emp> Emps => _database.GetCollection<Emp>("Emps");
        public IMongoCollection<Role> Roles => _database.GetCollection<Role>("Roles");
        public IMongoCollection<Func> Permissions => _database.GetCollection<Func>("Permissions");
        public IMongoCollection<EmpRole> UserRoles => _database.GetCollection<EmpRole>("UserRoles");
        public IMongoCollection<RoleFunc> RolePermissions => _database.GetCollection<RoleFunc>("RolePermissions");

      /*  public void SeedData()
        {
            // Check if the Emps collection is empty before seeding
            if (!Emps.AsQueryable().Any())
            {
                var emps = new List<Emp>
                {
                    new Emp
                    {
                        ID = 1,
                        Name = "John Doe",
                        EmailAddress = "john@example.com",
                        PhoneNumber = "123-456-7890",
                        Password = "hashed_password_1" // In production, ensure passwords are properly hashed
                    },
                    new Emp
                    {
                        ID = 2,
                        Name = "Jane Smith",
                        EmailAddress = "jane@example.com",
                        PhoneNumber = "123-456-7891",
                        Password = "hashed_password_2"
                    },
                    new Emp
                    {
                        ID = 3,
                        Name = "Bob Wilson",
                        EmailAddress = "bob@example.com",
                        PhoneNumber = "123-456-7892",
                        Password = "hashed_password_3"
                    }
                };
                Emps.InsertMany(emps);
            }

            // Seed Role data
            var roles = new List<Role>
            {
                new Role { ID = 1, RoleName = "Admin" },
                new Role { ID = 2, RoleName = "User" },
                new Role { ID = 3, RoleName = "Manager" }
            };
            Roles.InsertMany(roles);

            // Seed Func (Permission) data
            var funcs = new List<Func>
            {
                new Func { ID = 1, Code = "CREATE", Name = "Create Record" },
                new Func { ID = 2, Code = "READ", Name = "Read Record" },
                new Func { ID = 3, Code = "UPDATE", Name = "Update Record" }
            };
            Permissions.InsertMany(funcs);

            // Seed EmpRole relationships
            var empRoles = new List<EmpRole>
            {
                new EmpRole { EmpId = 1, RoleId = 1 }, // John is Admin
                new EmpRole { EmpId = 2, RoleId = 2 }, // Jane is User
                new EmpRole { EmpId = 3, RoleId = 3 }  // Bob is Manager
            };
            UserRoles.InsertMany(empRoles);

            // Seed RoleFunc relationships
            var roleFuncs = new List<RoleFunc>
            {
                new RoleFunc { RoleId = 1, FuncId = 1 }, // Admin can Create
                new RoleFunc { RoleId = 1, FuncId = 2 }, // Admin can Read
                new RoleFunc { RoleId = 1, FuncId = 3 }  // Admin can Update
            };
            RolePermissions.InsertMany(roleFuncs);
        }*/

        public IMongoCollection<T> GetCollection<T>()
        {
            if (typeof(T) == typeof(Emp))
                return (IMongoCollection<T>)Emps;
            if (typeof(T) == typeof(Role))
                return (IMongoCollection<T>)Roles;
            if (typeof(T) == typeof(Func))
                return (IMongoCollection<T>)Permissions;
            if (typeof(T) == typeof(EmpRole))
                return (IMongoCollection<T>)UserRoles;
            if (typeof(T) == typeof(RoleFunc))
                return (IMongoCollection<T>)RolePermissions;

            throw new ArgumentException("Collection not found for the given type");
        }
    }
}