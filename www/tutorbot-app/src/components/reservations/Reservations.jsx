import React from "react";

import styles from './Reservations.module.css';
import Table from "../utils/Table";
import ModalBody from "./ModalBody";


const ReservationsArray = [
  {
    id: 1,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 2,
    tutorNumber: 321321,
    tutorSurname: "Bossi",
    tutorName: "nario",
    examCode: "09999",
    studentNumber: "838354",
    timeStamp: "12-09-12 22:22:22",
    state: false,
    selected: false
  },
  {
    id: 3,
    tutorNumber: 321321,
    tutorSurname: "Nossi",
    tutorName: "Lario",
    examCode: "09999",
    studentNumber: "238354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 4,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 5,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 6,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 7,
    tutorNumber: 321321,
    tutorSurname: "Bossi",
    tutorName: "ario",
    examCode: "09999",
    studentNumber: "838354",
    timeStamp: "12-09-12 22:22:22",
    state: false,
    selected: false
  },
  {
    id: 8,
    tutorNumber: 321321,
    tutorSurname: "Nossi",
    tutorName: "Lario",
    examCode: "09999",
    studentNumber: "238354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 9,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "10-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 10,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 11,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 12,
    tutorNumber: 321321,
    tutorSurname: "Bossi",
    tutorName: "ario",
    examCode: "09999",
    studentNumber: "838354",
    timeStamp: "12-09-12 22:22:22",
    state: false,
    selected: false
  },
  {
    id: 13,
    tutorNumber: 321321,
    tutorSurname: "Nossi",
    tutorName: "Lario",
    examCode: "09999",
    studentNumber: "238354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 14,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 15,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 16,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 17,
    tutorNumber: 321321,
    tutorSurname: "Bossi",
    tutorName: "ario",
    examCode: "09999",
    studentNumber: "838354",
    timeStamp: "12-09-12 22:22:22",
    state: false,
    selected: false
  },
  {
    id: 18,
    tutorNumber: 321321,
    tutorSurname: "Nossi",
    tutorName: "Lario",
    examCode: "09999",
    studentNumber: "238354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 19,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 20,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 21,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 22,
    tutorNumber: 321321,
    tutorSurname: "Bossi",
    tutorName: "ario",
    examCode: "09999",
    studentNumber: "838354",
    timeStamp: "12-09-12 22:22:22",
    state: false,
    selected: false
  },
  {
    id: 23,
    tutorNumber: 321321,
    tutorSurname: "Nossi",
    tutorName: "Lario",
    examCode: "09999",
    studentNumber: "238354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 24,
    tutorNumber: 111321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 25,
    tutorNumber: 321321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 26,
    tutorNumber: 999321,
    tutorSurname: "Rossi",
    tutorName: "Mario",
    examCode: "09999",
    studentNumber: "111354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
];

const Headers = {
  id: "Prenotazione",
  tutorNumber: "Cod. Matr. Tutor",
  tutorSurname: "Cognome Tutor",
  tutorName: "Nome Tutor",
  examCode: "Codice Esame",
  studentNumber: "Cod. Matr. Studente",
  timeStamp: "Data"
};

function Reservations() {
  // prepare object arrays for tables
  let processedReservations = ReservationsArray.filter((x) => x.state);
  let pendingReservations = ReservationsArray.filter((x) => !x.state);
  processedReservations = processedReservations.map(({ state, ...key }) => key);
  pendingReservations = pendingReservations.map(({ state, ...key }) => key);


  return (
    <div className={styles.content}>
      <h1>Prenotazioni da Gestire</h1>
      <Table headers={Headers} content={pendingReservations} hasChecks={true}
        modalProps={{
          modalContent: ModalBody,
          modalTitle: "Conferma prenotazione selezionate"
        }}
      />
      <h1>Storico Prenotazioni</h1>
      <Table headers={Headers} content={processedReservations} hasChecks={false} />
    </div>
  );
}


export default Reservations;