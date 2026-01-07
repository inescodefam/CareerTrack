using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;

namespace CareerTrack.E2ETests
{
    [TestClass]
    public class GoalsE2ETests : PageTest
    {
        // Update this to your local or deployed app URL
        private const string BaseUrl = "http://localhost:5276"; // Change port if needed
        // For Render deployment, use: "https://careertrack-dl4t.onrender.com"

        [TestInitialize]
        public void Setup()
        {
            // Set default timeout for all operations
            Page.SetDefaultTimeout(300000);
            //await LoginTestUser();
        }

        #region User Registration and Login Tests

        [TestMethod]
        public async Task UserRegistration_WithValidData_ShouldSucceed()
        {
            // Arrange
            var timestamp = DateTime.Now.Ticks;
            var username = $"testuser{timestamp}";
            var email = $"test{timestamp}@example.com";

            // Act
            await Page.GotoAsync($"{BaseUrl}/User/Register");

            await Page.FillAsync("input[name='Username']", username);
            await Page.FillAsync("input[name='FirstName']", "Test");
            await Page.FillAsync("input[name='LastName']", "User");
            await Page.FillAsync("input[name='Email']", email);
            await Page.FillAsync("input[name='Password']", "TestPassword123!");
            await Page.FillAsync("input[name='Phone']", "0123456789");
            //await Page.FillAsync("input[name='ConfirmPassword']", "Test@123"); one day we have confim password

            await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();

            // Wait for navigation to complete
            await Page.WaitForURLAsync(new Regex(".*/User/Login"), new() { Timeout = 10000 });

            // Assert
            await Expect(Page).ToHaveURLAsync(new Regex(".*/User/Login"));
        }

        [TestMethod]
        public async Task UserLogin_WithValidCredentials_ShouldRedirectToGoals()
        {
            
            // Act
            await Page.GotoAsync($"{BaseUrl}/User/Login");

            await Page.FillAsync("input[name='Username']", "testuser");
            await Page.FillAsync("input[name='Password']", "Test@123");

            await Page.ClickAsync("button[type='submit']");

            // Assert
            await Expect(Page).ToHaveURLAsync(new Regex(".*/Goals"));
        }

        #endregion

        #region Goals CRUD Tests

        [TestMethod]
        public async Task GoalsIndex_WhenLoggedIn_ShouldDisplayGoalsList()
        {
            // Arrange
            await LoginTestUser();

            // Act
            await Page.GotoAsync($"{BaseUrl}/Goals");

            // Assert
            await Expect(Page.Locator("h1, h2")).ToContainTextAsync(new Regex("Goals|My Goals", RegexOptions.IgnoreCase));

            // Check for create button or link
            var createButton = Page.Locator("text=/Create|New Goal/i");
            await Expect(createButton.First).ToBeVisibleAsync();
        }

        [TestMethod]
        public async Task CreateGoal_WithValidData_ShouldAddNewGoal()
        {
            // Arrange
            await LoginTestUser();
            
            var goalName = $"Test Goal {DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}";
            var goalDescription = "This is an E2E test goal";

            // Act
            await Page.GotoAsync($"{BaseUrl}/Goals/Create");

            await Page.FillAsync("input[name='Name']", goalName);
            await Page.FillAsync("textarea[name='Description']", goalDescription);

            // Fill dates
            var startDate = DateTime.Today.ToString("yyyy-MM-dd");
            var targetDate = DateTime.Today.AddMonths(1).ToString("yyyy-MM-dd");

            await Page.FillAsync("input[name='startDate']", startDate);
            await Page.FillAsync("input[name='targetDate']", targetDate);

            await Page.ClickAsync("button[type='submit'], input[type='submit']");

            // Assert
            await Expect(Page).ToHaveURLAsync(new Regex(".*/Goals"));
            await Expect(Page.Locator($"text={goalName}")).ToBeVisibleAsync();
        }

        [TestMethod]
        public async Task CreateGoal_WithInvalidData_ShouldShowValidationErrors()
        {
            // Arrange
            await LoginTestUser();

            // Act
            await Page.GotoAsync($"{BaseUrl}/Goals/Create");

            // Leave name empty (invalid)
            await Page.FillAsync("input[name='Name']", "");

            await Page.ClickAsync("button[type='submit'], input[type='submit']");

            // Assert - Should stay on create page with validation error
            await Expect(Page).ToHaveURLAsync(new Regex(".*/Goals/Create"));

            // Check for validation error message
            var errorMessage = Page.Locator(".field-validation-error, .validation-summary-errors, .text-danger");
            await Expect(errorMessage.First).ToBeVisibleAsync();
        }

        [TestMethod]
        public async Task GoalDetails_WhenClicked_ShouldShowGoalInformation()
        {
            // Arrange
            await LoginTestUser();
            await Page.GotoAsync($"{BaseUrl}/Goals");

            // Act - Click on first goal
            var detailsLink = Page.Locator("a:has-text('Details'), a:has-text('View')").First;
            await detailsLink.ClickAsync();

            // Assert
           // await Expect(Page).ToHaveURLAsync(new Regex(".*/Goals/Details/\\d+"));

            // Check for goal details elements
            //await Expect(Page.Locator("text=/Test Goal ")).ToBeVisibleAsync();
            //await Expect(Page.GetByText("Test Goal")).ToBeVisibleAsync();

          
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            
            await Expect(Page).ToHaveURLAsync(new Regex(".*/Goals/Details/\\d+"));

           
            await Expect(Page.Locator("h2:has(i.bi-bullseye)")).ToBeVisibleAsync(); 
            await Expect(Page.Locator("h5:text('Description')")).ToBeVisibleAsync(); 
            await Expect(Page.Locator("text='Start Date'")).ToBeVisibleAsync(); 
                                                                                
            //await Expect(Page.Locator(".progress")).ToBeVisibleAsync(); 
        }

        [TestMethod]
        public async Task EditGoal_WithValidChanges_ShouldUpdateGoal()
        {
            // Arrange
            await LoginTestUser();
            await Page.GotoAsync($"{BaseUrl}/Goals");

            // Act - Click Edit on first goal
            var editLink = Page.Locator("a:has-text('Edit')").First;
            await editLink.ClickAsync();

            // Modify the goal name
            var updatedName = $"Updated Goal {DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}";
            await Page.FillAsync("input[name='Name']", updatedName);

            //await Page.ClickAsync("button[type='submit']");
            await Page.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("Save Changes", RegexOptions.IgnoreCase) }).ClickAsync();

   
            await Page.WaitForURLAsync(new Regex(".*/Goals$"), new() { Timeout = 10000 });
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Assert - Verify we're back on Goals page
            await Expect(Page).ToHaveURLAsync(new Regex(".*/Goals$"));

            await Expect(Page.Locator("h1:has-text('My Goals')")).ToBeVisibleAsync();

            var goalCards = Page.Locator(".card .card-title");
            await Expect(goalCards.First).ToBeVisibleAsync();
        }

        [TestMethod]
        public async Task DeleteGoal_WhenConfirmed_ShouldRemoveGoal()
        {
            // Arrange - First create a goal to delete
            await LoginTestUser();
            var goalToDelete = $"Goal To Delete {DateTime.Now.Ticks}";
            await CreateTestGoal(goalToDelete);

            await Page.GotoAsync($"{BaseUrl}/Goals");

            // Act - First click View to go to Details page (Delete button is on Details page, not Index)
            var viewLink = Page.GetByText(goalToDelete).Locator("..").Locator("..").Locator("..").GetByRole(AriaRole.Link, new() { NameRegex = new Regex("View", RegexOptions.IgnoreCase) });
            await viewLink.ClickAsync();

            // Now click Delete on the Details page
            await Page.GetByRole(AriaRole.Link, new() { NameRegex = new Regex("Delete Goal", RegexOptions.IgnoreCase) }).ClickAsync();


            // Confirm deletion
            await Page.ClickAsync("button[type='submit'], input[value='Delete']");

            // Assert
            await Expect(Page).ToHaveURLAsync(new Regex(".*/Goals"));

            // Goal should no longer be visible
            var deletedGoal = Page.Locator($"text={goalToDelete}");
            await Expect(deletedGoal).Not.ToBeVisibleAsync();
        }

        #endregion

        #region Progress Tracking Tests

        [TestMethod]
        public async Task UpdateProgress_WithValidPercentage_ShouldUpdateGoalProgress()
        {
            // Arrange
            await LoginTestUser();
            await Page.GotoAsync($"{BaseUrl}/Goals");

            // Navigate to goal details
            var detailsLink = Page.Locator("a:has-text('Details'), a:has-text('View')").First;
            await detailsLink.ClickAsync();

            // Act - Update progress
            var progressInput = Page.Locator("input[name='progressPercentage'], input[type='number']");
            if (await progressInput.CountAsync() > 0)
            {
                await progressInput.FillAsync("75");

                var notesInput = Page.Locator("textarea[name='notes'], input[name='notes']");
                if (await notesInput.CountAsync() > 0)
                {
                    await notesInput.FillAsync("E2E test progress update");
                }

                await Page.ClickAsync("button:has-text('Update'), input[value*='Update']");

                // Assert
                await Expect(Page.Locator("text=/75|Progress/i")).ToBeVisibleAsync();
            }
        }

        #endregion

        #region Export Tests

        [TestMethod]
        public async Task ExportGoal_ToPDF_ShouldDownloadFile()
        {
            // Arrange
            await LoginTestUser();
            await Page.GotoAsync($"{BaseUrl}/Goals");

            // Navigate to goal details
            var detailsLink = Page.Locator("a:has-text('Details'), a:has-text('View')").First;
            await detailsLink.ClickAsync();

            // Act - Click export/print button
            var exportButton = Page.Locator("a:has-text('Export'), a:has-text('Print'), a:has-text('PDF')");

            if (await exportButton.CountAsync() > 0)
            {
                // Wait for download
                var downloadTask = Page.WaitForDownloadAsync();
                await exportButton.First.ClickAsync();
                var download = await downloadTask;

                // Assert
                Assert.IsNotNull(download);
                Assert.IsTrue(download.SuggestedFilename.Contains("goal") || download.SuggestedFilename.Contains("pdf"));
            }
        }

        #endregion

        #region Navigation and UI Tests

        [TestMethod]
        public async Task NavigationMenu_ShouldContainGoalsLink()
        {
            // Arrange & Act
            await LoginTestUser();

            await Page.GotoAsync(BaseUrl);

            // Assert
            var goalsLink = Page.Locator("nav a:has-text('Goals'), header a:has-text('Goals')");
            await Expect(goalsLink.First).ToBeVisibleAsync();
        }

        [TestMethod]
        public async Task GoalsPage_ShouldBeResponsive()
        {
            // Arrange
            await LoginTestUser();

            // Test mobile viewport
            await Page.SetViewportSizeAsync(375, 667); // iPhone size
            await Page.GotoAsync($"{BaseUrl}/Goals");

            // Assert - Page should render without horizontal scroll
            var bodyWidth = await Page.EvaluateAsync<int>("document.body.scrollWidth");
            Assert.IsTrue(bodyWidth <= 375 + 20); // Allow small margin

            // Test tablet viewport
            await Page.SetViewportSizeAsync(768, 1024);
            await Page.GotoAsync($"{BaseUrl}/Goals");

            // Test desktop viewport
            await Page.SetViewportSizeAsync(1920, 1080);
            await Page.GotoAsync($"{BaseUrl}/Goals");

            // Page should load successfully in all viewports
            await Expect(Page.Locator("h1, h2")).ToBeVisibleAsync();
        }

        #endregion

        #region Helper Methods

        private async Task LoginTestUser()
        {
            await Page.GotoAsync($"{BaseUrl}/User/Login");

            // Make sure this user exists in your test database
            await Page.FillAsync("input[name='Username']", "testuser");
            await Page.FillAsync("input[name='Password']", "Test@123");

            await Page.ClickAsync("button[type='submit']");

            // Wait for redirect to complete
            await Page.WaitForURLAsync(new Regex(".*/Goals|Home"), new() { Timeout = 5000 });
        }

        private async Task CreateTestGoal(string goalName)
        {
            await Page.GotoAsync($"{BaseUrl}/Goals/Create");

            await Page.FillAsync("input[name='Name']", goalName);
            await Page.FillAsync("textarea[name='Description']", "E2E test goal for deletion");

            var startDate = DateTime.Today.ToString("yyyy-MM-dd");
            var targetDate = DateTime.Today.AddMonths(1).ToString("yyyy-MM-dd");

            await Page.FillAsync("input[name='startDate']", startDate);
            await Page.FillAsync("input[name='targetDate']", targetDate);

            await Page.ClickAsync("button[type='submit'], input[type='submit']");

            // Wait for redirect
            await Page.WaitForURLAsync(new Regex(".*/Goals"));
        }

        #endregion
    }
}