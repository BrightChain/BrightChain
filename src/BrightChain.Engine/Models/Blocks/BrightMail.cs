﻿namespace BrightChain.Engine.Models.Blocks
{
    using System.Collections.Generic;
    using BrightChain.Engine.Models.Hashes;

    public class BrightMail : BrightMessage
    {
        private readonly IEnumerable<string> Headers;
        private readonly IEnumerable<BlockHash> Attachments;
        private readonly bool recipientBcc;

        private bool _disposedValue;
    }
}
