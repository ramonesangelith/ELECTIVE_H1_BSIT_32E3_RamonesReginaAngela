namespace ramieChristmaseu
{
    public static class ApiError
    {
        public static object BadRequest(string message, params string[] details) =>
            new
            {
                error = "BadRequest",
                message,
                details
            };

        public static object NotFound(string message, params string[] details) =>
            new
            {
                error = "NotFound",
                message,
                details
            };
    }

}
