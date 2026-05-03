using System;
using System.Net.Http.Headers;

namespace servicetest.E2ETests;

public class VotesFlowTest : ITestFlow
{
    public async Task RunAsync(TestRunner test)
    {
        Console.WriteLine("RUNNING VOTES E2E FLOW");

        // Register and login user1 (author)
        string user1Email = $"user1_{Guid.NewGuid()}@test.com";
        await test.Step("Register user1 (author)", async () =>
        {
            var res = await test.PostAsync("api/users/register", new
            {
                Username = "TestUser1",
                Email = user1Email,
                Password = "Test123!",
                IsModerator = false
            });
            test.Assert(res.Code == 200, "Expected HTTP 200 for register user1.", res.Json.ToString());
        });

        await test.Step("Login user1", async () =>
        {
            var res = await test.PostAsync("api/users/auth", new
            {
                Email = user1Email,
                Password = "Test123!"
            });
            test.Assert(res.Code == 200, "Expected HTTP 200 for login user1.", res.Json.ToString());
            test.SetBearer(res.Json.GetProperty("JwtToken").GetString()!);
        });

        // Create tag and question with user1
        string tagName = $"vote-tag-{Guid.NewGuid()}";
        await test.Step("Create tag", async () =>
        {
            var res = await test.PostAsync("api/questions/tag", tagName);
            test.Assert(res.Code == 200, "Expected HTTP 200 for creating tag.", res.Json.ToString());
        });

        string tagId = string.Empty;
        await test.Step("Get tag by name", async () =>
        {
            var res = await test.GetAsync($"api/questions/tag={tagName}");
            test.Assert(res.Code == 200, "Expected HTTP 200 for getting tag.", res.Json.ToString());
            tagId = res.Json.GetProperty("Id").GetString()!;
        });

        string questionId = string.Empty;
        await test.Step("Create question as user1", async () =>
        {
            using var form = new MultipartFormDataContent();

            form.Add(new StringContent("Vote Test Question"), "Title");
            form.Add(new StringContent("This question will be voted on."), "Text");
            form.Add(new StringContent(tagId), "TagIds");

            var dummyImageBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
            var imageContent = new ByteArrayContent(dummyImageBytes);
            imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
            form.Add(imageContent, "Image", "test-image.jpg");

            var res = await test.PostAsync("api/questions/create", form);
            test.Assert(res.Code == 200, "Expected HTTP 200 for creating question.", res.Json.ToString());
            questionId = res.Json.GetProperty("Id").GetString()!;
        });

        // Register and login user2 (voter)
        string user2Email = $"user2_{Guid.NewGuid()}@test.com";
        await test.Step("Register user2 (voter)", async () =>
        {
            var res = await test.PostAsync("api/users/register", new
            {
                Username = "TestUser2",
                Email = user2Email,
                Password = "Test123!",
                IsModerator = false
            });
            test.Assert(res.Code == 200, "Expected HTTP 200 for register user2.", res.Json.ToString());
        });

        await test.Step("Login user2", async () =>
        {
            var res = await test.PostAsync("api/users/auth", new
            {
                Email = user2Email,
                Password = "Test123!"
            });
            test.Assert(res.Code == 200, "Expected HTTP 200 for login user2.", res.Json.ToString());
            test.SetBearer(res.Json.GetProperty("JwtToken").GetString()!);
        });

        // Question vote tests
        await test.Step("Cast upvote on question (Success)", async () =>
        {
            var res = await test.PostAsync("api/votes/cast", new
            {
                PostId = questionId,
                IsQuestion = true,
                IsDownvote = false
            });
            test.Assert(res.Code == 200, "Expected HTTP 200 for casting upvote.", res.Json.ToString());
        });

        await test.Step("Cast same vote again (toggle off - Success)", async () =>
        {
            var res = await test.PostAsync("api/votes/cast", new
            {
                PostId = questionId,
                IsQuestion = true,
                IsDownvote = false
            });
            test.Assert(res.Code == 200, "Expected HTTP 200 for toggling vote off.", res.Json.ToString());
        });

        await test.Step("Cast vote a third time (re-add - Success)", async () =>
        {
            var res = await test.PostAsync("api/votes/cast", new
            {
                PostId = questionId,
                IsQuestion = true,
                IsDownvote = false
            });
            test.Assert(res.Code == 200, "Expected HTTP 200 for re-adding vote.", res.Json.ToString());
        });

        await test.Step("Switch vote from upvote to downvote (Success)", async () =>
        {
            // First re-add upvote (user2 is logged in, vote was toggled off in previous step)
            await test.PostAsync("api/votes/cast", new
            {
                PostId = questionId,
                IsQuestion = true,
                IsDownvote = false
            });

            // Now switch to downvote
            var res = await test.PostAsync("api/votes/cast", new
            {
                PostId = questionId,
                IsQuestion = true,
                IsDownvote = true
            });
            test.Assert(res.Code == 200, "Expected HTTP 200 for switching vote direction.", res.Json.ToString());
        });

        await test.Step("Get question vote count (Success)", async () =>
        {
            var res = await test.GetAsync($"api/questions/{questionId}/votecount");
            test.Assert(res.Code == 200, "Expected HTTP 200 for getting question vote count.", res.Json.ToString());
        });

        // User2 creates a comment on user1's question
        string commentId = string.Empty;
        await test.Step("Create comment as user2", async () =>
        {
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(questionId), "QuestionId");
            form.Add(new StringContent("This is a test comment"), "Text");

            var res = await test.PostAsync("api/comments/create", form);
            test.Assert(res.Code == 200, "Expected HTTP 200 for creating comment.", res.Json.ToString());
            commentId = res.Json.GetProperty("Id").GetString()!;
        });

        // Switch to user1 to vote on user2's comment
        await test.Step("Login user1", async () =>
        {
            var res = await test.PostAsync("api/users/auth", new
            {
                Email = user1Email,
                Password = "Test123!"
            });
            test.Assert(res.Code == 200, "Expected HTTP 200 for login user1.", res.Json.ToString());
            test.SetBearer(res.Json.GetProperty("JwtToken").GetString()!);
        });

        await test.Step("Cast upvote on comment as user1 (Success)", async () =>
        {
            var res = await test.PostAsync("api/votes/cast", new
            {
                PostId = commentId,
                IsQuestion = false,
                IsDownvote = false
            });
            test.Assert(res.Code == 200, "Expected HTTP 200 for upvoting comment.", res.Json.ToString());
        });

        await test.Step("Self-vote on question (Fail)", async () =>
        {
            var res = await test.PostAsync("api/votes/cast", new
            {
                PostId = questionId,
                IsQuestion = true,
                IsDownvote = false
            });
            test.Assert(res.Code == 400, "Expected HTTP 400 for self-vote on question.", res.Json.ToString());
        });

        await test.Step("Self-vote on comment (Fail)", async () =>
        {
            // login user2 to self-vote on their own comment
            var authRes = await test.PostAsync("api/users/auth", new
            {
                Email = user2Email,
                Password = "Test123!"
            });
            test.SetBearer(authRes.Json.GetProperty("JwtToken").GetString()!);

            var res = await test.PostAsync("api/votes/cast", new
            {
                PostId = commentId,
                IsQuestion = false,
                IsDownvote = false
            });
            test.Assert(res.Code == 400, "Expected HTTP 400 for self-vote on comment.", res.Json.ToString());
        });

        // Check user scores
    }
}