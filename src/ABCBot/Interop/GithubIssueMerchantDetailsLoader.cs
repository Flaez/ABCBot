using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ABCBot.Models;
using ABCBot.Schema;
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

        public async Task<Option<MerchantDetails>> ExtractDetails(ISchemaItem schema, int identifier) {
            var issue = await githubService.GetIssue(RepositoryTarget.Upstream, identifier);

            var merchantDetails = new MerchantDetails();

            ExtractDetailsFromTitle(issue.Title, merchantDetails);

            var maybeYmlBlock = ExtractYmlCodeBlockFromIssueBody(issue.Body);
            maybeYmlBlock.MatchSome(ymlBlock =>
            {
                var baseSchemaItem = (((schema as MappingSchemaItem).Mapping["websites"] as SequenceSchemaItem).Items[0] as MappingSchemaItem);

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

                        MapYmlKeyToDetailsWithSchema(merchantDetails, baseSchemaItem, key, value);

                        line = ymlBlockReader.ReadLine();
                    }
                }
            });

            var issueComments = await githubService.GetIssueComments(RepositoryTarget.Upstream, issue.Number);

            await ApplyIssueCommentCommandsToMerchantDetails(issueComments, schema, merchantDetails);

            return Option.Some(merchantDetails);
        }

        private void MapYmlKeyToDetailsWithSchema(MerchantDetails merchantDetails, MappingSchemaItem baseSchemaItem, string schemaXPath, string value) {
            var key = schemaXPath;

            // If the key isn't in the schema, this isn't a valid field and can be ignored
            // The exception to this is the 'category' key, which isn't included in the schema document
            // TODO: Announce invalid fields
            if (baseSchemaItem.Mapping.TryGetValue(key, out var schemaItem)) {
                switch (schemaItem) {
                    case KeyValueSchemaItem kvItem: {
                            var merchantDetailsItem = merchantDetails.UpsertValue(key);

                            merchantDetailsItem.SchemaItem = schemaItem;
                            merchantDetailsItem.Value = value;
                        }
                        break;
                }
            }

            if (key == "category") {
                var merchantDetailsItem = merchantDetails.UpsertValue(key);

                merchantDetailsItem.Value = value.Trim('\'').Trim('\"');
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

        public bool ExtractDetailsFromTitle(string title, MerchantDetails merchantDetails) {
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


            merchantDetails.UpsertValue("name").Value = matches[0].Value.Trim('\'');
            merchantDetails.UpsertValue("category").Value = matches[1].Value.Trim('\'');

            Log.Debug("Extracted \"{name}\" and \"{category}\" from \"{title}\"", merchantDetails.Values["name"].Value, merchantDetails.Values["category"].Value, title);

            return true;
        }

        public async Task ApplyIssueCommentCommandsToMerchantDetails(IReadOnlyList<IssueComment> comments, ISchemaItem schema, MerchantDetails merchantDetails) {
            var baseSchemaItem = (((schema as MappingSchemaItem).Mapping["websites"] as SequenceSchemaItem).Items[0] as MappingSchemaItem);

            var collaboratorStates = new Dictionary<string, bool>();

            bool shouldStopExecuting = false;

            // Only apply comment commands from collaborators
            foreach (var comment in comments) {
                shouldStopExecuting = true; // If there are any comments, it should only trigger on a command

                var collaboratorState = false;
                if (!collaboratorStates.TryGetValue(comment.User.Login, out collaboratorState)) {
                    collaboratorState = await githubService.IsCollaborator(RepositoryTarget.Upstream, comment.User.Login);
                    collaboratorStates.Add(comment.User.Login, collaboratorState);
                }

                if (collaboratorState) { // Only process comments from collaborators - we don't want other users altering data

                    var lines = comment.Body.NormalizeLineEndings().Split('\n');
                    foreach (var line in lines) {
                        if (line.StartsWith("/abc ")) {
                            // A command was found! Allow triggering the bot. This will be reset again if this was not the last comment in the chain
                            shouldStopExecuting = false;

                            var command = line.Substring("/abc ".Length);

                            var firstSpacePosition = command.IndexOf(' ');

                            var key = command.Substring(0, firstSpacePosition);
                            var value = command.Substring(firstSpacePosition + 1, (command.Length - firstSpacePosition - 1));

                            MapYmlKeyToDetailsWithSchema(merchantDetails, baseSchemaItem, key, value);
                        }
                    }
                }
            }

            merchantDetails.ShouldStopExecuting = shouldStopExecuting;
        }
    }
}
