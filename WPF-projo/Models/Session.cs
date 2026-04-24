namespace WPF_projo.Models
{
    /// <summary>
    /// Prosta, statyczna sesja użytkownika — trzyma info o aktualnie zalogowanej osobie.
    /// </summary>
    public static class Session
    {
        public static UserModel? CurrentUser { get; set; }

        public static bool IsAuthenticated => CurrentUser != null;

        public static void Logout() => CurrentUser = null;
    }
}
