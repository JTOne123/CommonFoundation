﻿using System;
using System.IO;

namespace Beyova
{
    /// <summary>
    ///
    /// </summary>
    public class FileContainer : StorageContainer
    {
        /// <summary>
        /// Gets or sets the base directory.
        /// </summary>
        /// <value>
        /// The base directory.
        /// </value>
        public DirectoryInfo BaseDirectory { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileContainer"/> class.
        /// </summary>
        /// <param name="baseDirectory">The base directory.</param>
        public FileContainer(DirectoryInfo baseDirectory) : base()
        {
            BaseDirectory = baseDirectory;
            BaseDirectory.EnsureExistence();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileContainer"/> class.
        /// </summary>
        /// <param name="baseDirectory">The base directory.</param>
        public FileContainer(string baseDirectory) : this(new DirectoryInfo(baseDirectory))
        {
        }

        /// <summary>
        /// Extracts to destination.
        /// </summary>
        protected override void ExtractToDestination()
        {
            string currentPath = null;

            try
            {
                foreach (var current in _data)
                {
                    currentPath = current.Key.Replace('/', '\\').TrimStart('\\');

                    var fullPath = Path.Combine(BaseDirectory.FullName, currentPath);
                    var folder = Path.GetDirectoryName(fullPath);
                    new DirectoryInfo(folder).EnsureExistence();

                    current.Value.SaveTo(fullPath, true);
                }
            }
            catch (Exception ex)
            {
                throw ex.Handle(new { currentPath });
            }
        }
    }
}