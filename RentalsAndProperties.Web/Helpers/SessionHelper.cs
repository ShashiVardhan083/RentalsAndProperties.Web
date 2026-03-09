using System.Text.Json;

namespace RentalsAndProperties.Web.Helpers
{
    /// Centralized session management for auth state.
    public static class SessionHelper
    {
        private const string TokenKey = "JwtToken";
        private const string FullNameKey = "FullName";
        private const string PhoneKey = "PhoneNumber";
        private const string EmailKey = "Email";
        private const string RolesKey = "UserRoles";
        private const string ExpiryKey = "TokenExpiry";

        public static void SetAuthSession(
            ISession session,
            string token,
            string fullName,
            string phoneNumber,
            string? email,
            IEnumerable<string> roles,
            DateTime expiresAt)
        {
            session.SetString(TokenKey, token);
            session.SetString(FullNameKey, fullName);
            session.SetString(PhoneKey, phoneNumber);
            session.SetString(EmailKey, email ?? string.Empty);
            session.SetString(RolesKey, JsonSerializer.Serialize(roles));
            session.SetString(ExpiryKey, expiresAt.ToString("O")); //Round-trip ISO 8601 format. - Eg: 2026-03-09T10:15:30.0000000Z
        }

        public static void ClearAuthSession(ISession session)
        {
            session.Remove(TokenKey);
            session.Remove(FullNameKey);
            session.Remove(PhoneKey);
            session.Remove(EmailKey);
            session.Remove(RolesKey);
            session.Remove(ExpiryKey);
        }

        public static bool IsAuthenticated(ISession session)
        {
            var token = session.GetString(TokenKey);
            if (string.IsNullOrEmpty(token)) return false;

            var expiryStr = session.GetString(ExpiryKey);
            if (DateTime.TryParse(expiryStr, out var expiry))
                return DateTime.UtcNow < expiry;

            return false;
        }

        public static string? GetToken(ISession session) => session.GetString(TokenKey);
        public static string GetFullName(ISession session) => session.GetString(FullNameKey) ?? string.Empty;
        public static string GetPhone(ISession session) => session.GetString(PhoneKey) ?? string.Empty;

        public static List<string> GetRoles(ISession session)
        {
            var json = session.GetString(RolesKey);
            return string.IsNullOrEmpty(json)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }

        public static bool IsInRole(ISession session, string role) =>
            GetRoles(session).Contains(role, StringComparer.OrdinalIgnoreCase);
    }
}