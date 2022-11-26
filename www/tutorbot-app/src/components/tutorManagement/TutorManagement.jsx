import React from 'react';
import styles from './TutorManagement.module.css'
import AddTutor from './AddTutor';
import TutorsList from './TutorsList';

const TutorsArray = [
  {
    id: 1,
    tutorNumber: 111111,
    tutorSurname: "Mottadelli",
    tutorName: "Mario"
  },
  {
    id: 2,
    tutorNumber: 123123,
    tutorSurname: "Da Vinci",
    tutorName: "Mario"
  },
  {
    id: 3,
    tutorNumber: 123456,
    tutorSurname: "Manganelli",
    tutorName: "Gino"
  },
  {
    id: 4,
    tutorNumber: 654321,
    tutorSurname: "Zanichelli",
    tutorName: "Franco"
  },
  {
    id: 5,
    tutorNumber: 987654,
    tutorSurname: "Machiavelli",
    tutorName: "Mario"
  },
  {
    id: 6,
    tutorNumber: 987655,
    tutorSurname: "Lavario",
    tutorName: "Dario"
  },
]


const Headers = {
  tutorNumber: "Cod. Matr. Tutor",
  tutorSurname: "Cognome Tutor",
  tutorName: "Nome Tutor"
};

export default function TutorManagement() {
  return (
    <div className={styles.content}>
      <AddTutor />
      <TutorsList headers={Headers} tutorList={TutorsArray}/>
    </div>
  );
}

