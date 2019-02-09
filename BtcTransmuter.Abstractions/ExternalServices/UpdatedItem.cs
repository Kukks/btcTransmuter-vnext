using System;

namespace BtcTransmuter.Abstractions
{
    public class UpdatedItem<T> : EventArgs
    {
        public T Item { get; set; }

        public UpdateAction Action { get; set; }

        public enum UpdateAction
        {
            Added,
            Removed,
            Updated
        }
    }
}