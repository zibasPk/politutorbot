import React, { useEffect, useState } from 'react';
import configData from "../../config/config.json"
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
  TutorCode: "Cod. Matr. Tutor",
  Surname: "Cognome Tutor",
  Name: "Nome Tutor",
  OfaAvailable: "Per OFA",
  Ranking: "Posizione in graduatoria"
};

export default function TutorManagement()
{

  const [tutorsArray, setTutors] = useState([]);

  useEffect(() =>
  {
    refreshData();
  }, [])

  const refreshData = () =>
  {
    fetch(configData.botApiUrl + '/tutor', {
      method: 'GET',
      headers: {
        'Authorization': 'Basic ' + btoa(configData.authCredentials),
      }
    }).then(resp => resp.json())
      .then((tutors) =>
      {
        tutors.forEach((tutor, i) =>
        {
          tutor.OfaAvailable ? tutor.OfaAvailable = "SI" : tutor.OfaAvailable = "NO";
          tutor.Id = i;
        });
        setTutors(tutors);
      });
  }

  return (
    <div className={styles.content}>
      <AddTutor onChange={() => refreshData()} />
      <TutorsList headers={Headers} tutorList={tutorsArray} />
    </div>
  );
}

