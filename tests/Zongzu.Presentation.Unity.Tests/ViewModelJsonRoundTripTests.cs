using System.Text.Json;
using Newtonsoft.Json;
using NUnit.Framework;
using Zongzu.Presentation.Unity;

namespace Zongzu.Presentation.Unity.Tests;

[TestFixture]
public class ViewModelJsonRoundTripTests
{
    /// <summary>
    /// Validates that a shell serialized by the backend (System.Text.Json,
    /// PascalCase) can be deserialized by the Unity client (Newtonsoft.Json).
    /// This is the contract-stability gate: if this test breaks, Unity will
    /// receive default values for mismatched fields.
    /// </summary>
    [Test]
    public void PresentationShellViewModel_SystemTextJson_Serialize_NewtonsoftJson_Deserialize()
    {
        var original = new PresentationShellViewModel
        {
            GreatHall = new GreatHallDashboardViewModel
            {
                CurrentDateLabel = "正月上旬",
                FamilySummary = "宗族安泰",
            },
            DeskSandbox = new DeskSandboxViewModel
            {
                Settlements = new[]
                {
                    new SettlementNodeViewModel
                    {
                        SettlementName = "兰溪县",
                        Security = 80,
                    },
                },
            },
            NotificationCenter = new NotificationCenterViewModel
            {
                Items = new[]
                {
                    new NotificationItemViewModel
                    {
                        Title = "场屋得捷",
                        Summary = "族中子弟中举",
                    },
                },
            },
        };

        // Backend path: System.Text.Json with PascalCase (PropertyNamingPolicy = null)
        var systemTextOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
        };
        string json = System.Text.Json.JsonSerializer.Serialize(original, systemTextOptions);

        // Unity path: Newtonsoft.Json default (expects PascalCase)
        var roundTripped = JsonConvert.DeserializeObject<PresentationShellViewModel>(json);

        Assert.That(roundTripped, Is.Not.Null);
        Assert.That(roundTripped.GreatHall.CurrentDateLabel, Is.EqualTo(original.GreatHall.CurrentDateLabel));
        Assert.That(roundTripped.GreatHall.FamilySummary, Is.EqualTo(original.GreatHall.FamilySummary));
        Assert.That(roundTripped.DeskSandbox.Settlements, Is.Not.Null);
        Assert.That(roundTripped.DeskSandbox.Settlements.Count, Is.EqualTo(1));
        Assert.That(roundTripped.DeskSandbox.Settlements[0].SettlementName, Is.EqualTo("兰溪县"));
        Assert.That(roundTripped.NotificationCenter.Items, Is.Not.Null);
        Assert.That(roundTripped.NotificationCenter.Items.Count, Is.EqualTo(1));
        Assert.That(roundTripped.NotificationCenter.Items[0].Title, Is.EqualTo("场屋得捷"));
    }

    /// <summary>
    /// Validates that nested arrays (IReadOnlyList backed by List in Newtonsoft)
    /// survive the round-trip without losing elements.
    /// </summary>
    [Test]
    public void NotificationItemViewModel_Array_RoundTrip()
    {
        var original = new NotificationCenterViewModel
        {
            Items = new[]
            {
                new NotificationItemViewModel { Title = "A", Summary = "Summary A" },
                new NotificationItemViewModel { Title = "B", Summary = "Summary B" },
            },
        };

        var systemTextOptions = new JsonSerializerOptions { PropertyNamingPolicy = null };
        string json = System.Text.Json.JsonSerializer.Serialize(original, systemTextOptions);

        var roundTripped = JsonConvert.DeserializeObject<NotificationCenterViewModel>(json);

        Assert.That(roundTripped!.Items.Count, Is.EqualTo(2));
        Assert.That(roundTripped.Items[1].Title, Is.EqualTo("B"));
    }
}
