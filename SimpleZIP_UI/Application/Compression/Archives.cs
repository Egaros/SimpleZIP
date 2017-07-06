﻿using SimpleZIP_UI.Application.Compression.Algorithm;
using SimpleZIP_UI.Application.Compression.Algorithm.Type;
using System;
using System.Collections.Generic;

namespace SimpleZIP_UI.Application.Compression
{
    internal static class Archives
    {
        /// <summary>
        /// Enumeration to identify archive types.
        /// </summary>
        public enum ArchiveType
        {
            Zip, GZip, SevenZip, Tar, TarGz, TarBz2, TarLz
        }

        /// <summary>
        /// Maps file types for each archive. Consists only of file types with a single file name extension.
        /// </summary>
        internal static readonly IDictionary<string, ArchiveType> ArchiveFileTypes;

        /// <summary>
        /// Maps file types for each archive. Consists only of file types with multiple file name extensions.
        /// </summary>
        internal static readonly IDictionary<string, ArchiveType> ArchiveExtendedFileTypes;

        static Archives()
        {
            // populate dictionary that maps file types to archive types
            ArchiveFileTypes = new Dictionary<string, ArchiveType>(Enum.GetNames(typeof(ArchiveType)).Length * 2)
            {
                {".zip", ArchiveType.Zip},
                {".tar", ArchiveType.Tar},
                {".gzip", ArchiveType.GZip},
                {".gz", ArchiveType.GZip},
                {".tgz", ArchiveType.TarGz},
                {".tbz2", ArchiveType.TarBz2},
                {".tlz", ArchiveType.TarLz}
            };

            // populate dictionary that maps extended file types to archive types
            ArchiveExtendedFileTypes = new Dictionary<string, ArchiveType>
            {
                { ".tar.gz", ArchiveType.TarGz },
                { ".tar.bz2", ArchiveType.TarBz2 },
                { ".tar.lz", ArchiveType.TarLz }
            };
        }

        /// <summary>
        /// Assigns the correct algorithm instance to be used by evaluating the archive type.
        /// </summary>
        /// <param name="value">The enum value of the archive type.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when archive type matched no algorithm.</exception>
        /// <returns>An instance of the archiving algorithm that matches the specified value.</returns>
        public static IArchivingAlgorithm DetermineAlgorithm(ArchiveType value)
        {
            IArchivingAlgorithm algorithm;
            switch (value)
            {
                case ArchiveType.Zip:
                    algorithm = new Zip();
                    break;
                case ArchiveType.GZip:
                    algorithm = new GZip();
                    break;
                case ArchiveType.SevenZip:
                    algorithm = new SevenZip();
                    break;
                case ArchiveType.Tar:
                    algorithm = new Tar();
                    break;
                case ArchiveType.TarGz:
                    algorithm = new TarGzip();
                    break;
                case ArchiveType.TarBz2:
                    algorithm = new TarBzip2();
                    break;
                case ArchiveType.TarLz:
                    algorithm = new TarLzip();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
            return algorithm;
        }
    }
}