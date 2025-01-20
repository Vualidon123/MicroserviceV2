namespace AuthenService.Request
{
    public class CacheRequest
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public List<string> Functions { get; set; }
    }    

        public class RoleWithFunctions
        {
            public int Id { get; set; }
            public string RoleName { get; set; }
            public List<RoleFunction> Functions { get; set; }
        }

        public class RoleFunction
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }
    }

