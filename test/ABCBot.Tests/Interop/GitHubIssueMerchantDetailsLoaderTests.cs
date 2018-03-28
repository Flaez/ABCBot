using ABCBot.Interop;
using ABCBot.Models;
using ABCBot.Schema;
using ABCBot.Services;
using Moq;
using Octokit;
using Optional.Unsafe;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ABCBot.Tests.Interop
{
    public class GitHubIssueMerchantDetailsLoaderTests
    {
        string sampleMerchantName = "9figures";
        string sampleCategory = "General retail";
        string sampleBody = @"Requesting to add '9figures' to the 'General retail' category. 

Details follow: 
```yml 
- name: 9figures
      url: https://9figures.co.uk/
      img: 
      facebook: 9figuresuk
      email_address: 9figurescompany@gmail.com
      bch: Yes
      btc: No
      othercrypto: Yes
      doc: https://9figures.co.uk/blogs/news/accepting-cryptocurrency-1
```


Resources for adding this merchant:
[Link to 9figures](https://9figures.co.uk/)
If needed, try the facebook handles profile image: [fb.com/9figuresuk](https://fb.com/9figuresuk)


- [ ] Verify site is legitimate and safe to list.
- [ ] Correct data in form if any is innacurate.

If everything looks okay, Add it to the site:
- [ ] Assign to yourself when you begin work.
- [ ] Download and resize the image and put it into the proper img folder.
- [ ] Add listing alphabetically to proper .yml file.
- [ ] Commit changes mentioning this issue number with 'closes #[ISSUE NUMBER HERE]'.".NormalizeLineEndings();

        [Fact]
        public void ItShouldExtractMerchantDetailsFromIssueTitleSuccessfully() {
            var title = $"Add '{sampleMerchantName}' to the '{sampleCategory}' category";

            var detailsLoader = new GithubIssueMerchantDetailsLoader(Mock.Of<IGitHubService>());
            var merchantDetails = new MerchantDetails();

            var result = detailsLoader.ExtractDetailsFromTitle(title, merchantDetails);

            Assert.True(result);
            Assert.Equal(sampleMerchantName, merchantDetails.Values["name"].Value);
            Assert.Equal(sampleCategory, merchantDetails.Values["category"].Value);
        }

        [Fact]
        public void ItShouldExtractYmlBlockFromIssueBody() {
            var ymlString = @"- name: 9figures
      url: https://9figures.co.uk/
      img: 
      facebook: 9figuresuk
      email_address: 9figurescompany@gmail.com
      bch: Yes
      btc: No
      othercrypto: Yes
      doc: https://9figures.co.uk/blogs/news/accepting-cryptocurrency-1".NormalizeLineEndings();

            var detailsLoader = new GithubIssueMerchantDetailsLoader(Mock.Of<IGitHubService>());

            var result = detailsLoader.ExtractYmlCodeBlockFromIssueBody(sampleBody).ValueOrFailure();

            Assert.Equal(ymlString, result);
        }

        [Fact]
        public async Task ItShouldExtractDetailsFromIssueSuccessfully() {
            var title = $"Add '{sampleMerchantName}' to the '{sampleCategory}' category";
            var issue = new Issue("", "", "", "", 0, ItemState.Open, title, sampleBody, null, null, null, null, null, null, 0, null, null, DateTimeOffset.MinValue, null, 0, false, null);

            var schema = new MappingSchemaItem()
            {
                Mapping =
                {
                    { "websites", new SequenceSchemaItem()
                        {
                            Items =
                            {
                                new MappingSchemaItem()
                                {
                                    Name = "Website",
                                    Mapping =
                                    {
                                        { "name", new KeyValueSchemaItem() { Type = "str", Required = true, Unique = true } },
                                        { "url", new KeyValueSchemaItem() { Type = "str", Required = true, Unique = true } },
                                        { "img", new KeyValueSchemaItem() { Type = "str", Pattern = @"/\.png$/i" } },
                                        { "bch", new KeyValueSchemaItem() { Type = "bool", Required = true } },
                                        { "btc", new KeyValueSchemaItem() { Type = "bool" } },
                                        { "othercrypto", new KeyValueSchemaItem() { Type = "bool" } },
                                        { "facebook", new KeyValueSchemaItem() { Type = "str", Pattern = @"/(\w){1,50}$/" } },
                                        { "email_address", new KeyValueSchemaItem() { Type = "str", Pattern = @"/\A([\w+\-].?)+@[a-z\d\-]+(\.[a-z]+)*\.[a-z]+\z/i" } },
                                        { "doc", new KeyValueSchemaItem() { Type = "str" } }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var githubService = new Mock<IGitHubService>();
            githubService.Setup(x => x.GetIssue(It.IsAny<RepositoryTarget>(), It.IsAny<int>())).ReturnsAsync(issue);
            githubService.Setup(x => x.GetIssueComments(It.IsAny<RepositoryTarget>(), It.IsAny<int>())).ReturnsAsync(new List<IssueComment>());

            var detailsLoader = new GithubIssueMerchantDetailsLoader(githubService.Object);

            var result = await detailsLoader.ExtractDetails(schema, 0);

            var loadedMerchantDetails = result.ValueOrFailure();

            Assert.Equal(sampleMerchantName, loadedMerchantDetails.Values["name"].Value);
            Assert.Equal(sampleCategory, loadedMerchantDetails.Values["category"].Value);
            Assert.Equal("https://9figures.co.uk/", loadedMerchantDetails.Values["url"].Value);
            Assert.Equal("", loadedMerchantDetails.Values["img"].Value);
            Assert.Equal("9figuresuk", loadedMerchantDetails.Values["facebook"].Value);
            Assert.Equal("9figurescompany@gmail.com", loadedMerchantDetails.Values["email_address"].Value);
            Assert.True(loadedMerchantDetails.Values["bch"].Value.ToBoolean());
            Assert.False(loadedMerchantDetails.Values["btc"].Value.ToBoolean());
            Assert.True(loadedMerchantDetails.Values["othercrypto"].Value.ToBoolean());
            Assert.Equal("https://9figures.co.uk/blogs/news/accepting-cryptocurrency-1", loadedMerchantDetails.Values["doc"].Value);
        }

        [Fact]
        public async Task ItShouldApplyCommentCommandsToMerchantDetails() {
            var url = "https://google.com";
            var name = "Google";
            var category = "testing";

            var schema = new MappingSchemaItem()
            {
                Mapping =
                {
                    { "websites", new SequenceSchemaItem()
                        {
                            Items =
                            {
                                new MappingSchemaItem()
                                {
                                    Name = "Website",
                                    Mapping =
                                    {
                                        { "name", new KeyValueSchemaItem() { Type = "str", Required = true, Unique = true } },
                                        { "url", new KeyValueSchemaItem() { Type = "str", Required = true, Unique = true } }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var adminUser = new User("", "", "", 0, "", DateTimeOffset.MinValue, DateTimeOffset.MinValue, 0, "", 0,
                                     0, null, "", 0, 0, "", "admin", null, 0, null, 0, 0, 0, "",
                                     new RepositoryPermissions(true, true, true), false, "", null);

            var collaboratorUser = new User("", "", "", 0, "", DateTimeOffset.MinValue, DateTimeOffset.MinValue, 0, "", 0,
                                    0, null, "", 0, 0, "", "collaborator", null, 0, null, 0, 0, 0, "",
                                    new RepositoryPermissions(false, true, false), false, "", null);

            var externalUser = new User("", "", "", 0, "", DateTimeOffset.MinValue, DateTimeOffset.MinValue, 0, "", 0,
                                    0, null, "", 0, 0, "", "external", null, 0, null, 0, 0, 0, "",
                                    new RepositoryPermissions(false, false, false), false, "", null);

            var externalUserCommentBody = @"Wow, this looks great!

Will this be included in the next release? Can't wait!";
            var secondExternalUserCommentBody = @"/abc url https://evilsite.org";

            var collaboratorUserCommentBody = $"/abc url {url}\n/abc category {category}";
            var secondCollaboratorUserCommentBody = $"/abc name {name}";
            var adminUserCommentBody = @"LGTM";

            var externalUserComment = new IssueComment(0, "", "", externalUserCommentBody, DateTimeOffset.MinValue, null, externalUser);
            var collaboratorUserComment = new IssueComment(0, "", "", collaboratorUserCommentBody, DateTimeOffset.MinValue, null, collaboratorUser);
            var secondCollaboratorUserComment = new IssueComment(0, "", "", secondCollaboratorUserCommentBody, DateTimeOffset.MinValue, null, collaboratorUser);
            var adminUserComment = new IssueComment(0, "", "", adminUserCommentBody, DateTimeOffset.MinValue, null, adminUser);
            var secondExternalUserComment = new IssueComment(0, "", "", secondExternalUserCommentBody, DateTimeOffset.MinValue, null, externalUser);

            var issueComments = new List<IssueComment>()
            {
                externalUserComment,
                collaboratorUserComment,
                secondCollaboratorUserComment,
                adminUserComment,
                secondExternalUserComment
            };

            var merchantDetails = new MerchantDetails();

            var githubService = new Mock<IGitHubService>();
            githubService.Setup(x => x.GetIssueComments(It.IsAny<RepositoryTarget>(), It.IsAny<int>())).ReturnsAsync(issueComments);
            githubService.Setup(x => x.IsCollaborator(It.IsAny<RepositoryTarget>(), It.Is<string>(y => y == "admin" || y == "collaborator"))).ReturnsAsync(true);
            githubService.Setup(x => x.IsCollaborator(It.IsAny<RepositoryTarget>(), It.Is<string>(y => y == "external"))).ReturnsAsync(false);

            var detailsLoader = new GithubIssueMerchantDetailsLoader(githubService.Object);

            await detailsLoader.ApplyIssueCommentCommandsToMerchantDetails(issueComments, schema, merchantDetails);

            Assert.Equal(url, merchantDetails.Values["url"].Value);
            Assert.Equal(name, merchantDetails.Values["name"].Value);
            Assert.Equal(category, merchantDetails.Values["category"].Value);
            Assert.True(merchantDetails.ShouldStopExecuting);
        }

        [Fact]
        public async Task ItShouldAllowExecutingIfLastCommentWasCommand() {
            var url = "https://google.com";
            var name = "Google";

            var schema = new MappingSchemaItem()
            {
                Mapping =
                {
                    { "websites", new SequenceSchemaItem()
                        {
                            Items =
                            {
                                new MappingSchemaItem()
                                {
                                    Name = "Website",
                                    Mapping =
                                    {
                                        { "name", new KeyValueSchemaItem() { Type = "str", Required = true, Unique = true } },
                                        { "url", new KeyValueSchemaItem() { Type = "str", Required = true, Unique = true } }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var collaboratorUser = new User("", "", "", 0, "", DateTimeOffset.MinValue, DateTimeOffset.MinValue, 0, "", 0,
                                    0, null, "", 0, 0, "", "collaborator", null, 0, null, 0, 0, 0, "",
                                    new RepositoryPermissions(false, true, false), false, "", null);

            var externalUser = new User("", "", "", 0, "", DateTimeOffset.MinValue, DateTimeOffset.MinValue, 0, "", 0,
                                    0, null, "", 0, 0, "", "external", null, 0, null, 0, 0, 0, "",
                                    new RepositoryPermissions(false, false, false), false, "", null);

            var externalUserCommentBody = @"Wow, this looks great!

Will this be included in the next release? Can't wait!";

            var collaboratorUserCommentBody = $"/abc url {url}";
            var secondCollaboratorUserCommentBody = $"/abc name {name}";

            var externalUserComment = new IssueComment(0, "", "", externalUserCommentBody, DateTimeOffset.MinValue, null, externalUser);
            var collaboratorUserComment = new IssueComment(0, "", "", collaboratorUserCommentBody, DateTimeOffset.MinValue, null, collaboratorUser);
            var secondCollaboratorUserComment = new IssueComment(0, "", "", secondCollaboratorUserCommentBody, DateTimeOffset.MinValue, null, collaboratorUser);

            var issueComments = new List<IssueComment>()
            {
                externalUserComment,
                collaboratorUserComment,
                secondCollaboratorUserComment,
            };

            var merchantDetails = new MerchantDetails();

            var githubService = new Mock<IGitHubService>();
            githubService.Setup(x => x.GetIssueComments(It.IsAny<RepositoryTarget>(), It.IsAny<int>())).ReturnsAsync(issueComments);
            githubService.Setup(x => x.IsCollaborator(It.IsAny<RepositoryTarget>(), It.Is<string>(y => y == "admin" || y == "collaborator"))).ReturnsAsync(true);
            githubService.Setup(x => x.IsCollaborator(It.IsAny<RepositoryTarget>(), It.Is<string>(y => y == "external"))).ReturnsAsync(false);

            var detailsLoader = new GithubIssueMerchantDetailsLoader(githubService.Object);

            await detailsLoader.ApplyIssueCommentCommandsToMerchantDetails(issueComments, schema, merchantDetails);

            Assert.Equal(url, merchantDetails.Values["url"].Value);
            Assert.Equal(name, merchantDetails.Values["name"].Value);
            Assert.False(merchantDetails.ShouldStopExecuting);
        }

        [Fact]
        public async Task ItShouldAllowExecutingIfNoComments() {
            var schema = new MappingSchemaItem()
            {
                Mapping =
                {
                    { "websites", new SequenceSchemaItem()
                        {
                            Items =
                            {
                                new MappingSchemaItem()
                                {
                                    Name = "Website",
                                    Mapping =
                                    {
                                        { "name", new KeyValueSchemaItem() { Type = "str", Required = true, Unique = true } },
                                        { "url", new KeyValueSchemaItem() { Type = "str", Required = true, Unique = true } }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var issueComments = new List<IssueComment>();

            var merchantDetails = new MerchantDetails();

            var githubService = new Mock<IGitHubService>();
            githubService.Setup(x => x.GetIssueComments(It.IsAny<RepositoryTarget>(), It.IsAny<int>())).ReturnsAsync(issueComments);

            var detailsLoader = new GithubIssueMerchantDetailsLoader(githubService.Object);

            await detailsLoader.ApplyIssueCommentCommandsToMerchantDetails(issueComments, schema, merchantDetails);

            Assert.False(merchantDetails.ShouldStopExecuting);
        }
    }
}
