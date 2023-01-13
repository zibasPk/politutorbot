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

  public const string AlreadyOngoingTutoring = "Sei bloccato dal fare nuove richieste fino alla conclusione del tuo tutoraggi corrente.";

  public const string TutorSelected = "Tutor selezionato; " +
                                      "\nRiceverai a breve conferma via mail dell’abbinamento con il tutor e ulteriori istruzioni dalla segreteria del tutorato.";

  
  public const string OFAChoice = "Sei in cerca di un tutor per recupero OFA (NO OFA ENG)?";
  public const string ConversationReset = "Lo stato della conversazione è stato resettato per inattività." +
                                          " \nScrivi un qualsiasi messaggio per ricominciare";

  public const string InternalError = "Si è verificato un errore interno al Bot, ci scusiamo per l'inconveniente." +
                                      " \nLa conversazione verrà resettata..." +
                                      " \nScrivi un qualsiasi messaggio per ricominciare";
  
  // Instruction messages
  public static string AlreadyLinkedAccount(int studentCode)
  {
    return $"Il tuo id Telegram è già associato al codice matricola {studentCode}.\n" +
           "Vuoi reinserire la matricola?";
  }
  
  public const string LinkStudentCode = "Inserisci il tuo codice matricola (NO CODICE PERSONA):"; 
  public const string ReLinkStudentCode = "Associazione rimossa. \nReinserisci il tuo codice matricola:"; 
  public const string SelectOFATutor = "Scegli uno dei tutor disponibili per recupero OFA (NO OFA ENG):\n \n";
  public static string SelectTutor(string exam)
  {
    return $"Scegli uno dei tutor disponibili per {exam}:\n \n";
  }
  
  public const string SelectSchool= "Scegli la tua scuola";
  public const string SelectCourse = "Scegli il tuo corso di studi";
  public const string SelectYear = "Scegli il tuo anno di corso";
  public const string SelectExam = "Scegli l’insegnamento per cui ti serve un tutorato";

  // Invalid inputs
  public const string InvalidTutor = "Inserisci un tutor della lista";
  public const string InvalidTutorIndex = "Inserisci il numero di un tutor della lista";
  public const string InvalidStudentCode = "Inserisci un codice persona valido";
  public const string NotEnabledStudentCode = "Spiacente non sei tra gli studenti che possono richiedere il tutoring peer to peer.";
  public const string InvalidSchool = "Inserisci una scuola valid";
  public const string SchoolNotICAT = "Il servizio è momentaneamente attivo solo per la scuola ICAT";
  public const string InvalidYear = "Inserisci un anno valido";
  public const string InvalidCourse = "Inserisci un corso valido";
  public const string InvalidExam = "Inserisci una materia valida";
  


}