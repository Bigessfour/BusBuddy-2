using Bus_Buddy.Services;

namespace BusBuddy.Tests.Infrastructure
{
    /// <summary>
    /// Mock implementation of IUserContextService for testing
    /// Returns consistent test user values for audit field testing
    /// </summary>
    public class MockUserContextService : IUserContextService
    {
        public string CurrentUserId => "TEST_001";
        public string CurrentUserName => "TestUser";
        public string CurrentUserEmail => "testuser@busbuddy.test";
        public bool IsAuthenticated => true;

        public void SetCurrentUser(string userId, string userName, string userEmail)
        {
            // Mock implementation - values are fixed for testing
        }

        public void ClearCurrentUser()
        {
            // Mock implementation - no action needed
        }

        public string GetCurrentUserForAudit()
        {
            return CurrentUserName; // Return "TestUser" for audit fields
        }
    }
}
