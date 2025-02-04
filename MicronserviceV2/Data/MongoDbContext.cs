
using MongoDB.Bson;
using MongoDB.Driver;

namespace JWT_Authen.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<Emp> Emps => _database.GetCollection<Emp>("Emps");
        public IMongoCollection<Role> Roles => _database.GetCollection<Role>("Roles");
        public IMongoCollection<Func> Permissions => _database.GetCollection<Func>("Permissions");
        public IMongoCollection<EmpRole> UserRoles => _database.GetCollection<EmpRole>("UserRoles");
        public IMongoCollection<RoleFunc> RolePermissions => _database.GetCollection<RoleFunc>("RolePermissions");

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
        public void SeedData()
        {
            // Seed Emp data
            if (!Emps.Find(_ => true).Any())
            {
                var johnId = ObjectId.GenerateNewId();
                var janeId = ObjectId.GenerateNewId();
                var bobId = ObjectId.GenerateNewId();
                Emps.InsertMany(new[]
                {
            new Emp
            {
                ObjectId= johnId,
                ID = 1,
                Name = "John Doe",
                EmailAddress = "john@example.com",
                PhoneNumber = "123-456-7890",
                Password = "hashed_password_1" // In production, ensure passwords are properly hashed
            },
            new Emp
            {   
                ObjectId= janeId,
                ID = 2,
                Name = "Jane Smith",
                EmailAddress = "jane@example.com",
                PhoneNumber = "123-456-7891",
                Password = "hashed_password_2"
            },
            new Emp
            {   
                ObjectId= bobId,
                ID = 3,
                Name = "Bob Wilson",
                EmailAddress = "bob@example.com",
                PhoneNumber = "123-456-7892",
                Password = "hashed_password_3"
            }
        });
            }

            // Seed Role data
            if (!Roles.Find(_ => true).Any())
            {
                Roles.InsertMany(new[]
                {
            new Role { ID = 1, RoleName = "Admin" },
            new Role { ID = 2, RoleName = "User" },
            new Role { ID = 3, RoleName = "Manager" }
        });
            }

            // Seed Func (Permission) data
            if (!Permissions.Find(_ => true).Any())
            {
                Permissions.InsertMany(new[]
                {
            new Func { ID = 1, Code = "CREATE", Name = "Create Record" },
            new Func { ID = 2, Code = "READ", Name = "Read Record" },
            new Func { ID = 3, Code = "UPDATE", Name = "Update Record" }
        });
            }

            // Seed EmpRole relationships
            if (!UserRoles.Find(_ => true).Any())
            {
                UserRoles.InsertMany(new[]
                {
            new EmpRole { ID = 1, EmpId = 1, RoleId = 1 }, // John is Admin
            new EmpRole { ID = 2, EmpId = 2, RoleId = 2 }, // Jane is User
            new EmpRole { ID = 3, EmpId = 3, RoleId = 3 }  // Bob is Manager
        });
            }

            // Seed RoleFunc relationships
            if (!RolePermissions.Find(_ => true).Any())
            {
                RolePermissions.InsertMany(new[]
                {
            new RoleFunc { ID = 1, RoleId = 1, FuncId = 1 }, // Admin can Create
            new RoleFunc { ID = 2, RoleId = 1, FuncId = 2 }, // Admin can Read
            new RoleFunc { ID = 3, RoleId = 1, FuncId = 3 }  // Admin can Update
        });
            }
        }
    }
}
