using ABCBot.Services;
using ABCBot.ViewModels.GitHub;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABCBot.Controllers.Webhooks
{
    [Route("/webhooks/github")]
    public class GitHubController : Controller
    {
        IPipelineRunnerService pipelineRunnerService;
        IGitHubService gitHubService;

        public GitHubController(IPipelineRunnerService pipelineRunnerService, IGitHubService gitHubService) {
            this.pipelineRunnerService = pipelineRunnerService;
            this.gitHubService = gitHubService;
        }

        private bool IsSecretValid(string secretToTest) {
            if (string.IsNullOrEmpty(gitHubService.WebhookSecret)) {
                return true;
            }

            using (var sha1 = System.Security.Cryptography.SHA1.Create()) {
                var hash = sha1.ComputeHash(Encoding.ASCII.GetBytes(gitHubService.WebhookSecret));

                var hexString = string.Concat(hash.Select(@byte => @byte.ToString("X2")));

                return string.Equals(secretToTest, $"sha1={hexString}");
            }
        }

        [HttpPost("issues")]
        public async Task<IActionResult> HandleIssuesWebhook([FromHeader(Name = "X-Hub-Signature")] string secret, [FromBody] IssueWebHookViewModel viewModel) {
            if (!IsSecretValid(secret)) {
                return BadRequest();
            }

            if (viewModel.Action != "opened" && viewModel.Action != "edited") {
                return Ok();
            }

            Log.Information($"Starting to process incoming request for issue #{viewModel.Issue.Number}.");

            try {
                await pipelineRunnerService.ProcessIssue(viewModel.Issue.Number);
            } catch (Exception ex) {
                Log.Error("Error while processing issue {issue}: {error}", viewModel.Issue.Number, ex.ToString());
            }

            return Ok();
        }

        [HttpPost("issues/comments")]
        public async Task<IActionResult> HandleIssueCommentsWebhook([FromHeader(Name = "X-Hub-Signature")] string secret, [FromBody] IssueCommentWebHookViewModel viewModel) {
            if (!IsSecretValid(secret)) {
                return BadRequest();
            }

            if (viewModel.Action != "created") {
                return Ok();
            }

            // Stop the bot from responding to itself
            var currentUser = await gitHubService.GetCurrentUser();
            if (currentUser.Login == viewModel.Comment.User.Login) {
                return Ok();
            }

            Log.Information($"Starting to process incoming request for issue #{viewModel.Issue.Number}.");

            try {
                await pipelineRunnerService.ProcessIssue(viewModel.Issue.Number);
            } catch (Exception ex) {
                Log.Error("Error while processing issue {issue}: {error}", viewModel.Issue.Number, ex.ToString());
            }

            return Ok();
        }
    }
}
