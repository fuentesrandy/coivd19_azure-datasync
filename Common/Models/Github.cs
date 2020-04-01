using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Common.Model
{
    public class CommitAuthor
    {
        [JsonProperty("name")]
        public string name { get; set; }
        [JsonProperty("date")]
        public DateTime date { get; set; }
    }
    public class CommitData
    {
        [JsonProperty("author")]
        public CommitAuthor author { get; set; }
    }
    public class GithubRepoCommit
    {
        [JsonProperty("sha")]
        public string sha { get; set; }
        [JsonProperty("commit")]
        public CommitData commit { get; set; }
        [JsonProperty("url")]
        public string url { get; set; }
    }

   
    public class GitCommitFile {

        public string sha { get; set; }
        public string filename { get; set; }
        public string raw_url { get; set; }
    }

    public class GitCommitDetails {

        public string sha { get; set; }
        public CommitData commit { get; set; }
        public string url { get; set; }
        public IEnumerable<GitCommitFile> files { get; set; }
    }

}