namespace PhotoMoveYearMonthFolder.Services
{
    /// <summary>
    /// Interface for reporting file processing progress to the UI.
    /// Enables decoupling of processing logic from UI updates.
    /// </summary>
    public interface IProgressReporter
    {
        /// <summary>
        /// Reports progress for media file processing.
        /// </summary>
        void ReportMediaFileProgress(int count, string fileName);

        /// <summary>
        /// Reports progress for non-media file processing.
        /// </summary>
        void ReportOtherFileProgress(int count, string fileName);

        /// <summary>
        /// Reports progress during the file enumeration and classification phase.
        /// </summary>
        void ReportEnumerationProgress(int totalFound, int classifiedAsMedia, int classifiedAsOther);

        /// <summary>
        /// Sets the progress bar to marquee (indeterminate) mode.
        /// </summary>
        void SetMediaProgressMarquee();

        /// <summary>
        /// Sets the progress bar to marquee (indeterminate) mode for other files.
        /// </summary>
        void SetOtherProgressMarquee();

        /// <summary>
        /// Completes the media files progress indicator.
        /// </summary>
        void CompleteMediaProgress();

        /// <summary>
        /// Completes the other files progress indicator.
        /// </summary>
        void CompleteOtherProgress();

        /// <summary>
        /// Resets progress indicators to initial state.
        /// </summary>
        void ResetProgress();
    }
}