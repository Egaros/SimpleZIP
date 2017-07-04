﻿using SharpCompress.Common;
using SharpCompress.Writers;

namespace SimpleZIP_UI.Application.Compression.Algorithm.Type
{
    public class Tar : ArchivingAlgorithm
    {
        public Tar() : base(ArchiveType.Tar)
        {
        }

        protected override WriterOptions GetWriterOptions()
        {
            return new WriterOptions(CompressionType.None);
        }
    }
}
