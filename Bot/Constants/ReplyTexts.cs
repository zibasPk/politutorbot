using Bot.configs;

namespace Bot.Constants;

/// <summary>
/// Class containing bot reply messages.
/// </summary>
public static class ReplyTexts
{
  public const string NoTutoring = "Ci dispiace, al momento nessun tutor è disponibile per l’insegnamento richiesto";
  public const string NoOFATutoring = "Ci dispiace, al momento nessun tutor è disponibile per gli OFA";

  public static readonly string LockedUser = $"Sei bloccato dal fare nuove richieste per {GlobalConfig.BotConfig!.TutorLockHours} " +
                                             $"ore dalla tua precedente richiesta " +
                                             $"o fino a che la segreteria non l'avrà elaborata.";

  public const string TutorSelected = "Tutor selezionato; " +
                                      "\nRiceverai a breve conferma via mail dell’abbinamento con il tutor e ulteriori istruzioni dalla segreteria del tutorato.";

  public const string OFAChoice = "Sei in cerca di un tutor per recupero OFA (NO OFA ENG)?";
  public const string ConversationReset = "Lo stato della conversazione è stato resettato per inattività." +
                                          " \nScrivi un qualsiasi messaggio per ricominciare";

  public const string InternalError = "Si è verificato un errore interno al Bot, ci scusiamo per l'inconveniente." +
                                      " \nLa conversazione verrà resettata..." +
                                      " \nScrivi un qualsiasi messaggio per ricominciare";
}