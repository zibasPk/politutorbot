import React from 'react';

import Table from '../utils/Table';
import ActiveTutoringsModal from './ActiveTutoringsModal';
import EndedTutoringsModal from './EndedTutoringsModal';

import styles from "./ActiveTutorings.module.css"

const TutoringsArray = [
  {
    id: 1,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: "09999",
    studentNumber: "938354",
    start_date: new Date('2000-12-17T03:24:00'),
    end_date: new Date('1995-12-17T03:24:00'),
    selected: false
  },
  {
    id: 2,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: "09999",
    studentNumber: "938354",
    start_date: new Date('1995-12-17T03:24:00'),
    end_date: null,
    selected: false
  },
  {
    id: 3,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: 11999,
    studentNumber: "938354",
    start_date: new Date('1995-12-17T03:24:00'),
    end_date: new Date('1995-12-17T03:24:00'),
    selected: false
  },
  {
    id: 4,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: 11999,
    studentNumber: "938354",
    start_date: new Date('1995-12-17T03:24:00'),
    end_date: new Date('1995-12-17T03:24:00'),
    selected: false
  },
  {
    id: 5,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: 11999,
    studentNumber: "938354",
    start_date: new Date('1995-12-17T03:24:00'),
    end_date: new Date('1995-12-17T03:24:00'),
    selected: false
  },
  {
    id: 6,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: 11999,
    studentNumber: "938354",
    start_date: new Date('1995-12-17T03:24:00'),
    end_date: null,
    selected: false
  },
  {
    id: 7,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: 11999,
    studentNumber: "938354",
    start_date: new Date('1995-12-17T03:24:00'),
    end_date: new Date('1995-12-17T03:24:00'),
    selected: false
  },
  {
    id: 8,
    tutorNumber: 321321,
    tutorSurname: "Brontesi",
    tutorName: "Mario Claudiolini",
    examCode: 11999,
    studentNumber: "938354",
    start_date: new Date('1995-12-17T03:24:00'),
    end_date: null,
    selected: false
  },
  {
    id: 9,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: 11999,
    studentNumber: "938354",
    start_date: new Date('1995-12-17T03:24:00'),
    end_date: null,
    selected: false
  },
  {
    id: 10,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: 11999,
    studentNumber: "938354",
    start_date: new Date('1995-12-17T03:24:00'),
    end_date: null,
    selected: false
  },
  {
    id: 11,
    tutorNumber: 431321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: 11999,
    studentNumber: "938354",
    start_date: new Date('1995-12-17T03:24:00'),
    end_date: null,
    selected: false
  },
];

const ActiveHeaders = {
  tutorNumber: "Cod. Matr. Tutor",
  tutorSurname: "Cognome Tutor",
  tutorName: "Nome Tutor",
  examCode: "Cod. Matr. Studente",
  studentNumber: "Codice Esame",
  start_date: "Data Inizio",
}

const EndedHeaders = {
  tutorNumber: "Cod. Matr. Tutor",
  tutorSurname: "Cognome Tutor",
  tutorName: "Nome Tutor",
  examCode: "Cod. Matr. Studente",
  studentNumber: "Codice Esame",
  start_date: "Data Inizio",
  end_date: "Data Fine"
}

function ActiveTutorings() {
  let activeTutoringsArray = TutoringsArray.filter((t) => t.end_date == null);
  activeTutoringsArray = activeTutoringsArray.map(({ end_date, ...key }) => key);
  let endedTutoringsArray = TutoringsArray.filter((t) => t.end_date != null);
  return (
    <>
      <div className={styles.content} >
        <h1>Tutoraggi Attivi</h1>
        <Table headers={ActiveHeaders}
          content={activeTutoringsArray} hasChecks={true}
          modalProps={{
            modalContent: ActiveTutoringsModal,
            onConfirm: () => { console.log("azione confermata") },
            modalTitle: "Concludi Tutoraggi selezionati"
          }}/>
        <h1>Tutoraggi Conclusi</h1>
        <Table headers={EndedHeaders}
          content={endedTutoringsArray} hasChecks={true}
          modalProps={{
            modalContent: EndedTutoringsModal,
            modalTitle: "Esporta File CSV"
          }}
          />
      </div>
    </>
  );
}

export default ActiveTutorings;