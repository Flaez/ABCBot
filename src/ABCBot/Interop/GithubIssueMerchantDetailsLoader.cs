using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ABCBot.Models;
using ABCBot.Services;
using CommonMark;
using Octokit;
using Optional;
using Optional.Unsafe;
using Serilog;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ABCBot.Interop
{
    public class GithubIssueMerchantDetailsLoader : IMerchantDetailsLoader
    {
        IGitHubService githubService;

        public GithubIssueMerchantDetailsLoader(IGitHubService githubService) {
            this.githubService = githubService;
        }

        public async Task<Option<MerchantDetails>> ExtractDetails(int identifier) {
            var issue = await githubService.GetIssue(RepositoryTarget.Upstream, identifier);

            var merchantDetails = new MerchantDetails();

            var titleResults = TryExtractDetailsFromTitle(issue.Title, merchantDetails);
            if (!titleResults) {
                return Option.None<MerchantDetails>();
            }

            var maybeYmlBlock = ExtractYmlCodeBlockFromIssueBody(issue.Body);
            if (!maybeYmlBlock.HasValue) {
                return Option.None<MerchantDetails>();
            }

            var ymlBlock = maybeYmlBlock.ValueOrFailure();

            // YamlDotNet apparently can't load partial yml files
            // Dumb mapping loader ahead
            // Edit: Turns out YamlDotNet can load partial files, just in a not-so-obvious format. Implement that "later"
            using (var ymlBlockReader = new StringReader(ymlBlock)) {
                var line = ymlBlockReader.ReadLine();

                while (line != null) {
                    line = line.TrimStart('-').TrimStart();

                    var separatorIndex = line.IndexOf(':');
                    var key = line.Substring(0, separatorIndex).Trim();
                    var value = line.Substring(separatorIndex + 1, (line.Length - separatorIndex - 1)).Trim();

                    MapYmlKeyToDetails(merchantDetails, key, value);

                    line = ymlBlockReader.ReadLine();
                }
            }

            return Option.Some(merchantDetails);
        }

        private void MapYmlKeyToDetails(MerchantDetails merchantDetails, string key, string value) {
            switch (key) {
                case "name":
                    merchantDetails.Name = value;
                    break;
                case "url":
                    merchantDetails.Url = value;
                    break;
                case "status":
                    merchantDetails.StatusUrl = value;
                    break;
                case "img":
                    merchantDetails.ImageUrl = value;
                    break;
                case "email_address":
                    merchantDetails.EmailAddress = value;
                    break;
                case "city":
                    merchantDetails.City = value;
                    break;
                case "state":
                    merchantDetails.State = value;
                    break;
                case "region":
                    merchantDetails.Region = value;
                    break;
                case "country":
                    merchantDetails.Country = value;
                    break;
                case "twitter":
                    merchantDetails.TwitterHandle = value;
                    break;
                case "facebook":
                    merchantDetails.FacebookHandle = value;
                    break;
                case "bch":
                    merchantDetails.AcceptsBCH = value.ToBoolean();
                    break;
                case "btc":
                    merchantDetails.AcceptsBTC = value.ToBoolean();
                    break;
                case "othercrypto":
                    merchantDetails.AcceptsOtherCrypto = value.ToBoolean();
                    break;
                case "doc":
                    merchantDetails.Document = value;
                    break;
                case "lang":
                    merchantDetails.Language = value;
                    break;
                case "category":
                    merchantDetails.Category = value;
                    break;

            }
        }

        public Option<string> ExtractYmlCodeBlockFromIssueBody(string body) {
            var document = CommonMarkConverter.Parse(body);
            foreach (var node in document.AsEnumerable()) {
                if (node.IsOpening && node.Block?.Tag == CommonMark.Syntax.BlockTag.FencedCode) {
                    if (node.Block.FencedCodeData.Info == "yml") {
                        return Option.Some(node.Block.StringContent.ToString().Trim('\n'));
                    }
                }
            }

            return Option.None<string>();
        }

        public bool TryExtractDetailsFromTitle(string title, MerchantDetails merchantDetails) {
            // RegEx pattern to match strings inside quotes (including escaped quotes) taken from
            // https://stackoverflow.com/a/171499
            var regex = new Regex(@"([""'])(?:(?=(\\?))\2.)*?\1", RegexOptions.IgnoreCase);

            var matches = regex.Matches(title);

            if (matches.Count != 2) {
                // Ensure there are only two matches - otherwise the title is in an unknown format
                return false;
            }

            // Ensure both matches were successful
            if (!matches[0].Success && !matches[1].Success) {
                return false;
            }

            merchantDetails.Name = matches[0].Value.Trim('\'');
            merchantDetails.Category = matches[1].Value.Trim('\'');

            Log.Debug("Extracted \"{name}\" and \"{category}\" from \"{title}\"", merchantDetails.Name, merchantDetails.Category, title);

            return true;
        }

        public void ApplyIssueCommentCommandsToMerchantDetails(IReadOnlyList<IssueComment> comments, MerchantDetails merchantDetails) {
            // Only apply comment commands from collaborators
            foreach (var comment in comments.Where(x => x.User.Permissions.Admin || x.User.Permissions.Push)) {

                var document = CommonMarkConverter.Parse(comment.Body);
                foreach (var node in document.AsEnumerable()) {
                    if (node.IsOpening && node.Block?.Tag == CommonMark.Syntax.BlockTag.Paragraph) {
                        var maybeCommand = node.Block.InlineContent.LiteralContent;

                        if (maybeCommand.StartsWith("/abc ")) {
                            var command = maybeCommand.Substring("/abc ".Length);

                            var firstSpacePosition = command.IndexOf(' ');

                            var key = command.Substring(0, firstSpacePosition);
                            var value = command.Substring(firstSpacePosition + 1, (command.Length - firstSpacePosition - 1));

                            MapYmlKeyToDetails(merchantDetails, key, value);
                        }
                    }
                }

            }
        }
    }
}
