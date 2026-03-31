using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace servicetest.E2ETests;

public class QuestionsFlowTest : ITestFlow
{
    public async Task RunAsync(TestRunner test)
    {
        Console.WriteLine("RUNNING QUESTIONS E2E FLOW");

        string testEmail = $"user_{Guid.NewGuid()}@test.com";

        await test.Step("Register new user", async () =>
        {
            var res = await test.PostAsync("api/users/register", new
            {
                Username = "TestUser",
                Email = testEmail,
                Password = "Test123!",
                IsModerator = false
            });

            test.Assert(res.Code == 200, "Expected HTTP 200 OK for register.", res.Json.ToString());
        });

        await test.Step("Login user", async () =>
        {
            var res = await test.PostAsync("api/users/auth", new
            {
                Email = testEmail,
                Password = "Test123!"
            });

            test.Assert(res.Code == 200, "Expected HTTP 200 for login user.", res.Json.ToString());

            string token = res.Json.GetProperty("JwtToken").GetString()!;
            test.SetBearer(token);
        });

        string tagName = $"test-tag-{Guid.NewGuid()}";

        await test.Step("Create new tag", async () =>
        {
            var res = await test.PostAsync("api/questions/tag", tagName);

            test.Assert(res.Code == 200, "Expected HTTP 200 for creating new tag.", res.Json.ToString());
        });

        string tagId = string.Empty;
        await test.Step("Get tag by name", async () =>
        {
            var res = await test.GetAsync($"api/questions/tag={tagName}");

            test.Assert(res.Code == 200, "Expected HTTP 200 for getting the new tag.", res.Json.ToString());

            tagId = res.Json.GetProperty("Id").GetString()!;
        });

        string questionId = string.Empty;
        await test.Step("Create new question", async () =>
        {
            using var form = new MultipartFormDataContent();

            form.Add(new StringContent("My E2E Test Title"), "Title");
            form.Add(new StringContent("Here is the text for the E2E test."), "Text");
            form.Add(new StringContent(tagId), "TagIds");

            var dummyImageBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
            var imageContent = new ByteArrayContent(dummyImageBytes);
            imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");

            form.Add(imageContent, "Image", "e2e-test-image.jpg");

            var res = await test.PostAsync("api/questions/create", form);

            test.Assert(res.Code == 200, "Expected HTTP 200 for create question.", res.Json.ToString());
            questionId = res.Json.GetProperty("Id").GetString()!;
        });

        await test.Step("Get question by Id (Success)", async () =>
        {
            var res = await test.GetAsync($"api/questions/{questionId}");
            test.Assert(res.Code == 200, "Expected HTTP 200 for GetQuestionById.", res.Json.ToString());
        });

        await test.Step("Get question by Id (Fail - Invalid ID)", async () =>
        {
            var res = await test.GetAsync("api/questions/invalid-guid-12345");
            test.Assert(res.Code != 200, "Expected non-200 for non-existent question ID.", res.Json.ToString());
        });

        await test.Step("Get all questions paginated", async () =>
        {
            var res = await test.GetAsync("api/questions/page=1");
            test.Assert(res.Code == 200, "Expected HTTP 200 for fetching paginated questions.", res.Json.ToString());
        });

        await test.Step("Edit question (Success)", async () =>
        {
            using var form = new MultipartFormDataContent();

            form.Add(new StringContent("in_progress"), "Status");

            var res = await test.PutAsync($"api/questions/{questionId}", form);

            test.Assert(res.Code == 200, "Expected HTTP 200 because status changed", res.Json.ToString());
        });

        string updatedTitle = "My E2E Test Title - UPDATED";
        await test.Step("Edit question (Success)", async () =>
        {
            using var form = new MultipartFormDataContent();

            form.Add(new StringContent(updatedTitle), "Title");
            form.Add(new StringContent("This text has been updated via E2E test."), "Text");
            form.Add(new StringContent(tagId), "TagIds");
            form.Add(new StringContent("received"), "Status");

            var res = await test.PutAsync($"api/questions/{questionId}", form);

            test.Assert(res.Code == 200, "Expected HTTP 200 for successfully editing the question.", res.Json.ToString());
        });

        await test.Step("Get questions by Title", async () =>
        {
            var res = await test.GetAsync("api/questions/title/page=1", updatedTitle);
            test.Assert(res.Code == 200, "Expected HTTP 200 for fetching questions by title.", res.Json.ToString());
        });

        await test.Step("Get questions by Tag", async () =>
        {
            var res = await test.GetAsync("api/questions/tag/page=1", tagName);
            test.Assert(res.Code == 200, "Expected HTTP 200 for fetching questions by tag.", res.Json.ToString());
        });

        await test.Step("Solve question (Fail - Empty Question ID)", async () =>
        {
            var res = await test.PostAsync("api/questions/solve", "");
            test.Assert(res.Code == 400, "Expected HTTP 400 because the question ID is empty.", res.Json.ToString());
        });

        await test.Step("Solve question (Success)", async () =>
        {
            var res = await test.PostAsync("api/questions/solve", questionId);
            test.Assert(res.Code == 200, "Expected HTTP 200 for successfully solving the question.", res.Json.ToString());
        });

        await test.Step("Delete question (Fail - Invalid ID)", async () =>
        {
            var res = await test.DeleteAsync("api/questions/this-id-does-not-exist");
            test.Assert(res.Code != 200, "Expected failure (non-200) when deleting a non-existent question.", res.Json.ToString());
        });

        await test.Step("Delete question (Success)", async () =>
        {
            var res = await test.DeleteAsync($"api/questions/{questionId}");
            test.Assert(res.Code == 200, "Expected HTTP 200 for successfully deleting the question.", res.Json.ToString());
        });
    }
}
