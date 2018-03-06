using System;
using System.Collections.Generic;
using System.Text;

namespace ABCBot.Pipeline
{
    public class PipelineProcessingResult
    {
        public bool IsSuccess { get; }
        public string Details { get; }

        private PipelineProcessingResult(bool isSuccess) {
            this.IsSuccess = isSuccess;
        }

        private PipelineProcessingResult(bool isSuccess, string details) : this(isSuccess) {
            this.Details = details;
        }

        public static PipelineProcessingResult Success() {
            return new PipelineProcessingResult(true);
        }

        public static PipelineProcessingResult Failure(string details) {
            return new PipelineProcessingResult(false, details);
        }
    }
}
