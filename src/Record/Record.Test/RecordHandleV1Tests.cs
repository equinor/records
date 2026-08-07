using FluentAssertions;
using Records.RecordHandle;

namespace Records.Tests;

public class RecordHandleV1Tests
{
    [Theory]
    [InlineData(RecordHandleV1.KindFusekiDatasetRef)]
    [InlineData(RecordHandleV1.KindDagalogDatasetRef)]
    public void VerifyForKind_Accepts_Expected_Handle_Kind(string kind)
    {
        var handle = new RecordHandleV1(
            "record_123-abc",
            "https://ssi.example.com/record/1",
            DateTimeOffset.UtcNow.AddMinutes(5),
            kind);

        handle.VerifyForKind(kind).Should().BeTrue();
    }

    [Fact]
    public void VerifyForKind_Rejects_Other_Backend_Kinds()
    {
        var handle = RecordHandleV1.CreateDagalogDatasetRef(
            "record_123",
            "https://ssi.example.com/record/1",
            DateTimeOffset.UtcNow.AddMinutes(5));

        handle.VerifyForKind(RecordHandleV1.KindFusekiDatasetRef).Should().BeFalse();
    }

    [Theory]
    [InlineData("../$/server")]
    [InlineData("record/123")]
    [InlineData("record?graph=urn:x")]
    public void Verify_Rejects_Unsafe_Dataset_Path_Segments(string dataset)
    {
        var handle = RecordHandleV1.CreateDagalogDatasetRef(
            dataset,
            "https://ssi.example.com/record/1",
            DateTimeOffset.UtcNow.AddMinutes(5));

        handle.Verify().Should().BeFalse();
    }
}
