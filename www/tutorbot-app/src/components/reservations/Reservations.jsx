import React, { useState } from "react";

import './Reservations.css';
import ReservationTable from './ReservationTable';


const ReservationsArray = [
  {
    id: 1,
    tutorNumber: 321321,
    tutorName: "Mario",
    tutorSurname: "Rossi",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 2,
    tutorNumber: 321321,
    tutorName: "ario",
    tutorSurname: "Bossi",
    examCode: "09999",
    studentNumber: "838354",
    timeStamp: "12-09-12 22:22:22",
    state: false,
    selected: false
  },
  {
    id: 3,
    tutorNumber: 321321,
    tutorName: "Lario",
    tutorSurname: "Nossi",
    examCode: "09999",
    studentNumber: "238354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 4,
    tutorNumber: 321321,
    tutorName: "Mario",
    tutorSurname: "Rossi",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 5,
    tutorNumber: 321321,
    tutorName: "Mario",
    tutorSurname: "Rossi",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 6,
    tutorNumber: 321321,
    tutorName: "Mario",
    tutorSurname: "Rossi",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 7,
    tutorNumber: 321321,
    tutorName: "ario",
    tutorSurname: "Bossi",
    examCode: "09999",
    studentNumber: "838354",
    timeStamp: "12-09-12 22:22:22",
    state: false,
    selected: false
  },
  {
    id: 8,
    tutorNumber: 321321,
    tutorName: "Lario",
    tutorSurname: "Nossi",
    examCode: "09999",
    studentNumber: "238354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 9,
    tutorNumber: 321321,
    tutorName: "Mario",
    tutorSurname: "Rossi",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 10,
    tutorNumber: 321321,
    tutorName: "Mario",
    tutorSurname: "Rossi",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 11,
    tutorNumber: 321321,
    tutorName: "Mario",
    tutorSurname: "Rossi",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 12,
    tutorNumber: 321321,
    tutorName: "ario",
    tutorSurname: "Bossi",
    examCode: "09999",
    studentNumber: "838354",
    timeStamp: "12-09-12 22:22:22",
    state: false,
    selected: false
  },
  {
    id: 13,
    tutorNumber: 321321,
    tutorName: "Lario",
    tutorSurname: "Nossi",
    examCode: "09999",
    studentNumber: "238354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 14,
    tutorNumber: 321321,
    tutorName: "Mario",
    tutorSurname: "Rossi",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 15,
    tutorNumber: 321321,
    tutorName: "Mario",
    tutorSurname: "Rossi",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 16,
    tutorNumber: 321321,
    tutorName: "Mario",
    tutorSurname: "Rossi",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 17,
    tutorNumber: 321321,
    tutorName: "ario",
    tutorSurname: "Bossi",
    examCode: "09999",
    studentNumber: "838354",
    timeStamp: "12-09-12 22:22:22",
    state: false,
    selected: false
  },
  {
    id: 18,
    tutorNumber: 321321,
    tutorName: "Lario",
    tutorSurname: "Nossi",
    examCode: "09999",
    studentNumber: "238354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 19,
    tutorNumber: 321321,
    tutorName: "Mario",
    tutorSurname: "Rossi",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 20,
    tutorNumber: 321321,
    tutorName: "Mario",
    tutorSurname: "Rossi",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 21,
    tutorNumber: 321321,
    tutorName: "Mario",
    tutorSurname: "Rossi",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 22,
    tutorNumber: 321321,
    tutorName: "ario",
    tutorSurname: "Bossi",
    examCode: "09999",
    studentNumber: "838354",
    timeStamp: "12-09-12 22:22:22",
    state: false,
    selected: false
  },
  {
    id: 23,
    tutorNumber: 321321,
    tutorName: "Lario",
    tutorSurname: "Nossi",
    examCode: "09999",
    studentNumber: "238354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 24,
    tutorNumber: 111321,
    tutorName: "Mario",
    tutorSurname: "Rossi",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 25,
    tutorNumber: 321321,
    tutorName: "Mario",
    tutorSurname: "Rossi",
    examCode: "09999",
    studentNumber: "938354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
  {
    id: 26,
    tutorNumber: 999321,
    tutorName: "Mario",
    tutorSurname: "Rossi",
    examCode: "09999",
    studentNumber: "111354",
    timeStamp: "12-09-12 22:22:22",
    state: true,
    selected: false
  },
];


function Reservations() {
  return (
    <div className="content">
      <ReservationTable reservations={ReservationsArray} />
    </div>
  );
}


export default Reservations;