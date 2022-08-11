# BotTutorati
```mermaid
sequenceDiagram
    Bot->>Studente: Inserisci corso di studio
    Studente->>Bot: corso di studio
    Bot->>Studente: Inserisci esame
    Studente->>Bot: esame
    rect rgb(30, 30, 31)
    Bot->>Studente: Login: www.tutorapp.polimi.it/tutorati_bot/login/1a2b3c4d5e6f7g8h9i
    Studente->> www.tutorapp.polimi.it: Login Aunica
    www.tutorapp.polimi.it ->> www.tutorapp.polimi.it: identità confermata con successo
    www.tutorapp.polimi.it ->>Bot: 1a2b3c4d5e6f7g8h9i is 10612345
    end
    opt Codice Persona is valid
    Bot->>Studente: Ecco il tutor più adatto a te
    Studente->>Bot: Callback del bottone
    end
```
