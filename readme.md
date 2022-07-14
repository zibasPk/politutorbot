# BotTutorati

```mermaid
sequenceDiagram
    Bot->>Studente: Inserisci corso di studio
    Studente->>Bot: Informatica
    Bot->>Studente: Inserisci esame
    Studente->>Bot: Analisi 1
    Bot->>Studente: Login: www.example.com/tutorati_bot/login/1a2b3c4d5e6f7g8h9i
    Studente->>example.com: Login Aunica
    example.com->>Bot: 1a2b3c4d5e6f7g8h9i is 10612345
    opt CP is valid
    Bot->>Studente: Seleziona il tutor migliore da questa lista
    Studente->>Bot: Callback del bottone
    end
```
