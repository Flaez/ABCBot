using ABCBot.Interop;
using ABCBot.Models;
using ABCBot.Services;
using Moq;
using Octokit;
using Optional.Unsafe;
using System;
using System.Collections.Generic;
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

            var result = detailsLoader.TryExtractDetailsFromTitle(title, merchantDetails);

            Assert.True(result);
            Assert.Equal(sampleMerchantName, merchantDetails.Name);
            Assert.Equal(sampleCategory, merchantDetails.Category);
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

            var githubService = new Mock<IGitHubService>();
            githubService.Setup(x => x.GetIssue(It.IsAny<RepositoryTarget>(), It.IsAny<int>())).ReturnsAsync(issue);

            var detailsLoader = new GithubIssueMerchantDetailsLoader(githubService.Object);

            var result = await detailsLoader.ExtractDetails(0);

            var loadedMerchantDetails = result.ValueOrFailure();

            Assert.Equal(sampleMerchantName, loadedMerchantDetails.Name);
            Assert.Equal(sampleCategory, loadedMerchantDetails.Category);
            Assert.Equal("https://9figures.co.uk/", loadedMerchantDetails.Url);
            Assert.Equal("", loadedMerchantDetails.ImageUrl);
            Assert.Equal("9figuresuk", loadedMerchantDetails.FacebookHandle);
            Assert.Equal("9figurescompany@gmail.com", loadedMerchantDetails.EmailAddress);
            Assert.True(loadedMerchantDetails.AcceptsBCH);
            Assert.False(loadedMerchantDetails.AcceptsBTC);
            Assert.True(loadedMerchantDetails.AcceptsOtherCrypto);
            Assert.Equal("https://9figures.co.uk/blogs/news/accepting-cryptocurrency-1", loadedMerchantDetails.Document);
        }

        [Fact]
        public void ItShouldApplyCommentCommandsToMerchantDetails() {
            var url = "https://google.com";
            var name = "Google";

            var adminUser = new User("", "", "", 0, "", DateTimeOffset.MinValue, DateTimeOffset.MinValue, 0, "", 0,
                                     0, null, "", 0, 0, "", "", "", 0, null, 0, 0, 0, "",
                                     new RepositoryPermissions(true, true, true), false, "", null);

            var collaboratorUser = new User("", "", "", 0, "", DateTimeOffset.MinValue, DateTimeOffset.MinValue, 0, "", 0,
                                    0, null, "", 0, 0, "", "", "", 0, null, 0, 0, 0, "",
                                    new RepositoryPermissions(false, true, false), false, "", null);

            var externalUser = new User("", "", "", 0, "", DateTimeOffset.MinValue, DateTimeOffset.MinValue, 0, "", 0,
                                    0, null, "", 0, 0, "", "", "", 0, null, 0, 0, 0, "",
                                    new RepositoryPermissions(false, false, false), false, "", null);

            var externalUserCommentBody = @"Wow, this looks great!

Will this be included in the next release? Can't wait!";

            var collaboratorUserCommentBody = $"/abc url {url}";
            var secondCollaboratorUserCommentBody = $"/abc name {name}";
            var adminUserCommentBody = @"LGTM";

            var externalUserComment = new IssueComment(0, "", "", externalUserCommentBody, DateTimeOffset.MinValue, null, externalUser);
            var collaboratorUserComment = new IssueComment(0, "", "", collaboratorUserCommentBody, DateTimeOffset.MinValue, null, collaboratorUser);
            var secondCollaboratorUserComment = new IssueComment(0, "", "", secondCollaboratorUserCommentBody, DateTimeOffset.MinValue, null, collaboratorUser);
            var adminUserComment = new IssueComment(0, "", "", adminUserCommentBody, DateTimeOffset.MinValue, null, adminUser);

            var issueComments = new List<IssueComment>()
            {
                externalUserComment,
                collaboratorUserComment,
                secondCollaboratorUserComment,
                adminUserComment
            };

            var merchantDetails = new MerchantDetails();

            var githubService = new Mock<IGitHubService>();
            githubService.Setup(x => x.GetIssueComments(It.IsAny<RepositoryTarget>(), It.IsAny<int>())).ReturnsAsync(issueComments);

            var detailsLoader = new GithubIssueMerchantDetailsLoader(githubService.Object);

            detailsLoader.ApplyIssueCommentCommandsToMerchantDetails(issueComments, merchantDetails);

            Assert.Equal(url, merchantDetails.Url);
            Assert.Equal(name, merchantDetails.Name);
        }
    }
}
