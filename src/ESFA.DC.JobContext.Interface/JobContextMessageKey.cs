namespace ESFA.DC.JobContext.Interface
{
    public static class JobContextMessageKey
    {
        /// <summary>
        /// The UkPrn number. (int)
        /// </summary>
        public const string UkPrn = "UkPrn";

        /// <summary>
        /// The name of the container containing the file to process. (string)
        /// </summary>
        public const string Container = "Container";

        /// <summary>
        /// The name of the file to process. (string)
        /// </summary>
        public const string Filename = "Filename";

        /// <summary>
        /// The size of the file in bytes. (long)
        /// </summary>
        public const string FileSizeInBytes = "FileSizeInBytes";

        /// <summary>
        /// The username who started the operation. (string)
        /// </summary>
        public const string Username = "Username";

        /// <summary>
        /// The *Key* of the serialised and persisted valid learn ref numbers in storage. (string)
        /// </summary>
        public const string ValidLearnRefNumbers = "ValidLearnRefNumbers";

        /// <summary>
        /// The number of valid learners. (long)
        /// </summary>
        public const string ValidLearnRefNumbersCount = "ValidLearnRefNumbersCount";

        /// <summary>
        /// The *Key* of the serialised invalid learn ref numbers in storage. (string)
        /// </summary>
        public const string InvalidLearnRefNumbers = "InvalidLearnRefNumbers";

        /// <summary>
        /// The number of invalid learners. (long)
        /// </summary>
        public const string InvalidLearnRefNumbersCount = "InvalidLearnRefNumbersCount";

        /// <summary>
        /// The total number of validation errors (int)
        /// </summary>
        public const string ValidationTotalErrorCount = "ValidationTotalErrorCount";

        /// <summary>
        /// The total number of validation warnings (int)
        /// </summary>
        public const string ValidationTotalWarningCount = "ValidationTotalWarningCount";

        /// <summary>
        /// The *Key* of the serialised and persisted validation errors. (string)
        /// </summary>
        public const string ValidationErrors = "ValidationErrors";

        /// <summary>
        /// The *Key* of the serialised and persisted validation error lookups. (string)
        /// </summary>
        public const string ValidationErrorLookups = "ValidationErrorLookups";

        /// <summary>
        /// The *Key* of the serialised and persisted funding ALB output. (string)
        /// </summary>
        public const string FundingAlbOutput = "FundingAlbOutput";

         /// <summary>
        /// The *Key* of the serialised and persisted funding FM36 output. (string)
        /// </summary>
        public const string FundingFm36Output = "FundingFm36Output";

        /// <summary>
        /// The *Key* of the serialised and persisted funding FM35 output. (string)
        /// </summary>
        public const string FundingFm35Output = "FundingFm35Output";

        /// <summary>
        /// The *Key* of the serialised and persisted funding FM25 output. (string)
        /// </summary>
        public const string FundingFm25Output = "FundingFm25Output";

        /// <summary>
        /// Flag to determine if the job will be finished (marked as completed) or go to Awaiting Action status (bool)
        /// </summary>
        public const string PauseWhenFinished = "PauseWhenFinished";

        /// <summary>
        /// Flag to determine if the job has been cross loaded, marking the job exempt from finished status. (string)
        /// </summary>
        public const string JobIsCrossLoaded = "JobIsCrossLoaded";
    }
}
