// ReSharper disable CheckNamespace

using System;
using System.IO;

namespace SevenZipExtractor
{
    internal class ArchiveStreamCallback : IArchiveExtractCallback
    {
        private readonly uint fileNumber;
        private readonly Stream stream;
        private readonly EventHandler<EntryExtractionProgressEventArgs> progressEventHandler;
        
        private ulong currentCompleteValue;
        private ulong currentTotal;
        private bool finalProgressReported;

        public ArchiveStreamCallback(uint fileNumber, Stream stream, EventHandler<EntryExtractionProgressEventArgs> progressEventHandler)
        {
            this.fileNumber = fileNumber;
            this.stream = stream;
            this.progressEventHandler = progressEventHandler;
        }

        public void SetTotal(ulong total)
        {
            currentTotal = total;
            InvokeProgressCallback();
        }

        public void SetCompleted(ref ulong completeValue)
        {
            currentCompleteValue = completeValue;
            InvokeProgressCallback();
        }

        public int GetStream(uint index, out ISequentialOutStream outStream, AskMode askExtractMode)
        {
            if (index != fileNumber || askExtractMode != AskMode.kExtract)
            {
                outStream = null;
                return 0;
            }

            outStream = new OutStreamWrapper(stream);

            return 0;
        }

        public void PrepareOperation(AskMode askExtractMode)
        {
        }

        public void SetOperationResult(OperationResult resultEOperationResult)
        {
        }

        public void InvokeFinalProgressCallback()
        {
            if (!finalProgressReported)
            {
                // 7z doesn't invoke SetCompleted for all formats when an entry is fully extracted, so we fake it.
                SetCompleted(ref currentTotal);
            }
        }
        
        private void InvokeProgressCallback()
        {
            progressEventHandler?.Invoke(
                this,
                new EntryExtractionProgressEventArgs(currentCompleteValue, currentTotal)
            );

            if (currentCompleteValue == currentTotal)
            {
                finalProgressReported = true;
            }
        }
    }
}