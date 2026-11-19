using CommunityToolkit.Mvvm.Messaging.Messages;

namespace AcademiaDoZe.Presentation.AppMaui.Message;

public class CulturaPreferencesUpdatedMessage : ValueChangedMessage<string>
{
    public CulturaPreferencesUpdatedMessage(string cultura)
        : base(cultura)
    {
    }
}
