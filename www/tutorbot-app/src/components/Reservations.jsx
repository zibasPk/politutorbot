import React from 'react';
import './Reservations.css';
import CheckBoxOutlineBlankIcon from '@mui/icons-material/CheckBoxOutlineBlank';
import ArrowDownwardIcon from '@mui/icons-material/ArrowDownward';
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
    id: 10,
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
];


function Reservations() {
  return (
    <>
      <h1>Reservations</h1>
      <ReservationTable reservations={ReservationsArray} />
    </>
  );
}
export default Reservations;