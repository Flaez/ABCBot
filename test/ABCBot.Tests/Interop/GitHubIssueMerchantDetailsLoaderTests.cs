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
- [ ] Commit changes mentioning this issue number with 'closes #[ISSUE NUMBER HERE]'.";

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
        public async Task ItShouldExtractDetailsFromIssueSuccessfully() {
            var title = $"Add '{sampleMerchantName}' to the '{sampleCategory}' category";
            var issue = new Issue("", "", "", "", 0, ItemState.Open, title, sampleBody, null, null, null, null, null, null, 0, null, null, DateTimeOffset.MinValue, null, 0, false, null);

            var githubService = new Mock<IGitHubService>();
            githubService.Setup(x => x.GetIssue(It.IsAny<int>())).ReturnsAsync(issue);

            var detailsLoader = new GithubIssueMerchantDetailsLoader(githubService.Object);

            var result = await detailsLoader.ExtractDetails(0);

            var loadedMerchantDetails = result.ValueOrFailure();

            Assert.Equal(sampleMerchantName, loadedMerchantDetails.Name);
            Assert.Equal(sampleCategory, loadedMerchantDetails.Category);
        }
    }
}
