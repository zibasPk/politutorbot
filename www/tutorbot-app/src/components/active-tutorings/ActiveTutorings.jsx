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
    start_date: "12-09-12 22:22:22",
    end_date: "12-09-12 22:22:22",
    selected: false
  },
  {
    id: 2,
    tutorNumber: 321321,
    tutorName: "Mario",
    tutorSurname: "Rossi",
    examCode: "09999",
    studentNumber: "938354",
    start_date: "12-09-12 22:22:22",
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
    start_date: "12-09-12 22:22:22",
    end_date: "12-09-12 22:22:22",
    selected: false
  }
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