// ------------------------------------------------------------------------------
// Copyright (c) Stark EM, LLC 2024 - All Rights Reserved
// Unauthorized copying of this file, via any medium, is strictly prohibited.
// ------------------------------------------------------------------------------

namespace BackgroundUIUpdate.Messaging;

public class ScannedTag
{
    /// <summary>
    /// The unique identifier of the tag
    /// </summary>
    public string Uid { get; set; }

    /// <summary>
    /// NDEF data
    /// </summary>
    public string Ndef { get; set; }

}

public class TagDiscovered : Stark.Messaging.Message<ScannedTag>
{
    public TagDiscovered(string uid, string ndef)
    {
        Data = new ScannedTag { Uid = uid, Ndef = ndef };
    }
}
