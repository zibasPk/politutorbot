import React, { useEffect, useState } from 'react';
import configData from "../../config/config.json"
import styles from './TutorManagement.module.css'

import AddTutor from './AddTutor';
import TutorsList from './TutorsList';
import TutoringList from './TutoringList';

const TutorHeaders = {
  TutorCode: "Cod. Matr. Tutor",
  Surname: "Cognome Tutor",
  Name: "Nome Tutor",
  Course: "Corso di studi",
  OfaAvailable: "Per OFA",
  Ranking: "# Graduatoria",
  ContractState: "Stato Contratto"
};

const TutoringHeaders = {
  TutorCode: "Cod. Matr. Tutor",
  Surname: "Cognome Tutor",
  Name: "Nome Tutor",
  ExamCode: "Codice Esame",
  AvailableTutorings: "# Tutoraggi disponibili"
};

export default function TutorManagement()
{

  const [tutorsArray, setTutors] = useState([]);
  const [tutoringArray, setTutorings] = useState([]);

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
          switch (tutor.ContractState)
          {
            case 0:
              tutor.ContractState = "non inviato";
              break;
            case 1:
              tutor.ContractState = "inviato";
              break;
            case 2:
              tutor.ContractState = "firmato";
              break;
            default:
              tutor.ContractState = "invalido";
              break;
          }
          tutor.Id = i;
        });
        setTutors(tutors);
      });

    fetch(configData.botApiUrl + '/tutoring', {
      method: 'GET',
      headers: {
        'Authorization': 'Basic ' + btoa(configData.authCredentials),
      }
    }).then(resp => resp.json())
      .then((tutorings) =>
      {
        tutorings.forEach((tutoring, i) =>
        {
          tutoring.OfaAvailable ? tutoring.OfaAvailable = "SI" : tutoring.OfaAvailable = "NO";
          tutoring.Id = i;
        });
        setTutorings(tutorings);
      });
  }

  return (
    <div className={styles.content}>
      <AddTutor onChange={() => refreshData()} />
      <TutorsList headers={TutorHeaders} tutorList={tutorsArray} />
      <TutoringList headers={TutoringHeaders} tutoringList={tutoringArray} />
    </div>
  );
}

