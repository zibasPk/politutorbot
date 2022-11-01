import React from 'react';

import ActiveTutoringsTable from './ActiveTutoringsTable';
import EndedTutoringsTable from './EndedTutoringsTable';
import "./ActiveTutorings.css"

const TutoringsArray = [
  {
    id: 1,
    tutorNumber: 321321,
    tutorName: "Mario",
    tutorSurname: "Rossi",
    examCode: "09999",
    studentNumber: "938354",
    start_date: new Date('2000-12-17T03:24:00'),
    end_date: new Date('1995-12-17T03:24:00'),
    selected: false
  },
  {
    id: 2,
    tutorNumber: 321321,
    tutorName: "Mario",
    tutorSurname: "Rossi",
    examCode: "09999",
    studentNumber: "938354",
    start_date: new Date('1995-12-17T03:24:00'),
    end_date: null,
    selected: false
  },
  {
    id: 3,
    tutorNumber: 321321,
    tutorName: "Mario",
    tutorSurname: "Rossi",
    examCode: 11999,
    studentNumber: "938354",
    start_date: new Date('1995-12-17T03:24:00'),
    end_date: new Date('1995-12-17T03:24:00'),
    selected: false
  },
  {
    id: 4,
    tutorNumber: 321321,
    tutorName: "Mario",
    tutorSurname: "Rossi",
    examCode: 11999,
    studentNumber: "938354",
    start_date: new Date('1995-12-17T03:24:00'),
    end_date: new Date('1995-12-17T03:24:00'),
    selected: false
  },
  {
    id: 5,
    tutorNumber: 321321,
    tutorName: "Mario",
    tutorSurname: "Rossi",
    examCode: 11999,
    studentNumber: "938354",
    start_date: new Date('1995-12-17T03:24:00'),
    end_date: new Date('1995-12-17T03:24:00'),
    selected: false
  },
  {
    id: 6,
    tutorNumber: 321321,
    tutorName: "Mario",
    tutorSurname: "Rossi",
    examCode: 11999,
    studentNumber: "938354",
    start_date: new Date('1995-12-17T03:24:00'),
    end_date: null,
    selected: false
  },
  {
    id: 7,
    tutorNumber: 321321,
    tutorName: "Mario",
    tutorSurname: "Rossi",
    examCode: 11999,
    studentNumber: "938354",
    start_date: new Date('1995-12-17T03:24:00'),
    end_date: new Date('1995-12-17T03:24:00'),
    selected: false
  },
  {
    id: 8,
    tutorNumber: 321321,
    tutorName: "Mario Claudio",
    tutorSurname: "Brontesi",
    examCode: 11999,
    studentNumber: "938354",
    start_date: new Date('1995-12-17T03:24:00'),
    end_date: null,
    selected: false
  },
  {
    id: 9,
    tutorNumber: 321321,
    tutorName: "Mario",
    tutorSurname: "Rossi",
    examCode: 11999,
    studentNumber: "938354",
    start_date: new Date('1995-12-17T03:24:00'),
    end_date: null,
    selected: false
  },
  {
    id: 10,
    tutorNumber: 321321,
    tutorName: "Mario",
    tutorSurname: "Rossi",
    examCode: 11999,
    studentNumber: "938354",
    start_date: new Date('1995-12-17T03:24:00'),
    end_date: null,
    selected: false
  },
];


function ActiveTutorings() {
  let activeTutoringsArray = TutoringsArray.filter((t) => t.end_date == null);
  let endedTutoringsArray = TutoringsArray.filter((t) => t.end_date != null);
  return (
    <div className='content'>
      <ActiveTutoringsTable tutoringsArray={activeTutoringsArray} />
      <EndedTutoringsTable tutoringsArray={endedTutoringsArray} />
    </div>
  );
}

export default ActiveTutorings;