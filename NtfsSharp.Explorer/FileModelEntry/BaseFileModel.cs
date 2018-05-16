using System;
using System.Collections;
using Aga.Controls.Tree;
using NtfsSharp.Volumes;

namespace NtfsSharp.Explorer.FileModelEntry
{
    public abstract class BaseFileModel : ITreeModel, IDisposable
    {
        protected readonly Volume Volume;

        protected BaseFileModel(Volume volume)
        {
            Volume = volume;
        }

        public abstract IEnumerable GetChildren(object parent);
        public abstract bool HasChildren(object parent);
        public abstract void Dispose();
    }
}
