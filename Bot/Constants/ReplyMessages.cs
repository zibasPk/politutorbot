using Bot.configs;

namespace Bot.Constants;

/// <summary>
/// Class containing bot reply messages.
/// </summary>
public static class ReplyMessages
{
    public const string NoTutoring = "Ci dispiace, al momento nessun tutor è disponibile per l’insegnamento richiesto";
    public static readonly string LockedUser = $"Sei bloccato dal fare nuove richieste per {GlobalConfig.BotConfig!.TutorLockHours} " +
                                               $"ore dalla tua precedente richiesta " + 
                                               $"o fino a che la segreteria non l'avrà elaborata.";

    public const string TutorSelected = "Tutor selezionato; " +
                                        "\nRiceverai a breve conferma via mail dell’abbinamento con il tutor e ulteriori istruzioni dalla segreteria del tutorato.";
    
}