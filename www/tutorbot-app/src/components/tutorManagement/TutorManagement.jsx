import React, { useEffect, useState } from 'react';
import configData from "../../config/config.json"
import styles from './TutorManagement.module.css'
import { makeCall } from '../../MakeCall';

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
  ExamName: "Nome Esame",
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

  const refreshData = async () =>
  {
    let status = { code: 0 };
    let result = await makeCall({ url: configData.botApiUrl + '/tutor', method: "GET", hasAuth: true,status: status});

    result.forEach((tutor, i) =>
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
    setTutors(result);

    result = await makeCall({ url: configData.botApiUrl + '/tutoring', method: "GET", hasAuth: true, status: status });

    result.forEach((tutoring, i) =>
    {
      tutoring.OfaAvailable ? tutoring.OfaAvailable = "SI" : tutoring.OfaAvailable = "NO";
      tutoring.Id = i;
    });
    setTutorings(result);
  }


  return (
    <div className={styles.content}>
      <AddTutor onChange={() => refreshData()} />
      <TutorsList headers={TutorHeaders} tutorList={tutorsArray} refreshData={() => refreshData()} />
      <TutoringList headers={TutoringHeaders} tutoringList={tutoringArray} refreshData={() => refreshData()} />
    </div>
  );
}

