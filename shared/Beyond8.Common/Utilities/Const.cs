namespace Beyond8.Common.Utilities
{
    public static class Const
    {
        public const string Redis = "redis-cache";
        public const string RabbitMQ = "rabbitmq";
        public const string Qdrant = "qdrant";
        public const string IdentityServiceBaseUrl = "https://localhost:7123";

        public const string IdentityServiceDatabase = "identity-db";
        public const string IntegrationServiceDatabase = "integration-db";
        public const string CatalogServiceDatabase = "catalog-db";
    }

    public static class Role
    {
        public const string Student = "ROLE_STUDENT";
        public const string Instructor = "ROLE_INSTRUCTOR";
        public const string Staff = "ROLE_STAFF";
        public const string Admin = "ROLE_ADMIN";
    }
}