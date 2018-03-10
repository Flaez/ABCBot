using ABCBot.Controllers.Webhooks;
using ABCBot.Services;
using ABCBot.ViewModels.GitHub;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Octokit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ABCBot.Tests.Controllers.Webhooks
{
    public class GitHubControllerTests
    {
        [Theory]
        [InlineData("opened")]
        [InlineData("edited")]
        public async Task ItShouldProcessIssuesWebhookWhenActionIsOpenedOrEdited(string actionState) {
            var pipelineRunnerService = new Mock<IPipelineRunnerService>();

            var controller = new GitHubController(pipelineRunnerService.Object, Mock.Of<IGitHubService>());

            var viewModel = new IssueWebHookViewModel()
            {
                Action = actionState,
                Issue = new IssueViewModel()
                {
                    Number = 5
                }
            };

            var result = await controller.HandleIssuesWebhook("", viewModel);

            pipelineRunnerService.Verify(x => x.ProcessIssue(It.Is<int>(y => y == 5)), Times.Once());
            Assert.IsType<OkResult>(result);
        }

        [Theory]
        [InlineData("assigned")]
        [InlineData("unassigned")]
        [InlineData("labeled")]
        [InlineData("unlabeled")]
        [InlineData("milestoned")]
        [InlineData("demilestoned")]
        [InlineData("closed")]
        [InlineData("reopened")]
        public async Task ItShouldNotProcessIssuesWebhookWhenActionIsNotOpenedOrEdited(string actionState) {
            var pipelineRunnerService = new Mock<IPipelineRunnerService>();

            var controller = new GitHubController(pipelineRunnerService.Object, Mock.Of<IGitHubService>());

            var viewModel = new IssueWebHookViewModel()
            {
                Action = actionState,
                Issue = new IssueViewModel()
                {
                    Number = 5
                }
            };

            var result = await controller.HandleIssuesWebhook("", viewModel);

            pipelineRunnerService.Verify(x => x.ProcessIssue(It.Is<int>(y => y == 5)), Times.Never());
            Assert.IsType<OkResult>(result);
        }

        [Theory]
        [InlineData("created")]
        public async Task ItShouldProcessIssueCommentsWebhookWhenActionIsCreated(string actionState) {
            var currentUser = new User("", "", "", 0, "", DateTimeOffset.MinValue, DateTimeOffset.MinValue, 0, "", 0,
                              0, null, "", 0, 0, "", "currentUser", null, 0, null, 0, 0, 0, "",
                              new RepositoryPermissions(false, false, false), false, "", null);

            var githubService = new Mock<IGitHubService>();
            githubService.Setup(x => x.GetCurrentUser()).ReturnsAsync(currentUser);

            var pipelineRunnerService = new Mock<IPipelineRunnerService>();

            var controller = new GitHubController(pipelineRunnerService.Object, githubService.Object);

            var viewModel = new IssueCommentWebHookViewModel()
            {
                Action = actionState,
                Issue = new IssueViewModel()
                {
                    Number = 5
                },
                Comment = new IssueCommentViewModel()
                {
                    User = new UserViewModel()
                    {
                        Login = "otherUser"
                    }
                }
            };

            var result = await controller.HandleIssueCommentsWebhook("", viewModel);

            pipelineRunnerService.Verify(x => x.ProcessIssue(It.Is<int>(y => y == 5)), Times.Once());
            Assert.IsType<OkResult>(result);
        }

        [Theory]
        [InlineData("edited")]
        [InlineData("deleted")]
        public async Task ItShouldNotProcessIssueCommentsWebhookWhenActionIsNotCreated(string actionState) {
            var pipelineRunnerService = new Mock<IPipelineRunnerService>();

            var controller = new GitHubController(pipelineRunnerService.Object, Mock.Of<IGitHubService>());

            var viewModel = new IssueCommentWebHookViewModel()
            {
                Action = actionState,
                Issue = new IssueViewModel()
                {
                    Number = 5
                },
                Comment = new IssueCommentViewModel()
                {
                    User = new UserViewModel()
                    {
                        Login = "test"
                    }
                }
            };

            var result = await controller.HandleIssueCommentsWebhook("", viewModel);

            pipelineRunnerService.Verify(x => x.ProcessIssue(It.Is<int>(y => y == 5)), Times.Never());
            Assert.IsType<OkResult>(result);
        }

        [Theory]
        [InlineData("created")]
        public async Task ItShouldNotProcessIssueCommentsWebhookWhenActionIsCreatedButCommentAuthorIsBotUser(string actionState) {
            var currentUser = new User("", "", "", 0, "", DateTimeOffset.MinValue, DateTimeOffset.MinValue, 0, "", 0,
                             0, null, "", 0, 0, "", "currentUser", null, 0, null, 0, 0, 0, "",
                             new RepositoryPermissions(false, false, false), false, "", null);

            var githubService = new Mock<IGitHubService>();
            githubService.Setup(x => x.GetCurrentUser()).ReturnsAsync(currentUser);

            var pipelineRunnerService = new Mock<IPipelineRunnerService>();

            var controller = new GitHubController(pipelineRunnerService.Object, githubService.Object);

            var viewModel = new IssueCommentWebHookViewModel()
            {
                Action = actionState,
                Issue = new IssueViewModel()
                {
                    Number = 5
                },
                Comment = new IssueCommentViewModel()
                {
                    User = new UserViewModel()
                    {
                        Login = currentUser.Login
                    }
                }
            };

            var result = await controller.HandleIssueCommentsWebhook("", viewModel);

            pipelineRunnerService.Verify(x => x.ProcessIssue(It.Is<int>(y => y == 5)), Times.Never());
            Assert.IsType<OkResult>(result);
        }
    }
}
