namespace Mango.Services.OrderAPI.Utility
{
    public static class SD
    {
        public enum Role
        {
            ADMIN = 1,
            CUSTOMER = 2
        }

        public enum OrderStatus
        {
            PENDING = 1,
            APPROVED = 2,
            READYFORPICUP = 3,
            COMPLETED = 4,
            REFUNDED = 5,
            CANCELLED = 6,
        }
    }
}
